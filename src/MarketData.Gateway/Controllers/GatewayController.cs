using System;
using System.Threading;
using System.Threading.Tasks;
using MarketData.Gateway.Extensions;
using MarketData.Gateway.Models;
using MarketData.Gateway.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
using OpenTracing;

namespace MarketData.Gateway.Controllers
{
    /// <summary>
    /// Market Data Gateway
    /// </summary>
    [Route("api/gateway")]
    [ApiController]
    public class GatewayController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ITracer _tracer;
        private readonly IGatewayService _gatewayService;

        public GatewayController(
            ILogger<GatewayController> logger,
            ITracer tracer,
            IGatewayService gatewayService)
        {
            _logger = logger;
            _tracer = tracer;
            _gatewayService = gatewayService;
        }

        /// <summary>
        /// Process a market data contribution through the gateway and receive
        /// either a successful or unsuccessful response.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ProcessAsync(
            [FromBody] ContributionRequest request,
            CancellationToken cancellationToken)
        {
            _tracer.ActiveSpan.SetTags(
                HttpMethod.Post.ToString(),
                HttpContext.Request.GetDisplayUrl(),
                HttpContext.Connection.RemoteIpAddress?.MapToIPv6().ToString());

            ValidationResult validationResult;

            if (request.MarketDataType == MarketDataType.FxQuote)
            {
                validationResult = await _gatewayService.ProcessAsync(request.FxQuote, cancellationToken);
            }
            else
            {
                return BadRequest("Unrecognized Market Data Type provided");
            }

            return StatusCode(StatusCodes.Status201Created, validationResult);
        }

        /// <summary>
        /// Retrieve the details of a previously contributed market data.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> RetrieveAsync([FromQuery] string uniqueId, CancellationToken cancellationToken)
        {
            _tracer.ActiveSpan.SetTags(
                HttpMethod.Get.ToString(),
                HttpContext.Request.GetDisplayUrl(),
                HttpContext.Connection.RemoteIpAddress?.MapToIPv6().ToString());

            if (string.IsNullOrEmpty(uniqueId))
            {
                return BadRequest("Null or empty unique identifier provided");
            }

            Guid guid;
            try
            {
                guid = new Guid(uniqueId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid unique Id provided. {Guid}", uniqueId);
                return BadRequest($"Invalid uniqueId provided - {uniqueId}");
            }

            var result = await _gatewayService.RetrieveAsync(guid, cancellationToken);

            // ReSharper disable once InvertIf
            if (result is null)
            {
                _logger.LogWarning("Failed to find FxQuote. {Guid}", uniqueId);
                return NotFound();
            }

            return Ok(result);
        }
    }
}