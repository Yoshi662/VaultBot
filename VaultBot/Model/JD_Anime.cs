using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VaultBot
{
	public class JD_Anime : Anime
	{

		/// <summary>
		/// This is the Regex used in file validation
		/// <para>Groups: Spam, Title, Season, Ep, Resolution, Encoding, SubsLanguage, ReleaseType</para>
		/// </summary>
		//public static Regex TitleRegex { get; } = new Regex(@"(?<Spam>\[Judas\]) (?<Title>.*) ((?<Season>\(.*\))|(?<Ep>(- S\d\dE\d\d)|(- \d+))) (?<Resolution>\[.*\])(?<Encoding>\[.*\])(?<SubsLanguage>(\[.*\])) (?<ReleaseType>\(.*\))(?<VideoExtension>\.mkv)?(?<IsDownloading>\.!qB)?");
		public static Regex TitleRegex { get; } = new Regex(@"(?<Spam>\[Judas\]) (?<Title>.*) - S(?<Season>\d+)E(?<Ep>\d+)(?<VideoExtension>\.\w{3,4})?(?<IsDownloading>\.!qB)?");

		public override string FullPath { get => base.FullPath; set => base.FullPath = value; }
		public override string FileName { get => base.FileName; set => base.FileName = value; }
		public override string FolderPath { get => base.FolderPath; set => base.FolderPath = value; }
		public override bool IsDownloading { get => base.IsDownloading; set => base.IsDownloading = value; }
		public override bool PreEncode { get => base.PreEncode; set => base.PreEncode = value; }
		public override string Extension { get => base.Extension; set => base.Extension = value; }

		public string Title { get; set; }
		public string N_Ep { get; set; }
		public string N_Season { get; set; }


		public JD_Anime(string FullPath) : base(FullPath)
		{
			if (!TitleRegex.IsMatch(FileName))
			{
				throw new ArgumentException("The title sent does not match the Regex");
			}
			GroupCollection matches = TitleRegex.Match(FullPath).Groups;
			Title = matches["Title"].Value.Trim();
			N_Ep = matches["Ep"].Value.Trim();
			N_Season = matches["Season"].Value.Trim();
			Extension = matches["VideoExtension"].Value.Trim();
		}
		public override string GetInfo()
		{
			if (Title is null && N_Ep is null) return FileName;
			else return Title + (int.Parse(N_Season) > 1 ? $" {N_Season}" : "") + " - " + N_Ep;
		}

		public override string ToString()
		{
			if (Title is null && N_Ep is null) return base.FileName;

			string output = $"[Judas] {Title} - S{N_Season}E{N_Ep}{Extension}{IsDownloading}";
			output += Extension;
			if (IsDownloading) output += dw_ext;

			return output;
		}

		/// <summary>
		/// It checks if Title AND Episode number Coincides
		/// </summary>
		/// <param name="input">The anime to compare</param>
		public bool Coincide(JD_Anime input)
		{
			return input.Title == this.Title && input.N_Ep == this.N_Ep;
		}
	}
}
