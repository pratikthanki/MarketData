using System;
using System.Threading;
using System.Threading.Tasks;
using MarketData.Gateway.Extensions;
using MarketData.Gateway.Models;
using Microsoft.Extensions.Logging;
using OpenTracing;

namespace MarketData.Gateway.Services
{
    public interface IValidationClient
    {
        Task<ValidationResult> ValidateAsync(FxQuote fxQuote, CancellationToken cancellationToken);
    }

    public class ValidationClient : IValidationClient
    {
        private readonly ILogger _logger;
        private readonly ITracer _tracer;

        public ValidationClient(ILogger<ValidationClient> logger, ITracer tracer)
        {
            _logger = logger;
            _tracer = tracer;
        }

        public Task<ValidationResult> ValidateAsync(FxQuote fxQuote, CancellationToken cancellationToken)
        {
            using var scope = _tracer.BuildTrace(nameof(ValidateAsync));

            scope.LogStart(nameof(ValidateAsync));

            var result = new ValidationResult { Id = CreateId(), IsSuccessful = true };
            _logger.LogInformation("Validated {MarketData} with {Id}", nameof(FxQuote), result.Id);

            scope.LogEnd(nameof(ValidateAsync));

            return Task.FromResult(result);
        }

        private string CreateId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}