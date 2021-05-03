using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace hlsr_console
{
    class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0)
            {
				Console.WriteLine("Не было передано аргументов");
				return;
            }

			string mode = args[0];

			if (mode == "patch")
            {
				string patchPath = args[1];
				string extractPath = args[2];

				string[] patchFiles = Directory.GetFiles(patchPath);
                for (int i = 0; i < patchFiles.Length; i++)
                {
					string newPath = extractPath + "\\" + patchFiles[i].Split('\\').Last();
					File.Copy(patchFiles[i], newPath, true);
                }
            }
			else if (mode == "game")
            {
				string libraryPath = args[1];
				string appID = args[2];
				bool bxt = false;
				bool ri = false;
				bool livesplit = false;
				bool steam = false;
				bool dll = false;
				bool useAllCores = false;
				int coresCount = 0x0001;

				ProcessPriorityClass priority = ProcessPriorityClass.Normal;

				string startArguments = args.Last();

				if (args.Contains("-bxt"))
				{
					bxt = true;
				}
				if (args.Contains("-ri"))
				{
					ri = true;
				}
				if (args.Contains("-livesplit"))
				{
					livesplit = true;
				}
				if (args.Contains("-steam"))
				{
					steam = true;
				}
				if (args.Contains("-dll"))
				{
					dll = true;
				}
				if (args.Contains("-allcores"))
				{
					useAllCores = true;
				}

				if (args.Contains("-onecore"))
				{
					coresCount = 0x0001;
				}
				else if (args.Contains("-twocores"))
				{
					coresCount = 0x0003;
				}
				else if (args.Contains("-threecores"))
				{
					coresCount = 0x0007;
				}
				else if (args.Contains("-fourcores"))
				{
					coresCount = 0x000F;
				}

				if (args.Contains("-normal"))
					priority = ProcessPriorityClass.Normal;
				else if (args.Contains("-abovenormal"))
					priority = ProcessPriorityClass.AboveNormal;
				else if (args.Contains("-high"))
					priority = ProcessPriorityClass.High;
				else if (args.Contains("-realtime"))
					priority = ProcessPriorityClass.RealTime;

				ProcessStartInfo processStartInfo;

				if (appID == "220")
					processStartInfo = new ProcessStartInfo(libraryPath + "\\" + "Half-Life 2" + "\\" + "hl2.exe"); 
				else if (appID == "218")
					processStartInfo = new ProcessStartInfo(libraryPath + "\\" + "Ghosting" + "\\SSE\\" + "SSE.exe");
				else
					processStartInfo = new ProcessStartInfo(libraryPath + "\\" + "Half-Life" + "\\" + "hl.exe");

				switch (appID)
				{
					case "70":
						if (steam)
							processStartInfo.Arguments = "-game valve";
						if (!steam)
							processStartInfo.Arguments = "-game valve_WON";
						if (!steam && dll)
							processStartInfo.Arguments = "-game valve_WON_edited";
						break;
					case "50":
						if (steam)
							processStartInfo.Arguments = "-game gearbox";
						if (!steam)
							processStartInfo.Arguments = "-game gearbox_WON";
						break;
					case "130":
						processStartInfo.Arguments = "-game bshift";
						break;
					case "220":
						processStartInfo.Arguments = "-game hl2 -novid -dxlevel 95";
						break;
				}

				if (processStartInfo.Arguments.Length > 0)
					processStartInfo.Arguments += " " + startArguments;
				else
					processStartInfo.Arguments = startArguments;

				if (appID != "220" && appID != "218")
                {
					Process hlProc = Process.Start(processStartInfo);

					hlProc.PriorityClass = priority;

					if (useAllCores)
						hlProc.ProcessorAffinity = (IntPtr)((1 << Environment.ProcessorCount) - 1);
					else
						hlProc.ProcessorAffinity = (IntPtr)coresCount;

					while (string.IsNullOrEmpty(hlProc.MainWindowTitle))
					{
						Thread.Sleep(100);
						hlProc.Refresh();
					}

					Thread.Sleep(1000);

					if (bxt)
					{
						Process.Start(new ProcessStartInfo(libraryPath + "\\" + "Bunnymod XT" + "\\" + "Injector.exe"));
					}

					if (ri)
					{
						Process.Start(new ProcessStartInfo(libraryPath + "\\" + "RInput" + "\\" + "RInput.exe")
						{
							Arguments = "hl.exe"
						});
					}

					if (livesplit)
					{
						Process.Start(new ProcessStartInfo(libraryPath + "\\" + "LiveSplit" + "\\" + "LiveSplit.Register.exe")).WaitForExit();
						string splitsName = "Half-Life.lss";

						if (appID == "50")
						{
							splitsName = "Half-Life Opposing Force.lss";
						}
						else if (appID == "130")
						{
							splitsName = "Half-Life Blue Shift.lss";
						}
						Process.Start(libraryPath + "\\" + "LiveSplit" + "\\" + "Splits" + "\\" + splitsName);
					}

					hlProc.WaitForExit();
				}
				else
                {
					if (appID == "218")
                    {
						System.IO.File.WriteAllText(libraryPath + "\\" + "Ghosting" + "\\SSE\\SmartSteamEmu.ini",
$@"[Launcher]
Target = ..\hl2.exe
CommandLine = {processStartInfo.Arguments}
SteamClientPath = .\SSE.dll

[SmartSteamEmu]
AppId = 218
");
						processStartInfo.Arguments = "";
					}

					DateTime startTime = DateTime.Now;
					Process hl2Proc = Process.Start(processStartInfo);

					if (appID == "218")
					{
						while (Process.GetProcessesByName("hl2").Length == 0 && (DateTime.Now - startTime).TotalSeconds <= 5) { }

						Process[] processes = Process.GetProcessesByName("hl2");
						if (processes.Length > 0) hl2Proc = processes.First();
						else return;
					}

					hl2Proc.PriorityClass = priority;

					if (useAllCores)
						hl2Proc.ProcessorAffinity = (IntPtr)((1 << Environment.ProcessorCount) - 1);
					else
						hl2Proc.ProcessorAffinity = (IntPtr)coresCount;

					while (string.IsNullOrEmpty(hl2Proc.MainWindowTitle))
					{
						Thread.Sleep(100);
						hl2Proc.Refresh();
					}

					Thread.Sleep(1000);

					if (ri)
					{
						Process.Start(new ProcessStartInfo(libraryPath + "\\" + "RInput" + "\\" + "RInput.exe")
						{
							Arguments = "hl2.exe"
						});
					}

					if (livesplit)
					{
						Process.Start(new ProcessStartInfo(libraryPath + "\\" + "LiveSplit" + "\\" + "LiveSplit.Register.exe")).WaitForExit();
						
						if (appID == "220")
							Process.Start(libraryPath + "\\" + "LiveSplit" + "\\" + "Splits" + "\\Half-Life 2.lss");
						else if (appID == "218") 
							Process.Start(libraryPath + "\\" + "LiveSplit" + "\\" + "Splits" + "\\Half-Life 2 - HL1Movement.lss");
					}

					hl2Proc.WaitForExit();
				}
			}
		}
	}
}
