using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Runtime.InteropServices;

namespace tcpfwdc
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				IPAddress ipLocal = null, ipRemote = null, ipProxy = null;
				int iLocalPort = 0, iRemotePort = 0, iProxyPort = 0;

				string sErrorArg = "";

				if (!GetIp(args[0], out ipLocal, out sErrorArg)) { }
				else if (!int.TryParse(args[1], out iLocalPort)) sErrorArg = args[1];
				else if (!GetIp(args[2], out ipRemote, out sErrorArg)) { }
				else if (!int.TryParse(args[3], out iRemotePort)) sErrorArg = args[3];
				else if (args.Length > 5 && !GetIp(args[4], out ipProxy, out sErrorArg)) { }
				else if (args.Length > 5 && !int.TryParse(args[5], out iProxyPort)) sErrorArg = args[5];

				if (sErrorArg.Length == 0)
				{
					IPEndPoint epLocal = new IPEndPoint(ipLocal, iLocalPort);
					IPEndPoint epRemote = new IPEndPoint(ipRemote, iRemotePort);
					IPEndPoint epProxy = (ipProxy == null || iProxyPort == 0) ? null : new IPEndPoint(ipProxy, iProxyPort);
					bool bDaemon = args.Contains("d", StringComparer.OrdinalIgnoreCase) || args.Contains("daemon", StringComparer.OrdinalIgnoreCase);
					bool bStartedInCmd = (Console.CursorLeft != 0) || (Console.CursorTop != 0);

					Exception exError = null;
					try { TcpFwd tf = new TcpFwd(epLocal, epRemote, epProxy); }
					catch (Exception ex1) { exError = ex1; }

					if (bDaemon && !bStartedInCmd)
						SetWindowState(enWinState.SW_HIDE);

					if (!bDaemon && exError != null)
						throw exError;

					while (true) Thread.Sleep(3600000);
				}
				else
				{
					Console.WriteLine();
					Console.WriteLine("Error parsing argument: " + sErrorArg);
					Console.WriteLine();
					ShowHelp();
				}
			}
			catch (Exception ex)
			{
				string sOut = "Execption: ";
				Exception exInner = ex;
				do
				{
					sOut = sOut + exInner.Message + "\n" + exInner.StackTrace + "\n";
					exInner = exInner.InnerException;
				} while (exInner != null);

				Console.WriteLine(sOut);
				ShowHelp();
			}
		}

		private static void ShowHelp()
		{
			Console.WriteLine("Usage: tcpfwdc.exe LocalIP LocalPort RemoteIP RemotePort [ProxyIP ProxyPort] [d, daemon]");
			Console.WriteLine();
			Console.WriteLine("E.g. 1: tcpfwdc.exe 127.0.0.1 5959 192.168.1.1 5900");
			Console.WriteLine("E.g. 2: tcpfwdc.exe 127.0.0.1 5959 192.168.1.1 5900 10.10.10.1 8080");
			Console.WriteLine("E.g. 3: tcpfwdc.exe 127.0.0.1 7777 192.168.1.1 7070 10.10.10.1 8080");
			Console.WriteLine();
			Console.WriteLine("Press any key to exit . . .");
			Console.ReadKey();
		}
		private static bool GetIp(string hostNameOrAddress, out IPAddress ip, out string errorMsg)
		{
			ip = null;
			errorMsg = "";

			try
			{
				bool bIsIp = IPAddress.TryParse(hostNameOrAddress, out ip);

				if (bIsIp)
				{
					return true;
				}
				else
				{
					IPAddress[] ipAll = Dns.GetHostAddresses(hostNameOrAddress);
					for (int i = 0; i < ipAll.Length; i++)
					{
						if (ipAll[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
						{
							ip = ipAll[i];
							return true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				errorMsg = hostNameOrAddress + ": "+ ex.Message;
			}

			return false;
		}

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetConsoleWindow();
		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		private enum enWinState
		{
			SW_HIDE = 0,
			SW_NORMAL = 1,
			SW_SHOWMINIMIZED = 2,
			SW_MAXIMIZE = 3,
			SW_SHOWNOACTIVATE = 4,
			SW_SHOW = 5,
			SW_MINIMIZE = 6,
			SW_SHOWMINNOACTIVE = 7,
			SW_SHOWNA = 8,
			SW_RESTORE = 9,
			SW_SHOWDEFAULT = 10,
			SW_FORCEMINIMIZE = 11,
		}
		private static void SetWindowState(enWinState en)
		{
			ShowWindow(GetConsoleWindow(), (int)en);
		}
	}
}
