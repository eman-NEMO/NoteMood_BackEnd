using Azure;
using Microsoft.Extensions.Logging; // Add this for logging
using NoteMoodUOW.Core.Configurations;
using NoteMoodUOW.Core.Dtos.MachineDtos;
using NoteMoodUOW.Core.Interfaces;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NoteMoodUOW.EF.Repositories
{
    /// <summary>
    /// Provides functionality to interact with an external Flask API for machine learning functions.
    /// </summary>
    public class MachineAPI : IMachineAPI
    {
        private readonly Configuration _configuration;
        ///private readonly ILogger<MachineAPI> _logger; // Logger instance

        /// <summary>
        /// Initializes a new instance of the <see cref="MachineAPI"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration containing API settings.</param>
        /// <param name="logger">The logger instance for logging.</param>
        public MachineAPI(Configuration configuration/*, ILogger<MachineAPI> logger*/)
        {
            _configuration = configuration;
            ///_logger = logger;
        }

        /// <summary>
        /// Calls an external Flask API with retry logic, handling timeouts and other exceptions.
        /// </summary>
        /// <typeparam name="TResponse">The expected response type.</typeparam>
        /// <param name="content">The content to be sent to the Flask API.</param>
        /// <param name="url">The endpoint URL of the Flask API.</param>
        /// <returns>The deserialized response from the Flask API or an error object.</returns>
        public async Task<TResponse?> callFlaskAPI<TResponse>(string content, string url)
        {
            int maxRetries = 3; // Maximum number of retries
            int retryCount = 0; // Current retry attempt
            double retryTimeoutIncreaseFactor = 1.5; // Factor by which to increase the timeout on each retry

            while (retryCount < maxRetries)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        // Adjust the timeout based on the retry count
                        double adjustedTimeoutFactor = Math.Pow(retryTimeoutIncreaseFactor, retryCount);
                        client.Timeout = CalculateTimeout(content, adjustedTimeoutFactor);

                        var requestUrl = _configuration.Machine.Url + url;
                        var token = _configuration.Machine.BearerToken;

                        // Set the authorization header with the bearer token
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        // Serialize the request content
                        var request = new SentimentRequest { input_journal = content };
                        var jsonRequest = JsonSerializer.Serialize(request);

                        var requestContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                        // Send the request to the Flask API
                        var response = await client.PostAsync(requestUrl, requestContent);

                        // Check for a successful response
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            // Log and return an error response if the status code is not OK
                            var errorResponseJson = $"{{ \"errorMessage\": \"Server closed or request failed\", \"statusCode\": {(int)response.StatusCode} }}";
                            //_logger.LogWarning(errorResponseJson);
                            return JsonSerializer.Deserialize<TResponse>(errorResponseJson);
                        }

                        // Deserialize and return the successful response
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        return JsonSerializer.Deserialize<TResponse>(jsonResponse);
                    }
                }
                catch (TaskCanceledException ex)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        // Log and return a timeout error after max retries
                        var timeoutResponseJson = $"{{ \"errorMessage\": \"Request timed out after {retryCount} retries\", \"exceptionMessage\": \"{ex.Message}\" }}";
                        //_logger.LogError(timeoutResponseJson);
                        return JsonSerializer.Deserialize<TResponse>(timeoutResponseJson);
                    }
                    // Log the retry attempt
                    ///_logger.LogInformation($"Timeout occurred, retrying {retryCount}/{maxRetries}...");
                }
                catch (Exception ex)
                {
                    // Log and return a generic error for any other exceptions
                    var errorResponseJson = $"{{ \"errorMessage\": \"An error occurred\", \"exceptionMessage\": \"{ex.Message}\" }}";
                    ///_logger.LogError(errorResponseJson);
                    return JsonSerializer.Deserialize<TResponse>(errorResponseJson);
                }
            }

            // Return default if the loop exits without returning (should not happen)
            return default;
        }

        /// <summary>
        /// Calculates the timeout for the HTTP client based on the content length and retry attempt.
        /// </summary>
        /// <param name="content">The content to be sent to the Flask API.</param>
        /// <param name="adjustmentFactor">The adjustment factor based on the current retry attempt.</param>
        /// <returns>The calculated timeout as a <see cref="TimeSpan"/>.</returns>
        private TimeSpan CalculateTimeout(string content, double adjustmentFactor = 1)
        {
            double baseTimeoutSeconds = 300; // Base timeout of 300 seconds (5 minutes)
            double timeoutFactorPerCharacter = 0.1; // Timeout factor per character

            // Calculate the total timeout based on content length and adjustment factor
            double totalTimeoutSeconds = (baseTimeoutSeconds + (content.Length * timeoutFactorPerCharacter)) * adjustmentFactor;

            return TimeSpan.FromSeconds(totalTimeoutSeconds);
        }
    }
}