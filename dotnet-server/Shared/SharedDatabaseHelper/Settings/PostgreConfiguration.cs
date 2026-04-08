namespace SharedDatabaseHelper.Settings;

public class PostgreConfiguration(string template) : DatabaseConfiguration(template)
{
    private readonly string _host = Environment.GetEnvironmentVariable("DB_HOST") ?? Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
    private readonly string _port = Environment.GetEnvironmentVariable("DB_PORT") ?? Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
    private readonly string _name = Environment.GetEnvironmentVariable("DB_NAME") ?? "undefined";
    private readonly string _user = Environment.GetEnvironmentVariable("DB_USER") ?? "undefined";
    private readonly string _password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "undefined";

    public override string GetConnectionString(params string[] args)
    {
        var param = new List<string> { _host, _port, _name, _user, _password };
        param.AddRange(args);
        return base.GetConnectionString(param.ToArray());
    }
}
