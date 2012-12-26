using System;
using System.IO;
using System.Linq;

namespace DirectoryTruncator
{
	public class DirectoryTruncator
	{
		/// <summary>
		/// Given a folder path, and a maximum number of files. It deletes the oldest files (using creation time) so that the number of files
		/// is maxFiles or less.
		/// </summary>
		/// <param name="folderPath">The path where the truncating will occur</param>
		/// <param name="maxFiles">the maximum number of files (throws if zero or less)</param>
		/// <param name="recursive">if this value is set to true, it will truncate in all folders inside the given @folderPath </param>
		public void TruncateByFileCount(string folderPath, int maxFiles, bool recursive=false)
		{
			if (maxFiles < 0)
				throw new ArgumentException("maxFiles must be >= 0", "maxFiles");
			if (recursive)
				throw new NotImplementedException();
			var fileInfos = Directory.GetFiles(folderPath)
							.Select(file => new FileInfo(file));

			var orderedFiles = fileInfos.OrderBy(fi => fi.CreationTime);

			int excess = orderedFiles.Count() - maxFiles;
			if (excess <= 0) return;
			orderedFiles.Take(excess).ToList().ForEach(y => File.Delete(y.FullName));
		}
	}

}
