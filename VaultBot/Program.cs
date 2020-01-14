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
		internal readonly String version = "1.0.4";
		internal readonly String internalname = "Regex are nice"; //ofc not

		public AnimeHandler AnimeUpdater { get; set; }
		public DiscordClient Client { get; set; }

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
			this.Client.MessageCreated += Client_MessageCreated;

			await this.Client.ConnectAsync();

			AnimeUpdater = new AnimeHandler(cfgjson.AnimePath);
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

		private Task Client_MessageCreated(MessageCreateEventArgs e)
		{
			string mensaje = e.Message.Content.ToLower();

			if (!mensaje.StartsWith("-")) return Task.CompletedTask;

			if (mensaje.StartsWith("-ping"))
			{
				e.Message.RespondAsync("Pong! " + Client.Ping + "ms");
			}

			if (mensaje.StartsWith("-status"))
			{
				bool status = AnimeUpdater.MasterWatcher.EnableRaisingEvents;
				String texto = $"Notificaciones {(status ? "Activadas" : "Desactivadas")}";
				DiscordColor color = new DiscordColor(status ? "#00ff00" : "#ff0000");
				e.Channel.SendMessageAsync(null,false,QuickEmbed(texto,color));
			}
			//#00ff00 verde - #ff0000 rojo
			if (mensaje.StartsWith("-start"))
			{
				AnimeUpdater.MasterWatcher.EnableRaisingEvents = true;
				String texto = $"Notificaciones activadas por {e.Author.Username}";
				DiscordColor color = new DiscordColor("#00ff00");
				senderChannel.SendMessageAsync(null,false,QuickEmbed(texto,color));
			}

			if (mensaje.StartsWith("-stop"))
			{
				AnimeUpdater.MasterWatcher.EnableRaisingEvents = false;
				String texto = $"Notificaciones desactivadas por {e.Author.Username}";
				DiscordColor color = new DiscordColor("#ff0000");
				senderChannel.SendMessageAsync(null, false, QuickEmbed(texto, color));
			}

			if (mensaje.StartsWith("-version"))
			{
				senderChannel.SendMessageAsync(null, false, GetVersionEmbed());
			}

			return Task.CompletedTask;
		}

		public DiscordEmbed QuickEmbed(String s, DiscordColor color) {
			DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
					   .WithTitle(s)
					   .WithColor(color) //0x2461DC
					   .WithFooter(
						   "A Yoshi's Bot",
						   "https://i.imgur.com/rT9YocG.jpg"
					   );
			DiscordEmbed embed = builder.Build();
			return embed;
		}

		public DiscordEmbed GetVersionEmbed()
		{
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
			embedBuilder.WithThumbnailUrl("https://i.imgur.com/QeBaVkD.png");
			embedBuilder.WithFooter("Usando DSharpPlus", "https://dsharpplus.github.io/logo.png");
			embedBuilder.WithTitle($"VaultBot - v.{version}");
			embedBuilder.WithColor(new DiscordColor(0x2461DC));
			embedBuilder.AddField("Version ", $"{internalname}");
			embedBuilder.AddField("Codigo fuente", "Mira el codigo fuente en: https://github.com/Yoshi662/VaultBot");
			embedBuilder.AddField("DSharpPlus", $"Version: {Client.VersionString}");
			return embedBuilder.Build();
		}
	}

	public struct ConfigJson
	{
		[JsonProperty("token")]
		public string Token { get; private set; }

		[JsonProperty("prefix")]
		public string CommandPrefix { get; private set; }

		[JsonProperty("senderChannel")]
		public string senderChannel { get; private set; }

		[JsonProperty("animePath")]
		public string AnimePath { get; private set; }
	}
}
