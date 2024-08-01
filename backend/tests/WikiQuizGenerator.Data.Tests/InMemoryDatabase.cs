using Microsoft.EntityFrameworkCore;
using WikiQuizGenerator.Data;

namespace WikiQuizGenerator.Data;

internal static class TestContext
{
    public static WikiQuizDbContext CreateInMemoryDatabase()
    {
        var options = new DbContextOptionsBuilder<WikiQuizDbContext>()
            .UseInMemoryDatabase(databaseName: "WikiQuizTestDb")
            .Options;

        var context = new WikiQuizDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }
}