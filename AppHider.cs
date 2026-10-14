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

    public enum EDataFlow { eRender, eCapture, eAll }
    public enum ERole { eConsole, eMultimedia, eCommunications }

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class MMDeviceEnumeratorComObject { }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        [PreserveSig] int EnumAudioEndpoints(EDataFlow dataFlow, int dwStateMask, out IMMDeviceCollection ppDevices);
        [PreserveSig] int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);
    }

    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceCollection
    {
        [PreserveSig] int GetCount(out int pcDevices);
        [PreserveSig] int Item(int nDevice, out IMMDevice ppDevice);
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig] int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
    }

    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionManager2
    {
        [PreserveSig] int GetAudioSessionControl(ref Guid AudioSessionGuid, int StreamFlags, out IntPtr SessionControl);
        [PreserveSig] int GetSimpleAudioVolume(ref Guid AudioSessionGuid, int StreamFlags, out IntPtr AudioVolume);
        [PreserveSig] int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);
    }

    [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionEnumerator
    {
        [PreserveSig] int GetCount(out int SessionCount);
        [PreserveSig] int GetSession(int SessionIndex, out IAudioSessionControl Session);
    }

    [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionControl
    {
        [PreserveSig] int GetState(out int pRetVal);
        [PreserveSig] int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig] int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
        [PreserveSig] int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig] int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
        [PreserveSig] int GetGroupingParam(out Guid pRetVal);
        [PreserveSig] int SetGroupingParam(ref Guid Override, ref Guid EventContext);
        [PreserveSig] int RegisterAudioSessionNotification(IntPtr NewNotifications);
        [PreserveSig] int UnregisterAudioSessionNotification(IntPtr NewNotifications);
    }

    [Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionControl2 : IAudioSessionControl
    {
        [PreserveSig] new int GetState(out int pRetVal);
        [PreserveSig] new int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig] new int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
        [PreserveSig] new int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig] new int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
        [PreserveSig] new int GetGroupingParam(out Guid pRetVal);
        [PreserveSig] new int SetGroupingParam(ref Guid Override, ref Guid EventContext);
        [PreserveSig] new int RegisterAudioSessionNotification(IntPtr NewNotifications);
        [PreserveSig] new int UnregisterAudioSessionNotification(IntPtr NewNotifications);
        [PreserveSig] int GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig] int GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig] int GetProcessId(out uint pRetVal);
        [PreserveSig] int IsSystemSoundsSession();
        [PreserveSig] int SetDuckingPreference(bool optOut);
    }

    [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISimpleAudioVolume
    {
        [PreserveSig] int SetMasterVolume(float fLevel, ref Guid EventContext);
        [PreserveSig] int GetMasterVolume(out float pfLevel);
        [PreserveSig] int SetMute(bool bMute, ref Guid EventContext);
        [PreserveSig] int GetMute(out bool pbMute);
    }

    public static class AudioManager
    {
        public static void SetProcessMute(string procName, uint pid, bool mute)
        {
            IMMDeviceEnumerator enumerator = null;
            IMMDeviceCollection col = null;
            try
            {
                enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                if (enumerator.EnumAudioEndpoints(EDataFlow.eRender, 1 /* DEVICE_STATE_ACTIVE */, out col) != 0 || col == null) return;
                int devCount = 0;
                col.GetCount(out devCount);

                Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                Guid empty = Guid.Empty;

                for (int d = 0; d < devCount; d++)
                {
                    IMMDevice dev = null;
                    IAudioSessionManager2 mgr = null;
                    IAudioSessionEnumerator sessionEnum = null;
                    try
                    {
                        col.Item(d, out dev);
                        if (dev == null) continue;

                        object o;
                        if (dev.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o) != 0 || o == null) continue;
                        mgr = (IAudioSessionManager2)o;
                        if (mgr.GetSessionEnumerator(out sessionEnum) != 0 || sessionEnum == null) continue;

                        int count;
                        sessionEnum.GetCount(out count);
                        for (int i = 0; i < count; i++)
                        {
                            IAudioSessionControl ctl = null;
                            try
                            {
                                sessionEnum.GetSession(i, out ctl);
                                var ctl2 = ctl as IAudioSessionControl2;
                                if (ctl2 != null)
                                {
                                    uint sPid;
                                    ctl2.GetProcessId(out sPid);
                                    bool match = false;
                                    if (pid > 0 && sPid == pid) match = true;
                                    else if (!string.IsNullOrEmpty(procName) && sPid > 0)
                                    {
                                        try
                                        {
                                            string sName = Process.GetProcessById((int)sPid).ProcessName;
                                            if (sName.Equals(procName, StringComparison.OrdinalIgnoreCase) ||
                                                sName.StartsWith(procName + "-", StringComparison.OrdinalIgnoreCase) ||
                                                sName.StartsWith(procName + "_", StringComparison.OrdinalIgnoreCase) ||
                                                (procName.Length >= 6 && sName.StartsWith(procName, StringComparison.OrdinalIgnoreCase)) ||
                                                (sName.Length >= 6 && procName.StartsWith(sName, StringComparison.OrdinalIgnoreCase)))
                                            {
                                                match = true;
                                            }
                                        }
                                        catch { }
                                    }

                                    if (match)
                                    {
                                        var vol = ctl as ISimpleAudioVolume;
                                        if (vol != null)
                                        {
                                            vol.SetMute(mute, ref empty);
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                if (ctl != null) Marshal.ReleaseComObject(ctl);
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        if (sessionEnum != null) Marshal.ReleaseComObject(sessionEnum);
                        if (mgr != null) Marshal.ReleaseComObject(mgr);
                        if (dev != null) Marshal.ReleaseComObject(dev);
                    }
                }
            }
            catch { }
            finally
            {
                if (col != null) Marshal.ReleaseComObject(col);
                if (enumerator != null) Marshal.ReleaseComObject(enumerator);
            }
        }
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
        public static string ShowOnStart { get { return IsEng ? "Show window on startup" : "Hiện cửa sổ này khi khởi động"; } }
        public static string ShowOnStartSub { get { return IsEng ? "Open main window when AppHider starts" : "Mở cửa sổ chính khi AppHider chạy"; } }
        public static string MuteOnMinimize { get { return IsEng ? "Mute apps when minimized/hidden" : "Tắt tiếng ứng dụng khi thu nhỏ"; } }
        public static string MuteOnMinimizeSub { get { return IsEng ? "Automatically mute sound when window is hidden or minimized" : "Tự động tắt tiếng khi ứng dụng bị ẩn hoặc thu nhỏ"; } }
        public static string Schedule { get { return IsEng ? "Active time schedule" : "Khung giờ hoạt động"; } }
        public static string ScheduleSub { get { return IsEng ? "Only hide & mute during the specified time period" : "Chỉ tự động ẩn và tắt tiếng trong khoảng thời gian"; } }
        public static string ScheduleFrom { get { return IsEng ? "From:" : "Từ:"; } }
        public static string ScheduleTo { get { return IsEng ? "To:" : "Đến:"; } }
        public static string Hotkey { get { return IsEng ? "Restore hotkey" : "Phím tắt khôi phục"; } }
        public static string HotkeySub { get { return IsEng ? "Quickly re-open hidden applications" : "Mở lại nhanh các ứng dụng đang bị ẩn"; } }
        public static string Theme { get { return IsEng ? "Theme" : "Giao diện"; } }
        public static string Language { get { return IsEng ? "Language" : "Ngôn ngữ"; } }
        public static string SaveBtn { get { return IsEng ? "Save and minimize to tray" : "Lưu và thu xuống khay hệ thống"; } }
        public static string TrayText { get { return IsEng ? "AppHider – Double click to open" : "AppHider – Nhấp đúp để mở"; } }
        public static string MenuOpen { get { return IsEng ? "Open settings" : "Mở cửa sổ cài đặt"; } }
        public static string MenuRestore { get { return IsEng ? "Restore all apps" : "Khôi phục tất cả ứng dụng"; } }
        public static string MenuExit { get { return IsEng ? "Exit" : "Thoát"; } }
        public static string NotRunning { get { return IsEng ? "Not running" : "Không chạy"; } }
        public static string Credits { get { return "© Kkmt"; } }
        
        public static string[] Modes { get { return IsEng ? new string[] { "Normal mode", "Hide application", "Hide from taskbar (keep window visible)" } : new string[] { "Chế độ bình thường", "Chế độ: Ẩn ứng dụng", "Chế độ: Ẩn taskbar (không tự ẩn cửa sổ ứng dụng)" }; } }
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
            modeCombo.Font = new Font("Segoe UI", 9.5F);
            modeCombo.Width = 340;
            modeCombo.DropDownWidth = 360;
            modeCombo.Location = new Point(this.Width - 355, (this.Height - modeCombo.Height) / 2);
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
            
            main.ChangeProcessModeFromUI(this.ProcessName, this.Mode);
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

            int textMaxW = modeCombo.Left - 76;
            using (SolidBrush brPrimary = new SolidBrush(IsRunning ? Theme.TextPrimary : Theme.TextSecondary))
            using (SolidBrush brSecondary = new SolidBrush(Theme.TextSecondary))
            {
                if (textMaxW > 50)
                {
                    RectangleF titleRect = new RectangleF(60, 10, textMaxW, 20);
                    RectangleF subRect = new RectangleF(60, 30, textMaxW, 18);
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Trimming = StringTrimming.EllipsisCharacter;
                        sf.FormatFlags = StringFormatFlags.NoWrap;
                        e.Graphics.DrawString(WindowTitle, Theme.FontBody, brPrimary, titleRect, sf);
                        e.Graphics.DrawString(ProcessName + (IsRunning ? "" : " (" + Lang.NotRunning + ")"), Theme.FontBodySmall, brSecondary, subRect, sf);
                    }
                }
                else
                {
                    e.Graphics.DrawString(WindowTitle, Theme.FontBody, brPrimary, new PointF(60, 10));
                    e.Graphics.DrawString(ProcessName + (IsRunning ? "" : " (" + Lang.NotRunning + ")"), Theme.FontBodySmall, brSecondary, new PointF(60, 30));
                }
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

        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        
        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9001;
        private const int HOTKEY_ID_ALT = 9002;

        private class HotkeyListener : NativeWindow, IDisposable
        {
            private const int WM_HOTKEY = 0x0312;
            private const int HOTKEY_ID = 9001;
            private const int HOTKEY_ID_ALT = 9002;
            private MainForm _form;

            public HotkeyListener(MainForm form)
            {
                _form = form;
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY && (m.WParam.ToInt32() == HOTKEY_ID || m.WParam.ToInt32() == HOTKEY_ID_ALT))
                {
                    _form.QuickRestoreHiddenApps();
                }
                base.WndProc(ref m);
            }

            public bool Register(uint mods, uint vk)
            {
                try
                {
                    Unregister();
                    if (vk == 0) return true;
                    bool ok = RegisterHotKey(this.Handle, HOTKEY_ID, mods, vk);

                    if (vk == (uint)Keys.D0)
                    {
                        RegisterHotKey(this.Handle, HOTKEY_ID_ALT, mods, (uint)Keys.NumPad0);
                    }
                    else if (vk == (uint)Keys.NumPad0)
                    {
                        RegisterHotKey(this.Handle, HOTKEY_ID_ALT, mods, (uint)Keys.D0);
                    }

                    return ok;
                }
                catch { return false; }
            }

            public void Unregister()
            {
                try
                {
                    if (this.Handle != IntPtr.Zero)
                    {
                        UnregisterHotKey(this.Handle, HOTKEY_ID);
                        UnregisterHotKey(this.Handle, HOTKEY_ID_ALT);
                    }
                }
                catch { }
            }

            public void Dispose()
            {
                Unregister();
                DestroyHandle();
            }
        }

        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        const int SW_RESTORE = 9;

        private FlowLayoutPanel listPanel;
        private Button saveButton;
        private Button refreshButton;
        private FluentToggle startupToggle;
        private FluentToggle showStartToggle;
        private FluentToggle muteToggle;
        private FluentToggle scheduleToggle;
        private DateTimePicker dtpScheduleFrom, dtpScheduleTo;
        private TextBox hotkeyBox;
        private Label titleLabel, subTitleLabel, lblSet, lblStart, lblStartSub, startupLabel, lblTheme, lblLang, lblCredits;
        private Label lblShowStart, lblShowStartSub, showStartLabel;
        private Label lblMute, lblMuteSub, muteLabel;
        private Label lblSchedule, lblScheduleSub, scheduleLabel, lblScheduleFrom, lblScheduleTo;
        private Label lblHotkey, lblHotkeySub;
        
        public bool IsStartupLaunch = false;
        private bool _allowVisible = false;
        private NotifyIcon mainTrayIcon;
        private Timer monitorTimer;
        private ComboBox themeCombo;
        private ComboBox langCombo;
        private TextBox searchBox;
        private ContextMenu contextMenu;

        public HashSet<string> HideOnBlurApps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> HideFromTaskbarApps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public bool MuteOnMinimize = false;
        public bool ScheduleEnabled = false;
        public TimeSpan ScheduleFrom = new TimeSpan(8, 0, 0);
        public TimeSpan ScheduleTo = new TimeSpan(17, 0, 0);

        public uint HotkeyMods = 2; // Default: Ctrl (2)
        public uint HotkeyVK = 123; // Default: F12 (Keys.F12)
        private HotkeyListener hotkeyListener;
        private DateTime quickRestoreCooldownUntil = DateTime.MinValue;

        private HashSet<string> currentlyMutedProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        private Dictionary<IntPtr, NotifyIcon> blurHiddenIcons = new Dictionary<IntPtr, NotifyIcon>();
        private Dictionary<IntPtr, NotifyIcon> taskbarHiddenIcons = new Dictionary<IntPtr, NotifyIcon>();
        
        private string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
        private string appName = "AppHider";
        
        private ITaskbarList taskbar;

        public bool LoadShowStartSetting()
        {
            if (File.Exists(configFile))
            {
                foreach (string line in File.ReadAllLines(configFile)) {
                    if (line.StartsWith("ShowStart=", StringComparison.OrdinalIgnoreCase)) {
                        string val = line.Substring(10).Trim();
                        bool res;
                        if (bool.TryParse(val, out res)) return res;
                    }
                }
            }
            return true;
        }

        private void SaveConfigValue(string keyToSave, string valToSave)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (File.Exists(configFile)) {
                foreach (string line in File.ReadAllLines(configFile)) {
                    int idx = line.IndexOf('=');
                    if (idx > 0) dict[line.Substring(0, idx).Trim()] = line.Substring(idx + 1).Trim();
                }
            }
            dict[keyToSave] = valToSave;
            List<string> lines = new List<string>();
            foreach (var kvp in dict) lines.Add(kvp.Key + "=" + kvp.Value);
            File.WriteAllLines(configFile, lines.ToArray());
        }

        private void SaveConfigValues(Dictionary<string, string> dictToUpdate)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (File.Exists(configFile)) {
                foreach (string line in File.ReadAllLines(configFile)) {
                    int idx = line.IndexOf('=');
                    if (idx > 0) dict[line.Substring(0, idx).Trim()] = line.Substring(idx + 1).Trim();
                }
            }
            foreach (var kvp in dictToUpdate)
            {
                dict[kvp.Key] = kvp.Value;
            }
            List<string> lines = new List<string>();
            foreach (var kvp in dict) lines.Add(kvp.Key + "=" + kvp.Value);
            File.WriteAllLines(configFile, lines.ToArray());
        }

        public bool IsScheduleActive()
        {
            if (!ScheduleEnabled) return true;
            TimeSpan now = DateTime.Now.TimeOfDay;
            if (ScheduleFrom <= ScheduleTo)
            {
                return now >= ScheduleFrom && now <= ScheduleTo;
            }
            else
            {
                return now >= ScheduleFrom || now <= ScheduleTo;
            }
        }

        public static string GetHotkeyText(uint mods, uint vk)
        {
            if (vk == 0) return Lang.IsEng ? "None" : "Không";
            List<string> parts = new List<string>();
            if ((mods & 0x0002) != 0) parts.Add("Ctrl");
            if ((mods & 0x0001) != 0) parts.Add("Alt");
            if ((mods & 0x0004) != 0) parts.Add("Shift");
            if ((mods & 0x0008) != 0) parts.Add("Win");

            Keys key = (Keys)vk;
            string keyStr = key.ToString();
            if (key == Keys.Oemtilde) keyStr = "~";
            else if (key >= Keys.D0 && key <= Keys.D9) keyStr = ((char)('0' + (key - Keys.D0))).ToString();
            else if (key >= Keys.NumPad0 && key <= Keys.NumPad9) keyStr = "Num " + ((char)('0' + (key - Keys.NumPad0)));
            else if (key == Keys.OemQuestion) keyStr = "/";
            else if (key == Keys.OemPeriod) keyStr = ".";
            else if (key == Keys.Oemcomma) keyStr = ",";

            parts.Add(keyStr);
            return string.Join(" + ", parts.ToArray());
        }

        private bool RegisterGlobalHotkey(uint mods, uint vk)
        {
            if (hotkeyListener != null)
            {
                return hotkeyListener.Register(mods, vk);
            }
            return false;
        }

        private void UnregisterGlobalHotkey()
        {
            if (hotkeyListener != null)
            {
                hotkeyListener.Unregister();
            }
        }

        public void QuickRestoreHiddenApps()
        {
            quickRestoreCooldownUntil = DateTime.Now.AddSeconds(2.5);

            List<IntPtr> blurHwnds = new List<IntPtr>(blurHiddenIcons.Keys);
            foreach (IntPtr hWnd in blurHwnds)
            {
                ShowWindow(hWnd, SW_RESTORE);
                ShowWindow(hWnd, SW_SHOW);
                SetForegroundWindow(hWnd);
                SwitchToThisWindow(hWnd, true);
                if (blurHiddenIcons.ContainsKey(hWnd))
                {
                    blurHiddenIcons[hWnd].Visible = false;
                    blurHiddenIcons[hWnd].Dispose();
                    blurHiddenIcons.Remove(hWnd);
                }
            }

            List<IntPtr> taskbarHwnds = new List<IntPtr>(taskbarHiddenIcons.Keys);
            foreach (IntPtr hWnd in taskbarHwnds)
            {
                taskbar.AddTab(hWnd);
                if (IsIconic(hWnd)) ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
                SwitchToThisWindow(hWnd, true);
                if (taskbarHiddenIcons.ContainsKey(hWnd))
                {
                    taskbarHiddenIcons[hWnd].Visible = false;
                    taskbarHiddenIcons[hWnd].Dispose();
                    taskbarHiddenIcons.Remove(hWnd);
                }
            }

            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                if (IsWindow(hWnd))
                {
                    uint pid;
                    GetWindowThreadProcessId(hWnd, out pid);
                    try
                    {
                        string processName = Process.GetProcessById((int)pid).ProcessName;
                        if (HideOnBlurApps.Contains(processName) || HideFromTaskbarApps.Contains(processName))
                        {
                            if (IsIconic(hWnd))
                            {
                                ShowWindow(hWnd, SW_RESTORE);
                                SetForegroundWindow(hWnd);
                                SwitchToThisWindow(hWnd, true);
                            }
                        }
                    }
                    catch { }
                }
                return true;
            }, IntPtr.Zero);

            RestoreAllMutedProcesses();
        }

        protected override void SetVisibleCore(bool value)
        {
            if (IsStartupLaunch && !_allowVisible && !LoadShowStartSetting())
            {
                value = false;
                if (!this.IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        public MainForm()
        {
            this.Size = new Size(760, 830);
            this.MinimumSize = new Size(700, 760);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = Theme.FontBody;
            this.DoubleBuffered = true;

            taskbar = (ITaskbarList)new TaskbarListClass();
            taskbar.HrInit();

            LoadLangSetting();
            LoadSettings();

            InitUI();

            hotkeyListener = new HotkeyListener(this);
            RegisterGlobalHotkey(HotkeyMods, HotkeyVK);

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
            contextMenu.MenuItems.Add(new MenuItem("Khôi phục tất cả ứng dụng (" + GetHotkeyText(HotkeyMods, HotkeyVK) + ")", delegate { QuickRestoreHiddenApps(); }));
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(new MenuItem("Thoát", delegate { ExitApplication(); }));
            mainTrayIcon.ContextMenu = contextMenu;

            ApplyLanguage(); 

            monitorTimer = new Timer();
            monitorTimer.Interval = 500;
            monitorTimer.Tick += MonitorTimer_Tick;

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
            listPanel.Size = new Size(this.ClientSize.Width - 48, this.ClientSize.Height - 590);
            listPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            listPanel.AutoScroll = true;
            listPanel.FlowDirection = FlowDirection.TopDown;
            listPanel.WrapContents = false;
            this.Controls.Add(listPanel);

            lblSet = new Label() { Font = Theme.FontSubtitle, AutoSize = true };
            lblSet.Location = new Point(24, this.ClientSize.Height - 452);
            lblSet.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.Controls.Add(lblSet);

            Panel setPanel = new Panel();
            setPanel.Location = new Point(24, this.ClientSize.Height - 422);
            setPanel.Size = new Size(this.ClientSize.Width - 48, 345);
            setPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(setPanel);

            lblStart = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(16, 12) };
            lblStartSub = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(16, 32) };
            setPanel.Controls.Add(lblStart);
            setPanel.Controls.Add(lblStartSub);

            startupToggle = new FluentToggle();
            startupToggle.Location = new Point(setPanel.Width - 60, 16);
            startupToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            startupToggle.Checked = IsStartupEnabled();
            startupToggle.CheckedChanged += delegate { startupLabel.Text = startupToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt"); };
            setPanel.Controls.Add(startupToggle);

            startupLabel = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(setPanel.Width - 105, 18) };
            startupLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            startupLabel.Text = startupToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
            setPanel.Controls.Add(startupLabel);

            lblShowStart = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(16, 56) };
            lblShowStartSub = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(16, 76) };
            setPanel.Controls.Add(lblShowStart);
            setPanel.Controls.Add(lblShowStartSub);

            showStartToggle = new FluentToggle();
            showStartToggle.Location = new Point(setPanel.Width - 60, 60);
            showStartToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            showStartToggle.Checked = LoadShowStartSetting();
            showStartToggle.CheckedChanged += delegate { 
                showStartLabel.Text = showStartToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt"); 
                SaveConfigValue("ShowStart", showStartToggle.Checked.ToString());
            };
            setPanel.Controls.Add(showStartToggle);

            showStartLabel = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(setPanel.Width - 105, 62) };
            showStartLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            showStartLabel.Text = showStartToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
            setPanel.Controls.Add(showStartLabel);

            lblMute = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(16, 100) };
            lblMuteSub = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(16, 120) };
            setPanel.Controls.Add(lblMute);
            setPanel.Controls.Add(lblMuteSub);

            muteToggle = new FluentToggle();
            muteToggle.Location = new Point(setPanel.Width - 60, 104);
            muteToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            muteToggle.Checked = MuteOnMinimize;
            muteToggle.CheckedChanged += delegate {
                MuteOnMinimize = muteToggle.Checked;
                muteLabel.Text = muteToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
                SaveConfigValue("MuteOnMinimize", MuteOnMinimize.ToString());
                if (!MuteOnMinimize)
                {
                    RestoreAllMutedProcesses();
                }
            };
            setPanel.Controls.Add(muteToggle);

            muteLabel = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(setPanel.Width - 105, 106) };
            muteLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            muteLabel.Text = muteToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
            setPanel.Controls.Add(muteLabel);

            lblSchedule = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(16, 144) };
            lblScheduleSub = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(16, 164) };
            setPanel.Controls.Add(lblSchedule);
            setPanel.Controls.Add(lblScheduleSub);

            scheduleToggle = new FluentToggle();
            scheduleToggle.Location = new Point(setPanel.Width - 60, 148);
            scheduleToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            scheduleToggle.Checked = ScheduleEnabled;
            scheduleToggle.CheckedChanged += delegate {
                ScheduleEnabled = scheduleToggle.Checked;
                scheduleLabel.Text = scheduleToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
                dtpScheduleFrom.Enabled = scheduleToggle.Checked;
                dtpScheduleTo.Enabled = scheduleToggle.Checked;
                lblScheduleFrom.Enabled = scheduleToggle.Checked;
                lblScheduleTo.Enabled = scheduleToggle.Checked;
                SaveConfigValue("ScheduleEnabled", ScheduleEnabled.ToString());
            };
            setPanel.Controls.Add(scheduleToggle);

            scheduleLabel = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(setPanel.Width - 105, 150) };
            scheduleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            scheduleLabel.Text = scheduleToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
            setPanel.Controls.Add(scheduleLabel);

            lblScheduleFrom = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(16, 192), Enabled = ScheduleEnabled };
            setPanel.Controls.Add(lblScheduleFrom);

            DateTime today = DateTime.Today;

            dtpScheduleFrom = new DateTimePicker();
            dtpScheduleFrom.Format = DateTimePickerFormat.Custom;
            dtpScheduleFrom.CustomFormat = "HH:mm";
            dtpScheduleFrom.ShowUpDown = true;
            dtpScheduleFrom.Font = new Font("Segoe UI", 9.5F);
            dtpScheduleFrom.Location = new Point(56, 189);
            dtpScheduleFrom.Width = 70;
            dtpScheduleFrom.Value = today.Add(ScheduleFrom);
            dtpScheduleFrom.Enabled = ScheduleEnabled;
            dtpScheduleFrom.ValueChanged += delegate {
                ScheduleFrom = dtpScheduleFrom.Value.TimeOfDay;
                SaveConfigValue("ScheduleFrom", ScheduleFrom.ToString(@"hh\:mm"));
            };
            setPanel.Controls.Add(dtpScheduleFrom);

            lblScheduleTo = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(136, 192), Enabled = ScheduleEnabled };
            setPanel.Controls.Add(lblScheduleTo);

            dtpScheduleTo = new DateTimePicker();
            dtpScheduleTo.Format = DateTimePickerFormat.Custom;
            dtpScheduleTo.CustomFormat = "HH:mm";
            dtpScheduleTo.ShowUpDown = true;
            dtpScheduleTo.Font = new Font("Segoe UI", 9.5F);
            dtpScheduleTo.Location = new Point(176, 189);
            dtpScheduleTo.Width = 70;
            dtpScheduleTo.Value = today.Add(ScheduleTo);
            dtpScheduleTo.Enabled = ScheduleEnabled;
            dtpScheduleTo.ValueChanged += delegate {
                ScheduleTo = dtpScheduleTo.Value.TimeOfDay;
                SaveConfigValue("ScheduleTo", ScheduleTo.ToString(@"hh\:mm"));
            };
            setPanel.Controls.Add(dtpScheduleTo);

            lblHotkey = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(16, 226) };
            lblHotkeySub = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(16, 246) };
            setPanel.Controls.Add(lblHotkey);
            setPanel.Controls.Add(lblHotkeySub);

            hotkeyBox = new TextBox();
            hotkeyBox.ReadOnly = true;
            hotkeyBox.Font = new Font("Segoe UI Semibold", 9.5F);
            hotkeyBox.TextAlign = HorizontalAlignment.Center;
            hotkeyBox.Location = new Point(setPanel.Width - 140, 224);
            hotkeyBox.Width = 120;
            hotkeyBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            hotkeyBox.Cursor = Cursors.Hand;
            hotkeyBox.Text = GetHotkeyText(HotkeyMods, HotkeyVK);
            hotkeyBox.GotFocus += delegate { hotkeyBox.Text = Lang.IsEng ? "Press keys..." : "Bấm phím..."; hotkeyBox.SelectAll(); };
            hotkeyBox.LostFocus += delegate { hotkeyBox.Text = GetHotkeyText(HotkeyMods, HotkeyVK); };
            hotkeyBox.KeyDown += HotkeyBox_KeyDown;
            setPanel.Controls.Add(hotkeyBox);

            lblTheme = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(16, 270) };
            setPanel.Controls.Add(lblTheme);

            themeCombo = new ComboBox();
            themeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            themeCombo.Location = new Point(setPanel.Width - 140, 268);
            themeCombo.Width = 120;
            themeCombo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            themeCombo.SelectedIndexChanged += ThemeCombo_SelectedIndexChanged;
            setPanel.Controls.Add(themeCombo);

            lblLang = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(16, 308) };
            setPanel.Controls.Add(lblLang);

            langCombo = new ComboBox();
            langCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            langCombo.Items.AddRange(Lang.Languages);
            langCombo.Location = new Point(setPanel.Width - 140, 306);
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

        private void HotkeyBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Menu) return;

            if (e.KeyCode == Keys.Escape)
            {
                hotkeyBox.Text = GetHotkeyText(HotkeyMods, HotkeyVK);
                this.ActiveControl = null;
                return;
            }

            uint mods = 0;
            if (e.Control) mods |= 0x0002;
            if (e.Alt) mods |= 0x0001;
            if (e.Shift) mods |= 0x0004;

            uint vk = (uint)e.KeyCode;

            if (RegisterGlobalHotkey(mods, vk))
            {
                HotkeyMods = mods;
                HotkeyVK = vk;
                hotkeyBox.Text = GetHotkeyText(HotkeyMods, HotkeyVK);
                SaveConfigValue("Hotkey", HotkeyMods + "|" + HotkeyVK);
                if (contextMenu != null && contextMenu.MenuItems.Count > 1)
                {
                    contextMenu.MenuItems[1].Text = Lang.MenuRestore + " (" + GetHotkeyText(HotkeyMods, HotkeyVK) + ")";
                }
                this.ActiveControl = null;
            }
            else
            {
                MessageBox.Show(Lang.IsEng ? "This hotkey is already in use by another program!" : "Phím tắt này đang bị trùng với ứng dụng khác!", Lang.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                hotkeyBox.Text = GetHotkeyText(HotkeyMods, HotkeyVK);
            }
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
            lblShowStart.Text = Lang.ShowOnStart;
            lblShowStartSub.Text = Lang.ShowOnStartSub;
            lblMute.Text = Lang.MuteOnMinimize;
            lblMuteSub.Text = Lang.MuteOnMinimizeSub;
            lblSchedule.Text = Lang.Schedule;
            lblScheduleSub.Text = Lang.ScheduleSub;
            lblScheduleFrom.Text = Lang.ScheduleFrom;
            lblScheduleTo.Text = Lang.ScheduleTo;
            lblHotkey.Text = Lang.Hotkey;
            lblHotkeySub.Text = Lang.HotkeySub;
            lblTheme.Text = Lang.Theme;
            lblLang.Text = Lang.Language;
            saveButton.Text = Lang.SaveBtn;
            mainTrayIcon.Text = Lang.TrayText;
            
            startupLabel.Text = startupToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
            showStartLabel.Text = showStartToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
            muteLabel.Text = muteToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
            scheduleLabel.Text = scheduleToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
            
            if (hotkeyBox != null) hotkeyBox.Text = GetHotkeyText(HotkeyMods, HotkeyVK);

            int thIdx = themeCombo.SelectedIndex;
            themeCombo.Items.Clear();
            themeCombo.Items.AddRange(Lang.Themes);
            if (thIdx >= 0 && thIdx < themeCombo.Items.Count) themeCombo.SelectedIndex = thIdx;

            contextMenu.MenuItems[0].Text = Lang.MenuOpen;
            contextMenu.MenuItems[1].Text = Lang.MenuRestore + " (" + GetHotkeyText(HotkeyMods, HotkeyVK) + ")";
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
            SaveConfigValue("Lang", Lang.IsEng ? "en" : "vi");
            ApplyLanguage();
        }

        private void LoadLangSetting()
        {
            Lang.IsEng = false;
            if (File.Exists(configFile))
            {
                foreach (string line in File.ReadAllLines(configFile)) {
                    if (line.StartsWith("Lang=", StringComparison.OrdinalIgnoreCase)) {
                        Lang.IsEng = line.Substring(5).Trim() == "en";
                    }
                }
            }
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
            SaveConfigValue("Theme", Theme.IsDark ? "dark" : "light");
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
            lblShowStart.ForeColor = Theme.TextPrimary;
            lblShowStartSub.ForeColor = Theme.TextSecondary;
            lblMute.ForeColor = Theme.TextPrimary;
            lblMuteSub.ForeColor = Theme.TextSecondary;
            lblSchedule.ForeColor = Theme.TextPrimary;
            lblScheduleSub.ForeColor = Theme.TextSecondary;
            lblScheduleFrom.ForeColor = Theme.TextPrimary;
            lblScheduleTo.ForeColor = Theme.TextPrimary;
            lblHotkey.ForeColor = Theme.TextPrimary;
            lblHotkeySub.ForeColor = Theme.TextSecondary;
            lblTheme.ForeColor = Theme.TextPrimary;
            lblLang.ForeColor = Theme.TextPrimary;
            startupLabel.ForeColor = Theme.TextSecondary;
            showStartLabel.ForeColor = Theme.TextSecondary;
            muteLabel.ForeColor = Theme.TextSecondary;
            scheduleLabel.ForeColor = Theme.TextSecondary;
            lblCredits.ForeColor = Theme.TextSecondary;
            
            searchBox.BackColor = Theme.ControlFill;
            searchBox.ForeColor = Theme.TextPrimary;

            hotkeyBox.BackColor = Theme.ControlFill;
            hotkeyBox.ForeColor = Theme.TextPrimary;

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

            dtpScheduleFrom.CalendarForeColor = Theme.TextPrimary;
            dtpScheduleFrom.CalendarMonthBackground = Theme.ControlFill;
            dtpScheduleFrom.CalendarTitleBackColor = Theme.Accent;
            dtpScheduleTo.CalendarForeColor = Theme.TextPrimary;
            dtpScheduleTo.CalendarMonthBackground = Theme.ControlFill;
            dtpScheduleTo.CalendarTitleBackColor = Theme.Accent;
        }

        private void LoadThemeSetting()
        {
            Theme.IsDark = true;
            if (File.Exists(configFile))
            {
                foreach (string line in File.ReadAllLines(configFile)) {
                    if (line.StartsWith("Theme=", StringComparison.OrdinalIgnoreCase)) {
                        Theme.IsDark = line.Substring(6).Trim() != "light";
                    }
                }
            }
            
            themeCombo.SelectedIndex = Theme.IsDark ? 1 : 0;
        }

        private void LoadSettings()
        {
            HideOnBlurApps.Clear();
            HideFromTaskbarApps.Clear();
            if (File.Exists(configFile))
            {
                string[] lines = File.ReadAllLines(configFile);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    int idx = line.IndexOf('=');
                    if (idx > 0)
                    {
                        string key = line.Substring(0, idx).Trim();
                        string val = line.Substring(idx + 1).Trim();
                        if (key.Equals("HideOnBlur", StringComparison.OrdinalIgnoreCase)) {
                            foreach (var p in val.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries)) HideOnBlurApps.Add(p.Trim());
                        } else if (key.Equals("HideFromTaskbar", StringComparison.OrdinalIgnoreCase)) {
                            foreach (var p in val.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries)) HideFromTaskbarApps.Add(p.Trim());
                        } else if (key.Equals("MuteOnMinimize", StringComparison.OrdinalIgnoreCase)) {
                            bool res;
                            if (bool.TryParse(val, out res)) MuteOnMinimize = res;
                        } else if (key.Equals("ScheduleEnabled", StringComparison.OrdinalIgnoreCase)) {
                            bool res;
                            if (bool.TryParse(val, out res)) ScheduleEnabled = res;
                        } else if (key.Equals("ScheduleFrom", StringComparison.OrdinalIgnoreCase)) {
                            TimeSpan res;
                            if (TimeSpan.TryParse(val, out res)) ScheduleFrom = res;
                        } else if (key.Equals("ScheduleTo", StringComparison.OrdinalIgnoreCase)) {
                            TimeSpan res;
                            if (TimeSpan.TryParse(val, out res)) ScheduleTo = res;
                        } else if (key.Equals("Hotkey", StringComparison.OrdinalIgnoreCase)) {
                            string[] parts = val.Split('|');
                            if (parts.Length == 2) {
                                uint m, k;
                                if (uint.TryParse(parts[0], out m) && uint.TryParse(parts[1], out k)) {
                                    HotkeyMods = m;
                                    HotkeyVK = k;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SaveSettingsToFile()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict["HideOnBlur"] = string.Join(",", HideOnBlurApps);
            dict["HideFromTaskbar"] = string.Join(",", HideFromTaskbarApps);
            dict["MuteOnMinimize"] = MuteOnMinimize.ToString();
            dict["ScheduleEnabled"] = ScheduleEnabled.ToString();
            dict["ScheduleFrom"] = ScheduleFrom.ToString(@"hh\:mm");
            dict["ScheduleTo"] = ScheduleTo.ToString(@"hh\:mm");
            dict["Hotkey"] = HotkeyMods + "|" + HotkeyVK;
            SaveConfigValues(dict);
        }

        private void SaveSettingsFromUI()
        {
            if (listPanel.Controls.Count == 0) return;
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

            if (muteToggle != null) MuteOnMinimize = muteToggle.Checked;
            if (scheduleToggle != null) ScheduleEnabled = scheduleToggle.Checked;
            if (dtpScheduleFrom != null) ScheduleFrom = dtpScheduleFrom.Value.TimeOfDay;
            if (dtpScheduleTo != null) ScheduleTo = dtpScheduleTo.Value.TimeOfDay;

            SaveSettingsToFile();
        }

        private void RefreshWindowsList()
        {
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
        }

        private void ShowMainForm()
        {
            _allowVisible = true;
            RefreshWindowsList();
            this.Show();
            this.WindowState = FormWindowState.Normal;
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

            List<string> deadMuted = new List<string>();
            foreach (var proc in currentlyMutedProcesses)
            {
                try
                {
                    if (Process.GetProcessesByName(proc).Length == 0)
                        deadMuted.Add(proc);
                }
                catch { }
            }
            foreach (var p in deadMuted) currentlyMutedProcesses.Remove(p);
        }

        private void MonitorTimer_Tick(object sender, EventArgs e)
        {
            CleanDeadIcons();

            if (DateTime.Now < quickRestoreCooldownUntil)
            {
                return;
            }

            if (!IsScheduleActive())
            {
                if (blurHiddenIcons.Count > 0 || taskbarHiddenIcons.Count > 0)
                {
                    RestoreAllHiddenWindows();
                }
                if (currentlyMutedProcesses.Count > 0)
                {
                    RestoreAllMutedProcesses();
                }
                return;
            }

            if (HideOnBlurApps.Count == 0 && HideFromTaskbarApps.Count == 0)
            {
                if (currentlyMutedProcesses.Count > 0) RestoreAllMutedProcesses();
                return;
            }

            IntPtr currentFg = GetForegroundWindow();
            uint fgPid;
            GetWindowThreadProcessId(currentFg, out fgPid);
            
            string fgProcessName = "";
            try { if (fgPid > 0) fgProcessName = Process.GetProcessById((int)fgPid).ProcessName; } catch { }

            HashSet<string> processesToMute = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Any process that has a window currently hidden by Mode 1 ("Chế độ: Ẩn ứng dụng")
            foreach (var kvp in blurHiddenIcons)
            {
                uint pid;
                GetWindowThreadProcessId(kvp.Key, out pid);
                try
                {
                    string pName = Process.GetProcessById((int)pid).ProcessName;
                    processesToMute.Add(pName);
                }
                catch { }
            }

            Dictionary<string, bool> procHasVisibleNormalWindow = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, bool> procHasIconicWindow = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

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

                        bool inBlur = HideOnBlurApps.Contains(processName);
                        bool inTaskbar = HideFromTaskbarApps.Contains(processName);

                        if (inBlur || inTaskbar)
                        {
                            bool iconic = IsIconic(hWnd);
                            RECT r;
                            GetWindowRect(hWnd, out r);
                            int w = r.Right - r.Left;
                            int h = r.Bottom - r.Top;
                            bool isRealWindow = (w > 100 && h > 100);

                            if (iconic)
                            {
                                procHasIconicWindow[processName] = true;
                            }
                            else if (isRealWindow)
                            {
                                procHasVisibleNormalWindow[processName] = true;
                            }

                            bool isForegroundApp = processName.Equals(fgProcessName, StringComparison.OrdinalIgnoreCase) || hWnd == currentFg;

                            if (inBlur)
                            {
                                if (!isForegroundApp)
                                {
                                    if (!blurHiddenIcons.ContainsKey(hWnd))
                                    {
                                        StringBuilder sb = new StringBuilder(256);
                                        GetWindowText(hWnd, sb, 256);
                                        string windowTitle = string.IsNullOrWhiteSpace(sb.ToString()) ? processName : sb.ToString();

                                        ShowWindow(hWnd, SW_HIDE);
                                        CreateAppTrayIcon(hWnd, proc, windowTitle, blurHiddenIcons, true);
                                    }
                                    else
                                    {
                                        ShowWindow(hWnd, SW_HIDE);
                                    }
                                    processesToMute.Add(processName);
                                }
                                else
                                {
                                    if (blurHiddenIcons.ContainsKey(hWnd))
                                    {
                                        blurHiddenIcons[hWnd].Visible = false;
                                        blurHiddenIcons[hWnd].Dispose();
                                        blurHiddenIcons.Remove(hWnd);
                                    }
                                }
                            }

                            if (inTaskbar)
                            {
                                if (!taskbarHiddenIcons.ContainsKey(hWnd))
                                {
                                    StringBuilder sb = new StringBuilder(256);
                                    GetWindowText(hWnd, sb, 256);
                                    string windowTitle = string.IsNullOrWhiteSpace(sb.ToString()) ? processName : sb.ToString();

                                    taskbar.DeleteTab(hWnd);
                                    CreateAppTrayIcon(hWnd, proc, windowTitle, taskbarHiddenIcons, false);
                                }
                                else
                                {
                                    taskbar.DeleteTab(hWnd);
                                }
                            }
                        }
                    }
                    catch { }
                }
                return true;
            }, IntPtr.Zero);

            // 2. Check for minimized windows in Mode 2 ("Chế độ: Ẩn taskbar (không tự ẩn cửa sổ ứng dụng)")
            foreach (string proc in HideFromTaskbarApps)
            {
                bool hasNormal = procHasVisibleNormalWindow.ContainsKey(proc) && procHasVisibleNormalWindow[proc];
                bool hasIconic = procHasIconicWindow.ContainsKey(proc) && procHasIconicWindow[proc];
                if (hasIconic && !hasNormal)
                {
                    processesToMute.Add(proc);
                }
            }

            // Also for Mode 1: if user minimized it manually
            foreach (string proc in HideOnBlurApps)
            {
                bool hasNormal = procHasVisibleNormalWindow.ContainsKey(proc) && procHasVisibleNormalWindow[proc];
                bool hasIconic = procHasIconicWindow.ContainsKey(proc) && procHasIconicWindow[proc];
                if (hasIconic && !hasNormal)
                {
                    processesToMute.Add(proc);
                }
            }

            // 3. Apply muting state transitions:
            if (MuteOnMinimize)
            {
                foreach (string proc in processesToMute)
                {
                    if (!currentlyMutedProcesses.Contains(proc))
                    {
                        AudioManager.SetProcessMute(proc, 0, true);
                        currentlyMutedProcesses.Add(proc);
                    }
                }

                List<string> toUnmute = new List<string>();
                foreach (string proc in currentlyMutedProcesses)
                {
                    if (!processesToMute.Contains(proc))
                    {
                        toUnmute.Add(proc);
                    }
                }
                foreach (string proc in toUnmute)
                {
                    AudioManager.SetProcessMute(proc, 0, false);
                    currentlyMutedProcesses.Remove(proc);
                }
            }
            else
            {
                if (currentlyMutedProcesses.Count > 0)
                {
                    RestoreAllMutedProcesses();
                }
            }
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

                        if (currentlyMutedProcesses.Contains(proc.ProcessName))
                        {
                            AudioManager.SetProcessMute(proc.ProcessName, 0, false);
                            currentlyMutedProcesses.Remove(proc.ProcessName);
                        }
                    }
                    else
                    {
                        if (IsIconic(hWnd)) ShowWindow(hWnd, SW_RESTORE);
                        SetForegroundWindow(hWnd);

                        if (currentlyMutedProcesses.Contains(proc.ProcessName))
                        {
                            AudioManager.SetProcessMute(proc.ProcessName, 0, false);
                            currentlyMutedProcesses.Remove(proc.ProcessName);
                        }
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

            RestoreAllMutedProcesses();
        }

        private void RestoreAllMutedProcesses()
        {
            foreach (var proc in currentlyMutedProcesses)
            {
                try
                {
                    AudioManager.SetProcessMute(proc, 0, false);
                }
                catch { }
            }
            currentlyMutedProcesses.Clear();
        }

        private void ExitApplication()
        {
            if (hotkeyListener != null)
            {
                hotkeyListener.Dispose();
                hotkeyListener = null;
            }
            RestoreAllHiddenWindows();
            RestoreAllMutedProcesses();
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
            }
            else
            {
                if (hotkeyListener != null)
                {
                    hotkeyListener.Dispose();
                    hotkeyListener = null;
                }
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
                if (enable) rk.SetValue(appName, "\"" + Application.ExecutablePath + "\" -startup");
                else rk.DeleteValue(appName, false);
            } catch { }
        }

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                MainForm mf = new MainForm();
                if (args.Length > 0 && args[0] == "-startup")
                {
                    mf.IsStartupLaunch = true;
                }
                Application.Run(mf);
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_error.log"), ex.ToString());
            }
        }
    }
}
