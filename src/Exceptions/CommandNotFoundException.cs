namespace AskMeNowBot.Exceptions;

public class CommandNotFoundException(string name) : Exception($"Command not found: {name}");
