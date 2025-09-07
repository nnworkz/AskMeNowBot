namespace AskMeNowBot.Filter;

public record BaseFilter(long UserId, bool Spam, bool Terrorism, bool Drugs, bool Violence, bool Pornography) : IFilter
{
    public long UserId { get; set; } = UserId;
    public bool Spam { get; set; } = Spam;
    public bool Terrorism { get; set; } = Terrorism;
    public bool Drugs { get; set; } = Drugs;
    public bool Violence { get; set; } = Violence;
    public bool Pornography { get; set; } = Pornography;
}
