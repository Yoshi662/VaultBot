using Microsoft.VisualStudio.TestTools.UnitTesting;
using VaultBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VaultBot.Tests
{
	[TestClass()]
	public class ER_AnimeTests
	{
		[TestMethod()]
		public void ER_AnimeTest()
		{
			//Big test about files and if it saves and loads correctly

			String testingMainPath = @"D:\Temp\VaultBotUnitTesting\";
			//[Erai-raws] The Legend of Unit Testing - 14 [v0][v2][1080p][Multiple Subtitle].mkv
			string[] files = {
				@"[Erai-raws] BITCONEEEEEEEEEEEEEEEEEEEEEEEEEET - 03 END [1080p].mkv",
				@"[Erai-raws] BITCONEEEEEEEEEEEEEEEEEEEEEEEEEET - 03 [1080p].mkv",
				@"[Erai-raws] BITCONEEEEEEEEEEEEEEEEEEEEEEEEEET - 03 [1080p][Multiple Subtitle].mkv",
				@"[Erai-raws] BITCONEEEEEEEEEEEEEEEEEEEEEEEEEET - 03 [v0][1080p].mkv",
				@"[Erai-raws] BITCONEEEEEEEEEEEEEEEEEEEEEEEEEET - 03 [v2][1080p].mkv",
				@"[Erai-raws] CancerCells - 04 END [1080p].mkv",
				@"[Erai-raws] CancerCells - 04 [1080p].mkv",
				@"[Erai-raws] CancerCells - 04 [1080p][Multiple Subtitle].mkv",
				@"[Erai-raws] CancerCells - 04 [v0][1080p].mkv",
				@"[Erai-raws] CancerCells - 04 [v2][1080p].mkv",
				@"[Erai-raws] Markiplier points at things - 69 END [1080p].mkv",
				@"[Erai-raws] Markiplier points at things - 69 [1080p].mkv",
				@"[Erai-raws] Markiplier points at things - 69 [1080p][Multiple Subtitle].mkv",
				@"[Erai-raws] Markiplier points at things - 69 [v0][1080p].mkv",
				@"[Erai-raws] The Legend of the Ultimate Explosion - 01 END [1080p].mkv",
				@"[Erai-raws] The Legend of the Ultimate Explosion - 01 [1080p].mkv",
				@"[Erai-raws] The Legend of the Ultimate Explosion - 01 [1080p][Multiple Subtitle].mkv",
				@"[Erai-raws] The Legend of the Ultimate Explosion - 01 [v0][1080p].mkv",
				@"[Erai-raws] The Legend of the Ultimate Explosion - 01 [v2][1080p].mkv",
				@"[Erai-raws] ZeroFuks - 03 END [1080p].mkv",
				@"[Erai-raws] ZeroFuks - 03 [1080p].mkv",
				@"[Erai-raws] ZeroFuks - 03 [1080p][Multiple Subtitle].mkv",
				@"[Erai-raws] ZeroFuks - 03 [v0][1080p].mkv",
				@"[Erai-raws] ZeroFuks - 03 [v2][1080p].mkv",
			 };

			ER_Anime anime = new ER_Anime(testingMainPath + files[0]);

			Directory.CreateDirectory(testingMainPath);
			foreach (string s in files)
			{
				var stream = File.CreateText(testingMainPath + s);
				stream.WriteLine("testfile");
				stream.Close();
			}

			foreach (var item in files)
			{
				anime = new ER_Anime(testingMainPath + item);
				Assert.AreEqual(anime.FullPath, testingMainPath + item);
			}
			Directory.Delete(testingMainPath, true);


			//[Erai-raws] The Legend of Unit Testing - 14 [v0][v2][1080p][Multiple Subtitle].mkv
			anime = new ER_Anime(@"D:\Temp\VaultBotUnitTesting\[Erai-raws] The Legend Of Unit Testing - 03 [1080p].mkv");

			Assert.AreEqual(anime.FullPath, @"D:\Temp\VaultBotUnitTesting\[Erai-raws] The Legend Of Unit Testing - 03 [1080p].mkv");
			anime.FolderPath = @"D:\Temp";
			Assert.AreEqual(anime.FullPath, @"D:\Temp\[Erai-raws] The Legend Of Unit Testing - 03 [1080p].mkv");
			Assert.AreEqual(anime.FullFileName, @"[Erai-raws] The Legend Of Unit Testing - 03 [1080p].mkv");
			Assert.AreEqual(anime.FolderPath, @"D:\Temp");
			//We check  every fucking single thing posible

			Assert.AreEqual(anime.IsDownloading, false);
			Assert.AreEqual(anime.IsFinale, false);
			Assert.AreEqual(anime.IsV0, false);
			Assert.AreEqual(anime.IsV2, false);
			Assert.AreEqual(anime.PreEncode, false);
			Assert.AreEqual(anime.HasMulti, false);
			//We enable every fucking single thing posible
			anime.IsDownloading = true; anime.IsFinale = true; anime.IsV0 = true; anime.IsV2 = true; anime.PreEncode = true; anime.HasMulti = true;
			//We check  every fucking single thing posible
			Assert.AreEqual(anime.IsDownloading, true);
			Assert.AreEqual(anime.IsFinale, true);
			Assert.AreEqual(anime.IsV0, true);
			Assert.AreEqual(anime.IsV2, true);
			Assert.AreEqual(anime.PreEncode, true);
			Assert.AreEqual(anime.HasMulti, true);

			Assert.AreEqual(anime.FullPath, @"D:\Temp\[Erai-raws] The Legend Of Unit Testing - 03 END [v0][v2][1080p][pre-enc][Multiple Subtitle].mkv.!qB");
			Assert.AreEqual(anime.FullFileName, @"[Erai-raws] The Legend Of Unit Testing - 03 END [v0][v2][1080p][pre-enc][Multiple Subtitle].mkv.!qB");
			Assert.AreEqual(anime.FolderPath, @"D:\Temp");

			anime.Title = "I Want this shit to work so I can sleep more than 4 hours a day"; //This may not actually be true.

			Assert.AreEqual(anime.FullPath, @"D:\Temp\[Erai-raws] I Want this shit to work so I can sleep more than 4 hours a day - 03 END [v0][v2][1080p][pre-enc][Multiple Subtitle].mkv.!qB");
			Assert.AreEqual(anime.FullFileName, @"[Erai-raws] I Want this shit to work so I can sleep more than 4 hours a day - 03 END [v0][v2][1080p][pre-enc][Multiple Subtitle].mkv.!qB");
			Assert.AreEqual(anime.FolderPath, @"D:\Temp");
		}
	}
}