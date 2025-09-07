namespace AskMeNowBot.Handler.Sub;

public abstract class Action
{
    public const string Ban = "ban";
    public const string Cancel = "cancel";
    public const string Language = "language";
    public const string Respond = "respond";
    public const string Subscription = "subscription";
    public const string Unban = "unban";
    public const string Payment = "payment";
    public const string AddComment = "add_comment";
    public const string RemoveComment = "remove_comment";
    public const string CurrencyType = "currency_type";
    public const string CreateLink = "create_link";
    public const string Link = "link";
    public const string EditLink = "edit_link";
    public const string DeactivateLink = "deactivate_link";
    public const string RemoveLink = "remove_link";
    public const string EditLinkMaxUsages = "edit_link_max_usages";
    public const string EditLinkExpiresAt = "edit_link_expires_at";
    public const string ActivateLink = "activate_link";
    public const string Filter = "filter";
}
