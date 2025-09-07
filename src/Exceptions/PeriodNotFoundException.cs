namespace AskMeNowBot.Exceptions;

public class PeriodNotFoundException(string period) : Exception($"Period {period} not found");
