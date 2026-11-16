using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.ConsoleUi;
using Pos.Domain;
using Pos.Infrastructure;

// Точка входа: настраивает консоль, разбирает аргументы, собирает зависимости и запускает меню или демо.
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
try
{
    var options = CliOptions.Parse(args);
    if (options.Help)
    {
        Console.WriteLine("ПОС — преподавательская оценка студентов");
        Console.WriteLine("dotnet run --project src/Pos.Console -- [--data путь.json] [--policy zero|evaluated]");
        Console.WriteLine("  --demo                 Сквозной сценарий в памяти, без изменения основной базы");
        Console.WriteLine("  --export путь.csv      Путь экспорта демо (по умолчанию exports/demo.csv)");
        Console.WriteLine("  --help                 Эта справка");
        return 0;
    }
    if (string.Equals(Path.GetFullPath(options.DataPath), Path.GetFullPath(options.ExportPath), StringComparison.OrdinalIgnoreCase))
        throw new ArgumentException("Файлы базы и экспорта должны различаться.");

    // Точка сборки приложения: связываем интерфейсы с адаптерами и выбранной стратегией через DI.
    var services = new ServiceCollection();
    services.AddSingleton<ICourseRepository>(_ => options.Demo
        ? new InMemoryCourseRepository() : new JsonCourseRepository(options.DataPath));
    services.AddSingleton<IFinalScoreStrategy>(_ => options.Policy == "zero"
        ? new MissingAsZeroStrategy() : new EvaluatedOnlyStrategy());
    services.AddSingleton<IReportExporter, CsvReportExporter>();
    services.AddSingleton(TimeProvider.System);
    services.AddSingleton<FinalScoreService>();
    services.AddSingleton<GradebookFacade>();
    services.AddSingleton<ConsoleApplication>();
    using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    var ui = provider.GetRequiredService<ConsoleApplication>();
    if (options.Demo) ui.Demo(options.ExportPath);
    else ui.Run(options.DataPath);
    return 0;
}
catch (Exception ex) when (ex is ArgumentException or DomainException or StorageException or IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine($"Ошибка: {ex.Message}");
    return 1;
}
