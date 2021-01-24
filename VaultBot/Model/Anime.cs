﻿using Newtonsoft.Json;
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
		public const string dw_ext = ".!qB";
		public const string preEncodePrefix = "preencode_";

		private string _fullPath;

		/// <summary>
		/// It gets the full Absolute path to the EP
		/// <para>Ex: "C:\Users\Yoshi\Homework\Itadaki!_Seieki_01_HMV.MKV"</para>
		/// </summary>
		public virtual string FullPath
		{
			get { return _fullPath; }
			set { _fullPath = value; }
		}
		

		/// <summary>
		/// It gets the File Name
		/// <para>Ex: "Itadaki!_Seieki_01_HMV.MKV"</para>
		/// </summary>
		public virtual string FileName
		{
			get { return Path.GetFileName(_fullPath); }
			set { _fullPath = FolderPath + "\\" + value; }
		}


		/// <summary>
		/// It gets the Folder
		/// <para>Ex: "C:\Users\Yoshi\Homework\"</para>
		/// </summary>
		public virtual string FolderPath
		{
			get { return Path.GetDirectoryName(_fullPath); }
			set { _fullPath = value.TrimEnd(new[] { '/', '\\' }) + "\\" + FileName; }
		}


		public string Extension
		{
			get { return Path.GetExtension(_fullPath); }
			set { _fullPath = Path.ChangeExtension(_fullPath, value); }
		}

		/// <summary>
		/// This Only checks if it has the .!Qb Extension
		/// </summary>
		public virtual bool IsDownloading
		{
			get { return Path.GetExtension(_fullPath).Equals(dw_ext); }
			set
			{
				bool IsAlreadydownloading = _fullPath.Contains(dw_ext);
				if (value && !IsAlreadydownloading)
				{
					_fullPath += dw_ext;
				}
				if (!value && IsAlreadydownloading)
				{
					_fullPath.Remove(_fullPath.IndexOf(dw_ext), dw_ext.Length);
				}
			}
		}

		public virtual bool PreEncode
		{
			get { return FileName.StartsWith(preEncodePrefix);  }
			set
			{
			bool IsAlreadyaPreEncode = FileName.StartsWith(preEncodePrefix);
				if (value && !IsAlreadyaPreEncode)
				{
					FileName = preEncodePrefix + FileName;
				}
				if (!value && IsAlreadyaPreEncode)
				{
					FileName.Remove(_fullPath.IndexOf(preEncodePrefix), preEncodePrefix.Length);
				}
			}
		}


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


		/// <summary>
		/// Gets the Full Path
		/// </summary>
		public virtual string GetInfo() => FileName;
	}
}