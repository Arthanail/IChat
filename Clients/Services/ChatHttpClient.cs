using System.Net;

namespace Clients.Services;

public class ChatHttpClient(HttpClient httpClient)
{
    public async Task<List<MessageResult>?> GetMessages()
    {
        var result = await httpClient.PostAsJsonAsync(
            "http://webapi:8080/messages",
            new GetMessagesRequest(DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow));
        var resultContent = await result.Content.ReadAsStringAsync();
        return result.StatusCode != HttpStatusCode.OK ? null : MessageResult.FromJson(resultContent);
    }

    public async Task<string> SendMessage(string text, int serialNumber)
    {
        var result = await httpClient.PostAsJsonAsync("http://webapi:8080/message", new SendMessageRequest(text, serialNumber));
        return await result.Content.ReadAsStringAsync();
    }
}

public record GetMessagesRequest(DateTime StartDate, DateTime EndDate);

public record SendMessageRequest(string Text, int SerialNumber);