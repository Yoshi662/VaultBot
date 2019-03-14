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
	public class AnimeUpdater
	{
		private DiscordChannel _Channel;
		public DiscordChannel Channel { get => _Channel; set => _Channel = value; }

		private String _Path = @"F:\FTP\Multimedia\Anime";
		//private String _Path = @"D:\AAA";

		private List<Watcher> _Watchers = new List<Watcher>();




		//TODO leer la carpeta entera, 
		//seleccionar las carpetas cuya fecha de modificacion sea menor a dos semanas y añadirles Watchers
		//Crear Watchers
		//Crear una forma amigable de mostrar los animes actualizados



		public async Task ScanAsync()
		{
			String salida = "Se han añadido los siguientes animes\n";
			String[] listaAnimes = Directory.GetDirectories(_Path);
			await _Channel.SendMessageAsync("Buscando animes");

			DateTime minus2weeks = DateTime.Now.Subtract(new TimeSpan(14/*CATORCE*/, 0, 0, 0));
			foreach (String str in listaAnimes)
			{
				DateTime dt = Directory.GetLastAccessTime(str);
				if (dt.CompareTo(minus2weeks) >= 0)
				{
					//es mas reciente de dos semanas
					AddWatcherASync(new Watcher(str, _Channel));
					salida += "Añadido: " + str.Substring(24) + "\n";
				}
			}
			await _Channel.SendMessageAsync(salida);
		}


		private async Task AddWatcherASync(Watcher w)
		{
			_Watchers.Add(w);
		}

		public void SetPath(String s)
		{
			_Path = s;
			ScanAsync();
		}

		public async Task SendMessage(String s)
		{
			await _Channel.SendMessageAsync(s);
		}
		private void OnCreate(object source, FileSystemEventArgs e)
		{
			SendMessage($"Nuevo EP: {e.Name}");
		}
	}
}
