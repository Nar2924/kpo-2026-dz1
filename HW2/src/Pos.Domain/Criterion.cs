namespace Pos.Domain;

public class Criterion
{
    public Guid Id { get; }
    public string Name { get; }
    public decimal MaxScore { get; }
    public decimal Weight { get; }

    // Создаёт критерий с непустым названием, положительным максимумом и весом в диапазоне (0; 1].
    public Criterion(Guid id, string name, decimal maxScore, decimal weight)
    {
        Id = Guard.Id(id);
        Name = Guard.Text(name, "Название критерия");
        if (maxScore <= 0) throw new DomainException("Максимальный балл должен быть больше нуля.");
        if (weight <= 0 || weight > 1) throw new DomainException("Вес должен быть в диапазоне (0; 1].");
        MaxScore = maxScore;
        Weight = weight;
    }
}
