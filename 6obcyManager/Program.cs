using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using System.Media;
using System.IO;

namespace _6obcyManager
{
    class Program
    {
        static void Disconnect(ChromeDriver driver, bool doubleClick = true)
        {
            var b = driver.FindElementByClassName("o-esc");

            var discMessage = driver.FindElementByClassName("log-disconnected");
            if(discMessage.GetAttribute("style").Contains("block"))
            {
                Console.WriteLine("Sleeping before reconnecting...");
                Thread.Sleep(new Random().Next(0, 5000));
                b.Click();
                return;
            }

            // Disconnect because the other person is still connected
            b.Click();
            b.Click();

            Console.WriteLine("Sleeping before reconnecting...");
            Thread.Sleep(new Random().Next(0, 5000));
            

            // Find someone new
            b.Click();

            //b.Click();
            //if(doubleClick)
            //    b.Click();


        }

        struct Response
        {
            public string source; // Response to?
            public string response; // What to respond
            public Types type;

            public enum Types
            {
                None,
                Command,
                Contains,
                Equals
            }

            public static Types ParseType(char c)
            {
                switch (c)
                {
                    // event
                    case '$':
                        return Types.Command;

                    case 'C':
                        return Types.Contains;

                    case '=':
                        return Types.Equals;

                    default:
                        return Types.None;
                }
            }
        }

        static List<Response> responses;

        static void ParseString(string line)
        {
            Response.Types type = Response.ParseType(line[0]);
            line = line.Remove(0, 1);

            string source, response;
            source = response = "";
            bool quote = false;
            bool part = false;
            for (int i = 0;i<line.Length;i++)
            {
                if(line[i] == '\"')
                {
                    quote = !quote;
                    continue;
                }
                if(!quote && line[i] == ':')
                {
                    part = true;
                    continue;
                }

                if(quote)
                {
                    if(part)
                    {
                        response += line[i];
                    }
                    else
                    {
                        source += line[i];
                    }
                }
            }

            responses.Add(new Response { source = source, response = response, type = type });
        }

        static void ParseStrings()
        {
            responses = new List<Response>();
            string[] lines = File.ReadAllLines("strings.txt");
            for(int i = 0;i<lines.Length;i++)
            { 
                ParseString(lines[i]);
            }
        }

        static Dictionary<string, string> regions = new Dictionary<string, string>
        {
            { "slask", "7" },
            { "malopolska", "12" },
        };

        static bool ParseConfigLine(string header, string data)
        {
            switch (header)
            {
                case "region":
                    try
                    {
                        regionStr = regions[data];
                    }
                    catch(Exception e)
                    {
                        Debug.FatalError("Region " + data + " not found in database", 1, 2000);
                    }
                    return true;

                default:
                    Debug.LogError("Unknown config line header " + header);
                    return false;
            }
        }

        static void LoadConfig()
        {
            string[] lines = File.ReadAllLines("config.ini");
            for(int i = 0;i<lines.Length;i++)
            {
                string[] parts = lines[i].Split('=');
                ParseConfigLine(parts[0], parts[1]);
            }
        }

        static string regionStr = "12"; // małopolska
        static void Main(string[] args)
        {
            //ParseStrings();

            SoundPlayer alert = new SoundPlayer("alert.wav");
            SoundPlayer error = new SoundPlayer("error.wav");

            string dir = Directory.GetCurrentDirectory();
            if(!File.Exists(Path.Combine(dir, "chromedriver.exe")))
            {
                dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "6obcy");
            }
            using (ChromeDriver driver = new ChromeDriver(dir))
            {
                //driver.Navigate("https://6obcy.org");
                driver.Url = "https://6obcy.org";

                driver.Navigate();

                var stateButton = driver.FindElementById("intro-interface-location-button");
                stateButton.Click();

                LoadConfig();

                var malopolska = driver.FindElementByClassName("location-id-" + regionStr);
                malopolska.Click();

                var startbutton = driver.FindElementById("intro-start");
                startbutton.Click();

                Thread.Sleep(1000);
                var textInput = driver.FindElementById("box-interface-input");

                var chatBox = driver.FindElementById("log-dynamic");

                var welcomeMessage = driver.FindElementById("log-begin");

                while (true)
                {



                    try
                    {
                        string style = welcomeMessage.GetAttribute("style");
                        while (style.Contains("none"))
                        {
                            Console.WriteLine("Waiting for someone...");
                            style = welcomeMessage.GetAttribute("style");
                            Thread.Sleep(500);

                        }
                        var startTime = DateTime.Now;
                        for (int i = 0; ; i++)
                        {


                            if (i > 0)
                            {

                                var messages = chatBox.FindElements(By.ClassName("log-msg"));
                                if (messages.Count != 0)
                                {
                                    string msg = chatBox.FindElements(By.ClassName("log-msg")).LastOrDefault().Text;
                                    if (!msg.ToLower().Contains("k") && msg.ToLower().Contains("m"))
                                    {
                                        while ((DateTime.Now - startTime).Seconds < 5)
                                        {
                                            Thread.Sleep(250);
                                        }

                                        Disconnect(driver);

                                        error.Play();
                                        break;
                                    }
                                    if (msg.ToLower().Contains("km"))
                                    {
                                        textInput.SendKeys("M\n");
                                    }
                                    else
                                    {
                                        alert.Play();

                                        while (true)
                                        {
                                            var disconnectMessage = driver.FindElementByClassName("log-disconnected");
                                            style = disconnectMessage.GetAttribute("style");
                                            if (style.Contains("block"))
                                            {
                                                Disconnect(driver, false);
                                                break;
                                            }

                                            Thread.Sleep(1000);
                                        }
                                        break;
                                    }
                                }

                            }
                            else
                            {
                                textInput.SendKeys("Hej, M19 Km?\n");
                            }
                            int count = chatBox.FindElements(By.ClassName("log-msg")).Count;

                            bool disconnected = false;
                            while (count == chatBox.FindElements(By.ClassName("log-msg")).Count)
                            {

                                if ((DateTime.Now - startTime).Seconds > 10 && driver.FindElement(By.Id("log-stranger-typing")).GetAttribute("style") == "display: none;")
                                {
                                    Disconnect(driver);
                                    disconnected = true;
                                    break;
                                }

                                var disconnectMessage = driver.FindElementByClassName("log-disconnected");
                                style = disconnectMessage.GetAttribute("style");
                                if (style.Contains("block"))
                                {
                                    Disconnect(driver, false);
                                    disconnected = true;
                                    break;
                                }
                                Thread.Sleep(500);
                                //Console.WriteLine("Count: " + count.ToString());
                            }

                            if (disconnected) break;
                        }
                    }
                    catch(Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.ToString());
                        Console.ResetColor();
                    }
                }

                Console.WriteLine("Waiting for keypress...");
                Console.ReadKey();
            }
        }
    }
}
