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
		const string QueuePath = ".\\CurrentQueue.json";

		public bool SendUpdates = false;
		public DiscordChannel UpdatesChannel { get; set; }

		public LinkedList<Encode> EncodeQueue { get; private set; }

		private static Encoder _instance;
		public static Encoder Instance
		{
			get => _instance ?? (_instance ??= new Encoder());
		}

		private Encoder()
		{
			EncodeQueue = new LinkedList<Encode>();
		}

		public void AddAnimeToQueue(Encode e, bool priority = false) //E
		{
			//We Add the anime to the queue.
			if (!CheckIfExists(e.Anime))
			{
				Program.Client.Logger.Log(LogLevel.Information, Events.QueueAdd, $"Added \"{e.Anime.FullFileName}\" to the main queue");
				
				if (priority)
				{
					EncodeQueue.AddFirst(e);
				} else{
					EncodeQueue.AddLast(e);
				}
				SaveCurentQueueToFile(QueuePath);
				if (EncodeQueue.Count == 1)
				{
					Program.Client.Logger.Log(LogLevel.Debug, Events.Queue, "Main Loop Started, encodes pending = " + EncodeQueue.Count);
					Thread th = new Thread(new ThreadStart(() => { EncodeLoop(); }));
					th.Start();
				}
			}
			SendUpdateToChannel();
		}

		private bool CheckIfExists(ER_Anime input)
		{
			//Queue => array (or even list)
			//If two episodes names AND epNº coincide we return false
			bool exists = false;
			Encode[] encs = EncodeQueue.ToArray();
			foreach (Encode item in encs)
			{
				if (input.Title == item.Anime.Title && input.N_Ep == item.Anime.N_Ep)
				{
					exists = true;
				}
			}
			return exists;
		}

		public void EncodeLoop()
		{
			while (EncodeQueue.Count > 0)
			{
				Encode e = EncodeQueue.First();
				WaitUntilNextEncode(e);
				e.Anime = HelperMethods.RemoveDuplicates(e.Anime);
				Encode(e.Anime);
				//Since the starting of some tasks depends on the size of the Queue.
				//We don't remove the element until the very end of this loop
				EncodeQueue.RemoveFirst();
				SaveCurentQueueToFile(QueuePath);
				SendUpdateToChannel();
			}
		}

		private void WaitUntilNextEncode(Encode e)
		{
			while (DateTime.Now < e.EncodeDate)
			{  //TODO Incrase sleep on future 
				Thread.Sleep(TimeSpan.FromSeconds(10));
			}
		}

		private void Encode(ER_Anime anime)
		{
			Thread th = new Thread(new ThreadStart(() =>
			{
				//We filter for duplicates
				ER_Anime originalFiltered = HelperMethods.RemoveDuplicates(anime);

				//We generate the preencode and postenccode names
				ER_Anime preencode = (ER_Anime)originalFiltered.Clone();
				ER_Anime postencode = (ER_Anime)originalFiltered.Clone();

				//We rename the old file
				preencode.PreEncode = true;
				File.Move(originalFiltered.FullPath, preencode.FullPath);

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
				Program.Client.Logger.Log(LogLevel.Information, Events.EncodeStart, $"\"{preencode.Title} - {preencode.N_Ep}\" - Has Started Encoding");
				HandBrakeCLI.Start();
				if (SendUpdates && UpdatesChannel != null)
				{
					HandBrakeCLI.BeginOutputReadLine();
					DateTime lastedit = DateTime.Now;
					HandBrakeCLI.OutputDataReceived += async (object sender, DataReceivedEventArgs e) =>
					{
						if (!String.IsNullOrEmpty(e.Data))
						{
							//TODO increase timespan to 5 - 10 - 15 min ON RELEASE
							if (DateTime.Now - lastedit > TimeSpan.FromMinutes(15))
							{
								lastedit = DateTime.Now;
								SendUpdateToChannel(e.Data);
							}
						}
					};

				}
				HandBrakeCLI.WaitForExit();
				Program.Client.Logger.Log(LogLevel.Information, Events.EncodeEnd, $"\"{preencode.Title} - {preencode.N_Ep}\" - Has Finished Encoding");
				File.Delete(preencode.FullPath);
			}));
			th.Start();
			th.Join();
		}

		public void SaveCurentQueueToFile(string path = QueuePath)
		{
			File.WriteAllText(path, JsonConvert.SerializeObject(EncodeQueue));
		}

		public void LoadQueueFromFile(string path = QueuePath)
		{
			if (File.Exists(path))
			{
				EncodeQueue = JsonConvert.DeserializeObject<LinkedList<Encode>>(
					File.ReadAllText(path)
				);
			}
		}

		public async void SendUpdateToChannel(string data = "") //TODO sustituir embed especifico por 
		{
			DiscordEmbedBuilder builder = new DiscordEmbedBuilder(
			HelperMethods.QuickEmbed(
				@"Current Queue",
				EncodeQueue.Count > 0 ? @"*" + EncodeQueue.Count + " Elementos en la cola*" : @"*No hay items en la cola ahora mismo*"
			));
			Encode[] encodes = EncodeQueue.ToArray();
			
			builder.WithTimestamp(DateTime.Now);

			if (!string.IsNullOrWhiteSpace(data))
				builder.AddField($"{encodes[0].Anime.Title} - {encodes[0].Anime.N_Ep}", "`" + data.Substring(23) + "`");

			if (EncodeQueue.Count > 1)
			{
				for (int i = !string.IsNullOrWhiteSpace(data) ? 0 : 1; i < EncodeQueue.Count; i++)
				{
					builder.AddField($"{encodes[i].Anime.Title} - {encodes[i].Anime.N_Ep}", $"`Recode planeado para el {encodes[i].EncodeDate:yyyy-MM-dd}`");
				}
			}
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
			


			//UpdatesChannel.SendMessageAsync(null, false, builder.Build());
		}

	}
	public class Encode
	{
		public ER_Anime Anime { get; set; }
		public DateTime EncodeDate { get; private set; }
		public Encode(ER_Anime anime, DateTime EncodeDate)
		{
			this.Anime = anime;
			this.EncodeDate = EncodeDate;
		}


	}
}
