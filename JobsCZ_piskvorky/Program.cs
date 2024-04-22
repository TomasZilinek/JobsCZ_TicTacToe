using JobsCZ_piskvorky.AI;
using JobsCZ_piskvorky.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JobsCZ_piskvorky
{
    [FlagsAttribute]
    public enum EXECUTION_STATE : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001
        // Legacy flag, should not be used.
        // ES_USER_PRESENT = 0x00000004
    }

    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        static /*async Task*/ void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool playOnline = true;
            Enums.PlayerIdentityEnum playerIdentity = Enums.PlayerIdentityEnum.DiscordAdNew;

            if (playOnline)
            {
                bool once = false;

                if (once)
                {
                    Bot bot = new Bot(playerIdentity);
                    /*await*/ bot.Run();
                }
                else
                {
                    Console.Write("Roker interceptor? (y/n): ");
                    bool rokerInterceptor = Console.ReadLine() == "y";
                    int rounds = 0;

                    if (rokerInterceptor)
                        playerIdentity = Enums.PlayerIdentityEnum.RokerInterceptor;

                    while (true)
                    {
                        rounds++;

                        Console.WriteLine();
                        Bot bot = new Bot(playerIdentity);
                        /*await*/ bot.Run();

                        if (playerIdentity == Enums.PlayerIdentityEnum.RokerInterceptor)
                            Console.WriteLine($"Roker intercepted {rounds} times");

                        Thread.Sleep(1000);
                    }
                }
            }
            else
            {
                // SkuskaEvaluationFunctionManual();
                // PlayAgainstAI();
                // SendFeedback();
                UserRegistration();
            }

            Console.WriteLine("\nType anything (and press ENTER) to end the program... ");
            Console.ReadLine();
        }
        
        static void PlayAgainstAI()
        {
            /*
            MyTimer.CreateStopWatch("EvaluationFunction");
            MyTimer.CreateStopWatch("SetsStraight");
            MyTimer.CreateStopWatch("SetsDiagonal");
            MyTimer.CreateStopWatch("Overall");
            MyTimer.CreateStopWatch("Iteration");
            */

            ShowForm(new PlayAgainstAIForm(new MiniMaxAI()));
        }

        static void UserRegistration()
        {
            string nickName = "adwdawdawehhhhd";
            string email = "d@d.s";

            Bot bot = new Bot(Enums.PlayerIdentityEnum.Overmind);
            bot.UserRegistration(nickName, email, out JsonUserResponse userResponse);

            if (userResponse != null)
                Console.WriteLine($"User created\nuserId = {userResponse.userId}\nuserToken = {userResponse.userToken}");
        }

        static void SendFeedback()
        {
            string message = "";
            Bot bot = new Bot(Enums.PlayerIdentityEnum.Overmind);

            bot.SendFeedback(message);
        }

        [STAThread]
        static void ShowForm(Form form)
        {
            Application.Run(form);
        }
    }
}
