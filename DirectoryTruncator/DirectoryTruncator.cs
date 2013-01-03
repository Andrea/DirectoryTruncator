using System;
using System.IO;
using System.Linq;
using NLog;

namespace DirectoryTruncator
{
	public class DirectoryTruncator
	{
		private readonly string _targetDirectory;
		private static Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IFileSystemWrapper _fileSystemWrapper;

		public DirectoryTruncator(string targetDirectory, IFileSystemWrapper fileSystemWrapper)
		{
			if (string.IsNullOrWhiteSpace(targetDirectory))
				throw new ArgumentException();
			_targetDirectory = targetDirectory;
			
			_fileSystemWrapper = fileSystemWrapper ?? new FileSystemWrapper();
			if (!_fileSystemWrapper.DirectoryExists(targetDirectory))
				throw new ArgumentException("The target directory {0} does not exist", targetDirectory);
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

			var fileInfos = _fileSystemWrapper.DirectoryGetFiles(_targetDirectory)
							.Select(file => new FileInfo(file));

			var orderedFiles = fileInfos.OrderBy(fi => fi.CreationTimeUtc);

			int excess = orderedFiles.Count() - maxFiles;
			_logger.Trace("{0} files to be deleted", excess);
			if (excess <= 0)
			{
				_logger.Info("There was no files to delete");
				return;
			}

			orderedFiles.Take(excess).ToList().ForEach(y => TryDeleteFile(y.FullName));
		}

		public void TruncateByDirectory(int expected)
		{
			var directories = Directory.GetDirectories(_targetDirectory);
			var excess = directories.Length - expected;
			if (excess <= 0)
				return;
			var orderedDirectories = directories.Select(directory => new FileInfo(directory)).OrderBy(x => x.CreationTimeUtc);
			var fullNames = orderedDirectories.Select(x => Path.Combine(x.DirectoryName, x.Name)).ToArray();
			for (int i = 0; i < excess; i++)
			{
				Directory.Delete(fullNames[i], true);
				_logger.Info("Deleted directory {0}", fullNames[i]);
			}
		}

		private void TryDeleteFile(string fileName)
		{
			try
			{
				_fileSystemWrapper.FileDelete(fileName);
			}
			catch (FileNotFoundException fex)
			{
				_logger.Warn("The file that we were trying to delete, was not found.\n Full file name: {0} \n Details: {1}", fileName, fex.Message);

			}
			catch (DirectoryNotFoundException dex)
			{
				_logger.Warn("The directory for the file we were trying to delete was not found. \n Full file name: {0} \n Details {1}", fileName, dex.StackTrace);

			}
			catch (Exception ex)
			{
				_logger.Warn("There was a problem deleting the file {0}. \n Details {1}", fileName, ex.StackTrace);
			}
			finally
			{
				_logger.Info("Deleted {0}", fileName);
			}
		}
	}

}
