namespace AskMeNowBot.Exceptions;

public class CommandExistException(string name) : Exception($"Command {name} already exists");
