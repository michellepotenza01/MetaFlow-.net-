using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MetaFlow.API.Data
{
    public class MetaFlowDbContextFactory : IDesignTimeDbContextFactory<MetaFlowDbContext>
    {
        public MetaFlowDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<MetaFlowDbContext>();
            var connectionString = configuration.GetConnectionString("OracleConnection");

            optionsBuilder.UseOracle(connectionString, options =>
            {
                options.MigrationsAssembly("MetaFlow.API");
                options.CommandTimeout(300);
                options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            })
            .AddInterceptors(new OracleCommandInterceptor()); 

            return new MetaFlowDbContext(optionsBuilder.Options);
        }
    }
}