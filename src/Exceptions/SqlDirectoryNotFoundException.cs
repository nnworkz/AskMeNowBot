namespace AskMeNowBot.Exceptions;

public class SqlDirectoryNotFoundException(string resource) : Exception($"Sql directory not found: {resource}");
