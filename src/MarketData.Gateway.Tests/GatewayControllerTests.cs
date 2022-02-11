using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MarketData.Gateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MarketData.Gateway.Tests
{
    public class GatewayControllerTests : TestBase
    {
        private readonly Mock<IGatewayService> _gatewayServiceMock = new();
        
        [Fact]
        public async Task Gateway_Post()
        {
            // _gatewayServiceMock
            //     .Setup(g => g.ProcessAsync(It.IsAny<FxQuote>(), It.IsAny<CancellationToken>()))
            //     .ReturnsAsync(new ValidationResult() { Id = "some-id", IsSuccessful = true });

            var message = new HttpRequestMessage()
            {
                RequestUri = new Uri("api/gateway", UriKind.Relative),
                Method = HttpMethod.Post,
            };
           
            var response = await SendRequestAsync(message);
            var responseString = await response.Content.ReadAsStringAsync();
            
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        protected override void ConfigureMockServices(IServiceCollection services)
        {
            services.AddSingleton(_gatewayServiceMock.Object);
        }
    }
}