using NUnit.Framework;
using Microsoft.EntityFrameworkCore;

[TestFixture] // так у NUnit позначають тестові класи
public class PersonRepositoryTests
{
    private ApplicationDbContext? _context;

    [SetUp] // цей метод виконується перед кожним тестом
    public void SetUp()
    {
        // використовуємо InMemory базу даних для тестування
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options;
        _context = new ApplicationDbContext(options);
    }

    // тест для отримання всіх людей разом з їхніми захопленнями
    [Test] // таким атрибутом позначають усі звичайні тестові методи
    public void GetAllPeopleWithHobbies_ShouldReturnPeopleWithHobbies()
    {
        // готуємо дані
        var person1 = new Person { Name = "ОлександрТест", Hobbies = new List<Hobby> { new Hobby { Name = "Футбол" }, new Hobby { Name = "Читання" } } };
        var person2 = new Person { Name = "АртемТест", Hobbies = new List<Hobby> { new Hobby { Name = "Малювання" }, new Hobby { Name = "Подорожі" } } };
        _context?.People?.AddRange(person1, person2);
        _context?.SaveChanges();

        // отримуємо
        var result = _context?.People?.Include(p => p.Hobbies).ToList();

        // перевірки
        Assert.That(result?.Count, Is.EqualTo(2));
        Assert.That(result?[0].Name, Is.EqualTo("ОлександрТест"));
        Assert.That(result?[0].Hobbies.Count, Is.EqualTo(2));
        Assert.That(result?[0].Hobbies.ElementAt(0).Name, Is.EqualTo("Футбол"));
    }

    // тест для додавання нової людини з захопленнями
    [Test]
    public void AddPerson_ShouldAddPersonWithHobbies()
    {
        // підготовка даних
        var person = new Person { Name = "Василь", Hobbies = new List<Hobby> { new Hobby { Name = "Читання" }, new Hobby { Name = "Плавання" } } };

        // зберігаємо в БД
        _context?.People?.Add(person);
        _context?.SaveChanges();

        // перевірки
        var addedPerson = _context?.People?.FirstOrDefault(p => p.Name == "Василь");
        Assert.That(addedPerson, Is.Not.Null);
        Assert.That(addedPerson?.Name, Is.EqualTo("Василь"));
        Assert.That(addedPerson?.Hobbies.Count, Is.EqualTo(2));
    }

    // тест для видалення людини
    [Test]
    public void DeletePerson_ShouldRemovePerson()
    {
        // підготовка даних та збереження в БД
        var person = new Person { Name = "Марина" };
        _context?.People?.Add(person);
        _context?.SaveChanges();

        // видаляємо
        _context?.People?.Remove(person);
        _context?.SaveChanges();

        // перевіряємо
        var deletedPerson = _context?.People?.FirstOrDefault(p => p.Name == "Марина");
        Assert.That(deletedPerson, Is.Null);
    }

    // тест для оновлення захоплень людини
    [Test]
    public void UpdatePersonHobbies_ShouldUpdateHobbies()
    {
        // готуємо дані
        var person = new Person { Name = "Микола", Hobbies = new List<Hobby> { new Hobby { Name = "Плавання" } } };
        _context?.People?.Add(person);
        _context?.SaveChanges();

        // оновлюємо захоплення
        person.Hobbies.Clear();
        person.Hobbies.Add(new Hobby { Name = "Готування" });
        _context?.SaveChanges();

        // перевіряємо
        var updatedPerson = _context?.People?.Include(p => p.Hobbies).FirstOrDefault(p => p.Name == "Микола");
        Assert.That(updatedPerson, Is.Not.Null);
        Assert.That(updatedPerson?.Hobbies.Count, Is.EqualTo(1));
        Assert.That(updatedPerson?.Hobbies.ElementAt(0).Name, Is.EqualTo("Готування"));
    }

    // очищення бази даних після виконання кожного тесту
    [TearDown]
    public void TearDown()
    {
        _context?.Database.EnsureDeleted();
    }
}
