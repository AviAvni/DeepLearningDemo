using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WindowsInput;
using WindowsInput.Native;

namespace DeepLearningDemo.MarioKart
{
    public static class InterceptKeys
    {
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static LowLevelKeyboardProc proc = HandleKeys;
        private static IntPtr hookID;
        private static List<IObserver<(VirtualKeyCode, bool)>> obss = new List<IObserver<(VirtualKeyCode, bool)>>();

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SETTEXT = 0x000C;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("User32.dll")]
        private static extern int SetForegroundWindow(IntPtr point);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindows);

        [DllImport("User32.dll")]
        private static extern Int32 SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, StringBuilder lParam);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static IObservable<(VirtualKeyCode, bool)> CaptureKeys()
        {
            return Observable.Create<(VirtualKeyCode, bool)>(obs =>
            {
                obss.Add(obs);

                if (hookID == IntPtr.Zero)
                {
                    using (Process curProcess = Process.GetCurrentProcess())
                    using (ProcessModule curModule = curProcess.MainModule)
                    {
                        hookID = SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                            GetModuleHandle(curModule.ModuleName), 0);
                    }
                }

                return Disposable.Create(() =>
                {
                    obss.Remove(obs);
                    if (obss.Count == 0)
                    {
                        UnhookWindowsHookEx(hookID);
                    }
                });
            });
        }

        public static IntPtr HandleKeys(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                for (int i = 0; i < obss.Count; i++)
                {
                    var obs = obss[i];
                    obs.OnNext(((VirtualKeyCode)vkCode, true));
                }
            }

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                for (int i = 0; i < obss.Count; i++)
                {
                    var obs = obss[i];
                    obs.OnNext(((VirtualKeyCode)vkCode, false));
                }
            }

            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        public static void SendKeys()
        {
            IntPtr hWnd = FindWindow("Project64 2.3.2.202", null);
            var sim = new InputSimulator();

            if (!hWnd.Equals(IntPtr.Zero))
            {
                SetForegroundWindow(hWnd);
                ShowWindow(hWnd, SW_RESTORE); //Maximizes Window in case it was minimized.
                sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

                while (true)
                {
                    sim.Keyboard.KeyDown(VirtualKeyCode.VK_W);
                    System.Threading.Thread.Sleep(900);
                    sim.Keyboard.KeyUp(VirtualKeyCode.VK_W);
                    System.Threading.Thread.Sleep(500);

                }
            }
        }
    }
}