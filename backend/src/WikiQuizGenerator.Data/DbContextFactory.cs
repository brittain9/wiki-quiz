using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WikiQuizGenerator.Data;

// This class is needed to run migrations. Removing it causes errors
internal class WikiQuizDbContextFactory : IDesignTimeDbContextFactory<WikiQuizDbContext>
{
    public WikiQuizDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WikiQuizDbContext>();
        optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"),
            npgsqlOptions => npgsqlOptions.UseNodaTime());

        return new WikiQuizDbContext(optionsBuilder.Options);
    }
}
