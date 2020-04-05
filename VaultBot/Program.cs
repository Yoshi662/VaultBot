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


namespace VaultBot
{
    //TODO
    /*Crear propiedad privada que cambie si el supervisor de archivos de AnimeUpdater 
     */

    public class Program
    {
        internal readonly String version = "1.6.3";
        internal readonly String internalname = "OP 7 issues";

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

            e.Client.DebugLogger.LogMessage(LogLevel.Info, "Vaultbot", "Client is ready to process events.", DateTime.Now);

            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "Vaultbot", $"Guild available: {e.Guild.Name}", DateTime.Now);

            return Task.CompletedTask;
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "Vaultbot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

            while (!HasInternetConnection())
            {
                e.Client.DebugLogger.LogMessage(LogLevel.Error, "Vaultbot", $"Can't connect to the Discord Servers. Reconnecting", DateTime.Now);
                Thread.Sleep(5000);
            }

            prog.RunBotAsync().GetAwaiter().GetResult();
            return Task.CompletedTask;
        }

        private async Task<Task> Client_MessageCreated(MessageCreateEventArgs e)
        {

            string mensaje = e.Message.Content;

            if (mensaje.StartsWith("-new"))
            {
                string embedmensaje = mensaje.StartsWith("-new ") ? mensaje.Substring(5) : mensaje.Substring(4);
                await senderChannel.SendMessageAsync(null, false, NewThingEmbed(embedmensaje));
            }

            mensaje = mensaje.ToLower();

            if (!mensaje.StartsWith("-")) return Task.CompletedTask;

            if (mensaje.StartsWith("-ping"))
            {
                await e.Message.RespondAsync("Pong! " + Client.Ping + "ms");
                return Task.CompletedTask;
            }

            if (mensaje.StartsWith("-help"))
            {
                DiscordMember member = (DiscordMember)e.Author;
                await member.SendMessageAsync(GenerateHelp(member));

                await e.Message.RespondAsync("Ayuda enviada por mensaje privado");
                return Task.CompletedTask;
            }

            if (mensaje.StartsWith("-status"))
            {
                bool status = AnimeUpdater.MasterWatcher.EnableRaisingEvents;
                String texto = $"Notificaciones {(status ? "Activadas" : "Desactivadas")}";
                DiscordColor color = new DiscordColor(status ? "#00ff00" : "#ff0000");
                e.Channel.SendMessageAsync(null, false, QuickEmbed(texto, color));
                return Task.CompletedTask;
            }
            //#00ff00 verde - #ff0000 rojo
            if (mensaje.StartsWith("-start"))
            {
                AnimeUpdater.MasterWatcher.EnableRaisingEvents = true;
                await Client.UpdateStatusAsync(null, UserStatus.Online, null);
                String texto = $"Notificaciones activadas";
                DiscordColor color = new DiscordColor("#00ff00");
                await e.Channel.SendMessageAsync(null, false, QuickEmbed(texto, color));
                return Task.CompletedTask;
            }

            if (mensaje.StartsWith("-stop"))
            {
                AnimeUpdater.MasterWatcher.EnableRaisingEvents = false;
                await Client.UpdateStatusAsync(null, UserStatus.DoNotDisturb, null);
                String texto = $"Notificaciones desactivadas";
                DiscordColor color = new DiscordColor("#ff0000");
                await e.Channel.SendMessageAsync(null, false, QuickEmbed(texto, color));
                return Task.CompletedTask;
            }

            if (mensaje.StartsWith("-version"))
            {
                await e.Channel.SendMessageAsync(null, false, GetVersionEmbed());
                return Task.CompletedTask;
            }


            return Task.CompletedTask;
        }

        private DiscordEmbed NewThingEmbed(String titulo)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                        .WithTitle(titulo)
                        .WithDescription("Ahora disponible en el servidor")
                        .WithColor(new DiscordColor(0x2461DC))
                        .WithFooter(
                            "A Yoshi's Bot",
                            "https://i.imgur.com/rT9YocG.jpg"
                        ).WithThumbnailUrl("https://i.imgur.com/QeBaVkD.png");
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
        private string GenerateHelp(DiscordMember member)
        {
            String salida = ">>> Comandos Actuales:" +
                "\n-Help: Muestra este texto de ayuda" +
                "\n-Ping: Muestra la latencia del bot" +
                "\n-Version: Muestra la version actual de VaultBot" +
                "\n-Status: Muestra si las notificaciones estan activadas" +
                "\n-Start: Activa las notificaciones" +
                "\n-Stop: Para las notificaciones" +
                "\n-New: Muestra una notificacion por el canal de Updates";
            return salida;
        }

        private bool HasInternetConnection()
        {
            Ping sender = new Ping();
            PingReply respuesta = sender.Send("discordapp.com");
            return respuesta.Status.HasFlag(IPStatus.Success);
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
