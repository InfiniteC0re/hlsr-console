using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace hlsr_console
{
	class ArgumentsParser
    {
		public string mode;

		// patch mode
		public string patchPath;
		public string extractPath;

		// game mode
		public string libraryPath;
		public string appID;
		public bool bxt = false;
		public bool ri = false;
		public bool livesplit = false;
		public string splitsDir = "";
		public bool steam = false;
		public bool dll = false;
		public bool useAllCores = false;
		public int coresCount = 0x0001;
		public ProcessPriorityClass priority = ProcessPriorityClass.Normal;

		public void ParseArgs(string[] args)
        {
			if (args.Length == 0) throw new Exception("No arguments");
			mode = args[0];

			switch (mode)
            {
				case "patch":
					if (args.Length == 3)
                    {
						patchPath = args[1];
						extractPath = args[2];
                    }
					break;
				case "game":
					if (args.Length >= 3)
					{
						libraryPath = args[1];
						appID = args[2];
					}
					break;
				default:
					throw new Exception("Unknown mode");
            }

			if (mode == "game")
            {
				for (int i = 0; i < args.Length; i++)
				{
					switch (args[i])
                    {
						case "-bxt":
							bxt = true;
							break;
						case "-ri":
							ri = true;
							break;
						case "-livesplit":
							livesplit = true;
							if (++i < args.Length) splitsDir = args[i];
							break;
						case "-steam":
							steam = true;
							break;
						case "-dll":
							dll = true;
							break;
						case "-allcores":
							useAllCores = true;
							break;
						case "-onecore":
							coresCount = 0x0001;
							break;
						case "-twocores":
							coresCount = 0x0003;
							break;
						case "-threecores":
							coresCount = 0x0007;
							break;
						case "-fourcores":
							coresCount = 0x000F;
							break;
						case "-normal":
							priority = ProcessPriorityClass.Normal;
							break;
						case "-abovenormal":
							priority = ProcessPriorityClass.AboveNormal;
							break;
						case "-high":
							priority = ProcessPriorityClass.High;
							break;
						case "-realtime":
							priority = ProcessPriorityClass.RealTime;
							break;
					}
				}
            }
        }
	}

	class Program
	{
		static void Main(string[] args)
		{
			ArgumentsParser parser = new ArgumentsParser();
			parser.ParseArgs(args);

			if (parser.mode == "patch")
			{
				string[] patchFiles = Directory.GetFiles(parser.patchPath);

				for (int i = 0; i < patchFiles.Length; i++)
					File.Copy(patchFiles[i], Path.Combine(parser.extractPath, patchFiles[i].Split('\\').Last()), true);
			}
			else if (parser.mode == "game")
			{
				bool revEmu = false;
				bool noEmulator = false;
				string startArguments = args.Last();

				Process proc;
				ProcessStartInfo processStartInfo;
				string ghostingDir = Path.Combine(parser.libraryPath, "Ghosting");
				string bxtPath = Path.Combine(parser.libraryPath, "Bunnymod XT", "Injector.exe");
				string rinputPath = Path.Combine(parser.libraryPath, "RInput", "RInput.exe");
				string livesplitPath = Path.Combine(parser.libraryPath, "LiveSplit", "LiveSplit.Register.exe");
				string livesplitSplitsDir = Path.Combine(parser.libraryPath, "LiveSplit", "Splits");
				string splitsDir = "";
				string targetProcessName;

				if (parser.bxt) parser.bxt = File.Exists(bxtPath);
				if (parser.ri) parser.ri = File.Exists(rinputPath);
				if (parser.livesplit) parser.livesplit = File.Exists(livesplitPath);

				if (parser.livesplit)
                {
					// check the splits file exists
					if (parser.splitsDir.EndsWith(".lss"))
						if (File.Exists(parser.splitsDir))
							splitsDir = parser.splitsDir;
					else splitsDir = "";

					if (string.IsNullOrEmpty(splitsDir))
                    {
						string splitsName;

						switch (parser.appID)
                        {
							case "50":
								splitsName = "Half-Life Opposing Force.lss";
								break;
							case "130":
								splitsName = "Half-Life Blue Shift.lss";
								break;
							case "218":
								splitsName = "Half-Life 2 - HL1Movement.lss";
								break;
							case "220":
								splitsName = "Half-Life 2.lss";
								break;
							default:
								splitsName = "Half-Life.lss";
								break;
						}

						splitsDir = Path.Combine(livesplitSplitsDir, splitsName);
					}
				}

				if (parser.appID == "220")
				{
					targetProcessName = "hl2.exe";
					processStartInfo = new ProcessStartInfo(Path.Combine(parser.libraryPath, "Half-Life 2", "hl2.exe"));
				}
				else if (parser.appID == "218")
				{
					targetProcessName = "hl2.exe";
					noEmulator = File.Exists(Path.Combine(ghostingDir, "NoEmulator"));
					
					// check if the installed Ghosting has SSE emulator or revEmu emulator
					if (!noEmulator) revEmu = File.Exists(Path.Combine(ghostingDir, "revLoader.exe"));

					if (noEmulator)
						processStartInfo = new ProcessStartInfo(Path.Combine(ghostingDir, "hl2.exe"));
					else if (revEmu)
						processStartInfo = new ProcessStartInfo(Path.Combine(ghostingDir, "revLoader.exe"));
					else
						processStartInfo = new ProcessStartInfo(Path.Combine(ghostingDir, "SSE", "SSE.exe"));
				}
				else
				{
					targetProcessName = "hl.exe";
					processStartInfo = new ProcessStartInfo(Path.Combine(parser.libraryPath, "Half-Life", "hl.exe"));
				}

				switch (parser.appID)
				{
					case "70":
						if (parser.steam) processStartInfo.Arguments = "-game valve";
						else if (parser.dll) processStartInfo.Arguments = "-game valve_WON_edited";
						else processStartInfo.Arguments = "-game valve_WON";
						break;
					case "50":
						processStartInfo.Arguments = parser.steam ? "-game gearbox" : "-game gearbox_WON";
						break;
					case "130":
						processStartInfo.Arguments = "-game bshift";
						break;
					case "220":
						processStartInfo.Arguments = "-game hl2 -novid -dxlevel 95";
						break;
				}

				if (!string.IsNullOrEmpty(processStartInfo.Arguments)) 
					processStartInfo.Arguments += " " + startArguments;
				else processStartInfo.Arguments = startArguments;

				if (targetProcessName == "hl.exe")
				{
					proc = Process.Start(processStartInfo);
				}
				else
				{
					bool sseEmu = !noEmulator && !revEmu;
					if (parser.appID == "218" && sseEmu)
					{
						System.IO.File.WriteAllText(Path.Combine(ghostingDir, "SSE", "SmartSteamEmu.ini"), $"[Launcher]\nTarget = ..\\hl2.exe\nCommandLine = {processStartInfo.Arguments}\nSteamClientPath = .\\SSE.dll\n\n[SmartSteamEmu]\nAppId = 218");
						processStartInfo.Arguments = "";
					}

					if (revEmu)
						processStartInfo.Arguments = Path.Combine(ghostingDir, "hl2.exe") + " " + processStartInfo.Arguments;

					DateTime startTime = DateTime.Now;
					proc = Process.Start(processStartInfo);

					if (parser.appID == "218" && (revEmu || !noEmulator))
					{
						while (Process.GetProcessesByName("hl2").Length == 0 && (DateTime.Now - startTime).TotalSeconds <= 6) { }

						Process[] processes = Process.GetProcessesByName("hl2");
						if (processes.Length > 0) proc = processes.First();
						else return;
					}
				}

				proc.PriorityClass = parser.priority;

				if (parser.useAllCores)
					proc.ProcessorAffinity = (IntPtr)((1 << Environment.ProcessorCount) - 1);
				else
					proc.ProcessorAffinity = (IntPtr)parser.coresCount;

				while (string.IsNullOrEmpty(proc.MainWindowTitle))
				{
					Thread.Sleep(100);
					proc.Refresh();
				}

				Thread.Sleep(1000);

				if (parser.bxt)
					Process.Start(new ProcessStartInfo(bxtPath));
				if (parser.ri)
					Process.Start(new ProcessStartInfo(rinputPath, targetProcessName));

				if (parser.livesplit)
				{
					Process.Start(new ProcessStartInfo(livesplitPath)).WaitForExit();
					Process.Start(splitsDir);
				}

				proc.WaitForExit();
			}
		}
	}
}
