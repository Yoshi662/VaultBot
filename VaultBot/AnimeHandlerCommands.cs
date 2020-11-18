﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

namespace VaultBot
{
	public class AnimeHandlerCommands : BaseCommandModule
	{
		[Command("test"), RequireOwner(), Aliases(new[] {"t"})]
		public async Task test(CommandContext ctx) {
			String[] files = Directory.GetFiles(@"D:\temp\vaultbotTesting");
			foreach (string f in files)
			{
				File.Move(f, f + ".!qB");
			}
			Thread.Sleep(1500);
			foreach (string f in files)
			{
				File.Move(f + ".!qB", f.Substring(0, f.Length));
				Thread.Sleep(100);
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

		[Command("reencode"), Description("Recodifica un archivo y te avisa cuando ha terminado\n**Sobreescribe el archivo original**")]
		public async Task ReEncode(CommandContext ctx, [Description("Ruta completa del archivo a recodificar"), RemainingText] String Ruta_Completa)
		{
			if (!File.Exists(Ruta_Completa))
			{
				ctx.RespondAsync(null, false, HelperMethods.QuickEmbed("No se ha encontrado el archivo", "", false, "#ff0000"));
			} else
			{
				DiscordMessage confirm = await ctx.RespondAsync("Empezando recodificado:");
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



		[Command("respuestaingeniosa"), Description("Tu madre"), RequirePermissions(Permissions.Administrator), Aliases(new[] {"ri"})]
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
