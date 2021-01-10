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

			DateTime startEncodeDate = DateTime.Now.Add(delay);

			//On finished download
			if (Path.GetExtension(OldName) == ".!qB" && Path.GetExtension(NewName) != ".!qB")
			{

				if (ER_Anime.TitleRegex.IsMatch(NewName))
				{
					ER_Anime ER_Anime = new ER_Anime(NewPath + NewName);

					string titleOutput = ER_Anime.GetInfo();
					if (ER_Anime.HasMulti && ER_Anime.IsFinale)
					{
						titleOutput += "\n**FINALE** - *Multi Subs*";
					} else
					{
						titleOutput += "\n";
						titleOutput += ER_Anime.IsFinale ? "**FINALE**" : "";
						titleOutput += ER_Anime.HasMulti ? "*Multi Subs*" : "";
					}

					string descOutput = "";
					if (ER_Anime.IsV0) descOutput += "Version Preliminar\n";
					if (ER_Anime.IsV2) descOutput += "Version Verificada\n";
					descOutput += "Ahora disponible en el servidor";


					if (ER_Anime.Exists())
					{
						Encoder.Instance.AddAnimeToQueue(new Encode(ER_Anime, startEncodeDate));
						await Channel.SendMessageAsync(null, false, HelperMethods.QuickEmbed(titleOutput, descOutput));
					}

				} else if (SP_Anime.TitleRegex.IsMatch(NewName))
				{
					SP_Anime SP_Anime = new SP_Anime(NewPath + NewName);
					string titleOutput = SP_Anime.GetInfo();

					string descOutput = "";
					if (!String.IsNullOrWhiteSpace(SP_Anime.ImprovedVersion)) descOutput += $"Version Mejorada *{SP_Anime.ImprovedVersion}*\n";
					descOutput += "Ahora disponible en el servidor";

					if (SP_Anime.Exists())
					{
						Encoder.Instance.AddAnimeToQueue(new Encode(SP_Anime, startEncodeDate));
						await Channel.SendMessageAsync(null, false, HelperMethods.QuickEmbed(titleOutput, descOutput));
					}

				} else //We just publish the anime
				{
					Anime a = new Anime(NewPath + NewName); //TODO CHECK THIS LINE (We might need to add a + "//" + in the middle) //a
					await Channel.SendMessageAsync(null, false, HelperMethods.QuickEmbed(a.GetInfo(), "Ahora disponible en el servidor"));
					Encoder.Instance.AddAnimeToQueue(new Encode(a, startEncodeDate));
				}
			}
		}
	}
}
