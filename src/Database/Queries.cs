using AskMeNowBot.Exceptions;

namespace AskMeNowBot.Database;

public class Queries : IQueries
{
    private readonly Dictionary<QueryName, string> _queries = new();

    public Queries(DatabaseType type)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "resources", "sql", type.ToString().ToLower());

        if (!Directory.Exists(basePath))
        {
            throw new SqlDirectoryNotFoundException(basePath);
        }

        foreach (var path in Directory.GetFiles(basePath, "*.sql"))
        {
            var name = Path.GetFileNameWithoutExtension(path).ToLower();

            if (!Enum.TryParse<QueryName>(name, true, out var queryName))
            {
                throw new InvalidQueryNameException(name);
            }

            _queries[queryName] = File.ReadAllText(path);
        }
    }

    public string this[QueryName name]
    {
        get
        {
            if (_queries.TryGetValue(name, out var query))
            {
                return query;
            }

            throw new PropertyNotFoundException(name.ToString());
        }
    }
}
