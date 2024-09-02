using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Data;

// This class is needed to run migrations even if it's not called at all
// is this still needed?
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
