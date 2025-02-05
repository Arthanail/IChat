using System.Text.Json;
using System.Text.Json.Serialization;

namespace Clients.Services;

public class MessageResult
{
    [JsonPropertyName("serialNumber")]
    public int SerialNumber { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("dateOfCreation")]
    public string Date { get; set; }
    
    public static List<MessageResult> FromJson(string json)
    {
        return JsonSerializer.Deserialize<List<MessageResult>>(json)!;
    }
}