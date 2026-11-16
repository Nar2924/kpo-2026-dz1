using System.Globalization;
using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;

namespace Pos.ConsoleUi;

// Консольный интерфейс получает фасад через конструктор и передаёт ему действия пользователя.
public class ConsoleApplication(GradebookFacade facade)
{
    // Проверяет доступность базы и запускает цикл меню; показывает ошибки операций и завершает работу при выходе или конце ввода.
    public void Run(string dataPath)
    {
        facade.View(); // Проверяем базу до запуска меню, чтобы сразу сообщить о повреждённом файле.
        Console.WriteLine($"ПОС | Данные: {Path.GetFullPath(dataPath)}");
        Console.WriteLine("Вес: доля от 0 до 1; сумма не больше 1. Итог нормируется на 10 баллов.");
        while (true)
        {
            Console.WriteLine("\n1 Студенты   2 Добавить студента   3 Изменить студента   4 Удалить студента");
            Console.WriteLine("5 Критерии   6 Добавить критерий   7 Изменить критерий   8 Удалить критерий");
            Console.WriteLine("9 Оценки    10 Выставить/исправить  11 Отменить оценку   12 Итоги   13 Экспорт CSV   0 Выход");
            Console.Write("> ");
            var command = Console.ReadLine();
            if (command is null or "0") return;
            try
            {
                ExecuteCommand(command.Trim(), dataPath);
            }
            catch (EndOfStreamException) { return; }
            catch (Exception ex) when (ex is DomainException or StorageException or ArgumentException or IOException or UnauthorizedAccessException or OverflowException)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }

    // Вызывает нужную операцию по номеру команды; ввод данных и подтверждения выполняются здесь или во вспомогательных методах.
    private void ExecuteCommand(string command, string dataPath)
    {
        switch (command)
        {
            case "1":
                ShowStudents();
                break;
            case "2":
                facade.AddStudent(Read("ФИО"), Read("Уникальный номер студента"));
                break;
            case "3":
            {
                var student = ChooseStudent();
                facade.UpdateStudent(student, Read("Новое ФИО"), Read("Номер студента"));
                break;
            }
            case "4":
            {
                var student = ChooseStudent();
                if (Confirm("Удалить студента и все его оценки?"))
                    facade.RemoveStudent(student);
                break;
            }
            case "5":
                ShowCriteria();
                break;
            case "6":
                facade.AddCriterion(Read("Название"), Number("Максимальный балл"), Number("Вес (например 0,4)"));
                break;
            case "7":
            {
                var criterion = ChooseCriterion();
                facade.UpdateCriterion(criterion, Read("Новое название"), Number("Максимальный балл"), Number("Вес"));
                break;
            }
            case "8":
            {
                var criterion = ChooseCriterion();
                if (Confirm("Удалить критерий и все оценки по нему?"))
                    facade.RemoveCriterion(criterion);
                break;
            }
            case "9":
                ShowEvaluations();
                break;
            case "10":
                facade.Assign(ChooseStudent(), ChooseCriterion(), Number("Балл"), Read("Комментарий (можно пустой)"));
                break;
            case "11":
                facade.Cancel(ChooseStudent(), ChooseCriterion());
                break;
            case "12":
                ShowReport();
                break;
            case "13":
                ExportReport(dataPath);
                break;
            default:
                Console.WriteLine("Введите номер команды из меню.");
                break;
        }
    }

    // Запрашивает путь CSV, запрещает запись поверх базы и уточняет согласие на замену существующего файла.
    private void ExportReport(string dataPath)
    {
        var path = Path.GetFullPath(Read("Путь CSV"));
        if (string.Equals(path, Path.GetFullPath(dataPath), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Нельзя экспортировать поверх файла базы.");
        if (File.Exists(path) && !Confirm("Файл существует. Перезаписать?"))
            return;

        facade.Export(path);
        Console.WriteLine($"Экспорт: {path}");
    }

    // Выполняет демонстрацию: создаёт данные, проверяет отказ в неверной оценке, исправляет и отменяет оценки, выводит и экспортирует итоги.
    public void Demo(string exportPath)
    {
        Console.WriteLine("ПОС | Сквозная демонстрация (изолированная база в памяти)");
        var anna = facade.AddStudent("Анна Иванова", "S001");
        var ivan = facade.AddStudent("Иван Петров", "S002");
        var project = facade.AddCriterion("Проект", 10, 0.6m);
        var defense = facade.AddCriterion("Защита", 5, 0.4m);
        facade.Assign(anna, project, 9, "Хорошая архитектура");
        facade.Assign(anna, defense, 4, "Уверенная защита");
        facade.Assign(ivan, project, 7, "Нужны тесты");
        Console.WriteLine("Созданы 2 студента, 2 критерия; выставлены 3 оценки.");
        ShowStudents();
        ShowCriteria();
        ShowEvaluations();
        Console.WriteLine("Попытка выставить 12 из 10:");
        try { facade.Assign(anna, project, 12, ""); }
        catch (DomainException ex) { Console.WriteLine($"  Отклонено моделью: {ex.Message}"); }
        facade.Assign(ivan, project, 8, "Исправлена после проверки");
        facade.Assign(ivan, defense, 3, "Временная оценка");
        facade.Cancel(ivan, defense);
        Console.WriteLine("Оценка Ивана исправлена на 8; оценка за защиту выставлена и отменена.");
        ShowReport();
        facade.Export(exportPath);
        Console.WriteLine($"Итоги экспортированы: {Path.GetFullPath(exportPath)}");
    }

    // Загружает студентов из хранилища и передаёт список для вывода.
    private void ShowStudents() => ShowStudents(facade.View().Students);

    // Выводит переданный список студентов с номерами строк для выбора.
    private static void ShowStudents(IReadOnlyList<Student> students)
    {
        if (students.Count == 0) Console.WriteLine("Студентов пока нет.");
        for (var i = 0; i < students.Count; i++) Console.WriteLine($"{i + 1}. {students[i].Name} [{students[i].StudentNumber}]");
    }

    // Загружает критерии курса и передаёт список для вывода.
    private void ShowCriteria() => ShowCriteria(facade.View().Rubric.Criteria);

    // Выводит переданные критерии с номерами строк, максимумами и весами.
    private static void ShowCriteria(IReadOnlyList<Criterion> criteria)
    {
        if (criteria.Count == 0) Console.WriteLine("Критериев пока нет.");
        for (var i = 0; i < criteria.Count; i++)
            Console.WriteLine($"{i + 1}. {criteria[i].Name}: максимум {criteria[i].MaxScore}, вес {criteria[i].Weight}");
    }

    // Показывает студентов и возвращает идентификатор выбранной строки из того же снимка данных.
    private Guid ChooseStudent()
    {
        var students = facade.View().Students;
        ShowStudents(students);
        return students[Index("Номер строки студента", students.Count)].Id;
    }

    // Показывает критерии и возвращает идентификатор выбранной строки из того же снимка данных.
    private Guid ChooseCriterion()
    {
        var criteria = facade.View().Rubric.Criteria;
        ShowCriteria(criteria);
        return criteria[Index("Номер строки критерия", criteria.Count)].Id;
    }

    // Выводит все оценки с именами студентов, названиями критериев, максимумами, временем и комментариями.
    private void ShowEvaluations()
    {
        var course = facade.View();
        if (course.Evaluations.Count == 0) Console.WriteLine("Оценок пока нет.");
        foreach (var evaluation in course.Evaluations)
        {
            var student = course.GetStudent(evaluation.StudentId);
            var criterion = course.Rubric.Get(evaluation.CriterionId);
            Console.WriteLine($"{student.Name} [{student.StudentNumber}] | " +
                $"{criterion.Name}: {evaluation.Score.Value}/{criterion.MaxScore} | " +
                $"{evaluation.UpdatedAt:yyyy-MM-dd HH:mm} UTC | {evaluation.Comment}");
        }
    }

    // Выводит итог каждого студента, полноту оценивания и средний итог по отчёту.
    private void ShowReport()
    {
        var rows = facade.Report();
        if (rows.Count == 0) Console.WriteLine("Нет студентов для расчёта.");
        foreach (var row in rows)
            Console.WriteLine($"{row.StudentName} [{row.StudentNumber}]: {row.FinalScore:0.00}/10 | " +
                $"оценено {row.EvaluatedCount}/{row.CriteriaCount} | {(row.IsComplete ? "завершено" : "предварительно")} | {row.Policy}");
        if (rows.Count > 0) Console.WriteLine($"Средний итог: {rows.Average(r => r.FinalScore):0.00}/10");
    }

    // Выводит приглашение и читает строку; конец потока обозначает исключением для завершения меню.
    private static string Read(string prompt)
    {
        Console.Write($"{prompt}: ");
        return Console.ReadLine() ?? throw new EndOfStreamException();
    }

    // Читает десятичное число с точкой или запятой; разделители тысяч и некорректный ввод запрещены.
    private static decimal Number(string prompt)
    {
        var input = Read(prompt).Trim().Replace(',', '.');
        if (!decimal.TryParse(input, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var value))
            throw new ArgumentException("Введите число без разделителей тысяч, например 8,5 или 0.4.");
        return value;
    }

    // Проверяет введённый номер строки и переводит его в индекс с нуля; пустой список не позволяет сделать выбор.
    private static int Index(string prompt, int count)
    {
        if (count == 0) throw new DomainException("Список пуст. Сначала добавьте запись.");
        if (!int.TryParse(Read(prompt), out var number) || number < 1 || number > count)
            throw new ArgumentException($"Введите номер строки от 1 до {count}.");
        return number - 1;
    }

    // Запрашивает подтверждение; только ответ «да» без учёта регистра считается согласием.
    private static bool Confirm(string prompt) => string.Equals(Read(prompt + " [да/нет]"), "да", StringComparison.OrdinalIgnoreCase);
}
