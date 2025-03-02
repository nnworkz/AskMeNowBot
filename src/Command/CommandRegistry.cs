using AskMeNowBot.Exceptions;

namespace AskMeNowBot.Command;

public class CommandRegistry
{
	private readonly Dictionary<string, ICommand> _commands = new ();
	private readonly Dictionary<string, string> _aliases = new ();

	public void RegisterCommand(ICommand command)
	{
		var name = command.Name.ToLowerInvariant();

		if (!_commands.TryAdd(name, command))
		{
			throw new CommandExistException(name);
		}

		foreach (var alias in command.Aliases)
		{
			var aliasName = alias.ToLowerInvariant();
			
			if (_commands.ContainsKey(aliasName) || !_aliases.TryAdd(aliasName, name))
			{
				throw new CommandExistException(aliasName);
			}
		}
	}

	public ICommand? GetCommand(string name)
	{
		name = name.ToLowerInvariant();

		if (
			_commands.TryGetValue(name, out var command) ||
			_aliases.TryGetValue(name, out var originalName) && _commands.TryGetValue(originalName, out command)
		)
		{
			return command;
		}
		
		return null;
	}
}
