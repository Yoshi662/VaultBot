using System;

namespace VaultBot
{
	public class Encode
	{
		public ER_Anime Anime { get; set; }
		public DateTime EncodeDate { get; private set; }
		public Encode(ER_Anime anime, DateTime EncodeDate)
		{
			this.Anime = anime;
			this.EncodeDate = EncodeDate;
		}
	}
}
