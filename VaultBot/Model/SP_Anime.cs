using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VaultBot
{
	/// <summary>
	/// Helper Class to convert from a SubsPlease Episode to properties
	/// </summary>
	public class SP_Anime : Anime
	{

		//REGEX GROUPS
		//0: Spam shit -1: Title -2: Separator -3: Episode Number -4: Version? -5: Resolution -6: Hash -7: Preenc -8: Video extension -9: DownloadExtension
		/// <summary>
		/// This is the Regex used in file validation
		/// </summary>
		public static Regex TitleRegex { get; } = new Regex(@"(\[SubsPlease\] )(.*)( - )(\d*)(v\d)?( \(1080p\) )(\[\w{8}\])(\[pre-enc\])?(\.mkv)?(\.!qB)?");
		/// <summary>
		/// It gets the full Absolute path to the EP
		/// <para>Ex: "C:\Users\Yoshi\Homework\Itadaki!_Seieki_01_HMV.MKV"</para>
		/// </summary>
		public override string FullPath { get { return FolderPath + "\\" + this.ToString(); } }
		/// <summary>
		/// It gets the File Name
		/// <para>Ex: "Itadaki!_Seieki_01_HMV.MKV"</para>
		/// </summary>
		public override string FullFileName { get => this.ToString(); }
		public string Title { get; set; }
		public string N_Ep { get; set; }
		public override bool PreEncode { get; set; }
		public string ImprovedVersion { get; set; }
		public string Hash { get; set; }

		public SP_Anime(string fullpath) : base(fullpath)
		{
			if (!TitleRegex.IsMatch(FullFileName))
			{
				throw new ArgumentException("The title sent does not match the Regex");
			}

			GroupCollection matches = TitleRegex.Match(FullPath).Groups;
			this.Title = matches[2].Value.Trim();
			this.N_Ep = matches[4].Value.Trim();
			this.ImprovedVersion = matches[5].Value.Trim();
			this.Hash = matches[7].Value.Trim();
			this.PreEncode = !string.IsNullOrWhiteSpace(matches[8].Value);
			this.Extension = matches[9].Value.Trim();
			this.IsDownloading = !string.IsNullOrWhiteSpace(matches[10].Value);
		}

		public override string ToString()
		{
			if (Title is null && N_Ep is null) return base.FullFileName;

			return $"[SubsPlease] {Title} - {N_Ep}{ImprovedVersion} (1080p) {Hash}{(PreEncode ? "[pre-enc]" : "")}{Extension}{(IsDownloading ? dw_ext : "")}";
		}
		/// <summary>
		/// It checks if Title AND Episode number Coincides
		/// </summary>
		/// <param name="input">The anime to compare</param>
		public bool Coincide(SP_Anime input)
		{
			return input.Title == this.Title && input.N_Ep == this.N_Ep;
		}

		/// <summary>
		/// Gets the title and number of episode in a formatted way
		/// </summary>
		public override string GetInfo() => Title + " - " + N_Ep;
	}
}
