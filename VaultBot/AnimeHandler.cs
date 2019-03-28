using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.ObjectModel;

namespace VaultBot
{
    public class AnimeHandler
    {
        private DiscordChannel _Channel;
        public DiscordChannel Channel { get => _Channel; set => _Channel = value; }

        private FileSystemWatcher MasterWatcher = new FileSystemWatcher();

        private String _AnimePath = @"F:\FTP\Multimedia\Anime";
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
            if (attributes.HasFlag(FileAttributes.Directory))
                SendMessage($"Nuevo Anime: {Nombre}");
            else
                SendMessage($"Nuevo EP: {Nombre}");
        }
       

        public async Task SendMessage(String s)
        {
            await _Channel.SendMessageAsync(s);
        }
    }
}
