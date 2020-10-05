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
			DateTime dropTime;
			List<UserInfo> accounts = new List<UserInfo>();
			int defaultThreadsCount = Process.GetCurrentProcess().Threads.Count;
			
			string account = "";
			do
			{
				try
				{
					account = "";
					ConsoleKeyInfo key;
					Console.WriteLine("Enter your account in the format of 'email:password' or leave blank or type in the filename and press enter: ");
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
				if (customInbetweenAccountCount < 1) {
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
					Console.WriteLine($"[Error] The account provided with email {user.Email} is invalid.");
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
					delay = (int)(delay/customInbetweenAccountCount);
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
						Thread.Sleep(dropTime - DateTime.Now + TimeSpan.FromMilliseconds(customDelay) + TimeSpan.FromMilliseconds(delay * customInbetween));
					}
					catch { }
					for (int i = 0; i < 5; i++)
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
					threads[i].Start(new ThreadInfo(i * accounts.IndexOf(user)));
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
		public static void mutliAcctBlock()
		{
			DateTime dropTime;
			List<UserInfo> accounts = new List<UserInfo>();
			
			string account = "";
			do
			{
				try
				{
					account = "";
					ConsoleKeyInfo key;
					Console.WriteLine("Enter your account in the format of 'email:password' (max 30) or leave blank or type in the filename and press enter: ");
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
				
			} while (account != "");
			int k = 0;
			int n = accounts.Count;
			int l = 0;
			if (accounts.Count != 30)
				while (accounts.Count != 30)
				{
					accounts.Add(new UserInfo(accounts[k].Email, accounts[k].Password));
					k++;
					if (k == n)
					{
						k = 0;
						l++;
					}
					if (l > 2)
						break;
				}
			
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
			void acctPreCheck(object user2)
			{
				UserInfo user = (UserInfo)user2;
				HttpClient authClient = new HttpClient();
				try
				{
					var content = new StringContent($"{{\"agent\": {{\"name\": \"Minecraft\", \"version\": 1}},\"username\": \"{user.Email}\", \"password\": \"{user.Password}\"}}", Encoding.UTF8, "application/json");
					string tokenResponse = "";
					tokenResponse = authClient.PostAsync("https://authserver.mojang.com/authenticate", content).Result.Content.ReadAsStringAsync().Result;
				}
				catch
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"[Error] The account provided with email {user.Email} is invalid.");
					Console.ResetColor();
					return;
				}
			}
			foreach (var acct in accounts)
			{
				acctPreCheck(acct);
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
					Console.WriteLine($"[Error] The account provided with email {user.Email} is invalid.");
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
						Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(1200) + TimeSpan.FromMilliseconds(delay * 120));
					}
					catch { }
					for (int i = 0; i < 5; i++)
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
					}
				}
				for (int i = 0; i < 5; i++)
				{
					threads.Add(new Thread(new ParameterizedThreadStart(sniperthread)));
				}
				for (int i = 0; i < 5; i++)
				{
					threads[i].Start(new ThreadInfo(i * accounts.IndexOf(user)));
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
		public static void doProxies()
		{
			//get default thread #

			ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;
			int defaultThreadCount = currentThreads.Count;
			
			List<string> working = new List<string>();
			proxies = new List<string>();
			string proxy = " ";
			while (proxy != "")
			{
				Console.WriteLine("Enter HTTP proxy or leave blank and press enter: ");
				proxy = Console.ReadLine();
				if (proxy != "") { proxies.Add(proxy); }
				
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
			
		}
	}
}
