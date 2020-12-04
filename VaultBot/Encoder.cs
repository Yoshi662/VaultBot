using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DSharpPlus.Entities;

namespace VaultBot
{
	public class Encoder
	{
		/*TODO Check that everything works as expected (Expand)
		 *RECAP So please do some testing over the queue.
		 *RECAP How order and encodedate works altogether and if it is necesary to make a method to order the queue due to Encode date.
		 */
		const string QueuePath = ".\\CurrentQueue.json";

		public bool SendUpdates = false;
		public DiscordChannel UpdatesChannel { get; set; }

		public LinkedList<Encode> EncodeQueue { get; private set; }

		private static Encoder _instance;
		/// <summary>
		/// Gets the current Singleton Instance of the <see cref="Encoder"/>
		/// </summary>
		public static Encoder Instance
		{
			get => _instance ?? (_instance ??= new Encoder());
		}

		private Encoder()
		{
			EncodeQueue = new LinkedList<Encode>();
		}

		public void AddAnimeToQueue(Encode e, bool priority = false, bool updateMsgQueue = true) //E
		{
			//We Add the anime to the queue.
			if (!CheckIfExists(e.Anime))
			{
				Program.Client.Logger.Log(LogLevel.Information, Events.QueueAdd, $"Added \"{e.Anime.FullFileName}\" to the main queue");

				if (priority)
				{
					EncodeQueue.AddFirst(e);
				} else
				{
					EncodeQueue.AddLast(e);
				}
				SaveCurentQueueToFile(QueuePath);
				if (EncodeQueue.Count == 1)
				{
					Thread th = new Thread(new ThreadStart(() => { EncodeLoop(); }));
					th.Start();
				}
			}

			if (updateMsgQueue) SendUpdateToChannel();
		}

		private bool CheckIfExists(Anime input)
		{
			//Queue => array (or even list)
			//If two episodes names AND epNº coincide we return false
			bool exists = false;
			Encode[] encs = EncodeQueue.ToArray();
			foreach (Encode item in encs)
			{
				if (item.Anime is ER_Anime && input is ER_Anime)
				{
					exists = (item.Anime as ER_Anime).Coincide((ER_Anime)input);
				} else
				{
					exists = item.Anime.FullFileName == input.FullFileName;
				}
			}
			return exists;
		}

		public void EncodeLoop()
		{
			Program.Client.Logger.Log(LogLevel.Debug, Events.Queue, "Main Loop Started, encodes pending = " + EncodeQueue.Count);
			while (EncodeQueue.Count > 0)
			{
				//We wait until the first encode datetime has reached
				Encode e = WaitUntilFirstEncode();

				if (e.Anime is ER_Anime)
				{
					//We remove the duplicates
					e.Anime = HelperMethods.RemoveDuplicates((ER_Anime)e.Anime);
				}


				//And we encode the anime
				Encode(e.Anime);
				//Since the starting of some tasks depends on the size of the Queue.
				//We don't remove the element until the very end of this loop
				EncodeQueue.RemoveFirst();
				SaveCurentQueueToFile(QueuePath);
				SendUpdateToChannel();
			}
		}
		/// <summary>
		/// This method waits until the Encode date of the First <see cref="VaultBot.Encode"/> in <see cref="EncodeQueue"/> (Updated Dynamically)
		/// </summary>
		private Encode WaitUntilFirstEncode()
		{
			Encode e = EncodeQueue.First();
			while (DateTime.Now < e.EncodeDate)
			{
				Thread.Sleep(TimeSpan.FromMinutes(1));
				if (!e.Equals(EncodeQueue.First))
				{
					e = EncodeQueue.First();
				}
			}
			return e;
		}

		private void Encode(Anime anime)
		{
			Thread th = new Thread(new ThreadStart(() =>
			{
				//We generate the preencode and postenccode names
				Anime preencode = (Anime)anime.Clone();
				Anime postencode = (Anime)anime.Clone();

				if (anime is ER_Anime)
				{
					(preencode as ER_Anime).PreEncode = true;
				} else
				{
					preencode.FullFileName = "preencode_" + preencode.FullFileName;
				}

				//We rename the old file
				File.Move(anime.FullPath, preencode.FullPath);

				//We start encoding
				Process HandBrakeCLI = new Process();
				HandBrakeCLI.StartInfo = new ProcessStartInfo
				{
					FileName = "HandbrakeCLI.exe",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true,
					Arguments = $"--preset-import-file \"AnimePreset.json\" -Z \"General Purpose Anime H.265 10-bit\" --input \"{preencode.FullPath}\" --output \"{postencode.FullPath}\""
				};

				if (anime is ER_Anime)
				{
					Program.Client.Logger.Log(LogLevel.Information, Events.EncodeStart, $"\"{(preencode as ER_Anime).Title} - {(preencode as ER_Anime).N_Ep}\" - Has Started Encoding");
				} else
				{
					Program.Client.Logger.Log(LogLevel.Information, Events.EncodeStart, $"\"{preencode.FullFileName}\" - Has Started Encoding");
				}

				HandBrakeCLI.Start();
				if (SendUpdates && UpdatesChannel != null)
				{
					HandBrakeCLI.BeginOutputReadLine();
					DateTime lastedit = DateTime.Now;
					HandBrakeCLI.OutputDataReceived += async (object sender, DataReceivedEventArgs e) =>
					{
						if (!string.IsNullOrEmpty(e.Data))
						{
							if (DateTime.Now - lastedit > TimeSpan.FromMinutes(15))
							{
								lastedit = DateTime.Now;
								SendUpdateToChannel(e.Data);
							}
						}
					};

				}
				HandBrakeCLI.WaitForExit();
				if (anime is ER_Anime)
				{
					Program.Client.Logger.Log(LogLevel.Information, Events.EncodeEnd, $"\"{(preencode as ER_Anime).Title} - {(preencode as ER_Anime).N_Ep}\" - Has Finished Encoding");
				} else
				{
					Program.Client.Logger.Log(LogLevel.Information, Events.EncodeEnd, $"\"{preencode.FullFileName}\" - Has Finished Encoding");
				}

				File.Delete(preencode.FullPath);
			}));
			th.Start();
			th.Join();
		}

