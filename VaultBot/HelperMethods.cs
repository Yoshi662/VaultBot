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
		/// <param name="thumbnail">Añade el Thumbnail por defecto</param>
		/// <param name="color">Cadena Hexadecimal para el color del embed</param>
		/// <param name="footerspam">Habilita el footerSpam "A Yoshi's bot"</param>
		/// <returns></returns>
		public static DiscordEmbed QuickEmbed(string titulo = "", string descripcion = "", bool thumbnail = true, bool footerspam = true, string color = "#2461DC")
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
			if (thumbnail) builder.WithThumbnail(@"https://i.imgur.com/DxT09uJ.png");
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

		public static MemoryStream stringToMemoryStream(string input)
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
				if ((Proposal.FileName != item.FileName) && item.Exists())
				{
					File.Delete(item.FullPath);
				}
			}

			//We keep some properties from the original property
			Standard.HasMulti = Proposal.HasMulti;
			Standard.IsFinale = Proposal.IsFinale;
			Standard.PreEncode = Proposal.PreEncode;

			//We rename the file so we get rid of any unwanted tag.
			if (Proposal.Exists())
			{
				File.Move(Proposal.FullPath, Standard.FullPath);
			}
			return Proposal;
		}

		public static AnimeType GetAnimeType(Anime a)
		{
			switch (a.GetType().Name)
			{
				case "Anime":
					return AnimeType.Anime;
				case "ER_Anime":
					return AnimeType.ER_Anime;
				case "SP_Anime":
					return AnimeType.SP_Anime;
				default:
					throw new ArgumentOutOfRangeException($"{a.GetType().Name} - {a.GetInfo()}", "Uno de los items de la cola no es de ningun tipo Anime o Tipo heredado");
			}
		}

		public static AnimeType GetAnimeType(String fullpath)
		{
			if (ER_Anime.TitleRegex.IsMatch(fullpath))
			{
				return AnimeType.ER_Anime;
			} else if (SP_Anime.TitleRegex.IsMatch(fullpath))
			{
				return AnimeType.SP_Anime;
			} else {
				return AnimeType.Anime;
			}
		}
	}
}
