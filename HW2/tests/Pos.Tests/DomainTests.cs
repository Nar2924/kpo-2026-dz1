using Pos.Domain;
using Xunit;

namespace Pos.Tests;

public class DomainTests
{
    // Готовит общий пример: один студент и два критерия с весами 0,6 и 0,4.
    private static (Course Course, Guid Student, Guid Project, Guid Defense) Example()
    {
        var course = new Course();
        var student = Guid.NewGuid();
        var project = Guid.NewGuid();
        var defense = Guid.NewGuid();
        course.AddStudent(new Student(student, "Анна", "S001"));
        course.AddCriterion(new Criterion(project, "Проект", 10, 0.6m));
        course.AddCriterion(new Criterion(defense, "Защита", 5, 0.4m));
        return (course, student, project, defense);
    }

    // Проверяет отказ в оценке вне допустимого диапазона и отсутствие изменений в курсе.
    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void OutOfRangeEvaluationIsRejected(decimal score)
    {
        var (course, student, project, _) = Example();
        Assert.Throws<DomainException>(() => course.AssignEvaluation(student, project, new Score(score), "", DateTimeOffset.UtcNow));
        Assert.Empty(course.Evaluations);
    }

    // Проверяет, что нулевой балл и балл, равный максимуму, допустимы.
    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void BoundaryScoresAreAccepted(decimal score)
    {
        var (course, student, project, _) = Example();
        course.AssignEvaluation(student, project, new Score(score), "", DateTimeOffset.UtcNow);
        Assert.Equal(score, Assert.Single(course.Evaluations).Score.Value);
    }

