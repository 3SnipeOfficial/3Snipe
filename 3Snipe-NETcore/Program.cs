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
        static readonly string vCode = "v1.1.0";
        static bool snipedAlready;
        static void Main()
        {
            menu();
        }
        static void branding()
        {
            Console.WriteLine($@"                                                 
██████╗ ███████╗███╗   ██╗██╗██████╗ ███████╗
╚════██╗██╔════╝████╗  ██║██║██╔══██╗██╔════╝
 █████╔╝███████╗██╔██╗ ██║██║██████╔╝█████╗  
 ╚═══██╗╚════██║██║╚██╗██║██║██╔═══╝ ██╔══╝  
██████╔╝███████║██║ ╚████║██║██║     ███████╗
╚═════╝ ╚══════╝╚═╝  ╚═══╝╚═╝╚═╝     ╚══════╝

{vCode}

");
        }
        private static void Snipe()
        {
            DateTime dropTime;
            Console.Clear();
            string email;
            string password;
            try
            {
                string account = "";
                ConsoleKeyInfo key;
                Console.WriteLine("Enter your Minecraft account in the format of 'email:password' and press enter: ");
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
                    }
                    key = Console.ReadKey(true);
                }

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
            Console.Clear();
            string userUUID = "";
            Console.WriteLine("Enter name to snipe and press enter: ");
            string wantedName = Console.ReadLine();
            HttpClient sniperClient = new HttpClient();
            Console.WriteLine();
            try
            {
                JObject oldOwner = JObject.Parse(sniperClient.GetStringAsync($"https://api.mojang.com/users/profiles/minecraft/" + wantedName + "?at=" + (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3196800)).Result);
                string oldOwnerID = (string)oldOwner["id"];
                JArray oldOwnerNameHist = JArray.Parse(sniperClient.GetStringAsync($"https://api.mojang.com/user/profiles/" + oldOwnerID + "/names").Result);
                List<int> indexList = new List<int>();
                for (int i = 0; i < oldOwnerNameHist.Count; i++)
                {
                    string nameInHist = (string)oldOwnerNameHist[i]["name"];
                    if (wantedName.ToLower() == nameInHist.ToLower())
                        indexList.Add(i);
                }
                int lastIndex = indexList[indexList.Count - 1] + 1;
                dropTime = DateTimeOffset.FromUnixTimeMilliseconds((long)oldOwnerNameHist[lastIndex]["changedToAt"] + 3196800000).ToLocalTime().DateTime;
                try
                {
                    Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(30000));
                }
                catch { }
                string accessToken;
                string accessTokenCensored;
                try
                {
                    var content = new StringContent($"{{\"agent\": {{\"name\": \"Minecraft\", \"version\": 1}},\"username\": \"{email}\", \"password\": \"{password}\"}}", Encoding.UTF8, "application/json");
                    var tokenResponse = sniperClient.PostAsync("https://authserver.mojang.com/authenticate", content).Result.Content.ReadAsStringAsync().Result;
                    accessToken = (string)JObject.Parse(tokenResponse)["accessToken"];
                    accessTokenCensored = accessToken.Split('.')[1].Substring(0, 16);
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Error] The account provided is invalid. Press any key to return to the menu.");
                    Console.ResetColor();
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine($"[Info] Got token. First 16 characters of middle are {accessTokenCensored}");
                sniperClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",  accessToken);
                var payload = new StringContent("{\"name\": \"" + wantedName + "\", \"password\": \"" + password + "\"}", Encoding.UTF8,  "application/json");
                try
                {
                    string tempStr = sniperClient.GetStringAsync("https://api.mojang.com/user/security/challenges").Result;
                    Console.WriteLine("[Info] Readied token for usage.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Error] Security questions are unanswered (use the tool for this). Press any key to return to the menu.");
                    Console.ResetColor();
                    Console.ReadKey();
                    return;
                }
                var temp = sniperClient.GetStringAsync("https://api.mojang.com/user/profiles/agent/minecraft").Result;
                userUUID = (string)JObject.Parse(temp.Substring(1, temp.Length - 2))["id"];
                List<Thread> threads = new List<Thread>();
                void sniperThread(object info)
                {
                    int delay = ((ThreadInfo)info).ThreadID;
                    HttpClient sniperClient2 = new HttpClient();
                    sniperClient2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    try
                    {
                        Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(14) + TimeSpan.FromMilliseconds(delay * 5));
                    }
                    catch { }
                    for (int i = 0; i < 4; i++)
                    {
                        if (snipedAlready)
                            return;
                        try
                        {
                            var response = sniperClient2.PostAsync("https://api.mojang.com/user/profile/" + userUUID + "/name", payload).Result;
                            if (response.StatusCode == HttpStatusCode.NoContent)
                            {
                                Console.WriteLine($"[Info] Got status code of 2XX on a thread, request number {i}.");
                                snipedAlready = true;
                            }
                            else if (response.IsSuccessStatusCode)
                                Console.WriteLine($"[Info] Got status code of 204 on a thread, request number {i}.");
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
                        }
                    }
                }
                for (int i = 0; i < 25; i++)
                {
                    threads.Add(new Thread(new ParameterizedThreadStart(sniperThread)));
                }

                for (int i = 0; i < 25; i++)
                {
                    threads[i].Start(new ThreadInfo(i));
                }
                try
                {
                    Thread.Sleep(dropTime - DateTime.Now);
                }
                catch { }
                try
                {
                    Thread.Sleep(20000);
                }
                catch { }
                accessToken = "Disposed.";
                payload = null;
                if (snipedAlready)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Success. Set name to " + wantedName + ". Press any key to return to the menu.");
                    Console.ResetColor();
                    Console.ReadKey();
                    Console.Clear();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed snipe. Press any key to return to the menu.");
                    Console.ResetColor();
                    Console.ReadKey();
                    Console.Clear();
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Error] This name is not dropping or has already dropped. Press any key to return to the menu.");
                Console.ResetColor();
                Console.ReadKey();
            }
        }
        private static void mutliAcctSnipe()
        {
            DateTime dropTime;
            List<UserInfo> accounts = new List<UserInfo>();
            Console.Clear();
            string account;
            do
            {
                try
                {
                    account = "";
                    ConsoleKeyInfo key;
                    Console.WriteLine("Enter your Minecraft account in the format of 'email:password' or leave blank and press enter: ");
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
                    string email = account.Split(':')[0];
                    string password = account.Split(':')[1];
                    accounts.Add(new UserInfo(email, password));
                }
                catch
                {
                    break;
                }
                Console.Clear();
            } while (account != "");
            Console.Clear();
            Console.WriteLine("Enter name to snipe and press enter: ");
            string wantedName = Console.ReadLine();
            HttpClient sniperClient = new HttpClient();
            Console.WriteLine();
            try
            {
                JObject oldOwner = JObject.Parse(sniperClient.GetStringAsync($"https://api.mojang.com/users/profiles/minecraft/" + wantedName + "?at=" + (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3196800)).Result);
                string oldOwnerID = (string)oldOwner["id"];
                JArray oldOwnerNameHist = JArray.Parse(sniperClient.GetStringAsync($"https://api.mojang.com/user/profiles/" + oldOwnerID + "/names").Result);
                List<int> indexList = new List<int>();
                for (int i = 0; i < oldOwnerNameHist.Count; i++)
                {
                    string nameInHist = (string)oldOwnerNameHist[i]["name"];
                    if (wantedName.ToLower() == nameInHist.ToLower())
                        indexList.Add(i);
                }
                int lastIndex = indexList[indexList.Count - 1] + 1;
                dropTime = DateTimeOffset.FromUnixTimeMilliseconds((long)oldOwnerNameHist[lastIndex]["changedToAt"] + 3196800000).ToLocalTime().DateTime;
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
            void acctThread(object user2)
            {
                UserInfo user = (UserInfo)user2;
                string accessToken;
                string clientToken = ""; //used for refresh
                string accessTokenCensored;
                HttpClient authClient = new HttpClient();
                try
                {
                    var content = new StringContent($"{{\"agent\": {{\"name\": \"Minecraft\", \"version\": 1}},\"username\": \"{user.Email}\", \"password\": \"{user.Password}\"}}", Encoding.UTF8, "application/json");
                    string tokenResponse;
                    tokenResponse = authClient.PostAsync("https://authserver.mojang.com/authenticate", content).Result.Content.ReadAsStringAsync().Result;
                    accessToken = (string)JObject.Parse(tokenResponse)["accessToken"];
                    accessTokenCensored = accessToken.Split('.')[1].Substring(0, 16);
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Error] An account provided is invalid. Continuing execution without account.");
                    Console.ResetColor();
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine($"[Info] Got token. First 16 characters of middle are {accessTokenCensored}");
                var payload = new StringContent("{\"name\": \"" + wantedName + "\", \"password\": \"" + user.Password + "\"}", Encoding.UTF8, "application/json");
                try
                {
                    Console.WriteLine("[Info] Readying token for usage...");
                    authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    string tempStr = authClient.GetStringAsync("https://api.mojang.com/user/security/challenges").Result;
                    Console.WriteLine("[Info] Readied token for usage.");

                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Error] Security questions are unanswered (use the tool for this). Continuing execution.");
                    Console.ResetColor();
                    return;
                }
                string userUUID;
                string temp;
                try
                {
                    temp = authClient.GetStringAsync("https://api.mojang.com/user/profiles/agent/minecraft").Result;
                    userUUID = (string)JObject.Parse(temp.Substring(1, temp.Length - 2))["id"];
                    Console.WriteLine(temp);
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Error] Failed to get UUID. Continuing execution.");
                    Console.ResetColor();
                    return;
                }
                List<Thread> threads = new List<Thread>();
                void sniperthread(object info)
                {
                    int delay = ((ThreadInfo)info).ThreadID;
                    HttpClient sniperClient2 = new HttpClient();
                    sniperClient2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    try
                    {
                        Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(500) + TimeSpan.FromMilliseconds(delay * 5));
                    }
                    catch { }
                    for (int i = 0; i < 4; i++)
                    {
                        if (snipedAlready)
                            return;
                        try
                        {
                            var response = sniperClient2.PostAsync("https://api.mojang.com/user/profile/" + userUUID + "/name", payload).Result;
                            if (response.StatusCode == HttpStatusCode.NoContent)
                            {
                                Console.WriteLine($"[Info] Got status code of 2XX on a thread, request number {i}.");
                                snipedAlready = true;
                                emailSniped = user.Email;
                            }
                            else if (response.IsSuccessStatusCode)
                                Console.WriteLine($"[Info] Got status code of 204 on a thread, request number {i}.");
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
                            Console.WriteLine($"[Info] Got status code of {code} on a thread, request number {i}.");
                        }
                    }
                }
                for (int i = 0; i < 10; i++)
                {
                    threads.Add(new Thread(new ParameterizedThreadStart(sniperthread)));
                }
                for (int i = 0; i < 10; i++)
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
            try
            {
                Thread.Sleep(dropTime - DateTime.Now + TimeSpan.FromMilliseconds(30000));
            }
            catch { }
            if (snipedAlready)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success. Set name to " + wantedName + " on account " + emailSniped + ". Press any key to return to the menu.");
                Console.ResetColor();
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed snipe. Press any key to return to the menu.");
                Console.ResetColor();
                Console.ReadKey();
                Console.Clear();
            }
        }
        private static void configure()
        {
            Console.Clear();
            Console.WriteLine("Under construction. Press a key to go back.");
            Console.ReadKey();
            Console.Clear();
        }
        private static void tools()
        {

            for (; ; )
            {
                branding();
                Console.Clear();
                Console.WriteLine(@"Tools:
1) Do Security Questions
2) Multi-Account Snipe
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
                    case '2': mutliAcctSnipe(); break;
                    case '3': Console.Clear(); return;
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
            string accessTokenCensored = accessToken.Substring(0, 16);
            Console.WriteLine($"[Info] Got token. First 16 characters are {accessTokenCensored}");
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
                    throw new Exception("Questions needed, but response invalid.");
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
                    case '4': System.Environment.Exit(0); break;
                }
                snipedAlready = false;
            }
        }
    }
}