using Pos.Domain;

namespace Pos.Application;

// Facade объединяет загрузку, доменные операции, расчёт и экспорт в готовые сценарии для интерфейса.
// Конструктор получает хранилище, сервис расчёта, стратегию, экспортёр и источник времени через DI.
public class GradebookFacade(ICourseRepository repository, FinalScoreService scores,
    IFinalScoreStrategy strategy, IReportExporter exporter, TimeProvider clock)
{
    // Загружает состояние курса для просмотра без сохранения изменений.
    public Course View() => repository.Load();

    // Загружает курс, выполняет переданное действие и сохраняет результат только при его успешном завершении.
    private void Change(Action<Course> action)
    {
        var course = repository.Load();
        action(course);
        repository.Save(course);
    }

    // Создаёт и сохраняет студента с новым идентификатором; возвращает идентификатор для дальнейших операций.
    public Guid AddStudent(string name, string number)
    {
        var id = Guid.NewGuid();
        Change(c => c.AddStudent(new Student(id, name, number)));
        return id;
    }

    // Обновляет данные студента через агрегат курса и сохраняет результат.
    public void UpdateStudent(Guid id, string name, string number) => Change(c => c.UpdateStudent(id, name, number));

    // Удаляет студента и его оценки через агрегат курса, затем сохраняет результат.
    public void RemoveStudent(Guid id) => Change(c => c.RemoveStudent(id));

    // Создаёт и сохраняет критерий с новым идентификатором; возвращает этот идентификатор.
    public Guid AddCriterion(string name, decimal maxScore, decimal weight)
    {
        var id = Guid.NewGuid();
        Change(c => c.AddCriterion(new Criterion(id, name, maxScore, weight)));
        return id;
    }

    // Обновляет критерий с доменными проверками и сохраняет курс.
    public void UpdateCriterion(Guid id, string name, decimal maxScore, decimal weight) =>
        Change(c => c.UpdateCriterion(id, name, maxScore, weight));

    // Удаляет критерий вместе со связанными оценками и сохраняет курс.
    public void RemoveCriterion(Guid id) => Change(c => c.RemoveCriterion(id));

    // Выставляет или исправляет оценку, берёт время изменения из TimeProvider и сохраняет курс.
    public void Assign(Guid studentId, Guid criterionId, decimal score, string comment) =>
        Change(c => c.AssignEvaluation(studentId, criterionId, new Score(score), comment, clock.GetUtcNow()));

    // Отменяет оценку студента по критерию и сохраняет курс.
    public void Cancel(Guid studentId, Guid criterionId) => Change(c => c.CancelEvaluation(studentId, criterionId));

    // Строит отчёт по студентам в порядке их номеров, рассчитывая итоги выбранной стратегией.
    public IReadOnlyList<ReportRow> Report()
    {
        var course = repository.Load();
        var rows = new List<ReportRow>();
        foreach (var student in course.Students.OrderBy(s => s.StudentNumber, StringComparer.OrdinalIgnoreCase))
        {
            var score = scores.Calculate(course, student.Id, strategy);
            rows.Add(new ReportRow(student.StudentNumber, student.Name, score.Value,
                score.EvaluatedCount, score.CriteriaCount, score.IsComplete, score.Policy));
        }
        return rows.ToArray();
    }

    // Формирует актуальный отчёт и передаёт его экспортёру для записи по указанному пути.
    public void Export(string path) => exporter.Export(Report(), path);
}
