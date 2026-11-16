namespace Pos.Domain;

public class Rubric
{
    public IReadOnlyList<Criterion> Criteria { get; }

    // Создаёт независимую копию списка критериев, которую нельзя изменять снаружи.
    internal Rubric(IEnumerable<Criterion> criteria) => Criteria = Array.AsReadOnly(criteria.ToArray());

    // Находит критерий по идентификатору или сообщает, что его нет в рубрикаторе.
    public Criterion Get(Guid id) => Criteria.FirstOrDefault(c => c.Id == id)
        ?? throw new DomainException("Критерий отсутствует в рубрикаторе.");
}

// Builder собирает и проверяет критерии до создания готового рубрикатора.
public class RubricBuilder
{
    private readonly List<Criterion> _criteria = [];

    // Проверяет уникальность критерия и сумму весов, добавляет его и возвращает Builder для следующего вызова.
    public RubricBuilder Add(Criterion criterion)
    {
        ArgumentNullException.ThrowIfNull(criterion);
        if (_criteria.Any(c => c.Id == criterion.Id ||
            string.Equals(c.Name, criterion.Name, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("Критерий с таким идентификатором или названием уже существует.");
        if (_criteria.Sum(c => c.Weight) + criterion.Weight > 1)
            throw new DomainException("Сумма весов критериев не должна превышать 1.");
        _criteria.Add(criterion);
        return this;
    }

    // Создаёт рубрикатор, независимый от последующих изменений Builder.
    public Rubric Build() => new(_criteria);
}
