using Npgsql;

namespace WebApi.Database;

public static class DbInitialize
{
    public static async void Initialize(NpgsqlConnection connection)
    {
        var script = await File.ReadAllTextAsync("init_script.sql");

        await using var cmd = new NpgsqlCommand(script, connection);
        cmd.ExecuteNonQuery();
    }
}