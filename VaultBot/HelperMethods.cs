using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaultBot
{
	public static class HelperMethods
	{
		/// <summary>
		/// Genera un DiscordEmbed basico
		/// </summary>
		/// <param name="titulo">Titulo del embed</param>
		/// <param name="descripcion">Descripcion del embed</param>
		/// <param name="color">Cadena Hexadecimal para el color del embed</param>
		/// <param name="footerspam">Habilita el footerSpam "A Yoshi's bot"</param>
		/// <returns></returns>
		public static DiscordEmbed QuickEmbed(String titulo = "", string descripcion = "", bool footerspam = true, string color = "#2461DC")
		{
			DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
			builder.WithTitle(titulo)
			.WithDescription(descripcion)
			.WithColor(new DiscordColor(color));
			if (footerspam)
			{
				builder.WithFooter(
				"A Yoshi's Bot",
				"https://i.imgur.com/rT9YocG.jpg"
				);
			}
			return builder.Build();
		}

		public static DiscordEmbed DiscordSpamEmbed
		{
			get
			{
				DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
				builder.WithFooter(
				"A Yoshi's Bot",
				"https://i.imgur.com/rT9YocG.jpg"
				);
				return builder.Build();
			}
		}

		public static MemoryStream StringToMemoryStream(String input)
		{
			return new MemoryStream(Encoding.UTF8.GetBytes(input));
		}

		/// <summary>
		/// From this ER_Anime, checks another versions of it and removes all the previous versions
		/// </summary>
		/// <returns>The final</returns>
		public static ER_Anime RemoveDuplicates(ER_Anime anime)
		{
			ER_Anime Proposal = anime;

			//We create the possible versions of the anime
			ER_Anime Standard = (ER_Anime)anime.Clone();
			Standard.IsV0 = false; Standard.IsV2 = false; Standard.HasMulti = false;
			ER_Anime V0 = (ER_Anime)Standard.Clone();
			V0.IsV0 = true;
			ER_Anime V2 = (ER_Anime)Standard.Clone();
			V2.IsV2 = true;
			ER_Anime Multi = (ER_Anime)Standard.Clone();
			Multi.HasMulti = true;

			//We check if a better version of the release exists and assign it to proposal (So Multi is better than V2 etc...)
			ER_Anime[] releases = { V0, Standard, V2, Multi };
			foreach (ER_Anime item in releases)
			{
				if (item.Exists())
				{
					Proposal = (ER_Anime)item.Clone();
				}
			}
			//We delete every unwanted file
			foreach (ER_Anime item in releases)
			{
				if ((Proposal.FullFileName != item.FullFileName) && item.Exists())
				{
					File.Delete(item.FullPath);
				}
			}

			//We keep some properties from the original property
			Standard.HasMulti = Proposal.HasMulti;
			Standard.IsFinale = Proposal.IsFinale;
			Standard.PreEncode = Proposal.PreEncode; 

			//We rename the file so we get rid of any unwanted tag.
			File.Move(Proposal.FullPath, Standard.FullPath);

			return Proposal;
		}
	}
}
