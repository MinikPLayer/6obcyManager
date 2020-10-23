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

        static void Main(string[] args)
        {
            SoundPlayer alert = new SoundPlayer("alert.wav");
            SoundPlayer error = new SoundPlayer("error.wav");

            using (ChromeDriver driver = new ChromeDriver())
            {
                //driver.Navigate("https://6obcy.org");
                driver.Url = "https://6obcy.org";

                driver.Navigate();

                var stateButton = driver.FindElementById("intro-interface-location-button");
                stateButton.Click();

                var malopolska = driver.FindElementByClassName("location-id-12");
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
                                textInput.SendKeys("Hej M19, Km?\n");
                            }
                            int count = chatBox.FindElements(By.ClassName("log-msg")).Count;

                            bool disconnected = false;
                            while (count == chatBox.FindElements(By.ClassName("log-msg")).Count)
                            {

                                if ((DateTime.Now - startTime).Seconds > 10)
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
