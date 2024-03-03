using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;

namespace TTPService.Logging.Telemetry
{
    public class ApplicationInsightsLoggingMiddleware : IMiddleware
    {
        private const string RequestBody = "RequestBody";
        private const string ResponseBody = "ResponseBody";

        public ApplicationInsightsLoggingMiddleware(TelemetryClient telemetryClient)
        {
            TelemetryClient = telemetryClient;
        }

        private TelemetryClient TelemetryClient { get; }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Inbound (before the controller)
            var request = context?.Request;
            if (request == null)
            {
                await next(context);
                return;
            }

            request.EnableBuffering();  // Allows us to reuse the existing Request.Body

            // Swap the original Response.Body stream with one we can read / seek
            var originalResponseBody = context.Response.Body;
            using var replacementResponseBody = new MemoryStream();
            context.Response.Body = replacementResponseBody;

            await next(context); // Continue processing (additional middleware, controller, etc.)

            // Outbound (after the controller)
            replacementResponseBody.Position = 0;

            // Copy the response body to the original stream
            await replacementResponseBody.CopyToAsync(originalResponseBody).ConfigureAwait(false);
            context.Response.Body = originalResponseBody;

            var requestTelemetry = context.Features.Get<RequestTelemetry>();
            if (requestTelemetry == null)
            {
                return;
            }

            if (request.Body.CanRead)
            {
                var requestBodyString = await ReadBodyStream(request.Body).ConfigureAwait(false);
                AddTelemetry(requestTelemetry, RequestBody, requestBodyString);
            }

            if (replacementResponseBody.CanRead)
            {
                var responseBodyString = await ReadBodyStream(replacementResponseBody).ConfigureAwait(false);
                AddTelemetry(requestTelemetry, ResponseBody, responseBodyString);
            }
        }

        private async Task<string> ReadBodyStream(Stream body)
        {
            if (body.Length == 0)
            {
                return null;
            }

            body.Position = 0;

            using var reader = new StreamReader(body, leaveOpen: true);
            var bodyString = await reader.ReadToEndAsync().ConfigureAwait(false);
            body.Position = 0;

            return bodyString;
        }

        private void AddTelemetry(RequestTelemetry requestTelemetry, string name, string value)
        {
            requestTelemetry.Properties.Add(name, value); // limit: 8192 characters
            if (!string.IsNullOrEmpty(value) && value.Length >= 8192)
            {
                var overflow = $"{name}Overflow";
                var overflowValue = true.ToString().ToLower();
                requestTelemetry.Properties.Add(overflow, overflowValue);
                TelemetryClient.TrackTrace(value, new Dictionary<string, string> { { overflow, overflowValue } });
            }
        }
    }
}