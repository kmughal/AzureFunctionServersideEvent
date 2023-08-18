using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PartialContents;

public static class SSESample
{
    [FunctionName("SSE")]
    public static async Task<HttpResponseMessage> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        try
        {
            var count = 0;
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new PushStreamContent(async (stream, content, context) =>
            {
                using var streamWriter = new StreamWriter(stream);
                    // Send initial SSE message
                    await streamWriter.WriteLineAsync("data: Initial message");
                await streamWriter.WriteLineAsync();

                    // Send SSE messages at regular intervals
                    while (count < 20)
                {
                    await Task.Delay(3000);
                    var data = await GetStockSample();
                    count++;

                    var escapedData = data.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\"", "\\\"");
                    var sseMessage = $"data: {escapedData}\n\n";

                    await streamWriter.WriteLineAsync(sseMessage);
                    await streamWriter.WriteLineAsync();
                    await streamWriter.FlushAsync();
                }
            }, "text/event-stream");

            return response;
        }
        catch (Exception)
        {
            throw;
        }
    }


    static async Task<string> GetStockSample()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol=IBM&interval=5min&apikey=demo")
        };
        var response = await httpClient.GetAsync(string.Empty);
        var data = await response.Content.ReadAsStringAsync();
        return data;
    }
}

