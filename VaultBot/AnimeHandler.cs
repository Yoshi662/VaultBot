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

        public FileSystemWatcher MasterWatcher = new FileSystemWatcher();

        public Regex HS_regex = new Regex(@"(\[HorribleSubs\] )(.*)(- )(\d*)( \[\d{4}p\])([\.\w!]{4})*");

        //groups: 0(ER_Spam) 1(AnimeName) 2(- Nº RP) 3*(Finale) 4(Res) 5*(Multiple Subs) 6*(Extension*s)
        public Regex ER_regex = new Regex(@"(\[Erai\-raws\] )(.*)( - \d{1,3})( END)*( \[1080p\])(\[Multiple Subtitle\])*([\.\w!]{4})*");
        public AnimeHandler(String AnimePath)
        {
            this.MasterWatcher.Path = AnimePath;

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
            bool isER = ER_regex.IsMatch(Nombre);

            if (isHS)
            {
                String[] HS_subs = HS_regex.Split(Nombre);
                for (int i = 0; i < HS_subs.Length; i++) //Quitar : espacios
                {
                    HS_subs[i] = HS_subs[i].Trim();
                }
                SendUpdateEmbed(HS_subs[2], HS_subs[4]);
            }
            else if (isER)
            {
                string[] ER_rawrs = ER_regex.Split(Nombre);
                for (int i = 0; i < ER_rawrs.Length; i++) //Quitar : espacios
                {
                    ER_rawrs[i] = ER_rawrs[i].Trim();
                    if (i == 3)
                    {
                        ER_rawrs[i] = ER_rawrs[i].Substring(1).Trim();
                    }
                }

                bool isfinale = string.IsNullOrEmpty(ER_rawrs[4]),
                     hasmultiplesubs = string.IsNullOrEmpty(ER_rawrs[6]);
                string output = "\n" + (isfinale ? " FINALE" : "") + (hasmultiplesubs ? " Multi Subs" : "");

                SendUpdateEmbed(ER_rawrs[2], string.IsNullOrWhiteSpace(output) ? ER_rawrs[3] : ER_rawrs[2] + output);

            }
            else
            {
                String[] extensiones = { "mp4", "avi", "mkv", "!qB" }; //quitar : extensiones
                foreach(string s in extensiones)
                {
                    Nombre.Replace($".{s}", "");
                }
                SendUpdateEmbed(Nombre);
            }
        }

        private void SendUpdateEmbed(String archivo)
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
        private void SendUpdateEmbed(String Nombre, String N_Ep)
        {
            SendUpdateEmbed($"{Nombre} - {N_Ep}");
        }
        private void SendUpdateEmbed(String Nombre, int N_Ep)
        {
            SendUpdateEmbed(Nombre, N_Ep.ToString());
        }
    }
}
