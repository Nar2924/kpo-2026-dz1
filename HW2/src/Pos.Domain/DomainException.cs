namespace Pos.Domain;

// Ошибка нарушения правил предметной области; конструктор принимает понятное пользователю сообщение.
public class DomainException(string message) : Exception(message);

internal static class Guard
{
    // Проверяет, что текст не пустой, и убирает пробелы по краям; field используется в сообщении об ошибке.
    public static string Text(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{field}: значение не может быть пустым.");
        return value.Trim();
    }

    // Проверяет, что идентификатор не равен Guid.Empty, и возвращает его.
    public static Guid Id(Guid id)
    {
        if (id == Guid.Empty) throw new DomainException("Идентификатор не может быть пустым.");
        return id;
    }
}
