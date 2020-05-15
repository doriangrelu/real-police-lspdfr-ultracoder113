using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealPolicePlugin.Core
{
    class KeysManager
    {
        public static bool IsKeyCombinationDownComputerCheck(Keys MainKey, Keys ModifierKey)
        {
            if (MainKey != Keys.None)
            {
                return KeysManager.IsKeyDownComputerCheck(MainKey) && (KeysManager.IsKeyDownRightNowComputerCheck(ModifierKey)
                || (ModifierKey == Keys.None && !KeysManager.IsKeyDownRightNowComputerCheck(Keys.Shift) && !KeysManager.IsKeyDownRightNowComputerCheck(Keys.Control)
                && !KeysManager.IsKeyDownRightNowComputerCheck(Keys.LControlKey) && !KeysManager.IsKeyDownRightNowComputerCheck(Keys.LShiftKey)));
            }
            else
            {
                return false;
            }
        }

        public static bool IsKeyDownComputerCheck(Keys KeyPressed)
        {
            if (Rage.Native.NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>() != 0)
            {
                return Game.IsKeyDown(KeyPressed);
            }
            return false;

        }

        public static bool IsKeyDownRightNowComputerCheck(Keys KeyPressed)
        {
            if (Rage.Native.NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>() != 0)
            {
                return Game.IsKeyDownRightNow(KeyPressed);
            }
            return false;
        }

    }
}
