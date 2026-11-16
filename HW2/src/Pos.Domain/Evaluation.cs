namespace Pos.Domain;

public class Evaluation
{
    public Guid StudentId { get; }
    public Guid CriterionId { get; }
    public Score Score { get; }
    public string Comment { get; }
    public DateTimeOffset UpdatedAt { get; }

    // Оценка создаётся внутри Domain; перед этим Course проверяет наличие студента и критерия.
    // Создаёт оценку с проверкой максимального балла и убирает пробелы по краям комментария.
    internal Evaluation(Guid studentId, Criterion criterion, Score score, string? comment, DateTimeOffset updatedAt)
    {
        if (score.Value > criterion.MaxScore)
            throw new DomainException($"Балл должен быть от 0 до {criterion.MaxScore}.");
        StudentId = Guard.Id(studentId);
        CriterionId = criterion.Id;
        Score = score;
        Comment = comment?.Trim() ?? "";
        UpdatedAt = updatedAt;
    }
}
