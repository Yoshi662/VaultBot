using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace VaultBot
{
    public class AnimeHandler
    {
        private DiscordChannel _Channel;
        public DiscordChannel Channel { get => _Channel; set => _Channel = value; }

        private FileSystemWatcher MasterWatcher = new FileSystemWatcher();

        private String _AnimePath = @"F:\FTP\Multimedia\Anime";

        public Regex HS_regex = new Regex(@"(\[HorribleSubs\] )([\w ]*)(- )(\d*)( \[\d{4}p\])(.\w{3})");
        public AnimeHandler()
        {
            this.MasterWatcher.Path = _AnimePath;

            this.MasterWatcher.NotifyFilter = NotifyFilters.LastAccess
                                            | NotifyFilters.LastWrite
                                            | NotifyFilters.FileName
                                            | NotifyFilters.DirectoryName;

            this.MasterWatcher.IncludeSubdirectories = true;

            this.MasterWatcher.Created += OnCreated;

            this.MasterWatcher.EnableRaisingEvents = true;

            Task.Delay(-1);
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            FileAttributes attributes = File.GetAttributes(e.FullPath);
            String Nombre = e.Name.Split('\\').Last();
            bool isHS = HS_regex.IsMatch(Nombre);

            if(isHS)
            {
                String[] HS_subs = HS_regex.Split(Nombre);
                SendEmbed(HS_subs[2], HS_subs[4]);
            } else
            {
                Nombre = Nombre.Replace(".mkv", "");
                Nombre = Nombre.Replace(".!qB", "");
                SendEmbed(Nombre);
            }
        }

        private void SendEmbed(String archivo)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                        .WithTitle(archivo)
                        .WithDescription("Ahora disponible en el servidor")
                        .WithColor(new DiscordColor(0x2461DC))
                        .WithFooter(
                            "A Yoshi's Bot",
                            "https://i.imgur.com/rT9YocG.jpg"
                        ).WithThumbnailUrl("https://i.imgur.com/QeBaVkD.png");
            DiscordEmbed embed = builder.Build();
            Channel.SendMessageAsync(null, false, embed);
        }

        private void SendEmbed(String Nombre, String N_Ep)
        {
            SendEmbed($"{Nombre} - {N_Ep}");
        }
        private void SendEmbed(String Nombre, int N_Ep)
        {
            SendEmbed(Nombre, N_Ep.ToString());
        }

    }
}
