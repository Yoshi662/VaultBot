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
        public AnimeHandler AnimeUpdater { get; set; } = new AnimeHandler();


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


        [Command("Help")]
        public async Task Help(CommandContext ctx)
        {
            String salida = 
                "Comandos publicos\n" +
                "-Help: Muestra este mensaje)\n" +
                "Comandos privados\n" +
                "-SelectChannel: Selecciona el canal por el cual se van a enviar las notificaciones";
             await AnimeUpdater.SendMessage(salida);
            
        }
    }

}
