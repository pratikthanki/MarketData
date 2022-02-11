using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MarketData.Gateway.Extensions;
using MarketData.Gateway.Models;
using Microsoft.Extensions.Logging;
using OpenTracing;

namespace MarketData.Gateway.Services
{
    public interface IGatewayService
    {
        Task<ValidationResult> ProcessAsync(FxQuote fxQuote, CancellationToken cancellationToken);
        Task<FxQuote> RetrieveAsync(Guid guid, CancellationToken cancellationToken);
    }

    public class GatewayService : IGatewayService
    {
        private readonly ILogger _logger;
        private readonly ITracer _tracer;
        private readonly IValidationClient _validationClient;

        private readonly ConcurrentDictionary<Guid, FxQuote> _fxQuotes;

        public GatewayService(
            ILogger<GatewayService> logger,
            ITracer tracer,
            IValidationClient validationClient)
        {
            _logger = logger;
            _tracer = tracer;
            _validationClient = validationClient;

            _fxQuotes = new ConcurrentDictionary<Guid, FxQuote>();
        }

        public async Task<ValidationResult> ProcessAsync(FxQuote fxQuote, CancellationToken cancellationToken)
        {
            using var scope = _tracer.BuildTrace(nameof(ProcessAsync));
            scope.LogStart(nameof(ProcessAsync));

            var result = await _validationClient.ValidateAsync(fxQuote, cancellationToken);

            scope.LogEnd(nameof(ProcessAsync));

            // Return the validation result here as it was unsuccessful (i.e. no need to persist)
            if (!result.IsSuccessful)
            {
                return result;
            }

            var guid = new Guid(result.Id);
            _fxQuotes[guid] = fxQuote;

            return result;
        }

        public async Task<FxQuote> RetrieveAsync(Guid guid, CancellationToken cancellationToken)
        {
            using var scope = _tracer.BuildTrace(nameof(RetrieveAsync));
            scope.LogStart(nameof(RetrieveAsync));

            _logger.LogInformation("Fetching FxQuote with {Guid}", guid);
            var quote = _fxQuotes.TryGetValue(guid, out var fxQuote) ? fxQuote : null;

            scope.LogEnd(nameof(RetrieveAsync));

            return await Task.FromResult(quote);
        }
    }
}