namespace AskMeNowBot.Command.Registry;

public interface ICommandRegistry
{
	void RegisterCommand(ICommand command);
	ICommand GetCommand(string name);
}
