namespace AskMeNowBot.Exceptions;

public class UnsupportedLanguageException(string name) : Exception($"Unsupported language {name}");
