// список команд для установки библиотек:
/*
dotnet add package NUnit
dotnet add package NUnit3TestAdapter
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Microsoft.NET.Test.Sdk
*/

// в файле .csproj (клик по названию проекта) нужно добавить <StartupObject>Program</StartupObject> в элемент <PropertyGroup> 

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
        // используем InMemory БД для тестирования
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase("TestDatabase");
        }
    }
}

public class PersonRepository
{
    // контекст БД теперь передаётся как параметр в конструктор
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
            Console.WriteLine($"Человек: {person.Name}");
            foreach (var hobby in person.Hobbies)
            {
                Console.WriteLine($"  Увлечение: {hobby.Name}");
            }
        }
    }
}

internal class Program
{
    static void Main()
    {
        // для реальной работы с SQL Server используем привычную строку подключения
        // var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        //    .UseSqlServer("Server=localhost;Database=PeopleHobbies;Trusted_Connection=True;TrustServerCertificate=True;")
        //    .Options;

        // для тестов с InMemory
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
             .UseInMemoryDatabase("TestDatabase")
             .Options;

        using var context = new ApplicationDbContext(options);
        var repo = new PersonRepository(context);

        // добавление данных
        var person1 = new Person { Name = "Александр", Hobbies = new List<Hobby> { new Hobby { Name = "Футбол" }, new Hobby { Name = "Чтение" } } };
        var person2 = new Person { Name = "Артём", Hobbies = new List<Hobby> { new Hobby { Name = "Рисование" }, new Hobby { Name = "Путешествия" } } };
        context.People?.AddRange(person1, person2);
        context.SaveChanges();

        // получение и вывод всех людей с увлечениями
        repo.GetAllPeopleWithHobbies();
    }
}

// запуск тестов командой
// dotnet test