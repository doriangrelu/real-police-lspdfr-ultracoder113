using RealPolicePlugin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin.GameManager
{
    class LicensePlate
    {


        private static String[] INVALID_PLATE_LETTERS = { "FOR", "AXE", "JAM", "JAB", "ZIP", "ARE", "YOU",
            "JUG", "JAW", "JOY" };


        public static String GenerateLicensePlate()
        {
            String licensePlate;
            String letters;
            do
            {
                letters = GenerateLetters(3);
            } while (IllegalWord(letters));

            String digits = GenerateDigits(3);

            licensePlate = letters + "-" + digits;
            return licensePlate;
        }


        private static String GenerateLetters(int amount)
        {
            String letters = "";
            
            for (int i = 0; i < amount; i++)
            {
                int num = Tools.GetNextInt(0, 26);
                char letter = (char)('A' + num);
                letters += letter; 
            }
            return letters;
        }

        private static String GenerateDigits(int amount)
        {
            String digits = "";
            for (int i = 0; i < amount; i++)
            {
                char c = (char)Tools.GetNextInt(0, 10);
                digits += c;
            }
            return digits;
        }

      

        private static bool IllegalWord(String letters)
        {
            for (int i = 0; i < INVALID_PLATE_LETTERS.Length; i++)
            {
                if (letters.Equals(INVALID_PLATE_LETTERS[i]))
                {
                    return true;
                }
            }
            return false;
        }

     
    }
}
