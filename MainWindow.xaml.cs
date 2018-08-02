using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace EmbeddedWindow
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        //Sets window attributes
        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        //Gets window attributes
        [DllImport("USER32.DLL")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private Process pDocked;
        private IntPtr hWndOriginalParent;
        private IntPtr hWndDocked;
        public System.Windows.Forms.Panel pannel;

        public static int GWL_STYLE = -16;
        public static int WS_BORDER = 0x00800000; //window with border
        public static int WS_DLGFRAME = 0x00400000; //window with double border but no title
        public static int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar

        public MainWindow()
        {
            InitializeComponent();
            pannel = new System.Windows.Forms.Panel();
            host.Child = pannel;
            dockIt("notepad");
        }

        private void dockIt(string utility)
        {
            //Don't do anything if there already a window docked.
            if (hWndDocked != IntPtr.Zero) 
                return;

            pDocked = Process.Start(utility);
            while (hWndDocked == IntPtr.Zero)
            {
                //Wait for the window to be ready for input;
                pDocked.WaitForInputIdle(1000);
                //Update process info
                pDocked.Refresh();
                //Abort if the process finished before we got a handle.
                if (pDocked.HasExited)
                {
                    return;
                }

                //Cache the window handle
                hWndDocked = pDocked.MainWindowHandle;

                //Remove borders
                int style = GetWindowLong(hWndDocked, GWL_STYLE);
                SetWindowLong(hWndDocked, GWL_STYLE, (style & ~WS_CAPTION));
            }
            //Windows API call to change the parent of the target window.
            //It returns the hWnd of the window parent prior to this call.
            hWndOriginalParent = SetParent(hWndDocked, pannel.Handle);

            //Wire up the event to keep the window sized to match the control
            SizeChanged += window_SizeChanged;

            //Perform an initial call to set the size.
            AlignToPanel();
        }

        private void AlignToPanel()
        {
            MoveWindow(hWndDocked, 0, 0, pannel.Width, pannel.Height, true);
        }

        void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AlignToPanel();
        }

    }
}
