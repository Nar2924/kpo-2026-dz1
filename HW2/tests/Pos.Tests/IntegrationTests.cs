using System.Text;
using System.Text.Json;
using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;
using Xunit;

namespace Pos.Tests;

public class IntegrationTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "pos-tests-" + Guid.NewGuid());
    private string DataPath => Path.Combine(_directory, "course.json");

    // Удаляет временный каталог с файлами после завершения каждого теста.
    public void Dispose() { if (Directory.Exists(_directory)) Directory.Delete(_directory, true); }

    // Собирает фасад для теста с переданным репозиторием, стратегией пропусков как нулей и CSV-экспортёром.
    private static GradebookFacade Facade(ICourseRepository repository) => new(repository,
        new FinalScoreService(), new MissingAsZeroStrategy(), new CsvReportExporter(), TimeProvider.System);

    // Проверяет полный сценарий с JSON: сохранение, повторную загрузку, исправление, отмену и экспорт CSV.
    [Fact]
    public void EndToEndPersistsCorrectsCancelsAndExports()
    {
        var facade = Facade(new JsonCourseRepository(DataPath));
        var student = facade.AddStudent("Анна", "S001");
        var project = facade.AddCriterion("Проект", 10, 0.6m);
        var defense = facade.AddCriterion("Защита", 5, 0.4m);
        facade.Assign(student, project, 9, "Комментарий");
        facade.Assign(student, defense, 4, "");
        var reopened = Facade(new JsonCourseRepository(DataPath));
        Assert.Equal(8.6m, Assert.Single(reopened.Report()).FinalScore);
        reopened.Assign(student, project, 10, "Исправлено");
        reopened.Cancel(student, defense);
        var report = Assert.Single(reopened.Report());
        Assert.Equal(6, report.FinalScore);
        Assert.False(report.IsComplete);
        var csv = Path.Combine(_directory, "report.csv");
        reopened.Export(csv);
        Assert.Contains("6.00", File.ReadAllText(csv));
        Assert.Contains("Анна", File.ReadAllText(csv));
        Assert.Equal(1, Assert.Single(new JsonCourseRepository(DataPath).Load().Evaluations).Score.Value / 10);
    }

    // Проверяет, что отклонённая оценка не меняет файл базы даже побайтово.
    [Fact]
    public void FailedUseCaseDoesNotChangeStoredFile()
    {
        var facade = Facade(new JsonCourseRepository(DataPath));
        var student = facade.AddStudent("Анна", "1");
        var criterion = facade.AddCriterion("Проект", 10, 1);
        facade.Assign(student, criterion, 8, "");
        var previous = File.ReadAllBytes(DataPath);
        Assert.Throws<DomainException>(() => facade.Assign(student, criterion, 12, ""));
        Assert.Equal(previous, File.ReadAllBytes(DataPath));
    }

    // Проверяет, что некорректный JSON вызывает ошибку и сохраняется без изменений.
    [Theory]
    [InlineData("not json")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"Version\":2,\"Students\":[],\"Criteria\":[],\"Evaluations\":[]}")]
    [InlineData("{\"Version\":1,\"Students\":null,\"Criteria\":[],\"Evaluations\":[]}")]
    public void CorruptedFileIsReportedAndPreserved(string contents)
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(DataPath, contents);
        Assert.Throws<StorageException>(() => new JsonCourseRepository(DataPath).Load());
        Assert.Equal(contents, File.ReadAllText(DataPath));
    }

    // Проверяет, что загрузка из JSON не позволяет обойти ограничение максимального балла.
    [Fact]
    public void RehydrationRejectsScoreOutsideCriterion()
    {
        Directory.CreateDirectory(_directory);
        var student = Guid.NewGuid();
        var criterion = Guid.NewGuid();
        var snapshot = new CourseSnapshot(1, [new(student, "Анна", "1")],
            [new(criterion, "Проект", 10, 1)], [new(student, criterion, 12, "", DateTimeOffset.UtcNow)]);
        File.WriteAllText(DataPath, JsonSerializer.Serialize(snapshot));
        Assert.Throws<StorageException>(() => new JsonCourseRepository(DataPath).Load());
    }

    // Проверяет отказ в восстановлении снимка с двумя оценками для одной пары студент–критерий.
    [Fact]
    public void RehydrationRejectsDuplicateEvaluation()
    {
        var student = Guid.NewGuid();
        var criterion = Guid.NewGuid();
        var evaluation = new EvaluationData(student, criterion, 5, "", DateTimeOffset.UtcNow);
        var snapshot = new CourseSnapshot(1, [new(student, "Анна", "1")],
            [new(criterion, "Проект", 10, 1)], [evaluation, evaluation]);
        Assert.Throws<DomainException>(() => snapshot.Restore());
    }

    // Проверяет, что отсутствие файла базы создаёт пустой курс.
    [Fact]
    public void MissingFileStartsEmpty()
    {
        var course = new JsonCourseRepository(DataPath).Load();
        Assert.Empty(course.Students);
        Assert.Empty(course.Rubric.Criteria);
    }

    // Проверяет экранирование CSV, защиту от формул, формат числа и наличие UTF-8 BOM.
    [Fact]
    public void CsvEscapesTextAndProtectsFormulas()
    {
        var path = Path.Combine(_directory, "report.csv");
        new CsvReportExporter().Export([new("=1+1", "Анна; \"А\"\nБ", 8.5m, 1, 2, false, "zero")], path);
        var contents = File.ReadAllText(path);
        Assert.Contains("\"'=1+1\"", contents);
        Assert.Contains("\"Анна; \"\"А\"\"\nБ\"", contents);
        Assert.Contains("8.50", contents);
        Assert.True(File.ReadAllBytes(path).AsSpan(0, 3).SequenceEqual(Encoding.UTF8.GetPreamble()));
    }

    // Проверяет, что изменение загруженного курса без Save не затрагивает хранилище в памяти.
    [Fact]
    public void MemoryRepositoryDoesNotLeakUnsavedMutations()
    {
        var repository = new InMemoryCourseRepository();
        repository.Load().AddStudent(new Student(Guid.NewGuid(), "Анна", "1"));
        Assert.Empty(repository.Load().Students);
    }
}
