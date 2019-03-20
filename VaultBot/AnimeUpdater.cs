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
		public DiscordChannel Channel { get => _Channel; set { _Channel = value;
                foreach (Watcher w in _Watchers)
                {
                    w.Channel = value;
                }
            } }



        //private String _AnimePath = @"F:\FTP\Multimedia\Anime";
        private String _AnimePath = @"D:\AAA";
        private String _AnimeListPath = "AnimeDirectory.csv";
        

        private List<Watcher> _Watchers = new List<Watcher>();




		//TODO leer la carpeta entera, 
		//seleccionar las carpetas cuya fecha de modificacion sea menor a dos semanas y añadirles Watchers
		//Crear Watchers
		//Crear una forma amigable de mostrar los animes actualizados



		public async Task ScanAsync()
		{
			String[] listaAnimes = Directory.GetDirectories(_AnimePath);

			DateTime minus2weeks = DateTime.Now.Subtract(new TimeSpan(14/*CATORCE*/, 0, 0, 0));

            String csv = "";
            foreach (String str in listaAnimes)
            {
                DateTime dt = Directory.GetLastAccessTime(str);
                if (dt.CompareTo(minus2weeks) >= 0)
                {
                    csv += $"{str.Split('\\').Last()},{str}\n";
                }
            }
                File.WriteAllText(_AnimeListPath, csv);

            Load();
            
        await _Channel.SendMessageAsync("Lista de animes a supervisar actualizada");
    }


		

		public void SetPath(String s)
		{
			_AnimePath = s;
			ScanAsync();
		}

        public void Load()
        {//Vacia el array
            _Watchers.RemoveRange(0, _Watchers.Count);
            String csv = File.ReadAllText(_AnimeListPath);
            String[] csvWatchers = csv.Split('\n');
            String[] watcher;
            foreach (String strWatcher in csvWatchers)
            {
                watcher = strWatcher.Split(',');
                //0 = nombre , 1 = Path
                AddWatcherASync(new Watcher(watcher[1], _Channel));
            }
            _Channel.SendMessageAsync("Se ha empezado a supervisar animes, puedes usar el comando -List para saber cuales son");
        }

        internal async Task ListAsync(DiscordChannel discordChannel, DiscordMember member, DiscordUser discordUser)
        {
            String salida = "Lista de animes:\n";
            foreach (Watcher watcher in _Watchers)
            {
                salida += watcher.Name + "\n";
            }
            discordChannel.SendMessageAsync("Lista Enviada por DM");

            DiscordDmChannel dmChannel = await member.CreateDmChannelAsync();
            dmChannel.SendMessageAsync(salida);
                }

        public async Task SendMessage(String s)
		{
			await _Channel.SendMessageAsync(s);
		}
	
        private async Task AddWatcherASync(Watcher w)
        {
            _Watchers.Add(w);
        }
    }
}
