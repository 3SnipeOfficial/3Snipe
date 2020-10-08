using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;

namespace _3Snipe
{
	class Program
	{
		static readonly string vCode = "v2.0.0-beta.7 hotfix 2";
		
		static void Main(string[] args)
		{
			branding();
			menu();
		}
		static void branding()
		{
			Console.Title = "3Snipe";
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine($@"                                                 
██████╗ ███████╗███╗   ██╗██╗██████╗ ███████╗
╚════██╗██╔════╝████╗  ██║██║██╔══██╗██╔════╝
 █████╔╝███████╗██╔██╗ ██║██║██████╔╝█████╗  
 ╚═══██╗╚════██║██║╚██╗██║██║██╔═══╝ ██╔══╝  
██████╔╝███████║██║ ╚████║██║██║     ███████╗
╚═════╝ ╚══════╝╚═╝  ╚═══╝╚═╝╚═╝     ╚══════╝

{vCode}

");
			Console.ResetColor();
		}
		private static void Snipe()
		{
			Console.Write("\r \r");
			for (; ; )
			{
				
				Console.WriteLine(@"Sniping Menu:
1) Snipe (multi-account or single)
2) Block (multi-account or single) (Disabled)
3) Back
");
				char option = Console.ReadKey().KeyChar;
				char[] options = { '1', '2', '3' };
				while (!options.Contains(option))
				{
					Console.Write("\r \r");
					option = Console.ReadKey().KeyChar;
				}
				switch (option)
				{
					case '1': SniperClass.mutliAcctSnipe(); break;
					case '2': break;
					case '3': return;
					default: break;
				}
			}
		}
		
		
		private static void configure()
		{
			
			Console.WriteLine("Under construction. Press a key to go back.");
			Console.ReadKey();
			
			return;
		}
		private static void tools()
		{
			Console.Write("\r \r");
			for (; ; )
			{
				
				Console.WriteLine(@"Tools:
1) Do Security Questions
2) Load proxies (Disabled)
3) Back
");
				char option = Console.ReadKey().KeyChar;
				char[] options = { '1', '2', '3' };
				while (!options.Contains(option))
				{
					Console.Write("\r \r");
					option = Console.ReadKey().KeyChar;
				}
				switch (option)
				{
					case '1': doQuestions(); break;
					case '2': break;
					case '3': return;
					default: break;
				}
			}
		}
		
		private static void doQuestions()
		{
			/*
             * The flow of security questions is as follows:
             * GET https://api.mojang.com/user/security/challenges
             * ||
             * ||
             * \/
             * POST https://api.mojang.com/user/security/location
             */

			

			WebClient sniperClient = new WebClient();
			string email = "";
			string password = "";
			try
			{
				string account = "";
				ConsoleKeyInfo key;
				Console.WriteLine("Enter your Minecraft account in the format of 'email:password' and press enter: ");
				do
				{
					key = Console.ReadKey(true);
					if (key.Key != ConsoleKey.Backspace)
					{
						account += key.KeyChar;
						Console.Write("*");
					}
					else
					{
						Console.Write("\b \b");
					}
				}
				while (key.Key != ConsoleKey.Enter);
				account = account.Substring(0, account.Length - 1);

				email = account.Split(':')[0];
				password = account.Split(':')[1];
			}
			catch
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("[Error] Invalid input. Press any key to return to the menu.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}
			string accessToken = "";
			try
			{
				string tokenResponse = sniperClient.UploadString("https://authserver.mojang.com/authenticate", $"{{\"agent\": {{\"name\": \"Minecraft\", \"version\": 1}},\"username\": \"{email}\", \"password\": \"{password}\"}}");
				accessToken = (string)JObject.Parse(tokenResponse)["accessToken"];
			}
			catch
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("[Error] The account provided is invalid. Press any key to return to the menu.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}
			
			string f16 = accessToken.Substring(0, 16);
			Console.WriteLine($"[Info] Got token. First 16 characters are {f16}");
			sniperClient.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {accessToken}");
			try
			{
				string tempqs = sniperClient.DownloadString("https://api.mojang.com/user/security/challenges");
				if (tempqs == "[]")
				{
					Console.WriteLine("[Info] Questions not needed. Press any key to return to the menu.");
					Console.ReadKey();
					return;
				}
			}
			catch { }
			Console.WriteLine("[Info] Getting questions now.");
			List<string> questions = new List<string>();
			List<int> ids = new List<int>();
			try
			{
				string tempqs = sniperClient.DownloadString("https://api.mojang.com/user/security/challenges");
				JArray qarr = JArray.Parse(tempqs);
				for (int i = 0; i < 3; i++)
				{
					questions.Add((string)qarr[i]["question"]["question"]);
					ids.Add((int)qarr[i]["answer"]["id"]);
				}
			}
			catch
			{
				try
				{
					string tempqs = sniperClient.DownloadString("https://api.mojang.com/user/security/challenges");
					if (tempqs == "[]")
					{
						Console.WriteLine("[Info] Questions not needed. Press any key to return to the menu.");
						Console.ReadKey();
						return;
					}
					else { throw new Exception("Questions needed, but response invalid."); }
				}
				catch
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("[Error] Questions failed to fetch. Press any key to return to the menu.");
					Console.ResetColor();
					Console.ReadKey();
					return;
				}
			}
			Console.WriteLine("[Info] Questions starting. Respond with the EXACT answers.");
			Console.WriteLine($"[Question] {questions[0]}");
			string a1 = Console.ReadLine();
			Console.WriteLine($"[Question] {questions[1]}");
			string a2 = Console.ReadLine();
			Console.WriteLine($"[Question] {questions[2]}");
			string a3 = Console.ReadLine();
			Console.WriteLine("[Info] Sending responses.");
			try
			{
				sniperClient.UploadString("https://api.mojang.com/user/security/location", $"[{{\"id\": {ids[0]}, \"answer\": \"{a1}\"}}, {{\"id\": {ids[1]}, \"answer\": \"{a2}\"}}, {{\"id\": {ids[2]}, \"answer\": \"{a3}\"}}]");
				Console.WriteLine("[Info] Questions answered successfully. Press any key to return to the menu.");
				Console.ReadKey();
			}
			catch
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("[Error] At least one answer was wrong. Press any key to return to the menu.");
				Console.ResetColor();
				Console.ReadKey();
			}
		}
		private static void menu()
		{
			for (; ; )
			{
				Console.WriteLine(@"
Menu:
1) Snipe name
2) Settings (Disabled)
3) Tools
4) Exit
");
				char option = Console.ReadKey().KeyChar;
				char[] options = { '1', '2', '3', '4' };
				while (!options.Contains(option))
				{
					Console.Write("\r \r");
					option = Console.ReadKey().KeyChar;
				}
				switch (option)
				{
					case '1': Snipe(); break;
					case '2': break;
					case '3': tools(); break;
					case '4':  System.Environment.Exit(0); break;
					default: break;
				}
			}
		}
		public static void verify(string name)
		{
			
			Console.WriteLine("Do you wish to show this snipe in Discord?");
			if (Console.Read() == 'y' || Console.Read() == 'Y')
			{
				Console.WriteLine("Press enter when your snipe's NameMC is linked and you are ready to set the GitHub. You will have 90 seconds.");
				Console.ReadLine();
				HttpClient uploadClient = new HttpClient();
				string res = uploadClient.GetStringAsync($"https://3snipe.com/verify.php?name={name}").Result;
				Console.WriteLine($"Set GitHub to {res} now.");
				Thread.Sleep(90000);
				Console.WriteLine("90 seconds done. If verified successfully you will see your snipe.");
			}
			
		}
	}
}