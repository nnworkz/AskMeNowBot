namespace AskMeNowBot.Filter;

public interface IFilter
{
    public long UserId { get; set; }
    public bool Spam { get; set; }
    public bool Terrorism { get; set; }
    public bool Drugs { get; set; }
    public bool Violence { get; set; }
    public bool Pornography { get; set; }
}
