using System.Text.Json;
using System.Text.Json.Serialization;
using Pos.Domain;

namespace Pos.Infrastructure;

// Ошибка хранилища: конструктор сохраняет сообщение и исходную причину сбоя.
public class StorageException(string message, Exception inner) : Exception(message, inner);

// JSON-репозиторий получает путь к базе через конструктор и приводит его к абсолютному виду.
public class JsonCourseRepository(string path) : ICourseRepository
{
    private readonly string _path = Path.GetFullPath(path);
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        RespectRequiredConstructorParameters = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    // Читает JSON и восстанавливает курс с проверками; отсутствующий файл означает пустой курс, повреждённый — ошибку.
    public Course Load()
    {
        try
        {
            using var stream = File.OpenRead(_path);
            return (JsonSerializer.Deserialize<CourseSnapshot>(stream, Options)
                ?? throw new JsonException("Пустой документ.")).Restore();
        }
        catch (FileNotFoundException) { return new Course(); }
        catch (DirectoryNotFoundException) { return new Course(); }
        catch (Exception ex) when (ex is JsonException or DomainException or IOException or UnauthorizedAccessException)
        {
            throw new StorageException($"Не удалось прочитать данные из {_path}. Файл не изменён. {ex.Message}", ex);
        }
    }

    // Записывает JSON во временный файл, сбрасывает данные на диск и заменяет основной файл только после успешной записи.
    public void Save(Course course)
    {
        // Временный файл в том же каталоге позволяет заменить базу только после полной записи JSON.
        var temporary = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, CourseSnapshot.From(course), Options);
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporary, _path, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new StorageException($"Не удалось сохранить данные: {ex.Message}", ex);
        }
        finally
        {
            // Ошибка удаления временного файла не должна скрывать исходную ошибку сохранения.
            try { File.Delete(temporary); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
