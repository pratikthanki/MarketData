using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MarketData.Gateway.Models;
using MarketData.Gateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace MarketData.Gateway.Tests
{
    public class GatewayControllerTests : TestBase
    {
        private readonly Mock<IGatewayService> _gatewayServiceMock = new();

        [Test]
        public async Task Gateway_Post_WithoutBody()
        {
            var message = new HttpRequestMessage()
            {
                RequestUri = new Uri("api/gateway", UriKind.Relative),
                Method = HttpMethod.Post,
            };

            message.Content = new StringContent("");
           
            var response = await SendRequestAsync(message);
            Assert.AreEqual(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }
        
        [Test]
        public async Task Gateway_Post_WithBodySuccessful()
        {
            _gatewayServiceMock
                .Setup(g => g.ProcessAsync(It.IsAny<FxQuote>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult() { Id = "some-id", IsSuccessful = true });

            var contributionRequest =
                "{\"FxQuote\": {\"Currency\": \"eur\", \"bid\": 1.0, \"ask\": 1.0},\"marketdatatype\": \"fxquote\"}";

            var message = new HttpRequestMessage()
            {
                RequestUri = new Uri("api/gateway", UriKind.Relative),
                Method = HttpMethod.Post,
            };

            message.Content = new StringContent(contributionRequest, Encoding.UTF8, "application/json");

            var response = await SendRequestAsync(message);
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }
        
        [Test]
        public async Task Gateway_Post_WithBody_InvalidMarketDataType()
        {
            _gatewayServiceMock
                .Setup(g => g.ProcessAsync(It.IsAny<FxQuote>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult() { Id = "some-id", IsSuccessful = true });

            var contributionRequest =
                "{\"FxQuote\": {\"Currency\": \"eur\", \"bid\": 1.0, \"ask\": 1.0},\"marketdatatype\": \"-1\"}";

            var message = new HttpRequestMessage()
            {
                RequestUri = new Uri("api/gateway", UriKind.Relative),
                Method = HttpMethod.Post,
            };

            message.Content = JsonContent.Create(contributionRequest);

            var response = await SendRequestAsync(message);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        protected override void ConfigureMockServices(IServiceCollection services)
        {
            services.AddSingleton(_gatewayServiceMock.Object);
        }
    }
}