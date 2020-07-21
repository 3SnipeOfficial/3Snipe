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

NOTICE: Use an access token from 45 minutes or less ago.

");
            menu();
        }
        private static void snipe()
        {
            DateTime dropTime;
            Console.Clear();
            Console.WriteLine("Enter access token (bearer_token cookie) and press enter: ");
            string accessToken = Console.ReadLine();
            Console.WriteLine("Enter your Minecraft account password and press enter: ");
            string password = Console.ReadLine();
            string userUUID = "";
            Console.WriteLine("Enter name to snipe and press enter: ");
            string name = Console.ReadLine();
            WebClient sniperClient = new WebClient();
            Console.WriteLine();
            try
            {
                int l = 0;
                Console.WriteLine(l++);
                JObject yeet = JObject.Parse(sniperClient.DownloadString($"https://api.mojang.com/users/profiles/minecraft/" + name + "?at=" + (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3196800)));
                Console.WriteLine(l++);
                string oldOwnerID = (string)yeet["id"];
                Console.WriteLine(l++);
                JArray yeet2 = JArray.Parse(sniperClient.DownloadString($"https://api.mojang.com/user/profiles/" + oldOwnerID + "/names"));
                Console.WriteLine(l++);
                List<int> indexList = new List<int>();
                for (int i = 0; i < yeet2.Count; i++)
                {
                    string name2 = (string)yeet2[i]["name"];
                    if (name.ToLower() == name2.ToLower())
                        indexList.Add(i);
                    i++;
                }
                Console.WriteLine(l++);
                int lastIndex = indexList[indexList.Count - 1] + 1;
                Console.WriteLine(l++);
                dropTime = DateTimeOffset.FromUnixTimeMilliseconds((long)yeet2[lastIndex]["changedToAt"] + 3196800000).ToLocalTime().DateTime;
                Console.WriteLine(l++);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Error] This name is not dropping or has already dropped. Press any key to return to menu.");
                Console.ResetColor();
                Console.ReadKey();
                return;
            }
            sniperClient.Headers[HttpRequestHeader.Authorization] = $"Bearer {accessToken}";
            string payload = "{'name': '" + name + "', 'password': '" + password + "'}";

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
                    string response = sniperClient.UploadString("https://api.mojang.com/user/profile/" + userUUID + "/name", payload);
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
                Thread.Sleep(5000);
            } catch { }
            if (snipedAlready)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success. Set name to " + name + ". Press any key to return to menu.");
                Console.ResetColor();
                Console.ReadKey();
                Console.Clear();
                return;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed snipe. Press any key to return to menu.");
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
            Console.WriteLine("Under construction. Press a key to go back.");
            Console.ReadKey();
            Console.Clear();
            return;
        }
        private static void menu()
        {
            for (; ; )
            {
                Console.WriteLine(@"Welcome to 3Snipe CORE 1.0.0!
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
                Console.Clear();
            }
        }
    }
}
