namespace Pos.Domain;

public class Student
{
    public Guid Id { get; }
    public string Name { get; }
    public string StudentNumber { get; }

    // Создаёт студента, проверяет идентификатор и обязательные поля, убирает пробелы по краям ФИО и номера.
    public Student(Guid id, string name, string studentNumber)
    {
        Id = Guard.Id(id);
        Name = Guard.Text(name, "ФИО");
        StudentNumber = Guard.Text(studentNumber, "Номер студента");
    }
}
