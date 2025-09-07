namespace AskMeNowBot.Exceptions;

public class InvalidDatabaseException(string type) : Exception($"Unsupported database type: {type}");
