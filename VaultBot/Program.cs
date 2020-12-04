using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext.Exceptions;

namespace VaultBot
{
	public class Program
	{
		internal static readonly string version = "2.2.0";
		internal static readonly string internalname = "Reencode all the way";

		public static AnimeHandler AnimeUpdater { get; set; }
		public static DiscordClient Client { get; set; }

		private static Program prog;

		private DiscordChannel senderChannel;
		static CommandsNextExtension commands;

		public static void Main(string[] args)
		{
			prog = new Program();
			prog.RunBotAsync().GetAwaiter().GetResult();
		}

		public async Task RunBotAsync()
		{

			var json = "";
			using (var fs = File.OpenRead("config.json"))
			using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
				json = await sr.ReadToEndAsync();


			var cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);
			var cfg = new DiscordConfiguration
			{
				Token = cfgjson.Token,
				TokenType = TokenType.Bot,

				AutoReconnect = true,
				MinimumLogLevel = LogLevel.Debug
			};

			Client = new DiscordClient(cfg);

			Client.Ready += this.Client_Ready;
			Client.GuildAvailable += this.Client_GuildAvailable;
			Client.ClientErrored += this.Client_ClientError;

			Client.UseInteractivity(new InteractivityConfiguration
			{
				PaginationBehaviour = DSharpPlus.Interactivity.Enums.PaginationBehaviour.Ignore,
				Timeout = TimeSpan.FromMinutes(2)
			});

			commands = Client.UseCommandsNext(new CommandsNextConfiguration
			{
				StringPrefixes = new[] { cfgjson.CommandPrefix }, //How the fuck this works?
			});

			commands.RegisterCommands<AnimeHandlerCommands>();

			commands.CommandExecuted += this.Commands_CommandExecuted;
			commands.CommandErrored += this.Commands_CommandErrored;

			await Client.ConnectAsync();

			AnimeUpdater = new AnimeHandler(cfgjson.AnimePath);
			senderChannel = await Client.GetChannelAsync(ulong.Parse(cfgjson.SenderChannel));
			AnimeUpdater.Channel = senderChannel;

			Encoder.Instance.LoadQueueFromFile();
			Encoder.Instance.UpdatesChannel = await Client.GetChannelAsync(ulong.Parse(cfgjson.QueueChannel));
			Encoder.Instance.SendUpdates = true;
			Encoder.Instance.SendUpdateToChannel();

			await Task.Delay(-1);
		}

		private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
		{
			sender.Logger.Log(LogLevel.Information, "Client is ready to process events.");
			return Task.CompletedTask;
		}

		private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
		{
			sender.Logger.Log(LogLevel.Information, $"Guild available: {e.Guild.Name}");

			return Task.CompletedTask;
		}

		private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
		{
			sender.Logger.Log(LogLevel.Error, $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

			prog.RunBotAsync().GetAwaiter().GetResult();
			return Task.CompletedTask;
		}

		private Task Commands_CommandExecuted(CommandsNextExtension ctx, CommandExecutionEventArgs e)
		{
			e.Context.Client.Logger.LogInformation($"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");
			return Task.CompletedTask;
		}

		private async Task Commands_CommandErrored(CommandsNextExtension ctx, CommandErrorEventArgs e)
		{
			e.Context.Client.Logger.LogError($"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

			if (e.Exception is ChecksFailedException ex)
			{
				var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

				// let's wrap the response into an embed
				var embed = new DiscordEmbedBuilder
				{
					Title = "Acceso Denegado",
					Description = $"{emoji} No tienes los permisos necesarios para ejecutar este comando.",
					Color = new DiscordColor(0xFF0000) // red
				};
				await e.Context.RespondAsync("", embed: embed);
			}
		}
	}

	public struct ConfigJson
	{
		[JsonProperty("token")]
		public string Token { get; private set; }

		[JsonProperty("prefix")]
		public string CommandPrefix { get; private set; }

		[JsonProperty("senderChannel")]
		public string SenderChannel { get; private set; }

		[JsonProperty("queueChannel")]
		public string QueueChannel { get; private set; }

		[JsonProperty("animePath")]
		public string AnimePath { get; private set; }

	}
}