namespace AskMeNowBot.Exceptions;

public class InvalidLogLevelTypeException(string type) : Exception($"Invalid log level type: {type}");
