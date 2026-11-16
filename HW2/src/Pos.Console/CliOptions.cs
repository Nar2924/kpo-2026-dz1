namespace Pos.ConsoleUi;

// Параметры запуска: конструктор записи объединяет флаги, пути файлов и выбранную стратегию.
internal record CliOptions(bool Demo, bool Help, string DataPath, string ExportPath, string Policy)
{
    // Разбирает аргументы запуска, подставляет значения по умолчанию и проверяет название стратегии расчёта.
    public static CliOptions Parse(string[] args)
    {
        bool demo = false, help = false;
        string data = "data/course.json", export = "exports/demo.csv", policy = "zero";
        for (var i = 0; i < args.Length; i++)
        {
            // Читает значение после текущего флага и сдвигает индекс; отсутствие значения считается ошибкой.
            string Value()
            {
                if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
                    throw new ArgumentException($"Для {args[i]} требуется значение.");
                return args[++i];
            }
            switch (args[i])
            {
                case "--demo": demo = true; break;
                case "--help": help = true; break;
                case "--data": data = Value(); break;
                case "--export": export = Value(); break;
                case "--policy": policy = Value(); break;
                default: throw new ArgumentException($"Неизвестный аргумент: {args[i]}. Используйте --help.");
            }
        }
        if (policy is not ("zero" or "evaluated")) throw new ArgumentException("Правило: zero или evaluated.");
        return new CliOptions(demo, help, data, export, policy);
    }
}
