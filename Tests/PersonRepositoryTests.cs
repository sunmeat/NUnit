using NUnit.Framework;
using Microsoft.EntityFrameworkCore;

[TestFixture] // так в NUnit помечаются тестовые классы
public class PersonRepositoryTests
{
    private ApplicationDbContext? _context;
    
    [SetUp] // этот метод выполняется перед каждым тестом
    public void SetUp()
    {
        // используем InMemory базу данных для тестирования
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options;

        _context = new ApplicationDbContext(options);
    }

    // тест для получения всех людей с их увлечениями
    [Test] // таким аттрибутом помечаются все обычные тестовые методы
    public void GetAllPeopleWithHobbies_ShouldReturnPeopleWithHobbies()
    {
        // подготоваливаем данные
        var person1 = new Person { Name = "АлександрТест", Hobbies = new List<Hobby> { new Hobby { Name = "Футбол" }, new Hobby { Name = "Чтение" } } };
        var person2 = new Person { Name = "АртёмТест", Hobbies = new List<Hobby> { new Hobby { Name = "Рисование" }, new Hobby { Name = "Путешествия" } } };
        _context?.People?.AddRange(person1, person2);
        _context?.SaveChanges();

        // извлекаем
        var result = _context?.People?.Include(p => p.Hobbies).ToList();

        // проверки
        Assert.That(result?.Count, Is.EqualTo(2));
        Assert.That(result?[0].Name, Is.EqualTo("АлександрТест"));
        Assert.That(result?[0].Hobbies.Count, Is.EqualTo(2));
        Assert.That(result?[0].Hobbies.ElementAt(0).Name, Is.EqualTo("Футбол"));
    }

    // тест для добавления нового человека с увлечениями
    [Test]
    public void AddPerson_ShouldAddPersonWithHobbies()
    {
        // подготовка данных
        var person = new Person { Name = "Василий", Hobbies = new List<Hobby> { new Hobby { Name = "Чтение" }, new Hobby { Name = "Плавание" } } };

        // сохраняем в БД
        _context?.People?.Add(person);
        _context?.SaveChanges();

        // проверки
        var addedPerson = _context?.People?.FirstOrDefault(p => p.Name == "Василий");
        Assert.That(addedPerson, Is.Not.Null);
        Assert.That(addedPerson?.Name, Is.EqualTo("Василий"));
        Assert.That(addedPerson?.Hobbies.Count, Is.EqualTo(2));
    }

    // тест для удаления человека
    [Test]
    public void DeletePerson_ShouldRemovePerson()
    {
        // подготовка данных и сохранение в БД
        var person = new Person { Name = "Марина" };
        _context?.People?.Add(person);
        _context?.SaveChanges();

        // удаляем
        _context?.People?.Remove(person);
        _context?.SaveChanges();

        // проверяем
        var deletedPerson = _context?.People?.FirstOrDefault(p => p.Name == "Марина");
        Assert.That(deletedPerson, Is.Null);
    }

    // тест для обновления увлечений человека
    [Test]
    public void UpdatePersonHobbies_ShouldUpdateHobbies()
    {
        // подготоваливаем данные
        var person = new Person { Name = "Николай", Hobbies = new List<Hobby> { new Hobby { Name = "Плавание" } } };
        _context?.People?.Add(person);
        _context?.SaveChanges();

        // обновляем увлечения
        person.Hobbies.Clear();
        person.Hobbies.Add(new Hobby { Name = "Готовка" });
        _context?.SaveChanges();

        // проверяем
        var updatedPerson = _context?.People?.Include(p => p.Hobbies).FirstOrDefault(p => p.Name == "Николай");
        Assert.That(updatedPerson, Is.Not.Null);
        Assert.That(updatedPerson?.Hobbies.Count, Is.EqualTo(1));
        Assert.That(updatedPerson?.Hobbies.ElementAt(0).Name, Is.EqualTo("Готовка"));
    }

    // очистка базы данных после выполнения каждого теста
    [TearDown]
    public void TearDown()
    {
        _context?.Database.EnsureDeleted();
    }
}