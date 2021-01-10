using System;

namespace VaultBot
{
	public class Encode
	{
		public Anime Anime { get; set; }
		public DateTime EncodeDate { get; private set; }
		public Encode(Anime anime, DateTime EncodeDate)
		{
			this.Anime = anime;
			this.EncodeDate = EncodeDate;
		}
	}
}
