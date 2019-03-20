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
                if (!ctx.Channel.IsPrivate)
                {
                    AnimeUpdater.Channel = ctx.Channel;
                    await AnimeUpdater.SendMessage("Canal Seleccionado");
                }
                else
                {
                    await ctx.RespondAsync("No se pueden seleccionar mensajes privados para las publicaciones.");
                }
            }
            else
            {
                await ctx.RespondAsync("Solo el creador del bot tiene acceso a este comando");

            }
        }


        [Command("Load")]
        public async Task Load(CommandContext ctx)
        {
            if (ctx.User.Id == 66139444276625408)
            {
                AnimeUpdater.Load();
            }
            else
            {
                await ctx.RespondAsync("Solo el creador del bot tiene acceso a este comando");
            }
        }


        [Command("List")]
        public async Task List(CommandContext ctx)
        {
                     AnimeUpdater.ListAsync(ctx.Channel, ctx.Member, ctx.User);
        }


        [Command("Init")]
        public async Task Init(CommandContext ctx)
        {
            if (ctx.User.Id == 66139444276625408)
            {
                AnimeUpdater.Channel = ctx.Channel;
                AnimeUpdater.ScanAsync();
                AnimeUpdater.Load();
                AnimeUpdater.ListAsync(ctx.Channel, ctx.Member, ctx.User);
                await AnimeUpdater.SendMessage("Se ha inicializado el bot en este canal");
            }
            else
            {
                await ctx.RespondAsync("Solo el creador del bot tiene acceso a este comando");
            }
        }

        //TODO command Scan 

        [Command("Help")]
        public async Task Help(CommandContext ctx)
        {
            String salida = 
                "Comandos publicos:\n" +
                "-Help\n-Ping\n" +
                "Comandos privados:" +
                "-Init\n-Scan\n-SelectChannel\n";
             await AnimeUpdater.SendMessage(salida);
            
        }

        [Command("Scan")]
        public async Task Scan(CommandContext ctx)
        {
            if (ctx.User.Id == 66139444276625408)
            {
                await AnimeUpdater.ScanAsync();
            }
            else
            {
                await ctx.RespondAsync("Solo el creador del bot tiene acceso a este comando");
            }
        }
    }

}
