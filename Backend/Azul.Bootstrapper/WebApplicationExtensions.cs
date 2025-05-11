using Azul.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace Azul.Bootstrapper;

public static class WebApplicationExtensions
{
    public static void EnsureDatabaseIsCreated(this WebApplication app)
    {
        var scope = app.Services.CreateScope();
        AzulDbContext context = scope.ServiceProvider.GetRequiredService<AzulDbContext>();
        ILogger logger = scope.ServiceProvider.GetRequiredService<ILogger<WebApplication>>();
        try
        {
            //if the sql server container is not created on run docker compose this migration can't fail for network related exception.
            var retry = Policy.Handle<SqlException>()
                .WaitAndRetry(new TimeSpan[]
                {
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(8),
                });
            retry.Execute(() => context.Database.EnsureCreated());

            logger.LogInformation("Created database");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the database");
        }
    }
}