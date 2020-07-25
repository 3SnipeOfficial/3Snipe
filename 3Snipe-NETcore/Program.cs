using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Data.SqlTypes;

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
        //static readonly string vCode = "v1.1.0-beta.8";
        static readonly string vCode = "v1.1.0-beta.9";
        static object lockObj = new object();
        static bool snipedAlready = false;
        static readonly string willToLive = null;


        static void Main(string[] args)
        {



            menu();
        }
        static void branding()
        {

            String userDefinedColor;
            try { 
            //Pass the file path and file name to the StreamReader constructor
            StreamReader sr = new StreamReader("config.txt");
            //Read the first line of text
            userDefinedColor = sr.ReadLine();
                if (userDefinedColor == null) {
                    userDefinedColor = "Gray";
                }
            Console.WriteLine(userDefinedColor);
            sr.Close();
        }
catch(Exception e)
{
                Console.WriteLine("Exception: " + e.Message);
                userDefinedColor = "Gray";
            }
finally
{
                Console.WriteLine("Executing finally block.");
                Console.Clear();
}
            // AHHHH


            string[] splashMsgs = { "Déjà vu!",
                    "Doesn't use the U-word!",
                    "doot doot",
                    "From free range developers!",
                    "if not ok then return end",
                    "Now with 10% less fat!",
                    "sqrt(-1) love you!",
                    "Multithreaded!",
                    "Privet Russia!",
                    "Cough or sneeze into your elbow!",
                    "Don't skid!",
                    "MIT Licensed!",
                    "fastsnipe who?",
                    ".NET 3.1!",
                    "200 members!",
                    "Established 2020!",
                    "The Work of Notch",
                    "110813!",
                    "Tip: to disable this text, run rm -rf  /",
                    "3name users are not welcome in this realm.",
                    "alex is best dev"
                    };

             
            Random rand = new Random();
            
            int index = rand.Next(splashMsgs.Length);
           

            Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), userDefinedColor, true);
            Console.WriteLine($@"                                                 
██████╗ ███████╗███╗   ██╗██╗██████╗ ███████╗
╚════██╗██╔════╝████╗  ██║██║██╔══██╗██╔════╝
 █████╔╝███████╗██╔██╗ ██║██║██████╔╝█████╗  
 ╚═══██╗╚════██║██║╚██╗██║██║██╔═══╝ ██╔══╝  
██████╔╝███████║██║ ╚████║██║██║     ███████╗
╚═════╝ ╚══════╝╚═╝  ╚═══╝╚═╝╚═╝     ╚══════╝

{vCode}

{splashMsgs[index]}

");
        }
        private static void snipe()
        {
            DateTime dropTime;
            Console.Clear();
            string email = "";
            string password = "";
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
            string name = Console.ReadLine();
            WebClient sniperClient = new WebClient();
            sniperClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0");
            Console.WriteLine();
            try
            {
                JObject tempObj = JObject.Parse(sniperClient.DownloadString($"https://api.mojang.com/users/profiles/minecraft/" + name + "?at=" + (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3196800)));
                string oldOwnerID = (string)tempObj["id"];
                JArray tempArr = JArray.Parse(sniperClient.DownloadString($"https://api.mojang.com/user/profiles/" + oldOwnerID + "/names"));
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
                Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(600000));
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
                return;
            }
            string f16 = accessToken.Substring(0, 16);
            Console.WriteLine($"[Info] Got token. First 16 characters are {f16}");
            sniperClient.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {accessToken}");
            string payload = "{'name': '" + name + "', 'password': '" + password + "'}";
            try
            {
                Console.WriteLine("[Info] Readying token for usage...");
                string tempStr = sniperClient.DownloadString("https://api.mojang.com/user/security/challenges");
                if (tempStr == "[]")
                {
                    Console.WriteLine("[Info] Readied token for usage.");
                } else
                {
                    throw new Exception("Questions are unanswered.");
                }
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
            var temp = sniperClient.DownloadString("https://api.mojang.com/user/profiles/agent/minecraft");
            Console.WriteLine(temp);
            userUUID = (string)JObject.Parse(temp.Substring(1, temp.Length - 2))["id"];
            Console.WriteLine(userUUID);
            List<Thread> threads = new List<Thread>();
            void sniperthread(object info)
            {
                int delay = ((ThreadInfo)info).ThreadID;
                WebClient sniperClient2 = new WebClient();
                sniperClient2.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {accessToken}");
                sniperClient2.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0");
                Console.WriteLine(sniperClient2.Headers);
                try
                {
                    Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(25) + TimeSpan.FromMilliseconds(delay * 5));
                }
                catch { }
                for (int i = 0; i < 4; i++)
                {
                    if (snipedAlready)
                        return;
                    try
                    {
                        string response = sniperClient2.UploadString("https://api.mojang.com/user/profile/" + userUUID + "/name", payload);
                        if (response != string.Empty)
                            Console.WriteLine($"[Info] Got status code of 2XX on a thread, request number {i}.");
                        else
                            Console.WriteLine($"[Info] Got status code of 204 on a thread, request number {i}.");
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
                Thread.Sleep(dropTime - DateTime.Now);
            } catch { }
            try
            {
                Thread.Sleep(20000);
            } catch { }
            accessToken = "Disposed.";
            password = "Disposed.";
            payload = "Disposed.";
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
            string name = Console.ReadLine();
            WebClient sniperClient = new WebClient();
            sniperClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0");
            Console.WriteLine();
            try
            {
                JObject tempObj = JObject.Parse(sniperClient.DownloadString($"https://api.mojang.com/users/profiles/minecraft/" + name + "?at=" + (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3196800)));
                string oldOwnerID = (string)tempObj["id"];
                JArray tempArr = JArray.Parse(sniperClient.DownloadString($"https://api.mojang.com/user/profiles/" + oldOwnerID + "/names"));
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
                Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(600000));
            }
            catch { }
            string emailSniped = "";
            void acctThread(object user2)
            {
                WebClient authClient = new WebClient();
                UserInfo user = (UserInfo)user2;
                string accessToken = "";
                try
                {
                    string tokenResponse = "";
                    tokenResponse = authClient.UploadString("https://authserver.mojang.com/authenticate", $"{{\"agent\": {{\"name\": \"Minecraft\", \"version\": 1}},\"username\": \"{user.Email}\", \"password\": \"{user.Password}\"}}");
                    accessToken = (string)JObject.Parse(tokenResponse)["accessToken"];
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Error] An account provided is invalid. Continuing execution without account.");
                    Console.ResetColor();
                    Console.ReadKey();
                    return;
                }
                string f16 = accessToken.Substring(0, 16);
                Console.WriteLine($"[Info] Got token. First 16 characters are {f16}");
                string payload = "{'name': '" + name + "', 'password': '" + user.Password + "'}";
                try
                {
                    Console.WriteLine("[Info] Readying token for usage...");
                    authClient.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {accessToken}");
                    string tempStr = authClient.DownloadString("https://api.mojang.com/user/security/challenges");
                    if (tempStr == "[]")
                    {
                            Console.WriteLine("[Info] Readied token for usage.");
                    }
                    else
                    {
                        throw new Exception("Questions are unanswered.");
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Error] Security questions are unanswered (use the tool for this). Continuing execution.");
                    Console.ResetColor();
                    return;
                }
                string userUUID = "";
                string temp = "";
                authClient.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {accessToken}");
                temp = authClient.DownloadString("https://api.mojang.com/user/profiles/agent/minecraft");
                userUUID = (string)JObject.Parse(temp.Substring(1, temp.Length - 2))["id"];
                
                List<Thread> threads = new List<Thread>();
                void sniperthread(object info)
                {
                    int delay = ((ThreadInfo)info).ThreadID;
                    WebClient sniperClient2 = new WebClient();
                    sniperClient2.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {accessToken}");
                    sniperClient2.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0");
                    try
                    {
                        Thread.Sleep(dropTime - DateTime.Now - TimeSpan.FromMilliseconds(2.5 * accounts.Count) + TimeSpan.FromMilliseconds(delay * 5));
                    }
                    catch { }
                    for (int i = 0; i < 4; i++)
                    {
                        if (snipedAlready)
                            return;
                        try
                        {
                            string response = sniperClient2.UploadString("https://api.mojang.com/user/profile/" + userUUID + "/name", payload);
                            if (response != string.Empty)
                                Console.WriteLine($"[Info] Got status code of 2XX on a thread, request number {i}.");
                            else
                                Console.WriteLine($"[Info] Got status code of 204 on a thread, request number {i}.");
                            snipedAlready = true;
                            emailSniped = user.Email;
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
                payload = "Disposed.";
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
                Console.WriteLine("Success. Set name to " + name + " on account " + emailSniped + ". Press any key to return to the menu.");
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

            for (; ; )
            {
                branding();
                Console.Clear();
                Console.WriteLine(@"Tools:
1) Fun
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
                    case '1': funOptions(); break;
                    case '2': Console.Clear(); return;
                    default: break;
                }
            }
        }


        private static void funOptions()
        {

            for (; ; )
            {
                branding();
                Console.Clear();
                Console.WriteLine(@"Tools:
1) Colors
2) Back
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
                    case '1': colorOptions(); break;
                    case '2': Console.Clear(); return;
                    default: break;
                }
            }
        }


        private static void colorOptions()
        {
            
            while (69 != 420)
            {
                branding();
                Console.Clear();
                Console.WriteLine(@"Colors:
1) dark blue
2) dark green
3) dark cyan
4) dark red
5) dark purple
6) dark yellow
7) gray (default)
8) dark gray
9) blue
a) green (disabled, reserved for success message)
b) cyan
c) red (disabled, reserved for failure message)
d) magenta
e) yellow
f) white
t) Back
");
                char option = Console.ReadKey().KeyChar;
                char[] options = { '1', '2', '3', '4', '5', '6', '7', '8', '9', /*'a',*/ 'b', /*'c',*/ 'd', 'e', 'f', 't' };
                while (!options.Contains(option))
                {
                    Console.Write("\r \r");
                    option = Console.ReadKey().KeyChar;
                }
                switch (option)
                {
                    case '1': setColor("DarkBlue"); break;
                    case '2': setColor("DarkGreen"); break;
                    case '3': setColor("DarkCyan"); break;
                    case '4': setColor("DarkRed"); break;
                    case '5': setColor("DarkMagenta"); break;
                    case '6': setColor("DarkYellow"); break;
                    case '7': setColor("Gray"); break;
                    case '8': setColor("DarkGray"); break;
                    case '9': setColor("Blue"); break;
                    case 'a': setColor("Green"); break;
                    case 'b': setColor("Cyan"); break;
                    case 'c': setColor("Red"); break;
                    case 'd': setColor("Magenta"); break;
                    case 'e': setColor("Yellow"); break;
                    case 'f': setColor("White"); break;
                    case 't': Console.Clear(); return;
                    default: break;
                }
                   
            }
        }

        private static void setColor(string painIsMyLife)
        {

            for (; ; )
            {
                StreamWriter sw = new StreamWriter("config.txt");
                
                
                sw.WriteLine(painIsMyLife);
                sw.Close();
                Console.Clear();
                Console.WriteLine(@"Color set!
Please restart the software.
1) Reinitialize");
                char option = Console.ReadKey().KeyChar;
                char[] options = {'1'};
                while (!options.Contains(option))
                {
                    Console.Write("\r \r");
                    option = Console.ReadKey().KeyChar;
                }
                switch (option)
                {
                    case '1': { Console.Clear();
                            menu();
                            String plsSend;
                            String Help;
                            plsSend = "i am hurting. please send food and water i am dying. ";
                            Help = "tyler has me locked up in this dungeon and won't let me out";
                            Console.WriteLine(plsSend + Help);
                              Console.Clear();
                            return; }
                }
            }
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
                    string tempqs = sniperClient.DownloadString("https://api.mojang.com/user/security/challenges");
                    if (tempqs == "[]")
                    {
                        Console.WriteLine("[Info] Questions not needed. Press any key to return to the menu.");
                        Console.ReadKey();
                        return;
                    }
                    else { throw new Exception("Questions needed, but response invalid."); }
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
                    case '1': snipe(); break;
                    case '2': configure(); break;
                    case '3': tools(); break;
                    case '4': System.Environment.Exit(0); break;
                    default: break;
                }
                snipedAlready = false;
            }
        }
    }
}
