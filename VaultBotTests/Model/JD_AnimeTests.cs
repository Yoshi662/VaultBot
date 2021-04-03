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
	public class JD_AnimeTests
	{
		[TestMethod()]
		public void JD_AnimeTest()
		{
			//Big test about files and if it saves and loads correctly

			String testingMainPath = @"D:\Temp\VaultBotUnitTesting\";
			//[Erai-raws] The Legend of Unit Testing - 14 [v0][v2][1080p][Multiple Subtitle].mkv
			string[] files = {
				@"[Judas] The Legend of Unit Testing - S02E69.mkv"
			};


			JD_Anime anime = new JD_Anime(testingMainPath + files[0]);

			Directory.CreateDirectory(testingMainPath);
			foreach (string s in files)
			{
				var stream = File.CreateText(testingMainPath + s);
				stream.WriteLine("testfile");
				stream.Close();
			}

			foreach (var item in files)
			{
				anime = new JD_Anime(testingMainPath + item);
				Assert.AreEqual(anime.FullPath, testingMainPath + item);
			}
			Directory.Delete(testingMainPath, true);
		}
		
	}
}