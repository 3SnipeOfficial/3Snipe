using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace _3Snipe_NETcore
{
    class Program
    {
        static bool snipedAlready = false;
        static void Main(string[] args)
        {
            menu();
        }
        private static void snipe()
        {
            DateTime dropTime;
            Console.Clear();
            Console.WriteLine("Enter your Minecraft account in the format of 'email:password' and press enter: ");
            string account = Console.ReadLine();
            string email = account.Split(':')[0];
            string password = account.Split(':')[1];
            Console.Clear();
            string userUUID = "";
            Console.WriteLine("Enter name to snipe and press enter: ");
            string name = Console.ReadLine();
            WebClient sniperClient = new WebClient();
            Console.WriteLine();
            try
            {
                JObject yeet = JObject.Parse(sniperClient.DownloadString($"https://api.mojang.com/users/profiles/minecraft/" + name + "?at=" + (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3196800)));
                string oldOwnerID = (string)yeet["id"];
                JArray yeet2 = JArray.Parse(sniperClient.DownloadString($"https://api.mojang.com/user/profiles/" + oldOwnerID + "/names"));
                List<int> indexList = new List<int>();
                for (int i = 0; i < yeet2.Count; i++)
                {
                    string name2 = (string)yeet2[i]["name"];
                    if (name.ToLower() == name2.ToLower())
                        indexList.Add(i);
                }
                int lastIndex = indexList[indexList.Count - 1] + 1;
                dropTime = DateTimeOffset.FromUnixTimeMilliseconds((long)yeet2[lastIndex]["changedToAt"] + 3196800000).ToLocalTime().DateTime;
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
                Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(60000));
            }
            catch { }
            string accessToken = "";
            try
            {
                string tokenResponse = sniperClient.UploadString("https://authserver.mojang.com/authenticate", $"{{\"agent\": {{\"name\": \"Minecraft\", \"version\": 1}},\"username\": \"{email}\", \"password\": \"{password}\"}}");
                accessToken = (string)JObject.Parse(tokenResponse)["accessToken"];
            } catch {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Error] The account provided is invalid. Press any key to return to the menu.");
                Console.ResetColor();
                Console.ReadKey();
            }
            string f16 = accessToken.Substring(0, 16);
            Console.WriteLine($"[Info] Got token. First 16 characters are {f16}");
            sniperClient.Headers[HttpRequestHeader.Authorization] = $"Bearer {accessToken}";
            string payload = "{'name': '" + name + "', 'password': '" + password + "'}";
            try
            {
                Console.WriteLine("[Info] Readying token for usage...");
                sniperClient.DownloadString("https://api.mojang.com/user/security/location");
                Console.WriteLine("[Info] Readied token for usage.");
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Error] Token could not be readied or questions are unanswered (use the tool for this). Press any key to return to the menu.");
                Console.ResetColor();
                Console.ReadKey();
                return;
            }
            var temp = sniperClient.DownloadString("https://api.mojang.com/user/profiles/agent/minecraft");
            Console.WriteLine(temp);
            userUUID = (string)JObject.Parse(temp.Substring(1, temp.Length - 2))["id"];
            Console.WriteLine(userUUID);
            List<Thread> threads = new List<Thread>();
            void sniperthread()
            {
                if (snipedAlready)
                    return;
                try
                {
                    WebClient sniperClient2 = new WebClient();
                    sniperClient2.Headers[HttpRequestHeader.Authorization] = $"Bearer {accessToken}";
                    string response = sniperClient2.UploadString("https://api.mojang.com/user/profile/" + userUUID + "/name", payload);
                    if (response != string.Empty)
                        Console.WriteLine($"[Info] Got status code of 2XX on a thread.");
                    else
                        Console.WriteLine($"[Info] Got status code of 204 on a thread.");
                    snipedAlready = true;
                    return;
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
                    Console.WriteLine($"[Info] Got status code of {code} on a thread.");
                }
            }
            for (int i = 0; i < 20; i++)
            {
                threads.Add(new Thread(new ThreadStart(sniperthread)));
            }
            try
            {
                Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(10));
            }
            catch { }
            for (int i = 0; i < 20; i++)
            {
                if (snipedAlready)
                    break;
                threads[i].Start();
                Thread.Sleep(5);
            }
            accessToken = "Disposed.";
            password = "Disposed.";
            payload = "Disposed.";
            try
            {
                Thread.Sleep(20000);
            } catch { }
            if (snipedAlready)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success. Set name to " + name + ". Press any key to return to the menu.");
                Console.ResetColor();
                Console.ReadKey();
                Console.Clear();
                return;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed snipe. Press any key to return to the menu.");
                Console.ResetColor();
                Console.ReadKey();
                Console.Clear();
                return;
            }
        }
        private static void configure()
        {
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

            WebClient sniperClient = new WebClient();
            Console.WriteLine("Enter your Minecraft account in the format of 'email:password' and press enter: ");
            string account = Console.ReadLine();
            string email = account.Split(':')[0];
            string password = account.Split(':')[1];
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
            sniperClient.Headers[HttpRequestHeader.Authorization] = $"Bearer {accessToken}";
            try
            {
                string tempqs = sniperClient.DownloadString("https://api.mojang.com/user/security/location");
                Console.WriteLine("[Info] Questions not needed. Press any key to return to the menu.");
                Console.ReadKey();
                return;
            }
            catch { }
            Console.WriteLine("[Info] Getting questions now.");
            List<string> questions = new List<string>();
            List<int> ids = new List<int>();
            try {
                string tempqs = sniperClient.DownloadString("https://api.mojang.com/user/security/challenges");
                JArray qarr = JArray.Parse(tempqs);
                for (int i = 0; i < 3; i++)
                {
                    questions.Add((string)qarr[i]["question"]["question"]);
                    ids.Add((int)qarr[i]["answer"]["id"]);
                }
            } catch { 
                try
                {
                    string tempqs = sniperClient.DownloadString("https://api.mojang.com/user/security/location");
                    Console.WriteLine("[Info] Questions not needed. Press any key to return to the menu.");
                    Console.ReadKey();
                    return;
                } catch
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
            } catch
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
                Console.WriteLine(@"                                                 
                                                 
           ____                                  
          6MMMMb\         68b                    
         6M'    `         Y89                    
  ____   MM      ___  __  ___ __ ____     ____   
 6MMMMb  YM.     `MM 6MMb `MM `M6MMMMb   6MMMMb  
MM'  `Mb  YMMMMb  MMM9 `Mb MM  MM'  `Mb 6M'  `Mb 
      MM      `Mb MM'   MM MM  MM    MM MM    MM 
     .M9       MM MM    MM MM  MM    MM MMMMMMMM 
  MMMM         MM MM    MM MM  MM    MM MM       
     `Mb L    ,M9 MM    MM MM  MM.  ,M9 YM    d9 
      MM MYMMMM9 _MM_  _MM_MM_ MMYMMM9   YMMMM9  
      MM                       MM                
MM.  ,M9                       MM                
 YMMMM9                       _MM_              

");
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
                    case '1': snipe(); break;
                    case '2': configure(); break;
                    case '3': tools(); break;
                    case '4': System.Environment.Exit(0); break;
                    default: break;
                }
            }
        }
    }
}
