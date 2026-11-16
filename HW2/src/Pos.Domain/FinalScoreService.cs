namespace Pos.Domain;

// Результат расчёта: конструктор записи принимает итог, количество оценок и критериев, название стратегии.
public record FinalScore(decimal Value, int EvaluatedCount, int CriteriaCount, string Policy)
{
    public bool IsComplete => CriteriaCount > 0 && EvaluatedCount == CriteriaCount;
}

// Strategy определяет, как учитывать пропуски, сохраняя общий алгоритм расчёта итога.
public interface IFinalScoreStrategy
{
    string Name { get; }

    // Возвращает сумму весов, на которую нужно разделить взвешенные баллы согласно правилу пропусков.
    decimal Denominator(IReadOnlyList<Criterion> all, IReadOnlySet<Guid> evaluated);
}

public class MissingAsZeroStrategy : IFinalScoreStrategy
{
    public string Name => "Пропуски = 0";

    // Берёт сумму весов всех критериев: отсутствие оценки даёт нулевой вклад в итог.
    public decimal Denominator(IReadOnlyList<Criterion> all, IReadOnlySet<Guid> evaluated) => all.Sum(c => c.Weight);
}

public class EvaluatedOnlyStrategy : IFinalScoreStrategy
{
    public string Name => "Только выставленные";

    // Берёт сумму весов только оценённых критериев: пропуски не уменьшают промежуточный итог.
    public decimal Denominator(IReadOnlyList<Criterion> all, IReadOnlySet<Guid> evaluated) =>
        all.Where(c => evaluated.Contains(c.Id)).Sum(c => c.Weight);
}

public class FinalScoreService
{
    // Рассчитывает взвешенный итог из 10 по выбранной стратегии и округляет до двух знаков; без критериев расчёт невозможен.
    public FinalScore Calculate(Course course, Guid studentId, IFinalScoreStrategy strategy)
    {
        course.GetStudent(studentId);
        var criteria = course.Rubric.Criteria;
        if (criteria.Count == 0) throw new DomainException("Для расчёта итога добавьте хотя бы один критерий.");
        var evaluations = course.Evaluations.Where(e => e.StudentId == studentId).ToArray();
        decimal weighted = 0;
        foreach (var evaluation in evaluations)
        {
            var criterion = course.Rubric.Get(evaluation.CriterionId);
            weighted += evaluation.Score.Value / criterion.MaxScore * criterion.Weight;
        }
        var denominator = strategy.Denominator(criteria, evaluations.Select(e => e.CriterionId).ToHashSet());
        var result = denominator == 0 ? 0 : decimal.Round(10 * weighted / denominator, 2, MidpointRounding.AwayFromZero);
        return new FinalScore(result, evaluations.Length, criteria.Count, strategy.Name);
    }
}
