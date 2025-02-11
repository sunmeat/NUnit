
# Проект: Entity Framework Core с NUnit для тестирования

Этот проект демонстрирует использование Entity Framework Core с базой данных в памяти для тестирования, а также использование NUnit для написания модульных тестов.

## Установка библиотек

Для установки необходимых библиотек используйте следующие команды:

```
dotnet add package NUnit
dotnet add package NUnit3TestAdapter
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Microsoft.NET.Test.Sdk
```

## Конфигурация .csproj

В файле `.csproj` (кликните на имя проекта), добавьте следующую строку внутри элемента `<PropertyGroup>`:

```xml
<StartupObject>Program</StartupObject>
```

## Объяснение кода

### Класс Person

```csharp
public class Person
{
    public int PersonId { get; set; }
    public string? Name { get; set; }
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
    public decimal HobbySatisfaction { get; set; }
    public bool IsActive { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    public ICollection<Hobby> Hobbies { get; set; } = new List<Hobby>();
}
```

### Класс Hobby

```csharp
public class Hobby
{
    public int HobbyId { get; set; }
    public string? Name { get; set; }

    public ICollection<Person> People { get; set; } = new List<Person>();
}
```

### ApplicationDbContext

```csharp
public class ApplicationDbContext : DbContext
{
    public virtual DbSet<Person>? People { get; set; }
    public virtual DbSet<Hobby>? Hobbies { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase("TestDatabase");
        }
    }
}
```

### PersonRepository

```csharp
public class PersonRepository
{
    private readonly ApplicationDbContext _context;

    public PersonRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void GetAllPeopleWithHobbies()
    {
        var people = _context.People?.Include(p => p.Hobbies).ToList() ?? new List<Person>();

        foreach (var person in people)
        {
            Console.WriteLine($"Человек: {person.Name}");
            foreach (var hobby in person.Hobbies)
            {
                Console.WriteLine($"  Увлечение: {hobby.Name}");
            }
        }
    }
}
```

### Program

```csharp
internal class Program
{
    static void Main()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
             .UseInMemoryDatabase("TestDatabase")
             .Options;

        using var context = new ApplicationDbContext(options);
        var repo = new PersonRepository(context);

        var person1 = new Person { Name = "Александр", Hobbies = new List<Hobby> { new Hobby { Name = "Футбол" }, new Hobby { Name = "Чтение" } } };
        var person2 = new Person { Name = "Артём", Hobbies = new List<Hobby> { new Hobby { Name = "Рисование" }, new Hobby { Name = "Путешествия" } } };
        context.People?.AddRange(person1, person2);
        context.SaveChanges();

        repo.GetAllPeopleWithHobbies();
    }
}
```

### Запуск тестов

Чтобы запустить тесты, используйте следующую команду:

```
dotnet test
```

### Класс тестов: PersonRepositoryTests

```csharp
[TestFixture]
public class PersonRepositoryTests
{
    private ApplicationDbContext? _context;
    
    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Test]
    public void GetAllPeopleWithHobbies_ShouldReturnPeopleWithHobbies()
    {
        var person1 = new Person { Name = "АлександрТест", Hobbies = new List<Hobby> { new Hobby { Name = "Футбол" }, new Hobby { Name = "Чтение" } } };
        var person2 = new Person { Name = "АртёмТест", Hobbies = new List<Hobby> { new Hobby { Name = "Рисование" }, new Hobby { Name = "Путешествия" } } };
        _context?.People?.AddRange(person1, person2);
        _context?.SaveChanges();

        var result = _context?.People?.Include(p => p.Hobbies).ToList();

        Assert.That(result?.Count, Is.EqualTo(2));
        Assert.That(result?[0].Name, Is.EqualTo("АлександрТест"));
        Assert.That(result?[0].Hobbies.Count, Is.EqualTo(2));
        Assert.That(result?[0].Hobbies.ElementAt(0).Name, Is.EqualTo("Футбол"));
    }
}
```

## Примечания

- Этот проект использует базу данных в памяти для тестирования, которая симулирует операции с базой данных без необходимости в реальной базе данных.
- Убедитесь, что все необходимые пакеты установлены с помощью команд `dotnet add package`.
- Для запуска тестов используйте команду `dotnet test`.

## Лицензия

Этот проект лицензирован по лицензии MIT — подробности см. в файле [LICENSE](LICENSE).
