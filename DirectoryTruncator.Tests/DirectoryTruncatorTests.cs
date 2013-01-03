using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Moq;
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
			_directoryTruncator = new DirectoryTruncator(_testFolderPath, new FileSystemWrapper());
		}

		[TearDown]
		public void TearDown()
		{
			foreach (var file in Directory.GetFiles(_testFolderPath))
				File.Delete(file);
			foreach (var directory in Directory.GetDirectories(_testFolderPath))
			{
				Directory.Delete(directory, true);
			}
		}

		[Test]
		public void When_target_directory_doesnt_exist_Then_throw()
		{
			Assert.Throws<ArgumentException>(() => new DirectoryTruncator(_testFolderPath + "sads", new FileSystemWrapper()));
		}

		[Test]
		public void Can_truncate_a_directory_by_count_of_files_with_oldest_files_deleted_first()
		{
			const int expected = 2;
			CreateTestFiles(expected + 1);

			_directoryTruncator.TruncateByFileCount(expected);

			var files = Directory.GetFiles(_testFolderPath);

			AssertDirectoryContainsNumberOfFiles(expected);
			Assert.AreEqual(new[]
			                                     	{
			                                     		Path.Combine(_testFolderPath, "TestFile2.txt"), 
			                                     		Path.Combine(_testFolderPath, "TestFile3.txt")
			                                     	}, files);
		}

		[Test]
		public void When_maxFiles_zero_Then_empty_target_directory()
		{
			const int maxFiles = 0;
			const int testFilesCount = 2;
			CreateTestFiles(testFilesCount);

			_directoryTruncator.TruncateByFileCount(maxFiles);

			AssertDirectoryContainsNumberOfFiles(0);

		}

		[Test]
		public void Truncating_a_directory_has_no_effect_if_file_count_less_than_max_allowed()
		{
			const int maxFiles = 3;
			const int expected = 2;
			CreateTestFiles(expected);

			_directoryTruncator.TruncateByFileCount(maxFiles);

			var files = Directory.GetFiles(_testFolderPath);

			AssertDirectoryContainsNumberOfFiles(expected);
			Assert.AreEqual(new[]{
			                            Path.Combine(_testFolderPath, "TestFile1.txt"), 
			                            Path.Combine(_testFolderPath, "TestFile2.txt")
			                        }, files);
		}

		[Test]
		public void Truncating_has_no_effect_if_no_files_in_directory()
		{
			const int MaxFiles = 3;
			Assert.AreEqual(0, Directory.GetFiles(_testFolderPath).Length);

			_directoryTruncator.TruncateByFileCount(MaxFiles);

			AssertDirectoryContainsNumberOfFiles(0);
		}

		[Test]
		public void When_maxFiles_negative_Then_throws()
		{
			Assert.Throws<ArgumentException>(() => _directoryTruncator.TruncateByFileCount(-1));
		}

		[Test]
		public void When_subdirectories_within_subdirectories_Then_only_top_level_afects_count()
		{
			var numberOfDirectories = 3;
			CreateTestDirectories(numberOfDirectories+1, true);

			_directoryTruncator.TruncateByDirectory(numberOfDirectories);

			Assert.AreEqual(numberOfDirectories, Directory.GetDirectories(_testFolderPath).Length);
			
		}

		[Test]
		public void When_error_on_delete_file_Then_does_not_throw()
		{
			var fileSystemWrapperMock = CreateAndSetFileSystemWrapperMock(3);
			var directoryTruncator = new DirectoryTruncator(_testFolderPath, fileSystemWrapperMock.Object);
			fileSystemWrapperMock.Setup(x => x.FileDelete(It.IsAny<string>())).Throws(new FileNotFoundException());

			Assert.DoesNotThrow(() => directoryTruncator.TruncateByFileCount(1));
		}

		[Test]
		public void When_directory_error_Then_does_not_throw()
		{
			var fileSystemWrapperMock = CreateAndSetFileSystemWrapperMock(3);
			var directoryTruncator = new DirectoryTruncator(_testFolderPath, fileSystemWrapperMock.Object);
			fileSystemWrapperMock.Setup(x => x.FileDelete(It.IsAny<string>())).Throws(new DirectoryNotFoundException());

			Assert.DoesNotThrow(() => directoryTruncator.TruncateByFileCount(1));
		}

		[Test]
		public void When_4_subdirectories_and_max3_Then_deletes_one_directory()
		{
			const int expected = 3;
			CreateTestDirectories(expected + 1, false);

			_directoryTruncator.TruncateByDirectory(expected);

			Assert.AreEqual(expected, Directory.GetDirectories(_testFolderPath).Length);
		}

		[Test]
		public void When_subdirectories_have_subdirectories_Then_delete_subdirectory_anyway()
		{
			const int expected = 3;
			CreateTestDirectories(expected + 1, true);

			_directoryTruncator.TruncateByDirectory(expected);

			Assert.AreEqual(expected, Directory.GetDirectories(_testFolderPath).Length);
		}

		#region Private

		private Mock<IFileSystemWrapper> CreateAndSetFileSystemWrapperMock(int numberOfFiles)
		{
			CreateTestFiles(numberOfFiles);
			var fileSystemWrapperMock = new Mock<IFileSystemWrapper>();
			fileSystemWrapperMock.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
			var strings = new List<string>();
			for (int i = 1; i <= numberOfFiles; i++)
			{
				strings.Add(Path.Combine(_testFolderPath, "TestFile"+i+".txt"));
			}
			fileSystemWrapperMock.Setup(x => x.DirectoryGetFiles(It.IsAny<string>())).Returns(strings.ToArray);
			return fileSystemWrapperMock;
		}

		private void AssertDirectoryContainsNumberOfFiles(int expexted)
		{
			var remainingFiles = Directory.GetFiles(_testFolderPath);
			Assert.AreEqual(expexted, remainingFiles.Length);
		}

		private void CreateTestDirectories(int numberOfDirectories, bool hasSubdirectories)
		{
			for (int i = 0; i < numberOfDirectories; i++)
			{
				string directoryName = Path.Combine(_testFolderPath, "TestDirectory" + (i + 1));
				var directoryInfo = Directory.CreateDirectory(directoryName);
				if (hasSubdirectories)
				{
					directoryInfo.CreateSubdirectory("TestDirecotry");
				}

			}
			Assert.AreEqual(numberOfDirectories, Directory.GetDirectories(_testFolderPath).Length);
		}

		private void CreateTestFiles(int numberOfFiles)
		{
			for (int i = 0; i < numberOfFiles; i++)
			{
				string fileName = "TestFile" + (i + 1) + ".txt";
				File.AppendAllText(Path.Combine(_testFolderPath, fileName), "File " + i + " contents");
			}
			Assert.AreEqual(numberOfFiles, Directory.GetFiles(_testFolderPath).Length);
		}
		#endregion
	}

}
