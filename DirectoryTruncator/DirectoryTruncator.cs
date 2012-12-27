using System;
using System.IO;
using System.Linq;

namespace DirectoryTruncator
{
	public class DirectoryTruncator
	{
		private readonly string _targetDirectory;

		public DirectoryTruncator(string targetDirectory)
		{
			if (!Directory.Exists(targetDirectory))
				throw new ArgumentException("The target directory {0} does not exist", targetDirectory);
			_targetDirectory = targetDirectory;
		}

		/// <summary>
		/// Given a folder path, and a maximum number of files. It deletes the oldest files (using creation time) so that the number of files
		/// is maxFiles or less.
		/// </summary>
		/// <param name="folderPath">The path where the truncating will occur</param>
		/// <param name="maxFiles">the maximum number of files (throws if zero or less)</param>
		/// <param name="recursive">if this value is set to true, it will truncate in all folders inside the given @folderPath </param>
		public void TruncateByFileCount(int maxFiles, bool recursive = false)
		{
			if (maxFiles < 0)
				throw new ArgumentException("maxFiles must be >= 0", "maxFiles");
			if (recursive)
				throw new NotImplementedException();
			
			var fileInfos = Directory.GetFiles(_targetDirectory)
							.Select(file => new FileInfo(file));

			var orderedFiles = fileInfos.OrderBy(fi => fi.CreationTimeUtc);

			int excess = orderedFiles.Count() - maxFiles;
			if (excess <= 0)
				return;

			orderedFiles.Take(excess).ToList().ForEach(y => File.Delete(y.FullName));
		}


		public void TruncateByDirectory(int expected)
		{
			var directories = Directory.GetDirectories(_targetDirectory);
			var excess = directories.Length - expected;
			if (excess <= 0)
				return;
			var orderedDirectories = directories.Select(directory => new FileInfo(directory)).OrderBy(x => x.CreationTimeUtc);
			var array = orderedDirectories.Select(x => Path.Combine(x.DirectoryName, x.Name)).ToArray();
			for (int i = 0; i < excess; i++)
			{
				Directory.Delete(array[i], true);
			}
		}
	}

}
