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
			get { return _fullPath; }
			set { _fullPath = value; }
		}
		private string _fullPath;

		/// <summary>
		/// It gets the File Name
		/// <para>Ex: "Itadaki!_Seieki_01_HMV.MKV"</para>
		/// </summary>
		public virtual string FullFileName
		{
			get { return Path.GetFileName(_fullPath); }
			set { _fullPath = FolderPath + "\\" +  value; }
		}


		/// <summary>
		/// It gets the Folder
		/// <para>Ex: "C:\Users\Yoshi\Homework\"</para>
		/// </summary>
		public virtual string FolderPath
		{
			get { return Path.GetDirectoryName(_fullPath); }
			set { _fullPath = value.TrimEnd(new[] { '/', '\\' }) + "\\" + FullFileName; }
		}
		

		public string Extension { get; set; }
		/// <summary>
		/// This Only checks if it has the .!Qb Extension
		/// </summary>
		public bool IsDownloading { get; set; }


		public Anime(string FullPath)
		{
			if (!Path.IsPathRooted(FullPath))
			{
				throw new ArgumentException("You have not provided a full (Rooted) path");
			}
			this.FullPath = FullPath;
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}
		public bool Exists() => File.Exists(this.FullPath);

		public override string ToString()
		{
			return base.ToString();
		}
	}
}
/*
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
			get { return FolderPath + "\\" + _fullFileName; }
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
			this.FolderPath = Path.GetDirectoryName(FullPath);
			this.IsDownloading = Path.GetExtension(FullPath).Equals(".!qB");
			this.Extension = Path.GetExtension(FullPath);
			this.FullFileName = Path.GetFileName(FullPath);
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}
		public bool Exists() => File.Exists(this.FullPath);

		public override string ToString()
		{
			return base.ToString();
		}
	}
}
*/