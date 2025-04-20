using Azul.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Azul.Api.Tests.Util
{
    internal class TestWebApplicationFactory(Action<IServiceCollection>? overrideDependencies = null)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                overrideDependencies?.Invoke(services);
            });
        }

    }
}
