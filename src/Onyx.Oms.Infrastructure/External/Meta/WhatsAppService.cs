using Microsoft.Extensions.Logging;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Onyx.Oms.Infrastructure.External.Meta
{
    internal class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(HttpClient httpClient, ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Result<string>> UploadMediaAsync(byte[] fileBytes, string fileName, string mimeType, CancellationToken cancellationToken = default)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent("whatsapp"), "messaging_product");

            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
            content.Add(fileContent, "file", fileName);

            try
            {
                var response = await _httpClient.PostAsync("media", content, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadFromJsonAsync<MediaUploadResponse>(cancellationToken: cancellationToken);
                    return Result.Success(responseData?.Id ?? string.Empty);
                }

                //var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                //_logger.LogWarning("WhatsApp Media Upload failed. Status: {Status}. Body: {Body}", response.StatusCode, errorContent);
                //return Result.Failure<string>(Error.ThirdPartyService("Meta.MediaUploadFailed", "Failed to upload media to WhatsApp."));
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("WhatsApp API failed. Status: {Status}. Body: {Body}", response.StatusCode, errorContent);

                // Try to extract Meta's specific error message
                string metaErrorMessage = "An unknown error occurred.";
                try
                {
                    var metaError = JsonSerializer.Deserialize<MetaErrorResponse>(errorContent);
                    if (metaError?.Error != null)
                    {
                        metaErrorMessage = $"{metaError.Error.Type}: {metaError.Error.Message} (Code: {metaError.Error.Code})";
                    }
                }
                catch { /* Keep default message if deserialization fails */ }

                return response.StatusCode switch
                {
                    HttpStatusCode.TooManyRequests =>
                        Result.Failure<string>(Error.RateLimitExceeded("Meta.RateLimit", "WhatsApp API rate limit exceeded. Please try again later.")),

                    HttpStatusCode.ServiceUnavailable or HttpStatusCode.InternalServerError or HttpStatusCode.BadGateway =>
                        Result.Failure<string>(Error.ServiceUnavailable("Meta.Unavailable", "WhatsApp services are currently experiencing downtime.")),

                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                        Result.Failure<string>(Error.InvalidConfiguration("Meta.AuthFailed", "Authentication with WhatsApp failed. Check your API tokens.")),

                    // Catch-all for 400 Bad Request (invalid phone numbers, unapproved templates, etc.)
                    _ => Result.Failure<string>(Error.ThirdPartyService("Meta.ApiError", metaErrorMessage))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Network failure during WhatsApp media upload.");
                return Result.Failure<string>(Error.ServiceUnavailable("Meta.NetworkFailure", "Could not reach the WhatsApp servers."));
            }
        }

        public async Task<Result<string>> SendDocumentTemplateMessageAsync(
            string toPhoneNumber,
            string templateName,
            string languageCode,
            string mediaId,
            List<string> bodyVariables,
            CancellationToken cancellationToken = default)
        {
            // Map the string variables to Meta's expected parameter object
            var textParameters = bodyVariables.Select(v => new { type = "text", value = v }).ToList();

            var payload = new
            {
                messaging_product = "whatsapp",
                to = toPhoneNumber,
                type = "template",
                template = new
                {
                    name = templateName,
                    language = new { code = languageCode },
                    components = new object[]
                    {
                        new // header component
                        {
                            type = "header",
                            parameters = new object[]
                            {
                                new { type = "document", document = new { id = mediaId}}
                            }
                        },
                        new // body
                        {
                            type = "body",
                            parameters = textParameters
                        }
                    }
                }
            };

            return await ExecuteMessagePostAsync(payload, cancellationToken);
        }

        private async Task<Result<string>> ExecuteMessagePostAsync(object payload, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("messages", payload, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadFromJsonAsync<WhatsAppSuccessResponse>(cancellationToken: cancellationToken);
                    var messageId = responseData?.Messages?.FirstOrDefault()?.Id ?? "UnknownId";
                    return Result.Success(messageId);
                }

                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("WhatsApp API failed. Status: {Status}. Body: {Body}", response.StatusCode, errorContent);

                // Try to extract Meta's specific error message
                string metaErrorMessage = "An unknown error occurred.";
                try
                {
                    var metaError = JsonSerializer.Deserialize<MetaErrorResponse>(errorContent);
                    if (metaError?.Error != null)
                    {
                        metaErrorMessage = $"{metaError.Error.Type}: {metaError.Error.Message} (Code: {metaError.Error.Code})";
                    }
                }
                catch { /* Keep default message if deserialization fails */ }

                return response.StatusCode switch
                {
                    HttpStatusCode.TooManyRequests =>
                        Result.Failure<string>(Error.RateLimitExceeded("Meta.RateLimit", "WhatsApp API rate limit exceeded. Please try again later.")),

                    HttpStatusCode.ServiceUnavailable or HttpStatusCode.InternalServerError or HttpStatusCode.BadGateway =>
                        Result.Failure<string>(Error.ServiceUnavailable("Meta.Unavailable", "WhatsApp services are currently experiencing downtime.")),

                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                        Result.Failure<string>(Error.InvalidConfiguration("Meta.AuthFailed", "Authentication with WhatsApp failed. Check your API tokens.")),

                    // Catch-all for 400 Bad Request (invalid phone numbers, unapproved templates, etc.)
                    _ => Result.Failure<string>(Error.ThirdPartyService("Meta.ApiError", metaErrorMessage))
                };
            }
            catch (HttpRequestException ex)
            {
                // Handle actual network drops/DNS failures
                _logger.LogError(ex, "Network failure while reaching WhatsApp API.");
                return Result.Failure<string>(Error.ServiceUnavailable("Meta.NetworkFailure", "Could not reach the WhatsApp servers."));
            }
        }

        private record MediaUploadResponse([property: JsonPropertyName("id")] string Id);

        private record WhatsAppSuccessResponse([property: JsonPropertyName("messages")] List<WhatsAppMessage> Messages);
        private record WhatsAppMessage([property: JsonPropertyName("id")] string Id);

        private record MetaErrorResponse([property: JsonPropertyName("error")] MetaErrorDetails Error);
        private record MetaErrorDetails(
            [property: JsonPropertyName("message")] string Message,
            [property: JsonPropertyName("type")] string Type,
            [property: JsonPropertyName("code")] int Code);
    }
}
