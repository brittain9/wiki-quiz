using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Data;

// This class is needed to run migrations even if it's not called at all. Internal so we dont call this outside of this project
internal class WikiQuizDbContextFactory : IDesignTimeDbContextFactory<WikiQuizDbContext>
{
    public WikiQuizDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WikiQuizDbContext>();
        optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"));

        return new WikiQuizDbContext(optionsBuilder.Options);
    }
}
