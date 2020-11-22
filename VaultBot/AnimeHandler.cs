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

		/*valid Regexs: 
[Erai-raws] Enen no Shouboutai - Ni no Shou - 18 [1080p][Multiple Subtitle].mkv
[Erai-raws] Enen no Shouboutai - Ni no Shou - 19 [1080p].mkv
[Erai-raws] Dragon Quest - Dai no Daibouken (2020) - 01 [1080p][Multiple Subtitle].mkv
[Erai-raws] Jujutsu Kaisen - 06 [1080p][Multiple Subtitle].mkv
[Erai-raws] Re.Zero kara Hajimeru Isekai Seikatsu 2nd Season - 13 END [1080p][Multiple Subtitle].mkv
[Erai-raws] Majo no Tabitabi - 01 [v2][1080p].mkv
[Erai-raws] Majo no Tabitabi - 04 [v0][1080p].mkv

[Erai-raws] Enen no Shouboutai - Ni no Shou - 18 [1080p][Multiple Subtitle].mkv.!qB
[Erai-raws] Enen no Shouboutai - Ni no Shou - 19 [1080p].mkv.!qB
[Erai-raws] Dragon Quest - Dai no Daibouken (2020) - 01 [1080p][Multiple Subtitle].mkv.!qB
[Erai-raws] Jujutsu Kaisen - 06 [1080p][Multiple Subtitle].mkv.!qB
[Erai-raws] Re.Zero kara Hajimeru Isekai Seikatsu 2nd Season - 13 END [1080p][Multiple Subtitle].mkv.!qB
[Erai-raws] Majo no Tabitabi - 01 [v2][1080p].mkv.!qB
[Erai-raws] Majo no Tabitabi - 04 [v0][1080p].mkv.!qB

[Erai-raws] Enen no Shouboutai - Ni no Shou - 18 [1080p][pre-enc][Multiple Subtitle].mkv.!qB
[Erai-raws] Enen no Shouboutai - Ni no Shou - 19 [1080p][pre-enc].mkv.!qB
[Erai-raws] Dragon Quest - Dai no Daibouken (2020) - 01 [1080p][pre-enc][Multiple Subtitle].mkv.!qB
[Erai-raws] Jujutsu Kaisen - 06 [1080p][pre-enc][Multiple Subtitle].mkv.!qB
[Erai-raws] Re.Zero kara Hajimeru Isekai Seikatsu 2nd Season - 13 END [1080p][pre-enc][Multiple Subtitle].mkv.!qB
[Erai-raws] Majo no Tabitabi - 01 [v2][1080p][pre-enc].mkv.!qB
[Erai-raws] Majo no Tabitabi - 04 [v0][1080p][pre-enc].mkv.!qB
         */

		//groups: 0(ER_Spam) 1(AnimeName) 2(- Nº EP) 3?(Finale) 4?(V0) 5?(V2) 6(Res) 7?(Multiple Subs) 8?(Extension) 9?(Extension)
		public AnimeHandler(String AnimePath)
		{
			this.MasterWatcher.Path = AnimePath;

			this.MasterWatcher.NotifyFilter = NotifyFilters.FileName
											| NotifyFilters.DirectoryName;

			this.MasterWatcher.IncludeSubdirectories = true;

			this.MasterWatcher.Renamed += OnRenamedAsync;

			this.MasterWatcher.EnableRaisingEvents = true;

			Task.Delay(-1);
		}

		private async void OnRenamedAsync(object sender, RenamedEventArgs e)
		{
			String NewName = e.Name.Split('\\').Last();
			String NewPath = e.FullPath.Replace(NewName, "");
			String OldName = e.OldName.Split('\\').Last();
			String OldPath = e.OldFullPath.Replace(OldName, "");

			bool isER = ER_Anime.TitleRegex.IsMatch(NewName);
			//On finished download
			if (isER)
			{
				ER_Anime Newanime = new ER_Anime(NewPath + NewName);
				ER_Anime OldAnime = new ER_Anime(OldPath + OldName);
				if (OldAnime.IsDownloading && !Newanime.IsDownloading) //On Sucessful Download
				{
					if (File.Exists(Newanime.FullPath))
					{
						DateTime startEncodeDate = DateTime.Now.Add(delay);
						Encoder.Instance.AddAnimeToQueue(new Encode(Newanime, startEncodeDate));
					} 


					//Finally we send the update to the server
					string output = $"{Newanime.Title} - {Newanime.N_Ep}";
					if (Newanime.HasMulti && Newanime.IsFinale)
					{
						output += "\n**FINALE** - *Multi Subs*";
					} else
					{
						output += "\n";
						output += Newanime.IsFinale ? "**FINALE**" : "";
						output += Newanime.HasMulti ? "*Multi Subs*" : "";
					}
					//TODO descomentar en version final
					Program.Client.Logger.Log(LogLevel.Information, Events.AnimePublished, $"\"{Newanime.FullFileName}\" Has been downloaded");
					//await Channel.SendMessageAsync(null, false, HelperMethods.QuickEmbed(output, "Ahora disponible en el servidor"));

				} else //We just publish the anime
				{
					if (Path.GetExtension(OldPath) == ".!qB" && Path.GetExtension(NewPath) != ".!qB")
					{
						await Channel.SendMessageAsync(null, false, HelperMethods.QuickEmbed(NewName, "Ahora disponible en el servidor"));
					}
				}
			}
		}

		private void OnCreated(object sender, FileSystemEventArgs e)
		{
			String Nombre = e.Name.Split('\\').Last();
			String path = e.FullPath.Replace(Nombre, "");
			bool isER = ER_Anime.TitleRegex.IsMatch(Nombre);
			if (isER)
			{
				ER_Anime anime = new ER_Anime(Nombre)
				{
					PreEncode = true
				};
				File.Move(path + Nombre, path + anime.FullPath);
			}
		}
	}
}
