using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure;
using ElixBackend.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Tests.Infrastructure.Repository;

[TestFixture]
public class UserRepositoryTest
{
    private ElixDbContext _context;
    private UserRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ElixDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ElixDbContext(options);
        _repository = new UserRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddUserAsync_AddsUserAndReturnsIt()
    {
        var user = new User
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            PasswordHash = "hash"
        };

        var result = await _repository.AddUserAsync(user);
        await _repository.SaveChangesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo("john@doe.com"));
        Assert.That(await _context.Users.AnyAsync(u => u.Email == "john@doe.com"), Is.True);
    }

    [Test]
    public async Task GetUserByIdAsync_ReturnsUser()
    {
        var user = new User
        {
            Firstname = "Jane",
            Lastname = "Smith",
            Email = "jane@smith.com",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserByIdAsync(user.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo("jane@smith.com"));
    }

    [Test]
    public async Task GetUserByEmailAsync_ReturnsUser()
    {
        var user = new User
        {
            Firstname = "Alice",
            Lastname = "Wonder",
            Email = "alice@wonder.com",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserByEmailAsync("alice@wonder.com");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Firstname, Is.EqualTo("Alice"));
    }

    [Test]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        var users = new List<User>
        {
            new User { Firstname = "A", Lastname = "A", Email = "a@a.com", PasswordHash = "hash" },
            new User { Firstname = "B", Lastname = "B", Email = "b@b.com", PasswordHash = "hash" }
        };
        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllUsersAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task UpdateUserAsync_UpdatesUser()
    {
        var user = new User
        {
            Firstname = "Bob",
            Lastname = "Builder",
            Email = "bob@builder.com",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        user.Firstname = "Robert";
        var updated = await _repository.UpdateUserAsync(user);
        await _repository.SaveChangesAsync();

        var result = await _repository.GetUserByIdAsync(user.Id);
        Assert.That(result?.Firstname, Is.EqualTo("Robert"));
    }

    [Test]
    public async Task DeleteUserAsync_RemovesUser()
    {
        var user = new User
        {
            Firstname = "Del",
            Lastname = "User",
            Email = "del@user.com",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _repository.DeleteUserAsync(user.Id);
        await _repository.SaveChangesAsync();

        var exists = await _context.Users.AnyAsync(u => u.Id == user.Id);
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task UserExistsAsync_ReturnsTrueIfExists()
    {
        var user = new User
        {
            Firstname = "Exist",
            Lastname = "User",
            Email = "exist@user.com",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var exists = await _repository.UserExistsAsync(user.Id);
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task UserExistsAsync_ReturnsFalseIfNotExists()
    {
        var exists = await _repository.UserExistsAsync(999);
        Assert.That(exists, Is.False);
    }
}