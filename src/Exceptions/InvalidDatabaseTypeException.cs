namespace AskMeNowBot.Exceptions;

public class InvalidDatabaseTypeException(string type) : Exception($"Invalid database type: {type}");
