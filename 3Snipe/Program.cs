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
		static readonly string vCode = "v2.0.0-beta.12";
		
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
			Console.WriteLine("This menu has been deprecated in favor of accounts.txt. If you need to authenticate with questions,\nuse accounts.txt or use minecraft.net");
			return;
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