using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
		/// <summary>
		/// It gets the full Absolute path to the EP
		/// <para>Ex: "C:\Users\Yoshi\Homework\Itadaki!_Seieki_01_HMV.MKV"</para>
		/// </summary>
		public virtual string FullPath
		{
			get { return FolderPath + "\\" +_fullFileName; }
			set { _fullPath = value; }
		}
		private string _fullPath;

		/// <summary>
		/// It gets the File Name
		/// <para>Ex: "Itadaki!_Seieki_01_HMV.MKV"</para>
		/// </summary>
		public virtual string FullFileName
		{
			get { return _fullFileName; }
			set { _fullFileName = value; }
		}
		private string _fullFileName;

		/// <summary>
		/// It gets the Folder
		/// <para>Ex: "C:\Users\Yoshi\Homework\"</para>
		/// </summary>
		public virtual string FolderPath
		{
			get { return _folderPath; }
			set { _folderPath = value.TrimEnd(new[] { '/', '\\' }); }
		}
		private string _folderPath;

		public string Extension { get; set; }
		/// <summary>
		/// This Only checks if it has the .!Qb Extension
		/// </summary>
		public bool IsDownloading { get; set; }


		public Anime(string FullPath)
		{
			if (!Path.IsPathRooted(FullPath))
			{
				throw new ArgumentException("You have not provided a full path");
			}
			this.FullPath = FullPath;
			FolderPath = Path.GetDirectoryName(FullPath);
			IsDownloading = Path.GetExtension(FullPath).Equals(".!qB");
			Extension = Path.GetExtension(FullPath);
			FullFileName = Path.GetFileName(FullPath);
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}
		public bool Exists() => File.Exists(this.FullPath);
	}
}
