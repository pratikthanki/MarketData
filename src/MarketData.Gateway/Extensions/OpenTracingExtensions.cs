using OpenTracing;
using OpenTracing.Tag;

namespace MarketData.Gateway.Extensions
{
    public static class OpenTracingExtensions
    {
        public static void SetTags(
            this ISpan span,
            string httpMethod,
            string displayUrl,
            string ipv6Address)
        {
            span?.SetTag(Tags.SpanKind, Tags.SpanKindClient)
                .SetTag(Tags.HttpMethod, httpMethod)
                .SetTag(Tags.HttpUrl, displayUrl)
                .SetTag(Tags.PeerHostIpv6, ipv6Address);
        }

        public static IScope BuildTrace(this ITracer tracer, string actionName)
        {
            return tracer?.BuildSpan(actionName).StartActive(true);
        }

        public static void LogStart(this IScope scope, string method)
        {
            scope?.Span.Log($"Start{method}");
        }

        public static void LogEnd(this IScope scope, string method)
        {
            scope?.Span.Log($"End{method}");
        }

        public static void LogEvent(this IScope scope, params (string, object)[] args)
        {
            foreach (var arg in args)
                scope?.Span.Log($"{arg.Item1}: {arg.Item2}");
        }
    }
}