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

namespace _3Snipe_NETcore
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
        static readonly string vCode = "v1.1.2";
        static object lockObj = new object();
        static bool snipedAlready = false;
        static void Main(string[] args)
        {
            menu();
        }
        static void branding()
        {
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
                    case '2': mutliAcctSnipe(); break;
                    case '3': Console.Clear(); return;
                    default: break;
                }
            }
        }
        private static void mutliAcctSnipe()
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
                    if (accounts.Count == 3)
                        break;
                }
                catch
                {
                    if (File.Exists(account))
                    {
                        try
                        {
                            List<string> accounts2 = File.ReadAllLines(account).ToList();
                            if (accounts2.Count < 3)
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
                                for (int h = 0; h < 3; h++)
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
            try
            {
                Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(30000));
            }
            catch { }
            string emailSniped = "";
            int completeThreads = 0;
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
                    Console.ReadKey();
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
                    }
                    catch {
                        return;
                    }
                    try
                    {
                        string tempStr = sniperClient2.GetStringAsync("https://api.mojang.com/user/security/challenges").Result;
                    }
                    catch
                    {
                        return;
                    }
                    try
                    {
                        Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(2000) + TimeSpan.FromMilliseconds(delay * 100));
                    }
                    catch { }
                    for (int i = 0; i < 10; i++)
                    {
                        if (snipedAlready)
                            return;
                        try
                        {
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
                    lock (lockObj)
                    {
                        completeThreads++;
                    }
                }
                for (int i = 0; i < 20; i++)
                {
                    threads.Add(new Thread(new ParameterizedThreadStart(sniperthread)));
                }
                for (int i = 0; i < 20; i++)
                {
                    threads[i].Start(new ThreadInfo(i));
                }
                try
                {
                    Thread.Sleep(dropTime - DateTime.Now + TimeSpan.FromMilliseconds(30000));
                }
                catch { }
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
            var completedNeeded = 20 * accounts.Count;
            int tempComp = 0;
            lock (lockObj)
            {
                tempComp = completeThreads;
            }
            try
            {
                Thread.Sleep(dropTime - DateTime.Now);
            }
            catch { }
            DateTime start = DateTime.Now;
            while (tempComp != completedNeeded)
			{
                lock (lockObj)
                {
                    tempComp = completeThreads;
                }
                Thread.Sleep(10);
            }
            TimeSpan timeTaken = DateTime.Now - start;
            Console.WriteLine($"Requests done, all requests sent in {timeTaken}.");
            if (snipedAlready)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success. Set name to " + name + " on account " + emailSniped + ". Press any key to return to the menu.");
                Console.ResetColor();
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
                    if (accounts.Count == 3)
                        break;
                }
                catch
                {
                    if (File.Exists(account))
					{
                        try
                        {
                            List<string> accounts2 = File.ReadAllLines(account).ToList();
                            if (accounts2.Count < 3)
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
                                for (int h = 0; h < 3; h++)
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
                        } catch
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
            int completeThreads = 0;
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
                    Console.ReadKey();
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
                    }
                    catch
                    {
                        return;
                    }
                    try
                    {
                        string tempStr = sniperClient2.GetStringAsync("https://api.mojang.com/user/security/challenges").Result;
                    }
                    catch
                    {
                        return;
                    }
                    try
                    {
                        Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(2000) + TimeSpan.FromMilliseconds(delay * 200));
                    }
                    catch { }
                    for (int i = 0; i < 10; i++)
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
                    lock (lockObj)
                    {
                        completeThreads++;
                    }
                }
                for (int i = 0; i < 20; i++)
                {
                    threads.Add(new Thread(new ParameterizedThreadStart(sniperthread)));
                }
                for (int i = 0; i < 20; i++)
                {
                    threads[i].Start(new ThreadInfo(i));
                }
                try
                {
                    Thread.Sleep(dropTime - DateTime.Now + TimeSpan.FromMilliseconds(30000));
                }
                catch { }
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
            var completedNeeded = 200 * accounts.Count;
            try
            {
                Thread.Sleep(dropTime - DateTime.Now);
            }
            catch { }
            DateTime start = DateTime.Now;
            while (completeThreads != completedNeeded)
            { }
            TimeSpan timeTaken = DateTime.Now - start;
            Console.WriteLine($"Requests done, all requests sent in {timeTaken}.");
            if (snipedAlready)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success. Set name to " + name + " on account " + emailSniped + ". Press any key to return to the menu.");
                Console.ResetColor();
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
2) Back
");
                char option = Console.ReadKey().KeyChar;
                char[] options = { '1', '2' };
                while (!options.Contains(option))
                {
                    Console.Write("\r \r");
                    option = Console.ReadKey().KeyChar;
                }
                switch (option)
                {
                    case '1': doQuestions(); break;
                    case '2': Console.Clear(); return;
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
    }
}