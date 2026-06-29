using System.Diagnostics.CodeAnalysis;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Tests.TestHelpers;

[method: SetsRequiredMembers]
internal class TestAppDbContext(DbContextOptions options) : AppDbContext(options) { }

public static class TestDbContextFactory
{
    public static AppDbContext Create(string? databaseName = null)
    {
        var name = databaseName ?? Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(name).Options;

        return new TestAppDbContext(options);
    }
}
