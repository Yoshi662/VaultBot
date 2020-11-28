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
			String texto = $"Notificaciones {(status ? "Activadas" : "Desactivadas")}";
			DiscordColor color = new DiscordColor(status ? "#00ff00" : "#ff0000");
			ctx.RespondAsync(null, false, QuickEmbed(texto, color));
		}

		[Command("start"), Description("Activa las notificaciones")]
		public async Task Start(CommandContext ctx)
		{
			Program.AnimeUpdater.MasterWatcher.EnableRaisingEvents = true;
			await ctx.Client.UpdateStatusAsync(null, UserStatus.Online, null);
			String texto = $"Notificaciones activadas";
			DiscordColor color = new DiscordColor("#00FF00");
			await ctx.RespondAsync(null, false, QuickEmbed(texto, color));
		}

		[Command("stop"), Description("Desactiva las notificaciones")]
		public async Task Stop(CommandContext ctx)
		{
			Program.AnimeUpdater.MasterWatcher.EnableRaisingEvents = false;
			await ctx.Client.UpdateStatusAsync(null, UserStatus.DoNotDisturb, null);
			String texto = $"Notificaciones desactivadas";
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
		public async Task ReEncode(CommandContext ctx, [Description("Ruta completa del archivo a recodificar"), RemainingText] String Ruta_Completa)
		{
			if (!File.Exists(Ruta_Completa))
			{
				ctx.RespondAsync(null, false, HelperMethods.QuickEmbed("No se ha encontrado el archivo", "", false, "#ff0000"));
			} else
			{
				try
				{
					ER_Anime e = new ER_Anime(Ruta_Completa);
					Encoder.Instance.AddAnimeToQueue(new Encode(e, DateTime.Now), true);
					ctx.RespondAsync($"Se ha añadido {e.Title} - {e.N_Ep} a la cola\nSe recodificara lo antes posible");
				}
				catch (ArgumentException)
				{
					DiscordMessage confirm = await ctx.RespondAsync("Empezando recodificado:\nEste recode se ejecutara paralelamente a la cola de animes");
					String outputpath = Path.GetDirectoryName(Ruta_Completa) + "Recode_" + Path.GetFileName(Ruta_Completa);
					Process HandBrakeCLI = new Process();
					HandBrakeCLI.StartInfo = new ProcessStartInfo
					{
						FileName = "HandbrakeCLI.exe",
						RedirectStandardOutput = true,
						UseShellExecute = false,
						CreateNoWindow = true,
						Arguments = $"--preset-import-file \"AnimePreset.json\" -Z \"General Purpose Anime H.265 10-bit\" --input \"{Ruta_Completa}\" --output \"{outputpath}\""
					};
					HandBrakeCLI.Start();
					HandBrakeCLI.BeginOutputReadLine();

					DateTime lastedit = DateTime.Now;
					HandBrakeCLI.OutputDataReceived += async (object sender, DataReceivedEventArgs e) =>
					{
						if (!String.IsNullOrEmpty(e.Data))
						{
							if (DateTime.Now - lastedit > TimeSpan.FromMinutes(60))
							{
								confirm = await confirm.ModifyAsync($"Reccodificando archivo\n`{e.Data}`");
								lastedit = DateTime.Now;
							}
						}
					};
					HandBrakeCLI.WaitForExit();
					confirm.DeleteAsync();
					ctx.RespondAsync(ctx.User.Mention + " - Ha finalizado el recodificado del archivo");
				}
			}
		}

		/*[Command("addtorrent"), Description("Descarga un archivo en el servidor\n `\\\\Vault662\\Torrents\\TorrentsDescargados`"), Aliases(new[] {"torrent","add"})]
		public async Task AddTorrent(CommandContext ctx, DiscordAttachment file){
			WebClient client = new WebClient();
			client.DownloadFile(file.Url, "V:\\Vault\\Torrents\\" + file.FileName);
		}*/

		[Command("respuestaingeniosa"), Description("Tu madre"), RequirePermissions(Permissions.Administrator), Aliases(new[] { "ri" })]
		public async Task RI(CommandContext ctx)
		{
			ctx.RespondAsync("`Tu madre`\n" + @"https://i.pinimg.com/originals/66/8e/af/668eafb46ffa374281d272e0d98719e4.gif");
		}

		#region HelperCommands

		#endregion
		private DiscordEmbed NewThingEmbed(String titulo)
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

		public DiscordEmbed QuickEmbed(String s, DiscordColor color)
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
	}
}
