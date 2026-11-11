
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    public string FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogLine($"Received: {input}");

        dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());

        string issueUrl = (string)json.issue.html_url;

        string payload = JsonConvert.SerializeObject(new
        {
            text = $"Issue Created: {issueUrl}"
        });

        var client = new HttpClient();

        var webRequest = new HttpRequestMessage(
            HttpMethod.Post,
            Environment.GetEnvironmentVariable("SLACK_URL"))
        {
            Content = new StringContent(
                payload,
                Encoding.UTF8,
                "application/json")
        };

        var response = client.Send(webRequest);

        using var reader = new StreamReader(
            response.Content.ReadAsStream());

        return reader.ReadToEnd();
    }
}
