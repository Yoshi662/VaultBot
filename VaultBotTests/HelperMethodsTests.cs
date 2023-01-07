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
	public class HelperMethodsTests
	{
		[TestMethod()]
		public void RemoveDuplicatesTest()
		{
			String testingMainPath = @"D:\Temp\VaultBotUnitTesting\";
			//[Erai-raws] The Legend of Unit Testing - 14 [v0][v2][1080p][Multiple Subtitle][1234ABCD].mkv
			string[] files = {
				/*@"[Erai-raws] The Legend of Unit Testing - 14 [1080p][1234ABCD].mkv",												//Standard
				@"[Erai-raws] The Legend of Unit Testing - 14 [v0][1080p][1234ABCD].mkv",											//V0
				@"[Erai-raws] The Legend of Unit Testing - 14 [v2][1080p][1234ABCD].mkv",											//V2
				@"[Erai-raws] The Legend of Unit Testing - 14 [1080p][Multiple Subtitle][1234ABCD].mkv",	*/						//Multi	
				//FINALES
				@"[Erai-raws] The Legend of Unit Testing - 14 END [1080p][1234ABCD].mkv",												//Standard
				@"[Erai-raws] The Legend of Unit Testing - 14 END [v0][1080p][1234ABCD].mkv",											//V0
				@"[Erai-raws] The Legend of Unit Testing - 14 END [v2][1080p][1234ABCD].mkv",											//V2
				@"[Erai-raws] The Legend of Unit Testing - 14 END [1080p][Multiple Subtitle][1234ABCD].mkv",							//Multi	
				@"[Erai-raws] Trigun Stampede - 01 [1080p][Multiple Subtitle][1234ABCD][1234ABCD].mkv"
				
			  /*@"[Erai-raws] The Legend of Unit Testing - 14 [v0][v2][1080p][Multiple Subtitle][1234ABCD].mkv"					//Multiple
				@"[Erai-raws] The Legend of Unit Testing - 14 [pre-enc][1080p][1234ABCD].mkv",												//Standard
				@"[Erai-raws] The Legend of Unit Testing - 14 [v0][pre-enc][1080p][1234ABCD].mkv",											//V0
				@"[Erai-raws] The Legend of Unit Testing - 14 [v2][pre-enc][1080p][1234ABCD].mkv",											//V2
				@"[Erai-raws] The Legend of Unit Testing - 14 [pre-enc][1080p][1234ABCD].mkv",												//Multi
				@"[Erai-raws] The Legend of Unit Testing - 14 [v0][v2][pre-enc][1080p][Multiple Subtitle][1234ABCD].mkv"	*/				//Finale
			 };

			ER_Anime anime = new ER_Anime(testingMainPath + files[0]);

			Directory.CreateDirectory(testingMainPath);
			foreach (string s in files)
			{
				var stream = File.CreateText(testingMainPath + s);
				stream.WriteLine("testfile");
				stream.Close();
			}

			anime = Utilities.RemoveDuplicates(anime);

			int filesthatexist = 0;

			for (int i = 0; i < files.Length; i++)
			{
				if (File.Exists(testingMainPath + files[i]))
				{
					filesthatexist++;
					Console.WriteLine(testingMainPath + files[i]);
				}
			}

			Assert.AreEqual(filesthatexist, 1);
			Assert.AreEqual(anime.HasMulti, true);

			Directory.Delete(testingMainPath, true);
		}
	}
}