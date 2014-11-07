﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Orange
{
	public struct FileInfo
	{
		public string Path;
		public DateTime LastWriteTime;
	}

	public class FileEnumerator
	{
		public string Directory { get; private set; }

		public Func<FileInfo, bool> EnumerationFilter;

		List<FileInfo> files = new List<FileInfo>();

		public FileEnumerator(string directory)
		{
			Directory = directory;
			Rescan();
		}

		public void Rescan()
		{
			files.Clear();
			var dirInfo = new System.IO.DirectoryInfo(Directory);

			foreach (var fileInfo in dirInfo.GetFiles("*.*", SearchOption.AllDirectories)) {
				var file = fileInfo.FullName;
				if (file.Contains(".svn"))
					continue;
				file = file.Remove(0, dirInfo.FullName.Length + 1);
				file = Lime.AssetPath.CorrectSlashes(file);
				files.Add(new FileInfo { Path = file, LastWriteTime = fileInfo.LastWriteTime });
			}
			PluginLoader.FilterFiles(files);
		}

		public List<FileInfo> Enumerate(string extension = null)
		{
			if (extension == null && EnumerationFilter == null) {
				return files;
			}
			List<FileInfo> result = new List<FileInfo>();
			foreach (FileInfo file in files) {
				if (extension != null && Path.GetExtension(file.Path) != extension)
					continue;
				if (EnumerationFilter != null && !EnumerationFilter(file))
					continue;
				result.Add(file);
			}
			return result;
		}
	}
}
