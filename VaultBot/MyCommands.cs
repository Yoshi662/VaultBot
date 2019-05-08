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
    [Obsolete("Esta clase no tiene uso, pero se mantendra para futuros comandos")]
    public class MyCommands : BaseCommandModule
    {    
        [Command("Ping")]
        public async Task Pinger(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");
            await ctx.RespondAsync($"{emoji} Pong! Ping: {ctx.Client.Ping}ms");

        }

        [Command("Help")]
        public async Task Help(CommandContext ctx)
        {
            String salida = 
                "Comandos publicos:\n" +
                "    `-Help` - Muestra este mensaje\n" +
                "Comandos privados\n" +
                "    `-SelectChannel` - Selecciona el canal por el cual se van a enviar las notificaciones";
             await ctx.RespondAsync(salida);
            
        }
    }

}
