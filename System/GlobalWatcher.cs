using Discord;
using Discord.WebSocket;

public class Watcher {
    public struct Command {
        public enum CommandType {
            message,
            slash
        }
        public readonly ulong id;
        public readonly SlashCommandProperties? slashData;
        public readonly MessageCommandProperties? messageData;
        public readonly CommandType type;
        public Command(ulong id, SlashCommandProperties commandData) {
            this.id = id;
            this.slashData = commandData;
            this.messageData = null;
            type = CommandType.slash;
        }
        public Command(ulong id, MessageCommandProperties commandData) {
            this.id = id;
            this.slashData = null;
            this.messageData = commandData;
            type = CommandType.message;
        }
    }
    List<Command> registeredCommands = new();
    public void RegisterCommand(ulong commandID, SlashCommandProperties? slashData) {
        registeredCommands.Add(new(commandID, slashData));
    }
    public void RegisterCommand(ulong commandID, MessageCommandProperties? messageData) {
        registeredCommands.Add(new(commandID, messageData));
    }
    public int FindID(ulong commandID) {
        for (int i = 0; i < registeredCommands.Count(); i++) {
            if (registeredCommands[i].id == commandID) {
                return i;
            }
        }
        return -1;
    }
    public async void CheckCommands(DiscordSocketClient client, SocketGuild guild) {
        var commands = await guild.GetApplicationCommandsAsync();
        List<SocketApplicationCommand> commandList = commands.ToList();
        if (commandList.Count == registeredCommands.Count) {
            return;
        } else {
            List<Command> registerAgain = new();
            List<int> registerAgainIndexes = new();
            bool foundID = false;
            for (int i = 0; i < registeredCommands.Count(); i++) { // finds commands Discord removed or were otherwise removed some other way
                for (int j = 0; j < commandList.Count; j++) {
                    if (commandList[i].Id == registeredCommands[j].id) {
                        foundID = true;
                        break;
                    }
                }
                if (!foundID) {
                    registerAgain.Add(registeredCommands[i]);
                    registerAgainIndexes.Add(i);
                }
                foundID = false;
            }
            SocketApplicationCommand command;
            for (int i = 0; i < registerAgain.Count; i++) { // adds re-registered commands back
                switch(registerAgain[i].type) {
                    case Command.CommandType.slash:
                    command = await guild.CreateApplicationCommandAsync(registerAgain[i].slashData);
                    registeredCommands.Add(new(command.Id, registerAgain[i].slashData));

                    break;
                    case Command.CommandType.message:
                    command = await guild.CreateApplicationCommandAsync(registerAgain[i].messageData);
                    registeredCommands.Add(new(command.Id, registerAgain[i].messageData));
                    break;
                }
            }
            for (int i = registerAgainIndexes.Count - 1; 0 <= i; i--) {
                registeredCommands.RemoveAt(registerAgainIndexes[i]); // cleans up polution from re-registration
            }
        }
    }

}