using System.Runtime.CompilerServices;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using StarBot;

public class Watcher
{
    public List<string> commandNamesMissing = new();
    public struct Command
    {
        public enum CommandType
        {
            message,
            slash
        }
        public readonly ulong id;
        public readonly ulong guildID;
        public readonly SlashCommandProperties? slashData;
        public readonly MessageCommandProperties? messageData;
        public readonly CommandType type;
        public Command(ulong id, ulong guildID, SlashCommandProperties commandData)
        {
            this.id = id;
            this.guildID = guildID;
            this.slashData = commandData;
            this.messageData = null;
            type = CommandType.slash;
        }
        public Command(ulong id, ulong guildID, MessageCommandProperties commandData)
        {
            this.id = id;
            this.guildID = guildID;
            this.slashData = null;
            this.messageData = commandData;
            type = CommandType.message;
        }
    }
    public async void initialize(DiscordSocketClient client, Database data)
    {
        data.createDbIfNotExists("watcher-id");
        data.createDbIfNotExists("watcher-name");
        string[]? currentGuilds = data.getKeys("watcher-id");
        HashSet<string> guilds = currentGuilds.ToHashSet();
        List<SocketApplicationCommand> commandsOnDiscord = new();
        commandNamesMissing.AddRange(data.getKeys("watcher-name"));
        foreach (string guildID in guilds)
        {
            commandsOnDiscord.AddRange(await client.GetGuild(ulong.Parse(guildID)).GetApplicationCommandsAsync());
        }

        for (int i = 0; i < commandsOnDiscord.Count; i++)
        {
            commandNamesMissing.Remove(commandsOnDiscord[i].Name.ToString());
        }
    }
    List<Command> registeredCommands = new();
    public void RegisterCommand(ulong commandID, ulong guildID, Database data, SlashCommandProperties? slashData)
    {
        registeredCommands.Add(new(commandID, guildID, slashData));
        string? currentData = data.fetchValue(guildID.ToString(), "watcher-id");
        data.setValue(guildID.ToString(), currentData == "" ? currentData : currentData + "-" + commandID.ToString(), "watcher-id");
        data.setValue(commandID.ToString(), slashData.Name.Value + "-S", "watcher-name");
        //data.setValue(commandID.ToString(), JsonConvert.SerializeObject(slashData), guildID); TODO: Add in global watcher database sync, to remove the need to re-register commands every launch
    }
    public async void RegisterCommand(DiscordSocketClient client, ulong guildID, Database data, SlashCommandProperties? slashData)
    {
        if (commandNamesMissing.Contains(slashData.Name.Value))
        { // TODO: Fix invalid logic here, should check for matching names, instead of an array of IDs
            commandNamesMissing.Remove(slashData.Name.Value);
            SocketApplicationCommand command = await client.GetGuild(guildID).CreateApplicationCommandAsync(slashData); // TODO: add code to fully register command by clearing off stale data and registering command normally
            registeredCommands.Add(new(command.Id, guildID, slashData));
            string? currentData = data.fetchValue(guildID.ToString(), "watcher");
            data.setValue(guildID.ToString(), currentData == "" ? currentData : currentData + "-" + command.Id.ToString(), "watcher-id");
            data.setValue(command.Id.ToString(), slashData.Name.Value + "-M", "watcher-name");
        }
        //registeredCommands.Add(new(commandID, guildID, slashData));
        //string currentData = data.fetchValue(guildID.ToString(), "watcher");
        //data.setValue(guildID.ToString(), currentData == "" ? currentData : currentData + "-" + commandID.ToString(), "watcher-id");
        //data.setValue(commandID.ToString(), slashData.Name.Value + "-S", "watcher-name");
        //data.setValue(commandID.ToString(), JsonConvert.SerializeObject(slashData), guildID); TODO: Add in global watcher database sync, to remove the need to re-register commands every launch
    }
    public void RegisterCommand(ulong commandID, ulong guildID, Database data, MessageCommandProperties? messageData)
    {
        registeredCommands.Add(new(commandID, guildID, messageData));
        string? currentData = data.fetchValue(guildID.ToString(), "watcher-id");
        data.setValue(guildID.ToString(), currentData == "" ? currentData : currentData + "-" + commandID.ToString(), "watcher-id");
        data.setValue(commandID.ToString(), messageData.Name.Value + "-M", "watcher-name");
        //data.setValue(commandID.ToString(), JsonConvert.SerializeObject(messageData), guildID);
    }
    public async void RegisterCommand(DiscordSocketClient client, ulong guildID, Database data, MessageCommandProperties? messageData)
    {
        if (commandNamesMissing.Contains(messageData.Name.Value))
        { // TODO: Fix invalid logic here, should check for matching names, instead of an array of IDs
            commandNamesMissing.Remove(messageData.Name.Value);
            SocketApplicationCommand command = await client.GetGuild(guildID).CreateApplicationCommandAsync(messageData); // TODO: add code to fully register command by clearing off stale data and registering command normally
            registeredCommands.Add(new(command.Id, guildID, messageData));
            string? currentData = data.fetchValue(guildID.ToString(), "watcher");
            data.setValue(guildID.ToString(), currentData == "" ? currentData : currentData + "-" + command.Id.ToString(), "watcher-id");
            data.setValue(command.Id.ToString(), messageData.Name.Value + "-M", "watcher-name");
        }
        //registeredCommands.Add(new(commandID, guildID, messageData));
        //string currentData = data.fetchValue(guildID.ToString(), "watcher");
        //data.setValue(guildID.ToString(), currentData == "" ? currentData : currentData + "-" + commandID.ToString(), "watcher-id");
        //data.setValue(commandID.ToString(), messageData.Name.Value + "-M", "watcher-name");
        //data.setValue(commandID.ToString(), JsonConvert.SerializeObject(messageData), guildID);
    }
    public void FlushCommandsToDisk(Database data)
    {
        data.updateDB("watcher");
    }
    public int FindID(ulong commandID)
    {
        for (int i = 0; i < registeredCommands.Count(); i++)
        {
            if (registeredCommands[i].id == commandID)
            {
                return i;
            }
        }
        return -1;
    }
    public async void CheckCommands(DiscordSocketClient client, SocketGuild guild)
    {
        var commands = await guild.GetApplicationCommandsAsync();
        List<SocketApplicationCommand> commandList = commands.ToList();
        if (commandList.Count == registeredCommands.Count)
        {
            return;
        }
        else
        {
            List<Command> registerAgain = new();
            List<int> registerAgainIndexes = new();
            bool foundID = false;
            for (int i = 0; i < registeredCommands.Count(); i++)
            { // finds commands Discord removed or were otherwise removed some other way
                if (guild.Id == registeredCommands[i].guildID)
                {
                    for (int j = 0; j < commandList.Count; j++)
                    {
                        if (commandList[j].Id == registeredCommands[i].id)
                        {
                            foundID = true;
                            break;
                        }
                    }
                    if (!foundID)
                    {
                        registerAgain.Add(registeredCommands[i]);
                        registerAgainIndexes.Add(i);
                    }
                    foundID = false;
                }
            }
            SocketApplicationCommand command;
            for (int i = 0; i < registerAgain.Count; i++)
            { // adds re-registered commands back
                switch (registerAgain[i].type)
                {
                    case Command.CommandType.slash:
                        command = await guild.CreateApplicationCommandAsync(registerAgain[i].slashData);
                        registeredCommands.Add(new(command.Id, guild.Id, registerAgain[i].slashData));

                        break;
                    case Command.CommandType.message:
                        command = await guild.CreateApplicationCommandAsync(registerAgain[i].messageData);
                        registeredCommands.Add(new(command.Id, guild.Id, registerAgain[i].messageData));
                        break;
                }
            }
            for (int i = registerAgainIndexes.Count - 1; 0 <= i; i--)
            {
                registeredCommands.RemoveAt(registerAgainIndexes[i]); // cleans up polution from re-registration
            }
        }
    }

}