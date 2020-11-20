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

namespace VaultBot
{
	public class Encoder
	{
		const string QueuePath = ".\\CurrentQueue.json";

		public Queue<Encode> EncodeQueue { get; private set; }

		private static Encoder _instance;
		public static Encoder Instance
		{
			get => _instance ?? (_instance ??= new Encoder());
		}

		private Encoder()
		{
			EncodeQueue = new Queue<Encode>();
		}

		public void AddAnimeToQueue(Encode e) //E
		{
			//We Add the anime to the queue.
			if (!CheckIfExists(e.Anime))
			{
				Program.Client.Logger.Log(LogLevel.Information, Events.QueueAdd, $"Added \"{e.Anime.FullFileName}\" to the main queue");
				EncodeQueue.Enqueue(e);
				SaveCurentQueueToFile(QueuePath);
				if (EncodeQueue.Count == 1)
				{
					Program.Client.Logger.Log(LogLevel.Debug, Events.Queue, "Main Loop Started, encodes pending = " + EncodeQueue.Count);
					Thread th = new Thread(new ThreadStart(() => { EncodeLoop(); }));
					th.Start();
				}
			}
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

		private void EncodeLoop()
		{
			while (EncodeQueue.Count > 0)
			{
				Encode e = EncodeQueue.Peek();
				WaitUntilNextEncode(e);
				e.Anime = HelperMethods.RemoveDuplicates(e.Anime);
				Encode(e.Anime);
				//Since the starting of some tasks depends on the size of the Queue.
				//We don't remove the element until the very end of this loop
				e = EncodeQueue.Dequeue();
				SaveCurentQueueToFile(QueuePath);
			}
		}

		private void WaitUntilNextEncode(Encode e)
		{
			while (DateTime.Now < e.EncodeDate)
			{
				Thread.Sleep(TimeSpan.FromSeconds(1));
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
				HandBrakeCLI.BeginOutputReadLine();

				//I know that commenting code is bad. HOWEVER I plan to use this on a next iteration of the code so here it stays
				/*DateTime lastedit = DateTime.Now;
				HandBrakeCLI.OutputDataReceived += async (object sender, DataReceivedEventArgs e) =>
				{
					if (!String.IsNullOrEmpty(e.Data))
					{
						//TODO increase timespan to 5 - 10 - 15 min ON RELEASE
						if (DateTime.Now - lastedit > TimeSpan.FromSeconds(5))
						{
							//Console.WriteLine($"Reccodificando archivo\n{preencode.Title} - {preencode.N_Ep}\n{e.Data}");
							lastedit = DateTime.Now;
						}
					}
				};*/
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
			if(File.Exists(path)){
				EncodeQueue =  JsonConvert.DeserializeObject<Queue<Encode>>(
					File.ReadAllText(path)
				);
			} 
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
