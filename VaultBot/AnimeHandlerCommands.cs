using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VaultBot
{
	public class AnimeHandlerCommands : BaseCommandModule
	{
		//This might get useful one day
		/*[Command("test"), RequireOwner(), Hidden()] //Aliases(new[] { "t" }),
		public async Task test(CommandContext ctx)
		{ }*/
		[Command("updatequeue"), RequireOwner(), Hidden(), Aliases(new[] { "uq" })] //,
		public async Task test(CommandContext ctx)
		{
			Encoder.Instance.SendUpdateToChannel();
			ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
		}

		[Command("deletequeue"), RequireOwner(), Hidden(), Aliases(new[] { "dq" })] //,
		public async Task deletequeue(CommandContext ctx)
		{
			Encoder.Instance.EncodeQueue.Clear();
			Encoder.Instance.SaveCurentQueueToFile();
			ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
		}

		[Command("cleanER"), RequireOwner(), Aliases(new[] { "c" }), Hidden()]
		public async Task CleanDuplicates(CommandContext ctx, [RemainingText(), Description("Ruta completa a la carpeta")] string rutacompleta)
		{
			string[] files = Directory.GetFiles(rutacompleta);
			foreach (string f in files)
			{
				ER_Anime e = new ER_Anime(f);
				HelperMethods.RemoveDuplicates(e);
			}
		}

		[Command("ping"), Description("Hace un ping al bot")]
		public async Task Pinger(CommandContext ctx)
		{
			await ctx.TriggerTypingAsync();
			var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");
			await ctx.RespondAsync($"{emoji} Pong! Ping: {ctx.Client.Ping}ms");
		}

		[Command("new"), Description("Publica un nuevo episodio en #updates")]
		public async Task Publish(CommandContext ctx, [RemainingText] string s)
		{
			await ctx.TriggerTypingAsync();
			Program.AnimeUpdater.Channel.SendMessageAsync(null, false, NewThingEmbed(s));
		}

		[Command("status"), Description("Comrpueba si las notificaciones estan activadas")]
		public async Task Status(CommandContext ctx)
		{
			bool status = Program.AnimeUpdater.MasterWatcher.EnableRaisingEvents;
			string texto = $"Notificaciones {(status ? "Activadas" : "Desactivadas")}";
			DiscordColor color = new DiscordColor(status ? "#00ff00" : "#ff0000");
			ctx.RespondAsync(null, false, QuickEmbed(texto, color));
		}

		[Command("start"), Description("Activa las notificaciones")]
		public async Task Start(CommandContext ctx)
		{
			Program.AnimeUpdater.MasterWatcher.EnableRaisingEvents = true;
			await ctx.Client.UpdateStatusAsync(null, UserStatus.Online, null);
			string texto = $"Notificaciones activadas";
			DiscordColor color = new DiscordColor("#00FF00");
			await ctx.RespondAsync(null, false, QuickEmbed(texto, color));
		}

		[Command("stop"), Description("Desactiva las notificaciones")]
		public async Task Stop(CommandContext ctx)
		{
			Program.AnimeUpdater.MasterWatcher.EnableRaisingEvents = false;
			await ctx.Client.UpdateStatusAsync(null, UserStatus.DoNotDisturb, null);
			string texto = $"Notificaciones desactivadas";
			DiscordColor color = new DiscordColor("#FF0000");
			await ctx.RespondAsync(null, false, QuickEmbed(texto, color));
		}

		[Command("version"), Description("Muestra la version del bot")]
		public async Task Version(CommandContext ctx)
		{
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
			embedBuilder.WithThumbnail("https://i.imgur.com/QeBaVkD.png");
			embedBuilder.WithFooter("Usando DSharpPlus", "https://dsharpplus.github.io/logo.png");
			embedBuilder.WithTitle($"VaultBot - v.{Program.version}");
			embedBuilder.WithColor(new DiscordColor(0x2461DC));
			embedBuilder.AddField("Version ", $"{Program.internalname}");
			embedBuilder.AddField("Codigo fuente", "Mira el codigo fuente en: https://github.com/Yoshi662/VaultBot");
			embedBuilder.AddField("DSharpPlus", $"Version: {ctx.Client.VersionString}");

			await ctx.RespondAsync(null, false, embedBuilder.Build());
		}

		[Command("reencode"), Description("Añade un archivo manualmente a la cola\n**Sobreescribe el archivo original**"), Aliases(new[] { "e", "encode", "addqueue" })]
		public async Task ReEncode(CommandContext ctx, [Description("Ruta completa del archivo/carpeta a recodificar"), RemainingText] string Full_Path)
		{
			//Since there is a fair chance that the user can miss that the local system in Vault may be different from the system they are using. We will try our best to fix that and find the proper file
			string Rectified_Path = Path.GetPathRoot(Full_Path) + @"Vault" + Full_Path.Substring(Path.GetPathRoot(Full_Path).Length);

			bool isFile = File.Exists(Full_Path) || File.Exists(Rectified_Path);
			bool isDirectory = Directory.Exists(Full_Path) || Directory.Exists(Rectified_Path);
			bool isRectified = File.Exists(Rectified_Path) || Directory.Exists(Rectified_Path);

			if (isFile)
			{
				try
				{
					ER_Anime e = new ER_Anime(isRectified ? Rectified_Path : Full_Path);
					Encoder.Instance.AddAnimeToQueue(new Encode(e, DateTime.Now), true, false);
					ctx.RespondAsync($"Se ha añadido {e.Title} - {e.N_Ep} a la cola\nSe recodificara lo antes posible");
				}
				catch (ArgumentException)
				{
					Anime a = new Anime(Full_Path);
					Encoder.Instance.AddAnimeToQueue(new Encode(a, DateTime.Now), true, false);
					ctx.RespondAsync($"Se ha añadido {a.FullFileName} a la cola\nSe recodificara lo antes posible");
				}

			} else if (isDirectory)
			{
				string[] files = Directory.GetFiles(isRectified ? Rectified_Path : Full_Path);
				foreach (string f in files)
				{
					if (File.Exists(f))
					{
						try
						{
							ER_Anime e = new ER_Anime(f);
							Encoder.Instance.AddAnimeToQueue(new Encode(e, DateTime.Now), true, false);
						}
						catch (ArgumentException)
						{
							Anime a = new Anime(f);
							Encoder.Instance.AddAnimeToQueue(new Encode(a, DateTime.Now), true, false);
						}
					}
				}
				await ctx.RespondAsync(null, false, HelperMethods.QuickEmbed($"{files.Length} Videos Añadidos a la cola", "", false, false));
			} else
			{
				await ctx.RespondAsync(null, false, HelperMethods.QuickEmbed("No se ha podido encontrar el Archivo/Carpeta", "", false, false, DiscordColor.Red.ToString()));
			}
			Encoder.Instance.SendUpdateToChannel();
		}

		[Command("addtorrent"), Description("Descarga un archivo en el servidor\nRuta de descarga:`\\\\Vault_662\\Torrents\\TorrentsDescargados`"), Aliases(new[] { "torrent", "add" })]
		public async Task AddTorrent(CommandContext ctx)
		{
			try
			{
				DiscordAttachment file = ctx.Message.Attachments.First();
				WebClient client = new WebClient();
				client.DownloadFile(file.Url, Properties.Settings.Default.TorrentFolder + file.FileName);
				ctx.RespondAsync(null, false, HelperMethods.QuickEmbed("Se ha descargado el torrent"));
			}
			catch (Exception e)
			{
				ctx.RespondAsync(null, false, HelperMethods.QuickEmbed("No se ha podido descargar el torrent", "Error: " + e.Message, false, false, DiscordColor.Red.ToString()));
			}
		}

		[Command("respuestaingeniosa"), Description("Tu madre"), RequirePermissions(Permissions.Administrator), Aliases(new[] { "ri" })]
		public async Task RI(CommandContext ctx)
		{
			ctx.RespondAsync("`Tu madre`\n" + @"https://i.pinimg.com/originals/66/8e/af/668eafb46ffa374281d272e0d98719e4.gif");
		}

		#region HelperCommands
		private DiscordEmbed NewThingEmbed(string titulo)
		{
			DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
						.WithTitle(titulo)
						.WithDescription("Ahora disponible en el servidor")
						.WithColor(new DiscordColor(0x2461DC))
						.WithFooter(
							"A Yoshi's Bot",
							"https://i.imgur.com/rT9YocG.jpg"
						).WithThumbnail("https://i.imgur.com/QeBaVkD.png");
			return builder.Build();
		}

		public DiscordEmbed QuickEmbed(string s, DiscordColor color)
		{
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
		#endregion
	}
}
