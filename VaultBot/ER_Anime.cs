﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VaultBot
{
	/// <summary>
	/// Helper Class to convert from a Erai Raws Episode to properties
	/// </summary>
	public class ER_Anime : Anime
	{
	/// <summary>
	/// This is the Regex used in file validation
	/// </summary>
		public static Regex TitleRegex { get; } = new Regex(@"(\[Erai\-raws\] )(.*)( - \d{1,3})( END)?( \[v0\])?( \[v2\])?( *\[1080p\])(\[pre-enc\])?(\[Multiple Subtitle\])?(\.mkv)?(\.!qB)?");
		public string Title { get; set; }
		public string N_Ep { get; set; }
		public bool HasMulti { get; set; }
		public bool IsFinale { get; set; }
		public bool IsV0 { get; set; }
		public bool IsV2 { get; set; }
		public bool PreEncode { get; set; }

		public ER_Anime(string FullPath) : base(FullPath)
		{
			if (!TitleRegex.IsMatch(FullPath))
			{
				throw new ArgumentException("The title sent does not match the Regex");
			} 
			
			GroupCollection matches = TitleRegex.Match(FullPath).Groups;
			Title = matches[2].Value.Trim();
			N_Ep = matches[3].Value.Substring(2).Trim();
			IsFinale = !string.IsNullOrWhiteSpace(matches[4].Value);
			IsV0 = !string.IsNullOrWhiteSpace(matches[5].Value);
			IsV2 = !string.IsNullOrWhiteSpace(matches[6].Value);
			//7 resolution *1080p in this case*
			PreEncode = !string.IsNullOrWhiteSpace(matches[8].Value);
			HasMulti = !String.IsNullOrWhiteSpace(matches[9].Value);
			Extension = matches[10].Value.Trim();
			IsDownloading = !String.IsNullOrWhiteSpace(matches[11].Value);
		}
		public override string ToString()
		{
			string output = "";
			output += $"[Erai-raws] {Title} - {N_Ep} ";
			if (IsFinale) output += @"END ";
			if (IsV0) output += @"[v0]";
			if (IsV2) output += @"[v2]";
			output += @"[1080p]";
			if (PreEncode) output += @"[pre-enc]";
			if (HasMulti) output += @"[Multiple Subtitle]";
			output += Extension;
			if (IsDownloading) output += @".!qB";
			return output;
		}
	}
}
