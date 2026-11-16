using Pos.Domain;

namespace Pos.Infrastructure;

// DTO хранят данные вне доменной модели; восстановление использует те же проверки, что и действия пользователя.
// Данные студента для JSON; конструктор записи принимает идентификатор, ФИО и номер.
public record StudentData(Guid Id, string Name, string StudentNumber);

// Данные критерия для JSON; конструктор записи принимает идентификатор, название, максимум и вес.
public record CriterionData(Guid Id, string Name, decimal MaxScore, decimal Weight);

// Данные оценки для JSON: ссылки на студента и критерий, балл, комментарий и время изменения.
public record EvaluationData(Guid StudentId, Guid CriterionId, decimal Score, string Comment, DateTimeOffset UpdatedAt);

// Снимок курса: конструктор записи принимает версию формата и массивы данных для хранения.
public record CourseSnapshot(int Version, StudentData[] Students, CriterionData[] Criteria, EvaluationData[] Evaluations)
{
    // Преобразует курс в отдельный снимок данных версии 1 для хранения, не сохраняя ссылки на доменные коллекции.
    public static CourseSnapshot From(Course course) => new(1,
        course.Students.Select(s => new StudentData(s.Id, s.Name, s.StudentNumber)).ToArray(),
        course.Rubric.Criteria.Select(c => new CriterionData(c.Id, c.Name, c.MaxScore, c.Weight)).ToArray(),
        course.Evaluations.Select(e => new EvaluationData(e.StudentId, e.CriterionId, e.Score.Value, e.Comment, e.UpdatedAt)).ToArray());

    // Восстанавливает курс через доменные операции; отклоняет неверную версию, пустые записи и дубли оценок.
    public Course Restore()
    {
        if (Version != 1) throw new DomainException("Неподдерживаемая версия файла данных.");
        if (Students is null || Criteria is null || Evaluations is null)
            throw new DomainException("В файле отсутствуют обязательные коллекции.");
        var course = new Course();
        foreach (var s in Students)
        {
            if (s is null) throw new DomainException("Пустая запись студента.");
            course.AddStudent(new Student(s.Id, s.Name, s.StudentNumber));
        }
        foreach (var c in Criteria)
        {
            if (c is null) throw new DomainException("Пустая запись критерия.");
            course.AddCriterion(new Criterion(c.Id, c.Name, c.MaxScore, c.Weight));
        }
        var pairs = new HashSet<(Guid, Guid)>();
        foreach (var e in Evaluations)
        {
            if (e is null) throw new DomainException("Пустая запись оценки.");
            if (!pairs.Add((e.StudentId, e.CriterionId))) throw new DomainException("Дублирующаяся оценка в файле.");
            course.AssignEvaluation(e.StudentId, e.CriterionId, new Score(e.Score), e.Comment, e.UpdatedAt);
        }
        return course;
    }
}