		public void SaveCurentQueueToFile(string path = QueuePath)
		{
			List<TinyEncode> Queue = new List<TinyEncode>();
			foreach (var item in EncodeQueue)
			{
				Queue.Add(new TinyEncode
				{
					FullPath = item.Anime.FullPath,
					EncodeDate = item.EncodeDate,
					AnimeType = item.Anime is ER_Anime ? (int)AnimeType.ER_Anime : (int)AnimeType.Anime
				}); ;
			}
			File.WriteAllText(QueuePath, JsonConvert.SerializeObject(Queue, Formatting.Indented));

			Program.Client.Logger.Log(LogLevel.Debug, Events.QueueSave, "Queue has been saved in " + QueuePath);
		}

		public void LoadQueueFromFile(string path = QueuePath)
		{
			if (File.Exists(path) && EncodeQueue.Count == 0)
			{
				List<TinyEncode> queue = JsonConvert.DeserializeObject<List<TinyEncode>>(
				File.ReadAllText(path)
				);

				foreach (var item in queue)
				{
					Encode e;
					if (item.AnimeType == (int)AnimeType.ER_Anime)
					{
						e = new Encode(new ER_Anime(item.FullPath), item.EncodeDate);
					} else
					{
						e = new Encode(new Anime(item.FullPath), item.EncodeDate);
					}
					AddAnimeToQueue(e, false, false);
				}
				Program.Client.Logger.Log(LogLevel.Debug, Events.QueueLoad, "Queue has been loaded from " + QueuePath);
			}
		}

		public async void SendUpdateToChannel(string data = "")
		{
			DiscordEmbedBuilder builder = new DiscordEmbedBuilder(
			//We create the title and desc referencing a base
			HelperMethods.QuickEmbed(
				@"Recodificado de animes",
				EncodeQueue.Count > 0 ? @"*" + EncodeQueue.Count + " Elementos en la cola*" : @"*No hay items en la cola ahora mismo*"
			));
			//we set the actual time
			builder.WithTimestamp(DateTime.Now);
			Encode[] encodes = EncodeQueue.ToArray();

			//If we are not recoding something Data = ""
			//So if we recieve data we will parse it so it's more readable
			if (!string.IsNullOrWhiteSpace(data))
			{
				if (encodes[0].Anime is ER_Anime)
				{
					builder.AddField($"{(encodes[0].Anime as ER_Anime).Title} - {(encodes[0].Anime as ER_Anime).N_Ep}", "`" + data.Substring(23) + "`");
				} else
				{
					builder.AddField($"{encodes[0].Anime.FullFileName}", "`" + data.Substring(23) + "`");
				}

			}
			if (EncodeQueue.Count >= 1) //IF we have elements on the queue
			{
				//Depending if we have data we choose to start on the first or on the second
				for (int i = string.IsNullOrWhiteSpace(data) ? 0 : 1; //If there is data. we skip the first one since We've covered that already
						i < (EncodeQueue.Count > 24 ? 24 : EncodeQueue.Count); //There is a limit of fields there can be in an embed
						i++)
				{
					if (encodes[i].Anime is ER_Anime)
					{
						builder.AddField($"{(encodes[i].Anime as ER_Anime).Title} - {(encodes[i].Anime as ER_Anime).N_Ep}", $"`Recode planeado para el {encodes[i].EncodeDate:yyyy-MM-dd}`");
					} else
					{
						builder.AddField($"{encodes[i].Anime.FullFileName}", $"`Recode planeado para el {encodes[i].EncodeDate:yyyy-MM-dd}`");
					}
				}
			}

			//If there is no message we created and we pin it
			try
			{
				DiscordMessage msg = UpdatesChannel.GetPinnedMessagesAsync().Result.First();
				msg.ModifyAsync(null, builder.Build());
			}
			catch (InvalidOperationException)
			{
				DiscordMessage msg = await UpdatesChannel.SendMessageAsync(null, false, builder.Build());
				await msg.PinAsync();
			}

		}
	}

	/// <summary>
	/// We're using this struct to store the essential data
	/// </summary>
	public struct TinyEncode
	{
		public string FullPath { get; set; }
		public DateTime EncodeDate { get; set; }
		public int AnimeType { get; set; }
	}

	public enum AnimeType{
		Anime,
		ER_Anime
	}
}
