﻿using System;
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
	class ThreadInfo
	{
		private int threadNum;
		public int ThreadID { get { return threadNum; } set { threadNum = value; } }
		public ThreadInfo(int tid)
		{
			threadNum = tid;
		}
	}
	class UserInfo
	{
		public UserInfo(string eml, string password)
		{
			email = eml;
			pass = password;
		}
		private String email;
		public string Email { get { return email; } set { email = value; } }
		private String pass;
		public string Password { get { return pass; } set { pass = value; } }
	}
	class Program
	{
		static readonly string vCode = "v2.0.0-beta.1";
		static object lockObj = new object();
		static bool snipedAlready = false;
		private static List<string> proxies = new List<string>();
		static void Main(string[] args)
		{
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
			for (; ; )
			{
				Console.Clear();
				branding();
				Console.WriteLine(@"Sniping Menu:
1) Snipe (multi-account or single)
2) Block (multi-account or single)
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
					case '1': mutliAcctSnipe(); break;
					case '2': mutliAcctBlock(); break;
					case '3': Console.Clear(); return;
					default: break;
				}
			}
		}
		private static void mutliAcctSnipe()
		{
			DateTime dropTime;
			List<UserInfo> accounts = new List<UserInfo>();
			int defaultThreadsCount = Process.GetCurrentProcess().Threads.Count;
			Console.Clear();
			string account = "";
			do
			{
				try
				{
					account = "";
					ConsoleKeyInfo key;
					Console.WriteLine("Enter your account in the format of 'email:password' (max 30) or leave blank or type in the filename and press enter: ");
					key = Console.ReadKey();
					while (key.Key != ConsoleKey.Enter)
					{

						if (key.Key != ConsoleKey.Backspace)
						{
							account += key.KeyChar;
							Console.Write("*");
						}
						else
						{
							Console.Write("\b \b");
							try
							{
								account = account.Substring(0, account.Length - 1);
							}
							catch { }
						}
						key = Console.ReadKey(true);
					}
					List<string> splits = account.Split(':').ToList();
					string email = account.Split(':')[0];
					splits.RemoveAt(0);
					string password = "";
					for (int i = 0; i < splits.Count; i++)
					{
						password += splits[i];
						if (i != splits.Count - 1)
							password += ":";
					}
					accounts.Add(new UserInfo(email, password));
					if (accounts.Count == 30)
						break;
				}
				catch
				{
					if (File.Exists(account))
					{
						try
						{
							List<string> accounts2 = File.ReadAllLines(account).ToList();
							if (accounts2.Count < 30)
							{
								foreach (var acc in accounts2)
								{
									List<string> splits = acc.Split(':').ToList();
									string email = acc.Split(':')[0];
									splits.RemoveAt(0);
									string password = "";
									for (int i = 0; i < splits.Count; i++)
									{
										password += splits[i];
										if (i != splits.Count - 1)
											password += ":";
									}
									accounts.Add(new UserInfo(email, password));
								}
							}
							else
							{
								for (int h = 0; h < 30; h++)
								{
									string acc = accounts2[h];
									List<string> splits = acc.Split(':').ToList();
									string email = acc.Split(':')[0];
									splits.RemoveAt(0);
									string password = "";
									for (int i = 0; i < splits.Count; i++)
									{
										password += splits[i];
										if (i != splits.Count - 1)
											password += ":";
									}
									accounts.Add(new UserInfo(email, password));
								}
							}
						}
						catch
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("[Error] An error occured reading your accounts file. Make sure it is formatted properly. The proper format is email:password.");
							Console.ResetColor();
							Console.ReadKey();
							return;
						}
					}
					else
						break;
				}
				Console.Clear();
			} while (account != "");
			int k = 0;
			int n = accounts.Count;
			if (accounts.Count != 30)
				while (accounts.Count != 30)
				{
					accounts.Add(new UserInfo(accounts[k].Email, accounts[k].Password));
					k++;
					if (k == n)
						k = 0;
				}
			Console.Clear();
			Console.WriteLine("Enter name to snipe (leave blank to return to menu) and press enter: ");
			string name = Console.ReadLine();
			HttpClient sniperClient = new HttpClient();
			Console.WriteLine();
			try
			{
				JObject tempObj = JObject.Parse(sniperClient.GetStringAsync($"https://api.mojang.com/users/profiles/minecraft/" + name + "?at=" + (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3196800)).Result);
				string oldOwnerID = (string)tempObj["id"];
				JArray tempArr = JArray.Parse(sniperClient.GetStringAsync($"https://api.mojang.com/user/profiles/" + oldOwnerID + "/names").Result);
				List<int> indexList = new List<int>();
				for (int i = 0; i < tempArr.Count; i++)
				{
					string name2 = (string)tempArr[i]["name"];
					if (name.ToLower() == name2.ToLower())
						indexList.Add(i);
				}
				int lastIndex = indexList[indexList.Count - 1] + 1;
				dropTime = DateTimeOffset.FromUnixTimeMilliseconds((long)tempArr[lastIndex]["changedToAt"] + 3196800000).ToLocalTime().DateTime;
			}
			catch
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("[Error] This name is not dropping or has already dropped. Press any key to return to the menu.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}
			string emailSniped = "";
			int completeThreads = 0;
			ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;
			int defaultThreadCount = currentThreads.Count;
			void acctThread(object user2)
			{
				WebProxy proxy = null;
				if (proxies.Count > 0)
				{
					Random rand = new Random();
					int index = rand.Next(proxies.Count);
					try
					{
						proxy = new WebProxy(proxies[index]);
					}
					catch
					{
						proxy = null;
					}
				}
				UserInfo user = (UserInfo)user2;
				string accessToken = "";
				string clientToken = ""; //used for refresh
				string f16 = "";
				HttpClient authClient = new HttpClient();
				try
				{
					var content = new StringContent($"{{\"agent\": {{\"name\": \"Minecraft\", \"version\": 1}},\"username\": \"{user.Email}\", \"password\": \"{user.Password}\"}}", Encoding.UTF8, "application/json");
					string tokenResponse = "";
					tokenResponse = authClient.PostAsync("https://authserver.mojang.com/authenticate", content).Result.Content.ReadAsStringAsync().Result;
					accessToken = (string)JObject.Parse(tokenResponse)["accessToken"];
					clientToken = (string)JObject.Parse(tokenResponse)["clientToken"];
					f16 = accessToken.Split('.')[1].Substring(0, 16);
				}
				catch
				{
					lock (lockObj)
					{
						completeThreads++;
					}
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("[Error] An account provided is invalid. Continuing execution without account.");
					Console.ResetColor();
					return;
				}
				Console.WriteLine($"[Info] Got token. First 16 characters of middle are {f16}.");
				var payload = new StringContent("{\"name\": \"" + name + "\", \"password\": \"" + user.Password + "\"}", Encoding.UTF8, "application/json");
				try
				{
					authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
					string tempStr = authClient.GetStringAsync("https://api.mojang.com/user/security/challenges").Result;

				}
				catch
				{
					return;
				}
				string userUUID = "";
				string temp = "";
				try
				{
					temp = authClient.GetStringAsync("https://api.mojang.com/user/profiles/agent/minecraft").Result;
					userUUID = (string)JObject.Parse(temp.Substring(1, temp.Length - 2))["id"];
				}
				catch
				{
					return;
				}
				List<Thread> threads = new List<Thread>();
				var innerThreadsComplete = 0;
				void sniperthread(object info)
				{
					int delay = ((ThreadInfo)info).ThreadID;
					HttpClient sniperClient2 = new HttpClient(new HttpClientHandler()
					{
						Proxy = proxy
					});


					var content = new StringContent($"{{\"agent\": {{\"name\": \"Minecraft\", \"version\": 1}},\"username\": \"{user.Email}\", \"password\": \"{user.Password}\"}}", Encoding.UTF8, "application/json");
					string tokenResponse = "";
					try
					{
						tokenResponse = sniperClient2.PostAsync("https://authserver.mojang.com/authenticate", content).Result.Content.ReadAsStringAsync().Result;
						var accessToken2 = (string)JObject.Parse(tokenResponse)["accessToken"];
						sniperClient2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken2);
						string tempStr = sniperClient2.GetStringAsync("https://api.mojang.com/user/security/challenges").Result;
					}
					catch
					{
						lock (lockObj)
						{
							innerThreadsComplete++;
						}
						return;
					}
					try
					{
						Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(2000) + TimeSpan.FromMilliseconds(delay * 200));
					}
					catch { }
					for (int i = 0; i < 20; i++)
					{
						if (snipedAlready)
							return;
						var response = sniperClient2.PostAsync("https://api.mojang.com/user/profile/" + userUUID + "/name", payload).Result;
						if (response.StatusCode == HttpStatusCode.NoContent)
						{
							Console.WriteLine($"[Info] Got status code of 204 on a thread, request number {i}.");
							snipedAlready = true;
							emailSniped = user.Email;
						}
						else if (response.IsSuccessStatusCode)
							Console.WriteLine($"[Info] Got status code of {response.StatusCode} on a thread, request number {i}.");
						else
							Console.WriteLine($"[Info] Got status code of {response.StatusCode} on a thread, request number {i}.");
						Thread.Sleep(75);
					}
					lock (lockObj)
					{
						innerThreadsComplete++;
					}
				}
				for (int i = 0; i < 5; i++)
				{
					threads.Add(new Thread(new ParameterizedThreadStart(sniperthread)));
				}
				for (int i = 0; i < 5; i++)
				{
					threads[i].Start(new ThreadInfo(i));
				}
				while (innerThreadsComplete != 5)
				{ }
				accessToken = "Disposed.";
				user.Password = "Disposed.";
				payload = null;
			}
			var threads2 = new List<Thread>();
			foreach (var account2 in accounts)
			{
				threads2.Add(new Thread(new ParameterizedThreadStart(acctThread)));
			}
			try
			{
				Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(30000));
			}
			catch { }
			for (int i = 0; i < threads2.Count; i++)
			{
				threads2[i].Start(accounts[i]);
				Thread.Sleep(1000);
			}
			try
			{
				Thread.Sleep(dropTime - DateTime.Now);
			}
			catch { }
			DateTime start = DateTime.Now;
			int running = Process.GetCurrentProcess().Threads.Count;
			while (running > defaultThreadsCount)
			{
				running = Process.GetCurrentProcess().Threads.Count;
			}
			TimeSpan timeTaken = DateTime.Now - start;
			Console.WriteLine($"Requests done, all requests sent in {timeTaken}.");
			if (snipedAlready)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Success. Set name to " + name + " on account " + emailSniped + ".");
				Console.ResetColor();
				verify(name);
				Console.WriteLine("Press any key to return to the menu.");
				Console.ReadKey();
				Console.Clear();
				return;
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Failed snipe on name " + name + ". Press any key to return to the menu.");
				Console.ResetColor();
				Console.ReadKey();
				Console.Clear();
				return;
			}
		}
		private static void mutliAcctBlock()
		{
			DateTime dropTime;
			List<UserInfo> accounts = new List<UserInfo>();
			Console.Clear();
			string account = "";
			do
			{
				try
				{
					account = "";
					ConsoleKeyInfo key;
					Console.WriteLine("Enter your account in the format of 'email:password' (max 3) or leave blank or type in the filename and press enter: ");
					key = Console.ReadKey();
					while (key.Key != ConsoleKey.Enter)
					{

						if (key.Key != ConsoleKey.Backspace)
						{
							account += key.KeyChar;
							Console.Write("*");
						}
						else
						{
							Console.Write("\b \b");
							try
							{
								account = account.Substring(0, account.Length - 1);
							}
							catch { }
						}
						key = Console.ReadKey(true);
					}
					List<string> splits = account.Split(':').ToList();
					string email = account.Split(':')[0];
					splits.RemoveAt(0);
					string password = "";
					for (int i = 0; i < splits.Count; i++)
					{
						password += splits[i];
						if (i != splits.Count - 1)
							password += ":";
					}
					accounts.Add(new UserInfo(email, password));
					if (accounts.Count == 1000)
						break;
				}
				catch
				{
					if (File.Exists(account))
					{
						try
						{
							List<string> accounts2 = File.ReadAllLines(account).ToList();
							if (accounts2.Count < 1000)
							{
								foreach (var acc in accounts2)
								{
									List<string> splits = acc.Split(':').ToList();
									string email = acc.Split(':')[0];
									splits.RemoveAt(0);
									string password = "";
									for (int i = 0; i < splits.Count; i++)
									{
										password += splits[i];
										if (i != splits.Count - 1)
											password += ":";
									}
									accounts.Add(new UserInfo(email, password));
								}
							}
							else
							{
								for (int h = 0; h < 1000; h++)
								{
									string acc = accounts2[h];
									List<string> splits = acc.Split(':').ToList();
									string email = acc.Split(':')[0];
									splits.RemoveAt(0);
									string password = "";
									for (int i = 0; i < splits.Count; i++)
									{
										password += splits[i];
										if (i != splits.Count - 1)
											password += ":";
									}
									accounts.Add(new UserInfo(email, password));
								}
							}
						}
						catch
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("[Error] An error occured reading your accounts file. Make sure it is formatted properly. The proper format is email:password.");
							Console.ResetColor();
							Console.ReadKey();
							return;
						}
					}
					else
						break;
				}
				Console.Clear();
			} while (account != "");
			Console.Clear();
			Console.WriteLine("Enter name to block (leave blank to return to menu) and press enter: ");
			string name = Console.ReadLine();
			HttpClient sniperClient = new HttpClient();
			Console.WriteLine();
			try
			{
				JObject tempObj = JObject.Parse(sniperClient.GetStringAsync($"https://api.mojang.com/users/profiles/minecraft/" + name + "?at=" + (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3196800)).Result);
				string oldOwnerID = (string)tempObj["id"];
				JArray tempArr = JArray.Parse(sniperClient.GetStringAsync($"https://api.mojang.com/user/profiles/" + oldOwnerID + "/names").Result);
				List<int> indexList = new List<int>();
				for (int i = 0; i < tempArr.Count; i++)
				{
					string name2 = (string)tempArr[i]["name"];
					if (name.ToLower() == name2.ToLower())
						indexList.Add(i);
				}
				int lastIndex = indexList[indexList.Count - 1] + 1;
				dropTime = DateTimeOffset.FromUnixTimeMilliseconds((long)tempArr[lastIndex]["changedToAt"] + 3196800000).ToLocalTime().DateTime;
			}
			catch
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("[Error] This name is not dropping or has already dropped. Press any key to return to the menu.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}
			try
			{
				Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(30000));
			}
			catch { }
			string emailSniped = "";
			int defaultThreadsCount = Process.GetCurrentProcess().Threads.Count;
			void acctThread(object user2)
			{
				UserInfo user = (UserInfo)user2;
				string accessToken = "";
				string clientToken = ""; //used for refresh
				string f16 = "";
				HttpClient authClient = new HttpClient();
				try
				{
					var content = new StringContent($"{{\"agent\": {{\"name\": \"Minecraft\", \"version\": 1}},\"username\": \"{user.Email}\", \"password\": \"{user.Password}\"}}", Encoding.UTF8, "application/json");
					string tokenResponse = "";
					tokenResponse = authClient.PostAsync("https://authserver.mojang.com/authenticate", content).Result.Content.ReadAsStringAsync().Result;
					accessToken = (string)JObject.Parse(tokenResponse)["accessToken"];
					clientToken = (string)JObject.Parse(tokenResponse)["clientToken"];
					f16 = accessToken.Split('.')[1].Substring(0, 16);
				}
				catch
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("[Error] An account provided is invalid. Continuing execution without account.");
					Console.ResetColor();
					return;
				}
				Console.WriteLine($"[Info] Got token. First 16 characters of middle are {f16}.");
				var payload = new StringContent("", Encoding.UTF8, "application/json");
				try
				{
					authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
					string tempStr = authClient.GetStringAsync("https://api.mojang.com/user/security/challenges").Result;

				}
				catch
				{
					return;
				}
				string userUUID = "";
				string temp = "";
				try
				{
					temp = authClient.GetStringAsync("https://api.mojang.com/user/profiles/agent/minecraft").Result;
					userUUID = (string)JObject.Parse(temp.Substring(1, temp.Length - 2))["id"];
				}
				catch
				{
					return;
				}
				List<Thread> threads = new List<Thread>();
				var innerThreadsComplete = 0;
				void sniperthread(object info)
				{
					int delay = ((ThreadInfo)info).ThreadID;
					HttpClient sniperClient2 = new HttpClient();
					var content = new StringContent($"{{\"agent\": {{\"name\": \"Minecraft\", \"version\": 1}},\"username\": \"{user.Email}\", \"password\": \"{user.Password}\"}}", Encoding.UTF8, "application/json");
					string tokenResponse = "";
					tokenResponse = authClient.PostAsync("https://authserver.mojang.com/authenticate", content).Result.Content.ReadAsStringAsync().Result;
					try
					{
						var accessToken2 = (string)JObject.Parse(tokenResponse)["accessToken"];
						sniperClient2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken2);
						string tempStr = sniperClient2.GetStringAsync("https://api.mojang.com/user/security/challenges").Result;
					}
					catch
					{
						lock (lockObj)
						{
							innerThreadsComplete++;
						}
						return;
					}
					try
					{
						Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(2000) + TimeSpan.FromMilliseconds(delay * 200));
					}
					catch { }
					for (int i = 0; i < 20; i++)
					{
						if (snipedAlready)
							return;
						try
						{
							var response = sniperClient2.PutAsync("https://api.mojang.com/user/profile/agent/minecraft/" + name, payload).Result;
							if (response.StatusCode == HttpStatusCode.NoContent)
							{
								Console.WriteLine($"[Info] Got status code of 204 on a thread, request number {i}.");
								snipedAlready = true;
								emailSniped = user.Email;
							}
							else if (response.IsSuccessStatusCode)
								Console.WriteLine($"[Info] Got status code of {response.StatusCode} on a thread, request number {i}.");
							else
								Console.WriteLine($"[Info] Got status code of {response.StatusCode} on a thread, request number {i}.");
						}
						catch (WebException e)
						{
							HttpStatusCode code = ((HttpWebResponse)e.Response).StatusCode;
							if (code == HttpStatusCode.NoContent)
							{
								snipedAlready = true;
							}
							else
							{
							}
							Console.WriteLine($"[Info] Got status code of {code} on a thread, request number {i}.");
						}
						Thread.Sleep(75);
					}
				}
				for (int i = 0; i < 5; i++)
				{
					threads.Add(new Thread(new ParameterizedThreadStart(sniperthread)));
				}
				for (int i = 0; i < 5; i++)
				{
					threads[i].Start(new ThreadInfo(i));
				}
				while (innerThreadsComplete != 5)
				{ }
				accessToken = "Disposed.";
				user.Password = "Disposed.";
				payload = null;
			}
			var threads2 = new List<Thread>();
			foreach (var account2 in accounts)
			{
				threads2.Add(new Thread(new ParameterizedThreadStart(acctThread)));
			}
			for (int i = 0; i < threads2.Count; i++)
			{
				threads2[i].Start(accounts[i]);
				Thread.Sleep(1);
			}
			var completedNeeded = accounts.Count;
			try
			{
				Thread.Sleep(dropTime - DateTime.Now);
			}
			catch { }
			DateTime start = DateTime.Now;
			int running = Process.GetCurrentProcess().Threads.Count;
			while (running > defaultThreadsCount)
			{
				running = Process.GetCurrentProcess().Threads.Count;
			}
			TimeSpan timeTaken = DateTime.Now - start;
			Console.WriteLine($"Requests done, all requests sent in {timeTaken}.");
			if (snipedAlready)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Success. Set name to " + name + " on account " + emailSniped + ".");
				Console.ResetColor();
				verify(name);
				Console.WriteLine("Press any key to return to the menu.");
				Console.ReadKey();
				Console.Clear();
				return;
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Failed snipe on name " + name + ". Press any key to return to the menu.");
				Console.ResetColor();
				Console.ReadKey();
				Console.Clear();
				return;
			}
		}
		private static void configure()
		{
			Console.Clear();
			Console.WriteLine("Under construction. Press a key to go back.");
			Console.ReadKey();
			Console.Clear();
			return;
		}
		private static void tools()
		{

			for (; ; )
			{
				Console.Clear();
				branding();
				Console.WriteLine(@"Tools:
1) Do Security Questions
2) Load proxies
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
					case '2': doProxies(); break;
					case '3': Console.Clear(); return;
					default: break;
				}
			}
		}
		private static void doProxies()
		{
			//get default thread #

			ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;
			int defaultThreadCount = currentThreads.Count;
			Console.Clear();
			List<string> working = new List<string>();
			proxies = new List<string>();
			string proxy = " ";
			while (proxy != "")
			{
				Console.WriteLine("Enter HTTP proxy or leave blank and press enter: ");
				proxy = Console.ReadLine();
				if (proxy != "") { proxies.Add(proxy); }
				Console.Clear();
			}
			Console.WriteLine("Proxy check running.");
			int running = defaultThreadCount;
			void proxyUp(object proxyObj)
			{
				string proxy = (string)proxyObj;
				try
				{
					var proxy2 = new WebProxy(proxy, false);
					var httpClient = new HttpClient(new HttpClientHandler()
					{
						Proxy = proxy2,
						UseProxy = true
					});
					httpClient.Timeout = TimeSpan.FromMilliseconds(500);
					var apiRes = httpClient.GetStringAsync("https://google.com/").Result;
					Console.WriteLine(proxy + " working.");
					working.Add(proxy);
				}
				catch
				{
					Console.WriteLine(proxy + " not working.");
				}
				return;
			}

			List<Thread> threads = new List<Thread>();
			for (int i = 0; i < proxies.Count; i++)
			{
				threads.Add(new Thread(new ParameterizedThreadStart(proxyUp)));
				if (running < 500 + defaultThreadCount)
				{
					threads[i].Start(proxies[i]);
				}
				else
				{
					while (running >= 500 + defaultThreadCount)
					{
						Thread.Sleep(100);
						running = Process.GetCurrentProcess().Threads.Count;
					}
					threads[i].Start(proxies[i]);
				}
				running = Process.GetCurrentProcess().Threads.Count;
			}


			while (running > defaultThreadCount)
			{
				Thread.Sleep(100);
				running = Process.GetCurrentProcess().Threads.Count;
			}
			proxies = working;
			Console.WriteLine("Proxy check complete. Press any key to return to menu.");
			Console.ReadKey();
			Console.Clear();
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

			Console.Clear();

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
			Console.Clear();
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
				Console.Clear();
				branding();
				Console.WriteLine(@"
Menu:
1) Snipe name
2) Settings
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
					case '2': configure(); break;
					case '3': tools(); break;
					case '4': Console.Clear(); System.Environment.Exit(0); break;
					default: break;
				}
				snipedAlready = false;
			}
		}
		private static void verify(string name)
		{
			Console.Clear();
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
			Console.Clear();
		}
	}
}