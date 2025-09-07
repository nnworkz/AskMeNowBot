namespace AskMeNowBot.Exceptions;

public class InvalidLanguageNameException(string name) : Exception($"Invalid language name: {name}");
