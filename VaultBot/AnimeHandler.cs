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

		//HACK Improve the logic on this command
		//We could implement GetXX_UpdateEmbed as one complexy method
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

				if (animeType == AnimeType.ER_Anime)
				{
					ER_Anime ER_Anime = new ER_Anime(NewPath + NewName);

					if (ER_Anime.Exists())
					{
						Encoder.Instance.AddAnimeToQueue(new Encode(ER_Anime, startEncodeDate));
						if(ShowUpdates) await Channel.SendMessageAsync( GetER_UpdateEmbed(ER_Anime));
					}

				} else if (animeType == AnimeType.SP_Anime)
				{
					SP_Anime SP_Anime = new SP_Anime(NewPath + NewName);
					if (SP_Anime.Exists())
					{
						Encoder.Instance.AddAnimeToQueue(new Encode(SP_Anime, startEncodeDate));
						if (ShowUpdates) await Channel.SendMessageAsync( GetSP_UpdateEmbed(SP_Anime));
					}

				} else if (animeType == AnimeType.JD_Anime)
				{
					JD_Anime JD_Anime = new JD_Anime(NewPath + NewName);
					if (JD_Anime.Exists())
					{
						//Since judas torrents are already encoded we don't need to reencode them
						if (ShowUpdates) await Channel.SendMessageAsync( GetJD_UpdateEmbed(JD_Anime));
					}

				}
			}
		}

		private DiscordEmbed GetER_UpdateEmbed(ER_Anime e)
		{
			string titleOutput = e.GetInfo();
			if (e.HasMulti && e.IsFinale)
			{
				titleOutput += "\n**FINALE** - *Multi Subs*";
			} else
			{
				titleOutput += "\n";
				titleOutput += e.IsFinale ? "**FINALE**" : "";
				titleOutput += e.HasMulti ? "*Multi Subs*" : "";
			}

			string descOutput = "";
			if (e.IsV0) descOutput += "Version Preliminar\n";
			if (e.IsV2) descOutput += "Version Verificada\n";
			descOutput += "Ahora disponible en el servidor";
			return Utilities.QuickEmbed(titleOutput, descOutput);
		}

		private DiscordEmbed GetSP_UpdateEmbed(SP_Anime e)
		{
			string titleOutput = e.GetInfo();

			string descOutput = "";
			if (!String.IsNullOrWhiteSpace(e.ImprovedVersion)) descOutput += $"Version Mejorada *{e.ImprovedVersion}*\n";
			descOutput += "Ahora disponible en el servidor";
			return Utilities.QuickEmbed(titleOutput, descOutput);
		}

		private DiscordEmbed GetJD_UpdateEmbed(JD_Anime e)
		{
			return Utilities.QuickEmbed(
				e.GetInfo(),
				"Ahora disponible en el servidor"
			);
		}
	}
}
