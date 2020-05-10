using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin.Core
{
    class Tools
    {


        private static Random _Random = null;

        public static int GetNextInt(int Min = 1, int Max = 20)
        {
            if (Min < 0)
            {
                throw new Exception("Min Must be > 0");
            }
            return RandomTools.Next(Min, Max);
        }

        public static float GetNextFloat(int Min = 1, int Max = 20)
        {
            return (float)GetNextInt(Min, Max);
        }



        public static bool HavingChance(int NumberChance, int Total = 10)
        {
            if (NumberChance >= Total)
            {
                return true;
            }

            if (NumberChance < 0)
            {
                return false;
            }
            return NumberChance <= GetNextInt(1, Total + 1); // exclusive Max next int ^^ 
        }

        public static T GetRandomInArray<T>(T[] Array)
        {
            int RandomIndex = GetNextInt(1, Array.Length - 1);
            return Array[RandomIndex];
        }


        private static Random RandomTools
        {
            get
            {
                if (null == _Random)
                {
                    _Random = new Random();
                }

                return _Random;
            }
        }

    }
}
