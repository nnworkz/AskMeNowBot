namespace AskMeNowBot.Exceptions;

public class NotSupportedFilterNameException(string name) : Exception($"Filter {name} not supported");
