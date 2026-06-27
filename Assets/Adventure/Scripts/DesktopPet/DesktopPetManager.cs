using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Adventure.DesktopPet
{
    public class DesktopPetManager : MonoBehaviour
    {
        private struct MARGINS
        {
            public int left;
            public int right;
            public int top;
            public int bottom;
        }

        [DllImport("user32.dll")]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        [DllImport("user32.dll")]
        private static extern  IntPtr GetActiveWindow();
        
        [DllImport("Dwmapi.dll")]
        private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        
        [DllImport("user32.dll")]
        private static extern int SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        
        const int GWL_EXSTYLE = -20;
        const uint WS_EX_LAYERED = 0x00080000;
        const uint WS_EX_TRANSPARENT = 0x00000020;
        const uint LWA_COLORKEY = 0x00000001;
        
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private IntPtr hWnd;
        
        private void Start()
        {
#if !UNITY_EDITOR
            hWnd = GetActiveWindow();
            MARGINS margins = new MARGINS{left = -1};
            DwmExtendFrameIntoClientArea(hWnd, ref margins);
            SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
            // 窗口颜色为0的地方将作为透明
            SetLayeredWindowAttributes(hWnd, 0, 0, LWA_COLORKEY);
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, 0x0040);
#endif
            Application.runInBackground = true;
        }
    }
}
