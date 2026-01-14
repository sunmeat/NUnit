// список команд для встановлення пакетів:
/*
dotnet add package NUnit
dotnet add package NUnit3TestAdapter
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Microsoft.NET.Test.Sdk
*/
// у файлі .csproj (клік по назві проєкту) потрібно додати <StartupObject>Program</StartupObject> в елемент <PropertyGroup>

using Microsoft.EntityFrameworkCore;

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

public class Hobby
{
    public int HobbyId { get; set; }
    public string? Name { get; set; }
    public ICollection<Person> People { get; set; } = new List<Person>();
}

public class ApplicationDbContext : DbContext
{
    public virtual DbSet<Person>? People { get; set; }
    public virtual DbSet<Hobby>? Hobbies { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public ApplicationDbContext() { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // використовуємо InMemory БД для тестування
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase("TestDatabase");
        }
    }
}

public class PersonRepository
{
    // контекст БД тепер передається як параметр у конструктор
    private readonly ApplicationDbContext _context;

    public PersonRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void GetAllPeopleWithHobbies()
    {
        var people = _context.People?
                             .Include(p => p.Hobbies)
                             .ToList() ?? new List<Person>();

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

internal class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "Юніт тести в EF Core";

        // для реальної роботи з SQL Server використовуємо звичний рядок підключення
        // var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        //     .UseSqlServer("Server=localhost;Database=PeopleHobbies;Trusted_Connection=True;TrustServerCertificate=True;")
        //     .Options;

        // для тестів з InMemory
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options;

        using var context = new ApplicationDbContext(options);
        var repo = new PersonRepository(context);

        // додавання даних
        var person1 = new Person
        {
            Name = "Олександр",
            Hobbies = new List<Hobby>
            {
                new Hobby { Name = "Футбол" },
                new Hobby { Name = "Читання" }
            }
        };

        var person2 = new Person
        {
            Name = "Артем",
            Hobbies = new List<Hobby>
            {
                new Hobby { Name = "Малювання" },
                new Hobby { Name = "Подорожі" }
            }
        };

        context.People?.AddRange(person1, person2);
        context.SaveChanges();

        // отримання та вивід усіх людей із захопленнями
        repo.GetAllPeopleWithHobbies();

        Console.WriteLine("\nНатисніть будь-яку клавішу для завершення...");
        Console.ReadKey();
    }
}

// запуск тестів командою
// dotnet test
