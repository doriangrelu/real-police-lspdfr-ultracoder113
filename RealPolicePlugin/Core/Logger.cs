using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API; 

namespace RealPolicePlugin.Core
{
    class Logger
    {
        /// <summary>
        /// Log Trivial using Game function From RPH, and display or no notification in Game. 
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="DisplayNotification"></param>
        public static void Log(string Message, bool DisplayNotification = false)
        {
            Logger.LogTrivial(Message);
            if (DisplayNotification)
            {
                Logger.DisplayNotification(Message);
            }
        }


        public static void LogTrivial(string Message)
        {
            if (false == Main.IN_PRODUCTION)
            {
                Game.LogTrivial("Real Police Plugin By Ultracoder113: " + Message); 
            }
        }

        public static uint DisplayNotification(string Message)
        {
            return Game.DisplayNotification(Message);  
        }
    }
}
