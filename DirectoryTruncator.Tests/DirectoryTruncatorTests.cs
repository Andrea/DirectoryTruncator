using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace DirectoryTruncator.Tests
{
	[TestFixture]
	public class DirectoryTruncatorTests
	{
		private string _testFolderPath;
		private DirectoryTruncator _directoryTruncator;

		[TestFixtureSetUp]
		public void FixtureInit()
		{
			string assemblyDir = Path.GetDirectoryName(
				Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", ""));
			_testFolderPath = Path.GetFullPath(Path.Combine(assemblyDir, @"..\..\TestFolder"));
			if (!Directory.Exists(_testFolderPath))
				Directory.CreateDirectory(_testFolderPath);

			foreach (var file in Directory.GetFiles(_testFolderPath))
				File.Delete(file);
		}

		[SetUp]
		public void SetUp()
		{
			_directoryTruncator = new DirectoryTruncator();
		}

		[TearDown]
		public void TearDown()
		{
			foreach (var file in Directory.GetFiles(_testFolderPath))
				File.Delete(file);
		}

		[Test]
		public void Can_truncate_a_directory_by_count_of_files_with_oldest_files_deleted_first()
		{
			const int expected = 2;
			CreateTestFiles(expected + 1);
			

			_directoryTruncator.TruncateByFileCount(_testFolderPath, expected);

			var files = new List<string>(Directory.GetFiles(_testFolderPath));
			files.Sort();

			string[] remainingFiles = files.ToArray();
			Assert.AreEqual(expected, remainingFiles.Length);

			Assert.AreEqual(remainingFiles, new[]
			                                     	{
			                                     		Path.Combine(_testFolderPath, "TestFile2.txt"), 
			                                     		Path.Combine(_testFolderPath, "TestFile3.txt")
			                                     	});
		}

		[Test]
		public void When_maxFiles_zero_Then_empty_target_directory()
		{
			const int MaxFiles = 0;

			const int TestFilesCount = 2;
			CreateTestFiles(TestFilesCount);

			Assert.AreEqual(TestFilesCount, Directory.GetFiles(_testFolderPath).Length);

			_directoryTruncator.TruncateByFileCount(_testFolderPath, MaxFiles);

			string[] remainingFiles = Directory.GetFiles(_testFolderPath);
			Assert.AreEqual(0, remainingFiles.Length);
		}

		[Test]
		public void Truncating_a_directory_has_no_effect_if_file_count_less_than_max_allowed()
		{
			const int MaxFiles = 3;
			const int expected = 2;
			CreateTestFiles(expected);

			_directoryTruncator.TruncateByFileCount(_testFolderPath, MaxFiles);

			var files = new List<string>(Directory.GetFiles(_testFolderPath));
			files.Sort();

			string[] remainingFiles = files.ToArray();
			Assert.AreEqual(expected, remainingFiles.Length);
			Assert.AreEqual(remainingFiles, new[]
			                                     	{
			                                     		Path.Combine(_testFolderPath, "TestFile1.txt"), 
			                                     		Path.Combine(_testFolderPath, "TestFile2.txt")
			                                     	});
		}

		[Test]
		public void Truncating_a_directory_has_no_effect_if_no_files_in_dir()
		{
			const int MaxFiles = 3;
			Assert.AreEqual(0, Directory.GetFiles(_testFolderPath).Length);

			_directoryTruncator.TruncateByFileCount(_testFolderPath, MaxFiles);

			string[] remainingFiles = Directory.GetFiles(_testFolderPath);
			Assert.AreEqual(0, remainingFiles.Length);
		}

		[Test]
		public void When_maxFiles_negative_Then_throws()
		{
			Assert.Throws<ArgumentException>(()=> _directoryTruncator.TruncateByFileCount(_testFolderPath, -1));
		}

		#region Private
		private void CreateTestFiles(int numberOfFiles)
		{
			for (int i = 0; i < numberOfFiles; i++)
			{
				string fileName = "TestFile" + (i + 1) + ".txt";
				using (StreamWriter writer = new StreamWriter(Path.Combine(_testFolderPath, fileName)))
				{
					writer.WriteLine("File " + i + " contents");
					writer.Flush();
				}
			}
			Assert.AreEqual(numberOfFiles, Directory.GetFiles(_testFolderPath).Length);
		}
		#endregion
	}

}
