using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

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

    internal enum CursorDisplayMode
    {
        AlwaysFollow,
        ShowWhenIdle
    }

    internal static class IndicatorPoseHelper
    {
        public static readonly IndicatorPose[] All = new[] { IndicatorPose.Idle, IndicatorPose.Point, IndicatorPose.Cheer };

        public static string GetKey(IndicatorPose pose)
        {
            if (pose == IndicatorPose.Point)
                return "point";
            if (pose == IndicatorPose.Cheer)
                return "cheer";
            return "idle";
        }

        public static string GetDisplayName(IndicatorPose pose)
        {
            if (pose == IndicatorPose.Point)
                return "Point";
            if (pose == IndicatorPose.Cheer)
                return "Cheer";
            return "Idle";
        }

        public static bool TryParseKey(string key, out IndicatorPose pose)
        {
            if (key.Equals("point", StringComparison.OrdinalIgnoreCase))
            {
                pose = IndicatorPose.Point;
                return true;
            }

            if (key.Equals("cheer", StringComparison.OrdinalIgnoreCase))
            {
                pose = IndicatorPose.Cheer;
                return true;
            }

            if (key.Equals("idle", StringComparison.OrdinalIgnoreCase))
            {
                pose = IndicatorPose.Idle;
                return true;
            }

            pose = IndicatorPose.Idle;
            return false;
        }
    }

    internal static class IndicatorStates
    {
        public const string Korean = "ko";
        public const string EnglishLower = "en";
        public const string EnglishUpper = "EN";

        public static readonly string[] All = new[] { Korean, EnglishLower, EnglishUpper };

        public static string FromLabel(string label)
        {
            if (label == Labels.Korean)
                return Korean;
            if (label == Labels.EnglishUpper)
                return EnglishUpper;
            return EnglishLower;
        }

        public static string ToLabel(string stateKey)
        {
            if (stateKey == Korean)
                return Labels.Korean;
            if (stateKey == EnglishUpper)
                return Labels.EnglishUpper;
            return Labels.EnglishLower;
        }

        public static string GetDisplayName(string stateKey)
        {
            if (stateKey == Korean)
                return "\uD55C\uAE00 (ko)";
            if (stateKey == EnglishUpper)
                return "\uC601\uC5B4 \uB300\uBB38\uC790 (EN)";
            return "\uC601\uC5B4 \uC18C\uBB38\uC790 (en)";
        }

        public static bool IsValidKey(string stateKey)
        {
            return stateKey == Korean || stateKey == EnglishLower || stateKey == EnglishUpper;
        }

        public static string[] GetFilePrefixes(string stateKey)
        {
            if (stateKey == Korean)
                return new[] { "ko" };
            if (stateKey == EnglishUpper)
                return new[] { "EN", "upper", "caps" };
            return new[] { "en" };
        }
    }

    internal static class TextResources
    {
        public const string ToggleIndicator = "\uCEE4\uC11C \uC606 \uD45C\uC2DC \uCF1C\uAE30";
        public const string CurrentStatePrefix = "\uD604\uC7AC \uC0C1\uD0DC: ";
        public const string Checking = "\uD655\uC778 \uC911";
        public const string OpenImageFolder = "\uC774\uBBF8\uC9C0 \uD3F4\uB354 \uC5F4\uAE30";
        public const string ChooseImage = "\uC774\uBBF8\uC9C0 \uC120\uD0DD";
        public const string ReloadImages = "\uCEE4\uC2A4\uD140 \uC774\uBBF8\uC9C0 \uB2E4\uC2DC \uBD88\uB7EC\uC624\uAE30";
        public const string RemoveImageBackground = "\uC774\uBBF8\uC9C0 \uB204\uB07C \uCC98\uB9AC";
        public const string ImagePackMode = "\uC774\uBBF8\uC9C0 \uBAA8\uB4DC";
        public const string SharedPoseImages = "\uACF5\uD1B5 3\uC7A5";
        public const string StatePoseImages = "\uC0C1\uD0DC\uBCC4 9\uC7A5";
        public const string SelectFile = "\uD30C\uC77C \uC120\uD0DD";
        public const string RemoveSlotImage = "\uC2AC\uB86F \uBE44\uC6B0\uAE30";
        public const string ImageSlot = "\uC2AC\uB86F";
        public const string CurrentFile = "\uD604\uC7AC \uD30C\uC77C";
        public const string NoImageSelected = "\uC5C6\uC74C";
        public const string ImageInstalled = "\uC774\uBBF8\uC9C0\uB97C \uC801\uC6A9\uD588\uC2B5\uB2C8\uB2E4.";
        public const string ImageSlotCleared = "\uC2AC\uB86F\uC744 \uBE44\uC6E0\uC2B5\uB2C8\uB2E4.";
        public const string SaveSmallCutout = "\uC791\uAC8C \uC800\uC7A5";
        public const string MaxImageSize = "\uCD5C\uB300 \uD06C\uAE30";
        public const string SizeMenu = "\uD06C\uAE30";
        public const string DragSizeSettings = "\uB4DC\uB798\uADF8\uB85C \uD06C\uAE30 \uC870\uC815";
        public const string AdjustFaceCenter = "\uAE00\uC790 \uC704\uCE58 \uC870\uC815";
        public const string ShowLabel = "\uAE00\uC790 \uD45C\uC2DC";
        public const string DisplayModeMenu = "\uD45C\uC2DC \uBAA8\uB4DC";
        public const string DisplayModeAlwaysFollow = "\uD56D\uC0C1 \uB530\uB77C\uB2E4\uB2C8\uAE30";
        public const string DisplayModeShowWhenIdle = "\uBA48\uCDB0\uC744 \uB54C\uB9CC \uD45C\uC2DC";
        public const string MascotColorMenu = "\uBBF8\uB2C8\uBBF8 \uC0C9\uC0C1";
        public const string UseLanguageColors = "\uC0C1\uD0DC\uBCC4 \uC0C9\uC0C1 \uC0AC\uC6A9";
        public const string BaseColor = "\uAE30\uBCF8 \uC0C9\uC0C1 \uC120\uD0DD";
        public const string KoreanColor = "\uD55C\uAE00 \uC0C9\uC0C1 \uC120\uD0DD";
        public const string EnglishLowerColor = "\uC601\uC5B4 \uC18C\uBB38\uC790 \uC0C9\uC0C1 \uC120\uD0DD";
        public const string EnglishUpperColor = "\uC601\uC5B4 \uB300\uBB38\uC790 \uC0C9\uC0C1 \uC120\uD0DD";
        public const string LabelColorMenu = "\uAE00\uC528 \uC0C9\uC0C1";
        public const string KoreanLabelColor = "\uD55C\uAE00 \uAE00\uC528 \uC0C9\uC0C1";
        public const string EnglishLowerLabelColor = "\uC601\uC5B4 \uC18C\uBB38\uC790 \uAE00\uC528 \uC0C9\uC0C1";
        public const string EnglishUpperLabelColor = "\uC601\uC5B4 \uB300\uBB38\uC790 \uAE00\uC528 \uC0C9\uC0C1";
        public const string UseCutoutLine = "\uB77C\uC778\uC73C\uB85C \uB204\uB07C \uBCF4\uC815";
        public const string ForegroundCutoutLine = "\uC724\uACFD \uC548\uCABD\uB9CC \uB0A8\uAE30\uAE30";
        public const string BackgroundCutoutLine = "\uBC30\uACBD \uB77C\uC778 \uC81C\uAC70";
        public const string CutoutForegroundLineSelection = "\uD53C\uC0AC\uCCB4 \uC724\uACFD \uC120\uD0DD";
        public const string CutoutBackgroundLineSelection = "\uBC30\uACBD \uC81C\uAC70 \uB77C\uC778 \uC120\uD0DD";
        public const string CutoutForegroundLineHint = "\uD53C\uC0AC\uCCB4 \uBC14\uAE65 \uC724\uACFD\uC744 \uD55C \uBC14\uD034 \uB458\uB7EC \uADF8\uB9AC\uBA74 \uADF8 \uC548\uCABD\uB9CC \uB0A8\uAE41\uB2C8\uB2E4.";
        public const string CutoutBackgroundLineHint = "\uBC30\uACBD\uC73C\uB85C \uC9C0\uC6B8 \uC601\uC5ED\uC5D0 \uC120\uC744 \uADF8\uB9B0 \uB4A4 OK\uB97C \uB204\uB974\uC138\uC694.";
        public const string Undo = "\uB418\uB3CC\uB9AC\uAE30";
        public const string Clear = "\uCD08\uAE30\uD654";
        public const string SizeGain = "\uD06C\uAE30 \uAC8C\uC778";
        public const string FaceCenter = "\uAE00\uC790 \uC704\uCE58";
        public const string State = "\uC0C1\uD0DC";
        public const string Pose = "\uD3EC\uC988";
        public const string Reset = "\uAE30\uBCF8\uAC12";
        public const string Close = "\uB2EB\uAE30";
        public const string Exit = "\uC885\uB8CC";
        public const string TrayTitle = "\uD55C/En \uB9C8\uC6B0\uC2A4 \uD45C\uC2DC\uAE30";
        public const string VoiceMenu = "\uBCF4\uC774\uC2A4";
        public const string VoiceOnDrag = "\uB4DC\uB798\uADF8 \uD14D\uC2A4\uD2B8 \uC77D\uAE30";
        public const string VoiceSettings = "\uBCF4\uC774\uC2A4 \uC124\uC815";
        public const string VoiceTestClipboard = "\uD074\uB9BD\uBCF4\uB4DC \uD14D\uC2A4\uD2B8 \uD14C\uC2A4\uD2B8";
        public const string ApiKey = "API Key";
        public const string VoiceId = "Voice ID";
        public const string Language = "\uC5B8\uC5B4";
        public const string Model = "\uBAA8\uB378";
        public const string Style = "\uC2A4\uD0C0\uC77C";
        public const string Speed = "\uC18D\uB3C4";
        public const string MaxTextLength = "\uCD5C\uB300 \uBB38\uC790 \uC218";
        public const string Save = "\uC800\uC7A5";
        public const string ClearApiKey = "API Key \uC0AD\uC81C";
        public const string ApiKeySaved = "\uC800\uC7A5\uB41C API Key: \uC788\uC74C";
        public const string ApiKeyMissing = "\uC800\uC7A5\uB41C API Key: \uC5C6\uC74C";
        public const string VoiceSaved = "\uBCF4\uC774\uC2A4 \uC124\uC815\uC744 \uC800\uC7A5\uD588\uC2B5\uB2C8\uB2E4.";
        public const string VoiceMissingConfig = "API Key\uC640 Voice ID\uB97C \uBA3C\uC800 \uC124\uC815\uD558\uC138\uC694.";
        public const string VoiceNoText = "\uC77D\uC744 \uD14D\uC2A4\uD2B8\uAC00 \uC5C6\uC2B5\uB2C8\uB2E4.";
        public const string VoiceFailed = "\uC74C\uC131 \uC0DD\uC131 \uC2E4\uD328: ";
        public const string VoiceReady = "\uB4DC\uB798\uADF8\uD55C \uD14D\uC2A4\uD2B8\uB97C Supertone\uC73C\uB85C \uC77D\uC744 \uC900\uBE44\uAC00 \uB410\uC2B5\uB2C8\uB2E4.";
        public const string VoiceEngine = "TTS \uC5D4\uC9C4";
        public const string VoiceEngineSupertonic = "Supertonic \uB85C\uCEEC (\uBB34\uB8CC)";
        public const string VoiceEngineSupertoneApi = "Supertone API (\uD074\uB77C\uC6B0\uB4DC)";
        public const string VoiceLocalVoice = "\uB85C\uCEEC \uBCF4\uC774\uC2A4";
        public const string VoiceLocalMissing = "Supertonic \uC124\uCE58 \uD544\uC694: pip install \"supertonic[serve]\"";
        public const string VoiceLocalNotReady = "Supertonic \uB85C\uCEEC \uC5D4\uC9C4\uC774 \uC900\uBE44\uB418\uC9C0 \uC54A\uC558\uC2B5\uB2C8\uB2E4. \uC7A0\uC2DC \uD6C4 \uB2E4\uC2DC \uC2DC\uB3C4\uD558\uC138\uC694.";
        public const string VoiceGender = "\uC131\uBCC4";
        public const string GenderMale = "\uB0A8\uC131";
        public const string GenderFemale = "\uC5EC\uC131";
        public const string VoiceTone = "\uD1A4/\uBAA9\uC18C\uB9AC";
        public const string VoiceQuality = "\uD488\uC9C8(\uC2A4\uD15D)";
        public const string VoiceHotkeyMenu = "\uB2E8\uCD95\uD0A4 \uC124\uC815";
        public const string VoiceStopMenu = "\uC7AC\uC0DD \uC815\uC9C0";
        public const string VoiceStopped = "\uC7AC\uC0DD\uC744 \uC815\uC9C0\uD588\uC2B5\uB2C8\uB2E4.";
        public const string HotkeyToggleLabel = "\uCF1C\uAE30/\uB044\uAE30";
        public const string HotkeyStopLabel = "\uC7AC\uC0DD \uC815\uC9C0";
        public const string HotkeyLabel = "\uB2E8\uCD95\uD0A4";
        public const string HotkeyInputHint = "\uC5EC\uAE30\uB97C \uD074\uB9AD\uD558\uACE0 \uD0A4 \uC870\uD569\uC744 \uB204\uB974\uC138\uC694";
        public const string HotkeyNone = "(\uC5C6\uC74C)";
        public const string HotkeyNeedModifier = "Ctrl \uB610\uB294 Alt\uAC00 \uD3EC\uD568\uB41C \uC870\uD569\uC744 \uC0AC\uC6A9\uD558\uC138\uC694.";
        public const string HotkeyRegisterFailed = "\uB2E8\uCD95\uD0A4 \uB4F1\uB85D \uC2E4\uD328: \uB2E4\uB978 \uD504\uB85C\uADF8\uB7A8\uC774 \uC774\uBBF8 \uC0AC\uC6A9 \uC911\uC785\uB2C8\uB2E4.";
        public const string HotkeyClear = "\uC9C0\uC6B0\uAE30";
        public const string VoiceDisabledBalloon = "\uB4DC\uB798\uADF8 \uD14D\uC2A4\uD2B8 \uC77D\uAE30\uB97C \uAED0\uC2B5\uB2C8\uB2E4.";
        public const string LicenseMenu = "\uB77C\uC774\uC120\uC2A4";
        public const string LicenseRegister = "\uB77C\uC774\uC120\uC2A4 \uB4F1\uB85D";
        public const string LicenseStatus = "\uB77C\uC774\uC120\uC2A4 \uC0C1\uD0DC";
        public const string LicenseDeactivate = "\uC774 PC \uBE44\uD65C\uC131\uD654";
        public const string LicenseKey = "\uB77C\uC774\uC120\uC2A4 \uD0A4";
        public const string LicenseServer = "\uC11C\uBC84 URL";
        public const string Activate = "\uD65C\uC131\uD654";
        public const string Deactivate = "\uBE44\uD65C\uC131\uD654";
        public const string LicenseActivated = "\uB77C\uC774\uC120\uC2A4\uAC00 \uD65C\uC131\uD654\uB410\uC2B5\uB2C8\uB2E4.";
        public const string LicenseActivationFailed = "\uB77C\uC774\uC120\uC2A4 \uD65C\uC131\uD654 \uC2E4\uD328: ";
        public const string LicenseDeactivated = "\uC774 PC \uD65C\uC131\uD654\uB97C \uD574\uC81C\uD588\uC2B5\uB2C8\uB2E4.";
        public const string LicenseMissing = "\uB4F1\uB85D\uB41C \uB77C\uC774\uC120\uC2A4\uAC00 \uC5C6\uC2B5\uB2C8\uB2E4.";
        public const string LicenseValid = "\uD65C\uC131\uD654\uB428";
        public const string LicenseOfflineValid = "\uC624\uD504\uB77C\uC778 \uC0AC\uC6A9 \uAC00\uB2A5";
        public const string LicenseInvalid = "\uD65C\uC131\uD654 \uD544\uC694";
    }

    internal sealed class IndicatorContext : ApplicationContext
    {
        private readonly AppSettings settings;
        private readonly VoiceSettings voiceSettings;
        private readonly LicenseSettings licenseSettings;
        private readonly LicenseManager licenseManager;
        private readonly IndicatorAssets assets;
        private readonly IndicatorForm indicatorForm;
        private readonly System.Windows.Forms.Timer timer;
        private readonly NotifyIcon trayIcon;
        private readonly ToolStripMenuItem enabledItem;
        private readonly ToolStripMenuItem stateItem;
        private readonly ToolStripMenuItem sizeMenu;
        private readonly ToolStripMenuItem voiceMenu;
        private ToolStripMenuItem voiceEnabledItem;
        private ToolStripMenuItem voiceEngineSupertonicItem;
        private ToolStripMenuItem voiceEngineSupertoneApiItem;
        private const int VoiceToggleHotkeyId = 0xB001;
        private const int VoiceStopHotkeyId = 0xB002;

        private HotkeyWindow voiceHotkeyWindow;
        private HotkeySettingsForm hotkeySettingsForm;
        private volatile bool voiceStopRequested;
        private readonly ToolStripMenuItem showLabelItem;
        private readonly ToolStripMenuItem displayModeMenu;
        private ToolStripMenuItem colorMenu;
        private ToolStripMenuItem useLanguageColorsItem;
        private ToolStripMenuItem licenseMenu;
        private readonly List<ToolStripMenuItem> sizePresetItems = new List<ToolStripMenuItem>();
        private readonly List<ToolStripMenuItem> displayModeItems = new List<ToolStripMenuItem>();
        private readonly SynchronizationContext uiContext;
        private Icon currentTrayIcon;
        private SizeSettingsForm sizeSettingsForm;
        private FaceCenterSettingsForm faceCenterSettingsForm;
        private ImageSelectionForm imageSelectionForm;
        private VoiceSettingsForm voiceSettingsForm;
        private LicenseRegistrationForm licenseRegistrationForm;
        private SelectionDragWatcher selectionDragWatcher;
        private bool enabled = true;
        private bool trayMenuOpen;
        private bool voiceBusy;
        private bool missingVoiceConfigBalloonShown;
        private string lastText = "";
        private string lastVoiceText = "";
        private DateTime lastVoiceRequestUtc = DateTime.MinValue;
        private Point lastVisibilityCursorPosition;
        private DateTime lastVisibilityCursorMoveUtc = DateTime.UtcNow;
        private bool hasVisibilityCursorPosition;

        public IndicatorContext()
        {
            uiContext = SynchronizationContext.Current;
            if (uiContext == null)
            {
                uiContext = new WindowsFormsSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(uiContext);
            }

            settings = AppSettings.Load();
            voiceSettings = VoiceSettings.Load();
            licenseSettings = LicenseSettings.Load();
            licenseManager = new LicenseManager(licenseSettings);
            assets = new IndicatorAssets();
            indicatorForm = new IndicatorForm(assets, settings);

            enabledItem = new ToolStripMenuItem(TextResources.ToggleIndicator);
            enabledItem.Checked = true;
            enabledItem.CheckOnClick = true;
            enabledItem.CheckedChanged += OnEnabledChanged;

            stateItem = new ToolStripMenuItem(TextResources.CurrentStatePrefix + TextResources.Checking);
            stateItem.Enabled = false;

            sizeMenu = CreateSizeMenu();
            UpdateSizeMenuChecks();
            colorMenu = CreateColorMenu();
            voiceMenu = CreateVoiceMenu();
            voiceHotkeyWindow = new HotkeyWindow();
            ApplyVoiceHotkey(false);
            WarmUpLocalEngineIfNeeded();
            licenseMenu = CreateLicenseMenu();
            displayModeMenu = CreateDisplayModeMenu();
            UpdateDisplayModeMenuChecks();
            showLabelItem = new ToolStripMenuItem(TextResources.ShowLabel);
            showLabelItem.CheckOnClick = true;
            showLabelItem.Checked = settings.ShowLabel;
            showLabelItem.CheckedChanged += OnShowLabelChanged;

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Opening += OnTrayMenuOpening;
            menu.Opened += OnTrayMenuOpened;
            menu.Closed += OnTrayMenuClosed;
            menu.Items.Add(enabledItem);
            menu.Items.Add(stateItem);
            menu.Items.Add(new ToolStripMenuItem(TextResources.OpenImageFolder, null, OnOpenImageFolder));
            menu.Items.Add(new ToolStripMenuItem(TextResources.ChooseImage, null, OnChooseImage));
            menu.Items.Add(new ToolStripMenuItem(TextResources.ReloadImages, null, OnReloadImages));
            menu.Items.Add(new ToolStripMenuItem(TextResources.RemoveImageBackground, null, OnRemoveImageBackground));
            menu.Items.Add(sizeMenu);
            menu.Items.Add(displayModeMenu);
            menu.Items.Add(colorMenu);
            menu.Items.Add(showLabelItem);
            menu.Items.Add(new ToolStripMenuItem(TextResources.AdjustFaceCenter, null, OnOpenFaceCenterSettings));
            menu.Items.Add(voiceMenu);
            menu.Items.Add(licenseMenu);
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

            UpdateVoiceWatcher();
            ValidateLicenseInBackground(false);
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

        private ToolStripMenuItem CreateDisplayModeMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem(TextResources.DisplayModeMenu);

            ToolStripMenuItem alwaysFollowItem = new ToolStripMenuItem(TextResources.DisplayModeAlwaysFollow);
            alwaysFollowItem.Tag = CursorDisplayMode.AlwaysFollow;
            alwaysFollowItem.Click += OnDisplayModeClick;
            displayModeItems.Add(alwaysFollowItem);
            menu.DropDownItems.Add(alwaysFollowItem);

            ToolStripMenuItem showWhenIdleItem = new ToolStripMenuItem(TextResources.DisplayModeShowWhenIdle);
            showWhenIdleItem.Tag = CursorDisplayMode.ShowWhenIdle;
            showWhenIdleItem.Click += OnDisplayModeClick;
            displayModeItems.Add(showWhenIdleItem);
            menu.DropDownItems.Add(showWhenIdleItem);

            return menu;
        }

        private ToolStripMenuItem CreateColorMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem(TextResources.MascotColorMenu);
            useLanguageColorsItem = new ToolStripMenuItem(TextResources.UseLanguageColors);
            useLanguageColorsItem.CheckOnClick = true;
            useLanguageColorsItem.Checked = settings.UseLanguageColors;
            useLanguageColorsItem.CheckedChanged += OnUseLanguageColorsChanged;

            menu.DropDownItems.Add(useLanguageColorsItem);
            menu.DropDownItems.Add(new ToolStripSeparator());
            menu.DropDownItems.Add(new ToolStripMenuItem(TextResources.BaseColor, null, OnChooseBaseColor));
            menu.DropDownItems.Add(new ToolStripMenuItem(TextResources.KoreanColor, null, OnChooseKoreanColor));
            menu.DropDownItems.Add(new ToolStripMenuItem(TextResources.EnglishLowerColor, null, OnChooseEnglishLowerColor));
            menu.DropDownItems.Add(new ToolStripMenuItem(TextResources.EnglishUpperColor, null, OnChooseEnglishUpperColor));
            menu.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem labelColorMenu = new ToolStripMenuItem(TextResources.LabelColorMenu);
            labelColorMenu.DropDownItems.Add(new ToolStripMenuItem(TextResources.KoreanLabelColor, null, OnChooseKoreanLabelColor));
            labelColorMenu.DropDownItems.Add(new ToolStripMenuItem(TextResources.EnglishLowerLabelColor, null, OnChooseEnglishLowerLabelColor));
            labelColorMenu.DropDownItems.Add(new ToolStripMenuItem(TextResources.EnglishUpperLabelColor, null, OnChooseEnglishUpperLabelColor));
            menu.DropDownItems.Add(labelColorMenu);
            return menu;
        }

        private ToolStripMenuItem CreateVoiceMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem(TextResources.VoiceMenu);

            voiceEnabledItem = new ToolStripMenuItem(TextResources.VoiceOnDrag);
            voiceEnabledItem.CheckOnClick = true;
            voiceEnabledItem.Checked = voiceSettings.Enabled;
            voiceEnabledItem.CheckedChanged += OnVoiceEnabledChanged;

            ToolStripMenuItem engineMenu = new ToolStripMenuItem(TextResources.VoiceEngine);
            voiceEngineSupertonicItem = new ToolStripMenuItem(TextResources.VoiceEngineSupertonic, null, OnVoiceEngineSupertonic);
            voiceEngineSupertoneApiItem = new ToolStripMenuItem(TextResources.VoiceEngineSupertoneApi, null, OnVoiceEngineSupertoneApi);
            engineMenu.DropDownItems.Add(voiceEngineSupertonicItem);
            engineMenu.DropDownItems.Add(voiceEngineSupertoneApiItem);
            UpdateVoiceEngineChecks();

            menu.DropDownItems.Add(voiceEnabledItem);
            menu.DropDownItems.Add(new ToolStripMenuItem(TextResources.VoiceStopMenu, null, delegate { OnVoiceStopHotkeyPressed(); }));
            menu.DropDownItems.Add(engineMenu);
            menu.DropDownItems.Add(new ToolStripMenuItem(TextResources.VoiceHotkeyMenu, null, OnOpenHotkeySettings));
            menu.DropDownItems.Add(new ToolStripMenuItem(TextResources.VoiceSettings, null, OnOpenVoiceSettings));
            menu.DropDownItems.Add(new ToolStripMenuItem(TextResources.VoiceTestClipboard, null, OnVoiceTestClipboard));
            return menu;
        }

        private void OnOpenHotkeySettings(object sender, EventArgs e)
        {
            if (hotkeySettingsForm == null || hotkeySettingsForm.IsDisposed)
                hotkeySettingsForm = new HotkeySettingsForm(voiceSettings, OnVoiceHotkeySaved);

            hotkeySettingsForm.Reload();
            hotkeySettingsForm.Show();
            hotkeySettingsForm.Activate();
        }

        private void OnVoiceHotkeySaved()
        {
            ApplyVoiceHotkey(true);
            ShowVoiceBalloon(TextResources.VoiceSaved, 2200);
        }

        private void ApplyVoiceHotkey(bool notifyFailure)
        {
            if (voiceHotkeyWindow == null)
                return;

            voiceHotkeyWindow.Unregister(VoiceToggleHotkeyId);
            voiceHotkeyWindow.Unregister(VoiceStopHotkeyId);

            bool failed = false;
            if (voiceSettings.HotkeyKey != 0)
                failed |= !voiceHotkeyWindow.Register(VoiceToggleHotkeyId, (uint)voiceSettings.HotkeyModifiers, (uint)voiceSettings.HotkeyKey, OnVoiceHotkeyPressed);
            if (voiceSettings.StopHotkeyKey != 0)
                failed |= !voiceHotkeyWindow.Register(VoiceStopHotkeyId, (uint)voiceSettings.StopHotkeyModifiers, (uint)voiceSettings.StopHotkeyKey, OnVoiceStopHotkeyPressed);

            if (failed && notifyFailure)
                ShowVoiceBalloon(TextResources.HotkeyRegisterFailed, 3500);
        }

        private void OnVoiceStopHotkeyPressed()
        {
            voiceStopRequested = true;
            bool stopped = VoiceAudioPlayer.StopCurrent();
            if (stopped || voiceBusy)
                ShowVoiceBalloon(TextResources.VoiceStopped, 1200);
        }

        private void OnVoiceHotkeyPressed()
        {
            if (voiceEnabledItem != null)
            {
                voiceEnabledItem.Checked = !voiceEnabledItem.Checked;
                return;
            }

            voiceSettings.Enabled = !voiceSettings.Enabled;
            voiceSettings.Save();
            UpdateVoiceWatcher();
        }

        private void UpdateVoiceEngineChecks()
        {
            bool local = voiceSettings.UsesSupertonicEngine();
            if (voiceEngineSupertonicItem != null)
                voiceEngineSupertonicItem.Checked = local;
            if (voiceEngineSupertoneApiItem != null)
                voiceEngineSupertoneApiItem.Checked = !local;
        }

        private void OnVoiceEngineSupertonic(object sender, EventArgs e)
        {
            voiceSettings.Engine = VoiceSettings.EngineSupertonic;
            voiceSettings.Save();
            UpdateVoiceEngineChecks();
            WarmUpLocalEngineIfNeeded();
        }

        private void OnVoiceEngineSupertoneApi(object sender, EventArgs e)
        {
            voiceSettings.Engine = VoiceSettings.EngineSupertoneApi;
            voiceSettings.Save();
            UpdateVoiceEngineChecks();
        }

        private void WarmUpLocalEngineIfNeeded()
        {
            if (!voiceSettings.Enabled || !voiceSettings.UsesSupertonicEngine())
                return;

            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    SupertonicLocalClient.WarmUp();
                }
                catch
                {
                }
            });
        }

        private ToolStripMenuItem CreateLicenseMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem(TextResources.LicenseMenu);
            menu.DropDownItems.Add(new ToolStripMenuItem(TextResources.LicenseRegister, null, OnOpenLicenseRegistration));
            menu.DropDownItems.Add(new ToolStripMenuItem(TextResources.LicenseStatus, null, OnShowLicenseStatus));
            menu.DropDownItems.Add(new ToolStripMenuItem(TextResources.LicenseDeactivate, null, OnDeactivateLicense));
            return menu;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (trayMenuOpen)
            {
                indicatorForm.Hide();
                return;
            }

            Point cursor = Cursor.Position;
            string text = ImeStateReader.GetIndicatorText();
            stateItem.Text = TextResources.CurrentStatePrefix + text;

            if (text != lastText)
            {
                lastText = text;
                indicatorForm.SetIndicatorText(text);
                ReplaceTrayIcon(text);
            }

            indicatorForm.TickAnimations(cursor);

            if (!enabled)
            {
                indicatorForm.Hide();
                return;
            }

            if (!ShouldShowForDisplayMode(cursor))
            {
                indicatorForm.Hide();
                return;
            }

            Rectangle area = Screen.FromPoint(cursor).WorkingArea;
            int x = cursor.X + 8;
            int y = cursor.Y - (indicatorForm.Height / 2) + 6;

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

        private bool ShouldShowForDisplayMode(Point cursor)
        {
            if (settings.DisplayMode == CursorDisplayMode.AlwaysFollow)
                return true;

            const int idleDelayMilliseconds = 800;
            DateTime now = DateTime.UtcNow;
            if (!hasVisibilityCursorPosition)
            {
                lastVisibilityCursorPosition = cursor;
                lastVisibilityCursorMoveUtc = now;
                hasVisibilityCursorPosition = true;
                return false;
            }

            if (cursor != lastVisibilityCursorPosition)
            {
                lastVisibilityCursorPosition = cursor;
                lastVisibilityCursorMoveUtc = now;
                return false;
            }

            return (now - lastVisibilityCursorMoveUtc).TotalMilliseconds >= idleDelayMilliseconds;
        }

        private void OnTrayMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PauseIndicatorForTrayMenu();
        }

        private void OnTrayMenuOpened(object sender, EventArgs e)
        {
            PauseIndicatorForTrayMenu();
        }

        private void OnTrayMenuClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            trayMenuOpen = false;
            ResetIdleVisibility();
            if (!timer.Enabled)
                timer.Start();
        }

        private void PauseIndicatorForTrayMenu()
        {
            trayMenuOpen = true;
            indicatorForm.Hide();
            if (timer.Enabled)
                timer.Stop();
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

        private void OnChooseImage(object sender, EventArgs e)
        {
            if (imageSelectionForm == null || imageSelectionForm.IsDisposed)
            {
                imageSelectionForm = new ImageSelectionForm(assets, OnImageSelectionChanged);
                imageSelectionForm.FormClosed += OnImageSelectionFormClosed;
            }

            imageSelectionForm.RefreshSelection();
            imageSelectionForm.Show();
            imageSelectionForm.Activate();
        }

        private void OnImageSelectionFormClosed(object sender, FormClosedEventArgs e)
        {
            imageSelectionForm = null;
        }

        private void OnImageSelectionChanged(string message)
        {
            assets.Reload();
            indicatorForm.RefreshAssets();

            if (faceCenterSettingsForm != null && !faceCenterSettingsForm.IsDisposed)
                faceCenterSettingsForm.RefreshPreview();

            ShowImageSelectionResult(message);
        }

        private void OnRemoveImageBackground(object sender, EventArgs e)
        {
            Directory.CreateDirectory(assets.ImageDirectory);

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = TextResources.RemoveImageBackground;
                dialog.InitialDirectory = assets.ImageDirectory;
                dialog.Multiselect = true;
                dialog.Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*";
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                CutoutOptions options;
                using (CutoutOptionsForm optionsForm = new CutoutOptionsForm())
                {
                    if (optionsForm.ShowDialog() != DialogResult.OK)
                        return;

                    options = optionsForm.Options;
                }

                int saved = 0;
                int failed = 0;
                foreach (string path in dialog.FileNames)
                {
                    try
                    {
                        string outputPath = BackgroundRemover.GetOutputPath(path);
                        List<CutoutLine> cutoutLines = new List<CutoutLine>();
                        if (options.UseCutoutLine)
                        {
                            using (CutoutLineSelectionForm lineForm = new CutoutLineSelectionForm(path, options.LineKind))
                            {
                                if (lineForm.ShowDialog() != DialogResult.OK)
                                    continue;

                                cutoutLines = lineForm.Lines;
                            }
                        }

                        BackgroundRemover.SaveTransparentCopy(path, outputPath, options.ResizeEnabled ? options.MaxSize : 0, cutoutLines);
                        saved++;
                    }
                    catch
                    {
                        failed++;
                    }
                }

                assets.Reload();
                indicatorForm.RefreshAssets();
                ShowBackgroundRemovalResult(saved, failed);
            }
        }

        private void OnSizePresetClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null || item.Tag == null)
                return;

            SetSizePercent((int)item.Tag);
        }

        private void OnDisplayModeClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null || item.Tag == null)
                return;

            settings.DisplayMode = (CursorDisplayMode)item.Tag;
            settings.Save();
            ResetIdleVisibility();
            UpdateDisplayModeMenuChecks();
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

        private void OnOpenFaceCenterSettings(object sender, EventArgs e)
        {
            if (faceCenterSettingsForm == null || faceCenterSettingsForm.IsDisposed)
            {
                faceCenterSettingsForm = new FaceCenterSettingsForm(assets, settings, SetFaceCenter);
                faceCenterSettingsForm.FormClosed += OnFaceCenterSettingsClosed;
            }

            faceCenterSettingsForm.RefreshPreview();
            faceCenterSettingsForm.Show();
            faceCenterSettingsForm.Activate();
        }

        private void OnSizeSettingsClosed(object sender, FormClosedEventArgs e)
        {
            sizeSettingsForm = null;
        }

        private void OnFaceCenterSettingsClosed(object sender, FormClosedEventArgs e)
        {
            faceCenterSettingsForm = null;
        }

        private void OnVoiceEnabledChanged(object sender, EventArgs e)
        {
            voiceSettings.Enabled = voiceEnabledItem.Checked;
            voiceSettings.Save();
            missingVoiceConfigBalloonShown = false;
            UpdateVoiceWatcher();

            if (voiceSettings.Enabled)
            {
                ShowVoiceBalloon(TextResources.VoiceReady, 2000);
                WarmUpLocalEngineIfNeeded();
            }
            else
            {
                ShowVoiceBalloon(TextResources.VoiceDisabledBalloon, 1500);
            }
        }

        private void OnOpenVoiceSettings(object sender, EventArgs e)
        {
            if (voiceSettingsForm == null || voiceSettingsForm.IsDisposed)
            {
                voiceSettingsForm = new VoiceSettingsForm(voiceSettings, OnVoiceSettingsSaved);
                voiceSettingsForm.FormClosed += OnVoiceSettingsFormClosed;
            }

            voiceSettingsForm.Reload();
            voiceSettingsForm.Show();
            voiceSettingsForm.Activate();
        }

        private void OnVoiceSettingsFormClosed(object sender, FormClosedEventArgs e)
        {
            voiceSettingsForm = null;
        }

        private void OnVoiceSettingsSaved()
        {
            voiceSettings.Save();
            missingVoiceConfigBalloonShown = false;
            UpdateVoiceEngineChecks();

            if (voiceEnabledItem != null && voiceEnabledItem.Checked != voiceSettings.Enabled)
                voiceEnabledItem.Checked = voiceSettings.Enabled;
            else
                UpdateVoiceWatcher();

            WarmUpLocalEngineIfNeeded();
            ShowVoiceBalloon(TextResources.VoiceSaved, 2200);
        }

        private void OnVoiceTestClipboard(object sender, EventArgs e)
        {
            string text = "";
            try
            {
                if (Clipboard.ContainsText())
                    text = Clipboard.GetText();
            }
            catch
            {
            }

            SpeakSanitizedText(text, true);
        }

        private void OnOpenLicenseRegistration(object sender, EventArgs e)
        {
            if (licenseRegistrationForm == null || licenseRegistrationForm.IsDisposed)
            {
                licenseRegistrationForm = new LicenseRegistrationForm(licenseSettings, licenseManager, OnLicenseChanged);
                licenseRegistrationForm.FormClosed += OnLicenseRegistrationFormClosed;
            }

            licenseRegistrationForm.Reload();
            licenseRegistrationForm.Show();
            licenseRegistrationForm.Activate();
        }

        private void OnLicenseRegistrationFormClosed(object sender, FormClosedEventArgs e)
        {
            licenseRegistrationForm = null;
        }

        private void OnLicenseChanged(LicenseStatus status)
        {
            licenseSettings.Save();
            ShowLicenseBalloon(status.Message.Length > 0 ? status.Message : LicenseStatusText(status), 3000);
        }

        private void OnShowLicenseStatus(object sender, EventArgs e)
        {
            ValidateLicenseInBackground(true);
        }

        private void OnDeactivateLicense(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                LicenseStatus status;
                try
                {
                    status = licenseManager.Deactivate();
                }
                catch (Exception ex)
                {
                    status = new LicenseStatus();
                    status.State = LicenseState.Invalid;
                    status.Message = ex.Message;
                }

                PostToUi(delegate
                {
                    if (licenseRegistrationForm != null && !licenseRegistrationForm.IsDisposed)
                        licenseRegistrationForm.Reload();

                    ShowLicenseBalloon(status.Message.Length > 0 ? status.Message : LicenseStatusText(status), 3500);
                });
            });
        }

        private void ValidateLicenseInBackground(bool showBalloon)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                LicenseStatus status;
                try
                {
                    status = licenseManager.GetStatus(true);
                }
                catch (Exception ex)
                {
                    status = new LicenseStatus();
                    status.State = LicenseState.Invalid;
                    status.Message = ex.Message;
                }

                if (!showBalloon && status.State == LicenseState.Missing)
                    return;

                PostToUi(delegate
                {
                    if (licenseRegistrationForm != null && !licenseRegistrationForm.IsDisposed)
                        licenseRegistrationForm.Reload();

                    ShowLicenseBalloon(LicenseStatusText(status), 3500);
                });
            });
        }

        private static string LicenseStatusText(LicenseStatus status)
        {
            if (status.State == LicenseState.Active)
                return TextResources.LicenseValid + " (" + status.Detail + ")";
            if (status.State == LicenseState.OfflineActive)
                return TextResources.LicenseOfflineValid + " (" + status.Detail + ")";
            if (status.State == LicenseState.Missing)
                return TextResources.LicenseMissing;
            return TextResources.LicenseInvalid + (status.Message.Length > 0 ? ": " + status.Message : "");
        }

        private void ShowLicenseBalloon(string text, int timeout)
        {
            trayIcon.BalloonTipTitle = TextResources.LicenseMenu;
            trayIcon.BalloonTipText = text;
            trayIcon.ShowBalloonTip(timeout);
        }

        private void UpdateVoiceWatcher()
        {
            if (voiceSettings.Enabled)
            {
                if (selectionDragWatcher == null)
                    selectionDragWatcher = new SelectionDragWatcher(uiContext, OnSelectionDragCompleted);
                selectionDragWatcher.Start();
                return;
            }

            if (selectionDragWatcher != null)
                selectionDragWatcher.Stop();
        }

        private void OnSelectionDragCompleted()
        {
            VoiceDebugLog.Write("drag completed; enabled=" + voiceSettings.Enabled);
            if (!voiceSettings.Enabled)
                return;

            DateTime now = DateTime.UtcNow;
            if ((now - lastVoiceRequestUtc).TotalMilliseconds < 900)
            {
                VoiceDebugLog.Write("skip: throttle 900ms");
                return;
            }

            string selectedText = ClipboardSelectionReader.TryCopySelectionText();
            VoiceDebugLog.Write("copied length=" + (selectedText == null ? -1 : selectedText.Length));
            SpeakSanitizedText(selectedText, false);
        }

        private void SpeakSanitizedText(string rawText, bool manual)
        {
            string text = VoiceTextSanitizer.Sanitize(rawText, voiceSettings.MaxTextLength);
            VoiceDebugLog.Write("sanitized length=" + text.Length);
            if (text.Length == 0)
            {
                if (manual)
                    ShowVoiceBalloon(TextResources.VoiceNoText, 2500);
                return;
            }

            DateTime now = DateTime.UtcNow;
            if (!manual && text == lastVoiceText && (now - lastVoiceRequestUtc).TotalSeconds < 2)
            {
                VoiceDebugLog.Write("skip: duplicate text");
                return;
            }

            SpeakText(text, manual);
        }

        private void SpeakText(string text, bool manual)
        {
            bool useLocalEngine = voiceSettings.UsesSupertonicEngine();
            string apiKey = "";
            if (!useLocalEngine)
            {
                apiKey = VoiceSettings.LoadApiKey();
                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(voiceSettings.VoiceId.Trim()))
                {
                    if (manual || !missingVoiceConfigBalloonShown)
                        ShowVoiceBalloon(TextResources.VoiceMissingConfig, 3500);

                    missingVoiceConfigBalloonShown = true;
                    return;
                }
            }

            if (voiceBusy)
            {
                VoiceDebugLog.Write("skip: voiceBusy");
                return;
            }

            VoiceRequestOptions request = voiceSettings.CreateRequest(text, apiKey);
            voiceBusy = true;
            voiceStopRequested = false;
            lastVoiceText = text;
            lastVoiceRequestUtc = DateTime.UtcNow;
            VoiceDebugLog.Write("synth start; engine=" + (useLocalEngine ? "supertonic" : "supertone_api"));

            ThreadPool.QueueUserWorkItem(delegate
            {
                string error = null;
                try
                {
                    string audioPath = useLocalEngine
                        ? SupertonicLocalClient.CreateSpeechFile(request)
                        : SupertoneTtsClient.CreateSpeechFile(request);
                    if (voiceStopRequested)
                    {
                        VoiceDebugLog.Write("playback skipped: stop requested");
                        try { File.Delete(audioPath); } catch { }
                    }
                    else
                    {
                        VoiceDebugLog.Write("synth ok; playing");
                        VoiceAudioPlayer.PlayWavAndDelete(audioPath);
                        VoiceDebugLog.Write("play done");
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    VoiceDebugLog.Write("synth FAILED: " + ex.Message);
                }

                PostToUi(delegate
                {
                    voiceBusy = false;
                    if (!string.IsNullOrEmpty(error))
                        ShowVoiceBalloon(TextResources.VoiceFailed + error, 4500);
                });
            });
        }

        private void PostToUi(Action action)
        {
            if (uiContext != null)
                uiContext.Post(delegate { action(); }, null);
            else
                action();
        }

        private void ShowVoiceBalloon(string text, int timeout)
        {
            trayIcon.BalloonTipTitle = TextResources.VoiceMenu;
            trayIcon.BalloonTipText = text;
            trayIcon.ShowBalloonTip(timeout);
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

        private void ResetIdleVisibility()
        {
            hasVisibilityCursorPosition = false;
            lastVisibilityCursorMoveUtc = DateTime.UtcNow;

            if (settings.DisplayMode == CursorDisplayMode.ShowWhenIdle)
                indicatorForm.Hide();
        }

        private void SetFaceCenter(string stateKey, IndicatorPose pose, PointF center)
        {
            settings.SetLabelCenter(stateKey, pose, center);
            settings.Save();
            indicatorForm.RefreshFaceCenter();

            if (faceCenterSettingsForm != null && !faceCenterSettingsForm.IsDisposed)
                faceCenterSettingsForm.RefreshPreview();
        }

        private void OnShowLabelChanged(object sender, EventArgs e)
        {
            settings.ShowLabel = showLabelItem.Checked;
            settings.Save();
            indicatorForm.RefreshFaceCenter();

            if (faceCenterSettingsForm != null && !faceCenterSettingsForm.IsDisposed)
                faceCenterSettingsForm.RefreshPreview();
        }

        private void OnUseLanguageColorsChanged(object sender, EventArgs e)
        {
            settings.UseLanguageColors = useLanguageColorsItem.Checked;
            settings.Save();
            indicatorForm.RefreshColors();
        }

        private void OnChooseBaseColor(object sender, EventArgs e)
        {
            ChooseMascotColor(settings.BaseMascotColor, delegate(Color color) { settings.BaseMascotColor = color; });
        }

        private void OnChooseKoreanColor(object sender, EventArgs e)
        {
            ChooseMascotColor(settings.KoreanMascotColor, delegate(Color color) { settings.KoreanMascotColor = color; });
        }

        private void OnChooseEnglishLowerColor(object sender, EventArgs e)
        {
            ChooseMascotColor(settings.EnglishLowerMascotColor, delegate(Color color) { settings.EnglishLowerMascotColor = color; });
        }

        private void OnChooseEnglishUpperColor(object sender, EventArgs e)
        {
            ChooseMascotColor(settings.EnglishUpperMascotColor, delegate(Color color) { settings.EnglishUpperMascotColor = color; });
        }

        private void OnChooseKoreanLabelColor(object sender, EventArgs e)
        {
            ChooseMascotColor(settings.KoreanLabelColor, delegate(Color color) { settings.KoreanLabelColor = color; });
        }

        private void OnChooseEnglishLowerLabelColor(object sender, EventArgs e)
        {
            ChooseMascotColor(settings.EnglishLowerLabelColor, delegate(Color color) { settings.EnglishLowerLabelColor = color; });
        }

        private void OnChooseEnglishUpperLabelColor(object sender, EventArgs e)
        {
            ChooseMascotColor(settings.EnglishUpperLabelColor, delegate(Color color) { settings.EnglishUpperLabelColor = color; });
        }

        private void ChooseMascotColor(Color initialColor, Action<Color> apply)
        {
            using (ColorDialog dialog = new ColorDialog())
            {
                dialog.FullOpen = true;
                dialog.Color = initialColor;
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                apply(dialog.Color);
                settings.Save();
                indicatorForm.RefreshColors();

                if (faceCenterSettingsForm != null && !faceCenterSettingsForm.IsDisposed)
                    faceCenterSettingsForm.RefreshPreview();
            }
        }

        private void UpdateSizeMenuChecks()
        {
            foreach (ToolStripMenuItem item in sizePresetItems)
                item.Checked = item.Tag != null && (int)item.Tag == settings.SizePercent;

            sizeMenu.Text = TextResources.SizeMenu + " (" + settings.SizePercent + "%)";
        }

        private void UpdateDisplayModeMenuChecks()
        {
            foreach (ToolStripMenuItem item in displayModeItems)
                item.Checked = item.Tag != null && (CursorDisplayMode)item.Tag == settings.DisplayMode;
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
                : "No custom images found. Put idle.png, point.png, cheer.png, or state-pose images in the images folder.";
            trayIcon.ShowBalloonTip(2500);
        }

        private void ShowBackgroundRemovalResult(int saved, int failed)
        {
            trayIcon.BalloonTipTitle = TextResources.RemoveImageBackground;
            trayIcon.BalloonTipText = failed == 0
                ? "Saved " + saved + " transparent image(s)."
                : "Saved " + saved + " transparent image(s), failed " + failed + ".";
            trayIcon.ShowBalloonTip(3000);
        }

        private void ShowImageSelectionResult(string message)
        {
            trayIcon.BalloonTipTitle = TextResources.ChooseImage;
            trayIcon.BalloonTipText = message;
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
                if (faceCenterSettingsForm != null)
                    faceCenterSettingsForm.Dispose();
                if (imageSelectionForm != null)
                    imageSelectionForm.Dispose();
                if (voiceSettingsForm != null)
                    voiceSettingsForm.Dispose();
                if (licenseRegistrationForm != null)
                    licenseRegistrationForm.Dispose();
                if (selectionDragWatcher != null)
                    selectionDragWatcher.Dispose();
                if (hotkeySettingsForm != null)
                    hotkeySettingsForm.Dispose();
                if (voiceHotkeyWindow != null)
                    voiceHotkeyWindow.Dispose();
                SupertonicLocalClient.StopServerIfStarted();
            }

            base.Dispose(disposing);
        }
    }

    internal sealed class IndicatorForm : Form
    {
        private const int PointMilliseconds = 1000;
        private const int CursorMovingMilliseconds = 180;
        private static readonly Color TransparentBackColor = Color.FromArgb(255, 1, 2, 3);

        private readonly IndicatorAssets assets;
        private readonly AppSettings settings;
        private readonly TintedImageCache tintedImageCache = new TintedImageCache();
        private readonly Font textFont;
        private string indicatorText = Labels.Korean;
        private int sizePercent;
        private IndicatorPose currentPose = IndicatorPose.Idle;
        private DateTime stateChangedAtUtc = DateTime.UtcNow;
        private DateTime lastCursorMoveUtc = DateTime.MinValue;
        private Point lastCursorPosition;
        private bool hasCursorPosition;

        public IndicatorForm(IndicatorAssets assets, AppSettings settings)
        {
            this.assets = assets;
            this.settings = settings;
            this.sizePercent = AppSettings.ClampSizePercent(settings.SizePercent);
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

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_NCHITTEST)
            {
                m.Result = new IntPtr(NativeMethods.HTTRANSPARENT);
                return;
            }

            if (m.Msg == NativeMethods.WM_MOUSEACTIVATE)
            {
                m.Result = new IntPtr(NativeMethods.MA_NOACTIVATEANDEAT);
                return;
            }

            base.WndProc(ref m);
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
            tintedImageCache.Clear();
            ApplyDesiredSize();
            stateChangedAtUtc = DateTime.UtcNow;
            currentPose = IndicatorPose.Point;
            Invalidate();
        }

        public void RefreshFaceCenter()
        {
            tintedImageCache.Clear();
            Invalidate();
        }

        public void RefreshColors()
        {
            tintedImageCache.Clear();
            Invalidate();
        }

        public void TickAnimations(Point cursorPosition)
        {
            TrackCursorMovement(cursorPosition);

            IndicatorPose nextPose = CalculatePose();
            if (nextPose != currentPose)
            {
                currentPose = nextPose;
                ApplyDesiredSize();
                Invalidate();
            }

            bool mascotImage;
            IndicatorImage image = GetCurrentImage(out mascotImage);
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

            bool mascotImage;
            IndicatorImage image = GetCurrentImage(out mascotImage);
            float scale = GetPopScale();

            if (image != null)
            {
                Rectangle imageRect = DrawImageIndicator(e.Graphics, image.Image, scale, mascotImage);
                if (mascotImage && settings.ShowLabel)
                    DrawFaceLabel(e.Graphics, imageRect, indicatorText, scale);
                return;
            }

            if (settings.ShowLabel)
                DrawTextIndicator(e.Graphics, scale);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (textFont != null)
                    textFont.Dispose();
                if (tintedImageCache != null)
                    tintedImageCache.Dispose();
            }

            base.Dispose(disposing);
        }

        private IndicatorPose CalculatePose()
        {
            DateTime now = DateTime.UtcNow;
            double sinceChange = (now - stateChangedAtUtc).TotalMilliseconds;
            if (sinceChange < PointMilliseconds && assets.HasPoseForLabel(indicatorText, IndicatorPose.Point))
                return IndicatorPose.Point;

            if ((now - lastCursorMoveUtc).TotalMilliseconds < CursorMovingMilliseconds && assets.HasPoseForLabel(indicatorText, IndicatorPose.Cheer))
                return IndicatorPose.Cheer;

            return IndicatorPose.Idle;
        }

        private void TrackCursorMovement(Point cursorPosition)
        {
            if (!hasCursorPosition)
            {
                lastCursorPosition = cursorPosition;
                hasCursorPosition = true;
                return;
            }

            if (cursorPosition == lastCursorPosition)
                return;

            lastCursorPosition = cursorPosition;
            lastCursorMoveUtc = DateTime.UtcNow;
        }

        private IndicatorImage GetCurrentImage(out bool mascotImage)
        {
            return assets.GetImage(indicatorText, currentPose, out mascotImage);
        }

        private void ApplyDesiredSize()
        {
            Size target = GetDesiredSize();
            if (Width != target.Width || Height != target.Height)
                Size = target;
        }

        private Size GetDesiredSize()
        {
            bool mascotImage;
            IndicatorImage image = GetCurrentImage(out mascotImage);
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

        private Rectangle DrawImageIndicator(Graphics graphics, Image image, float popScale, bool mascotImage)
        {
            Size drawSize = GetImageDrawSize(image);
            int scaledWidth = Math.Max(1, (int)Math.Round(drawSize.Width * popScale));
            int scaledHeight = Math.Max(1, (int)Math.Round(drawSize.Height * popScale));
            Rectangle rect = new Rectangle(
                (Width - scaledWidth) / 2,
                (Height - scaledHeight) / 2,
                scaledWidth,
                scaledHeight);

            if (mascotImage)
            {
                if (ImageAnimator.CanAnimate(image))
                {
                    if (settings.UseLanguageColors)
                    {
                        Color tint = settings.GetMascotColor(indicatorText);
                        using (Bitmap tinted = MascotColorizer.CreateTintedBitmap(image, tint, settings.GetFaceCenter(currentPose)))
                        {
                            graphics.DrawImage(tinted, rect);
                        }
                    }
                    else
                    {
                        graphics.DrawImage(image, rect);
                    }
                }
                else
                {
                    Color tint = settings.GetMascotColor(indicatorText);
                    Bitmap tinted = tintedImageCache.Get(image, tint, settings.GetFaceCenter(currentPose));
                    graphics.DrawImage(tinted, rect);
                }
            }
            else
            {
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
            }

            return rect;
        }

        private void DrawFaceLabel(Graphics graphics, Rectangle imageRect, string text, float popScale)
        {
            PointF faceCenter = settings.GetLabelCenter(text, currentPose);
            RectangleF faceRect = LabelGeometry.CreateLabelRect(imageRect, faceCenter);

            float fontSize = Math.Max(7.0f, imageRect.Height * (text == Labels.Korean ? 0.155f : 0.14f));
            using (Font font = new Font("Malgun Gothic", fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
            using (SolidBrush fill = new SolidBrush(settings.GetLabelColor(text)))
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

    internal sealed class TintedImageCache : IDisposable
    {
        private readonly Dictionary<string, Bitmap> cache = new Dictionary<string, Bitmap>(StringComparer.Ordinal);

        public Bitmap Get(Image image, Color tint, PointF protectedFaceCenter)
        {
            string key = RuntimeHelpers.GetHashCode(image).ToString(CultureInfo.InvariantCulture)
                + "|" + tint.ToArgb().ToString(CultureInfo.InvariantCulture)
                + "|" + protectedFaceCenter.X.ToString("0.###", CultureInfo.InvariantCulture)
                + "," + protectedFaceCenter.Y.ToString("0.###", CultureInfo.InvariantCulture);

            Bitmap bitmap;
            if (!cache.TryGetValue(key, out bitmap))
            {
                bitmap = MascotColorizer.CreateTintedBitmap(image, tint, protectedFaceCenter);
                cache[key] = bitmap;
            }

            return bitmap;
        }

        public void Clear()
        {
            foreach (Bitmap bitmap in cache.Values)
                bitmap.Dispose();

            cache.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }

    internal static class LabelGeometry
    {
        public static RectangleF CreateLabelRect(Rectangle imageRect, PointF center)
        {
            float width = imageRect.Width * 0.38f;
            float height = imageRect.Height * 0.26f;
            float centerX = imageRect.Left + imageRect.Width * center.X;
            float centerY = imageRect.Top + imageRect.Height * center.Y;
            float x = centerX - (width / 2.0f);
            float y = centerY - (height / 2.0f);

            if (x < imageRect.Left)
                x = imageRect.Left;
            if (y < imageRect.Top)
                y = imageRect.Top;
            if (x + width > imageRect.Right)
                x = imageRect.Right - width;
            if (y + height > imageRect.Bottom)
                y = imageRect.Bottom - height;

            return new RectangleF(x, y, width, height);
        }
    }

    internal sealed class IndicatorAssets : IDisposable
    {
        private static readonly string[] Extensions = new[] { ".gif", ".png", ".jpg", ".jpeg", ".jfif", ".bmp" };
        private Dictionary<IndicatorPose, IndicatorImage> poseImages = new Dictionary<IndicatorPose, IndicatorImage>();
        private Dictionary<string, IndicatorImage> statePoseImages = new Dictionary<string, IndicatorImage>(StringComparer.Ordinal);
        private Dictionary<string, IndicatorImage> legacyImages = new Dictionary<string, IndicatorImage>();

        public IndicatorAssets()
        {
            Reload();
        }

        public string ImageDirectory
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images"); }
        }

        public int LoadedCount
        {
            get { return poseImages.Count + statePoseImages.Count + legacyImages.Count; }
        }

        public IndicatorImage GetImage(string label, IndicatorPose pose, out bool mascotImage)
        {
            string stateKey = IndicatorStates.FromLabel(label);
            IndicatorImage image = GetStatePoseExact(stateKey, pose);
            if (image != null)
            {
                mascotImage = true;
                return image;
            }

            image = GetPoseExact(pose);
            if (image != null)
            {
                mascotImage = true;
                return image;
            }

            image = GetPoseExact(IndicatorPose.Idle);
            if (image != null)
            {
                mascotImage = true;
                return image;
            }

            mascotImage = false;
            return GetLegacy(label);
        }

        public IndicatorImage GetImageByStateKey(string stateKey, IndicatorPose pose, out bool mascotImage)
        {
            return GetImage(IndicatorStates.ToLabel(stateKey), pose, out mascotImage);
        }

        public IndicatorImage GetPose(IndicatorPose pose)
        {
            return GetPoseExact(pose);
        }

        public bool HasPoseForLabel(string label, IndicatorPose pose)
        {
            string stateKey = IndicatorStates.FromLabel(label);
            return GetStatePoseExact(stateKey, pose) != null || GetPoseExact(pose) != null;
        }

        public string GetSharedPoseFileName(IndicatorPose pose)
        {
            return GetExistingSlotFileName(new[] { IndicatorPoseHelper.GetKey(pose) }, false);
        }

        public string GetStatePoseFileName(string stateKey, IndicatorPose pose)
        {
            return GetExistingSlotFileName(GetStatePoseBaseNames(stateKey, pose), true);
        }

        public void InstallSharedPoseImage(IndicatorPose pose, string sourcePath)
        {
            InstallSlotImage(new[] { IndicatorPoseHelper.GetKey(pose) }, false, sourcePath);
        }

        public void InstallStatePoseImage(string stateKey, IndicatorPose pose, string sourcePath)
        {
            InstallSlotImage(GetStatePoseBaseNames(stateKey, pose), true, sourcePath);
        }

        public void ClearSharedPoseImage(IndicatorPose pose)
        {
            ClearSlotImages(new[] { IndicatorPoseHelper.GetKey(pose) }, false);
        }

        public void ClearStatePoseImage(string stateKey, IndicatorPose pose)
        {
            ClearSlotImages(GetStatePoseBaseNames(stateKey, pose), true);
        }

        public void Reload()
        {
            Dictionary<IndicatorPose, IndicatorImage> oldPoseImages = poseImages;
            Dictionary<string, IndicatorImage> oldStatePoseImages = statePoseImages;
            Dictionary<string, IndicatorImage> oldLegacyImages = legacyImages;
            Dictionary<IndicatorPose, IndicatorImage> newPoseImages = new Dictionary<IndicatorPose, IndicatorImage>();
            Dictionary<string, IndicatorImage> newStatePoseImages = new Dictionary<string, IndicatorImage>(StringComparer.Ordinal);
            Dictionary<string, IndicatorImage> newLegacyImages = new Dictionary<string, IndicatorImage>();

            foreach (IndicatorPose pose in IndicatorPoseHelper.All)
                TryLoadPose(newPoseImages, ImageDirectory, pose, IndicatorPoseHelper.GetKey(pose));

            foreach (string stateKey in IndicatorStates.All)
            {
                foreach (IndicatorPose pose in IndicatorPoseHelper.All)
                    TryLoadStatePose(newStatePoseImages, ImageDirectory, stateKey, pose);
            }

            TryLoadLegacy(newLegacyImages, ImageDirectory, Labels.Korean, "han");
            TryLoadLegacy(newLegacyImages, ImageDirectory, Labels.EnglishLower, "en");

            poseImages = newPoseImages;
            statePoseImages = newStatePoseImages;
            legacyImages = newLegacyImages;

            DisposeImages(oldPoseImages.Values);
            DisposeImages(oldStatePoseImages.Values);
            DisposeImages(oldLegacyImages.Values);
        }

        public void Dispose()
        {
            DisposeImages(poseImages.Values);
            DisposeImages(statePoseImages.Values);
            DisposeImages(legacyImages.Values);
            poseImages.Clear();
            statePoseImages.Clear();
            legacyImages.Clear();
        }

        private static void DisposeImages(IEnumerable<IndicatorImage> images)
        {
            foreach (IndicatorImage image in images)
                image.Dispose();
        }

        private static string[] GetStatePoseBaseNames(string stateKey, IndicatorPose pose)
        {
            string poseKey = IndicatorPoseHelper.GetKey(pose);
            if (stateKey == IndicatorStates.EnglishUpper)
                return new[] { "upper-" + poseKey, "EN-" + poseKey, "caps-" + poseKey };

            List<string> names = new List<string>();
            foreach (string prefix in IndicatorStates.GetFilePrefixes(stateKey))
                names.Add(prefix + "-" + poseKey);
            return names.ToArray();
        }

        private string GetExistingSlotFileName(IEnumerable<string> baseNames, bool exactFileName)
        {
            foreach (string baseName in baseNames)
            {
                foreach (string extension in Extensions)
                {
                    string path = exactFileName
                        ? FindExactImagePath(ImageDirectory, baseName + extension)
                        : Path.Combine(ImageDirectory, baseName + extension);
                    if (File.Exists(path))
                        return Path.GetFileName(path);
                }
            }

            return "";
        }

        private void InstallSlotImage(string[] baseNames, bool exactFileName, string sourcePath)
        {
            string extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            if (!IsSupportedExtension(extension))
                throw new InvalidOperationException("Unsupported image file.");

            Directory.CreateDirectory(ImageDirectory);
            byte[] bytes = File.ReadAllBytes(sourcePath);
            ClearSlotImages(baseNames, exactFileName);

            string targetBaseName = baseNames.Length > 0 ? baseNames[0] : "idle";
            string targetPath = Path.Combine(ImageDirectory, targetBaseName + extension);
            File.WriteAllBytes(targetPath, bytes);
        }

        private void ClearSlotImages(IEnumerable<string> baseNames, bool exactFileName)
        {
            foreach (string baseName in baseNames)
            {
                foreach (string extension in Extensions)
                {
                    string path = exactFileName
                        ? FindExactImagePath(ImageDirectory, baseName + extension)
                        : Path.Combine(ImageDirectory, baseName + extension);
                    try
                    {
                        if (File.Exists(path))
                            File.Delete(path);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static bool IsSupportedExtension(string extension)
        {
            foreach (string candidate in Extensions)
            {
                if (candidate.Equals(extension, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static void TryLoadPose(Dictionary<IndicatorPose, IndicatorImage> target, string imageDirectory, IndicatorPose pose, string fileNameWithoutExtension)
        {
            IndicatorImage image = TryLoadFromFile(imageDirectory, fileNameWithoutExtension);
            if (image != null)
                target[pose] = image;
        }

        private static void TryLoadStatePose(Dictionary<string, IndicatorImage> target, string imageDirectory, string stateKey, IndicatorPose pose)
        {
            string poseKey = IndicatorPoseHelper.GetKey(pose);
            foreach (string prefix in IndicatorStates.GetFilePrefixes(stateKey))
            {
                IndicatorImage image = TryLoadFromFileExact(imageDirectory, prefix + "-" + poseKey);
                if (image != null)
                {
                    target[MakeStatePoseKey(stateKey, pose)] = image;
                    return;
                }
            }
        }

        private static void TryLoadLegacy(Dictionary<string, IndicatorImage> target, string imageDirectory, string label, string fileNameWithoutExtension)
        {
            IndicatorImage image = TryLoadFromFile(imageDirectory, fileNameWithoutExtension);
            if (image != null)
                target[label] = image;
        }

        private IndicatorImage GetStatePoseExact(string stateKey, IndicatorPose pose)
        {
            IndicatorImage image;
            if (statePoseImages.TryGetValue(MakeStatePoseKey(stateKey, pose), out image))
                return image;

            return null;
        }

        private IndicatorImage GetPoseExact(IndicatorPose pose)
        {
            IndicatorImage image;
            if (poseImages.TryGetValue(pose, out image))
                return image;

            return null;
        }

        private IndicatorImage GetLegacy(string label)
        {
            IndicatorImage image;
            if (legacyImages.TryGetValue(label, out image))
                return image;

            if (label == Labels.EnglishUpper && legacyImages.TryGetValue(Labels.EnglishLower, out image))
                return image;

            return null;
        }

        private static string MakeStatePoseKey(string stateKey, IndicatorPose pose)
        {
            return stateKey + "|" + IndicatorPoseHelper.GetKey(pose);
        }

        private static IndicatorImage TryLoadFromFile(string imageDirectory, string fileNameWithoutExtension)
        {
            return TryLoadFromFile(imageDirectory, fileNameWithoutExtension, false);
        }

        private static IndicatorImage TryLoadFromFileExact(string imageDirectory, string fileNameWithoutExtension)
        {
            return TryLoadFromFile(imageDirectory, fileNameWithoutExtension, true);
        }

        private static IndicatorImage TryLoadFromFile(string imageDirectory, string fileNameWithoutExtension, bool exactFileName)
        {
            foreach (string extension in Extensions)
            {
                string path = exactFileName
                    ? FindExactImagePath(imageDirectory, fileNameWithoutExtension + extension)
                    : Path.Combine(imageDirectory, fileNameWithoutExtension + extension);
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

        private static string FindExactImagePath(string imageDirectory, string fileName)
        {
            try
            {
                if (Directory.Exists(imageDirectory))
                {
                    foreach (string path in Directory.GetFiles(imageDirectory, fileName))
                    {
                        if (Path.GetFileName(path).Equals(fileName, StringComparison.Ordinal))
                            return path;
                    }
                }
            }
            catch
            {
            }

            return Path.Combine(imageDirectory, "__missing__" + fileName);
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

    internal static class MascotColorizer
    {
        public static Bitmap CreateTintedBitmap(Image image, Color tint, PointF faceCenter)
        {
            Bitmap source = new Bitmap(image);
            Bitmap target = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Color pixel = source.GetPixel(x, y);
                    if (pixel.A == 0)
                    {
                        target.SetPixel(x, y, pixel);
                        continue;
                    }

                    if (ShouldTintPixel(pixel, x / (float)source.Width, y / (float)source.Height, faceCenter))
                        target.SetPixel(x, y, ApplyTint(pixel, tint));
                    else
                        target.SetPixel(x, y, pixel);
                }
            }

            source.Dispose();
            return target;
        }

        private static bool ShouldTintPixel(Color pixel, float xRatio, float yRatio, PointF faceCenter)
        {
            float dx = (xRatio - faceCenter.X) / 0.23f;
            float dy = (yRatio - faceCenter.Y) / 0.18f;
            if ((dx * dx) + (dy * dy) < 1.0f)
                return false;

            int max = Math.Max(pixel.R, Math.Max(pixel.G, pixel.B));
            int min = Math.Min(pixel.R, Math.Min(pixel.G, pixel.B));
            if (max < 70)
                return false;
            if (max > 246 && min > 235)
                return false;

            float saturation = max == 0 ? 0.0f : (max - min) / (float)max;
            return saturation < 0.32f;
        }

        private static Color ApplyTint(Color pixel, Color tint)
        {
            int luminance = (int)Math.Round((pixel.R * 0.299d) + (pixel.G * 0.587d) + (pixel.B * 0.114d));
            double shade = Math.Max(0.38d, Math.Min(1.35d, luminance / 210.0d));
            int r = ClampColor((int)Math.Round(tint.R * shade));
            int g = ClampColor((int)Math.Round(tint.G * shade));
            int b = ClampColor((int)Math.Round(tint.B * shade));

            return Color.FromArgb(
                pixel.A,
                ClampColor((int)Math.Round((r * 0.78d) + (pixel.R * 0.22d))),
                ClampColor((int)Math.Round((g * 0.78d) + (pixel.G * 0.22d))),
                ClampColor((int)Math.Round((b * 0.78d) + (pixel.B * 0.22d))));
        }

        private static int ClampColor(int value)
        {
            if (value < 0)
                return 0;
            if (value > 255)
                return 255;
            return value;
        }
    }

    internal enum CutoutLineKind
    {
        Foreground,
        Background
    }

    internal sealed class CutoutLine
    {
        public CutoutLine(PointF start, PointF end)
            : this(start, end, CutoutLineKind.Foreground)
        {
        }

        public CutoutLine(PointF start, PointF end, CutoutLineKind kind)
        {
            Start = start;
            End = end;
            Kind = kind;
        }

        public PointF Start { get; private set; }

        public PointF End { get; private set; }

        public CutoutLineKind Kind { get; private set; }
    }

    internal static class BackgroundRemover
    {
        public static string GetOutputPath(string inputPath)
        {
            string directory = Path.GetDirectoryName(inputPath);
            string name = Path.GetFileNameWithoutExtension(inputPath);
            string outputPath = Path.Combine(directory, name + "-cutout.png");
            int index = 2;

            while (File.Exists(outputPath))
            {
                outputPath = Path.Combine(directory, name + "-cutout-" + index + ".png");
                index++;
            }

            return outputPath;
        }

        public static void SaveTransparentCopy(string inputPath, string outputPath)
        {
            SaveTransparentCopy(inputPath, outputPath, 0);
        }

        public static void SaveTransparentCopy(string inputPath, string outputPath, int maxSize)
        {
            SaveTransparentCopy(inputPath, outputPath, maxSize, null);
        }

        public static void SaveTransparentCopy(string inputPath, string outputPath, int maxSize, IEnumerable<CutoutLine> backgroundLines)
        {
            using (Bitmap bitmap = LoadArgbBitmap(inputPath))
            {
                RemoveEdgeBackground(bitmap, backgroundLines);
                if (maxSize > 0)
                {
                    using (Bitmap resized = CreateResizedCutout(bitmap, maxSize))
                        resized.Save(outputPath, ImageFormat.Png);
                }
                else
                {
                    bitmap.Save(outputPath, ImageFormat.Png);
                }
            }
        }

        private static Bitmap LoadArgbBitmap(string path)
        {
            using (Image source = Image.FromFile(path))
            {
                Bitmap bitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.DrawImage(source, 0, 0, source.Width, source.Height);
                }

                return bitmap;
            }
        }

        private static void RemoveEdgeBackground(Bitmap bitmap, IEnumerable<CutoutLine> cutoutLines)
        {
            List<CutoutLine> lines = NormalizeCutoutLines(cutoutLines);
            List<CutoutLine> foregroundLines = FilterCutoutLines(lines, CutoutLineKind.Foreground);
            List<CutoutLine> backgroundLines = FilterCutoutLines(lines, CutoutLineKind.Background);
            int width = bitmap.Width;
            int height = bitmap.Height;
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            try
            {
                int stride = data.Stride;
                byte[] bytes = new byte[Math.Abs(stride) * height];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

                bool[] background = FindConnectedBackground(bytes, width, height, stride, true, backgroundLines);
                if (RemovesTooMuch(bytes, background, width, height, stride))
                {
                    background = FindConnectedBackground(bytes, width, height, stride, true, null);
                    if (RemovesTooMuch(bytes, background, width, height, stride))
                        background = FindConnectedBackground(bytes, width, height, stride, false, null);
                }

                if (foregroundLines.Count > 0)
                {
                    if (!ApplyForegroundOutlineMask(background, width, height, foregroundLines))
                    {
                        ApplyForegroundLineCropFallback(bytes, background, width, height, stride, foregroundLines);
                        ProtectForegroundLines(bytes, background, width, height, stride, foregroundLines);
                    }
                }
                else
                {
                    ApplySubjectCropFallback(bytes, background, width, height, stride);
                }

                for (int p = 0; p < background.Length; p++)
                {
                    if (!background[p])
                        continue;

                    int x = p % width;
                    int y = p / width;
                    int offset = (y * stride) + (x * 4);
                    bytes[offset + 3] = 0;
                }

                SoftenWhiteHalo(bytes, background, width, height, stride);
                Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        private static bool ApplyForegroundOutlineMask(bool[] background, int width, int height, IList<CutoutLine> foregroundLines)
        {
            List<Point> polygon = BuildOutlinePolygon(foregroundLines, width, height);
            if (polygon.Count < 3)
                return false;

            double area = Math.Abs(GetPolygonArea(polygon));
            if (area < width * height * 0.002d)
                return false;

            if (area > width * height * 0.90d)
                return false;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    background[(y * width) + x] = !IsPointInsidePolygon(x + 0.5d, y + 0.5d, polygon);
            }

            return true;
        }

        private static void ApplySubjectCropFallback(byte[] bytes, bool[] background, int width, int height, int stride)
        {
            Rectangle remaining = GetRemainingBounds(bytes, background, width, height, stride);
            if (remaining.Width <= 0 || remaining.Height <= 0)
                return;

            if (remaining.Width < width * 0.72d || remaining.Height < height * 0.62d)
                return;

            Rectangle crop;
            bool[] subjectMask;
            if (!TryFindSalientSubjectCrop(bytes, background, width, height, stride, out crop, out subjectMask))
                return;

            if (crop.Width <= 0 || crop.Height <= 0)
                return;

            if (subjectMask != null && CountMask(subjectMask) > (width * height * 0.004d) && CountMask(subjectMask) < (width * height * 0.62d))
            {
                for (int p = 0; p < background.Length; p++)
                    background[p] = background[p] || !subjectMask[p];

                return;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (crop.Contains(x, y))
                        continue;

                    int p = (y * width) + x;
                    background[p] = true;
                }
            }
        }

        private static void ApplyForegroundLineCropFallback(byte[] bytes, bool[] background, int width, int height, int stride, IList<CutoutLine> foregroundLines)
        {
            Rectangle lineBounds = GetLineBounds(foregroundLines, width, height);
            if (lineBounds.IsEmpty)
                return;

            if (lineBounds.Width < width * 0.025d && lineBounds.Height < height * 0.025d)
                return;

            Rectangle crop = ExpandForegroundLineCrop(lineBounds, width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (crop.Contains(x, y))
                        continue;

                    background[(y * width) + x] = true;
                }
            }
        }

        private static void ProtectForegroundLines(byte[] bytes, bool[] background, int width, int height, int stride, IList<CutoutLine> foregroundLines)
        {
            if (foregroundLines == null || foregroundLines.Count == 0)
                return;

            Rectangle lineBounds = GetLineBounds(foregroundLines, width, height);
            if (lineBounds.IsEmpty)
                return;

            Rectangle limit = ExpandForegroundLineCrop(lineBounds, width, height);
            bool[] protectedMask = BuildForegroundLineMask(bytes, width, height, stride, foregroundLines, limit);
            int protectedCount = CountMask(protectedMask);
            if (protectedCount <= 0 || protectedCount > width * height * 0.72d)
                return;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int p = (y * width) + x;
                    if (protectedMask[p])
                    {
                        background[p] = false;
                    }
                    else if (limit.Contains(x, y))
                    {
                        background[p] = true;
                    }
                }
            }
        }

        private static bool[] BuildForegroundLineMask(byte[] bytes, int width, int height, int stride, IList<CutoutLine> foregroundLines, Rectangle limit)
        {
            bool[] mask = new bool[width * height];
            bool[] visited = new bool[width * height];
            int[] queue = new int[width * height];
            ForegroundLineColorModel model = ForegroundLineColorModel.Create(bytes, width, height, stride, foregroundLines);
            int head = 0;
            int tail = 0;

            AddForegroundLineSeeds(bytes, visited, mask, queue, ref tail, width, height, stride, foregroundLines, limit);

            while (head < tail)
            {
                int p = queue[head++];
                int x = p % width;
                int y = p / width;

                AddForegroundNeighbor(bytes, visited, mask, queue, ref tail, width, height, stride, model, limit, x - 1, y);
                AddForegroundNeighbor(bytes, visited, mask, queue, ref tail, width, height, stride, model, limit, x + 1, y);
                AddForegroundNeighbor(bytes, visited, mask, queue, ref tail, width, height, stride, model, limit, x, y - 1);
                AddForegroundNeighbor(bytes, visited, mask, queue, ref tail, width, height, stride, model, limit, x, y + 1);
            }

            Rectangle maskBounds = GetMaskBounds(mask, width, height);
            if (!maskBounds.IsEmpty)
            {
                Rectangle dilateLimit = ExpandSubjectCrop(maskBounds, width, height);
                mask = DilateMask(mask, width, height, Math.Max(2, Math.Min(width, height) / 96), dilateLimit);
            }

            return mask;
        }

        private static Rectangle GetRemainingBounds(byte[] bytes, bool[] background, int width, int height, int stride)
        {
            int left = width;
            int top = height;
            int right = -1;
            int bottom = -1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int p = (y * width) + x;
                    if (background[p])
                        continue;

                    int offset = (y * stride) + (x * 4);
                    if (bytes[offset + 3] <= 12)
                        continue;

                    if (x < left)
                        left = x;
                    if (y < top)
                        top = y;
                    if (x > right)
                        right = x;
                    if (y > bottom)
                        bottom = y;
                }
            }

            if (right < left || bottom < top)
                return Rectangle.Empty;

            return Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
        }

        private static Bitmap CreateResizedCutout(Bitmap source, int maxSize)
        {
            int targetSize = Math.Max(32, Math.Min(1024, maxSize));
            Rectangle bounds = GetAlphaBounds(source);
            Bitmap target = new Bitmap(targetSize, targetSize, PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(target))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                float padding = Math.Max(2.0f, targetSize * 0.03f);
                float maxWidth = targetSize - (padding * 2.0f);
                float maxHeight = targetSize - (padding * 2.0f);
                float scale = Math.Min(maxWidth / Math.Max(1, bounds.Width), maxHeight / Math.Max(1, bounds.Height));
                float width = bounds.Width * scale;
                float height = bounds.Height * scale;
                RectangleF dest = new RectangleF(
                    (targetSize - width) / 2.0f,
                    (targetSize - height) / 2.0f,
                    width,
                    height);

                graphics.DrawImage(source, dest, bounds, GraphicsUnit.Pixel);
            }

            return target;
        }

        private static Rectangle GetAlphaBounds(Bitmap bitmap)
        {
            int left = bitmap.Width;
            int top = bitmap.Height;
            int right = -1;
            int bottom = -1;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y).A <= 12)
                        continue;

                    if (x < left)
                        left = x;
                    if (y < top)
                        top = y;
                    if (x > right)
                        right = x;
                    if (y > bottom)
                        bottom = y;
                }
            }

            if (right < left || bottom < top)
                return new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            return Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
        }

        private static bool RemovesTooMuch(byte[] bytes, bool[] background, int width, int height, int stride)
        {
            int opaque = 0;
            int removed = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int p = (y * width) + x;
                    int offset = (y * stride) + (x * 4);
                    if (bytes[offset + 3] <= 12)
                        continue;

                    opaque++;
                    if (background[p])
                        removed++;
                }
            }

            return opaque > 0 && removed > opaque * 0.88d;
        }

        private static bool[] FindConnectedBackground(byte[] bytes, int width, int height, int stride, bool useEdgeColorModel, IList<CutoutLine> backgroundLines)
        {
            bool[] background = new bool[width * height];
            int[] queue = new int[width * height];
            BackgroundColorModel model = useEdgeColorModel
                ? BackgroundColorModel.Create(bytes, width, height, stride, backgroundLines)
                : BackgroundColorModel.Empty;
            int head = 0;
            int tail = 0;

            AddSeed(bytes, background, queue, ref tail, width, height, stride, model, 0, 0);
            AddSeed(bytes, background, queue, ref tail, width, height, stride, model, width - 1, 0);
            AddSeed(bytes, background, queue, ref tail, width, height, stride, model, 0, height - 1);
            AddSeed(bytes, background, queue, ref tail, width, height, stride, model, width - 1, height - 1);

            for (int x = 0; x < width; x++)
            {
                AddSeed(bytes, background, queue, ref tail, width, height, stride, model, x, 0);
                AddSeed(bytes, background, queue, ref tail, width, height, stride, model, x, height - 1);
            }

            for (int y = 0; y < height; y++)
            {
                AddSeed(bytes, background, queue, ref tail, width, height, stride, model, 0, y);
                AddSeed(bytes, background, queue, ref tail, width, height, stride, model, width - 1, y);
            }

            AddLineSeeds(bytes, background, queue, ref tail, width, height, stride, backgroundLines);

            while (head < tail)
            {
                int p = queue[head++];
                int x = p % width;
                int y = p / width;

                AddSeed(bytes, background, queue, ref tail, width, height, stride, model, x - 1, y);
                AddSeed(bytes, background, queue, ref tail, width, height, stride, model, x + 1, y);
                AddSeed(bytes, background, queue, ref tail, width, height, stride, model, x, y - 1);
                AddSeed(bytes, background, queue, ref tail, width, height, stride, model, x, y + 1);
            }

            return background;
        }

        private static bool TryFindSalientSubjectCrop(byte[] bytes, bool[] background, int width, int height, int stride, out Rectangle crop, out bool[] subjectMask)
        {
            bool[] salient = new bool[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int p = (y * width) + x;
                    if (background[p])
                        continue;

                    int offset = (y * stride) + (x * 4);
                    if (bytes[offset + 3] <= 12)
                        continue;

                    salient[p] = IsSalientForegroundPixel(bytes, offset);
                }
            }

            bool[] visited = new bool[width * height];
            int[] queue = new int[width * height];
            double bestScore = 0.0d;
            Rectangle best = Rectangle.Empty;
            bool[] bestMask = null;
            int minCount = Math.Max(20, (width * height) / 20000);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int start = (y * width) + x;
                    if (!salient[start] || visited[start])
                        continue;

                    int head = 0;
                    int tail = 0;
                    int count = 0;
                    int left = x;
                    int top = y;
                    int right = x;
                    int bottom = y;
                    double sumX = 0.0d;
                    double sumY = 0.0d;
                    bool touchesEdge = false;

                    visited[start] = true;
                    queue[tail++] = start;

                    while (head < tail)
                    {
                        int p = queue[head++];
                        int px = p % width;
                        int py = p / width;

                        count++;
                        sumX += px;
                        sumY += py;
                        if (px < left)
                            left = px;
                        if (py < top)
                            top = py;
                        if (px > right)
                            right = px;
                        if (py > bottom)
                            bottom = py;
                        if (px <= 1 || py <= 1 || px >= width - 2 || py >= height - 2)
                            touchesEdge = true;

                        AddSalientNeighbor(salient, visited, queue, ref tail, width, height, px - 1, py);
                        AddSalientNeighbor(salient, visited, queue, ref tail, width, height, px + 1, py);
                        AddSalientNeighbor(salient, visited, queue, ref tail, width, height, px, py - 1);
                        AddSalientNeighbor(salient, visited, queue, ref tail, width, height, px, py + 1);
                    }

                    if (touchesEdge || count < minCount)
                        continue;

                    double centerX = sumX / count;
                    double centerY = sumY / count;
                    double dx = (centerX - (width / 2.0d)) / Math.Max(1.0d, width / 2.0d);
                    double dy = (centerY - (height / 2.0d)) / Math.Max(1.0d, height / 2.0d);
                    double distance = Math.Sqrt((dx * dx) + (dy * dy));
                    double boxArea = Math.Max(1, (right - left + 1) * (bottom - top + 1));
                    double density = count / boxArea;
                    double score = count * Math.Max(0.18d, 1.0d - (distance * 0.72d)) * Math.Max(0.30d, density);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
                        bestMask = new bool[width * height];
                        for (int i = 0; i < tail; i++)
                            bestMask[queue[i]] = true;
                    }
                }
            }

            if (bestScore <= 0.0d || best.IsEmpty)
            {
                crop = Rectangle.Empty;
                subjectMask = null;
                return false;
            }

            crop = ExpandSubjectCrop(best, width, height);
            subjectMask = DilateMask(bestMask, width, height, Math.Max(5, Math.Min(width, height) / 58), crop);
            return crop.Width > 0 && crop.Height > 0;
        }

        private static int CountMask(bool[] mask)
        {
            int count = 0;
            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i])
                    count++;
            }

            return count;
        }

        private static bool[] DilateMask(bool[] mask, int width, int height, int iterations, Rectangle limit)
        {
            if (mask == null)
                return null;

            bool[] current = mask;
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                bool[] next = new bool[current.Length];
                for (int y = limit.Top; y < limit.Bottom; y++)
                {
                    for (int x = limit.Left; x < limit.Right; x++)
                    {
                        int p = (y * width) + x;
                        if (!current[p])
                            continue;

                        SetMask(next, width, height, limit, x, y);
                        SetMask(next, width, height, limit, x - 1, y);
                        SetMask(next, width, height, limit, x + 1, y);
                        SetMask(next, width, height, limit, x, y - 1);
                        SetMask(next, width, height, limit, x, y + 1);
                        SetMask(next, width, height, limit, x - 1, y - 1);
                        SetMask(next, width, height, limit, x + 1, y - 1);
                        SetMask(next, width, height, limit, x - 1, y + 1);
                        SetMask(next, width, height, limit, x + 1, y + 1);
                    }
                }

                current = next;
            }

            return current;
        }

        private static Rectangle GetMaskBounds(bool[] mask, int width, int height)
        {
            int left = width;
            int top = height;
            int right = -1;
            int bottom = -1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!mask[(y * width) + x])
                        continue;

                    if (x < left)
                        left = x;
                    if (y < top)
                        top = y;
                    if (x > right)
                        right = x;
                    if (y > bottom)
                        bottom = y;
                }
            }

            if (right < left || bottom < top)
                return Rectangle.Empty;

            return Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
        }

        private static void SetMask(bool[] mask, int width, int height, Rectangle limit, int x, int y)
        {
            if (x < limit.Left || y < limit.Top || x >= limit.Right || y >= limit.Bottom)
                return;
            if (x < 0 || y < 0 || x >= width || y >= height)
                return;

            mask[(y * width) + x] = true;
        }

        private static void AddSalientNeighbor(bool[] salient, bool[] visited, int[] queue, ref int tail, int width, int height, int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return;

            int p = (y * width) + x;
            if (visited[p] || !salient[p])
                return;

            visited[p] = true;
            queue[tail++] = p;
        }

        private static Rectangle ExpandSubjectCrop(Rectangle box, int width, int height)
        {
            int padX = Math.Max((int)Math.Round(width * 0.035d), (int)Math.Round(box.Width * 0.28d));
            int padY = Math.Max((int)Math.Round(height * 0.045d), (int)Math.Round(box.Height * 0.20d));
            int left = Math.Max(0, box.Left - padX);
            int top = Math.Max(0, box.Top - padY);
            int right = Math.Min(width, box.Right + padX);
            int bottom = Math.Min(height, box.Bottom + padY);

            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        private static bool IsSalientForegroundPixel(byte[] bytes, int offset)
        {
            int b = bytes[offset];
            int g = bytes[offset + 1];
            int r = bytes[offset + 2];
            int max = Math.Max(r, Math.Max(g, b));
            int min = Math.Min(r, Math.Min(g, b));
            double average = (r + g + b) / 3.0d;
            double saturation = max == 0 ? 0.0d : (max - min) / (double)max;

            if (average < 28.0d || average > 245.0d)
                return false;

            bool redDominant = r > 105 && r > g * 1.22d && r > b * 1.18d && saturation > 0.24d;
            bool yellowDominant = r > 130 && g > 80 && r > b * 1.35d && saturation > 0.22d;
            bool greenDominant = g > 70 && g > b * 1.12d && saturation > 0.26d;

            return saturation > 0.38d || redDominant || yellowDominant || greenDominant;
        }

        private static void AddSeed(byte[] bytes, bool[] background, int[] queue, ref int tail, int width, int height, int stride, BackgroundColorModel model, int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return;

            int p = (y * width) + x;
            if (background[p])
                return;

            int offset = (y * stride) + (x * 4);
            if (!IsBackgroundPixel(bytes, offset, model))
                return;

            background[p] = true;
            queue[tail] = p;
            tail++;
        }

        private static void AddLineSeeds(byte[] bytes, bool[] background, int[] queue, ref int tail, int width, int height, int stride, IList<CutoutLine> backgroundLines)
        {
            if (backgroundLines == null || backgroundLines.Count == 0)
                return;

            foreach (CutoutLine line in backgroundLines)
            {
                int x1 = RatioToPixel(line.Start.X, width);
                int y1 = RatioToPixel(line.Start.Y, height);
                int x2 = RatioToPixel(line.End.X, width);
                int y2 = RatioToPixel(line.End.Y, height);
                int steps = Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
                if (steps < 1)
                    steps = 1;

                for (int i = 0; i <= steps; i++)
                {
                    double t = i / (double)steps;
                    int x = (int)Math.Round(x1 + ((x2 - x1) * t));
                    int y = (int)Math.Round(y1 + ((y2 - y1) * t));
                    AddManualSeed(bytes, background, queue, ref tail, width, height, stride, x, y);
                    AddManualSeed(bytes, background, queue, ref tail, width, height, stride, x - 1, y);
                    AddManualSeed(bytes, background, queue, ref tail, width, height, stride, x + 1, y);
                    AddManualSeed(bytes, background, queue, ref tail, width, height, stride, x, y - 1);
                    AddManualSeed(bytes, background, queue, ref tail, width, height, stride, x, y + 1);
                }
            }
        }

        private static void AddManualSeed(byte[] bytes, bool[] background, int[] queue, ref int tail, int width, int height, int stride, int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return;

            int p = (y * width) + x;
            if (background[p])
                return;

            int offset = (y * stride) + (x * 4);
            if (bytes[offset + 3] < 8)
                return;

            background[p] = true;
            queue[tail] = p;
            tail++;
        }

        private static void AddForegroundLineSeeds(byte[] bytes, bool[] visited, bool[] mask, int[] queue, ref int tail, int width, int height, int stride, IList<CutoutLine> foregroundLines, Rectangle limit)
        {
            foreach (CutoutLine line in foregroundLines)
            {
                int x1 = RatioToPixel(line.Start.X, width);
                int y1 = RatioToPixel(line.Start.Y, height);
                int x2 = RatioToPixel(line.End.X, width);
                int y2 = RatioToPixel(line.End.Y, height);
                int steps = Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
                if (steps < 1)
                    steps = 1;

                for (int i = 0; i <= steps; i++)
                {
                    double t = i / (double)steps;
                    int x = (int)Math.Round(x1 + ((x2 - x1) * t));
                    int y = (int)Math.Round(y1 + ((y2 - y1) * t));
                    AddForegroundSeed(bytes, visited, mask, queue, ref tail, width, height, stride, limit, x, y);
                    AddForegroundSeed(bytes, visited, mask, queue, ref tail, width, height, stride, limit, x - 1, y);
                    AddForegroundSeed(bytes, visited, mask, queue, ref tail, width, height, stride, limit, x + 1, y);
                    AddForegroundSeed(bytes, visited, mask, queue, ref tail, width, height, stride, limit, x, y - 1);
                    AddForegroundSeed(bytes, visited, mask, queue, ref tail, width, height, stride, limit, x, y + 1);
                }
            }
        }

        private static void AddForegroundSeed(byte[] bytes, bool[] visited, bool[] mask, int[] queue, ref int tail, int width, int height, int stride, Rectangle limit, int x, int y)
        {
            if (x < limit.Left || y < limit.Top || x >= limit.Right || y >= limit.Bottom)
                return;
            if (x < 0 || y < 0 || x >= width || y >= height)
                return;

            int p = (y * width) + x;
            if (visited[p])
                return;

            int offset = (y * stride) + (x * 4);
            if (bytes[offset + 3] < 8)
                return;

            visited[p] = true;
            mask[p] = true;
            queue[tail++] = p;
        }

        private static void AddForegroundNeighbor(byte[] bytes, bool[] visited, bool[] mask, int[] queue, ref int tail, int width, int height, int stride, ForegroundLineColorModel model, Rectangle limit, int x, int y)
        {
            if (x < limit.Left || y < limit.Top || x >= limit.Right || y >= limit.Bottom)
                return;
            if (x < 0 || y < 0 || x >= width || y >= height)
                return;

            int p = (y * width) + x;
            if (visited[p])
                return;

            int offset = (y * stride) + (x * 4);
            if (!IsForegroundLinePixel(bytes, offset, model))
                return;

            visited[p] = true;
            mask[p] = true;
            queue[tail++] = p;
        }

        private static bool IsForegroundLinePixel(byte[] bytes, int offset, ForegroundLineColorModel model)
        {
            if (bytes[offset + 3] < 8)
                return false;

            if (model.Matches(bytes, offset))
                return IsMarkedSubjectCandidate(bytes, offset);

            return false;
        }

        private static bool IsMarkedSubjectCandidate(byte[] bytes, int offset)
        {
            int b = bytes[offset];
            int g = bytes[offset + 1];
            int r = bytes[offset + 2];
            int max = Math.Max(r, Math.Max(g, b));
            int min = Math.Min(r, Math.Min(g, b));
            double average = (r + g + b) / 3.0d;
            double saturation = max == 0 ? 0.0d : (max - min) / (double)max;

            if (saturation >= 0.24d)
                return true;

            bool skinLike = r > 95 && g > 50 && b > 38 && r > g * 1.04d && r > b * 1.12d;
            bool brightFace = average > 150.0d && saturation >= 0.10d;
            return skinLike || brightFace;
        }

        private static bool IsBackgroundPixel(byte[] bytes, int offset, BackgroundColorModel model)
        {
            int b = bytes[offset];
            int g = bytes[offset + 1];
            int r = bytes[offset + 2];
            int a = bytes[offset + 3];

            if (a < 8)
                return true;

            int max = Math.Max(r, Math.Max(g, b));
            int min = Math.Min(r, Math.Min(g, b));
            double average = (r + g + b) / 3.0d;
            double saturation = max == 0 ? 0.0d : (max - min) / (double)max;

            if (r > 238 && g > 238 && b > 238)
                return true;

            if (average > 188.0d && saturation < 0.16d)
                return true;

            return model.Matches(bytes, offset);
        }

        private static void SoftenWhiteHalo(byte[] bytes, bool[] background, int width, int height, int stride)
        {
            Dictionary<int, byte> updates = new Dictionary<int, byte>();
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int p = (y * width) + x;
                    if (background[p])
                        continue;

                    bool touchesBackground = background[p - 1] || background[p + 1] || background[p - width] || background[p + width];
                    if (!touchesBackground)
                        continue;

                    int offset = (y * stride) + (x * 4);
                    if (IsSoftWhiteHalo(bytes, offset))
                        updates[offset + 3] = 120;
                }
            }

            foreach (KeyValuePair<int, byte> update in updates)
            {
                if (bytes[update.Key] > update.Value)
                    bytes[update.Key] = update.Value;
            }
        }

        private static bool IsSoftWhiteHalo(byte[] bytes, int offset)
        {
            int b = bytes[offset];
            int g = bytes[offset + 1];
            int r = bytes[offset + 2];
            int max = Math.Max(r, Math.Max(g, b));
            int min = Math.Min(r, Math.Min(g, b));
            double average = (r + g + b) / 3.0d;
            double saturation = max == 0 ? 0.0d : (max - min) / (double)max;

            return average > 220.0d && saturation < 0.20d;
        }

        private static List<CutoutLine> NormalizeCutoutLines(IEnumerable<CutoutLine> backgroundLines)
        {
            List<CutoutLine> lines = new List<CutoutLine>();
            if (backgroundLines == null)
                return lines;

            foreach (CutoutLine line in backgroundLines)
            {
                if (line == null)
                    continue;

                PointF start = new PointF(ClampRatio(line.Start.X), ClampRatio(line.Start.Y));
                PointF end = new PointF(ClampRatio(line.End.X), ClampRatio(line.End.Y));
                if (Math.Abs(start.X - end.X) < 0.001f && Math.Abs(start.Y - end.Y) < 0.001f)
                    continue;

                lines.Add(new CutoutLine(start, end, line.Kind));
            }

            return lines;
        }

        private static List<CutoutLine> FilterCutoutLines(IEnumerable<CutoutLine> lines, CutoutLineKind kind)
        {
            List<CutoutLine> filtered = new List<CutoutLine>();
            if (lines == null)
                return filtered;

            foreach (CutoutLine line in lines)
            {
                if (line != null && line.Kind == kind)
                    filtered.Add(line);
            }

            return filtered;
        }

        private static Rectangle GetLineBounds(IList<CutoutLine> lines, int width, int height)
        {
            if (lines == null || lines.Count == 0)
                return Rectangle.Empty;

            int left = width;
            int top = height;
            int right = -1;
            int bottom = -1;

            foreach (CutoutLine line in lines)
            {
                int x1 = RatioToPixel(line.Start.X, width);
                int y1 = RatioToPixel(line.Start.Y, height);
                int x2 = RatioToPixel(line.End.X, width);
                int y2 = RatioToPixel(line.End.Y, height);
                left = Math.Min(left, Math.Min(x1, x2));
                top = Math.Min(top, Math.Min(y1, y2));
                right = Math.Max(right, Math.Max(x1, x2));
                bottom = Math.Max(bottom, Math.Max(y1, y2));
            }

            if (right < left || bottom < top)
                return Rectangle.Empty;

            return Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
        }

        private static Rectangle ExpandForegroundLineCrop(Rectangle box, int width, int height)
        {
            if (box.IsEmpty)
                return Rectangle.Empty;

            int padX = Math.Max((int)Math.Round(width * 0.11d), (int)Math.Round(box.Width * 0.38d));
            int padY = Math.Max((int)Math.Round(height * 0.14d), (int)Math.Round(box.Height * 0.46d));
            int left = Math.Max(0, box.Left - padX);
            int top = Math.Max(0, box.Top - padY);
            int right = Math.Min(width, box.Right + padX);
            int bottom = Math.Min(height, box.Bottom + padY);
            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        private static List<Point> BuildOutlinePolygon(IList<CutoutLine> lines, int width, int height)
        {
            List<Point> points = new List<Point>();
            if (lines == null)
                return points;

            foreach (CutoutLine line in lines)
            {
                if (line == null)
                    continue;

                Point start = new Point(RatioToPixel(line.Start.X, width), RatioToPixel(line.Start.Y, height));
                Point end = new Point(RatioToPixel(line.End.X, width), RatioToPixel(line.End.Y, height));
                if (points.Count == 0)
                {
                    points.Add(start);
                    points.Add(end);
                    continue;
                }

                Point previous = points[points.Count - 1];
                if (DistanceSquared(previous, start) > DistanceSquared(previous, end))
                {
                    Point temp = start;
                    start = end;
                    end = temp;
                }

                if (DistanceSquared(points[points.Count - 1], start) > 9)
                    points.Add(start);

                if (DistanceSquared(points[points.Count - 1], end) > 2)
                    points.Add(end);
            }

            RemoveNearlyDuplicatePoints(points);
            return points;
        }

        private static void RemoveNearlyDuplicatePoints(List<Point> points)
        {
            for (int i = points.Count - 1; i > 0; i--)
            {
                if (DistanceSquared(points[i], points[i - 1]) <= 2)
                    points.RemoveAt(i);
            }

            if (points.Count > 2 && DistanceSquared(points[0], points[points.Count - 1]) <= 2)
                points.RemoveAt(points.Count - 1);
        }

        private static int DistanceSquared(Point a, Point b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return (dx * dx) + (dy * dy);
        }

        private static double GetPolygonArea(IList<Point> polygon)
        {
            double area = 0.0d;
            for (int i = 0; i < polygon.Count; i++)
            {
                Point a = polygon[i];
                Point b = polygon[(i + 1) % polygon.Count];
                area += (a.X * b.Y) - (b.X * a.Y);
            }

            return area / 2.0d;
        }

        private static bool IsPointInsidePolygon(double x, double y, IList<Point> polygon)
        {
            bool inside = false;
            int j = polygon.Count - 1;
            for (int i = 0; i < polygon.Count; i++)
            {
                Point pi = polygon[i];
                Point pj = polygon[j];
                bool crosses = ((pi.Y > y) != (pj.Y > y))
                    && (x < ((pj.X - pi.X) * (y - pi.Y) / (double)(pj.Y - pi.Y)) + pi.X);
                if (crosses)
                    inside = !inside;

                j = i;
            }

            return inside;
        }

        private static float ClampRatio(float value)
        {
            if (value < 0.0f)
                return 0.0f;
            if (value > 1.0f)
                return 1.0f;
            return value;
        }

        private static int RatioToPixel(float ratio, int size)
        {
            return Math.Max(0, Math.Min(size - 1, (int)Math.Round(ClampRatio(ratio) * (size - 1))));
        }

        private sealed class BackgroundColorModel
        {
            public static readonly BackgroundColorModel Empty = new BackgroundColorModel(new List<ColorSample>(), 0.0d);

            private readonly List<ColorSample> samples;
            private readonly double toleranceSquared;

            private BackgroundColorModel(List<ColorSample> samples, double tolerance)
            {
                this.samples = samples;
                toleranceSquared = tolerance * tolerance;
            }

            public static BackgroundColorModel Create(byte[] bytes, int width, int height, int stride, IList<CutoutLine> backgroundLines)
            {
                List<ColorSample> edgeSamples = new List<ColorSample>();
                List<ColorSample> lineSamples = new List<ColorSample>();
                int step = Math.Max(1, Math.Min(width, height) / 48);

                for (int x = 0; x < width; x += step)
                {
                    AddSample(edgeSamples, bytes, 0, x, 0, stride);
                    AddSample(edgeSamples, bytes, 0, x, height - 1, stride);
                }

                for (int y = 0; y < height; y += step)
                {
                    AddSample(edgeSamples, bytes, 0, 0, y, stride);
                    AddSample(edgeSamples, bytes, 0, width - 1, y, stride);
                }

                AddSample(edgeSamples, bytes, 0, width - 1, 0, stride);
                AddSample(edgeSamples, bytes, 0, 0, height - 1, stride);
                AddSample(edgeSamples, bytes, 0, width - 1, height - 1, stride);
                AddLineSamples(lineSamples, bytes, width, height, stride, backgroundLines);
                edgeSamples.AddRange(lineSamples);

                if (edgeSamples.Count == 0)
                    return new BackgroundColorModel(new List<ColorSample>(), 0.0d);

                double r = 0.0d;
                double g = 0.0d;
                double b = 0.0d;
                foreach (ColorSample sample in edgeSamples)
                {
                    r += sample.R;
                    g += sample.G;
                    b += sample.B;
                }

                r /= edgeSamples.Count;
                g /= edgeSamples.Count;
                b /= edgeSamples.Count;

                double variance = 0.0d;
                foreach (ColorSample sample in edgeSamples)
                {
                    double dr = sample.R - r;
                    double dg = sample.G - g;
                    double db = sample.B - b;
                    variance += (dr * dr) + (dg * dg) + (db * db);
                }

                double deviation = Math.Sqrt(variance / edgeSamples.Count);
                double tolerance = Math.Max(34.0d, Math.Min(76.0d, 28.0d + (deviation * 0.85d)));

                List<ColorSample> anchors = new List<ColorSample>();
                anchors.Add(new ColorSample((int)Math.Round(r), (int)Math.Round(g), (int)Math.Round(b)));
                foreach (ColorSample sample in lineSamples)
                {
                    if (anchors.Count >= 28)
                        break;

                    anchors.Add(sample);
                }

                int strideSamples = Math.Max(1, edgeSamples.Count / 24);
                for (int i = 0; i < edgeSamples.Count && anchors.Count < 28; i += strideSamples)
                    anchors.Add(edgeSamples[i]);

                return new BackgroundColorModel(anchors, tolerance);
            }

            public bool Matches(byte[] bytes, int offset)
            {
                if (samples.Count == 0)
                    return false;

                int b = bytes[offset];
                int g = bytes[offset + 1];
                int r = bytes[offset + 2];

                foreach (ColorSample sample in samples)
                {
                    double dr = r - sample.R;
                    double dg = g - sample.G;
                    double db = b - sample.B;
                    if ((dr * dr) + (dg * dg) + (db * db) <= toleranceSquared)
                        return true;
                }

                return false;
            }

            private static void AddSample(List<ColorSample> samples, byte[] bytes, int unused, int x, int y, int stride)
            {
                int offset = (y * stride) + (x * 4);
                if (bytes[offset + 3] < 8)
                    return;

                samples.Add(new ColorSample(bytes[offset + 2], bytes[offset + 1], bytes[offset]));
            }

            private static void AddLineSamples(List<ColorSample> samples, byte[] bytes, int width, int height, int stride, IList<CutoutLine> backgroundLines)
            {
                if (backgroundLines == null || backgroundLines.Count == 0)
                    return;

                foreach (CutoutLine line in backgroundLines)
                {
                    int x1 = RatioToPixel(line.Start.X, width);
                    int y1 = RatioToPixel(line.Start.Y, height);
                    int x2 = RatioToPixel(line.End.X, width);
                    int y2 = RatioToPixel(line.End.Y, height);
                    int steps = Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
                    if (steps < 1)
                        steps = 1;

                    int strideStep = Math.Max(1, steps / 40);
                    for (int i = 0; i <= steps; i += strideStep)
                    {
                        double t = i / (double)steps;
                        int x = (int)Math.Round(x1 + ((x2 - x1) * t));
                        int y = (int)Math.Round(y1 + ((y2 - y1) * t));
                        AddSample(samples, bytes, 0, x, y, stride);
                    }
                }
            }
        }

        private sealed class ForegroundLineColorModel
        {
            private readonly List<ColorSample> samples;
            private readonly double toleranceSquared;

            private ForegroundLineColorModel(List<ColorSample> samples, double tolerance)
            {
                this.samples = samples;
                toleranceSquared = tolerance * tolerance;
            }

            public static ForegroundLineColorModel Create(byte[] bytes, int width, int height, int stride, IList<CutoutLine> foregroundLines)
            {
                List<ColorSample> samples = new List<ColorSample>();
                if (foregroundLines != null)
                {
                    foreach (CutoutLine line in foregroundLines)
                    {
                        int x1 = RatioToPixel(line.Start.X, width);
                        int y1 = RatioToPixel(line.Start.Y, height);
                        int x2 = RatioToPixel(line.End.X, width);
                        int y2 = RatioToPixel(line.End.Y, height);
                        int steps = Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
                        if (steps < 1)
                            steps = 1;

                        int strideStep = Math.Max(1, steps / 56);
                        for (int i = 0; i <= steps; i += strideStep)
                        {
                            double t = i / (double)steps;
                            int x = (int)Math.Round(x1 + ((x2 - x1) * t));
                            int y = (int)Math.Round(y1 + ((y2 - y1) * t));
                            AddSample(samples, bytes, x, y, width, height, stride);
                        }
                    }
                }

                return new ForegroundLineColorModel(samples, 46.0d);
            }

            public bool Matches(byte[] bytes, int offset)
            {
                if (samples.Count == 0)
                    return false;

                int b = bytes[offset];
                int g = bytes[offset + 1];
                int r = bytes[offset + 2];

                foreach (ColorSample sample in samples)
                {
                    double dr = r - sample.R;
                    double dg = g - sample.G;
                    double db = b - sample.B;
                    if ((dr * dr) + (dg * dg) + (db * db) <= toleranceSquared)
                        return true;
                }

                return false;
            }

            private static void AddSample(List<ColorSample> samples, byte[] bytes, int x, int y, int width, int height, int stride)
            {
                if (x < 0 || y < 0 || x >= width || y >= height)
                    return;

                int offset = (y * stride) + (x * 4);
                if (bytes[offset + 3] < 8)
                    return;

                samples.Add(new ColorSample(bytes[offset + 2], bytes[offset + 1], bytes[offset]));
            }
        }

        private struct ColorSample
        {
            public readonly int R;
            public readonly int G;
            public readonly int B;

            public ColorSample(int r, int g, int b)
            {
                R = r;
                G = g;
                B = b;
            }
        }
    }

    internal sealed class CutoutOptions
    {
        public bool ResizeEnabled { get; set; }

        public int MaxSize { get; set; }

        public bool UseCutoutLine { get; set; }

        public CutoutLineKind LineKind { get; set; }
    }

    internal sealed class ImageSelectionForm : Form
    {
        private readonly IndicatorAssets assets;
        private readonly Action<string> onChanged;
        private readonly ComboBox modeCombo;
        private readonly ComboBox stateCombo;
        private readonly ComboBox poseCombo;
        private readonly Label slotLabel;
        private readonly Label currentFileLabel;

        public ImageSelectionForm(IndicatorAssets assets, Action<string> onChanged)
        {
            this.assets = assets;
            this.onChanged = onChanged;

            Text = TextResources.ChooseImage;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(390, 224);

            Label modeLabel = new Label();
            modeLabel.Text = TextResources.ImagePackMode;
            modeLabel.Location = new Point(14, 18);
            modeLabel.Size = new Size(92, 22);

            modeCombo = new ComboBox();
            modeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            modeCombo.Location = new Point(112, 16);
            modeCombo.Size = new Size(160, 24);
            modeCombo.Items.Add(new ModeItem(false, TextResources.SharedPoseImages));
            modeCombo.Items.Add(new ModeItem(true, TextResources.StatePoseImages));
            modeCombo.SelectedIndexChanged += OnSelectionChanged;

            Label stateLabel = new Label();
            stateLabel.Text = TextResources.State;
            stateLabel.Location = new Point(14, 52);
            stateLabel.Size = new Size(92, 22);

            stateCombo = new ComboBox();
            stateCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            stateCombo.Location = new Point(112, 50);
            stateCombo.Size = new Size(160, 24);
            foreach (string stateKey in IndicatorStates.All)
                stateCombo.Items.Add(new StateChoice(stateKey, IndicatorStates.GetDisplayName(stateKey)));
            stateCombo.SelectedIndexChanged += OnSelectionChanged;

            Label poseLabel = new Label();
            poseLabel.Text = TextResources.Pose;
            poseLabel.Location = new Point(14, 86);
            poseLabel.Size = new Size(92, 22);

            poseCombo = new ComboBox();
            poseCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            poseCombo.Location = new Point(112, 84);
            poseCombo.Size = new Size(160, 24);
            foreach (IndicatorPose pose in IndicatorPoseHelper.All)
                poseCombo.Items.Add(new PoseChoice(pose, IndicatorPoseHelper.GetDisplayName(pose)));
            poseCombo.SelectedIndexChanged += OnSelectionChanged;

            Label slotTitleLabel = new Label();
            slotTitleLabel.Text = TextResources.ImageSlot;
            slotTitleLabel.Location = new Point(14, 122);
            slotTitleLabel.Size = new Size(92, 22);

            slotLabel = new Label();
            slotLabel.Location = new Point(112, 122);
            slotLabel.Size = new Size(260, 22);
            slotLabel.AutoEllipsis = true;

            Label currentTitleLabel = new Label();
            currentTitleLabel.Text = TextResources.CurrentFile;
            currentTitleLabel.Location = new Point(14, 150);
            currentTitleLabel.Size = new Size(92, 22);

            currentFileLabel = new Label();
            currentFileLabel.Location = new Point(112, 150);
            currentFileLabel.Size = new Size(260, 22);
            currentFileLabel.AutoEllipsis = true;

            Button selectButton = new Button();
            selectButton.Text = TextResources.SelectFile;
            selectButton.Location = new Point(126, 184);
            selectButton.Size = new Size(86, 28);
            selectButton.Click += OnSelectFileClicked;

            Button clearButton = new Button();
            clearButton.Text = TextResources.RemoveSlotImage;
            clearButton.Location = new Point(218, 184);
            clearButton.Size = new Size(86, 28);
            clearButton.Click += OnClearSlotClicked;

            Button closeButton = new Button();
            closeButton.Text = TextResources.Close;
            closeButton.Location = new Point(310, 184);
            closeButton.Size = new Size(66, 28);
            closeButton.Click += OnCloseClicked;

            Controls.Add(modeLabel);
            Controls.Add(modeCombo);
            Controls.Add(stateLabel);
            Controls.Add(stateCombo);
            Controls.Add(poseLabel);
            Controls.Add(poseCombo);
            Controls.Add(slotTitleLabel);
            Controls.Add(slotLabel);
            Controls.Add(currentTitleLabel);
            Controls.Add(currentFileLabel);
            Controls.Add(selectButton);
            Controls.Add(clearButton);
            Controls.Add(closeButton);

            modeCombo.SelectedIndex = 0;
            stateCombo.SelectedIndex = 0;
            poseCombo.SelectedIndex = 0;
            RefreshSelection();
        }

        public void RefreshSelection()
        {
            bool stateMode = IsStateMode();
            stateCombo.Enabled = stateMode;

            IndicatorPose pose = GetSelectedPose();
            string fileName = stateMode
                ? assets.GetStatePoseFileName(GetSelectedStateKey(), pose)
                : assets.GetSharedPoseFileName(pose);

            slotLabel.Text = GetSlotName();
            currentFileLabel.Text = string.IsNullOrEmpty(fileName) ? TextResources.NoImageSelected : fileName;
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            RefreshSelection();
        }

        private void OnSelectFileClicked(object sender, EventArgs e)
        {
            Directory.CreateDirectory(assets.ImageDirectory);
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = TextResources.SelectFile;
                dialog.InitialDirectory = assets.ImageDirectory;
                dialog.Multiselect = false;
                dialog.Filter = "Image files|*.gif;*.png;*.jpg;*.jpeg;*.jfif;*.bmp|All files|*.*";
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    if (IsStateMode())
                        assets.InstallStatePoseImage(GetSelectedStateKey(), GetSelectedPose(), dialog.FileName);
                    else
                        assets.InstallSharedPoseImage(GetSelectedPose(), dialog.FileName);

                    NotifyChanged(TextResources.ImageInstalled);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, TextResources.ChooseImage, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void OnClearSlotClicked(object sender, EventArgs e)
        {
            if (IsStateMode())
                assets.ClearStatePoseImage(GetSelectedStateKey(), GetSelectedPose());
            else
                assets.ClearSharedPoseImage(GetSelectedPose());

            NotifyChanged(TextResources.ImageSlotCleared);
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close();
        }

        private void NotifyChanged(string message)
        {
            if (onChanged != null)
                onChanged(message);

            RefreshSelection();
        }

        private bool IsStateMode()
        {
            ModeItem item = modeCombo.SelectedItem as ModeItem;
            return item != null && item.StateMode;
        }

        private string GetSelectedStateKey()
        {
            StateChoice item = stateCombo.SelectedItem as StateChoice;
            return item == null ? IndicatorStates.Korean : item.StateKey;
        }

        private IndicatorPose GetSelectedPose()
        {
            PoseChoice item = poseCombo.SelectedItem as PoseChoice;
            return item == null ? IndicatorPose.Idle : item.Pose;
        }

        private string GetSlotName()
        {
            string poseKey = IndicatorPoseHelper.GetKey(GetSelectedPose());
            if (!IsStateMode())
                return poseKey;

            string stateKey = GetSelectedStateKey();
            if (stateKey == IndicatorStates.EnglishUpper)
                stateKey = "upper";

            return stateKey + "-" + poseKey;
        }

        private sealed class ModeItem
        {
            public ModeItem(bool stateMode, string name)
            {
                StateMode = stateMode;
                Name = name;
            }

            public bool StateMode { get; private set; }

            private string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        private sealed class StateChoice
        {
            public StateChoice(string stateKey, string name)
            {
                StateKey = stateKey;
                Name = name;
            }

            public string StateKey { get; private set; }

            private string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        private sealed class PoseChoice
        {
            public PoseChoice(IndicatorPose pose, string name)
            {
                Pose = pose;
                Name = name;
            }

            public IndicatorPose Pose { get; private set; }

            private string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }
    }

    internal sealed class CutoutOptionsForm : Form
    {
        private readonly CheckBox resizeCheckBox;
        private readonly CheckBox useLineCheckBox;
        private readonly RadioButton foregroundLineRadio;
        private readonly RadioButton backgroundLineRadio;
        private readonly NumericUpDown sizeNumeric;

        public CutoutOptionsForm()
        {
            Text = TextResources.RemoveImageBackground;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(320, 186);

            resizeCheckBox = new CheckBox();
            resizeCheckBox.Text = TextResources.SaveSmallCutout;
            resizeCheckBox.Checked = true;
            resizeCheckBox.Location = new Point(14, 14);
            resizeCheckBox.Size = new Size(180, 24);
            resizeCheckBox.CheckedChanged += OnResizeChanged;

            Label sizeLabel = new Label();
            sizeLabel.Text = TextResources.MaxImageSize;
            sizeLabel.Location = new Point(34, 48);
            sizeLabel.Size = new Size(88, 22);

            sizeNumeric = new NumericUpDown();
            sizeNumeric.Minimum = 64;
            sizeNumeric.Maximum = 512;
            sizeNumeric.Increment = 16;
            sizeNumeric.Value = 160;
            sizeNumeric.Location = new Point(130, 46);
            sizeNumeric.Size = new Size(80, 24);

            Label pxLabel = new Label();
            pxLabel.Text = "px";
            pxLabel.Location = new Point(216, 48);
            pxLabel.Size = new Size(34, 22);

            useLineCheckBox = new CheckBox();
            useLineCheckBox.Text = TextResources.UseCutoutLine;
            useLineCheckBox.Checked = false;
            useLineCheckBox.Location = new Point(14, 78);
            useLineCheckBox.Size = new Size(240, 24);
            useLineCheckBox.CheckedChanged += OnLineOptionChanged;

            foregroundLineRadio = new RadioButton();
            foregroundLineRadio.Text = TextResources.ForegroundCutoutLine;
            foregroundLineRadio.Checked = true;
            foregroundLineRadio.Location = new Point(34, 104);
            foregroundLineRadio.Size = new Size(132, 22);

            backgroundLineRadio = new RadioButton();
            backgroundLineRadio.Text = TextResources.BackgroundCutoutLine;
            backgroundLineRadio.Location = new Point(174, 104);
            backgroundLineRadio.Size = new Size(132, 22);

            Button okButton = new Button();
            okButton.Text = "OK";
            okButton.DialogResult = DialogResult.OK;
            okButton.Location = new Point(150, 148);
            okButton.Size = new Size(72, 26);

            Button cancelButton = new Button();
            cancelButton.Text = TextResources.Close;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(230, 148);
            cancelButton.Size = new Size(72, 26);

            AcceptButton = okButton;
            CancelButton = cancelButton;

            Controls.Add(resizeCheckBox);
            Controls.Add(sizeLabel);
            Controls.Add(sizeNumeric);
            Controls.Add(pxLabel);
            Controls.Add(useLineCheckBox);
            Controls.Add(foregroundLineRadio);
            Controls.Add(backgroundLineRadio);
            Controls.Add(okButton);
            Controls.Add(cancelButton);
            OnResizeChanged(this, EventArgs.Empty);
            OnLineOptionChanged(this, EventArgs.Empty);
        }

        public CutoutOptions Options
        {
            get
            {
                return new CutoutOptions
                {
                    ResizeEnabled = resizeCheckBox.Checked,
                    MaxSize = (int)sizeNumeric.Value,
                    UseCutoutLine = useLineCheckBox.Checked,
                    LineKind = backgroundLineRadio.Checked ? CutoutLineKind.Background : CutoutLineKind.Foreground
                };
            }
        }

        private void OnResizeChanged(object sender, EventArgs e)
        {
            sizeNumeric.Enabled = resizeCheckBox.Checked;
        }

        private void OnLineOptionChanged(object sender, EventArgs e)
        {
            foregroundLineRadio.Enabled = useLineCheckBox.Checked;
            backgroundLineRadio.Enabled = useLineCheckBox.Checked;
        }
    }

    internal sealed class CutoutLineSelectionForm : Form
    {
        private readonly CutoutLinePreview preview;

        public CutoutLineSelectionForm(string imagePath, CutoutLineKind lineKind)
        {
            Text = lineKind == CutoutLineKind.Background ? TextResources.CutoutBackgroundLineSelection : TextResources.CutoutForegroundLineSelection;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(560, 628);

            Label hintLabel = new Label();
            hintLabel.Text = lineKind == CutoutLineKind.Background ? TextResources.CutoutBackgroundLineHint : TextResources.CutoutForegroundLineHint;
            hintLabel.Location = new Point(14, 12);
            hintLabel.Size = new Size(532, 24);

            preview = new CutoutLinePreview(imagePath, lineKind);
            preview.Location = new Point(14, 42);
            preview.Size = new Size(532, 532);

            Button undoButton = new Button();
            undoButton.Text = TextResources.Undo;
            undoButton.Location = new Point(14, 588);
            undoButton.Size = new Size(82, 28);
            undoButton.Click += OnUndoClicked;

            Button clearButton = new Button();
            clearButton.Text = TextResources.Clear;
            clearButton.Location = new Point(102, 588);
            clearButton.Size = new Size(82, 28);
            clearButton.Click += OnClearClicked;

            Button okButton = new Button();
            okButton.Text = "OK";
            okButton.DialogResult = DialogResult.OK;
            okButton.Location = new Point(386, 588);
            okButton.Size = new Size(76, 28);

            Button cancelButton = new Button();
            cancelButton.Text = TextResources.Close;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(470, 588);
            cancelButton.Size = new Size(76, 28);

            AcceptButton = okButton;
            CancelButton = cancelButton;

            Controls.Add(hintLabel);
            Controls.Add(preview);
            Controls.Add(undoButton);
            Controls.Add(clearButton);
            Controls.Add(okButton);
            Controls.Add(cancelButton);
        }

        public List<CutoutLine> Lines
        {
            get { return preview.GetLines(); }
        }

        private void OnUndoClicked(object sender, EventArgs e)
        {
            preview.Undo();
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            preview.ClearLines();
        }
    }

    internal sealed class CutoutLinePreview : Control
    {
        private readonly MemoryStream stream;
        private readonly Image image;
        private readonly CutoutLineKind lineKind;
        private readonly List<CutoutLine> lines = new List<CutoutLine>();
        private readonly List<int> strokeStartIndexes = new List<int>();
        private bool drawing;
        private PointF lastRatio;
        private PointF currentRatio;

        public CutoutLinePreview(string imagePath, CutoutLineKind lineKind)
        {
            this.lineKind = lineKind;
            byte[] bytes = File.ReadAllBytes(imagePath);
            stream = new MemoryStream(bytes);
            image = Image.FromStream(stream);
            DoubleBuffered = true;
            BackColor = Color.FromArgb(246, 248, 252);
        }

        public List<CutoutLine> GetLines()
        {
            return new List<CutoutLine>(lines);
        }

        public void Undo()
        {
            if (lines.Count == 0)
                return;

            int startIndex = strokeStartIndexes.Count > 0 ? strokeStartIndexes[strokeStartIndexes.Count - 1] : lines.Count - 1;
            if (startIndex < 0 || startIndex >= lines.Count)
                startIndex = lines.Count - 1;

            lines.RemoveRange(startIndex, lines.Count - startIndex);
            if (strokeStartIndexes.Count > 0)
                strokeStartIndexes.RemoveAt(strokeStartIndexes.Count - 1);

            Invalidate();
        }

        public void ClearLines()
        {
            lines.Clear();
            strokeStartIndexes.Clear();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (SolidBrush background = new SolidBrush(BackColor))
            using (Pen border = new Pen(Color.FromArgb(203, 213, 225)))
            {
                e.Graphics.FillRectangle(background, ClientRectangle);
                e.Graphics.DrawRectangle(border, new Rectangle(0, 0, Width - 1, Height - 1));
            }

            Rectangle imageRect = GetImageRect();
            e.Graphics.DrawImage(image, imageRect);

            if (lineKind == CutoutLineKind.Foreground)
                DrawForegroundPreviewFill(e.Graphics, imageRect);

            Color lineColor = lineKind == CutoutLineKind.Background
                ? Color.FromArgb(230, 14, 165, 233)
                : Color.FromArgb(230, 34, 197, 94);
            using (Pen shadow = new Pen(Color.FromArgb(150, Color.White), 6))
            using (Pen pen = new Pen(lineColor, 3))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                shadow.StartCap = LineCap.Round;
                shadow.EndCap = LineCap.Round;

                foreach (CutoutLine line in lines)
                    DrawLine(e.Graphics, imageRect, line.Start, line.End, shadow, pen);

                if (drawing)
                    DrawLine(e.Graphics, imageRect, lastRatio, currentRatio, shadow, pen);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left)
                return;

            lastRatio = PointToRatio(e.Location);
            currentRatio = lastRatio;
            strokeStartIndexes.Add(lines.Count);
            drawing = true;
            Capture = true;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!drawing)
                return;

            PointF nextRatio = PointToRatio(e.Location);
            if (ShouldAddSegment(lastRatio, nextRatio))
            {
                lines.Add(new CutoutLine(lastRatio, nextRatio, lineKind));
                lastRatio = nextRatio;
            }

            currentRatio = nextRatio;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!drawing)
                return;

            currentRatio = PointToRatio(e.Location);
            drawing = false;
            Capture = false;

            if (ShouldAddSegment(lastRatio, currentRatio))
                lines.Add(new CutoutLine(lastRatio, currentRatio, lineKind));

            if (strokeStartIndexes.Count > 0 && strokeStartIndexes[strokeStartIndexes.Count - 1] == lines.Count)
                strokeStartIndexes.RemoveAt(strokeStartIndexes.Count - 1);

            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (image != null)
                    image.Dispose();
                if (stream != null)
                    stream.Dispose();
            }

            base.Dispose(disposing);
        }

        private void DrawLine(Graphics graphics, Rectangle imageRect, PointF start, PointF end, Pen shadow, Pen pen)
        {
            Point startPoint = RatioToPoint(imageRect, start);
            Point endPoint = RatioToPoint(imageRect, end);
            graphics.DrawLine(shadow, startPoint, endPoint);
            graphics.DrawLine(pen, startPoint, endPoint);
        }

        private void DrawForegroundPreviewFill(Graphics graphics, Rectangle imageRect)
        {
            List<Point> points = BuildPreviewPolygon(imageRect);
            if (points.Count < 3)
                return;

            using (GraphicsPath path = new GraphicsPath())
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(42, 34, 197, 94)))
            {
                path.AddPolygon(points.ToArray());
                graphics.FillPath(fill, path);
            }
        }

        private List<Point> BuildPreviewPolygon(Rectangle imageRect)
        {
            List<Point> points = new List<Point>();
            foreach (CutoutLine line in lines)
            {
                Point start = RatioToPoint(imageRect, line.Start);
                Point end = RatioToPoint(imageRect, line.End);
                if (points.Count == 0)
                {
                    points.Add(start);
                    points.Add(end);
                    continue;
                }

                Point previous = points[points.Count - 1];
                if (GetDistanceSquared(previous, start) > GetDistanceSquared(previous, end))
                {
                    Point temp = start;
                    start = end;
                    end = temp;
                }

                if (GetDistanceSquared(points[points.Count - 1], start) > 9)
                    points.Add(start);

                if (GetDistanceSquared(points[points.Count - 1], end) > 2)
                    points.Add(end);
            }

            return points;
        }

        private static int GetDistanceSquared(Point a, Point b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return (dx * dx) + (dy * dy);
        }

        private static bool ShouldAddSegment(PointF start, PointF end)
        {
            float dx = start.X - end.X;
            float dy = start.Y - end.Y;
            return (dx * dx) + (dy * dy) > 0.000025f;
        }

        private Rectangle GetImageRect()
        {
            int margin = 12;
            int availableWidth = Math.Max(1, Width - (margin * 2));
            int availableHeight = Math.Max(1, Height - (margin * 2));
            float ratio = Math.Min(availableWidth / (float)Math.Max(1, image.Width), availableHeight / (float)Math.Max(1, image.Height));
            int drawWidth = Math.Max(1, (int)Math.Round(image.Width * ratio));
            int drawHeight = Math.Max(1, (int)Math.Round(image.Height * ratio));
            return new Rectangle((Width - drawWidth) / 2, (Height - drawHeight) / 2, drawWidth, drawHeight);
        }

        private PointF PointToRatio(Point point)
        {
            Rectangle rect = GetImageRect();
            float x = (point.X - rect.Left) / (float)Math.Max(1, rect.Width);
            float y = (point.Y - rect.Top) / (float)Math.Max(1, rect.Height);
            return new PointF(ClampRatio(x), ClampRatio(y));
        }

        private static Point RatioToPoint(Rectangle rect, PointF ratio)
        {
            return new Point(
                rect.Left + (int)Math.Round(rect.Width * ClampRatio(ratio.X)),
                rect.Top + (int)Math.Round(rect.Height * ClampRatio(ratio.Y)));
        }

        private static float ClampRatio(float value)
        {
            if (value < 0.0f)
                return 0.0f;
            if (value > 1.0f)
                return 1.0f;
            return value;
        }
    }

    internal sealed class VoiceSettingsForm : Form
    {
        private readonly VoiceSettings settings;
        private readonly Action onSaved;
        private readonly CheckBox enabledCheck;
        private readonly ComboBox engineCombo;
        private readonly CheckBox maleCheck;
        private readonly CheckBox femaleCheck;
        private readonly TrackBar toneTrack;
        private readonly Label toneValueLabel;
        private readonly TrackBar speedTrack;
        private readonly Label speedValueLabel;
        private readonly TrackBar stepsTrack;
        private readonly Label stepsValueLabel;
        private readonly TextBox apiKeyBox;
        private readonly Label apiKeyStatusLabel;
        private readonly TextBox voiceIdBox;
        private readonly ComboBox languageCombo;
        private readonly ComboBox modelCombo;
        private readonly TextBox styleBox;
        private readonly NumericUpDown maxTextLengthNumeric;
        private bool suppressGenderEvents;

        public VoiceSettingsForm(VoiceSettings settings, Action onSaved)
        {
            this.settings = settings;
            this.onSaved = onSaved;

            Text = TextResources.VoiceSettings;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(500, 540);

            enabledCheck = new CheckBox();
            enabledCheck.Text = TextResources.VoiceOnDrag;
            enabledCheck.Location = new Point(14, 14);
            enabledCheck.Size = new Size(250, 24);

            Label engineLabel = CreateLabel(TextResources.VoiceEngine, 14, 230);
            engineCombo = new ComboBox();
            engineCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            engineCombo.Location = new Point(112, 228);
            engineCombo.Size = new Size(200, 24);
            engineCombo.Items.Add(TextResources.VoiceEngineSupertonic);
            engineCombo.Items.Add(TextResources.VoiceEngineSupertoneApi);

            Label genderLabel = CreateLabel(TextResources.VoiceGender, 14, 264);
            maleCheck = new CheckBox();
            maleCheck.Text = TextResources.GenderMale;
            maleCheck.Location = new Point(112, 262);
            maleCheck.Size = new Size(70, 24);
            maleCheck.CheckedChanged += OnGenderCheckedChanged;

            femaleCheck = new CheckBox();
            femaleCheck.Text = TextResources.GenderFemale;
            femaleCheck.Location = new Point(190, 262);
            femaleCheck.Size = new Size(70, 24);
            femaleCheck.CheckedChanged += OnGenderCheckedChanged;

            Label toneLabel = CreateLabel(TextResources.VoiceTone, 14, 298);
            toneTrack = new TrackBar();
            toneTrack.Location = new Point(112, 294);
            toneTrack.Size = new Size(300, 45);
            toneTrack.Minimum = 1;
            toneTrack.Maximum = 5;
            toneTrack.TickFrequency = 1;
            toneTrack.SmallChange = 1;
            toneTrack.LargeChange = 1;
            toneTrack.ValueChanged += delegate { UpdateGaugeLabels(); };

            toneValueLabel = new Label();
            toneValueLabel.Location = new Point(420, 301);
            toneValueLabel.Size = new Size(58, 20);

            Label speedLabel = CreateLabel(TextResources.Speed, 14, 352);
            speedTrack = new TrackBar();
            speedTrack.Location = new Point(112, 348);
            speedTrack.Size = new Size(300, 45);
            speedTrack.Minimum = VoiceSettings.MinSpeedPercent;
            speedTrack.Maximum = VoiceSettings.MaxSpeedPercent;
            speedTrack.TickFrequency = 25;
            speedTrack.SmallChange = 5;
            speedTrack.LargeChange = 25;
            speedTrack.ValueChanged += delegate { UpdateGaugeLabels(); };

            speedValueLabel = new Label();
            speedValueLabel.Location = new Point(420, 355);
            speedValueLabel.Size = new Size(58, 20);

            Label stepsLabel = CreateLabel(TextResources.VoiceQuality, 14, 406);
            stepsTrack = new TrackBar();
            stepsTrack.Location = new Point(112, 402);
            stepsTrack.Size = new Size(300, 45);
            stepsTrack.Minimum = VoiceSettings.MinLocalSteps;
            stepsTrack.Maximum = VoiceSettings.MaxLocalSteps;
            stepsTrack.TickFrequency = 4;
            stepsTrack.SmallChange = 1;
            stepsTrack.LargeChange = 4;
            stepsTrack.ValueChanged += delegate { UpdateGaugeLabels(); };

            stepsValueLabel = new Label();
            stepsValueLabel.Location = new Point(420, 409);
            stepsValueLabel.Size = new Size(58, 20);

            Label apiLabel = CreateLabel(TextResources.ApiKey, 14, 50);
            apiKeyBox = new TextBox();
            apiKeyBox.Location = new Point(112, 48);
            apiKeyBox.Size = new Size(366, 22);
            apiKeyBox.PasswordChar = '*';

            apiKeyStatusLabel = new Label();
            apiKeyStatusLabel.Location = new Point(112, 72);
            apiKeyStatusLabel.Size = new Size(366, 20);

            Label voiceIdLabel = CreateLabel(TextResources.VoiceId, 14, 100);
            voiceIdBox = new TextBox();
            voiceIdBox.Location = new Point(112, 98);
            voiceIdBox.Size = new Size(366, 22);

            Label languageLabel = CreateLabel(TextResources.Language, 14, 130);
            languageCombo = new ComboBox();
            languageCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            languageCombo.Location = new Point(112, 128);
            languageCombo.Size = new Size(100, 24);
            languageCombo.Items.Add("ko");
            languageCombo.Items.Add("en");
            languageCombo.Items.Add("ja");
            languageCombo.Items.Add("bg");
            languageCombo.Items.Add("cs");
            languageCombo.Items.Add("da");
            languageCombo.Items.Add("el");
            languageCombo.Items.Add("es");
            languageCombo.Items.Add("et");
            languageCombo.Items.Add("fi");
            languageCombo.Items.Add("hu");
            languageCombo.Items.Add("it");
            languageCombo.Items.Add("nl");
            languageCombo.Items.Add("pl");
            languageCombo.Items.Add("pt");
            languageCombo.Items.Add("ro");
            languageCombo.Items.Add("ar");
            languageCombo.Items.Add("de");
            languageCombo.Items.Add("fr");
            languageCombo.Items.Add("hi");
            languageCombo.Items.Add("id");
            languageCombo.Items.Add("ru");
            languageCombo.Items.Add("vi");

            Label modelLabel = CreateLabel(TextResources.Model, 222, 130);
            modelLabel.Size = new Size(54, 20);
            modelCombo = new ComboBox();
            modelCombo.DropDownStyle = ComboBoxStyle.DropDown;
            modelCombo.Location = new Point(284, 128);
            modelCombo.Size = new Size(194, 24);
            modelCombo.Items.Add("sona_speech_1");
            modelCombo.Items.Add("sona_speech_2");
            modelCombo.Items.Add("sona_speech_2_flash");
            modelCombo.Items.Add("sona_speech_2t");
            modelCombo.Items.Add("supertonic_api_1");

            Label styleLabel = CreateLabel(TextResources.Style, 14, 162);
            styleBox = new TextBox();
            styleBox.Location = new Point(112, 160);
            styleBox.Size = new Size(366, 22);

            Label maxTextLengthLabel = CreateLabel(TextResources.MaxTextLength, 14, 194);
            maxTextLengthNumeric = new NumericUpDown();
            maxTextLengthNumeric.Location = new Point(112, 192);
            maxTextLengthNumeric.Size = new Size(70, 22);
            maxTextLengthNumeric.Minimum = 1;
            maxTextLengthNumeric.Maximum = VoiceSettings.MaxAllowedTextLength;

            Button clearKeyButton = new Button();
            clearKeyButton.Text = TextResources.ClearApiKey;
            clearKeyButton.Location = new Point(14, 460);
            clearKeyButton.Size = new Size(116, 28);
            clearKeyButton.Click += OnClearApiKeyClicked;

            Button saveButton = new Button();
            saveButton.Text = TextResources.Save;
            saveButton.Location = new Point(318, 496);
            saveButton.Size = new Size(76, 28);
            saveButton.Click += OnSaveClicked;

            Button closeButton = new Button();
            closeButton.Text = TextResources.Close;
            closeButton.Location = new Point(402, 496);
            closeButton.Size = new Size(76, 28);
            closeButton.Click += OnCloseClicked;

            Controls.Add(enabledCheck);
            Controls.Add(engineLabel);
            Controls.Add(engineCombo);
            Controls.Add(genderLabel);
            Controls.Add(maleCheck);
            Controls.Add(femaleCheck);
            Controls.Add(toneLabel);
            Controls.Add(toneTrack);
            Controls.Add(toneValueLabel);
            Controls.Add(speedLabel);
            Controls.Add(speedTrack);
            Controls.Add(speedValueLabel);
            Controls.Add(stepsLabel);
            Controls.Add(stepsTrack);
            Controls.Add(stepsValueLabel);
            Controls.Add(apiLabel);
            Controls.Add(apiKeyBox);
            Controls.Add(apiKeyStatusLabel);
            Controls.Add(voiceIdLabel);
            Controls.Add(voiceIdBox);
            Controls.Add(languageLabel);
            Controls.Add(languageCombo);
            Controls.Add(modelLabel);
            Controls.Add(modelCombo);
            Controls.Add(styleLabel);
            Controls.Add(styleBox);
            Controls.Add(maxTextLengthLabel);
            Controls.Add(maxTextLengthNumeric);
            Controls.Add(clearKeyButton);
            Controls.Add(saveButton);
            Controls.Add(closeButton);

            Reload();
        }

        private void OnGenderCheckedChanged(object sender, EventArgs e)
        {
            if (suppressGenderEvents)
                return;

            suppressGenderEvents = true;
            if (sender == maleCheck && maleCheck.Checked)
                femaleCheck.Checked = false;
            else if (sender == femaleCheck && femaleCheck.Checked)
                maleCheck.Checked = false;
            else if (!maleCheck.Checked && !femaleCheck.Checked)
            {
                if (sender == maleCheck)
                    femaleCheck.Checked = true;
                else
                    maleCheck.Checked = true;
            }
            suppressGenderEvents = false;
        }

        private void UpdateGaugeLabels()
        {
            toneValueLabel.Text = toneTrack.Value.ToString(CultureInfo.InvariantCulture);
            speedValueLabel.Text = speedTrack.Value.ToString(CultureInfo.InvariantCulture) + "%";
            stepsValueLabel.Text = stepsTrack.Value.ToString(CultureInfo.InvariantCulture);
        }

        public void Reload()
        {
            enabledCheck.Checked = settings.Enabled;
            engineCombo.SelectedIndex = settings.UsesSupertonicEngine() ? 0 : 1;

            string localVoice = VoiceSettings.NormalizeLocalVoice(settings.LocalVoice);
            bool isMale = localVoice.StartsWith("M", StringComparison.OrdinalIgnoreCase);
            int variant = 1;
            char lastChar = localVoice[localVoice.Length - 1];
            if (lastChar >= '1' && lastChar <= '5')
                variant = lastChar - '0';

            suppressGenderEvents = true;
            maleCheck.Checked = isMale;
            femaleCheck.Checked = !isMale;
            suppressGenderEvents = false;

            toneTrack.Value = variant;
            speedTrack.Value = VoiceSettings.ClampSpeedPercent(settings.SpeedPercent);
            stepsTrack.Value = VoiceSettings.ClampLocalSteps(settings.LocalSteps);
            UpdateGaugeLabels();

            apiKeyBox.Text = "";
            voiceIdBox.Text = settings.VoiceId;
            SelectComboText(languageCombo, settings.Language);
            modelCombo.Text = settings.Model;
            styleBox.Text = settings.Style;
            maxTextLengthNumeric.Value = VoiceSettings.ClampMaxTextLength(settings.MaxTextLength);
            UpdateApiKeyStatus();
        }

        private static Label CreateLabel(string text, int x, int y)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = new Point(x, y + 3);
            label.Size = new Size(98, 20);
            return label;
        }

        private static void SelectComboText(ComboBox comboBox, string text)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (string.Equals(comboBox.Items[i].ToString(), text, StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedIndex = i;
                    return;
                }
            }

            if (comboBox.Items.Count > 0)
                comboBox.SelectedIndex = 0;
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                settings.Enabled = enabledCheck.Checked;
                settings.Engine = engineCombo.SelectedIndex == 1 ? VoiceSettings.EngineSupertoneApi : VoiceSettings.EngineSupertonic;
                settings.LocalVoice = (maleCheck.Checked ? "M" : "F") + toneTrack.Value.ToString(CultureInfo.InvariantCulture);
                settings.LocalSteps = VoiceSettings.ClampLocalSteps(stepsTrack.Value);
                settings.VoiceId = voiceIdBox.Text.Trim();
                settings.Language = languageCombo.Text.Trim().ToLowerInvariant();
                settings.Model = modelCombo.Text.Trim();
                settings.Style = styleBox.Text.Trim();
                settings.SpeedPercent = VoiceSettings.ClampSpeedPercent(speedTrack.Value);
                settings.MaxTextLength = VoiceSettings.ClampMaxTextLength((int)maxTextLengthNumeric.Value);

                string apiKey = apiKeyBox.Text.Trim();
                if (apiKey.Length > 0)
                {
                    VoiceSettings.SaveApiKey(apiKey);
                    apiKeyBox.Text = "";
                }

                settings.Save();
                UpdateApiKeyStatus();
                if (onSaved != null)
                    onSaved();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, TextResources.VoiceSettings, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnClearApiKeyClicked(object sender, EventArgs e)
        {
            VoiceSettings.ClearApiKey();
            apiKeyBox.Text = "";
            UpdateApiKeyStatus();
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close();
        }

        private void UpdateApiKeyStatus()
        {
            apiKeyStatusLabel.Text = VoiceSettings.HasApiKey() ? TextResources.ApiKeySaved : TextResources.ApiKeyMissing;
        }
    }

    internal sealed class HotkeySettingsForm : Form
    {
        private readonly VoiceSettings settings;
        private readonly Action onSaved;
        private readonly TextBox toggleBox;
        private readonly TextBox stopBox;
        private int pendingToggleModifiers;
        private int pendingToggleKey;
        private int pendingStopModifiers;
        private int pendingStopKey;

        public HotkeySettingsForm(VoiceSettings settings, Action onSaved)
        {
            this.settings = settings;
            this.onSaved = onSaved;

            Text = TextResources.VoiceHotkeyMenu;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(420, 132);

            Label toggleLabel = new Label();
            toggleLabel.Text = TextResources.HotkeyToggleLabel;
            toggleLabel.Location = new Point(14, 17);
            toggleLabel.Size = new Size(76, 20);

            toggleBox = new TextBox();
            toggleBox.Location = new Point(96, 14);
            toggleBox.Size = new Size(230, 22);
            toggleBox.ReadOnly = true;
            toggleBox.BackColor = SystemColors.Window;
            toggleBox.KeyDown += OnHotkeyBoxKeyDown;
            toggleBox.GotFocus += delegate { UpdateBoxes(); };
            toggleBox.LostFocus += delegate { UpdateBoxes(); };

            Button toggleClearButton = new Button();
            toggleClearButton.Text = TextResources.HotkeyClear;
            toggleClearButton.Location = new Point(338, 12);
            toggleClearButton.Size = new Size(68, 26);
            toggleClearButton.Click += delegate
            {
                pendingToggleModifiers = 0;
                pendingToggleKey = 0;
                UpdateBoxes();
            };

            Label stopLabel = new Label();
            stopLabel.Text = TextResources.HotkeyStopLabel;
            stopLabel.Location = new Point(14, 51);
            stopLabel.Size = new Size(76, 20);

            stopBox = new TextBox();
            stopBox.Location = new Point(96, 48);
            stopBox.Size = new Size(230, 22);
            stopBox.ReadOnly = true;
            stopBox.BackColor = SystemColors.Window;
            stopBox.KeyDown += OnHotkeyBoxKeyDown;
            stopBox.GotFocus += delegate { UpdateBoxes(); };
            stopBox.LostFocus += delegate { UpdateBoxes(); };

            Button stopClearButton = new Button();
            stopClearButton.Text = TextResources.HotkeyClear;
            stopClearButton.Location = new Point(338, 46);
            stopClearButton.Size = new Size(68, 26);
            stopClearButton.Click += delegate
            {
                pendingStopModifiers = 0;
                pendingStopKey = 0;
                UpdateBoxes();
            };

            Button saveButton = new Button();
            saveButton.Text = TextResources.Save;
            saveButton.Location = new Point(238, 90);
            saveButton.Size = new Size(76, 28);
            saveButton.Click += OnSaveClicked;

            Button closeButton = new Button();
            closeButton.Text = TextResources.Close;
            closeButton.Location = new Point(322, 90);
            closeButton.Size = new Size(76, 28);
            closeButton.Click += delegate { Close(); };

            Controls.Add(toggleLabel);
            Controls.Add(toggleBox);
            Controls.Add(toggleClearButton);
            Controls.Add(stopLabel);
            Controls.Add(stopBox);
            Controls.Add(stopClearButton);
            Controls.Add(saveButton);
            Controls.Add(closeButton);

            Reload();
        }

        public void Reload()
        {
            pendingToggleModifiers = settings.HotkeyModifiers;
            pendingToggleKey = settings.HotkeyKey;
            pendingStopModifiers = settings.StopHotkeyModifiers;
            pendingStopKey = settings.StopHotkeyKey;
            UpdateBoxes();
        }

        private void UpdateBoxes()
        {
            if (pendingToggleKey == 0 && toggleBox.Focused)
                toggleBox.Text = TextResources.HotkeyInputHint;
            else
                toggleBox.Text = VoiceSettings.FormatHotkey(pendingToggleModifiers, pendingToggleKey);

            if (pendingStopKey == 0 && stopBox.Focused)
                stopBox.Text = TextResources.HotkeyInputHint;
            else
                stopBox.Text = VoiceSettings.FormatHotkey(pendingStopModifiers, pendingStopKey);
        }

        private void OnHotkeyBoxKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;

            Keys code = e.KeyCode;
            if (code == Keys.ControlKey || code == Keys.ShiftKey || code == Keys.Menu ||
                code == Keys.LWin || code == Keys.RWin || code == Keys.None)
            {
                return;
            }

            int modifiers = 0;
            if ((e.Modifiers & Keys.Control) != 0)
                modifiers |= 2;
            if ((e.Modifiers & Keys.Alt) != 0)
                modifiers |= 1;
            if ((e.Modifiers & Keys.Shift) != 0)
                modifiers |= 4;

            if (sender == toggleBox)
            {
                pendingToggleModifiers = modifiers;
                pendingToggleKey = (int)code;
            }
            else
            {
                pendingStopModifiers = modifiers;
                pendingStopKey = (int)code;
            }

            UpdateBoxes();
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            bool toggleInvalid = pendingToggleKey != 0 && (pendingToggleModifiers & 3) == 0;
            bool stopInvalid = pendingStopKey != 0 && (pendingStopModifiers & 3) == 0;
            if (toggleInvalid || stopInvalid)
            {
                MessageBox.Show(TextResources.HotkeyNeedModifier, TextResources.VoiceHotkeyMenu, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            settings.HotkeyModifiers = pendingToggleModifiers;
            settings.HotkeyKey = pendingToggleKey;
            settings.StopHotkeyModifiers = pendingStopModifiers;
            settings.StopHotkeyKey = pendingStopKey;
            settings.Save();
            if (onSaved != null)
                onSaved();
            Close();
        }
    }

    internal sealed class LicenseRegistrationForm : Form
    {
        private readonly LicenseSettings settings;
        private readonly LicenseManager manager;
        private readonly Action<LicenseStatus> onChanged;
        private readonly TextBox serverBox;
        private readonly TextBox licenseKeyBox;
        private readonly Label statusLabel;
        private readonly Button activateButton;
        private readonly Button deactivateButton;

        public LicenseRegistrationForm(LicenseSettings settings, LicenseManager manager, Action<LicenseStatus> onChanged)
        {
            this.settings = settings;
            this.manager = manager;
            this.onChanged = onChanged;

            Text = TextResources.LicenseRegister;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(520, 214);

            Label serverLabel = CreateLabel(TextResources.LicenseServer, 14, 18);
            serverBox = new TextBox();
            serverBox.Location = new Point(122, 16);
            serverBox.Size = new Size(376, 22);

            Label keyLabel = CreateLabel(TextResources.LicenseKey, 14, 56);
            licenseKeyBox = new TextBox();
            licenseKeyBox.Location = new Point(122, 54);
            licenseKeyBox.Size = new Size(376, 22);

            statusLabel = new Label();
            statusLabel.Location = new Point(122, 86);
            statusLabel.Size = new Size(376, 44);
            statusLabel.AutoEllipsis = true;

            activateButton = new Button();
            activateButton.Text = TextResources.Activate;
            activateButton.Location = new Point(250, 162);
            activateButton.Size = new Size(78, 28);
            activateButton.Click += OnActivateClicked;

            deactivateButton = new Button();
            deactivateButton.Text = TextResources.Deactivate;
            deactivateButton.Location = new Point(334, 162);
            deactivateButton.Size = new Size(78, 28);
            deactivateButton.Click += OnDeactivateClicked;

            Button closeButton = new Button();
            closeButton.Text = TextResources.Close;
            closeButton.Location = new Point(420, 162);
            closeButton.Size = new Size(78, 28);
            closeButton.Click += OnCloseClicked;

            Controls.Add(serverLabel);
            Controls.Add(serverBox);
            Controls.Add(keyLabel);
            Controls.Add(licenseKeyBox);
            Controls.Add(statusLabel);
            Controls.Add(activateButton);
            Controls.Add(deactivateButton);
            Controls.Add(closeButton);

            Reload();
        }

        public void Reload()
        {
            serverBox.Text = LicenseSettings.NormalizeApiBaseUrl(settings.ApiBaseUrl);
            licenseKeyBox.Text = "";
            LicenseStatus status = manager.GetStatus(false);
            statusLabel.Text = FormatStatus(status);
            deactivateButton.Enabled = status.State != LicenseState.Missing;
        }

        private void OnActivateClicked(object sender, EventArgs e)
        {
            string serverUrl = serverBox.Text.Trim();
            string licenseKey = licenseKeyBox.Text.Trim();
            SetBusy(true);
            statusLabel.Text = TextResources.Checking;

            ThreadPool.QueueUserWorkItem(delegate
            {
                LicenseStatus status;
                try
                {
                    status = manager.Activate(serverUrl, licenseKey);
                }
                catch (Exception ex)
                {
                    status = new LicenseStatus();
                    status.State = LicenseState.Invalid;
                    status.Message = ex.Message;
                }

                BeginInvokeIfAlive(delegate
                {
                    SetBusy(false);
                    statusLabel.Text = FormatStatus(status);
                    licenseKeyBox.Text = "";
                    deactivateButton.Enabled = status.State != LicenseState.Missing;
                    if (onChanged != null)
                        onChanged(status);
                });
            });
        }

        private void OnDeactivateClicked(object sender, EventArgs e)
        {
            SetBusy(true);
            statusLabel.Text = TextResources.Checking;
            ThreadPool.QueueUserWorkItem(delegate
            {
                LicenseStatus status;
                try
                {
                    status = manager.Deactivate();
                }
                catch (Exception ex)
                {
                    status = new LicenseStatus();
                    status.State = LicenseState.Invalid;
                    status.Message = ex.Message;
                }

                BeginInvokeIfAlive(delegate
                {
                    SetBusy(false);
                    statusLabel.Text = FormatStatus(status);
                    deactivateButton.Enabled = false;
                    if (onChanged != null)
                        onChanged(status);
                });
            });
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close();
        }

        private void SetBusy(bool busy)
        {
            activateButton.Enabled = !busy;
            deactivateButton.Enabled = !busy && LicenseSettings.LoadLicenseKey().Length > 0;
            serverBox.Enabled = !busy;
            licenseKeyBox.Enabled = !busy;
        }

        private void BeginInvokeIfAlive(Action action)
        {
            if (IsDisposed)
                return;

            try
            {
                BeginInvoke(action);
            }
            catch
            {
            }
        }

        private static string FormatStatus(LicenseStatus status)
        {
            if (status.State == LicenseState.Active)
                return TextResources.LicenseValid + " - " + status.Detail;
            if (status.State == LicenseState.OfflineActive)
                return TextResources.LicenseOfflineValid + " - " + status.Detail;
            if (status.State == LicenseState.Missing)
                return TextResources.LicenseMissing;
            return TextResources.LicenseInvalid + (status.Message.Length > 0 ? " - " + status.Message : "");
        }

        private static Label CreateLabel(string text, int x, int y)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = new Point(x, y + 3);
            label.Size = new Size(104, 20);
            return label;
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
            ClientSize = new Size(380, 132);

            valueLabel = new Label();
            valueLabel.AutoSize = false;
            valueLabel.TextAlign = ContentAlignment.MiddleLeft;
            valueLabel.Location = new Point(12, 10);
            valueLabel.Size = new Size(250, 24);

            numeric = new NumericUpDown();
            numeric.Minimum = AppSettings.MinSizePercent;
            numeric.Maximum = AppSettings.MaxSizePercent;
            numeric.Increment = 5;
            numeric.Location = new Point(282, 10);
            numeric.Size = new Size(78, 24);
            numeric.ValueChanged += OnNumericChanged;

            trackBar = new TrackBar();
            trackBar.Minimum = AppSettings.MinSizePercent;
            trackBar.Maximum = AppSettings.MaxSizePercent;
            trackBar.TickFrequency = 25;
            trackBar.SmallChange = 5;
            trackBar.LargeChange = 25;
            trackBar.Location = new Point(10, 42);
            trackBar.Size = new Size(358, 45);
            trackBar.Scroll += OnTrackBarChanged;

            Button closeButton = new Button();
            closeButton.Text = TextResources.Close;
            closeButton.Location = new Point(298, 96);
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

    internal sealed class FaceCenterSettingsForm : Form
    {
        private readonly IndicatorAssets assets;
        private readonly AppSettings settings;
        private readonly Action<string, IndicatorPose, PointF> onChanged;
        private readonly ComboBox stateCombo;
        private readonly ComboBox poseCombo;
        private readonly FaceCenterPreview preview;
        private readonly Label coordinateLabel;

        public FaceCenterSettingsForm(IndicatorAssets assets, AppSettings settings, Action<string, IndicatorPose, PointF> onChanged)
        {
            this.assets = assets;
            this.settings = settings;
            this.onChanged = onChanged;

            Text = TextResources.AdjustFaceCenter;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(390, 420);

            Label stateLabel = new Label();
            stateLabel.Text = TextResources.State;
            stateLabel.Location = new Point(14, 14);
            stateLabel.Size = new Size(48, 22);

            stateCombo = new ComboBox();
            stateCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            stateCombo.Location = new Point(66, 12);
            stateCombo.Size = new Size(150, 24);
            foreach (string stateKey in IndicatorStates.All)
                stateCombo.Items.Add(new StateItem(stateKey, IndicatorStates.GetDisplayName(stateKey)));
            stateCombo.SelectedIndexChanged += OnSelectionChanged;

            Label poseLabel = new Label();
            poseLabel.Text = TextResources.Pose;
            poseLabel.Location = new Point(230, 14);
            poseLabel.Size = new Size(48, 22);

            poseCombo = new ComboBox();
            poseCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            poseCombo.Location = new Point(278, 12);
            poseCombo.Size = new Size(96, 24);
            foreach (IndicatorPose pose in IndicatorPoseHelper.All)
                poseCombo.Items.Add(new PoseItem(pose, IndicatorPoseHelper.GetDisplayName(pose)));
            poseCombo.SelectedIndexChanged += OnSelectionChanged;

            coordinateLabel = new Label();
            coordinateLabel.AutoSize = false;
            coordinateLabel.TextAlign = ContentAlignment.MiddleLeft;
            coordinateLabel.Location = new Point(14, 42);
            coordinateLabel.Size = new Size(360, 24);

            preview = new FaceCenterPreview(assets, settings);
            preview.Location = new Point(55, 74);
            preview.Size = new Size(280, 280);
            preview.CenterChanged += OnPreviewCenterChanged;

            Button resetButton = new Button();
            resetButton.Text = TextResources.Reset;
            resetButton.Location = new Point(218, 376);
            resetButton.Size = new Size(76, 26);
            resetButton.Click += OnResetClicked;

            Button closeButton = new Button();
            closeButton.Text = TextResources.Close;
            closeButton.Location = new Point(300, 376);
            closeButton.Size = new Size(74, 26);
            closeButton.Click += OnCloseClicked;

            Controls.Add(stateLabel);
            Controls.Add(stateCombo);
            Controls.Add(poseLabel);
            Controls.Add(poseCombo);
            Controls.Add(coordinateLabel);
            Controls.Add(preview);
            Controls.Add(resetButton);
            Controls.Add(closeButton);
            stateCombo.SelectedIndex = 0;
            poseCombo.SelectedIndex = 0;
            RefreshPreview();
        }

        public void RefreshPreview()
        {
            if (preview == null || coordinateLabel == null || poseCombo == null)
                return;

            string stateKey = GetSelectedStateKey();
            IndicatorPose pose = GetSelectedPose();
            preview.SetSelection(stateKey, pose);
            PointF center = settings.GetLabelCenterByState(stateKey, pose);
            coordinateLabel.Text = string.Format(
                CultureInfo.InvariantCulture,
                "{0} / {1}    X {2:0}%  Y {3:0}%",
                stateKey,
                IndicatorPoseHelper.GetKey(pose),
                center.X * 100.0f,
                center.Y * 100.0f);
        }

        private string GetSelectedStateKey()
        {
            StateItem item = stateCombo.SelectedItem as StateItem;
            if (item == null)
                return IndicatorStates.Korean;

            return item.StateKey;
        }

        private IndicatorPose GetSelectedPose()
        {
            PoseItem item = poseCombo.SelectedItem as PoseItem;
            if (item == null)
                return IndicatorPose.Idle;

            return item.Pose;
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            RefreshPreview();
        }

        private void OnPreviewCenterChanged(object sender, FaceCenterChangedEventArgs e)
        {
            onChanged(GetSelectedStateKey(), GetSelectedPose(), e.Center);
        }

        private void OnResetClicked(object sender, EventArgs e)
        {
            string stateKey = GetSelectedStateKey();
            IndicatorPose pose = GetSelectedPose();
            onChanged(stateKey, pose, AppSettings.GetDefaultFaceCenter(pose));
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close();
        }

        private sealed class StateItem
        {
            public StateItem(string stateKey, string name)
            {
                StateKey = stateKey;
                Name = name;
            }

            public string StateKey { get; private set; }

            private string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        private sealed class PoseItem
        {
            public PoseItem(IndicatorPose pose, string name)
            {
                Pose = pose;
                Name = name;
            }

            public IndicatorPose Pose { get; private set; }

            private string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }
    }

    internal sealed class FaceCenterPreview : Control
    {
        private readonly IndicatorAssets assets;
        private readonly AppSettings settings;
        private string stateKey = IndicatorStates.Korean;
        private IndicatorPose pose = IndicatorPose.Idle;
        private bool dragging;

        public FaceCenterPreview(IndicatorAssets assets, AppSettings settings)
        {
            this.assets = assets;
            this.settings = settings;
            DoubleBuffered = true;
            BackColor = Color.White;
            Cursor = Cursors.Cross;
        }

        public event EventHandler<FaceCenterChangedEventArgs> CenterChanged;

        public void SetSelection(string stateKey, IndicatorPose pose)
        {
            this.stateKey = IndicatorStates.IsValidKey(stateKey) ? stateKey : IndicatorStates.Korean;
            this.pose = pose;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle imageRect = GetImageRect();
            using (SolidBrush background = new SolidBrush(Color.FromArgb(246, 248, 252)))
            using (Pen border = new Pen(Color.FromArgb(203, 213, 225)))
            {
                e.Graphics.FillRectangle(background, ClientRectangle);
                e.Graphics.DrawRectangle(border, new Rectangle(0, 0, Width - 1, Height - 1));
            }

            bool mascotImage;
            IndicatorImage image = assets.GetImageByStateKey(stateKey, pose, out mascotImage);
            if (image != null)
            {
                string label = IndicatorStates.ToLabel(stateKey);
                using (Bitmap tinted = MascotColorizer.CreateTintedBitmap(image.Image, settings.GetMascotColor(label), settings.GetFaceCenter(pose)))
                {
                    e.Graphics.DrawImage(tinted, imageRect);
                }
            }

            PointF center = settings.GetLabelCenterByState(stateKey, pose);
            Point marker = new Point(
                imageRect.Left + (int)Math.Round(imageRect.Width * center.X),
                imageRect.Top + (int)Math.Round(imageRect.Height * center.Y));

            DrawSampleLabel(e.Graphics, imageRect, center);

            using (Pen pen = new Pen(Color.FromArgb(220, 30, 64, 175), 2))
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(240, 255, 255, 255)))
            {
                e.Graphics.DrawLine(pen, marker.X - 12, marker.Y, marker.X + 12, marker.Y);
                e.Graphics.DrawLine(pen, marker.X, marker.Y - 12, marker.X, marker.Y + 12);
                e.Graphics.FillEllipse(fill, marker.X - 5, marker.Y - 5, 10, 10);
                e.Graphics.DrawEllipse(pen, marker.X - 5, marker.Y - 5, 10, 10);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left)
                return;

            dragging = true;
            Capture = true;
            UpdateCenterFromMouse(e.Location);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragging)
                UpdateCenterFromMouse(e.Location);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            dragging = false;
            Capture = false;
        }

        private Rectangle GetImageRect()
        {
            int side = Math.Min(Width, Height) - 24;
            return new Rectangle((Width - side) / 2, (Height - side) / 2, side, side);
        }

        private void UpdateCenterFromMouse(Point point)
        {
            Rectangle rect = GetImageRect();
            float x = (point.X - rect.Left) / (float)Math.Max(1, rect.Width);
            float y = (point.Y - rect.Top) / (float)Math.Max(1, rect.Height);
            PointF center = AppSettings.ClampFaceCenter(new PointF(x, y));

            EventHandler<FaceCenterChangedEventArgs> handler = CenterChanged;
            if (handler != null)
                handler(this, new FaceCenterChangedEventArgs(center));

            Invalidate();
        }

        private void DrawSampleLabel(Graphics graphics, Rectangle imageRect, PointF center)
        {
            string label = IndicatorStates.ToLabel(stateKey);
            RectangleF faceRect = LabelGeometry.CreateLabelRect(imageRect, center);

            using (Font font = new Font("Malgun Gothic", Math.Max(11.0f, imageRect.Height * 0.13f), FontStyle.Bold, GraphicsUnit.Pixel))
            using (SolidBrush fill = new SolidBrush(settings.GetLabelColor(label)))
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(130, Color.White)))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                RectangleF shadowRect = new RectangleF(faceRect.X + 1, faceRect.Y + 1, faceRect.Width, faceRect.Height);
                graphics.DrawString(label, font, shadow, shadowRect, format);
                graphics.DrawString(label, font, fill, faceRect, format);
            }
        }
    }

    internal sealed class FaceCenterChangedEventArgs : EventArgs
    {
        public FaceCenterChangedEventArgs(PointF center)
        {
            Center = center;
        }

        public PointF Center { get; private set; }
    }

    internal sealed class VoiceSettings
    {
        public const int MinSpeedPercent = 50;
        public const int MaxSpeedPercent = 200;
        public const int MaxAllowedTextLength = 300;

        public const string EngineSupertonic = "supertonic";
        public const string EngineSupertoneApi = "supertone_api";

        public const int MinLocalSteps = 1;
        public const int MaxLocalSteps = 32;

        public bool Enabled = false;
        public string Engine = EngineSupertonic;
        public string LocalVoice = "F1";
        public int LocalSteps = 8;
        public int HotkeyModifiers = 0;
        public int HotkeyKey = 0;
        public int StopHotkeyModifiers = 0;
        public int StopHotkeyKey = 0;
        public string VoiceId = "";
        public string Language = "ko";
        public string Model = "sona_speech_1";
        public string Style = "";
        public int SpeedPercent = 100;
        public int MaxTextLength = MaxAllowedTextLength;

        public bool UsesSupertonicEngine()
        {
            return Engine != EngineSupertoneApi;
        }

        public static VoiceSettings Load()
        {
            VoiceSettings settings = new VoiceSettings();
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

                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    if (key.Equals("enabled", StringComparison.OrdinalIgnoreCase))
                    {
                        bool enabled;
                        if (bool.TryParse(value, out enabled))
                            settings.Enabled = enabled;
                    }
                    else if (key.Equals("engine", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.Engine = NormalizeEngine(value);
                    }
                    else if (key.Equals("localVoice", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.LocalVoice = NormalizeLocalVoice(value);
                    }
                    else if (key.Equals("localSteps", StringComparison.OrdinalIgnoreCase))
                    {
                        int steps;
                        if (int.TryParse(value, out steps))
                            settings.LocalSteps = ClampLocalSteps(steps);
                    }
                    else if (key.Equals("hotkeyModifiers", StringComparison.OrdinalIgnoreCase))
                    {
                        int modifiers;
                        if (int.TryParse(value, out modifiers))
                            settings.HotkeyModifiers = modifiers;
                    }
                    else if (key.Equals("hotkeyKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int hotkey;
                        if (int.TryParse(value, out hotkey))
                            settings.HotkeyKey = hotkey;
                    }
                    else if (key.Equals("stopHotkeyModifiers", StringComparison.OrdinalIgnoreCase))
                    {
                        int modifiers;
                        if (int.TryParse(value, out modifiers))
                            settings.StopHotkeyModifiers = modifiers;
                    }
                    else if (key.Equals("stopHotkeyKey", StringComparison.OrdinalIgnoreCase))
                    {
                        int hotkey;
                        if (int.TryParse(value, out hotkey))
                            settings.StopHotkeyKey = hotkey;
                    }
                    else if (key.Equals("voiceId", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.VoiceId = value;
                    }
                    else if (key.Equals("language", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.Language = NormalizeLanguage(value);
                    }
                    else if (key.Equals("model", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.Model = value;
                    }
                    else if (key.Equals("style", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.Style = value;
                    }
                    else if (key.Equals("speedPercent", StringComparison.OrdinalIgnoreCase))
                    {
                        int speed;
                        if (int.TryParse(value, out speed))
                            settings.SpeedPercent = ClampSpeedPercent(speed);
                    }
                    else if (key.Equals("maxTextLength", StringComparison.OrdinalIgnoreCase))
                    {
                        int maxTextLength;
                        if (int.TryParse(value, out maxTextLength))
                            settings.MaxTextLength = ClampMaxTextLength(maxTextLength);
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
                List<string> lines = new List<string>();
                lines.Add("enabled=" + Enabled);
                lines.Add("engine=" + NormalizeEngine(Engine));
                lines.Add("localVoice=" + NormalizeLocalVoice(LocalVoice));
                lines.Add("localSteps=" + ClampLocalSteps(LocalSteps).ToString(CultureInfo.InvariantCulture));
                lines.Add("hotkeyModifiers=" + HotkeyModifiers.ToString(CultureInfo.InvariantCulture));
                lines.Add("hotkeyKey=" + HotkeyKey.ToString(CultureInfo.InvariantCulture));
                lines.Add("stopHotkeyModifiers=" + StopHotkeyModifiers.ToString(CultureInfo.InvariantCulture));
                lines.Add("stopHotkeyKey=" + StopHotkeyKey.ToString(CultureInfo.InvariantCulture));
                lines.Add("voiceId=" + VoiceId.Trim());
                lines.Add("language=" + NormalizeLanguage(Language));
                lines.Add("model=" + Model.Trim());
                lines.Add("style=" + Style.Trim());
                lines.Add("speedPercent=" + ClampSpeedPercent(SpeedPercent));
                lines.Add("maxTextLength=" + ClampMaxTextLength(MaxTextLength));
                File.WriteAllLines(path, lines.ToArray());
            }
            catch
            {
            }
        }

        public VoiceRequestOptions CreateRequest(string text, string apiKey)
        {
            VoiceRequestOptions request = new VoiceRequestOptions();
            request.ApiKey = apiKey;
            request.VoiceId = VoiceId.Trim();
            request.LocalVoice = NormalizeLocalVoice(LocalVoice);
            request.LocalSteps = ClampLocalSteps(LocalSteps);
            request.Text = text;
            request.Language = NormalizeLanguage(Language);
            request.Model = Model.Trim();
            request.Style = Style.Trim();
            request.SpeedPercent = ClampSpeedPercent(SpeedPercent);
            return request;
        }

        public static string NormalizeEngine(string engine)
        {
            string value = (engine ?? "").Trim().ToLowerInvariant();
            return value == EngineSupertoneApi ? EngineSupertoneApi : EngineSupertonic;
        }

        public static string NormalizeLocalVoice(string voice)
        {
            string value = (voice ?? "").Trim();
            return value.Length == 0 ? "F1" : value;
        }

        public static string FormatHotkey(int modifiers, int key)
        {
            if (key == 0)
                return TextResources.HotkeyNone;

            StringBuilder builder = new StringBuilder();
            if ((modifiers & 2) != 0)
                builder.Append("Ctrl+");
            if ((modifiers & 1) != 0)
                builder.Append("Alt+");
            if ((modifiers & 4) != 0)
                builder.Append("Shift+");
            if ((modifiers & 8) != 0)
                builder.Append("Win+");

            Keys keyCode = (Keys)key;
            string name;
            if (keyCode >= Keys.D0 && keyCode <= Keys.D9)
                name = ((char)('0' + (key - (int)Keys.D0))).ToString();
            else
                name = keyCode.ToString();

            builder.Append(name);
            return builder.ToString();
        }

        public static int ClampSpeedPercent(int value)
        {
            if (value < MinSpeedPercent)
                return MinSpeedPercent;
            if (value > MaxSpeedPercent)
                return MaxSpeedPercent;
            return value;
        }

        public static int ClampLocalSteps(int value)
        {
            if (value < MinLocalSteps)
                return MinLocalSteps;
            if (value > MaxLocalSteps)
                return MaxLocalSteps;
            return value;
        }

        public static int ClampMaxTextLength(int value)
        {
            if (value < 1)
                return 1;
            if (value > MaxAllowedTextLength)
                return MaxAllowedTextLength;
            return value;
        }

        public static bool HasApiKey()
        {
            return LoadApiKey().Length > 0;
        }

        public static string LoadApiKey()
        {
            try
            {
                string path = GetApiKeyPath();
                if (!File.Exists(path))
                    return "";

                byte[] protectedBytes = Convert.FromBase64String(File.ReadAllText(path).Trim());
                byte[] bytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return "";
            }
        }

        public static void SaveApiKey(string apiKey)
        {
            string path = GetApiKeyPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            byte[] bytes = Encoding.UTF8.GetBytes(apiKey);
            byte[] protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            File.WriteAllText(path, Convert.ToBase64String(protectedBytes));
        }

        public static void ClearApiKey()
        {
            try
            {
                string path = GetApiKeyPath();
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
            }
        }

        private static string NormalizeLanguage(string language)
        {
            string value = (language ?? "").Trim().ToLowerInvariant();
            string[] supported = new[] { "en", "ko", "ja", "bg", "cs", "da", "el", "es", "et", "fi", "hu", "it", "nl", "pl", "pt", "ro", "ar", "de", "fr", "hi", "id", "ru", "vi" };
            foreach (string item in supported)
            {
                if (value == item)
                    return value;
            }

            return "ko";
        }

        private static string GetSettingsPath()
        {
            return Path.Combine(GetSettingsDirectory(), "voice.ini");
        }

        private static string GetApiKeyPath()
        {
            return Path.Combine(GetSettingsDirectory(), "supertone.key");
        }

        private static string GetSettingsDirectory()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "HanEnCursorIndicator");
        }
    }

    internal sealed class VoiceRequestOptions
    {
        public string ApiKey;
        public string VoiceId;
        public string LocalVoice;
        public int LocalSteps;
        public string Text;
        public string Language;
        public string Model;
        public string Style;
        public int SpeedPercent;
    }

    internal static class VoiceTextSanitizer
    {
        private static readonly char[] SentenceBreaks = new[] { '.', '?', '!' };

        public static string Sanitize(string rawText, int maxLength)
        {
            if (string.IsNullOrEmpty(rawText))
                return "";

            int limit = VoiceSettings.ClampMaxTextLength(maxLength);
            StringBuilder builder = new StringBuilder();
            bool lastWasSpace = true;
            bool lastWasPunctuation = false;

            foreach (char c in rawText)
            {
                if (IsAllowedTextCharacter(c))
                {
                    builder.Append(c);
                    lastWasSpace = false;
                    lastWasPunctuation = false;
                    continue;
                }

                char punctuation;
                if (TryNormalizePunctuation(c, out punctuation))
                {
                    TrimTrailingSpace(builder);
                    if (builder.Length > 0)
                    {
                        if (!lastWasPunctuation)
                            builder.Append(punctuation);
                        builder.Append(' ');
                    }

                    lastWasSpace = true;
                    lastWasPunctuation = true;
                    continue;
                }

                if (!lastWasSpace && builder.Length > 0)
                {
                    builder.Append(' ');
                    lastWasSpace = true;
                }
            }

            string text = builder.ToString().Trim();
            if (!ContainsReadableCharacter(text))
                return "";

            return TrimToLength(text, limit);
        }

        private static bool IsAllowedTextCharacter(char c)
        {
            if (IsHangulJamo(c))
                return false;

            if (char.IsLetterOrDigit(c))
                return true;

            return false;
        }

        private static bool ContainsReadableCharacter(string text)
        {
            foreach (char c in text)
            {
                if (IsAllowedTextCharacter(c))
                    return true;
            }

            return false;
        }

        private static bool TryNormalizePunctuation(char c, out char punctuation)
        {
            if (c == '.' || c == '?' || c == '!' || c == ',')
            {
                punctuation = c;
                return true;
            }

            if (c == '\u3002')
            {
                punctuation = '.';
                return true;
            }

            if (c == '\uFF1F')
            {
                punctuation = '?';
                return true;
            }

            if (c == '\uFF01')
            {
                punctuation = '!';
                return true;
            }

            if (c == '\u3001' || c == '\uFF0C')
            {
                punctuation = ',';
                return true;
            }

            punctuation = '\0';
            return false;
        }

        private static bool IsHangulJamo(char c)
        {
            return (c >= '\u3130' && c <= '\u318F') || (c >= '\u1100' && c <= '\u11FF');
        }

        private static void TrimTrailingSpace(StringBuilder builder)
        {
            while (builder.Length > 0 && builder[builder.Length - 1] == ' ')
                builder.Length--;
        }

        private static string TrimToLength(string text, int limit)
        {
            if (text.Length <= limit)
                return text;

            int sentenceCut = text.LastIndexOfAny(SentenceBreaks, limit - 1);
            if (sentenceCut > Math.Max(12, limit / 2))
                return text.Substring(0, sentenceCut + 1).Trim();

            int spaceCut = text.LastIndexOf(' ', limit - 1);
            if (spaceCut > Math.Max(12, limit / 2))
                return text.Substring(0, spaceCut).Trim();

            return text.Substring(0, limit).Trim();
        }
    }

    internal static class ClipboardSelectionReader
    {
        // Restoring the previous clipboard too eagerly destroys a copy the user
        // makes right after drag-selecting (their Ctrl+C lands inside our
        // copy/restore window and gets overwritten). So the restore is deferred,
        // and skipped entirely if the clipboard sequence number moved in the
        // meantime — that means someone else (typically the user) wrote to the
        // clipboard and their data must win.
        private const int RestoreDelayMs = 700;

        private static System.Windows.Forms.Timer restoreTimer;
        private static IDataObject pendingRestoreData;
        private static bool pendingHadData;
        private static uint pendingSequence;

        public static string TryCopySelectionText()
        {
            CancelPendingRestore();

            // Clipboard.GetDataObject() hands back a live proxy whose data dies
            // with Clipboard.Clear(), so the contents must be copied into a
            // fresh DataObject to survive until the deferred restore.
            IDataObject previousData = SnapshotClipboard();
            bool hadPreviousData = previousData != null;

            string copied;
            try
            {
                TryClearClipboard();
                SendKeys.SendWait("^c");
                copied = ReadTextWithRetry() ?? "";
            }
            catch
            {
                copied = "";
            }

            ScheduleRestore(previousData, hadPreviousData, NativeMethods.GetClipboardSequenceNumber());
            return copied;
        }

        private static void ScheduleRestore(IDataObject previousData, bool hadPreviousData, uint sequence)
        {
            pendingRestoreData = previousData;
            pendingHadData = hadPreviousData;
            pendingSequence = sequence;

            if (restoreTimer == null)
            {
                restoreTimer = new System.Windows.Forms.Timer();
                restoreTimer.Interval = RestoreDelayMs;
                restoreTimer.Tick += OnRestoreTimerTick;
            }

            restoreTimer.Stop();
            restoreTimer.Start();
        }

        private static void OnRestoreTimerTick(object sender, EventArgs e)
        {
            restoreTimer.Stop();
            IDataObject data = pendingRestoreData;
            bool hadData = pendingHadData;
            uint sequence = pendingSequence;
            pendingRestoreData = null;

            try
            {
                if (NativeMethods.GetClipboardSequenceNumber() != sequence)
                {
                    VoiceDebugLog.Write("clipboard restore skipped: user copied in the meantime");
                    return;
                }

                if (hadData && data != null)
                    Clipboard.SetDataObject(data, true);
            }
            catch
            {
            }
        }

        private static void CancelPendingRestore()
        {
            if (restoreTimer != null)
                restoreTimer.Stop();
            pendingRestoreData = null;
        }

        private static IDataObject SnapshotClipboard()
        {
            try
            {
                IDataObject source = Clipboard.GetDataObject();
                if (source == null)
                    return null;

                string[] formats = source.GetFormats(false);
                if (formats == null || formats.Length == 0)
                    return null;

                DataObject copy = new DataObject();
                bool copiedAny = false;
                foreach (string format in formats)
                {
                    try
                    {
                        object data = source.GetData(format, false);
                        if (data != null)
                        {
                            copy.SetData(format, data);
                            copiedAny = true;
                        }
                    }
                    catch
                    {
                    }
                }

                return copiedAny ? copy : null;
            }
            catch
            {
                return null;
            }
        }

        private static string ReadTextWithRetry()
        {
            for (int i = 0; i < 8; i++)
            {
                Thread.Sleep(35);
                try
                {
                    if (Clipboard.ContainsText())
                        return Clipboard.GetText();
                }
                catch
                {
                }
            }

            return "";
        }

        private static void TryClearClipboard()
        {
            try
            {
                Clipboard.Clear();
            }
            catch
            {
            }
        }

    }

    internal static class SupertoneTtsClient
    {
        private const string EndpointBase = "https://supertoneapi.com/v1/text-to-speech/";

        public static string CreateSpeechFile(VoiceRequestOptions request)
        {
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;

            string url = EndpointBase + Uri.EscapeDataString(request.VoiceId);
            byte[] body = Encoding.UTF8.GetBytes(BuildRequestJson(request));

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.Accept = "audio/wav";
            webRequest.Headers["x-sup-api-key"] = request.ApiKey;
            webRequest.Timeout = 30000;
            webRequest.ReadWriteTimeout = 30000;
            webRequest.ContentLength = body.Length;

            using (Stream requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(body, 0, body.Length);
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                {
                    string path = CreateTempAudioPath();
                    using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        CopyStream(responseStream, fileStream);
                    }

                    return path;
                }
            }
            catch (WebException ex)
            {
                throw new InvalidOperationException(ReadWebException(ex));
            }
        }

        private static string BuildRequestJson(VoiceRequestOptions request)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('{');
            AppendJsonField(builder, "text", request.Text, true);
            AppendJsonField(builder, "language", request.Language, true);

            if (!string.IsNullOrEmpty(request.Style))
                AppendJsonField(builder, "style", request.Style, true);

            if (!string.IsNullOrEmpty(request.Model))
                AppendJsonField(builder, "model", request.Model, true);

            AppendJsonField(builder, "output_format", "wav", true);

            double speed = request.SpeedPercent / 100.0d;
            builder.Append("\"voice_settings\":{\"speed\":");
            builder.Append(speed.ToString("0.###", CultureInfo.InvariantCulture));
            builder.Append("},\"include_phonemes\":false}");
            return builder.ToString();
        }

        private static void AppendJsonField(StringBuilder builder, string name, string value, bool appendComma)
        {
            builder.Append('"');
            builder.Append(name);
            builder.Append("\":\"");
            builder.Append(EscapeJson(value ?? ""));
            builder.Append('"');
            if (appendComma)
                builder.Append(',');
        }

        private static string EscapeJson(string value)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in value)
            {
                if (c == '\\' || c == '"')
                {
                    builder.Append('\\');
                    builder.Append(c);
                }
                else if (c == '\r')
                {
                    builder.Append("\\r");
                }
                else if (c == '\n')
                {
                    builder.Append("\\n");
                }
                else if (c == '\t')
                {
                    builder.Append("\\t");
                }
                else if (char.IsControl(c))
                {
                    builder.Append("\\u");
                    builder.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        private static string CreateTempAudioPath()
        {
            string directory = Path.Combine(Path.GetTempPath(), "HanEnCursorIndicator");
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, "supertone-" + DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture) + ".wav");
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[81920];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                output.Write(buffer, 0, read);
        }

        private static string ReadWebException(WebException ex)
        {
            HttpWebResponse response = ex.Response as HttpWebResponse;
            string status = response == null ? ex.Message : ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture) + " " + response.StatusDescription;
            string detail = "";

            try
            {
                if (response != null)
                {
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        detail = reader.ReadToEnd();
                    }
                }
            }
            catch
            {
            }

            if (detail.Length > 180)
                detail = detail.Substring(0, 180);

            return string.IsNullOrEmpty(detail) ? status : status + " / " + detail;
        }
    }

    internal static class VoiceDebugLog
    {
        private static readonly object Sync = new object();

        public static void Write(string message)
        {
            try
            {
                lock (Sync)
                {
                    string directory = Path.Combine(Path.GetTempPath(), "HanEnCursorIndicator");
                    Directory.CreateDirectory(directory);
                    File.AppendAllText(
                        Path.Combine(directory, "voice-debug.log"),
                        DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + " " + message + "\r\n");
                }
            }
            catch
            {
            }
        }
    }

    internal static class SupertonicLocalClient
    {
        private const int Port = 7788;
        private const string BaseUrl = "http://127.0.0.1:7788";

        private static readonly object StartLock = new object();
        private static Process startedProcess;

        public static void WarmUp()
        {
            EnsureServerReady(180000);
        }

        public static void StopServerIfStarted()
        {
            lock (StartLock)
            {
                try
                {
                    if (startedProcess != null && !startedProcess.HasExited)
                        startedProcess.Kill();
                }
                catch
                {
                }

                startedProcess = null;
            }
        }

        public static string CreateSpeechFile(VoiceRequestOptions request)
        {
            EnsureServerReady(180000);

            byte[] body = Encoding.UTF8.GetBytes(BuildRequestJson(request));

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(BaseUrl + "/v1/tts");
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.Accept = "audio/wav";
            webRequest.Timeout = 60000;
            webRequest.ReadWriteTimeout = 60000;
            webRequest.ContentLength = body.Length;

            using (Stream requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(body, 0, body.Length);
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                {
                    string path = CreateTempAudioPath();
                    using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        byte[] buffer = new byte[81920];
                        int read;
                        while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            fileStream.Write(buffer, 0, read);
                    }

                    return path;
                }
            }
            catch (WebException ex)
            {
                throw new InvalidOperationException(ReadWebException(ex));
            }
        }

        private static void EnsureServerReady(int timeoutMs)
        {
            if (CheckHealth(2000))
                return;

            lock (StartLock)
            {
                if (CheckHealth(2000))
                    return;

                if (startedProcess == null || startedProcess.HasExited)
                    startedProcess = StartServer();

                DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
                while (DateTime.UtcNow < deadline)
                {
                    Thread.Sleep(1000);
                    if (CheckHealth(2000))
                        return;

                    if (startedProcess != null && startedProcess.HasExited)
                        break;
                }

                throw new InvalidOperationException(TextResources.VoiceLocalNotReady);
            }
        }

        private static bool CheckHealth(int timeoutMs)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(BaseUrl + "/v1/health");
                webRequest.Method = "GET";
                webRequest.Timeout = timeoutMs;
                webRequest.ReadWriteTimeout = timeoutMs;

                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    string text = reader.ReadToEnd();
                    return text.IndexOf("\"ok\"", StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static Process StartServer()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] candidates = new[]
            {
                Path.Combine(userProfile, "anaconda3\\Scripts\\supertonic.exe"),
                Path.Combine(userProfile, "miniconda3\\Scripts\\supertonic.exe"),
                "supertonic.exe"
            };

            foreach (string candidate in candidates)
            {
                bool isPathLookup = candidate.IndexOf('\\') < 0;
                if (!isPathLookup && !File.Exists(candidate))
                    continue;

                try
                {
                    ProcessStartInfo info = new ProcessStartInfo();
                    info.FileName = candidate;
                    info.Arguments = "serve --host 127.0.0.1 --port " + Port.ToString(CultureInfo.InvariantCulture) + " --log-level warning";
                    info.UseShellExecute = false;
                    info.CreateNoWindow = true;
                    return Process.Start(info);
                }
                catch
                {
                }
            }

            throw new InvalidOperationException(TextResources.VoiceLocalMissing);
        }

        private static string BuildRequestJson(VoiceRequestOptions request)
        {
            double speed = request.SpeedPercent / 100.0d;
            if (speed < 0.7d)
                speed = 0.7d;
            if (speed > 2.0d)
                speed = 2.0d;

            StringBuilder builder = new StringBuilder();
            builder.Append("{\"text\":\"");
            builder.Append(EscapeJson(request.Text ?? ""));
            builder.Append("\",\"voice\":\"");
            builder.Append(EscapeJson(string.IsNullOrEmpty(request.LocalVoice) ? "F1" : request.LocalVoice));
            builder.Append("\",\"lang\":\"");
            builder.Append(EscapeJson(request.Language ?? "ko"));
            builder.Append("\",\"speed\":");
            builder.Append(speed.ToString("0.###", CultureInfo.InvariantCulture));
            builder.Append(",\"steps\":");
            builder.Append(VoiceSettings.ClampLocalSteps(request.LocalSteps == 0 ? 8 : request.LocalSteps).ToString(CultureInfo.InvariantCulture));
            builder.Append(",\"response_format\":\"wav\"}");
            return builder.ToString();
        }

        private static string EscapeJson(string value)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in value)
            {
                if (c == '\\' || c == '"')
                {
                    builder.Append('\\');
                    builder.Append(c);
                }
                else if (c == '\r')
                {
                    builder.Append("\\r");
                }
                else if (c == '\n')
                {
                    builder.Append("\\n");
                }
                else if (c == '\t')
                {
                    builder.Append("\\t");
                }
                else if (char.IsControl(c))
                {
                    builder.Append("\\u");
                    builder.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        private static string CreateTempAudioPath()
        {
            string directory = Path.Combine(Path.GetTempPath(), "HanEnCursorIndicator");
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, "supertonic-local-" + DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture) + ".wav");
        }

        private static string ReadWebException(WebException ex)
        {
            HttpWebResponse response = ex.Response as HttpWebResponse;
            string status = response == null ? ex.Message : ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture) + " " + response.StatusDescription;
            string detail = "";

            try
            {
                if (response != null)
                {
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        detail = reader.ReadToEnd();
                    }
                }
            }
            catch
            {
            }

            if (detail.Length > 180)
                detail = detail.Substring(0, 180);

            return string.IsNullOrEmpty(detail) ? status : status + " / " + detail;
        }
    }

    internal sealed class HotkeyWindow : NativeWindow, IDisposable
    {
        private readonly Dictionary<int, Action> actions = new Dictionary<int, Action>();

        public HotkeyWindow()
        {
            CreateHandle(new CreateParams());
        }

        public bool Register(int id, uint modifiers, uint vk, Action action)
        {
            Unregister(id);
            if (vk == 0 || action == null)
                return false;

            bool registered = NativeMethods.RegisterHotKey(Handle, id, modifiers, vk);
            if (registered)
                actions[id] = action;
            return registered;
        }

        public void Unregister(int id)
        {
            if (!actions.ContainsKey(id))
                return;

            try
            {
                NativeMethods.UnregisterHotKey(Handle, id);
            }
            catch
            {
            }

            actions.Remove(id);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_HOTKEY)
            {
                Action action;
                if (actions.TryGetValue((int)m.WParam, out action) && action != null)
                    action();
            }

            base.WndProc(ref m);
        }

        public void Dispose()
        {
            foreach (int id in new List<int>(actions.Keys))
                Unregister(id);
            DestroyHandle();
        }
    }

    internal static class VoiceAudioPlayer
    {
        private static readonly object Sync = new object();
        private static System.Media.SoundPlayer current;

        public static void PlayWavAndDelete(string path)
        {
            try
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(path);
                lock (Sync)
                {
                    current = player;
                }

                try
                {
                    player.Load();
                    player.PlaySync();
                }
                finally
                {
                    lock (Sync)
                    {
                        if (current == player)
                            current = null;
                    }
                    player.Dispose();
                }
            }
            finally
            {
                try
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
                catch
                {
                }
            }
        }

        public static bool StopCurrent()
        {
            System.Media.SoundPlayer player;
            lock (Sync)
            {
                player = current;
            }

            if (player == null)
                return false;

            try
            {
                player.Stop();
            }
            catch
            {
            }

            VoiceDebugLog.Write("playback stop requested");
            return true;
        }
    }

    internal enum LicenseState
    {
        Missing,
        Active,
        OfflineActive,
        Invalid
    }

    internal sealed class LicenseStatus
    {
        public LicenseState State = LicenseState.Missing;
        public string Message = "";
        public string Detail = "";
    }

    internal sealed class LicenseTokenRecord
    {
        public string Token = "";
        public DateTime LastValidatedUtc = DateTime.MinValue;
        public DateTime OfflineUntilUtc = DateTime.MinValue;
    }

    internal sealed class LicenseSettings
    {
        private const string DefaultApiBaseUrl = "https://hanen-cursor-indicator.vercel.app";
        public string ApiBaseUrl = DefaultApiBaseUrl;

        public static LicenseSettings Load()
        {
            LicenseSettings settings = new LicenseSettings();
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

                    if (parts[0].Trim().Equals("apiBaseUrl", StringComparison.OrdinalIgnoreCase))
                        settings.ApiBaseUrl = NormalizeApiBaseUrl(parts[1].Trim());
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
                File.WriteAllLines(path, new[] { "apiBaseUrl=" + NormalizeApiBaseUrl(ApiBaseUrl) });
            }
            catch
            {
            }
        }

        public static string NormalizeApiBaseUrl(string value)
        {
            string text = (value ?? "").Trim();
            if (text.Length == 0)
                text = DefaultApiBaseUrl;

            while (text.EndsWith("/", StringComparison.Ordinal))
                text = text.Substring(0, text.Length - 1);

            return text;
        }

        public static string LoadLicenseKey()
        {
            return ReadProtectedText(GetLicenseKeyPath());
        }

        public static void SaveLicenseKey(string licenseKey)
        {
            WriteProtectedText(GetLicenseKeyPath(), licenseKey.Trim());
        }

        public static void ClearLicenseKey()
        {
            DeleteIfExists(GetLicenseKeyPath());
        }

        public static LicenseTokenRecord LoadTokenRecord()
        {
            LicenseTokenRecord record = new LicenseTokenRecord();
            string text = ReadProtectedText(GetTokenPath());
            if (text.Length == 0)
                return record;

            string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] parts = line.Split(new[] { '=' }, 2);
                if (parts.Length != 2)
                    continue;

                string key = parts[0].Trim();
                string value = parts[1].Trim();
                if (key.Equals("token", StringComparison.OrdinalIgnoreCase))
                    record.Token = value;
                else if (key.Equals("lastValidatedUtc", StringComparison.OrdinalIgnoreCase))
                    record.LastValidatedUtc = ParseUtc(value);
                else if (key.Equals("offlineUntilUtc", StringComparison.OrdinalIgnoreCase))
                    record.OfflineUntilUtc = ParseUtc(value);
            }

            return record;
        }

        public static void SaveTokenRecord(LicenseTokenRecord record)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("token=").Append(record.Token ?? "").Append('\n');
            builder.Append("lastValidatedUtc=").Append(FormatUtc(record.LastValidatedUtc)).Append('\n');
            builder.Append("offlineUntilUtc=").Append(FormatUtc(record.OfflineUntilUtc)).Append('\n');
            WriteProtectedText(GetTokenPath(), builder.ToString());
        }

        public static void ClearTokenRecord()
        {
            DeleteIfExists(GetTokenPath());
        }

        public static string MaskLicenseKey(string licenseKey)
        {
            string value = (licenseKey ?? "").Trim();
            if (value.Length <= 8)
                return value.Length == 0 ? "" : "****";

            return value.Substring(0, 4) + "..." + value.Substring(value.Length - 4);
        }

        private static string ReadProtectedText(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return "";

                byte[] protectedBytes = Convert.FromBase64String(File.ReadAllText(path).Trim());
                byte[] bytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return "";
            }
        }

        private static void WriteProtectedText(string path, string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            byte[] bytes = Encoding.UTF8.GetBytes(text ?? "");
            byte[] protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            File.WriteAllText(path, Convert.ToBase64String(protectedBytes));
        }

        private static void DeleteIfExists(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
            }
        }

        private static DateTime ParseUtc(string text)
        {
            DateTime value;
            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out value))
                return value.ToUniversalTime();

            return DateTime.MinValue;
        }

        private static string FormatUtc(DateTime value)
        {
            if (value == DateTime.MinValue)
                return "";

            return value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
        }

        private static string GetSettingsPath()
        {
            return Path.Combine(GetSettingsDirectory(), "license.ini");
        }

        private static string GetLicenseKeyPath()
        {
            return Path.Combine(GetSettingsDirectory(), "license.key");
        }

        private static string GetTokenPath()
        {
            return Path.Combine(GetSettingsDirectory(), "license.token");
        }

        private static string GetSettingsDirectory()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "HanEnCursorIndicator");
        }
    }

    internal sealed class LicenseManager
    {
        private readonly LicenseSettings settings;

        public LicenseManager(LicenseSettings settings)
        {
            this.settings = settings;
        }

        public LicenseStatus Activate(string apiBaseUrl, string licenseKey)
        {
            string normalizedUrl = LicenseSettings.NormalizeApiBaseUrl(apiBaseUrl);
            string normalizedKey = (licenseKey ?? "").Trim();
            if (normalizedKey.Length == 0)
                return Invalid(TextResources.LicenseMissing);

            settings.ApiBaseUrl = normalizedUrl;
            settings.Save();

            LicenseApiResult result = PostJson(normalizedUrl + "/api/license/activate", BuildCommonBody(normalizedKey, ""));
            if (!result.Ok)
                return Invalid(result.Message);

            SaveSuccessfulLicense(normalizedKey, result);
            LicenseStatus status = new LicenseStatus();
            status.State = LicenseState.Active;
            status.Message = TextResources.LicenseActivated;
            status.Detail = CreateDetail(normalizedKey, result.OfflineUntilUtc);
            return status;
        }

        public LicenseStatus GetStatus(bool validateOnline)
        {
            string licenseKey = LicenseSettings.LoadLicenseKey();
            if (licenseKey.Length == 0)
                return Missing();

            LicenseTokenRecord record = LicenseSettings.LoadTokenRecord();
            if (validateOnline)
            {
                try
                {
                    LicenseApiResult result = PostJson(settings.ApiBaseUrl + "/api/license/validate", BuildCommonBody(licenseKey, record.Token));
                    if (result.Ok)
                    {
                        SaveSuccessfulLicense(licenseKey, result);
                        LicenseStatus active = new LicenseStatus();
                        active.State = LicenseState.Active;
                        active.Detail = CreateDetail(licenseKey, result.OfflineUntilUtc);
                        return active;
                    }
                }
                catch
                {
                }
            }

            if (record.Token.Length > 0 && DateTime.UtcNow <= record.OfflineUntilUtc)
            {
                LicenseStatus offline = new LicenseStatus();
                offline.State = LicenseState.OfflineActive;
                offline.Detail = CreateDetail(licenseKey, record.OfflineUntilUtc);
                return offline;
            }

            return Invalid("Online validation required.");
        }

        public LicenseStatus Deactivate()
        {
            string licenseKey = LicenseSettings.LoadLicenseKey();
            LicenseTokenRecord record = LicenseSettings.LoadTokenRecord();

            if (licenseKey.Length == 0)
                return Missing();

            try
            {
                PostJson(settings.ApiBaseUrl + "/api/license/deactivate", BuildCommonBody(licenseKey, record.Token));
            }
            catch
            {
            }

            LicenseSettings.ClearLicenseKey();
            LicenseSettings.ClearTokenRecord();

            LicenseStatus status = new LicenseStatus();
            status.State = LicenseState.Missing;
            status.Message = TextResources.LicenseDeactivated;
            return status;
        }

        private void SaveSuccessfulLicense(string licenseKey, LicenseApiResult result)
        {
            LicenseSettings.SaveLicenseKey(licenseKey);

            LicenseTokenRecord record = new LicenseTokenRecord();
            record.Token = result.Token;
            record.LastValidatedUtc = DateTime.UtcNow;
            record.OfflineUntilUtc = result.OfflineUntilUtc == DateTime.MinValue ? DateTime.UtcNow.AddDays(14) : result.OfflineUntilUtc;
            LicenseSettings.SaveTokenRecord(record);
        }

        private string BuildCommonBody(string licenseKey, string token)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('{');
            AppendJsonField(builder, "licenseKey", licenseKey, true);
            AppendJsonField(builder, "machineHash", MachineIdentity.GetMachineHash(), true);
            AppendJsonField(builder, "appVersion", Application.ProductVersion, true);
            AppendJsonField(builder, "token", token ?? "", false);
            builder.Append('}');
            return builder.ToString();
        }

        private static LicenseStatus Missing()
        {
            LicenseStatus status = new LicenseStatus();
            status.State = LicenseState.Missing;
            status.Message = TextResources.LicenseMissing;
            return status;
        }

        private static LicenseStatus Invalid(string message)
        {
            LicenseStatus status = new LicenseStatus();
            status.State = LicenseState.Invalid;
            status.Message = message ?? "";
            return status;
        }

        private static string CreateDetail(string licenseKey, DateTime offlineUntilUtc)
        {
            string detail = LicenseSettings.MaskLicenseKey(licenseKey);
            if (offlineUntilUtc != DateTime.MinValue)
                detail += ", offline until " + offlineUntilUtc.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            return detail;
        }

        private static LicenseApiResult PostJson(string url, string json)
        {
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
            byte[] body = Encoding.UTF8.GetBytes(json);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.Accept = "application/json";
            request.Timeout = 15000;
            request.ReadWriteTimeout = 15000;
            request.ContentLength = body.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(body, 0, body.Length);
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    return LicenseApiResult.Parse(reader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                string message = ReadWebException(ex);
                LicenseApiResult parsed = LicenseApiResult.Parse(message);
                if (parsed.Message.Length > 0)
                    return parsed;

                LicenseApiResult result = new LicenseApiResult();
                result.Ok = false;
                result.Message = message;
                return result;
            }
        }

        private static string ReadWebException(WebException ex)
        {
            try
            {
                if (ex.Response != null)
                {
                    using (Stream stream = ex.Response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        return reader.ReadToEnd();
                }
            }
            catch
            {
            }

            return ex.Message;
        }

        private static void AppendJsonField(StringBuilder builder, string name, string value, bool appendComma)
        {
            builder.Append('"').Append(name).Append("\":\"");
            builder.Append(EscapeJson(value ?? ""));
            builder.Append('"');
            if (appendComma)
                builder.Append(',');
        }

        private static string EscapeJson(string value)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in value)
            {
                if (c == '\\' || c == '"')
                    builder.Append('\\').Append(c);
                else if (c == '\r')
                    builder.Append("\\r");
                else if (c == '\n')
                    builder.Append("\\n");
                else if (c == '\t')
                    builder.Append("\\t");
                else if (char.IsControl(c))
                    builder.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                else
                    builder.Append(c);
            }

            return builder.ToString();
        }
    }

    internal sealed class LicenseApiResult
    {
        public bool Ok;
        public string Message = "";
        public string Token = "";
        public DateTime OfflineUntilUtc = DateTime.MinValue;

        public static LicenseApiResult Parse(string json)
        {
            LicenseApiResult result = new LicenseApiResult();
            string text = json ?? "";
            string ok = JsonValueReader.GetRawValue(text, "ok");
            result.Ok = ok.Equals("true", StringComparison.OrdinalIgnoreCase);
            if (!result.Ok)
            {
                string success = JsonValueReader.GetRawValue(text, "success");
                result.Ok = success.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            result.Message = JsonValueReader.GetString(text, "message");
            if (result.Message.Length == 0)
                result.Message = JsonValueReader.GetString(text, "error");

            result.Token = JsonValueReader.GetString(text, "token");
            string offlineUntil = JsonValueReader.GetString(text, "offlineUntil");
            if (offlineUntil.Length == 0)
                offlineUntil = JsonValueReader.GetString(text, "offlineUntilUtc");

            DateTime parsed;
            if (DateTime.TryParse(offlineUntil, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out parsed))
                result.OfflineUntilUtc = parsed.ToUniversalTime();

            if (result.Ok && result.Token.Length == 0)
                result.Ok = false;
            if (!result.Ok && result.Message.Length == 0 && text.Length > 0 && text.Length < 240)
                result.Message = text;

            return result;
        }
    }

    internal static class JsonValueReader
    {
        public static string GetString(string json, string name)
        {
            int index = FindProperty(json, name);
            if (index < 0)
                return "";

            int colon = json.IndexOf(':', index);
            if (colon < 0)
                return "";

            int quote = FindNextNonWhite(json, colon + 1);
            if (quote < 0 || json[quote] != '"')
                return "";

            StringBuilder builder = new StringBuilder();
            bool escape = false;
            for (int i = quote + 1; i < json.Length; i++)
            {
                char c = json[i];
                if (escape)
                {
                    if (c == 'n')
                        builder.Append('\n');
                    else if (c == 'r')
                        builder.Append('\r');
                    else if (c == 't')
                        builder.Append('\t');
                    else
                        builder.Append(c);
                    escape = false;
                    continue;
                }

                if (c == '\\')
                {
                    escape = true;
                    continue;
                }

                if (c == '"')
                    break;

                builder.Append(c);
            }

            return builder.ToString();
        }

        public static string GetRawValue(string json, string name)
        {
            int index = FindProperty(json, name);
            if (index < 0)
                return "";

            int colon = json.IndexOf(':', index);
            if (colon < 0)
                return "";

            int start = FindNextNonWhite(json, colon + 1);
            if (start < 0)
                return "";

            int end = start;
            while (end < json.Length && json[end] != ',' && json[end] != '}' && !char.IsWhiteSpace(json[end]))
                end++;

            return json.Substring(start, end - start).Trim().Trim('"');
        }

        private static int FindProperty(string json, string name)
        {
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(name))
                return -1;

            string needle = "\"" + name + "\"";
            return json.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
        }

        private static int FindNextNonWhite(string text, int start)
        {
            for (int i = start; i < text.Length; i++)
            {
                if (!char.IsWhiteSpace(text[i]))
                    return i;
            }

            return -1;
        }
    }

    internal static class MachineIdentity
    {
        public static string GetMachineHash()
        {
            string source = ReadMachineGuid();
            if (source.Length == 0)
                source = Environment.MachineName + "|" + Environment.UserName;

            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(source));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hash)
                    builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));

                return builder.ToString();
            }
        }

        private static string ReadMachineGuid()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    if (key == null)
                        return "";

                    object value = key.GetValue("MachineGuid");
                    return value == null ? "" : value.ToString();
                }
            }
            catch
            {
                return "";
            }
        }
    }

    internal sealed class SelectionDragWatcher : IDisposable
    {
        private const int DragThreshold = 12;
        private readonly SynchronizationContext context;
        private readonly Action onDragCompleted;
        private NativeMethods.HookProc hookProc;
        private IntPtr hookHandle = IntPtr.Zero;
        private Thread hookThread;
        private uint hookThreadId;
        private volatile bool stopRequested;
        private Point mouseDownPoint;
        private DateTime mouseDownUtc = DateTime.MinValue;

        public SelectionDragWatcher(SynchronizationContext context, Action onDragCompleted)
        {
            this.context = context;
            this.onDragCompleted = onDragCompleted;
        }

        public void Start()
        {
            if (hookThread != null && hookThread.IsAlive)
                return;

            stopRequested = false;
            hookThread = new Thread(HookThreadMain);
            hookThread.IsBackground = true;
            hookThread.Name = "SelectionDragHook";
            hookThread.Start();
        }

        public void Stop()
        {
            stopRequested = true;
            uint threadId = hookThreadId;
            if (threadId != 0)
                NativeMethods.PostThreadMessage(threadId, NativeMethods.WM_QUIT, IntPtr.Zero, IntPtr.Zero);

            hookThread = null;
        }

        public void Dispose()
        {
            Stop();
        }

        private void HookThreadMain()
        {
            // The hook lives on its own thread whose only job is pumping this
            // loop, so callbacks always return within the LowLevelHooksTimeout
            // budget; a stalled UI thread can no longer get the hook silently
            // unhooked by Windows.
            hookThreadId = NativeMethods.GetCurrentThreadId();
            hookProc = HookCallback;
            hookHandle = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, hookProc, NativeMethods.GetModuleHandle(null), 0);
            VoiceDebugLog.Write("hook installed on dedicated thread; handle=" + hookHandle);

            if (!stopRequested)
            {
                NativeMethods.NativeMessage msg;
                while (NativeMethods.GetMessage(out msg, IntPtr.Zero, 0, 0) > 0)
                {
                    NativeMethods.TranslateMessage(ref msg);
                    NativeMethods.DispatchMessage(ref msg);
                }
            }

            if (hookHandle != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(hookHandle);
                hookHandle = IntPtr.Zero;
            }

            hookThreadId = 0;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int message = wParam.ToInt32();
                NativeMethods.MouseHookStruct info = (NativeMethods.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(NativeMethods.MouseHookStruct));

                if (message == NativeMethods.WM_LBUTTONDOWN)
                {
                    mouseDownPoint = new Point(info.pt.X, info.pt.Y);
                    mouseDownUtc = DateTime.UtcNow;
                }
                else if (message == NativeMethods.WM_LBUTTONUP)
                {
                    Point upPoint = new Point(info.pt.X, info.pt.Y);
                    double distance = Math.Sqrt(Math.Pow(upPoint.X - mouseDownPoint.X, 2) + Math.Pow(upPoint.Y - mouseDownPoint.Y, 2));
                    double elapsed = (DateTime.UtcNow - mouseDownUtc).TotalMilliseconds;
                    if (distance >= DragThreshold && elapsed >= 80 && elapsed <= 12000)
                        RaiseDragCompleted((int)distance, (int)elapsed);
                }
            }

            return NativeMethods.CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        private void RaiseDragCompleted(int distance, int elapsed)
        {
            if (onDragCompleted == null)
                return;

            if (context != null)
            {
                context.Post(delegate
                {
                    VoiceDebugLog.Write("drag detected; dist=" + distance + " elapsed=" + elapsed);
                    onDragCompleted();
                }, null);
            }
            else
            {
                onDragCompleted();
            }
        }
    }

    internal sealed class AppSettings
    {
        public const int MinSizePercent = 50;
        public const int MaxSizePercent = 250;
        private const int DefaultSizePercent = 100;
        private const float MinFaceCenter = 0.0f;
        private const float MaxFaceCenter = 1.0f;

        public int SizePercent = DefaultSizePercent;
        public bool ShowLabel = true;
        public CursorDisplayMode DisplayMode = CursorDisplayMode.AlwaysFollow;
        public PointF IdleFaceCenter = GetDefaultFaceCenter(IndicatorPose.Idle);
        public PointF PointFaceCenter = GetDefaultFaceCenter(IndicatorPose.Point);
        public PointF CheerFaceCenter = GetDefaultFaceCenter(IndicatorPose.Cheer);
        public bool UseLanguageColors = false;
        public Color BaseMascotColor = Color.FromArgb(238, 224, 198);
        public Color KoreanMascotColor = Color.FromArgb(80, 190, 145);
        public Color EnglishLowerMascotColor = Color.FromArgb(90, 135, 220);
        public Color EnglishUpperMascotColor = Color.FromArgb(120, 100, 220);
        public Color KoreanLabelColor = Color.FromArgb(24, 128, 91);
        public Color EnglishLowerLabelColor = Color.FromArgb(38, 78, 140);
        public Color EnglishUpperLabelColor = Color.FromArgb(21, 70, 160);
        private readonly Dictionary<string, PointF> labelCenters = new Dictionary<string, PointF>(StringComparer.Ordinal);

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

                    string key = parts[0].Trim();
                    string valueText = parts[1].Trim();
                    if (TryLoadLabelCenter(settings, key, valueText))
                        continue;

                    if (key.Equals("sizePercent", StringComparison.OrdinalIgnoreCase))
                    {
                        int value;
                        if (int.TryParse(valueText, out value))
                            settings.SizePercent = ClampSizePercent(value);
                    }
                    else if (key.Equals("showLabel", StringComparison.OrdinalIgnoreCase))
                    {
                        bool value;
                        if (bool.TryParse(valueText, out value))
                            settings.ShowLabel = value;
                    }
                    else if (key.Equals("displayMode", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.DisplayMode = ParseDisplayMode(valueText);
                    }
                    else if (key.Equals("idleFace", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.IdleFaceCenter = ParseFaceCenter(valueText, GetDefaultFaceCenter(IndicatorPose.Idle));
                    }
                    else if (key.Equals("pointFace", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.PointFaceCenter = ParseFaceCenter(valueText, GetDefaultFaceCenter(IndicatorPose.Point));
                    }
                    else if (key.Equals("cheerFace", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.CheerFaceCenter = ParseFaceCenter(valueText, GetDefaultFaceCenter(IndicatorPose.Cheer));
                    }
                    else if (key.Equals("useLanguageColors", StringComparison.OrdinalIgnoreCase))
                    {
                        bool value;
                        if (bool.TryParse(valueText, out value))
                            settings.UseLanguageColors = value;
                    }
                    else if (key.Equals("baseMascotColor", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.BaseMascotColor = ParseColor(valueText, settings.BaseMascotColor);
                    }
                    else if (key.Equals("koreanMascotColor", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.KoreanMascotColor = ParseColor(valueText, settings.KoreanMascotColor);
                    }
                    else if (key.Equals("englishMascotColor", StringComparison.OrdinalIgnoreCase))
                    {
                        Color legacyColor = ParseColor(valueText, settings.EnglishLowerMascotColor);
                        settings.EnglishLowerMascotColor = legacyColor;
                        settings.EnglishUpperMascotColor = legacyColor;
                    }
                    else if (key.Equals("englishLowerMascotColor", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.EnglishLowerMascotColor = ParseColor(valueText, settings.EnglishLowerMascotColor);
                    }
                    else if (key.Equals("englishUpperMascotColor", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.EnglishUpperMascotColor = ParseColor(valueText, settings.EnglishUpperMascotColor);
                    }
                    else if (key.Equals("koreanLabelColor", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.KoreanLabelColor = ParseColor(valueText, settings.KoreanLabelColor);
                    }
                    else if (key.Equals("englishLabelColor", StringComparison.OrdinalIgnoreCase))
                    {
                        Color legacyColor = ParseColor(valueText, settings.EnglishLowerLabelColor);
                        settings.EnglishLowerLabelColor = legacyColor;
                        settings.EnglishUpperLabelColor = legacyColor;
                    }
                    else if (key.Equals("englishLowerLabelColor", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.EnglishLowerLabelColor = ParseColor(valueText, settings.EnglishLowerLabelColor);
                    }
                    else if (key.Equals("englishUpperLabelColor", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.EnglishUpperLabelColor = ParseColor(valueText, settings.EnglishUpperLabelColor);
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
                List<string> lines = new List<string>();
                lines.Add("sizePercent=" + ClampSizePercent(SizePercent));
                lines.Add("showLabel=" + ShowLabel);
                lines.Add("displayMode=" + FormatDisplayMode(DisplayMode));
                lines.Add("idleFace=" + FormatFaceCenter(IdleFaceCenter));
                lines.Add("pointFace=" + FormatFaceCenter(PointFaceCenter));
                lines.Add("cheerFace=" + FormatFaceCenter(CheerFaceCenter));
                foreach (string stateKey in IndicatorStates.All)
                {
                    foreach (IndicatorPose pose in IndicatorPoseHelper.All)
                    {
                        lines.Add("label." + stateKey + "." + IndicatorPoseHelper.GetKey(pose) + "=" + FormatFaceCenter(GetLabelCenterByState(stateKey, pose)));
                    }
                }
                lines.Add("useLanguageColors=" + UseLanguageColors);
                lines.Add("baseMascotColor=" + FormatColor(BaseMascotColor));
                lines.Add("koreanMascotColor=" + FormatColor(KoreanMascotColor));
                lines.Add("englishLowerMascotColor=" + FormatColor(EnglishLowerMascotColor));
                lines.Add("englishUpperMascotColor=" + FormatColor(EnglishUpperMascotColor));
                lines.Add("koreanLabelColor=" + FormatColor(KoreanLabelColor));
                lines.Add("englishLowerLabelColor=" + FormatColor(EnglishLowerLabelColor));
                lines.Add("englishUpperLabelColor=" + FormatColor(EnglishUpperLabelColor));
                File.WriteAllLines(path, lines.ToArray());
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

        private static CursorDisplayMode ParseDisplayMode(string value)
        {
            string text = (value ?? "").Trim();
            if (text.Equals("showWhenIdle", StringComparison.OrdinalIgnoreCase) ||
                text.Equals("idle", StringComparison.OrdinalIgnoreCase))
                return CursorDisplayMode.ShowWhenIdle;

            return CursorDisplayMode.AlwaysFollow;
        }

        private static string FormatDisplayMode(CursorDisplayMode mode)
        {
            return mode == CursorDisplayMode.ShowWhenIdle ? "showWhenIdle" : "alwaysFollow";
        }

        public PointF GetFaceCenter(IndicatorPose pose)
        {
            if (pose == IndicatorPose.Point)
                return PointFaceCenter;
            if (pose == IndicatorPose.Cheer)
                return CheerFaceCenter;
            return IdleFaceCenter;
        }

        public void SetFaceCenter(IndicatorPose pose, PointF center)
        {
            PointF clamped = ClampFaceCenter(center);
            if (pose == IndicatorPose.Point)
                PointFaceCenter = clamped;
            else if (pose == IndicatorPose.Cheer)
                CheerFaceCenter = clamped;
            else
                IdleFaceCenter = clamped;
        }

        public PointF GetLabelCenter(string label, IndicatorPose pose)
        {
            return GetLabelCenterByState(IndicatorStates.FromLabel(label), pose);
        }

        public PointF GetLabelCenterByState(string stateKey, IndicatorPose pose)
        {
            PointF center;
            if (labelCenters.TryGetValue(MakeLabelCenterKey(stateKey, pose), out center))
                return center;

            return GetFaceCenter(pose);
        }

        public void SetLabelCenter(string stateKey, IndicatorPose pose, PointF center)
        {
            if (!IndicatorStates.IsValidKey(stateKey))
                stateKey = IndicatorStates.Korean;

            labelCenters[MakeLabelCenterKey(stateKey, pose)] = ClampFaceCenter(center);
        }

        public static PointF GetDefaultFaceCenter(IndicatorPose pose)
        {
            if (pose == IndicatorPose.Point)
                return new PointF(0.543f, 0.37f);
            if (pose == IndicatorPose.Cheer)
                return new PointF(0.505f, 0.37f);
            return new PointF(0.5f, 0.37f);
        }

        public static PointF ClampFaceCenter(PointF center)
        {
            return new PointF(ClampFloat(center.X, MinFaceCenter, MaxFaceCenter), ClampFloat(center.Y, MinFaceCenter, MaxFaceCenter));
        }

        public Color GetMascotColor(string label)
        {
            if (!UseLanguageColors)
                return BaseMascotColor;

            if (label == Labels.Korean)
                return KoreanMascotColor;

            if (label == Labels.EnglishUpper)
                return EnglishUpperMascotColor;

            return EnglishLowerMascotColor;
        }

        public Color GetLabelColor(string label)
        {
            if (label == Labels.Korean)
                return KoreanLabelColor;

            if (label == Labels.EnglishUpper)
                return EnglishUpperLabelColor;

            return EnglishLowerLabelColor;
        }

        private static float ClampFloat(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        private static string FormatFaceCenter(PointF center)
        {
            PointF clamped = ClampFaceCenter(center);
            return clamped.X.ToString("0.###", CultureInfo.InvariantCulture) + "," + clamped.Y.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static bool TryLoadLabelCenter(AppSettings settings, string key, string value)
        {
            if (!key.StartsWith("label.", StringComparison.Ordinal))
                return false;

            string[] keyParts = key.Split('.');
            if (keyParts.Length != 3)
                return true;

            string stateKey = keyParts[1];
            IndicatorPose pose;
            if (!IndicatorStates.IsValidKey(stateKey) || !IndicatorPoseHelper.TryParseKey(keyParts[2], out pose))
                return true;

            settings.labelCenters[MakeLabelCenterKey(stateKey, pose)] = ParseFaceCenter(value, GetDefaultFaceCenter(pose));
            return true;
        }

        private static string MakeLabelCenterKey(string stateKey, IndicatorPose pose)
        {
            return stateKey + "|" + IndicatorPoseHelper.GetKey(pose);
        }

        private static PointF ParseFaceCenter(string value, PointF fallback)
        {
            string[] parts = value.Split(',');
            if (parts.Length != 2)
                return fallback;

            float x;
            float y;
            if (!float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out x))
                return fallback;
            if (!float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out y))
                return fallback;

            return ClampFaceCenter(new PointF(x, y));
        }

        private static string FormatColor(Color color)
        {
            return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }

        private static Color ParseColor(string value, Color fallback)
        {
            string text = value.Trim();
            if (text.StartsWith("#", StringComparison.Ordinal))
                text = text.Substring(1);

            if (text.Length != 6)
                return fallback;

            int rgb;
            if (!int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out rgb))
                return fallback;

            return Color.FromArgb((rgb >> 16) & 255, (rgb >> 8) & 255, rgb & 255);
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
        public const int WH_MOUSE_LL = 14;
        public const int WM_MOUSEACTIVATE = 0x0021;
        public const int WM_NCHITTEST = 0x0084;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int HTTRANSPARENT = -1;
        public const int MA_NOACTIVATEANDEAT = 4;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct HookPoint
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseHookStruct
        {
            public HookPoint pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

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

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        public const int WM_QUIT = 0x0012;

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public HookPoint pt;
        }

        [DllImport("user32.dll")]
        public static extern int GetMessage(out NativeMessage lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TranslateMessage(ref NativeMessage lpMsg);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage(ref NativeMessage lpMsg);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostThreadMessage(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        public const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern uint GetClipboardSequenceNumber();

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
