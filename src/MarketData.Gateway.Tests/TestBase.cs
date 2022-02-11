using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarketData.Gateway.Tests
{
    public abstract class TestBase : IDisposable
    {
        private readonly HttpClient _client;

        protected TestBase()
        {
            var factory = new WebApplicationFactory<Startup>();
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder
                    .ConfigureAppConfiguration(ConfigureAppConfiguration)
                    .ConfigureTestServices(ConfigureTestServices);

            }).CreateClient();
        }

        protected abstract void ConfigureMockServices(IServiceCollection services);

        protected virtual void ConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder builder)
        {
            builder.Sources.Clear();
            builder.AddInMemoryCollection();
        }

        protected void ConfigureTestServices(IServiceCollection services)
        {
            ConfigureMockServices(services);
        }

        protected async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage requestMessage)
        {
            return await _client.SendAsync(requestMessage);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}