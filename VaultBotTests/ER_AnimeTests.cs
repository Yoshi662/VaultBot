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
		}
	}
}