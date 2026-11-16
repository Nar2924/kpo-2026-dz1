using System.Globalization;
using System.Text;
using Pos.Application;

namespace Pos.Infrastructure;

public class CsvReportExporter : IReportExporter
{
    // Записывает отчёт в CSV с разделителем «;» и UTF-8 BOM, заменяя целевой файл после завершения записи временного.
    public void Export(IReadOnlyList<ReportRow> rows, string path)
    {
        var target = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        var temporary = target + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var writer = new StreamWriter(temporary, false, new UTF8Encoding(true)))
            {
                writer.WriteLine("Номер;Студент;Итог из 10;Оценено;Критериев;Завершено;Правило");
                foreach (var row in rows)
                    writer.WriteLine(string.Join(";", Cell(row.StudentNumber), Cell(row.StudentName),
                        row.FinalScore.ToString("0.00", CultureInfo.InvariantCulture), row.EvaluatedCount,
                        row.CriteriaCount, row.IsComplete ? "да" : "нет", Cell(row.Policy)));
            }
            File.Move(temporary, target, true);
        }
        finally
        {
            try { File.Delete(temporary); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    // Заключает текст в кавычки, экранирует кавычки внутри и добавляет апостроф перед возможными формулами таблиц.
    private static string Cell(string text)
    {
        // Кавычки сохраняют разделители и переносы строк в одной ячейке, апостроф защищает от выполнения формул.
        var trimmed = text.TrimStart();
        if ((trimmed.Length > 0 && trimmed[0] is '=' or '+' or '-' or '@') ||
            text.StartsWith('\t') || text.StartsWith('\r'))
            text = "'" + text;
        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }
}
