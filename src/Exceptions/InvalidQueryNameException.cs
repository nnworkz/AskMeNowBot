namespace AskMeNowBot.Exceptions;

public class InvalidQueryNameException(string name) : Exception($"Invalid query name: {name}");
