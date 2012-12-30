using System.IO;

namespace DirectoryTruncator
{
	public interface IFileSystemWrapper
	{
		bool DirectoryExists(string directory);
		string[] DirectoryGetFiles(string path);
		void FileDelete(string path);
	}

	public class FileSystemWrapper : IFileSystemWrapper
	{
		public bool DirectoryExists(string directory)
		{
			return Directory.Exists(directory);
		}

		public string[] DirectoryGetFiles(string path)
		{
			return Directory.GetFiles(path);
		}

		public void FileDelete(string path)
		{
			File.Delete(path);
		}
	}

	
}