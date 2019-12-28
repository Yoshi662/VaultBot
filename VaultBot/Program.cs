using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;


namespace VaultBot
{
	public class Program
	{
		internal readonly String version = "1.0.1";
		internal readonly String internalname = "Beautify";

		public AnimeHandler AnimeUpdater { get; set; } = new AnimeHandler();
		public DiscordClient Client { get; set; }
		public static CommandsNextExtension Commands { get; set; }

        private static Program prog;
		private DiscordChannel senderChannel;

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
				LogLevel = LogLevel.Debug,
				UseInternalLogHandler = true
			};

			this.Client = new DiscordClient(cfg);

			this.Client.Ready += this.Client_Ready;
			this.Client.GuildAvailable += this.Client_GuildAvailable;
			this.Client.ClientErrored += this.Client_ClientError;

			await this.Client.ConnectAsync();

			senderChannel = await Client.GetChannelAsync(ulong.Parse(cfgjson.senderChannel));
			AnimeUpdater.Channel = senderChannel;

			await Task.Delay(-1);
		}


		private Task Client_Ready(ReadyEventArgs e)
		{

			e.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", "Client is ready to process events.", DateTime.Now);

			return Task.CompletedTask;
		}

		private Task Client_GuildAvailable(GuildCreateEventArgs e)
		{
			e.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", $"Guild available: {e.Guild.Name}", DateTime.Now);

			return Task.CompletedTask;
		}

		private Task Client_ClientError(ClientErrorEventArgs e)
		{
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "ExampleBot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            prog.RunBotAsync().GetAwaiter().GetResult();
            return Task.CompletedTask;
		}
	}

	// this structure will hold data from config.json
	public struct ConfigJson
	{
		[JsonProperty("token")]
		public string Token { get; private set; }

		[JsonProperty("prefix")]
		public string CommandPrefix { get; private set; }

		[JsonProperty("senderChannel")]
		public string senderChannel { get; private set; }
	}
}
