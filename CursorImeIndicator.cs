using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace CursorImeIndicator
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool created;
            using (new Mutex(true, "CursorImeIndicator.SingleInstance", out created))
            {
                if (!created)
                    return;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new IndicatorContext());
            }
        }
    }

    internal sealed class IndicatorContext : ApplicationContext
    {
        private readonly IndicatorForm indicatorForm;
        private readonly System.Windows.Forms.Timer timer;
        private readonly NotifyIcon trayIcon;
        private readonly ToolStripMenuItem enabledItem;
        private readonly ToolStripMenuItem stateItem;
        private Icon currentTrayIcon;
        private bool enabled = true;
        private string lastText = "";

        public IndicatorContext()
        {
            indicatorForm = new IndicatorForm();

            enabledItem = new ToolStripMenuItem("커서 옆 표시 켜기");
            enabledItem.Checked = true;
            enabledItem.CheckOnClick = true;
            enabledItem.CheckedChanged += OnEnabledChanged;

            stateItem = new ToolStripMenuItem("현재 상태: 확인 중");
            stateItem.Enabled = false;

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add(enabledItem);
            menu.Items.Add(stateItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("종료", null, OnExit));

            currentTrayIcon = IconFactory.Create("한");
            trayIcon = new NotifyIcon();
            trayIcon.Icon = currentTrayIcon;
            trayIcon.Text = "한/En 마우스 표시기";
            trayIcon.ContextMenuStrip = menu;
            trayIcon.Visible = true;
            trayIcon.MouseDoubleClick += OnTrayDoubleClick;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 30;
            timer.Tick += OnTimerTick;
            timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            string text = ImeStateReader.GetIndicatorText();
            stateItem.Text = "현재 상태: " + text;

            if (text != lastText)
            {
                lastText = text;
                indicatorForm.SetIndicatorText(text);
                ReplaceTrayIcon(text);
            }

            if (!enabled)
                return;

            Point cursor = Cursor.Position;
            Rectangle area = Screen.FromPoint(cursor).WorkingArea;
            int x = cursor.X + 16;
            int y = cursor.Y + 2;

            if (x + indicatorForm.Width > area.Right)
                x = cursor.X - indicatorForm.Width - 8;
            if (y + indicatorForm.Height > area.Bottom)
                y = area.Bottom - indicatorForm.Height;
            if (y < area.Top)
                y = area.Top;

            indicatorForm.MoveWithoutActivating(x, y);
            if (!indicatorForm.Visible)
                indicatorForm.ShowWithoutStealingFocus();
        }

        private void OnEnabledChanged(object sender, EventArgs e)
        {
            enabled = enabledItem.Checked;
            if (!enabled)
                indicatorForm.Hide();
        }

        private void OnTrayDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                enabledItem.Checked = !enabledItem.Checked;
        }

        private void OnExit(object sender, EventArgs e)
        {
            timer.Stop();
            trayIcon.Visible = false;
            indicatorForm.Hide();
            Application.Exit();
        }

        private void ReplaceTrayIcon(string text)
        {
            Icon oldIcon = currentTrayIcon;
            currentTrayIcon = IconFactory.Create(text);
            trayIcon.Icon = currentTrayIcon;
            if (oldIcon != null)
                oldIcon.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (timer != null)
                    timer.Dispose();
                if (trayIcon != null)
                    trayIcon.Dispose();
                if (currentTrayIcon != null)
                    currentTrayIcon.Dispose();
                if (indicatorForm != null)
                    indicatorForm.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    internal sealed class IndicatorForm : Form
    {
        private const int WidthValue = 34;
        private const int HeightValue = 24;
        private readonly Font textFont;
        private string indicatorText = "한";

        public IndicatorForm()
        {
            textFont = new Font("Malgun Gothic", 9.5f, FontStyle.Bold, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            Width = WidthValue;
            Height = HeightValue;
            Opacity = 0.92d;
            BackColor = Color.FromArgb(28, 32, 38);
            DoubleBuffered = true;
            UpdateRegion();
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= NativeMethods.WS_EX_TOOLWINDOW;
                cp.ExStyle |= NativeMethods.WS_EX_NOACTIVATE;
                cp.ExStyle |= NativeMethods.WS_EX_TRANSPARENT;
                cp.ExStyle |= NativeMethods.WS_EX_LAYERED;
                return cp;
            }
        }

        public void SetIndicatorText(string text)
        {
            if (indicatorText == text)
                return;

            indicatorText = text;
            Invalidate();
        }

        public void ShowWithoutStealingFocus()
        {
            if (!Visible)
                Show();

            NativeMethods.SetWindowPos(
                Handle,
                NativeMethods.HWND_TOPMOST,
                Left,
                Top,
                Width,
                Height,
                NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
        }

        public void MoveWithoutActivating(int x, int y)
        {
            if (Left != x || Top != y)
                Location = new Point(x, y);

            if (!Visible)
                return;

            NativeMethods.SetWindowPos(
                Handle,
                NativeMethods.HWND_TOPMOST,
                x,
                y,
                Width,
                Height,
                NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateRegion();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            bool korean = indicatorText == "한";
            Color fill = korean ? Color.FromArgb(24, 128, 91) : Color.FromArgb(38, 78, 140);

            using (GraphicsPath path = CreateRoundRectangle(rect, 6))
            using (SolidBrush brush = new SolidBrush(fill))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            using (StringFormat format = new StringFormat())
            {
                e.Graphics.FillPath(brush, path);
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(indicatorText, textFont, textBrush, rect, format);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && textFont != null)
                textFont.Dispose();

            base.Dispose(disposing);
        }

        private void UpdateRegion()
        {
            using (GraphicsPath path = CreateRoundRectangle(new Rectangle(0, 0, Width, Height), 6))
            {
                Region oldRegion = Region;
                Region = new Region(path);
                if (oldRegion != null)
                    oldRegion.Dispose();
            }
        }

        private static GraphicsPath CreateRoundRectangle(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }

    internal static class ImeStateReader
    {
        private const int KoreanPrimaryLanguageId = 0x12;
        private const int ImeCmodeNative = 0x0001;
        private const int WmImeControl = 0x0283;
        private const int ImcGetConversionMode = 0x0001;
        private const int ImcGetOpenStatus = 0x0005;

        public static string GetIndicatorText()
        {
            return IsKoreanInputMode() ? "한" : "en";
        }

        private static bool IsKoreanInputMode()
        {
            IntPtr foreground = NativeMethods.GetForegroundWindow();
            if (foreground == IntPtr.Zero)
                return false;

            uint processId;
            uint threadId = NativeMethods.GetWindowThreadProcessId(foreground, out processId);
            IntPtr keyboardLayout = NativeMethods.GetKeyboardLayout(threadId);
            int languageId = (int)(keyboardLayout.ToInt64() & 0xffff);

            if ((languageId & 0x03ff) != KoreanPrimaryLanguageId)
                return false;

            IntPtr focusWindow = GetFocusedWindow(threadId, foreground);
            return IsNativeImeMode(focusWindow) || (focusWindow != foreground && IsNativeImeMode(foreground));
        }

        private static IntPtr GetFocusedWindow(uint threadId, IntPtr fallback)
        {
            NativeMethods.GuiThreadInfo info = new NativeMethods.GuiThreadInfo();
            info.cbSize = Marshal.SizeOf(typeof(NativeMethods.GuiThreadInfo));

            if (NativeMethods.GetGUIThreadInfo(threadId, ref info) && info.hwndFocus != IntPtr.Zero)
                return info.hwndFocus;

            return fallback;
        }

        private static bool IsNativeImeMode(IntPtr window)
        {
            if (window == IntPtr.Zero)
                return false;

            IntPtr context = NativeMethods.ImmGetContext(window);
            if (context == IntPtr.Zero)
                return IsNativeModeFromDefaultImeWindow(window);

            try
            {
                int conversionMode;
                int sentenceMode;

                if (!NativeMethods.ImmGetOpenStatus(context))
                    return IsNativeModeFromDefaultImeWindow(window);

                if (!NativeMethods.ImmGetConversionStatus(context, out conversionMode, out sentenceMode))
                    return IsNativeModeFromDefaultImeWindow(window);

                return (conversionMode & ImeCmodeNative) != 0;
            }
            finally
            {
                NativeMethods.ImmReleaseContext(window, context);
            }
        }

        private static bool IsNativeModeFromDefaultImeWindow(IntPtr window)
        {
            IntPtr imeWindow = NativeMethods.ImmGetDefaultIMEWnd(window);
            if (imeWindow == IntPtr.Zero)
                return false;

            IntPtr openStatus = NativeMethods.SendMessage(
                imeWindow,
                WmImeControl,
                new IntPtr(ImcGetOpenStatus),
                IntPtr.Zero);

            if (openStatus == IntPtr.Zero)
                return false;

            IntPtr conversionMode = NativeMethods.SendMessage(
                imeWindow,
                WmImeControl,
                new IntPtr(ImcGetConversionMode),
                IntPtr.Zero);

            return (conversionMode.ToInt64() & ImeCmodeNative) != 0;
        }
    }

    internal static class IconFactory
    {
        public static Icon Create(string text)
        {
            Bitmap bitmap = new Bitmap(16, 16);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Font font = new Font("Malgun Gothic", text == "한" ? 8.2f : 6.8f, FontStyle.Bold, GraphicsUnit.Point))
            using (SolidBrush fill = new SolidBrush(text == "한" ? Color.FromArgb(24, 128, 91) : Color.FromArgb(38, 78, 140)))
            using (SolidBrush brush = new SolidBrush(Color.White))
            using (StringFormat format = new StringFormat())
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.FillEllipse(fill, new Rectangle(0, 0, 15, 15));
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                graphics.DrawString(text, font, brush, new RectangleF(0, -1, 16, 16), format);
            }

            IntPtr iconHandle = bitmap.GetHicon();
            bitmap.Dispose();

            Icon icon = (Icon)Icon.FromHandle(iconHandle).Clone();
            NativeMethods.DestroyIcon(iconHandle);
            return icon;
        }
    }

    internal static class NativeMethods
    {
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GuiThreadInfo
        {
            public int cbSize;
            public int flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public Rect rcCaret;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetGUIThreadInfo(uint idThread, ref GuiThreadInfo lpgui);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

        [DllImport("imm32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("imm32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ImmGetOpenStatus(IntPtr hIMC);

        [DllImport("imm32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ImmGetConversionStatus(IntPtr hIMC, out int lpfdwConversion, out int lpfdwSentence);
    }
}
