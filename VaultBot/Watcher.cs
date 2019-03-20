using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaultBot
{
    class Watcher
    {
        private DiscordChannel _channel;
        public DiscordChannel Channel { set => _channel = value; }

        private String _name;
        public string Name { get => _name; }

        private String _path;

        

        private FileSystemWatcher _watcher = new FileSystemWatcher();
        public bool IsEnabled { get => _watcher.EnableRaisingEvents; set => _watcher.EnableRaisingEvents = value; }
    public Watcher(String path, DiscordChannel channel)
        {
            this._channel = channel;
            this._path = path;
            this._name = path.Split('\\').Last();

            _watcher.Path = _path;

            _watcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;

            _watcher.Created += OnCreate;
            _watcher.EnableRaisingEvents = true;

        }



        public String Save()
        {
            return $"{_name},{_path}";
        }


        public async Task SendMessage(String s)
        {
            await _channel.SendMessageAsync(s);
        }
        private void OnCreate(object source, FileSystemEventArgs e)
        {
            SendMessage($"Nuevo EP: {e.Name}");
        }
    }
}