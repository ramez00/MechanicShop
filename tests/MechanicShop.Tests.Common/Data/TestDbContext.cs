using MechanicShop.infrastructure.Data;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace MechanicShop.Tests.Common.Data;

/// <summary>
/// Wraps a real <see cref="AppDbContext"/> backed by a SQLite in-memory database.
/// <para>
/// The SQLite connection must stay open for the lifetime of the context — an in-memory
/// database is destroyed when its last connection closes. Dispose this wrapper to tear
/// everything down.
/// </para>
/// <para>
/// A substitute <see cref="IPublisher"/> is supplied because <see cref="AppDbContext.SaveChangesAsync"/>
/// dispatches domain events through MediatR. Inspect <see cref="Publisher"/> to assert that
/// domain events were published.
/// </para>
/// </summary>
public sealed class TestDbContext : IDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContext Context { get; }

    public IPublisher Publisher { get; }

    public TestDbContext()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Publisher = Substitute.For<IPublisher>();

        Context = new AppDbContext(options, Publisher);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
