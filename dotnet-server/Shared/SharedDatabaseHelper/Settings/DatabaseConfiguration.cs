namespace SharedDatabaseHelper.Settings;

public abstract class DatabaseConfiguration(string template)
{
    private readonly string template = template;

    public virtual string GetConnectionString(params string[] args)
    {
        return string.Format(template, args);
    }
}
