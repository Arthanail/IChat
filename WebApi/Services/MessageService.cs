using System.Net.WebSockets;
using System.Text;
using Npgsql;

namespace WebApi.Services;

public class MessageService(ChatService chatService)
{
    public async Task<List<Message>> GetMessages(GetMessagesRequest request, NpgsqlConnection connection)
    {
        var result = new List<Message>();
        await using (var cmd = new NpgsqlCommand(
                         "SELECT * FROM messages WHERE dateOfCreation >= @Startdate AND dateOfCreation <= @EndDate;",
                         connection))
        {
            if (request.StartDate != null)
                cmd.Parameters.AddWithValue("StartDate", DateTime.Parse(request.StartDate).ToUniversalTime());
            if (request.EndDate != null)
                cmd.Parameters.AddWithValue("EndDate", DateTime.Parse(request.EndDate).ToUniversalTime());

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Add(new Message(reader.GetInt32(0), reader.GetString(1), reader.GetDateTime(2)));
                }
            }
        }

        return result;
    }

    public async Task<int> SaveMessage(SaveMessageRequest request, NpgsqlConnection connection)
    {
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO messages (text, dateOfCreation, serialNumber) VALUES (@text, @dateOfCreation, @serialNumber)",
            connection);
        cmd.Parameters.AddWithValue("serialNumber", request.SerialNumber);
        cmd.Parameters.AddWithValue("text", request.Text);
        cmd.Parameters.AddWithValue("dateOfCreation", DateTime.UtcNow);
        var result = cmd.ExecuteNonQuery();
        if (result <= 0) return result;
        var message = $"Serial Number: {request.SerialNumber}, Text: {request.Text}, Date: {DateTime.UtcNow}";
        var messageBytes = Encoding.UTF8.GetBytes(message);
        foreach (var s in chatService.Clients)
        {
            await s.SendAsync(messageBytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        return result;
    }
}

public record GetMessagesRequest(string? StartDate, string? EndDate);

public record Message(int SerialNumber, string Text, DateTime DateOfCreation);

public record SaveMessageRequest(string Text, int SerialNumber);