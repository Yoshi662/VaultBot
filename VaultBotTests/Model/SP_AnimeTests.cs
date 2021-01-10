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
	/*
	[SubsPlease] Hataraku Saibou Black - 01 (1080p) [0BA46656].mkv
	[SubsPlease] Yakusoku no Neverland S2 - 01 (1080p) [D1AA4F5C].mkv
	[SubsPlease] Re Zero kara Hajimeru Isekai Seikatsu - 39 (1080p) [26C6BE62].mkv
	[SubsPlease] Yuru Camp S2 - 01v2 (1080p) [8539B48E].mkv
	[SubsPlease] Yuru Camp S2 - 01v2 (1080p) [8539B48E][pre-enc].mkv
	*/
	[TestClass()]
	public class SP_AnimeTests
	{
		[TestMethod()]
		public void SP_AnimeTest()
		{
			//Big test about files and if it saves and loads correctly

			String testingMainPath = @"D:\Temp\VaultBotUnitTesting\";
			string[] files = {
				@"[SubsPlease] Hataraku Saibou Black - 01 (1080p) [0BA46656].mkv",
				@"[SubsPlease] Yakusoku no Neverland S2 - 01 (1080p) [D1AA4F5C].mkv",
				@"[SubsPlease] Re Zero kara Hajimeru Isekai Seikatsu - 39 (1080p) [26C6BE62].mkv",
				@"[SubsPlease] Yuru Camp S2 - 01v2 (1080p) [8539B48E].mkv",
				@"[SubsPlease] Yuru Camp S2 - 01v2 (1080p) [8539B48E][pre-enc].mkv"
			};

			SP_Anime anime = new SP_Anime(testingMainPath + files[0]);

			Directory.CreateDirectory(testingMainPath);
			foreach (string s in files)
			{
				var stream = File.CreateText(testingMainPath + s);
				stream.WriteLine("testfile");
				stream.Close();
			}

			foreach (var item in files)
			{
				anime = new SP_Anime(testingMainPath + item);

				Assert.AreEqual(anime.FullPath, testingMainPath + item);
			}
			Directory.Delete(testingMainPath, true);

			//[Erai-raws] The Legend of Unit Testing - 14 [v0][v2][1080p][Multiple Subtitle].mkv
			anime = new SP_Anime(@"D:\Temp\VaultBotUnitTesting\[SubsPlease] Yuru Camp S2 - 01v2 (1080p) [8539B48E][pre-enc].mkv");
			Assert.AreEqual(anime.FullPath, @"D:\Temp\VaultBotUnitTesting\[SubsPlease] Yuru Camp S2 - 01v2 (1080p) [8539B48E][pre-enc].mkv");
			anime.FolderPath = @"D:\Temp";
			Assert.AreEqual(anime.FullPath, @"D:\Temp\[SubsPlease] Yuru Camp S2 - 01v2 (1080p) [8539B48E][pre-enc].mkv");
			Assert.AreEqual(anime.FullFileName, @"[SubsPlease] Yuru Camp S2 - 01v2 (1080p) [8539B48E][pre-enc].mkv");
			Assert.AreEqual(anime.FolderPath, @"D:\Temp");

			Assert.AreEqual(anime.ImprovedVersion, "v2");
			Assert.AreEqual(anime.Hash, "[8539B48E]");
			Assert.AreEqual(anime.PreEncode, true);
			anime.ImprovedVersion = "v14";
			anime.Hash = "[00X00X00]";
			anime.PreEncode = false;
			Assert.AreEqual(anime.ImprovedVersion, "v14");
			Assert.AreEqual(anime.Hash, "[00X00X00]");
			Assert.AreEqual(anime.PreEncode, false);

			anime.Title = "UnitTesting is fun"; //Like a shot in the balls
			anime.N_Ep = "27";

			Assert.AreEqual(anime.FullPath, @"D:\Temp\[SubsPlease] UnitTesting is fun - 27v14 (1080p) [00X00X00].mkv");
			Assert.AreEqual(anime.FullFileName, @"[SubsPlease] UnitTesting is fun - 27v14 (1080p) [00X00X00].mkv");
			Assert.AreEqual(anime.FolderPath, @"D:\Temp");

		}
	}
}