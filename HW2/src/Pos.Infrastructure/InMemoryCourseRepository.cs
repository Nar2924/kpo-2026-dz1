using Pos.Domain;

namespace Pos.Infrastructure;

// Снимок в памяти сохраняет тот же порядок работы, что и файловый адаптер: загрузка, изменение, сохранение.
public class InMemoryCourseRepository : ICourseRepository
{
    private CourseSnapshot _snapshot = CourseSnapshot.From(new Course());

    // Восстанавливает независимый курс из снимка, чтобы несохранённые изменения не затрагивали хранилище.
    public Course Load() => _snapshot.Restore();

    // Заменяет снимок в памяти актуальными данными курса.
    public void Save(Course course) => _snapshot = CourseSnapshot.From(course);
}
