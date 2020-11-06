using System;
using System.Diagnostics;
using System.Linq;


namespace HLSR_Console
{
    class Program
    {
        static void Main(string[] args)
        {
			string libraryPath = args[0];
			string appID = args[1];
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

            if(args.Contains("-onecore"))
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
            {
                priority = ProcessPriorityClass.Normal;
            }
            else if (args.Contains("-abovenormal"))
            {
                priority = ProcessPriorityClass.AboveNormal;
            }
            else if (args.Contains("-high"))
            {
                priority = ProcessPriorityClass.High;
            }
            else if (args.Contains("-realtime"))
            {
                priority = ProcessPriorityClass.RealTime;
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo(libraryPath + "\\" + "Half-Life" + "\\" + "hl.exe");

			switch (appID)
			{
				case "70":
					if (steam)
					{
						processStartInfo.Arguments = "-game valve";
					}
					if (!steam)
					{
						processStartInfo.Arguments = "-game valve_WON";
					}
					if (!steam && dll)
					{
						processStartInfo.Arguments = "-game valve_WON_edited";
					}
					break;
				case "50":
					if (steam)
					{
						processStartInfo.Arguments = "-game gearbox";
					}
					if (!steam)
					{
						processStartInfo.Arguments = "-game gearbox_WON";
					}
					break;
				case "130":
					if (steam)
					{
						processStartInfo.Arguments = "-game bshift";
					}
					break;
			}

			processStartInfo.Arguments += " " + startArguments;

			Process hlProc = Process.Start(processStartInfo);

            if (true)
                hlProc.PriorityClass = priority;

            if (useAllCores)
                hlProc.ProcessorAffinity = (IntPtr)((1 << Environment.ProcessorCount) - 1);
            else
                hlProc.ProcessorAffinity = (IntPtr)coresCount;

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
				if (appID == "130")
				{
					splitsName = "Half-Life Blue Shift.lss";
				}
				Process.Start(libraryPath + "\\" + "LiveSplit" + "\\" + "Splits" + "\\" + splitsName);
			}
		}
    }
}
