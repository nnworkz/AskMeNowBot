namespace AskMeNowBot.Database;

public enum QueryName
{
    InitUsers,
    AddUser,
    RemoveUser,
    IsRegistered,
    GetUser,
    
    InitSubscriptions,
    AddSubscriber,
    RemoveSubscriber,
    IsSubscriber,
    GetSubscriber,

    InitWaits,
    AddWait,
    RemoveWait,
    InWait,
    GetWait,

    InitBans,
    AddBan,
    RemoveBan,
    IsBanned,
    GetBan,
    
    InitLinks,
    AddLink,
    RemoveLink,
    LinkExist,
    GetLink,
    GetLinks,
    
    InitTransactions,
    AddTransaction,
    GetTransactions,
    
    InitFilters,
    AddFilter,
    RemoveFilter,
    GetFilter
}
