namespace Pos.Application;

// Строка отчёта: конструктор записи принимает данные студента, итог и сведения о полноте оценивания.
public record ReportRow(string StudentNumber, string StudentName, decimal FinalScore,
    int EvaluatedCount, int CriteriaCount, bool IsComplete, string Policy);

public interface IReportExporter
{
    // Записывает готовые строки отчёта по указанному пути; формат определяет реализация экспортёра.
    void Export(IReadOnlyList<ReportRow> rows, string path);
}
