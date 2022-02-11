using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace MarketData.Gateway.Tests
{
    public abstract class TestBase : IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory = new();
        private HttpClient _client;

        [SetUp]
        protected void SetUp()
        {
            _client = _factory.WithWebHostBuilder(builder =>
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
            _factory?.Dispose();
            _client?.Dispose();
        }
    }
}