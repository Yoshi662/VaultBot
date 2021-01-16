﻿using System;

namespace VaultBot
{
	public class Encode
	{
		public Anime Anime { get; set; }
		public DateTime EncodeDate { get; set; }
		public Encode(Anime anime, DateTime EncodeDate)
		{
			this.Anime = anime;
			this.EncodeDate = EncodeDate;
		}
		/// <summary>
		/// Will create an appropiate Childen of Anime (ER_Anime or SP_Anime Depending on the input)
		/// </summary>
		/// <param name="fullpath">The full rooted path to the file</param>
		/// <param name="EncodeDate">The Encode date to the File</param>
		public Encode(String fullpath, DateTime EncodeDate)
		{
			this.EncodeDate = EncodeDate;

			if (ER_Anime.TitleRegex.IsMatch(fullpath))
			{
				this.Anime = new ER_Anime(fullpath);
			} else if (SP_Anime.TitleRegex.IsMatch(fullpath))
			{
				this.Anime = new SP_Anime(fullpath);
			} else {
				this.Anime = new Anime(fullpath);
			}

		}
	}
}