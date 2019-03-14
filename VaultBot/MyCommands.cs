using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace VaultBot
{
	public class MyCommands
	{
		public AnimeUpdater AnimeUpdater { get; set; } = new AnimeUpdater();


		[Command("Ping")]
		public async Task Pinger(CommandContext ctx)
		{

			// let's trigger a typing indicator to let
			// users know we're working
			await ctx.TriggerTypingAsync();

			// let's make the message a bit more colourful
			var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");

			// respond with ping
			await ctx.RespondAsync($"{emoji} Pong! Ping: {ctx.Client.Ping}ms");

		}

		[Command("SelectChannel")]
		public async Task SetChannel(CommandContext ctx)
		{
			if (ctx.User.Id == 66139444276625408)
			{
				AnimeUpdater.Channel = ctx.Channel;
				await AnimeUpdater.SendMessage("Canal Seleccionado");
			}
			else
			{
				await ctx.RespondAsync("Solo el creador del bot tiene acceso a este comando");
			}
		}


		[Command("Init")]
		public async Task Init(CommandContext ctx)
		{
			if (ctx.User.Id == 66139444276625408)
			{
				AnimeUpdater.Channel = ctx.Channel;
				await AnimeUpdater.ScanAsync();
				await AnimeUpdater.SendMessage("Se ha inicializado el bot en este canal");
			}
			else
			{
				await ctx.RespondAsync("Solo el creador del bot tiene acceso a este comando");
			}
		}

		//TODO command Scan 

		

		[Command("Scan")]
		public async Task Scan(CommandContext ctx)
		{
			if (ctx.User.Id == 66139444276625408)
			{
				AnimeUpdater.ScanAsync();
				await AnimeUpdater.SendMessage("Se ha inicializado el servicio de vigilancia");
			}
			else
			{
				await ctx.RespondAsync("Solo el creador del bot tiene acceso a este comando");
			}
		}
	}

}
