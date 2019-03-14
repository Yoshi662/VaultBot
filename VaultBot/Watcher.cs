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
		private String _path;
		private FileSystemWatcher _watcher = new FileSystemWatcher();
		public Watcher(String path, DiscordChannel channel)
		{
			this._channel = channel;
			this._path = path;

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
			SendMessage($"Nuevo EP: {e.Name}");
		}
	}
}