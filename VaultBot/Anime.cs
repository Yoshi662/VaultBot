using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaultBot
{
	public class Anime : ICloneable
	{
		public string Extension { get; set; }
		/// <summary>
		/// This Only checks if it has the .!Qb Extension
		/// </summary>
		public bool IsDownloading { get; set; }
		protected string FolderPath { get; set; }
		/// <summary>
		/// It gets the full Absolute path to the EP
		/// <para>Ex: "C:\Users\Yoshi\Homework\Itadaki!_Seieki_01_HMV.MKV"</para>
		/// </summary>
		public string FullPath { get { return FolderPath + "\\" + this.ToString(); } }
		/// <summary>
		/// It gets the File Name
		/// <para>Ex: "Itadaki!_Seieki_01_HMV.MKV"</para>
		/// </summary>
		public string FullFileName { get { return this.ToString(); } }

		public Anime(string FullPath)
		{
			if (!Path.IsPathRooted(FullPath))
			{
				throw new ArgumentException("You have not provided a full path");
			}
			FolderPath = Path.GetDirectoryName(FullPath);
			IsDownloading = Path.GetExtension(FullPath).Equals(".!qB");
			Extension = Path.GetExtension(FullPath);
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}
		public bool Exists() => File.Exists(this.FullPath);
	}
}
