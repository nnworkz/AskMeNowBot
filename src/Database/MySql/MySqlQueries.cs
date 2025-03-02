namespace AskMeNowBot.Database.MySql;

public class MySqlQueries : IQueries
{
	public string SelectDatabase => "SELECT DATABASE()";

	public string InitUsers =>
		"""
		    CREATE TABLE IF NOT EXISTS users
		    (
		        id BIGINT PRIMARY KEY,
		        language VARCHAR(2) NOT NULL
		    )
		""";
	
	public string InitExpectations =>
		"""
		    CREATE TABLE IF NOT EXISTS expectations
			(
		    	sender BIGINT PRIMARY KEY,
		    	recipient BIGINT NOT NULL,
		    	is_response BOOLEAN NOT NULL
			)
		""";
	
	public string InitBanList =>
		"""
		    CREATE TABLE IF NOT EXISTS banlist
			(
		    	sender BIGINT NOT NULL,
		    	recipient BIGINT NOT NULL,
		    	PRIMARY KEY (sender, recipient)
			)
		""";

	public string AddUser =>
		"""
			INSERT INTO users (id, language)
			VALUES (@id, @language)
			ON DUPLICATE KEY UPDATE language = VALUES(language)
		""";
	
	public string AddExpectation =>
		"""
			INSERT INTO expectations (sender, recipient, is_response)
			VALUES (@sender, @recipient, @is_response)
		""";
	
	public string AddBan =>
		"""
		    INSERT INTO banlist (sender, recipient)
			VALUES (@sender, @recipient)
			ON DUPLICATE KEY UPDATE sender = sender
		""";
	
	public string RemoveExpectation => "DELETE FROM expectations WHERE sender = @sender";
	
	public string RemoveBan => "DELETE FROM banlist WHERE sender = @sender AND recipient = @recipient";
	
	public string InExpectation => "SELECT EXISTS (SELECT 1 FROM expectations WHERE sender = @sender)";

	public string IsRegistered => "SELECT EXISTS (SELECT 1 FROM users WHERE id = @id)";
	
	public string IsResponse => "SELECT is_response FROM expectations WHERE sender = @sender";
	
	public string IsBanned => "SELECT EXISTS (SELECT 1 FROM banlist WHERE sender = @sender AND recipient = @recipient)";

	public string GetLanguage => "SELECT language FROM users WHERE id = @id";

	public string GetRecipient => "SELECT recipient FROM expectations WHERE sender = @sender";
}
