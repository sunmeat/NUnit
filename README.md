# Проєкт: Entity Framework Core з NUnit для тестування

Цей проєкт демонструє використання Entity Framework Core з базою даних у пам’яті для тестування, а також використання NUnit для написання модульних тестів.

## Встановлення бібліотек

Для встановлення необхідних бібліотек використовуйте такі команди в терміналі:

```
dotnet add package NUnit
dotnet add package NUnit3TestAdapter
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Microsoft.NET.Test.Sdk
```

## Конфігурація .csproj

У файлі `.csproj` (клацніть на назву проєкту) додайте такий рядок всередині елемента `<PropertyGroup>`:

```xml
<StartupObject>Program</StartupObject>
```

## Опис коду

### Клас Person

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

### Клас Hobby

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
            Console.WriteLine($"Персона: {person.Name}");
            foreach (var hobby in person.Hobbies)
            {
                Console.WriteLine($"  Захоплення: {hobby.Name}");
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

        var person1 = new Person { Name = "Олександр", Hobbies = new List<Hobby> { new Hobby { Name = "Футбол" }, new Hobby { Name = "Читання" } } };
        var person2 = new Person { Name = "Артем", Hobbies = new List<Hobby> { new Hobby { Name = "Малювання" }, new Hobby { Name = "Подорожі" } } };
        context.People?.AddRange(person1, person2);
        context.SaveChanges();

        repo.GetAllPeopleWithHobbies();
    }
}
```

### Запуск тестів

Щоб запустити тести, використовуйте таку команду:

```
dotnet test
```

### Клас тестів: PersonRepositoryTests

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
        var person1 = new Person { Name = "ОлександрТест", Hobbies = new List<Hobby> { new Hobby { Name = "Футбол" }, new Hobby { Name = "Читання" } } };
        var person2 = new Person { Name = "АртемТест", Hobbies = new List<Hobby> { new Hobby { Name = "Малювання" }, new Hobby { Name = "Подорожі" } } };
        _context?.People?.AddRange(person1, person2);
        _context?.SaveChanges();

        var result = _context?.People?.Include(p => p.Hobbies).ToList();

        Assert.That(result?.Count, Is.EqualTo(2));
        Assert.That(result?[0].Name, Is.EqualTo("ОлександрТест"));
        Assert.That(result?[0].Hobbies.Count, Is.EqualTo(2));
        Assert.That(result?[0].Hobbies.ElementAt(0).Name, Is.EqualTo("Футбол"));
    }
}
```

## Примітки

- Цей проєкт використовує базу даних у пам’яті для тестування, яка імітує роботу з базою даних без необхідності використання реальної БД.
- Переконайтеся, що всі необхідні пакети встановлено за допомогою команд `dotnet add package`.
- Для запуску тестів використовуйте команду `dotnet test`.

## Ліцензія

Цей проєкт ліцензовано за ліцензією MIT — детальніше див. файл [LICENSE.md](LICENSE).
