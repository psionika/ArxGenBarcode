using System.Runtime.InteropServices;
using System.Threading;

namespace ArxGenBarcode.Helpers
{
    public static class ForKeyboard
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public static void PressKeysToScissors()
        {
            const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
            const int KEYEVENTF_KEYUP = 0x0002; //Key up flag

            const int VK_LWIN = 0x5B;
            const int VK_SHIFT = 0x10;
            const int S_KEY = 0x53;

            Thread.Sleep(100);
            keybd_event(VK_LWIN, 0, KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event(VK_SHIFT, 0, KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event(S_KEY, 0, KEYEVENTF_EXTENDEDKEY, 0);
            Thread.Sleep(100);
            keybd_event(S_KEY, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, 0);
            Thread.Sleep(100);
        }
    }
}
