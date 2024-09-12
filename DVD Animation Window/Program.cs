using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

public class WindowBouncer
{

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    #region DLL Imports
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr GetShellWindow();
    #endregion

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    static List<Tuple<IntPtr, string>> GetOpenWindows()
    {
        var windows = new List<Tuple<IntPtr, string>>();
        IntPtr shellWindow = GetShellWindow();
        EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
        {
            const int maxTitleLength = 255;
            System.Text.StringBuilder windowTitle = new System.Text.StringBuilder(maxTitleLength);
            GetWindowText(hWnd, windowTitle, maxTitleLength);

            if (IsWindowVisible(hWnd) && hWnd != shellWindow && !string.IsNullOrEmpty(windowTitle.ToString()))
            {
                windows.Add(Tuple.Create(hWnd, windowTitle.ToString()));
            }
            return true;
        }, IntPtr.Zero);
        return windows;
    }
    static void Main()
    {
        var windows = GetOpenWindows();
        if (windows.Count == 0)
        {
            Console.WriteLine("No windows found.");
            return;
        }

        Console.WriteLine("Select a window to animate:");
        for (int i = 0; i < windows.Count; i++)
        {
            Console.WriteLine($"{i + 1}: {windows[i].Item2}");
        }

        int selectedIndex = 0;

        while (true)
        {
            Console.Write("Enter the number of the window: ");
            if (int.TryParse(Console.ReadLine(), out selectedIndex) && selectedIndex > 0 && selectedIndex <= windows.Count)
            {
                break;
            }
            Console.WriteLine("Invalid selection. Please try again.");
        }

        IntPtr hWnd = windows[selectedIndex - 1].Item1;

        if (hWnd == IntPtr.Zero)
        {
            Console.WriteLine("Window not found!");
            return;
        }
        else
        {
            Console.WriteLine($"Window found");
        }

        RECT rect = new RECT();
        if (!GetWindowRect(hWnd, ref rect))
        {
            Console.WriteLine("Failed to get window rect!");
            return;
        }
        else
        {
            Console.WriteLine("Found window rect");
        }

        int windowWidth = rect.Right - rect.Left;
        int windowHeight = rect.Bottom - rect.Top;
        Point windowPos = new Point(rect.Left, rect.Top);
        Point velocity = new Point(10, 10);

        Rectangle screenBounds = Screen.PrimaryScreen.Bounds;

        while (true)
        {
            windowPos.X += velocity.X;
            windowPos.Y += velocity.Y;

            if (windowPos.X < 0 || windowPos.X + windowWidth > screenBounds.Width)
                velocity.X *= -1;
            if (windowPos.Y < 0 || windowPos.Y + windowHeight > screenBounds.Height)
                velocity.Y *= -1;

            MoveWindow(hWnd, windowPos.X, windowPos.Y, windowWidth, windowHeight, true);

            // Controls animation speed
            Thread.Sleep(20);
        }
    }
}
