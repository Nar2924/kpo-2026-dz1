namespace Pos.Domain;

// Агрегат одного курса: здесь проверяются общие правила для студентов, критериев и оценок.
public class Course
{
    private readonly List<Student> _students = [];
    private readonly List<Evaluation> _evaluations = [];
    public IReadOnlyList<Student> Students => _students.AsReadOnly();
    public IReadOnlyList<Evaluation> Evaluations => _evaluations.AsReadOnly();
    public Rubric Rubric { get; private set; } = new RubricBuilder().Build();

    // Добавляет студента, предварительно проверив уникальность его идентификатора и номера.
    public void AddStudent(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);
        EnsureUniqueStudent(student);
        _students.Add(student);
    }

    // Меняет ФИО и номер студента, сохраняя его идентификатор и связанные оценки.
    public void UpdateStudent(Guid id, string name, string number)
    {
        var previous = GetStudent(id);
        var updated = new Student(id, name, number);
        EnsureUniqueStudent(updated, id);
        _students[_students.IndexOf(previous)] = updated;
    }

    // Проверяет уникальность идентификатора и номера без учёта регистра; при изменении исключает самого студента.
    private void EnsureUniqueStudent(Student student, Guid? exceptId = null)
    {
        if (_students.Any(s => s.Id != exceptId && (s.Id == student.Id ||
            string.Equals(s.StudentNumber, student.StudentNumber, StringComparison.OrdinalIgnoreCase))))
            throw new DomainException("Студент с таким идентификатором или номером уже существует.");
    }

    // Находит студента по идентификатору или сообщает, что такого студента нет.
    public Student GetStudent(Guid id) => _students.FirstOrDefault(s => s.Id == id)
        ?? throw new DomainException("Студент не найден.");

    // Удаляет студента вместе со всеми его оценками.
    public void RemoveStudent(Guid id)
    {
        _students.Remove(GetStudent(id));
        _evaluations.RemoveAll(e => e.StudentId == id);
    }

    // Добавляет критерий через Builder, который проверяет дубликаты и сумму весов.
    public void AddCriterion(Criterion criterion) => ReplaceRubric(Rubric.Criteria.Append(criterion));

    // Меняет критерий; запрещает уменьшать максимум ниже уже выставленных баллов.
    public void UpdateCriterion(Guid id, string name, decimal maxScore, decimal weight)
    {
        Rubric.Get(id);
        var updated = new Criterion(id, name, maxScore, weight);
        if (_evaluations.Any(e => e.CriterionId == id && e.Score.Value > maxScore))
            throw new DomainException("Новый максимум меньше уже выставленной оценки. Сначала исправьте оценки.");
        ReplaceRubric(Rubric.Criteria.Select(c => c.Id == id ? updated : c));
    }

    // Удаляет существующий критерий из рубрикатора и все оценки по нему.
    public void RemoveCriterion(Guid id)
    {
        Rubric.Get(id);
        ReplaceRubric(Rubric.Criteria.Where(c => c.Id != id));
        _evaluations.RemoveAll(e => e.CriterionId == id);
    }

    // Собирает новый рубрикатор и заменяет старый только после успешной проверки всех критериев.
    private void ReplaceRubric(IEnumerable<Criterion> criteria)
    {
        var builder = new RubricBuilder();
        foreach (var criterion in criteria) builder.Add(criterion);
        Rubric = builder.Build();
    }

    // Выставляет или исправляет оценку после проверки студента, критерия и допустимого балла.
    public void AssignEvaluation(Guid studentId, Guid criterionId, Score score, string? comment, DateTimeOffset updatedAt)
    {
        GetStudent(studentId);
        var evaluation = new Evaluation(studentId, Rubric.Get(criterionId), score, comment, updatedAt);

        // Сначала проверяем новую оценку: при ошибке прежние балл и комментарий сохраняются.
        _evaluations.RemoveAll(e => e.StudentId == studentId && e.CriterionId == criterionId);
        _evaluations.Add(evaluation);
    }

    // Удаляет оценку указанного студента по критерию; сообщает об ошибке, если оценка отсутствует.
    public void CancelEvaluation(Guid studentId, Guid criterionId)
    {
        GetStudent(studentId);
        Rubric.Get(criterionId);
        if (_evaluations.RemoveAll(e => e.StudentId == studentId && e.CriterionId == criterionId) == 0)
            throw new DomainException("Оценка ещё не выставлена.");
    }
}
