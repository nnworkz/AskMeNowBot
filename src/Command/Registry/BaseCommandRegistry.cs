using AskMeNowBot.Exceptions;

namespace AskMeNowBot.Command.Registry;

public class BaseCommandRegistry : ICommandRegistry
{
    private readonly Dictionary<string, ICommand> _commands = new();
    private readonly Dictionary<string, string> _aliases = new();

    public void RegisterCommand(ICommand command)
    {
        var name = command.Name.ToLower();

        if (!_commands.TryAdd(name, command))
        {
            throw new CommandExistException(name);
        }

        foreach (var alias in command.Aliases.Select(a => a.ToLower()))
        {
            if (_commands.ContainsKey(alias) || !_aliases.TryAdd(alias, name))
            {
                throw new CommandExistException(alias);
            }
        }
    }

    public ICommand GetCommand(string name)
    {
        name = name.ToLower();
        
        if (_commands.TryGetValue(name, out var command))
        {
            return command;
        }
        
        if (_aliases.TryGetValue(name, out var originalName) && _commands.TryGetValue(originalName, out command))
        {
            return command;
        }
        
        throw new CommandNotFoundException(name);
    }
}
