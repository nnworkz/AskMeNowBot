namespace AskMeNowBot.Exceptions;

public class ConfigNotInitializedException(string path) : Exception($"Config {path} is not initialized");
