namespace Pos.Domain;

// Объект-значение: баллы сравниваются по значению, а decimal хранит десятичные дроби без погрешностей двоичного представления.
public readonly record struct Score
{
    public decimal Value { get; }

    // Создаёт значение балла; отрицательные значения запрещены, максимум проверяется при выставлении оценки.
    public Score(decimal value)
    {
        if (value < 0) throw new DomainException("Балл не может быть отрицательным.");
        Value = value;
    }
}
