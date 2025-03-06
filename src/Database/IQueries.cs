namespace AskMeNowBot.Database;

public interface IQueries
{
	string SelectDatabase { get; }

	string InitUsers { get; }
	string AddUser { get; }
	string IsRegistered { get; }
	string GetLanguage { get; }

	string InitExpectations { get; }
	string AddExpectation { get; }
	string RemoveExpectation { get; }
	string InExpectation { get; }
	string GetRecipient { get; }
	string IsResponse { get; }

	string InitBanList { get; }
	string AddBan { get; }
	string RemoveBan { get; }
	string IsBanned { get; }
}
