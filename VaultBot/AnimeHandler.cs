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
//BUG No guarda bien el nombre del archivo cuando NO es un archivo de Erai, "Guarda la carpeta"
	public class AnimeHandler
	{

		private DiscordChannel _Channel;

		public DiscordChannel Channel { get => _Channel; set => _Channel = value; }

		private TimeSpan delay = TimeSpan.FromDays(7);

		public FileSystemWatcher MasterWatcher = new FileSystemWatcher();

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
		private async void OnRenamedAsync(object sender, RenamedEventArgs e)
		{
			string NewName = e.Name.Split('\\').Last();
			string NewPath = e.FullPath.Replace(NewName, "");
			string OldName = e.OldName.Split('\\').Last();
			string OldPath = e.OldFullPath.Replace(OldName, "");

			bool isER = ER_Anime.TitleRegex.IsMatch(NewName);
			//On finished download
			if (isER)
			{
				ER_Anime Newanime = new ER_Anime(NewPath + NewName);
				ER_Anime OldAnime = new ER_Anime(OldPath + OldName);
				if (OldAnime.IsDownloading && !Newanime.IsDownloading) //On Sucessful Download
				{
					//We start the encode
					if (File.Exists(Newanime.FullPath))
					{
						DateTime startEncodeDate = DateTime.Now.Add(delay);
						Encoder.Instance.AddAnimeToQueue(new Encode(Newanime, startEncodeDate));
					}


					//Finally we send the update to the server
					string titleOutput = $"{Newanime.Title} - {Newanime.N_Ep}";
					if (Newanime.HasMulti && Newanime.IsFinale)
					{
						titleOutput += "\n**FINALE** - *Multi Subs*";
					} else
					{
						titleOutput += "\n";
						titleOutput += Newanime.IsFinale ? "**FINALE**" : "";
						titleOutput += Newanime.HasMulti ? "*Multi Subs*" : "";
					}

					string descOutput = "";
					if (Newanime.IsV0) descOutput += "Version Preliminar\n";
					if (Newanime.IsV2) descOutput += "Version Verificada\n";
					descOutput += "Ahora disponible en el servidor";


					Program.Client.Logger.Log(LogLevel.Information, Events.AnimePublished, $"\"{Newanime.FullFileName}\" Has been downloaded");
					await Channel.SendMessageAsync(null, false, HelperMethods.QuickEmbed(titleOutput, descOutput));
				}
			} else //We just publish the anime
			{
				if (Path.GetExtension(OldName) == ".!qB" && Path.GetExtension(NewName) != ".!qB")
				{
					await Channel.SendMessageAsync(null, false, HelperMethods.QuickEmbed(NewName, "Ahora disponible en el servidor"));
					DateTime startEncodeDate = DateTime.Now.Add(delay);
					Encoder.Instance.AddAnimeToQueue(new Encode(new Anime(NewPath), startEncodeDate));
				}
			}
		}
	}
}