    // Проверяет запрет оценивания неизвестного студента или по неизвестному критерию.
    [Fact]
    public void UnknownCriterionOrStudentCannotBeEvaluated()
    {
        var (course, student, project, _) = Example();
        Assert.Throws<DomainException>(() => course.AssignEvaluation(student, Guid.NewGuid(), new Score(1), "", DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => course.AssignEvaluation(Guid.NewGuid(), project, new Score(1), "", DateTimeOffset.UtcNow));
    }

    // Проверяет, что неудачное исправление сохраняет прежние балл и комментарий.
    [Fact]
    public void InvalidCorrectionPreservesPreviousEvaluation()
    {
        var (course, student, project, _) = Example();
        course.AssignEvaluation(student, project, new Score(8), "old", DateTimeOffset.UtcNow);
        Assert.Throws<DomainException>(() => course.AssignEvaluation(student, project, new Score(12), "new", DateTimeOffset.UtcNow));
        Assert.Equal(8, Assert.Single(course.Evaluations).Score.Value);
        Assert.Equal("old", course.Evaluations[0].Comment);
    }

    // Проверяет, что исправление заменяет оценку, а не создаёт дубликат.
    [Fact]
    public void CorrectionReplacesInsteadOfDuplicating()
    {
        var (course, student, project, _) = Example();
        course.AssignEvaluation(student, project, new Score(8), "", DateTimeOffset.UtcNow);
        course.AssignEvaluation(student, project, new Score(9), "fixed", DateTimeOffset.UtcNow);
        Assert.Equal(9, Assert.Single(course.Evaluations).Score.Value);
    }

    // Проверяет расчёт итога 8,6 для критериев с разными максимумами и весами.
    [Fact]
    public void WeightedFinalUsesNormalizedCriterionScores()
    {
        var (course, student, project, defense) = Example();
        course.AssignEvaluation(student, project, new Score(9), "", DateTimeOffset.UtcNow);
        course.AssignEvaluation(student, defense, new Score(4), "", DateTimeOffset.UtcNow);
        var result = new FinalScoreService().Calculate(course, student, new MissingAsZeroStrategy());
        Assert.Equal(8.6m, result.Value);
        Assert.True(result.IsComplete);
    }

    // Проверяет различие итогов двух стратегий при пропуске оценки и предварительный статус.
    [Fact]
    public void StrategiesHandleMissingEvaluationsDifferently()
    {
        var (course, student, project, _) = Example();
        course.AssignEvaluation(student, project, new Score(8), "", DateTimeOffset.UtcNow);
        var service = new FinalScoreService();
        Assert.Equal(4.8m, service.Calculate(course, student, new MissingAsZeroStrategy()).Value);
        var result = service.Calculate(course, student, new EvaluatedOnlyStrategy());
        Assert.Equal(8, result.Value);
        Assert.False(result.IsComplete);
    }

    // Проверяет, что обе стратегии возвращают ноль, когда оценок ещё нет.
    [Fact]
    public void BothStrategiesReturnZeroWhenNoGradesExist()
    {
        var (course, student, _, _) = Example();
        var service = new FinalScoreService();
        Assert.Equal(0, service.Calculate(course, student, new MissingAsZeroStrategy()).Value);
        Assert.Equal(0, service.Calculate(course, student, new EvaluatedOnlyStrategy()).Value);
    }

    // Проверяет запрет расчёта итога без критериев.
    [Fact]
    public void EmptyRubricHasNoMeaningfulFinal()
    {
        var course = new Course();
        var id = Guid.NewGuid();
        course.AddStudent(new Student(id, "Анна", "1"));
        Assert.Throws<DomainException>(() => new FinalScoreService().Calculate(course, id, new MissingAsZeroStrategy()));
    }

    // Проверяет пересчёт итога после отмены оценки и ошибку при повторной отмене.
    [Fact]
    public void CancelRecalculatesFinal()
    {
        var (course, student, project, _) = Example();
        course.AssignEvaluation(student, project, new Score(10), "", DateTimeOffset.UtcNow);
        course.CancelEvaluation(student, project);
        Assert.Empty(course.Evaluations);
        Assert.Equal(0, new FinalScoreService().Calculate(course, student, new MissingAsZeroStrategy()).Value);
        Assert.Throws<DomainException>(() => course.CancelEvaluation(student, project));
    }

    // Проверяет запрет уменьшения максимума ниже выставленного балла и сохранение прежнего критерия.
    [Fact]
    public void CannotReduceMaximumBelowExistingScore()
    {
        var (course, student, project, _) = Example();
        course.AssignEvaluation(student, project, new Score(8), "", DateTimeOffset.UtcNow);
        Assert.Throws<DomainException>(() => course.UpdateCriterion(project, "Новое имя", 7, 0.6m));
        Assert.Equal(10, course.Rubric.Get(project).MaxScore);
        Assert.Equal("Проект", course.Rubric.Get(project).Name);
    }

    // Проверяет, что новый вес критерия учитывается уже при следующем расчёте.
    [Fact]
    public void UpdatedWeightImmediatelyChangesFinal()
    {
        var (course, student, project, _) = Example();
        course.AssignEvaluation(student, project, new Score(10), "", DateTimeOffset.UtcNow);
        course.UpdateCriterion(project, "Проект", 10, 0.4m);
        Assert.Equal(5, new FinalScoreService().Calculate(course, student, new MissingAsZeroStrategy()).Value);
    }

    // Проверяет запрет повторного названия и превышения суммы весов без изменения Builder.
    [Fact]
    public void RubricRejectsDuplicateNameAndExcessWeight()
    {
        var builder = new RubricBuilder().Add(new Criterion(Guid.NewGuid(), "Проект", 10, 0.6m));
        Assert.Throws<DomainException>(() => builder.Add(new Criterion(Guid.NewGuid(), "проект", 10, 0.2m)));
        Assert.Throws<DomainException>(() => builder.Add(new Criterion(Guid.NewGuid(), "Защита", 10, 0.5m)));
        Assert.Single(builder.Build().Criteria);
    }

    // Проверяет, что добавление критерия в Builder не меняет уже созданный рубрикатор.
    [Fact]
    public void BuiltRubricIsIndependentOfFurtherBuilderChanges()
    {
        var builder = new RubricBuilder().Add(new Criterion(Guid.NewGuid(), "Проект", 10, 0.5m));
        var rubric = builder.Build();
        builder.Add(new Criterion(Guid.NewGuid(), "Защита", 10, 0.5m));
        Assert.Single(rubric.Criteria);
    }

    // Проверяет отказ в создании критерия с недопустимым максимумом или весом.
    [Theory]
    [InlineData(0, 0.5)]
    [InlineData(-1, 0.5)]
    [InlineData(10, 0)]
    [InlineData(10, -0.1)]
    [InlineData(10, 1.1)]
    public void InvalidCriterionCannotBeCreated(decimal maximum, decimal weight) =>
        Assert.Throws<DomainException>(() => new Criterion(Guid.NewGuid(), "Проект", maximum, weight));

    // Проверяет запрет пустого ФИО и ФИО из одних пробелов.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyStudentNameIsRejected(string name) =>
        Assert.Throws<DomainException>(() => new Student(Guid.NewGuid(), name, "1"));

    // Проверяет уникальность номера студента без учёта регистра.
    [Fact]
    public void StudentNumberMustBeUniqueIgnoringCase()
    {
        var (course, _, _, _) = Example();
        Assert.Throws<DomainException>(() => course.AddStudent(new Student(Guid.NewGuid(), "Другой", "s001")));
        Assert.Single(course.Students);
    }

    // Проверяет сохранение идентификатора и оценок при изменении данных студента.
    [Fact]
    public void StudentUpdateKeepsIdentityAndGrades()
    {
        var (course, student, project, _) = Example();
        course.AssignEvaluation(student, project, new Score(8), "", DateTimeOffset.UtcNow);
        course.UpdateStudent(student, "Новое имя", "S002");
        Assert.Equal("Новое имя", course.GetStudent(student).Name);
        Assert.Equal(student, Assert.Single(course.Evaluations).StudentId);
    }

    // Проверяет удаление связанных оценок при удалении студента или критерия.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RemovingStudentOrCriterionCascadesEvaluations(bool removeStudent)
    {
        var (course, student, project, _) = Example();
        course.AssignEvaluation(student, project, new Score(10), "", DateTimeOffset.UtcNow);
        if (removeStudent) course.RemoveStudent(student);
        else course.RemoveCriterion(project);
        Assert.Empty(course.Evaluations);
    }

    // Проверяет, что баллы сравниваются по значению.
    [Fact]
    public void ScoreIsValueObject() => Assert.Equal(new Score(8.5m), new Score(8.5m));
}
