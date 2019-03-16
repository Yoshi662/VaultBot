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
        public DiscordChannel Channel { get => _channel; set => _channel = value; }

        private String _path;
        public readonly String AnimeName;

        private FileSystemWatcher _watcher = new FileSystemWatcher();

       
        public Watcher(String path, DiscordChannel channel)
		{
			this._channel = channel;
            this._path = path;

            String[] temp = path.Split('\\');

            this.AnimeName = temp.Last();

			_watcher.Path = _path;

			_watcher.NotifyFilter = NotifyFilters.LastAccess
								 | NotifyFilters.LastWrite
								 | NotifyFilters.FileName
								 | NotifyFilters.DirectoryName;

			_watcher.Created += OnCreate;
			_watcher.EnableRaisingEvents = true;
			
		}

       

        public async Task SendMessage(String s)
		{
			await _channel.SendMessageAsync(s);
		}
		private void OnCreate(object source, FileSystemEventArgs e)
		{
			SendMessage($"Nuevo EP: {AnimeName}");
		}
	}
}