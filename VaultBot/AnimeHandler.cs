using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VaultBot
{
	public class AnimeHandler
	{

		private DiscordChannel _Channel;

		public DiscordChannel Channel { get => _Channel; set => _Channel = value; }

		private TimeSpan delay = TimeSpan.FromDays(7);

		public FileSystemWatcher MasterWatcher = new FileSystemWatcher();

		public bool ShowUpdates = true;

		public AnimeHandler(string AnimePath)
		{
			this.MasterWatcher.Path = AnimePath;

			this.MasterWatcher.InternalBufferSize = 32768; //32KB

			this.MasterWatcher.NotifyFilter = NotifyFilters.FileName
											| NotifyFilters.DirectoryName;

			this.MasterWatcher.IncludeSubdirectories = true;

			this.MasterWatcher.Renamed += OnRenamedAsync;

			this.MasterWatcher.EnableRaisingEvents = true;

			Task.Delay(-1);
		}

		private async void OnRenamedAsync(object sender, RenamedEventArgs e)
		{
			string NewName = e.Name.Split('\\').Last();
			string NewPath = e.FullPath.Replace(NewName, "");
			string OldName = e.OldName.Split('\\').Last();
			string OldPath = e.OldFullPath.Replace(OldName, "");

			DateTime startEncodeDate = DateTime.Now.Add(delay);

			//On finished download
			if (Path.GetExtension(OldName) == ".!qB" && Path.GetExtension(NewName) != ".!qB")
			{
				//We get the type of anime
				AnimeType animeType = Utilities.GetAnimeType(NewName);
				Anime a = animeType switch
				{
					AnimeType.ER_Anime => new ER_Anime(NewPath + NewName),
					AnimeType.SP_Anime => new SP_Anime(NewPath + NewName),
					AnimeType.JD_Anime => new JD_Anime(NewPath + NewName),
					AnimeType.EM_Anime => new EM_Anime(NewPath + NewName),
					_ => new Anime(NewPath + NewName),
				};
				if (a.Exists())
				{
					if (a.ShowUpdates) await Channel.SendMessageAsync(a.UpdateEmbed);
					if (!a.IsEncoded) Encoder.Instance.AddAnimeToQueue(new Encode(a, startEncodeDate));
				}
			}
		}
	}
}
