using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
namespace _3Snipe
{
	class SniperClass
	{
		static object lockObj = new object();
		static bool snipedAlready = false;
		private static List<string> proxies = new List<string>();
		public static void mutliAcctSnipe()
		{
			Console.WriteLine("\r \r");
			DateTime dropTime;
			List<UserInfo> accounts = new List<UserInfo>();
			int defaultThreadsCount = Process.GetCurrentProcess().Threads.Count;
			if (!File.Exists("./accounts.txt"))
			{
				string account = "";
				do
				{
					try
					{
						account = "";
						ConsoleKeyInfo key;
						Console.WriteLine("Enter your account in the format of 'email:password' or leave blank to stop: ");
						key = Console.ReadKey(true);
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
						Console.WriteLine();
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
					}
					catch
					{
						break;
					}
				} while (account != "");
			}
			else
			{
				try
				{
					StreamReader accountReader = new StreamReader("./accounts.txt");
					string account;
					while ((account = accountReader.ReadLine()) != null)
					{
						try
						{
							string[] questions = null;
							try
							{
								questions = account.Split("    ")[1].Split(':');
							}
							catch { }
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
							accounts.Add((questions == null) ? new UserInfo(email, password) : new UserInfo(email, password, questions));
						}
						catch { }
					}
					accountReader.Close();
				}
				catch
				{
					Console.WriteLine("Failed to open accounts.txt even though it exists.");
					return;
				}
			}
			Console.WriteLine("Enter name to snipe (leave blank to return to menu): ");
			string name = Console.ReadLine();
			HttpClient sniperClient = new HttpClient();
			Console.WriteLine();
			try
			{
				JObject tempObj = JObject.Parse(sniperClient.GetStringAsync($"https://api.nathan.cx/check/{name}").Result);
				dropTime = DateTime.Parse((string)tempObj["drop_time"]);
			}
			catch
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("[Error] This name is not dropping or has already dropped. Press any key to return to the menu.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}
			double customDelay = 0;
			Console.WriteLine("Enter an offset for timing (defaults to 0 if blank/invalid): ");
			try
			{
				customDelay = Double.Parse(Console.ReadLine());
			}
			catch { }
			double customInbetween = 120;
			Console.WriteLine("Enter an offset for timing between accounts (defaults to 120 if blank/invalid): ");
			try
			{
				customInbetween = Double.Parse(Console.ReadLine());
			}
			catch { }
			double customInbetweenAccountCount = 1;
			Console.WriteLine("Enter the number of accounts for each timing (defaults to 1 if blank/invalid): ");
			try
			{
				customInbetweenAccountCount = Double.Parse(Console.ReadLine());
				if (customInbetweenAccountCount < 1)
				{
					customInbetweenAccountCount = 1;
				}
			}
			catch { }
			string emailSniped = "";
			int completeThreads = 0;
			ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;
			int defaultThreadCount = currentThreads.Count;
			void acctPreCheck(object user2)
			{
				UserInfo user = (UserInfo)user2;
				HttpClient authClient = new HttpClient();
				try
				{
					var content = new StringContent($"{{\"agent\": {{\"name\": \"Minecraft\", \"version\": 1}},\"username\": \"{user.Email}\", \"password\": \"{user.Password}\"}}", Encoding.UTF8, "application/json");
					string tokenResponse = "";
					tokenResponse = authClient.PostAsync("https://authserver.mojang.com/authenticate", content).Result.Content.ReadAsStringAsync().Result;
					string token = (string)JObject.Parse(tokenResponse)["accessToken"];
				}
				catch
				{
					Console.ForegroundColor = ConsoleColor.Red;
					accounts.RemoveAt(accounts.IndexOf(user));
					Console.WriteLine($"[Error] The account provided with email {user.Email} is invalid. Removed from account list (not the file, just program.)");
					Console.ResetColor();
					return;
				}
			}
			foreach (var acct in accounts)
			{
				acctPreCheck(acct);
			}
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
					Console.WriteLine($"[Error] The account provided with email {user.Email} is invalid.");
					Console.ResetColor();
					return;
				}
				Console.WriteLine($"[Info] Got token. First 16 characters of middle are {f16}.");
				var payload = new StringContent("{\"name\": \"" + name + "\", \"password\": \"" + user.Password + "\"}", Encoding.UTF8, "application/json");
				try
				{
					authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
					HttpResponseMessage tempStr = authClient.GetAsync("https://api.mojang.com/user/security/location").Result;
					if (tempStr.StatusCode != HttpStatusCode.NoContent)
					{
						JArray questionRes = JArray.Parse(authClient.GetStringAsync("https://api.mojang.com/user/security/challenges").Result);
						string req = $"[{{\"id\": \"{(string)questionRes[0]["answer"]["id"]}\", \"answer\": \"{user.SecurityInfo[0]}\"}}, {{\"id\": \"{(string)questionRes[1]["answer"]["id"]}\", \"answer\": \"{user.SecurityInfo[1]}\"}}, {{\"id\": \"{(string)questionRes[2]["answer"]["id"]}\", \"answer\": \"{user.SecurityInfo[2]}\"}}]";
						HttpResponseMessage res = authClient.PostAsync("https://api.mojang.com/user/security/location", new StringContent(req)).Result;
						if (res.StatusCode != HttpStatusCode.NoContent) return;
					}
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
					delay = (int)(delay / customInbetweenAccountCount);
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
						Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(customDelay) + TimeSpan.FromMilliseconds(delay * customInbetween));
					}
					catch { }
					for (int i = 0; i < 3; i++)
					{
						if (snipedAlready)
							return;
						var response = sniperClient2.PostAsync("https://api.minecraftservices.com/minecraft/profile/name/" + name, new StringContent("")).Result;
						if (response.StatusCode == HttpStatusCode.OK)
						{
							Console.WriteLine($"[Info] Got status code of 200 on a thread, request number {i}. Time = {DateTime.Now.ToLongTimeString()}");
							snipedAlready = true;
							emailSniped = user.Email;
						}
						else if (response.IsSuccessStatusCode)
							Console.WriteLine($"[Info] Got status code of {response.StatusCode} on a thread, request number {i}. Time = {DateTime.Now.ToLongTimeString()}");
						else
							Console.WriteLine($"[Info] Got status code of {response.StatusCode} on a thread, request number {i}. Time = {DateTime.Now.ToLongTimeString()}");
					}
					accessToken = "Disposed.";
					user.Password = "Disposed.";
					payload = null;
				}
				var thread = new Thread(new ParameterizedThreadStart(sniperthread));
				thread.Start(new ThreadInfo(accounts.IndexOf(user)));
				while (payload != null)
				{ }
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
			Console.WriteLine($"Requests done, all requests sent in {timeTaken}. RPS = {(accounts.Count * 3) / timeTaken.TotalSeconds}");
			if (snipedAlready)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Success. Set name to " + name + " on account " + emailSniped + ".");
				Console.ResetColor();
				Program.verify(name);
				Console.WriteLine("Press any key to return to the menu.");
				snipedAlready = false;
				Console.ReadKey();

				return;
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Failed snipe on name " + name + ". Press any key to return to the menu.");
				snipedAlready = false;
				Console.ResetColor();
				Console.ReadKey();

				return;
			}
		}
	}
}
