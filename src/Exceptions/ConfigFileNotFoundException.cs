namespace AskMeNowBot.Exceptions;

public class ConfigFileNotFoundException(string path) : Exception($"Config file not found: {path}");
