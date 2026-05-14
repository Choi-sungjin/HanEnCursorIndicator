using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
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

    internal static class Labels
    {
        public const string Korean = "\uD55C";
        public const string EnglishLower = "en";
        public const string EnglishUpper = "EN";
    }

    internal enum IndicatorPose
    {
        Idle,
        Point,
        Cheer
    }

    internal static class TextResources
    {
        public const string ToggleIndicator = "\uCEE4\uC11C \uC606 \uD45C\uC2DC \uCF1C\uAE30";
        public const string CurrentStatePrefix = "\uD604\uC7AC \uC0C1\uD0DC: ";
        public const string Checking = "\uD655\uC778 \uC911";
        public const string OpenImageFolder = "\uC774\uBBF8\uC9C0 \uD3F4\uB354 \uC5F4\uAE30";
        public const string ReloadImages = "\uCEE4\uC2A4\uD140 \uC774\uBBF8\uC9C0 \uB2E4\uC2DC \uBD88\uB7EC\uC624\uAE30";
        public const string SizeMenu = "\uD06C\uAE30";
        public const string DragSizeSettings = "\uB4DC\uB798\uADF8\uB85C \uD06C\uAE30 \uC870\uC815";
        public const string SizeGain = "\uD06C\uAE30 \uAC8C\uC778";
        public const string Close = "\uB2EB\uAE30";
        public const string Exit = "\uC885\uB8CC";
        public const string TrayTitle = "\uD55C/En \uB9C8\uC6B0\uC2A4 \uD45C\uC2DC\uAE30";
    }

    internal sealed class IndicatorContext : ApplicationContext
    {
        private readonly AppSettings settings;
        private readonly IndicatorAssets assets;
        private readonly IndicatorForm indicatorForm;
        private readonly System.Windows.Forms.Timer timer;
        private readonly NotifyIcon trayIcon;
        private readonly ToolStripMenuItem enabledItem;
        private readonly ToolStripMenuItem stateItem;
        private readonly ToolStripMenuItem sizeMenu;
        private readonly List<ToolStripMenuItem> sizePresetItems = new List<ToolStripMenuItem>();
        private Icon currentTrayIcon;
        private SizeSettingsForm sizeSettingsForm;
        private bool enabled = true;
        private string lastText = "";

        public IndicatorContext()
        {
            settings = AppSettings.Load();
            assets = new IndicatorAssets();
            indicatorForm = new IndicatorForm(assets, settings.SizePercent);

            enabledItem = new ToolStripMenuItem(TextResources.ToggleIndicator);
            enabledItem.Checked = true;
            enabledItem.CheckOnClick = true;
            enabledItem.CheckedChanged += OnEnabledChanged;

            stateItem = new ToolStripMenuItem(TextResources.CurrentStatePrefix + TextResources.Checking);
            stateItem.Enabled = false;

            sizeMenu = CreateSizeMenu();
            UpdateSizeMenuChecks();

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add(enabledItem);
            menu.Items.Add(stateItem);
            menu.Items.Add(new ToolStripMenuItem(TextResources.OpenImageFolder, null, OnOpenImageFolder));
            menu.Items.Add(new ToolStripMenuItem(TextResources.ReloadImages, null, OnReloadImages));
            menu.Items.Add(sizeMenu);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem(TextResources.Exit, null, OnExit));

            currentTrayIcon = IconFactory.Create(Labels.Korean);
            trayIcon = new NotifyIcon();
            trayIcon.Icon = currentTrayIcon;
            trayIcon.Text = TextResources.TrayTitle;
            trayIcon.ContextMenuStrip = menu;
            trayIcon.Visible = true;
            trayIcon.MouseDoubleClick += OnTrayDoubleClick;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 30;
            timer.Tick += OnTimerTick;
            timer.Start();
        }

        private ToolStripMenuItem CreateSizeMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem(TextResources.SizeMenu);
            int[] presets = new[] { 50, 75, 100, 125, 150, 200, 250 };

            foreach (int preset in presets)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(preset + "%");
                item.Tag = preset;
                item.Click += OnSizePresetClick;
                sizePresetItems.Add(item);
                menu.DropDownItems.Add(item);
            }

            menu.DropDownItems.Add(new ToolStripSeparator());
            menu.DropDownItems.Add(new ToolStripMenuItem(TextResources.DragSizeSettings, null, OnOpenSizeSettings));
            return menu;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            string text = ImeStateReader.GetIndicatorText();
            stateItem.Text = TextResources.CurrentStatePrefix + text;

            if (text != lastText)
            {
                lastText = text;
                indicatorForm.SetIndicatorText(text);
                ReplaceTrayIcon(text);
            }

            indicatorForm.TickAnimations();

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

        private void OnReloadImages(object sender, EventArgs e)
        {
            assets.Reload();
            indicatorForm.RefreshAssets();
            ShowReloadResult();
        }

        private void OnOpenImageFolder(object sender, EventArgs e)
        {
            Directory.CreateDirectory(assets.ImageDirectory);
            Process.Start(assets.ImageDirectory);
        }

        private void OnSizePresetClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null || item.Tag == null)
                return;

            SetSizePercent((int)item.Tag);
        }

        private void OnOpenSizeSettings(object sender, EventArgs e)
        {
            if (sizeSettingsForm == null || sizeSettingsForm.IsDisposed)
            {
                sizeSettingsForm = new SizeSettingsForm(settings.SizePercent, SetSizePercent);
                sizeSettingsForm.FormClosed += OnSizeSettingsClosed;
            }

            sizeSettingsForm.SetValue(settings.SizePercent);
            sizeSettingsForm.Show();
            sizeSettingsForm.Activate();
        }

        private void OnSizeSettingsClosed(object sender, FormClosedEventArgs e)
        {
            sizeSettingsForm = null;
        }

        private void SetSizePercent(int percent)
        {
            settings.SizePercent = AppSettings.ClampSizePercent(percent);
            settings.Save();
            indicatorForm.SetSizePercent(settings.SizePercent);

            if (sizeSettingsForm != null && !sizeSettingsForm.IsDisposed)
                sizeSettingsForm.SetValue(settings.SizePercent);

            UpdateSizeMenuChecks();
        }

        private void UpdateSizeMenuChecks()
        {
            foreach (ToolStripMenuItem item in sizePresetItems)
                item.Checked = item.Tag != null && (int)item.Tag == settings.SizePercent;

            sizeMenu.Text = TextResources.SizeMenu + " (" + settings.SizePercent + "%)";
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

        private void ShowReloadResult()
        {
            trayIcon.BalloonTipTitle = TextResources.TrayTitle;
            trayIcon.BalloonTipText = assets.LoadedCount > 0
                ? "Loaded " + assets.LoadedCount + " custom image(s)."
                : "No custom images found. Put idle.png, point.png, and cheer.png in the images folder.";
            trayIcon.ShowBalloonTip(2500);
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
                if (assets != null)
                    assets.Dispose();
                if (sizeSettingsForm != null)
                    sizeSettingsForm.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    internal sealed class IndicatorForm : Form
    {
        private const int PointMilliseconds = 1000;
        private const int CheerCycleMilliseconds = 9000;
        private const int CheerDurationMilliseconds = 1200;
        private static readonly Color TransparentBackColor = Color.FromArgb(255, 1, 2, 3);

        private readonly IndicatorAssets assets;
        private readonly Font textFont;
        private string indicatorText = Labels.Korean;
        private int sizePercent;
        private IndicatorPose currentPose = IndicatorPose.Idle;
        private DateTime startedAtUtc = DateTime.UtcNow;
        private DateTime stateChangedAtUtc = DateTime.UtcNow;

        public IndicatorForm(IndicatorAssets assets, int sizePercent)
        {
            this.assets = assets;
            this.sizePercent = AppSettings.ClampSizePercent(sizePercent);
            textFont = new Font("Malgun Gothic", 9.5f, FontStyle.Bold, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            Opacity = 0.98d;
            BackColor = TransparentBackColor;
            TransparencyKey = TransparentBackColor;
            DoubleBuffered = true;
            ApplyDesiredSize();
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
            stateChangedAtUtc = DateTime.UtcNow;
            currentPose = IndicatorPose.Point;
            ApplyDesiredSize();
            Invalidate();
        }

        public void SetSizePercent(int percent)
        {
            sizePercent = AppSettings.ClampSizePercent(percent);
            ApplyDesiredSize();
            Invalidate();
        }

        public void RefreshAssets()
        {
            ApplyDesiredSize();
            stateChangedAtUtc = DateTime.UtcNow;
            currentPose = IndicatorPose.Point;
            Invalidate();
        }

        public void TickAnimations()
        {
            IndicatorPose nextPose = CalculatePose();
            if (nextPose != currentPose)
            {
                currentPose = nextPose;
                ApplyDesiredSize();
                Invalidate();
            }

            IndicatorImage image = GetCurrentImage();
            if (image != null && image.Animated)
                image.UpdateFrame();

            if ((DateTime.UtcNow - stateChangedAtUtc).TotalMilliseconds < 260 || (image != null && image.Animated))
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(TransparentBackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            IndicatorImage image = GetCurrentImage();
            float scale = GetPopScale();

            if (image != null)
            {
                Rectangle imageRect = DrawImageIndicator(e.Graphics, image.Image, scale);
                if (assets.HasPoseImages)
                    DrawFaceLabel(e.Graphics, imageRect, indicatorText, scale);
                return;
            }

            DrawTextIndicator(e.Graphics, scale);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && textFont != null)
                textFont.Dispose();

            base.Dispose(disposing);
        }

        private IndicatorPose CalculatePose()
        {
            double sinceChange = (DateTime.UtcNow - stateChangedAtUtc).TotalMilliseconds;
            if (sinceChange < PointMilliseconds && assets.GetPose(IndicatorPose.Point) != null)
                return IndicatorPose.Point;

            double sinceStart = (DateTime.UtcNow - startedAtUtc).TotalMilliseconds;
            double cycle = sinceStart % CheerCycleMilliseconds;
            if (sinceStart > 3000 && cycle < CheerDurationMilliseconds && assets.GetPose(IndicatorPose.Cheer) != null)
                return IndicatorPose.Cheer;

            return IndicatorPose.Idle;
        }

        private IndicatorImage GetCurrentImage()
        {
            IndicatorImage poseImage = assets.GetPose(currentPose);
            if (poseImage != null)
                return poseImage;

            return assets.GetLegacy(indicatorText);
        }

        private void ApplyDesiredSize()
        {
            Size target = GetDesiredSize();
            if (Width != target.Width || Height != target.Height)
                Size = target;
        }

        private Size GetDesiredSize()
        {
            IndicatorImage image = GetCurrentImage();
            if (image == null)
            {
                float ratio = sizePercent / 100.0f;
                return new Size(
                    Math.Max(18, (int)Math.Round(42 * ratio)),
                    Math.Max(14, (int)Math.Round(30 * ratio)));
            }

            Size imageSize = GetImageDrawSize(image.Image);
            return new Size(imageSize.Width + 16, imageSize.Height + 16);
        }

        private Size GetImageDrawSize(Image image)
        {
            int maxSide = Math.Max(24, (int)Math.Round(64 * (sizePercent / 100.0f)));
            int minSide = Math.Max(16, (int)Math.Round(24 * (sizePercent / 100.0f)));
            int sourceWidth = Math.Max(1, image.Width);
            int sourceHeight = Math.Max(1, image.Height);
            float ratio = Math.Min(maxSide / (float)sourceWidth, maxSide / (float)sourceHeight);

            if (ratio > 1.0f && sourceWidth < minSide && sourceHeight < minSide)
                ratio = Math.Min(minSide / (float)sourceWidth, minSide / (float)sourceHeight);
            else if (ratio > 1.0f)
                ratio = 1.0f;

            return new Size(
                Math.Max(12, (int)Math.Round(sourceWidth * ratio)),
                Math.Max(12, (int)Math.Round(sourceHeight * ratio)));
        }

        private float GetPopScale()
        {
            double elapsed = (DateTime.UtcNow - stateChangedAtUtc).TotalMilliseconds;
            if (elapsed <= 0 || elapsed >= 240)
                return 1.0f;

            double progress = elapsed / 240.0d;
            return 1.0f + (float)(Math.Sin(progress * Math.PI) * 0.16d);
        }

        private Rectangle DrawImageIndicator(Graphics graphics, Image image, float popScale)
        {
            Size drawSize = GetImageDrawSize(image);
            int scaledWidth = Math.Max(1, (int)Math.Round(drawSize.Width * popScale));
            int scaledHeight = Math.Max(1, (int)Math.Round(drawSize.Height * popScale));
            Rectangle rect = new Rectangle(
                (Width - scaledWidth) / 2,
                (Height - scaledHeight) / 2,
                scaledWidth,
                scaledHeight);

            using (ImageAttributes attributes = new ImageAttributes())
            {
                graphics.DrawImage(
                    image,
                    rect,
                    0,
                    0,
                    image.Width,
                    image.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }

            return rect;
        }

        private void DrawFaceLabel(Graphics graphics, Rectangle imageRect, string text, float popScale)
        {
            RectangleF faceRect = new RectangleF(
                imageRect.Left + imageRect.Width * 0.31f,
                imageRect.Top + imageRect.Height * 0.19f,
                imageRect.Width * 0.38f,
                imageRect.Height * 0.26f);

            float fontSize = Math.Max(7.0f, imageRect.Height * (text == Labels.Korean ? 0.19f : 0.14f));
            using (Font font = new Font("Malgun Gothic", fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
            using (SolidBrush fill = new SolidBrush(GetLabelColor(text)))
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(110, Color.White)))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                RectangleF shadowRect = new RectangleF(faceRect.X + 1, faceRect.Y + 1, faceRect.Width, faceRect.Height);
                graphics.DrawString(text, font, shadow, shadowRect, format);
                graphics.DrawString(text, font, fill, faceRect, format);
            }
        }

        private static Color GetLabelColor(string text)
        {
            if (text == Labels.Korean)
                return Color.FromArgb(24, 128, 91);
            if (text == Labels.EnglishUpper)
                return Color.FromArgb(21, 70, 160);
            return Color.FromArgb(38, 78, 140);
        }

        private void DrawTextIndicator(Graphics graphics, float popScale)
        {
            float sizeRatio = sizePercent / 100.0f;
            int baseWidth = (int)Math.Round(34 * sizeRatio);
            int baseHeight = (int)Math.Round(24 * sizeRatio);
            int scaledWidth = Math.Max(1, (int)Math.Round(baseWidth * popScale));
            int scaledHeight = Math.Max(1, (int)Math.Round(baseHeight * popScale));
            Rectangle rect = new Rectangle(
                (Width - scaledWidth) / 2,
                (Height - scaledHeight) / 2,
                scaledWidth,
                scaledHeight);

            bool korean = indicatorText == Labels.Korean;
            Color fill = korean ? Color.FromArgb(24, 128, 91) : Color.FromArgb(38, 78, 140);

            using (GraphicsPath path = CreateRoundRectangle(rect, Math.Max(4, (int)Math.Round(6 * sizeRatio))))
            using (SolidBrush brush = new SolidBrush(fill))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            using (Font font = new Font("Malgun Gothic", Math.Max(7.0f, 9.5f * sizeRatio), FontStyle.Bold, GraphicsUnit.Point))
            using (StringFormat format = new StringFormat())
            {
                graphics.FillPath(brush, path);
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                graphics.DrawString(indicatorText, font, textBrush, rect, format);
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

    internal sealed class IndicatorAssets : IDisposable
    {
        private static readonly string[] Extensions = new[] { ".gif", ".png", ".jpg", ".jpeg", ".bmp" };
        private readonly Dictionary<IndicatorPose, string> poseNames = new Dictionary<IndicatorPose, string>();
        private Dictionary<IndicatorPose, IndicatorImage> poseImages = new Dictionary<IndicatorPose, IndicatorImage>();
        private Dictionary<string, IndicatorImage> legacyImages = new Dictionary<string, IndicatorImage>();

        public IndicatorAssets()
        {
            poseNames[IndicatorPose.Idle] = "idle";
            poseNames[IndicatorPose.Point] = "point";
            poseNames[IndicatorPose.Cheer] = "cheer";
            Reload();
        }

        public string ImageDirectory
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images"); }
        }

        public int LoadedCount
        {
            get { return poseImages.Count + legacyImages.Count; }
        }

        public bool HasPoseImages
        {
            get { return poseImages.Count > 0; }
        }

        public IndicatorImage GetPose(IndicatorPose pose)
        {
            IndicatorImage image;
            if (poseImages.TryGetValue(pose, out image))
                return image;

            if (poseImages.TryGetValue(IndicatorPose.Idle, out image))
                return image;

            return null;
        }

        public IndicatorImage GetLegacy(string label)
        {
            IndicatorImage image;
            if (legacyImages.TryGetValue(label, out image))
                return image;

            if (label == Labels.EnglishUpper && legacyImages.TryGetValue(Labels.EnglishLower, out image))
                return image;

            return null;
        }

        public void Reload()
        {
            Dictionary<IndicatorPose, IndicatorImage> oldPoseImages = poseImages;
            Dictionary<string, IndicatorImage> oldLegacyImages = legacyImages;
            Dictionary<IndicatorPose, IndicatorImage> newPoseImages = new Dictionary<IndicatorPose, IndicatorImage>();
            Dictionary<string, IndicatorImage> newLegacyImages = new Dictionary<string, IndicatorImage>();

            foreach (KeyValuePair<IndicatorPose, string> pair in poseNames)
                TryLoadPose(newPoseImages, ImageDirectory, pair.Key, pair.Value);

            TryLoadLegacy(newLegacyImages, ImageDirectory, Labels.Korean, "han");
            TryLoadLegacy(newLegacyImages, ImageDirectory, Labels.EnglishLower, "en");

            poseImages = newPoseImages;
            legacyImages = newLegacyImages;

            DisposeImages(oldPoseImages.Values);
            DisposeImages(oldLegacyImages.Values);
        }

        public void Dispose()
        {
            DisposeImages(poseImages.Values);
            DisposeImages(legacyImages.Values);
            poseImages.Clear();
            legacyImages.Clear();
        }

        private static void DisposeImages(IEnumerable<IndicatorImage> images)
        {
            foreach (IndicatorImage image in images)
                image.Dispose();
        }

        private static void TryLoadPose(Dictionary<IndicatorPose, IndicatorImage> target, string imageDirectory, IndicatorPose pose, string fileNameWithoutExtension)
        {
            IndicatorImage image = TryLoadFromFile(imageDirectory, fileNameWithoutExtension);
            if (image != null)
                target[pose] = image;
        }

        private static void TryLoadLegacy(Dictionary<string, IndicatorImage> target, string imageDirectory, string label, string fileNameWithoutExtension)
        {
            IndicatorImage image = TryLoadFromFile(imageDirectory, fileNameWithoutExtension);
            if (image != null)
                target[label] = image;
        }

        private static IndicatorImage TryLoadFromFile(string imageDirectory, string fileNameWithoutExtension)
        {
            foreach (string extension in Extensions)
            {
                string path = Path.Combine(imageDirectory, fileNameWithoutExtension + extension);
                if (!File.Exists(path))
                    continue;

                try
                {
                    return IndicatorImage.Load(path);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
    }

    internal sealed class IndicatorImage : IDisposable
    {
        private readonly MemoryStream stream;
        private readonly EventHandler animationHandler;

        private IndicatorImage(Image image, MemoryStream stream)
        {
            Image = image;
            this.stream = stream;
            Animated = ImageAnimator.CanAnimate(image);

            if (Animated)
            {
                animationHandler = OnFrameChanged;
                ImageAnimator.Animate(Image, animationHandler);
            }
        }

        public Image Image { get; private set; }

        public bool Animated { get; private set; }

        public static IndicatorImage Load(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            MemoryStream stream = new MemoryStream(bytes);
            Image image = Image.FromStream(stream);
            return new IndicatorImage(image, stream);
        }

        public void UpdateFrame()
        {
            if (Animated)
                ImageAnimator.UpdateFrames(Image);
        }

        public void Dispose()
        {
            if (Image != null)
            {
                if (Animated && animationHandler != null)
                    ImageAnimator.StopAnimate(Image, animationHandler);

                Image.Dispose();
                Image = null;
            }

            if (stream != null)
                stream.Dispose();
        }

        private static void OnFrameChanged(object sender, EventArgs e)
        {
        }
    }

    internal sealed class SizeSettingsForm : Form
    {
        private readonly TrackBar trackBar;
        private readonly NumericUpDown numeric;
        private readonly Label valueLabel;
        private readonly Action<int> onChanged;
        private bool updating;

        public SizeSettingsForm(int initialPercent, Action<int> onChanged)
        {
            this.onChanged = onChanged;
            Text = TextResources.DragSizeSettings;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(310, 118);

            valueLabel = new Label();
            valueLabel.AutoSize = false;
            valueLabel.TextAlign = ContentAlignment.MiddleLeft;
            valueLabel.Location = new Point(12, 10);
            valueLabel.Size = new Size(180, 22);

            numeric = new NumericUpDown();
            numeric.Minimum = AppSettings.MinSizePercent;
            numeric.Maximum = AppSettings.MaxSizePercent;
            numeric.Increment = 5;
            numeric.Location = new Point(210, 10);
            numeric.Size = new Size(78, 22);
            numeric.ValueChanged += OnNumericChanged;

            trackBar = new TrackBar();
            trackBar.Minimum = AppSettings.MinSizePercent;
            trackBar.Maximum = AppSettings.MaxSizePercent;
            trackBar.TickFrequency = 25;
            trackBar.SmallChange = 5;
            trackBar.LargeChange = 25;
            trackBar.Location = new Point(10, 42);
            trackBar.Size = new Size(286, 44);
            trackBar.Scroll += OnTrackBarChanged;

            Button closeButton = new Button();
            closeButton.Text = TextResources.Close;
            closeButton.Location = new Point(218, 86);
            closeButton.Size = new Size(70, 24);
            closeButton.Click += OnCloseClicked;

            Controls.Add(valueLabel);
            Controls.Add(numeric);
            Controls.Add(trackBar);
            Controls.Add(closeButton);

            SetValue(initialPercent);
        }

        public void SetValue(int percent)
        {
            int value = AppSettings.ClampSizePercent(percent);
            updating = true;
            trackBar.Value = value;
            numeric.Value = value;
            valueLabel.Text = TextResources.SizeGain + ": " + value + "%";
            updating = false;
        }

        private void OnTrackBarChanged(object sender, EventArgs e)
        {
            if (updating)
                return;

            int rounded = (int)Math.Round(trackBar.Value / 5.0d) * 5;
            if (rounded != trackBar.Value)
                trackBar.Value = rounded;

            onChanged(rounded);
        }

        private void OnNumericChanged(object sender, EventArgs e)
        {
            if (updating)
                return;

            onChanged((int)numeric.Value);
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close();
        }
    }

    internal sealed class AppSettings
    {
        public const int MinSizePercent = 50;
        public const int MaxSizePercent = 250;
        private const int DefaultSizePercent = 100;

        public int SizePercent = DefaultSizePercent;

        public static AppSettings Load()
        {
            AppSettings settings = new AppSettings();
            try
            {
                string path = GetSettingsPath();
                if (!File.Exists(path))
                    return settings;

                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length != 2)
                        continue;

                    if (parts[0].Trim().Equals("sizePercent", StringComparison.OrdinalIgnoreCase))
                    {
                        int value;
                        if (int.TryParse(parts[1].Trim(), out value))
                            settings.SizePercent = ClampSizePercent(value);
                    }
                }
            }
            catch
            {
            }

            return settings;
        }

        public void Save()
        {
            try
            {
                string path = GetSettingsPath();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, "sizePercent=" + ClampSizePercent(SizePercent));
            }
            catch
            {
            }
        }

        public static int ClampSizePercent(int value)
        {
            if (value < MinSizePercent)
                return MinSizePercent;
            if (value > MaxSizePercent)
                return MaxSizePercent;
            return value;
        }

        private static string GetSettingsPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "HanEnCursorIndicator", "settings.ini");
        }
    }

    internal static class ImeStateReader
    {
        private const int KoreanPrimaryLanguageId = 0x12;
        private const int ImeCmodeNative = 0x0001;
        private const int WmImeControl = 0x0283;
        private const int ImcGetConversionMode = 0x0001;
        private const int ImcGetOpenStatus = 0x0005;
        private const int VkShift = 0x10;
        private const int VkCapital = 0x14;

        public static string GetIndicatorText()
        {
            if (IsKoreanInputMode())
                return Labels.Korean;

            return IsUppercaseEnglishMode() ? Labels.EnglishUpper : Labels.EnglishLower;
        }

        private static bool IsUppercaseEnglishMode()
        {
            bool capsLock = (NativeMethods.GetKeyState(VkCapital) & 0x0001) != 0;
            bool shiftDown = (NativeMethods.GetAsyncKeyState(VkShift) & unchecked((short)0x8000)) != 0;
            return capsLock ^ shiftDown;
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
            using (Font font = new Font("Malgun Gothic", text == Labels.Korean ? 8.2f : 6.6f, FontStyle.Bold, GraphicsUnit.Point))
            using (SolidBrush fill = new SolidBrush(text == Labels.Korean ? Color.FromArgb(24, 128, 91) : Color.FromArgb(38, 78, 140)))
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
        public static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

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
