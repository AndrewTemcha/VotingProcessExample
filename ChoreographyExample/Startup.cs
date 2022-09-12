using ChoreographyExample;
using ChoreographyExample.DAL;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

[assembly: FunctionsStartup(typeof(Startup))]
namespace ChoreographyExample
{
    public class Startup : FunctionsStartup
    {
        // override
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddDbContextFactory<VotingDbContext>(
                options =>
                options.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=ContosoUniversity1;Integrated Security=SSPI;"));
        }
    }
}
