using System;
using DirectoryTruncator;
using Mono.Options;
using NLog;

namespace DirectoryTrucator.Console
{
	public class Program
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		static void Main(string[] args)
		{
			string targetDirectory = null;
			var count = 0;
			bool showHelp = false;
			bool directory = false;
			bool files = false;
			var optionSet = new OptionSet {
				{ "t|target=", "[Mandatory] Specify the output directory. Example  -t c:\\myOutputDirectory", v => targetDirectory = v },
				{ "d|directory=", "[Mandatory] Specify true if truncating directories. Example -d=true", v => bool.TryParse(v, out directory) },
				{ "f|files=", "[Mandatory] Specify true if truncating files inside target directory. Example -f=true", v => bool.TryParse(v, out files) },
				{ "c|count=", "[Mandatory] Specify the content file", v =>
					                                                {
						                                                if(!int.TryParse(v, out count))
																			Logger.Error("Count needs to be a number (int), not {0}", v);

					                                                } },
				{ "h|help",  "show this message and exit", v => showHelp = v != null },
			};

			try
			{
				optionSet.Parse(args);
			}
			catch (Exception exception)
			{
				Logger.Error("There was a problem {0}", exception.Message, exception);
				System.Console.WriteLine(exception.Message);
				System.Console.WriteLine("Try `DirectoryTruncator.Console --help' for more information.");
				return;
			}

			if (showHelp)
			{
				ShowHelp(optionSet);
				return;
			}
			try
			{
				var directoryTrucator = new DirectoryTruncator.DirectoryTruncator(targetDirectory, new FileSystemWrapper());
				if(directory)
					directoryTrucator.TruncateByDirectory(count);
				else if(files)
					directoryTrucator.TruncateByFileCount(count);
			}
			catch (Exception exception)
			{
				Logger.Error("There was an error trying to truncate directory {0}", exception.Message);
				System.Console.WriteLine("Try `DirectoryTruncator.Console --help' for more information.");
			}
			
		}

		static void ShowHelp(OptionSet p)
		{
			System.Console.WriteLine("Directory Trucator");
			System.Console.WriteLine("Trims a given directory to a set number of subdirectories based on it's creation time");
			System.Console.WriteLine("Older subdirectories will be deleted first");
			System.Console.WriteLine();
			System.Console.WriteLine("Options:");
			p.WriteOptionDescriptions(System.Console.Out);
		}
	}
}
