using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace BossKeyTool
{
    [ComImportAttribute()]
    [GuidAttribute("56FDF342-FD6D-11d0-958A-006097C9A090")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITaskbarList
    {
        [PreserveSig] void HrInit();
        [PreserveSig] void AddTab(IntPtr hwnd);
        [PreserveSig] void DeleteTab(IntPtr hwnd);
        [PreserveSig] void ActivateTab(IntPtr hwnd);
        [PreserveSig] void SetActiveAlt(IntPtr hwnd);
    }

    [ComImportAttribute()]
    [GuidAttribute("56FDF344-FD6D-11d0-958A-006097C9A090")]
    internal class TaskbarListClass { }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    public static class GraphicsExt
    {
        public static GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0) { path.AddRectangle(bounds); return path; }
            int d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public static class Lang
    {
        public static bool IsEng = false;
        
        public static string Title { get { return IsEng ? "AppHider - Hide Windows" : "AppHider - Ẩn ứng dụng"; } }
        public static string SubTitle { get { return IsEng ? "Select hide mode for each running app." : "Chọn chế độ ẩn cho từng ứng dụng đang chạy."; } }
        public static string Refresh { get { return IsEng ? "Refresh" : "Làm mới"; } }
        public static string Settings { get { return IsEng ? "General settings" : "Cài đặt chung"; } }
        public static string Startup { get { return IsEng ? "Run at startup" : "Khởi động cùng Windows"; } }
        public static string StartupSub { get { return IsEng ? "Run AppHider automatically when logging in" : "Tự chạy AppHider khi đăng nhập"; } }
        public static string Theme { get { return IsEng ? "Theme" : "Giao diện"; } }
        public static string Language { get { return IsEng ? "Language" : "Ngôn ngữ"; } }
        public static string SaveBtn { get { return IsEng ? "Save and minimize to tray" : "Lưu và thu xuống khay hệ thống"; } }
        public static string TrayText { get { return IsEng ? "AppHider – Double click to open" : "AppHider – Nhấp đúp để mở"; } }
        public static string MenuOpen { get { return IsEng ? "Open settings" : "Mở cửa sổ cài đặt"; } }
        public static string MenuRestore { get { return IsEng ? "Restore all apps" : "Khôi phục tất cả ứng dụng"; } }
        public static string MenuExit { get { return IsEng ? "Exit" : "Thoát"; } }
        public static string NotRunning { get { return IsEng ? "Not running" : "Không chạy"; } }
        public static string Credits { get { return "© Kkmt"; } }
        
        public static string[] Modes { get { return IsEng ? new string[] { "Normal", "Hide on Alt+Tab (Remove from Alt+Tab)", "Hide Taskbar (Keep in Alt+Tab)" } : new string[] { "Bình thường", "Ẩn khi Alt+Tab (Mất khỏi Alt+Tab)", "Chỉ ẩn Taskbar (Còn trong Alt+Tab)" }; } }
        public static string[] Themes { get { return IsEng ? new string[] { "Light", "Dark" } : new string[] { "Sáng", "Tối" }; } }
        public static string[] Languages { get { return new string[] { "Tiếng Việt", "English" }; } }
    }

    public class Theme
    {
        public static bool IsDark = true;
        
        public static Color Bg { get { return IsDark ? Color.FromArgb(32, 32, 32) : Color.FromArgb(243, 243, 243); } }
        public static Color Card { get { return IsDark ? Color.FromArgb(43, 43, 43) : Color.White; } }
        public static Color CardHover { get { return IsDark ? Color.FromArgb(50, 50, 50) : Color.FromArgb(249, 249, 249); } }
        public static Color Stroke { get { return IsDark ? Color.FromArgb(58, 58, 58) : Color.FromArgb(229, 229, 229); } }
        public static Color ControlFill { get { return IsDark ? Color.FromArgb(45, 45, 45) : Color.FromArgb(251, 251, 251); } }
        public static Color ControlFillHover { get { return IsDark ? Color.FromArgb(50, 50, 50) : Color.FromArgb(246, 246, 246); } }
        public static Color TextPrimary { get { return IsDark ? Color.White : Color.FromArgb(26, 26, 26); } }
        public static Color TextSecondary { get { return IsDark ? Color.FromArgb(197, 197, 197) : Color.FromArgb(95, 95, 95); } }
        public static Color Accent { get { return IsDark ? Color.FromArgb(96, 205, 255) : Color.FromArgb(0, 103, 192); } }
        public static Color OnAccent { get { return IsDark ? Color.Black : Color.White; } }
        
        public static Color DotNormal { get { return IsDark ? Color.FromArgb(138, 138, 138) : Color.FromArgb(107, 107, 107); } }
        public static Color DotBlur { get { return IsDark ? Color.FromArgb(255, 185, 0) : Color.FromArgb(157, 93, 0); } }
        public static Color DotTask { get { return Accent; } }

        public static Font FontTitle = new Font("Segoe UI Semibold", 15F);
        public static Font FontSubtitle = new Font("Segoe UI Semibold", 10.5F);
        public static Font FontBody = new Font("Segoe UI", 10.5F);
        public static Font FontBodySmall = new Font("Segoe UI", 9F);
    }

    public class FluentToggle : Control
    {
        private bool _checked = false;
        public bool Checked
        {
            get { return _checked; }
            set { _checked = value; Invalidate(); }
        }

        public event EventHandler CheckedChanged;

        public FluentToggle()
        {
            this.Size = new Size(40, 20);
            this.DoubleBuffered = true;
            this.Cursor = Cursors.Hand;
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Parent.BackColor);

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (GraphicsPath path = GraphicsExt.GetRoundedRect(rect, 10))
            {
                if (Checked)
                {
                    using (SolidBrush br = new SolidBrush(Theme.Accent))
                        e.Graphics.FillPath(br, path);
                }
                else
                {
                    using (Pen p = new Pen(Theme.TextSecondary, 1.5f))
                        e.Graphics.DrawPath(p, path);
                }
            }

            int thumbSize = 12;
            int thumbY = (this.Height - thumbSize) / 2;
            int thumbX = Checked ? this.Width - thumbSize - 4 : 4;

            using (SolidBrush thumbBr = new SolidBrush(Checked ? Theme.OnAccent : Theme.TextSecondary))
            {
                e.Graphics.FillEllipse(thumbBr, thumbX, thumbY, thumbSize, thumbSize);
            }
        }
    }

    public class AppRowControl : Panel
    {
        public string ProcessName;
        public string WindowTitle;
        public int Mode = 0; 
        public Icon AppIcon;
        public bool IsRunning;
        private MainForm main;

        private ComboBox modeCombo;

        public AppRowControl(MainForm main, string proc, string title, int mode, bool isRunning)
        {
            this.main = main;
            this.ProcessName = proc;
            this.WindowTitle = title;
            this.Mode = mode;
            this.IsRunning = isRunning;
            this.Height = 56;
            this.Width = 600;
            this.Margin = new Padding(0);
            this.DoubleBuffered = true;
            this.Cursor = Cursors.Hand;

            try
            {
                if (isRunning)
                {
                    Process[] procs = Process.GetProcessesByName(proc);
                    if (procs.Length > 0)
                        AppIcon = Icon.ExtractAssociatedIcon(procs[0].MainModule.FileName);
                }
            }
            catch { }

            modeCombo = new ComboBox();
            modeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            modeCombo.Items.AddRange(Lang.Modes);
            modeCombo.SelectedIndex = mode;
            modeCombo.Font = Theme.FontBody;
            modeCombo.Width = 240;
            modeCombo.Location = new Point(this.Width - 260, (this.Height - modeCombo.Height) / 2);
            modeCombo.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            modeCombo.BackColor = Theme.ControlFill;
            modeCombo.ForeColor = Theme.TextPrimary;
            modeCombo.FlatStyle = FlatStyle.Flat;
            modeCombo.SelectedIndexChanged += ModeCombo_SelectedIndexChanged;
            this.Controls.Add(modeCombo);
        }

        private void ModeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Mode = modeCombo.SelectedIndex;
            this.Invalidate();
            
            if (this.IsRunning)
            {
                main.ChangeProcessModeFromUI(this.ProcessName, this.Mode);
            }
        }

        public void SetModeExternally(int newMode)
        {
            this.Mode = newMode;
            if (newMode >= 0 && newMode < modeCombo.Items.Count)
            {
                modeCombo.SelectedIndexChanged -= ModeCombo_SelectedIndexChanged;
                modeCombo.SelectedIndex = newMode;
                modeCombo.SelectedIndexChanged += ModeCombo_SelectedIndexChanged;
            }
            this.Invalidate();
        }

        public void UpdateLanguage()
        {
            int idx = modeCombo.SelectedIndex;
            modeCombo.SelectedIndexChanged -= ModeCombo_SelectedIndexChanged;
            modeCombo.Items.Clear();
            modeCombo.Items.AddRange(Lang.Modes);
            if (idx >= 0 && idx < modeCombo.Items.Count) modeCombo.SelectedIndex = idx;
            modeCombo.SelectedIndexChanged += ModeCombo_SelectedIndexChanged;
            this.Invalidate();
        }

        public void ApplyTheme()
        {
            modeCombo.BackColor = Theme.ControlFill;
            modeCombo.ForeColor = Theme.TextPrimary;
            this.BackColor = Theme.Card;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using (Pen p = new Pen(Theme.Stroke))
                e.Graphics.DrawLine(p, 16, 0, this.Width - 16, 0);

            Rectangle iconRect = new Rectangle(16, 12, 32, 32);
            if (AppIcon != null) e.Graphics.DrawIcon(AppIcon, iconRect);
            else e.Graphics.DrawIcon(SystemIcons.Application, iconRect);

            if (!IsRunning)
            {
                using (SolidBrush overlay = new SolidBrush(Color.FromArgb(128, Theme.Card)))
                    e.Graphics.FillRectangle(overlay, iconRect);
            }

            using (SolidBrush brPrimary = new SolidBrush(IsRunning ? Theme.TextPrimary : Theme.TextSecondary))
            using (SolidBrush brSecondary = new SolidBrush(Theme.TextSecondary))
            {
                e.Graphics.DrawString(WindowTitle, Theme.FontBody, brPrimary, new PointF(60, 10));
                e.Graphics.DrawString(ProcessName + (IsRunning ? "" : " (" + Lang.NotRunning + ")"), Theme.FontBodySmall, brSecondary, new PointF(60, 30));
            }

            Color dotColor = Theme.DotNormal;
            if (Mode == 1) dotColor = Theme.DotBlur;
            else if (Mode == 2) dotColor = Theme.DotTask;

            using (SolidBrush dotBr = new SolidBrush(dotColor))
            {
                e.Graphics.FillEllipse(dotBr, modeCombo.Left - 16, (this.Height - 8) / 2, 8, 8);
            }
        }
    }

    public class MainForm : Form
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        
        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        const int SW_RESTORE = 9;

        private FlowLayoutPanel listPanel;
        private Button saveButton;
        private Button refreshButton;
        private FluentToggle startupToggle;
        private Label titleLabel, subTitleLabel, lblSet, lblStart, lblStartSub, startupLabel, lblTheme, lblLang, lblCredits;
        private NotifyIcon mainTrayIcon;
        private Timer monitorTimer;
        private ComboBox themeCombo;
        private ComboBox langCombo;
        private TextBox searchBox;
        private ContextMenu contextMenu;

        public HashSet<string> HideOnBlurApps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> HideFromTaskbarApps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        private Dictionary<IntPtr, NotifyIcon> blurHiddenIcons = new Dictionary<IntPtr, NotifyIcon>();
        private Dictionary<IntPtr, NotifyIcon> taskbarHiddenIcons = new Dictionary<IntPtr, NotifyIcon>();
        
        private string settingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");
        private string themeFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme.txt");
        private string langFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang.txt");
        private string appName = "AppHider";
        
        private ITaskbarList taskbar;

        public MainForm()
        {
            this.Size = new Size(680, 640);
            this.MinimumSize = new Size(600, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = Theme.FontBody;
            this.DoubleBuffered = true;

            taskbar = (ITaskbarList)new TaskbarListClass();
            taskbar.HrInit();

            LoadLangSetting();

            InitUI();

            mainTrayIcon = new NotifyIcon();
            try {
                mainTrayIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            } catch {
                mainTrayIcon.Icon = SystemIcons.Application;
            }
            mainTrayIcon.Visible = true;
            mainTrayIcon.DoubleClick += MainTrayIcon_DoubleClick;

            contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(new MenuItem("Mở cửa sổ cài đặt", delegate { ShowMainForm(); }));
            contextMenu.MenuItems.Add(new MenuItem("Khôi phục tất cả ứng dụng", delegate { RestoreAllHiddenWindows(); }));
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(new MenuItem("Thoát", delegate { ExitApplication(); }));
            mainTrayIcon.ContextMenu = contextMenu;

            ApplyLanguage(); 

            monitorTimer = new Timer();
            monitorTimer.Interval = 500;
            monitorTimer.Tick += MonitorTimer_Tick;

            LoadSettings();
            RefreshWindowsList();

            LoadThemeSetting();
            ApplyTheme();

            monitorTimer.Start();
        }

        public void ChangeProcessModeFromUI(string procName, int mode)
        {
            HideOnBlurApps.Remove(procName);
            HideFromTaskbarApps.Remove(procName);

            if (mode == 1) HideOnBlurApps.Add(procName);
            else if (mode == 2) HideFromTaskbarApps.Add(procName);

            SaveSettingsToFile();
        }

        private void InitUI()
        {
            titleLabel = new Label() { Font = Theme.FontTitle, AutoSize = true, Location = new Point(24, 24) };
            subTitleLabel = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(24, 55) };
            this.Controls.Add(titleLabel);
            this.Controls.Add(subTitleLabel);

            searchBox = new TextBox();
            searchBox.Font = new Font("Segoe UI", 11F);
            searchBox.Location = new Point(24, 85);
            searchBox.Size = new Size(this.ClientSize.Width - 148, 28);
            searchBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            searchBox.BorderStyle = BorderStyle.FixedSingle;
            searchBox.TextChanged += SearchBox_TextChanged;
            this.Controls.Add(searchBox);

            refreshButton = new Button();
            refreshButton.Size = new Size(100, 28);
            refreshButton.Location = new Point(this.ClientSize.Width - 124, 85);
            refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refreshButton.FlatStyle = FlatStyle.Flat;
            refreshButton.Cursor = Cursors.Hand;
            refreshButton.Click += RefreshButton_Click;
            this.Controls.Add(refreshButton);

            listPanel = new FlowLayoutPanel();
            listPanel.Location = new Point(24, 125);
            listPanel.Size = new Size(this.ClientSize.Width - 48, 230);
            listPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            listPanel.AutoScroll = true;
            listPanel.FlowDirection = FlowDirection.TopDown;
            listPanel.WrapContents = false;
            this.Controls.Add(listPanel);

            lblSet = new Label() { Font = Theme.FontSubtitle, AutoSize = true };
            lblSet.Location = new Point(24, this.ClientSize.Height - 250);
            lblSet.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.Controls.Add(lblSet);

            Panel setPanel = new Panel();
            setPanel.Location = new Point(24, this.ClientSize.Height - 220);
            setPanel.Size = new Size(this.ClientSize.Width - 48, 140);
            setPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(setPanel);

            lblStart = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(16, 16) };
            lblStartSub = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(16, 36) };
            setPanel.Controls.Add(lblStart);
            setPanel.Controls.Add(lblStartSub);

            startupToggle = new FluentToggle();
            startupToggle.Location = new Point(setPanel.Width - 60, 20);
            startupToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            startupToggle.Checked = IsStartupEnabled();
            startupToggle.CheckedChanged += delegate { startupLabel.Text = startupToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt"); };
            setPanel.Controls.Add(startupToggle);

            startupLabel = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(setPanel.Width - 105, 22) };
            startupLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            startupLabel.Text = startupToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
            setPanel.Controls.Add(startupLabel);

            lblTheme = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(16, 60) };
            setPanel.Controls.Add(lblTheme);

            themeCombo = new ComboBox();
            themeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            themeCombo.Location = new Point(setPanel.Width - 140, 58);
            themeCombo.Width = 120;
            themeCombo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            themeCombo.SelectedIndexChanged += ThemeCombo_SelectedIndexChanged;
            setPanel.Controls.Add(themeCombo);

            lblLang = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(16, 100) };
            setPanel.Controls.Add(lblLang);

            langCombo = new ComboBox();
            langCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            langCombo.Items.AddRange(Lang.Languages);
            langCombo.Location = new Point(setPanel.Width - 140, 98);
            langCombo.Width = 120;
            langCombo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            langCombo.SelectedIndex = Lang.IsEng ? 1 : 0;
            langCombo.SelectedIndexChanged += LangCombo_SelectedIndexChanged;
            setPanel.Controls.Add(langCombo);

            saveButton = new Button();
            saveButton.Size = new Size(this.ClientSize.Width - 48, 38);
            saveButton.Location = new Point(24, this.ClientSize.Height - 68);
            saveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            saveButton.FlatStyle = FlatStyle.Flat;
            saveButton.Cursor = Cursors.Hand;
            saveButton.Font = new Font("Segoe UI Semibold", 11F);
            saveButton.Click += SaveButton_Click;
            this.Controls.Add(saveButton);

            lblCredits = new Label();
            lblCredits.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblCredits.AutoSize = true;
            lblCredits.Location = new Point(this.ClientSize.Width - 140, this.ClientSize.Height - 24);
            lblCredits.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.Controls.Add(lblCredits);
        }

        private void ApplyLanguage()
        {
            this.Text = Lang.Title;
            titleLabel.Text = Lang.Title;
            subTitleLabel.Text = Lang.SubTitle;
            refreshButton.Text = Lang.Refresh;
            lblSet.Text = Lang.Settings;
            lblStart.Text = Lang.Startup;
            lblStartSub.Text = Lang.StartupSub;
            lblTheme.Text = Lang.Theme;
            lblLang.Text = Lang.Language;
            saveButton.Text = Lang.SaveBtn;
            mainTrayIcon.Text = Lang.TrayText;
            
            startupLabel.Text = startupToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
            
            int thIdx = themeCombo.SelectedIndex;
            themeCombo.Items.Clear();
            themeCombo.Items.AddRange(Lang.Themes);
            if (thIdx >= 0 && thIdx < themeCombo.Items.Count) themeCombo.SelectedIndex = thIdx;

            contextMenu.MenuItems[0].Text = Lang.MenuOpen;
            contextMenu.MenuItems[1].Text = Lang.MenuRestore;
            contextMenu.MenuItems[3].Text = Lang.MenuExit;

            lblCredits.Text = Lang.Credits;
            lblCredits.Left = this.ClientSize.Width - 24 - lblCredits.PreferredWidth;

            foreach (Control c in listPanel.Controls)
            {
                AppRowControl row = c as AppRowControl;
                if (row != null) row.UpdateLanguage();
            }
        }

        private void LangCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Lang.IsEng = langCombo.SelectedIndex == 1;
            File.WriteAllText(langFile, Lang.IsEng ? "en" : "vi");
            ApplyLanguage();
        }

        private void LoadLangSetting()
        {
            if (File.Exists(langFile))
            {
                string text = File.ReadAllText(langFile).Trim();
                Lang.IsEng = text == "en";
            }
            else Lang.IsEng = false;
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            string q = searchBox.Text.Trim().ToLower();
            foreach (Control c in listPanel.Controls)
            {
                AppRowControl row = c as AppRowControl;
                if (row != null)
                {
                    if (string.IsNullOrEmpty(q))
                        row.Visible = true;
                    else
                    {
                        bool match = row.ProcessName.ToLower().Contains(q) || row.WindowTitle.ToLower().Contains(q);
                        row.Visible = match;
                    }
                }
            }
        }

        private void ThemeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Theme.IsDark = themeCombo.SelectedIndex == 1;
            File.WriteAllText(themeFile, Theme.IsDark ? "dark" : "light");
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = Theme.Bg;
            this.ForeColor = Theme.TextPrimary;

            int useImmersiveDarkMode = Theme.IsDark ? 1 : 0;
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int));

            foreach (Control c in this.Controls)
            {
                Label lbl = c as Label;
                if (lbl != null) lbl.ForeColor = Theme.TextPrimary;
            }
            lblStart.ForeColor = Theme.TextPrimary;
            lblStartSub.ForeColor = Theme.TextSecondary;
            lblTheme.ForeColor = Theme.TextPrimary;
            lblLang.ForeColor = Theme.TextPrimary;
            startupLabel.ForeColor = Theme.TextSecondary;
            lblCredits.ForeColor = Theme.TextSecondary;
            
            searchBox.BackColor = Theme.ControlFill;
            searchBox.ForeColor = Theme.TextPrimary;

            listPanel.BackColor = Theme.Card;
            foreach (Control c in listPanel.Controls)
            {
                AppRowControl row = c as AppRowControl;
                if (row != null) row.ApplyTheme();
            }

            refreshButton.BackColor = Theme.ControlFill;
            refreshButton.ForeColor = Theme.TextPrimary;
            refreshButton.FlatAppearance.BorderColor = Theme.Stroke;

            saveButton.BackColor = Theme.Accent;
            saveButton.ForeColor = Theme.OnAccent;
            saveButton.FlatAppearance.BorderSize = 0;

            themeCombo.BackColor = Theme.ControlFill;
            themeCombo.ForeColor = Theme.TextPrimary;

            langCombo.BackColor = Theme.ControlFill;
            langCombo.ForeColor = Theme.TextPrimary;
        }

        private void LoadThemeSetting()
        {
            if (File.Exists(themeFile))
            {
                string text = File.ReadAllText(themeFile).Trim();
                Theme.IsDark = text != "light";
            }
            else Theme.IsDark = true;
            
            themeCombo.SelectedIndex = Theme.IsDark ? 1 : 0;
        }

        private void LoadSettings()
        {
            if (File.Exists(settingsFile))
            {
                string[] lines = File.ReadAllLines(settingsFile);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split('|');
                    if (parts.Length == 2)
                    {
                        if (parts[1] == "HideOnBlur") HideOnBlurApps.Add(parts[0]);
                        else if (parts[1] == "HideFromTaskbar") HideFromTaskbarApps.Add(parts[0]);
                    }
                }
            }
        }

        private void SaveSettingsToFile()
        {
            List<string> lines = new List<string>();
            foreach (var p in HideOnBlurApps) lines.Add(string.Format("{0}|HideOnBlur", p));
            foreach (var p in HideFromTaskbarApps) lines.Add(string.Format("{0}|HideFromTaskbar", p));
            File.WriteAllLines(settingsFile, lines.ToArray());
        }

        private void SaveSettingsFromUI()
        {
            HideOnBlurApps.Clear();
            HideFromTaskbarApps.Clear();

            foreach (Control c in listPanel.Controls)
            {
                AppRowControl row = c as AppRowControl;
                if (row != null)
                {
                    if (row.Mode == 1) HideOnBlurApps.Add(row.ProcessName);
                    else if (row.Mode == 2) HideFromTaskbarApps.Add(row.ProcessName);
                }
            }
            SaveSettingsToFile();
        }

        private void RefreshWindowsList()
        {
            SaveSettingsFromUI();
            listPanel.Controls.Clear();
            HashSet<string> currentProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                if (IsWindowVisible(hWnd))
                {
                    StringBuilder sb = new StringBuilder(256);
                    GetWindowText(hWnd, sb, 256);
                    string title = sb.ToString();

                    if (!string.IsNullOrWhiteSpace(title) && title != this.Text)
                    {
                        uint pid;
                        GetWindowThreadProcessId(hWnd, out pid);
                        try
                        {
                            Process proc = Process.GetProcessById((int)pid);
                            string processName = proc.ProcessName;
                            
                            if (processName != "explorer" && processName != "Idle" && processName != "TextInputHost")
                            {
                                if (!currentProcesses.Contains(processName))
                                {
                                    currentProcesses.Add(processName);
                                    int mode = 0;
                                    if (HideOnBlurApps.Contains(processName)) mode = 1;
                                    else if (HideFromTaskbarApps.Contains(processName)) mode = 2;
                                    
                                    AppRowControl row = new AppRowControl(this, processName, title, mode, true);
                                    row.Width = listPanel.ClientSize.Width - 5;
                                    row.ApplyTheme();
                                    listPanel.Controls.Add(row);
                                }
                            }
                        }
                        catch { }
                    }
                }
                return true;
            }, IntPtr.Zero);
            
            foreach(string p in HideOnBlurApps)
            {
                if (!currentProcesses.Contains(p)) {
                    AppRowControl row = new AppRowControl(this, p, p, 1, false);
                    row.Width = listPanel.ClientSize.Width - 5;
                    row.ApplyTheme();
                    listPanel.Controls.Add(row);
                    currentProcesses.Add(p);
                }
            }
            foreach(string p in HideFromTaskbarApps)
            {
                if (!currentProcesses.Contains(p)) {
                    AppRowControl row = new AppRowControl(this, p, p, 2, false);
                    row.Width = listPanel.ClientSize.Width - 5;
                    row.ApplyTheme();
                    listPanel.Controls.Add(row);
                    currentProcesses.Add(p);
                }
            }
            SearchBox_TextChanged(null, null); 
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            RefreshWindowsList();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveSettingsFromUI();
            SetStartup(startupToggle.Checked);
            
            List<IntPtr> toRestore = new List<IntPtr>();
            foreach(var kvp in taskbarHiddenIcons) {
                uint pid;
                GetWindowThreadProcessId(kvp.Key, out pid);
                try {
                    string proc = Process.GetProcessById((int)pid).ProcessName;
                    if (!HideFromTaskbarApps.Contains(proc)) toRestore.Add(kvp.Key);
                } catch { }
            }
            foreach(var hWnd in toRestore) {
                taskbar.AddTab(hWnd);
                taskbarHiddenIcons[hWnd].Dispose();
                taskbarHiddenIcons.Remove(hWnd);
            }

            this.Hide();
            this.ShowInTaskbar = false;
        }

        private void ShowMainForm()
        {
            RefreshWindowsList();
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.Activate();
        }

        private void MainTrayIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowMainForm();
        }

        private void CleanDeadIcons()
        {
            List<IntPtr> deadBlur = new List<IntPtr>();
            foreach(var kvp in blurHiddenIcons) {
                if (!IsWindow(kvp.Key)) {
                    kvp.Value.Dispose();
                    deadBlur.Add(kvp.Key);
                }
            }
            foreach(var h in deadBlur) blurHiddenIcons.Remove(h);

            List<IntPtr> deadTaskbar = new List<IntPtr>();
            foreach(var kvp in taskbarHiddenIcons) {
                if (!IsWindow(kvp.Key)) {
                    kvp.Value.Dispose();
                    deadTaskbar.Add(kvp.Key);
                }
            }
            foreach(var h in deadTaskbar) taskbarHiddenIcons.Remove(h);
        }

        private void MonitorTimer_Tick(object sender, EventArgs e)
        {
            CleanDeadIcons();

            if (HideOnBlurApps.Count == 0 && HideFromTaskbarApps.Count == 0) return;

            IntPtr currentFg = GetForegroundWindow();
            uint fgPid;
            GetWindowThreadProcessId(currentFg, out fgPid);
            
            string fgProcessName = "";
            try { if (fgPid > 0) fgProcessName = Process.GetProcessById((int)fgPid).ProcessName; } catch { }

            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                if (IsWindowVisible(hWnd))
                {
                    uint pid;
                    GetWindowThreadProcessId(hWnd, out pid);
                    try
                    {
                        Process proc = Process.GetProcessById((int)pid);
                        string processName = proc.ProcessName;
                        
                        if (HideOnBlurApps.Contains(processName) && !processName.Equals(fgProcessName, StringComparison.OrdinalIgnoreCase) && hWnd != currentFg)
                        {
                            if (!blurHiddenIcons.ContainsKey(hWnd))
                            {
                                StringBuilder sb = new StringBuilder(256);
                                GetWindowText(hWnd, sb, 256);
                                string windowTitle = string.IsNullOrWhiteSpace(sb.ToString()) ? processName : sb.ToString();
                                
                                ShowWindow(hWnd, SW_HIDE);
                                CreateAppTrayIcon(hWnd, proc, windowTitle, blurHiddenIcons, true);
                            }
                        }

                        if (HideFromTaskbarApps.Contains(processName))
                        {
                            if (!taskbarHiddenIcons.ContainsKey(hWnd))
                            {
                                StringBuilder sb = new StringBuilder(256);
                                GetWindowText(hWnd, sb, 256);
                                string windowTitle = string.IsNullOrWhiteSpace(sb.ToString()) ? processName : sb.ToString();

                                taskbar.DeleteTab(hWnd);
                                CreateAppTrayIcon(hWnd, proc, windowTitle, taskbarHiddenIcons, false);
                            }
                        }
                    }
                    catch { }
                }
                return true;
            }, IntPtr.Zero);
        }

        private void CreateAppTrayIcon(IntPtr hWnd, Process proc, string windowTitle, Dictionary<IntPtr, NotifyIcon> dict, bool isBlurMode)
        {
            NotifyIcon appIcon = new NotifyIcon();
            string tooltip = windowTitle.Length > 63 ? windowTitle.Substring(0, 60) + "..." : windowTitle;
            appIcon.Text = tooltip;

            Icon iconToUse = SystemIcons.Application;
            try {
                Icon extIcon = Icon.ExtractAssociatedIcon(proc.MainModule.FileName);
                if (extIcon != null) iconToUse = extIcon;
            } catch { }

            appIcon.Icon = iconToUse;
            appIcon.Visible = true;

            appIcon.MouseClick += delegate (object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                {
                    if (isBlurMode)
                    {
                        ShowWindow(hWnd, SW_SHOW);
                        SetForegroundWindow(hWnd);
                        appIcon.Visible = false;
                        appIcon.Dispose();
                        dict.Remove(hWnd);
                    }
                    else
                    {
                        if (IsIconic(hWnd)) ShowWindow(hWnd, SW_RESTORE);
                        SetForegroundWindow(hWnd);
                    }
                }
            };

            dict.Add(hWnd, appIcon);
        }

        private void RestoreAllHiddenWindows()
        {
            List<IntPtr> blurHwnds = new List<IntPtr>(blurHiddenIcons.Keys);
            foreach (IntPtr hWnd in blurHwnds)
            {
                ShowWindow(hWnd, SW_SHOW);
                blurHiddenIcons[hWnd].Visible = false;
                blurHiddenIcons[hWnd].Dispose();
            }
            blurHiddenIcons.Clear();

            List<IntPtr> taskbarHwnds = new List<IntPtr>(taskbarHiddenIcons.Keys);
            foreach (IntPtr hWnd in taskbarHwnds)
            {
                taskbar.AddTab(hWnd);
                taskbarHiddenIcons[hWnd].Visible = false;
                taskbarHiddenIcons[hWnd].Dispose();
            }
            taskbarHiddenIcons.Clear();
        }

        private void ExitApplication()
        {
            RestoreAllHiddenWindows();
            mainTrayIcon.Visible = false;
            Application.Exit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                SaveSettingsFromUI();
                SetStartup(startupToggle.Checked);
                this.Hide();
                this.ShowInTaskbar = false;
            }
            else
            {
                RestoreAllHiddenWindows();
            }
        }

        private bool IsStartupEnabled()
        {
            try {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
                return rk.GetValue(appName) != null;
            } catch { return false; }
        }

        private void SetStartup(bool enable)
        {
            try {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (enable) rk.SetValue(appName, Application.ExecutablePath);
                else rk.DeleteValue(appName, false);
            } catch { }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
