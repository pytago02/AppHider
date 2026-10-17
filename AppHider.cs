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
        public static string ScheduleDaysLabel { get { return IsEng ? "Days:" : "Thứ:"; } } 
        public static string CustomScheduleBtn { get { return IsEng ? "Custom" : "Tùy chỉnh"; } }
        public static string CustomScheduleActive { get { return IsEng ? "⚡ Per-day schedule active" : "⚡ Lịch riêng từng ngày đang bật"; } }
        public static string CustomScheduleDialogTitle { get { return IsEng ? "Per-Day Schedule Settings" : "Cài đặt lịch hoạt động theo thứ"; } }
        public static string CustomScheduleDialogSub { get { return IsEng ? "Set active days and custom operating hours for each day of the week." : "Chọn các thứ hoạt động và tùy chỉnh khung giờ riêng cho từng ngày."; } }
        public static string CustomScheduleEnableToggle { get { return IsEng ? "Enable separate hours for each day" : "Áp dụng khung giờ riêng cho từng ngày"; } }
        public static string CustomScheduleEnableToggleSub { get { return IsEng ? "When disabled, all selected days use the main schedule hours" : "Khi tắt, tất cả các thứ được chọn sẽ dùng chung khung giờ ở bảng cài đặt"; } }
        public static string PresetWorkdays { get { return IsEng ? "Mon - Fri" : "Thứ 2 - Thứ 6"; } }
        public static string PresetWorkdaysSat { get { return IsEng ? "Mon - Sat" : "Thứ 2 - Thứ 7"; } }
        public static string PresetAllWeek { get { return IsEng ? "All Week" : "Cả tuần"; } }
        public static string PresetCopyMon { get { return IsEng ? "Copy Mon hours to all" : "Sao chép giờ T2 cho tất cả"; } }
        public static string DialogSave { get { return IsEng ? "Save" : "Lưu"; } }
        public static string DialogCancel { get { return IsEng ? "Cancel" : "Hủy"; } }

        public static string[] ShortDaysVi = new string[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
        public static string[] ShortDaysEng = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        public static string[] FullDaysVi = new string[] { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
        public static string[] FullDaysEng = new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
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
        
        public static string[] Modes { get { return IsEng ? new string[] { "Normal mode", "Hide application", "Hide from taskbar" } : new string[] { "Chế độ bình thường", "Chế độ: Ẩn ứng dụng", "Chế độ: Ẩn taskbar" }; } }
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

        public static Font FontTitle = new Font("Segoe UI Semibold", 13.5F);
        public static Font FontSubtitle = new Font("Segoe UI Semibold", 9.75F);
        public static Font FontBody = new Font("Segoe UI", 9.25F);
        public static Font FontBodySmall = new Font("Segoe UI", 8.25F);
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

    public class DaySchedule
    {
        public DayOfWeek Day;
        public bool Enabled;
        public TimeSpan From;
        public TimeSpan To;

        public DaySchedule(DayOfWeek day, bool enabled, TimeSpan from, TimeSpan to)
        {
            this.Day = day;
            this.Enabled = enabled;
            this.From = from;
            this.To = to;
        }

        public DaySchedule Clone()
        {
            return new DaySchedule(this.Day, this.Enabled, this.From, this.To);
        }
    }

    public class DayPillButton : Control
    {
        private bool _selected = false;
        public DayOfWeek Day { get; set; }
        public string DisplayText { get; set; }

        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; Invalidate(); }
        }

        public event EventHandler SelectedChanged;

        public DayPillButton(DayOfWeek day, string text, bool selected)
        {
            this.Day = day;
            this.DisplayText = text;
            this._selected = selected;
            this.Size = new Size(30, 24);
            this.DoubleBuffered = true;
            this.Cursor = Cursors.Hand;
        }

        protected override void OnClick(EventArgs e)
        {
            if (!this.Enabled) return;
            Selected = !Selected;
            if (SelectedChanged != null) SelectedChanged(this, EventArgs.Empty);
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color parentBg = Parent != null ? Parent.BackColor : Theme.Bg;
            e.Graphics.Clear(parentBg);

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (GraphicsPath path = GraphicsExt.GetRoundedRect(rect, 5))
            {
                if (!this.Enabled)
                {
                    using (SolidBrush br = new SolidBrush(Theme.Card))
                        e.Graphics.FillPath(br, path);
                    using (Pen p = new Pen(Theme.Stroke, 1f))
                        e.Graphics.DrawPath(p, path);
                    using (SolidBrush textBr = new SolidBrush(Color.FromArgb(120, Theme.TextSecondary)))
                    {
                        StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString(DisplayText, Theme.FontBodySmall, textBr, rect, sf);
                    }
                    return;
                }

                if (Selected)
                {
                    using (SolidBrush br = new SolidBrush(Theme.Accent))
                        e.Graphics.FillPath(br, path);
                    using (SolidBrush textBr = new SolidBrush(Theme.OnAccent))
                    {
                        StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        using (Font boldFont = new Font(Theme.FontBodySmall.FontFamily, Theme.FontBodySmall.Size, FontStyle.Bold))
                        {
                            e.Graphics.DrawString(DisplayText, boldFont, textBr, rect, sf);
                        }
                    }
                }
                else
                {
                    using (SolidBrush br = new SolidBrush(Theme.ControlFill))
                        e.Graphics.FillPath(br, path);
                    using (Pen p = new Pen(Theme.Stroke, 1f))
                        e.Graphics.DrawPath(p, path);
                    using (SolidBrush textBr = new SolidBrush(Theme.TextSecondary))
                    {
                        StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString(DisplayText, Theme.FontBodySmall, textBr, rect, sf);
                    }
                }
            }
        }
    }

    public class DarkTimePicker : Control
    {
        private DateTime _value = DateTime.Today.AddHours(8).AddMinutes(30);
        private bool _selectMinute = false;
        private int _hoverPart = 0; // 0=none, 1=up, 2=down

        public event EventHandler ValueChanged;

        public DateTime Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    Invalidate();
                    if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
                }
            }
        }

        public TimeSpan TimeOfDay
        {
            get { return _value.TimeOfDay; }
            set { Value = _value.Date.Add(value); }
        }

        // Compatibility properties for DateTimePicker
        public DateTimePickerFormat Format { get; set; }
        public string CustomFormat { get; set; }
        public bool ShowUpDown { get; set; }
        public Color CalendarForeColor { get; set; }
        public Color CalendarMonthBackground { get; set; }
        public Color CalendarTitleBackColor { get; set; }

        public DarkTimePicker()
        {
            this.Size = new Size(66, 26);
            this.DoubleBuffered = true;
            this.Cursor = Cursors.Hand;
            this.Font = Theme.FontBody;
            this.SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.TabStop = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.Focus();
            int btnWidth = 16;
            if (e.X >= this.Width - btnWidth)
            {
                if (e.Y < this.Height / 2) AdjustTime(1);
                else AdjustTime(-1);
            }
            else
            {
                _selectMinute = (e.X > (this.Width - btnWidth) / 2);
                _digitStep = 0;
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int btnWidth = 16;
            int oldHover = _hoverPart;
            if (e.X >= this.Width - btnWidth)
            {
                _hoverPart = (e.Y < this.Height / 2) ? 1 : 2;
            }
            else
            {
                _hoverPart = 0;
            }
            if (oldHover != _hoverPart) Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hoverPart = 0;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0) AdjustTime(1);
            else if (e.Delta < 0) AdjustTime(-1);
            base.OnMouseWheel(e);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Up) { AdjustTime(1); return true; }
            if (keyData == Keys.Down) { AdjustTime(-1); return true; }
            if (keyData == Keys.Left) { _selectMinute = false; _digitStep = 0; Invalidate(); return true; }
            if (keyData == Keys.Right) { _selectMinute = true; _digitStep = 0; Invalidate(); return true; }
            return base.ProcessDialogKey(keyData);
        }

        private int _digitStep = 0;
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!this.Enabled) return;
            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
            {
                ApplyDigit(e.KeyCode - Keys.D0);
                e.Handled = true;
                return;
            }
            if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
            {
                ApplyDigit(e.KeyCode - Keys.NumPad0);
                e.Handled = true;
                return;
            }
            base.OnKeyDown(e);
        }

        private void ApplyDigit(int digit)
        {
            if (_selectMinute)
            {
                int current = _value.Minute;
                int next = (_digitStep == 0) ? digit * 10 : (current / 10) * 10 + digit;
                if (next >= 60) next = 59;
                _digitStep = (_digitStep == 0) ? 1 : 0;
                Value = new DateTime(_value.Year, _value.Month, _value.Day, _value.Hour, next, 0);
            }
            else
            {
                int current = _value.Hour;
                int next = (_digitStep == 0) ? digit * 10 : (current / 10) * 10 + digit;
                if (next >= 24) next = 23;
                _digitStep = (_digitStep == 0) ? 1 : 0;
                Value = new DateTime(_value.Year, _value.Month, _value.Day, next, _value.Minute, 0);
            }
        }

        private void AdjustTime(int delta)
        {
            if (!this.Enabled) return;
            _digitStep = 0;
            if (_selectMinute)
            {
                int m = _value.Minute + delta * 5;
                if (m < 0) m = 55;
                else if (m >= 60) m = 0;
                Value = new DateTime(_value.Year, _value.Month, _value.Day, _value.Hour, m, 0);
            }
            else
            {
                int h = _value.Hour + delta;
                if (h < 0) h = 23;
                else if (h >= 24) h = 0;
                Value = new DateTime(_value.Year, _value.Month, _value.Day, h, _value.Minute, 0);
            }
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); _digitStep = 0; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color parentBg = this.Parent != null ? this.Parent.BackColor : Theme.Bg;
            e.Graphics.Clear(parentBg);

            Color bg = this.Enabled ? Theme.ControlFill : Theme.Card;
            Color fg = this.Enabled ? Theme.TextPrimary : Theme.TextSecondary;
            Color stroke = this.Focused ? Theme.Accent : Theme.Stroke;

            Rectangle r = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (GraphicsPath gp = GraphicsExt.GetRoundedRect(r, 4))
            {
                using (SolidBrush b = new SolidBrush(bg)) e.Graphics.FillPath(b, gp);
                using (Pen p = new Pen(stroke, 1f)) e.Graphics.DrawPath(p, gp);
            }

            int btnW = 16;
            int btnX = this.Width - btnW;
            using (Pen p = new Pen(Theme.Stroke, 1f))
            {
                e.Graphics.DrawLine(p, btnX, 1, btnX, this.Height - 2);
                e.Graphics.DrawLine(p, btnX, this.Height / 2, this.Width - 2, this.Height / 2);
            }

            if (_hoverPart == 1)
            {
                using (SolidBrush b = new SolidBrush(Theme.ControlFillHover))
                    e.Graphics.FillRectangle(b, btnX + 1, 1, btnW - 2, this.Height / 2 - 1);
            }
            else if (_hoverPart == 2)
            {
                using (SolidBrush b = new SolidBrush(Theme.ControlFillHover))
                    e.Graphics.FillRectangle(b, btnX + 1, this.Height / 2 + 1, btnW - 2, this.Height / 2 - 2);
            }

            using (SolidBrush br = new SolidBrush(fg))
            {
                Point[] upArrow = new Point[] {
                    new Point(btnX + 4, this.Height / 4 + 2),
                    new Point(btnX + 8, this.Height / 4 - 2),
                    new Point(btnX + 12, this.Height / 4 + 2)
                };
                e.Graphics.FillPolygon(br, upArrow);

                Point[] dnArrow = new Point[] {
                    new Point(btnX + 4, this.Height * 3 / 4 - 2),
                    new Point(btnX + 8, this.Height * 3 / 4 + 2),
                    new Point(btnX + 12, this.Height * 3 / 4 - 2)
                };
                e.Graphics.FillPolygon(br, dnArrow);
            }

            string hStr = _value.ToString("HH");
            string mStr = _value.ToString("mm");
            int textW = btnX;
            Rectangle textRect = new Rectangle(0, 0, textW, this.Height);
            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                using (SolidBrush br = new SolidBrush(fg))
                {
                    e.Graphics.DrawString(hStr + ":" + mStr, this.Font, br, textRect, sf);
                }
            }

            if (this.Focused)
            {
                using (Pen accentPen = new Pen(Theme.Accent, 1.5f))
                {
                    int lineY = this.Height - 3;
                    if (_selectMinute)
                        e.Graphics.DrawLine(accentPen, textW / 2 + 2, lineY, textW - 4, lineY);
                    else
                        e.Graphics.DrawLine(accentPen, 4, lineY, textW / 2 - 2, lineY);
                }
            }
        }
    }

    public class NoScrollComboBox : ComboBox
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_PAINT = 0x000F;

        public NoScrollComboBox()
        {
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.DropDownStyle = ComboBoxStyle.DropDownList;
            this.FlatStyle = FlatStyle.Flat;
            this.DoubleBuffered = true;
            this.ItemHeight = 22;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try
            {
                SetWindowTheme(this.Handle, Theme.IsDark ? "DarkMode_CFD" : "CFD", null);
            }
            catch { }
        }

        public void ApplyTheme()
        {
            this.BackColor = Theme.ControlFill;
            this.ForeColor = Theme.TextPrimary;
            try
            {
                if (this.IsHandleCreated)
                    SetWindowTheme(this.Handle, Theme.IsDark ? "DarkMode_CFD" : "CFD", null);
            }
            catch { }
            this.Invalidate();
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); Invalidate(); }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            bool isComboEdit = (e.State & DrawItemState.ComboBoxEdit) != 0;
            bool selected = (e.State & DrawItemState.Selected) != 0 && !isComboEdit;
            Color bg = selected ? Theme.Accent : this.BackColor;
            Color fg = selected ? Theme.OnAccent : this.ForeColor;

            using (SolidBrush b = new SolidBrush(bg))
                e.Graphics.FillRectangle(b, e.Bounds);

            using (SolidBrush b = new SolidBrush(fg))
            {
                string text = this.Items[e.Index].ToString();
                StringFormat sf = new StringFormat()
                {
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                Rectangle r = new Rectangle(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height);
                e.Graphics.DrawString(text, this.Font, b, r, sf);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_MOUSEWHEEL)
            {
                if (!this.DroppedDown)
                {
                    Control p = this.Parent;
                    while (p != null)
                    {
                        ScrollableControl sc = p as ScrollableControl;
                        if (sc != null && sc.AutoScroll)
                        {
                            SendMessage(sc.Handle, m.Msg, m.WParam, m.LParam);
                            break;
                        }
                        p = p.Parent;
                    }
                    return;
                }
            }

            base.WndProc(ref m);

            if (m.Msg == WM_PAINT)
            {
                using (Graphics g = Graphics.FromHwnd(this.Handle))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    int btnW = 22;
                    Rectangle btnRect = new Rectangle(this.Width - btnW, 1, btnW - 1, this.Height - 2);
                    using (SolidBrush b = new SolidBrush(this.BackColor))
                        g.FillRectangle(b, btnRect);

                    int arrowX = btnRect.X + (btnRect.Width - 8) / 2;
                    int arrowY = (this.Height - 4) / 2;
                    using (Pen p = new Pen(this.ForeColor, 1.4f))
                    {
                        p.StartCap = LineCap.Round;
                        p.EndCap = LineCap.Round;
                        p.LineJoin = LineJoin.Round;
                        g.DrawLines(p, new Point[] {
                            new Point(arrowX, arrowY),
                            new Point(arrowX + 4, arrowY + 4),
                            new Point(arrowX + 8, arrowY)
                        });
                    }

                    Rectangle borderRect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                    using (Pen p = new Pen(this.Focused ? Theme.Accent : Theme.Stroke, 1f))
                        g.DrawRectangle(p, borderRect);
                }
            }
        }
    }

    public class CustomScheduleForm : Form
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [DllImport("uxtheme.dll", EntryPoint = "#133", ExactSpelling = true)]
        private static extern bool AllowDarkModeForWindow(IntPtr hWnd, bool allow);

        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        private MainForm _mainForm;
        private FluentToggle customToggle;
        private Label lblTitle, lblSub, lblToggle, lblToggleSub;
        private Panel togglePanel;
        private Panel daysPanel;
        private Button btnPresetWorkdays, btnPresetSat, btnPresetAll, btnCopyMon;
        private Button btnSave, btnCancel;

        private Dictionary<DayOfWeek, CheckBox> chkDays = new Dictionary<DayOfWeek, CheckBox>();
        private Dictionary<DayOfWeek, Label> lblsFrom = new Dictionary<DayOfWeek, Label>();
        private Dictionary<DayOfWeek, DarkTimePicker> dtpsFrom = new Dictionary<DayOfWeek, DarkTimePicker>();
        private Dictionary<DayOfWeek, Label> lblsTo = new Dictionary<DayOfWeek, Label>();
        private Dictionary<DayOfWeek, DarkTimePicker> dtpsTo = new Dictionary<DayOfWeek, DarkTimePicker>();

        public CustomScheduleForm(MainForm mainForm)
        {
            this._mainForm = mainForm;
            this.Text = Lang.CustomScheduleDialogTitle;
            this.ClientSize = new Size(520, 510);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.DoubleBuffered = true;
            this.Font = Theme.FontBody;
            this.BackColor = Theme.Bg;
            this.ForeColor = Theme.TextPrimary;

            int useImmersiveDarkMode = Theme.IsDark ? 1 : 0;
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int));
            try { AllowDarkModeForWindow(this.Handle, true); } catch { }

            InitFormUI();
            LoadData();
        }

        private void InitFormUI()
        {
            lblTitle = new Label() {
                Text = Lang.CustomScheduleDialogTitle,
                Font = Theme.FontTitle,
                ForeColor = Theme.TextPrimary,
                Location = new Point(20, 16),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            lblSub = new Label() {
                Text = Lang.CustomScheduleDialogSub,
                Font = Theme.FontBodySmall,
                ForeColor = Theme.TextSecondary,
                Location = new Point(20, 44),
                AutoSize = true
            };
            this.Controls.Add(lblSub);

            togglePanel = new Panel() {
                Location = new Point(20, 70),
                Size = new Size(this.ClientSize.Width - 40, 50),
                BackColor = Theme.Card
            };
            togglePanel.Paint += delegate(object s, PaintEventArgs pe) {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, togglePanel.Width - 1, togglePanel.Height - 1);
                using (GraphicsPath gp = GraphicsExt.GetRoundedRect(r, 6))
                {
                    using (Pen p = new Pen(Theme.Stroke, 1f)) pe.Graphics.DrawPath(p, gp);
                }
            };
            this.Controls.Add(togglePanel);

            lblToggle = new Label() {
                Text = Lang.CustomScheduleEnableToggle,
                Font = Theme.FontBody,
                ForeColor = Theme.TextPrimary,
                Location = new Point(14, 8),
                AutoSize = true
            };
            togglePanel.Controls.Add(lblToggle);

            lblToggleSub = new Label() {
                Text = Lang.CustomScheduleEnableToggleSub,
                Font = Theme.FontBodySmall,
                ForeColor = Theme.TextSecondary,
                Location = new Point(14, 26),
                AutoSize = true
            };
            togglePanel.Controls.Add(lblToggleSub);

            customToggle = new FluentToggle();
            customToggle.Location = new Point(togglePanel.Width - 54, 15);
            customToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            customToggle.CheckedChanged += delegate {
                UpdateControlsState();
            };
            togglePanel.Controls.Add(customToggle);

            int btnY = 130;
            btnPresetWorkdays = CreatePresetButton(Lang.PresetWorkdays, 20, btnY, 100, delegate {
                SetPresetDays(true, true, true, true, true, false, false);
            });
            this.Controls.Add(btnPresetWorkdays);

            btnPresetSat = CreatePresetButton(Lang.PresetWorkdaysSat, 126, btnY, 100, delegate {
                SetPresetDays(true, true, true, true, true, true, false);
            });
            this.Controls.Add(btnPresetSat);

            btnPresetAll = CreatePresetButton(Lang.PresetAllWeek, 232, btnY, 90, delegate {
                SetPresetDays(true, true, true, true, true, true, true);
            });
            this.Controls.Add(btnPresetAll);

            btnCopyMon = CreatePresetButton(Lang.PresetCopyMon, 328, btnY, 172, delegate {
                CopyMonHoursToAll();
            });
            this.Controls.Add(btnCopyMon);

            daysPanel = new Panel() {
                Location = new Point(20, 164),
                Size = new Size(this.ClientSize.Width - 40, 282),
                BackColor = Theme.Card
            };
            daysPanel.HandleCreated += delegate {
                try {
                    AllowDarkModeForWindow(daysPanel.Handle, true);
                    SetWindowTheme(daysPanel.Handle, Theme.IsDark ? "DarkMode_Explorer" : "Explorer", null);
                } catch { }
            };
            daysPanel.Paint += delegate(object s, PaintEventArgs pe) {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, daysPanel.Width - 1, daysPanel.Height - 1);
                using (GraphicsPath gp = GraphicsExt.GetRoundedRect(r, 6))
                {
                    using (Pen p = new Pen(Theme.Stroke, 1f)) pe.Graphics.DrawPath(p, gp);
                }
            };
            this.Controls.Add(daysPanel);

            for (int i = 0; i < MainForm.OrderedDaysOfWeek.Length; i++)
            {
                DayOfWeek d = MainForm.OrderedDaysOfWeek[i];
                int rowY = 8 + i * 38;

                CheckBox chk = new CheckBox();
                chk.Text = Lang.IsEng ? Lang.FullDaysEng[i] : Lang.FullDaysVi[i];
                chk.Font = Theme.FontBody;
                chk.ForeColor = Theme.TextPrimary;
                chk.Location = new Point(14, rowY + 3);
                chk.Size = new Size(114, 24);
                chk.Cursor = Cursors.Hand;
                chk.CheckedChanged += delegate { UpdateControlsState(); };
                daysPanel.Controls.Add(chk);
                chkDays[d] = chk;

                Label lblF = new Label() {
                    Text = Lang.ScheduleFrom,
                    Font = Theme.FontBodySmall,
                    ForeColor = Theme.TextSecondary,
                    Location = new Point(134, rowY + 6),
                    AutoSize = true
                };
                daysPanel.Controls.Add(lblF);
                lblsFrom[d] = lblF;

                DarkTimePicker dtpF = new DarkTimePicker();
                dtpF.Location = new Point(172, rowY + 3);
                dtpF.Width = 66;
                daysPanel.Controls.Add(dtpF);
                dtpsFrom[d] = dtpF;

                Label lblT = new Label() {
                    Text = Lang.ScheduleTo,
                    Font = Theme.FontBodySmall,
                    ForeColor = Theme.TextSecondary,
                    Location = new Point(246, rowY + 6),
                    AutoSize = true
                };
                daysPanel.Controls.Add(lblT);
                lblsTo[d] = lblT;

                DarkTimePicker dtpT = new DarkTimePicker();
                dtpT.Location = new Point(286, rowY + 3);
                dtpT.Width = 66;
                daysPanel.Controls.Add(dtpT);
                dtpsTo[d] = dtpT;
            }

            btnCancel = new Button() {
                Text = Lang.DialogCancel,
                Location = new Point(this.ClientSize.Width - 20 - 90, this.ClientSize.Height - 48),
                Size = new Size(90, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.ControlFill,
                ForeColor = Theme.TextPrimary,
                Font = Theme.FontBody,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Theme.Stroke;
            btnCancel.Click += delegate {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            this.Controls.Add(btnCancel);

            btnSave = new Button() {
                Text = Lang.DialogSave,
                Location = new Point(this.ClientSize.Width - 20 - 90 - 10 - 90, this.ClientSize.Height - 48),
                Size = new Size(90, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.Accent,
                ForeColor = Theme.OnAccent,
                Font = new Font("Segoe UI Semibold", 9.5F),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += SaveAndClose;
            this.Controls.Add(btnSave);
        }

        private Button CreatePresetButton(string text, int x, int y, int width, EventHandler onClick)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, 26);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Theme.Stroke;
            btn.BackColor = Theme.ControlFill;
            btn.ForeColor = Theme.TextPrimary;
            btn.Font = Theme.FontBodySmall;
            btn.Cursor = Cursors.Hand;
            btn.Click += onClick;
            return btn;
        }

        private void SetPresetDays(bool mon, bool tue, bool wed, bool thu, bool fri, bool sat, bool sun)
        {
            bool[] states = new bool[] { mon, tue, wed, thu, fri, sat, sun };
            for (int i = 0; i < MainForm.OrderedDaysOfWeek.Length; i++)
            {
                DayOfWeek d = MainForm.OrderedDaysOfWeek[i];
                if (chkDays.ContainsKey(d)) chkDays[d].Checked = states[i];
            }
            UpdateControlsState();
        }

        private void CopyMonHoursToAll()
        {
            DayOfWeek mon = DayOfWeek.Monday;
            if (!dtpsFrom.ContainsKey(mon) || !dtpsTo.ContainsKey(mon)) return;
            DateTime fromVal = dtpsFrom[mon].Value;
            DateTime toVal = dtpsTo[mon].Value;

            for (int i = 1; i < MainForm.OrderedDaysOfWeek.Length; i++)
            {
                DayOfWeek d = MainForm.OrderedDaysOfWeek[i];
                if (dtpsFrom.ContainsKey(d)) dtpsFrom[d].Value = fromVal;
                if (dtpsTo.ContainsKey(d)) dtpsTo[d].Value = toVal;
            }
        }

        private void LoadData()
        {
            customToggle.Checked = _mainForm.ScheduleCustomPerDay;
            DateTime today = DateTime.Today;

            for (int i = 0; i < MainForm.OrderedDaysOfWeek.Length; i++)
            {
                DayOfWeek d = MainForm.OrderedDaysOfWeek[i];
                DaySchedule ds;
                if (_mainForm.CustomDaySchedules.TryGetValue(d, out ds))
                {
                    chkDays[d].Checked = ds.Enabled;
                    dtpsFrom[d].Value = today.Add(ds.From);
                    dtpsTo[d].Value = today.Add(ds.To);
                }
                else
                {
                    bool isSel = _mainForm.ScheduleDays.Contains(d);
                    chkDays[d].Checked = isSel;
                    dtpsFrom[d].Value = today.Add(_mainForm.ScheduleFrom);
                    dtpsTo[d].Value = today.Add(_mainForm.ScheduleTo);
                }
            }

            UpdateControlsState();
        }

        private void UpdateControlsState()
        {
            bool isCustom = customToggle.Checked;
            DateTime today = DateTime.Today;

            foreach (DayOfWeek d in MainForm.OrderedDaysOfWeek)
            {
                bool dayActive = chkDays[d].Checked;
                bool enablePickers = isCustom && dayActive;

                dtpsFrom[d].Enabled = enablePickers;
                dtpsTo[d].Enabled = enablePickers;
                lblsFrom[d].Enabled = enablePickers;
                lblsTo[d].Enabled = enablePickers;

                if (!isCustom)
                {
                    dtpsFrom[d].Value = today.Add(_mainForm.ScheduleFrom);
                    dtpsTo[d].Value = today.Add(_mainForm.ScheduleTo);
                }
            }

            btnCopyMon.Enabled = isCustom;
        }

        private void SaveAndClose(object sender, EventArgs e)
        {
            _mainForm.ScheduleCustomPerDay = customToggle.Checked;
            _mainForm.ScheduleDays.Clear();

            foreach (DayOfWeek d in MainForm.OrderedDaysOfWeek)
            {
                bool en = chkDays[d].Checked;
                if (en) _mainForm.ScheduleDays.Add(d);

                TimeSpan f = dtpsFrom[d].Value.TimeOfDay;
                TimeSpan t = dtpsTo[d].Value.TimeOfDay;
                _mainForm.CustomDaySchedules[d] = new DaySchedule(d, en, f, t);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
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

        private NoScrollComboBox modeCombo;

        public AppRowControl(MainForm main, string proc, string title, int mode, bool isRunning)
        {
            this.main = main;
            this.ProcessName = proc;
            this.WindowTitle = title;
            this.Mode = mode;
            this.IsRunning = isRunning;
            this.Height = 52;
            int initialWidth = (main != null && main.listPanel != null && main.listPanel.ClientSize.Width > 0) 
                ? main.listPanel.ClientSize.Width 
                : 560;
            this.Width = initialWidth;
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

            modeCombo = new NoScrollComboBox();
            modeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            modeCombo.Items.AddRange(Lang.Modes);
            modeCombo.SelectedIndex = mode;
            modeCombo.Font = new Font("Segoe UI", 9F);
            modeCombo.Width = 190;
            modeCombo.DropDownWidth = 190;
            modeCombo.Location = new Point(this.Width - 190 - 16, (this.Height - modeCombo.Height) / 2);
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
            modeCombo.ApplyTheme();
            this.BackColor = Theme.Card;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using (Pen p = new Pen(Theme.Stroke))
                e.Graphics.DrawLine(p, 14, 0, this.Width - 14, 0);

            Rectangle iconRect = new Rectangle(14, 11, 30, 30);
            if (AppIcon != null) e.Graphics.DrawIcon(AppIcon, iconRect);
            else e.Graphics.DrawIcon(SystemIcons.Application, iconRect);

            if (!IsRunning)
            {
                using (SolidBrush overlay = new SolidBrush(Color.FromArgb(128, Theme.Card)))
                    e.Graphics.FillRectangle(overlay, iconRect);
            }

            int textLeft = 52;
            int textMaxW = modeCombo.Left - textLeft - 22;
            using (SolidBrush brPrimary = new SolidBrush(IsRunning ? Theme.TextPrimary : Theme.TextSecondary))
            using (SolidBrush brSecondary = new SolidBrush(Theme.TextSecondary))
            {
                if (textMaxW > 40)
                {
                    RectangleF titleRect = new RectangleF(textLeft, 8, textMaxW, 19);
                    RectangleF subRect = new RectangleF(textLeft, 27, textMaxW, 17);
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
                    e.Graphics.DrawString(WindowTitle, Theme.FontBody, brPrimary, new PointF(textLeft, 8));
                    e.Graphics.DrawString(ProcessName + (IsRunning ? "" : " (" + Lang.NotRunning + ")"), Theme.FontBodySmall, brSecondary, new PointF(textLeft, 27));
                }
            }

            Color dotColor = Theme.DotNormal;
            if (Mode == 1) dotColor = Theme.DotBlur;
            else if (Mode == 2) dotColor = Theme.DotTask;

            using (SolidBrush dotBr = new SolidBrush(dotColor))
            {
                e.Graphics.FillEllipse(dotBr, modeCombo.Left - 14, (this.Height - 7) / 2, 7, 7);
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

        [DllImport("uxtheme.dll", EntryPoint = "#135", ExactSpelling = true)]
        public static extern int SetPreferredAppMode(int appMode);

        [DllImport("uxtheme.dll", EntryPoint = "#133", ExactSpelling = true)]
        public static extern bool AllowDarkModeForWindow(IntPtr hWnd, bool allow);

        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, [MarshalAs(UnmanagedType.Bool)] bool bShow);
        private const int SB_HORZ = 0;
        private const int SB_VERT = 1;

        [DllImport("user32.dll")]
        private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);
        private const uint RDW_INVALIDATE = 0x0001;
        private const uint RDW_FRAME = 0x0400;
        private const uint RDW_UPDATENOW = 0x0100;

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

        public FlowLayoutPanel listPanel;
        private Button saveButton;
        private Button refreshButton;
        private FluentToggle startupToggle;
        private FluentToggle showStartToggle;
        private FluentToggle muteToggle;
        private FluentToggle scheduleToggle;
        private DarkTimePicker dtpScheduleFrom, dtpScheduleTo;
        private Label lblScheduleDays;
        private DayPillButton[] dayPillButtons;
        private Button btnCustomSchedule;
        private Label lblCustomIndicator;
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
        private NoScrollComboBox themeCombo;
        private NoScrollComboBox langCombo;
        private TextBox searchBox;
        private ContextMenu contextMenu;

        public HashSet<string> HideOnBlurApps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> HideFromTaskbarApps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public bool MuteOnMinimize = false;
        public bool ScheduleEnabled = false;
        public TimeSpan ScheduleFrom = new TimeSpan(8, 0, 0);
        public TimeSpan ScheduleTo = new TimeSpan(17, 0, 0);
        public static readonly DayOfWeek[] OrderedDaysOfWeek = new DayOfWeek[] {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        };
        public HashSet<DayOfWeek> ScheduleDays = new HashSet<DayOfWeek>() {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday
        };
        public bool ScheduleCustomPerDay = false;
        public Dictionary<DayOfWeek, DaySchedule> CustomDaySchedules = new Dictionary<DayOfWeek, DaySchedule>();

        public uint HotkeyMods = 2; // Default: Ctrl (2)
        public uint HotkeyVK = 123; // Default: F12 (Keys.F12)
        private HotkeyListener hotkeyListener;
        private DateTime quickRestoreCooldownUntil = DateTime.MinValue;

        private HashSet<string> currentlyMutedProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        private Dictionary<IntPtr, NotifyIcon> blurHiddenIcons = new Dictionary<IntPtr, NotifyIcon>();
        private Dictionary<IntPtr, NotifyIcon> taskbarHiddenIcons = new Dictionary<IntPtr, NotifyIcon>();
        
        private string configFile = Path.Combine(Application.StartupPath, "config.ini");
        private string appName = "AppHider";
        
        private ITaskbarList taskbar;

        public bool LoadShowStartSetting()
        {
            try
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
            } catch { }
            return true;
        }

        private void SaveConfigValue(string keyToSave, string valToSave)
        {
            try
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
            } catch { }
        }

        private void SaveConfigValues(Dictionary<string, string> dictToUpdate)
        {
            try
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
            } catch { }
        }

        public bool IsScheduleActive()
        {
            if (!ScheduleEnabled) return true;
            DateTime now = DateTime.Now;
            DayOfWeek today = now.DayOfWeek;
            TimeSpan currentTime = now.TimeOfDay;

            if (ScheduleCustomPerDay)
            {
                DaySchedule ds;
                if (CustomDaySchedules.TryGetValue(today, out ds))
                {
                    if (!ds.Enabled) return false;
                    return IsTimeInRange(currentTime, ds.From, ds.To);
                }
                return false;
            }
            else
            {
                if (!ScheduleDays.Contains(today)) return false;
                return IsTimeInRange(currentTime, ScheduleFrom, ScheduleTo);
            }
        }

        private static bool IsTimeInRange(TimeSpan now, TimeSpan from, TimeSpan to)
        {
            if (from <= to)
            {
                return now >= from && now <= to;
            }
            else
            {
                return now >= from || now <= to;
            }
        }

        private static bool TryParseDayOfWeek(string str, out DayOfWeek dow)
        {
            dow = DayOfWeek.Monday;
            if (string.IsNullOrEmpty(str)) return false;
            str = str.Trim().ToLowerInvariant();
            if (str == "mon" || str == "monday" || str == "1" || str == "t2" || str == "thu2") { dow = DayOfWeek.Monday; return true; }
            if (str == "tue" || str == "tuesday" || str == "2" || str == "t3" || str == "thu3") { dow = DayOfWeek.Tuesday; return true; }
            if (str == "wed" || str == "wednesday" || str == "3" || str == "t4" || str == "thu4") { dow = DayOfWeek.Wednesday; return true; }
            if (str == "thu" || str == "thursday" || str == "4" || str == "t5" || str == "thu5") { dow = DayOfWeek.Thursday; return true; }
            if (str == "fri" || str == "friday" || str == "5" || str == "t6" || str == "thu6") { dow = DayOfWeek.Friday; return true; }
            if (str == "sat" || str == "saturday" || str == "6" || str == "t7" || str == "thu7") { dow = DayOfWeek.Saturday; return true; }
            if (str == "sun" || str == "sunday" || str == "0" || str == "7" || str == "cn") { dow = DayOfWeek.Sunday; return true; }
            return false;
        }

        public static string GetShortDayName(DayOfWeek d)
        {
            switch (d)
            {
                case DayOfWeek.Monday: return "Mon";
                case DayOfWeek.Tuesday: return "Tue";
                case DayOfWeek.Wednesday: return "Wed";
                case DayOfWeek.Thursday: return "Thu";
                case DayOfWeek.Friday: return "Fri";
                case DayOfWeek.Saturday: return "Sat";
                case DayOfWeek.Sunday: return "Sun";
                default: return d.ToString();
            }
        }

        public static List<string> GetDayKeys(IEnumerable<DayOfWeek> days)
        {
            HashSet<DayOfWeek> set = new HashSet<DayOfWeek>(days);
            List<string> list = new List<string>();
            foreach (DayOfWeek d in OrderedDaysOfWeek)
            {
                if (set.Contains(d)) list.Add(GetShortDayName(d));
            }
            return list;
        }

        private void OpenCustomScheduleDialog()
        {
            using (CustomScheduleForm dlg = new CustomScheduleForm(this))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    UpdateScheduleUIState();
                    SaveSettingsToFile();
                }
            }
        }

        public void UpdateScheduleUIState()
        {
            if (dayPillButtons != null)
            {
                for (int i = 0; i < OrderedDaysOfWeek.Length; i++)
                {
                    DayOfWeek d = OrderedDaysOfWeek[i];
                    dayPillButtons[i].DisplayText = Lang.IsEng ? Lang.ShortDaysEng[i] : Lang.ShortDaysVi[i];
                    dayPillButtons[i].Selected = ScheduleDays.Contains(d);
                    dayPillButtons[i].Enabled = ScheduleEnabled;
                    dayPillButtons[i].Invalidate();
                }
            }

            if (lblScheduleDays != null)
            {
                lblScheduleDays.Text = Lang.ScheduleDaysLabel;
                lblScheduleDays.ForeColor = Theme.TextSecondary;
                lblScheduleDays.Enabled = ScheduleEnabled;
            }

            if (btnCustomSchedule != null)
            {
                btnCustomSchedule.Text = Lang.CustomScheduleBtn;
                btnCustomSchedule.Enabled = ScheduleEnabled;
                btnCustomSchedule.BackColor = Theme.ControlFill;
                btnCustomSchedule.ForeColor = Theme.TextPrimary;
                btnCustomSchedule.FlatAppearance.BorderColor = Theme.Stroke;
            }

            if (lblCustomIndicator != null)
            {
                lblCustomIndicator.Text = Lang.CustomScheduleActive;
                lblCustomIndicator.ForeColor = Theme.Accent;
                lblCustomIndicator.Visible = ScheduleEnabled && ScheduleCustomPerDay;
            }

            bool enableGlobalTimes = ScheduleEnabled && !ScheduleCustomPerDay;
            if (dtpScheduleFrom != null) dtpScheduleFrom.Enabled = enableGlobalTimes;
            if (dtpScheduleTo != null) dtpScheduleTo.Enabled = enableGlobalTimes;
            if (lblScheduleFrom != null) lblScheduleFrom.Enabled = enableGlobalTimes;
            if (lblScheduleTo != null) lblScheduleTo.Enabled = enableGlobalTimes;
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
            this.Size = new Size(620, 875);
            this.MinimumSize = new Size(560, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = Theme.FontBody;
            this.DoubleBuffered = true;

            taskbar = (ITaskbarList)new TaskbarListClass();
            taskbar.HrInit();

            foreach (DayOfWeek d in OrderedDaysOfWeek)
            {
                bool isWeekday = (d != DayOfWeek.Saturday && d != DayOfWeek.Sunday);
                CustomDaySchedules[d] = new DaySchedule(d, isWeekday, ScheduleFrom, ScheduleTo);
            }

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
            titleLabel = new Label() { Font = Theme.FontTitle, AutoSize = true, Location = new Point(20, 18) };
            subTitleLabel = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(20, 48) };
            this.Controls.Add(titleLabel);
            this.Controls.Add(subTitleLabel);

            searchBox = new TextBox();
            searchBox.Font = new Font("Segoe UI", 9.5F);
            searchBox.Location = new Point(20, 78);
            searchBox.Size = new Size(this.ClientSize.Width - 115, 26);
            searchBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            searchBox.BorderStyle = BorderStyle.FixedSingle;
            searchBox.TextChanged += SearchBox_TextChanged;
            this.Controls.Add(searchBox);

            refreshButton = new Button();
            refreshButton.Size = new Size(85, 26);
            refreshButton.Location = new Point(this.ClientSize.Width - 95, 77);
            refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refreshButton.FlatStyle = FlatStyle.Flat;
            refreshButton.Font = Theme.FontBodySmall;
            refreshButton.Cursor = Cursors.Hand;
            refreshButton.Click += RefreshButton_Click;
            this.Controls.Add(refreshButton);

            listPanel = new FlowLayoutPanel();
            listPanel.Location = new Point(20, 114);
            listPanel.Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 624);
            listPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            listPanel.AutoScroll = true;
            listPanel.HorizontalScroll.Maximum = 0;
            listPanel.HorizontalScroll.Enabled = false;
            listPanel.HorizontalScroll.Visible = false;
            listPanel.FlowDirection = FlowDirection.TopDown;
            listPanel.WrapContents = false;
            this.Controls.Add(listPanel);

            listPanel.HandleCreated += delegate {
                try {
                    AllowDarkModeForWindow(listPanel.Handle, Theme.IsDark);
                    SetWindowTheme(listPanel.Handle, Theme.IsDark ? "DarkMode_Explorer" : "Explorer", null);
                    ShowScrollBar(listPanel.Handle, SB_HORZ, false);
                } catch { }
            };

            listPanel.Layout += delegate {
                try {
                    listPanel.HorizontalScroll.Maximum = 0;
                    listPanel.HorizontalScroll.Visible = false;
                    ShowScrollBar(listPanel.Handle, SB_HORZ, false);
                    int targetW = listPanel.ClientSize.Width;
                    foreach (Control c in listPanel.Controls) {
                        if (c.Width != targetW) c.Width = targetW;
                    }
                } catch { }
            };

            listPanel.SizeChanged += delegate {
                try {
                    listPanel.HorizontalScroll.Maximum = 0;
                    listPanel.HorizontalScroll.Visible = false;
                    ShowScrollBar(listPanel.Handle, SB_HORZ, false);
                    int targetW = listPanel.ClientSize.Width;
                    foreach (Control c in listPanel.Controls) {
                        if (c.Width != targetW) c.Width = targetW;
                    }
                } catch { }
            };

            lblSet = new Label() { Font = Theme.FontSubtitle, AutoSize = true };
            lblSet.Location = new Point(20, this.ClientSize.Height - 495);
            lblSet.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.Controls.Add(lblSet);

            Panel setPanel = new Panel();
            setPanel.Location = new Point(20, this.ClientSize.Height - 465);
            setPanel.Size = new Size(this.ClientSize.Width - 40, 388);
            setPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(setPanel);

            lblStart = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(14, 16) };
            lblStartSub = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(14, 38) };
            setPanel.Controls.Add(lblStart);
            setPanel.Controls.Add(lblStartSub);

            startupToggle = new FluentToggle();
            startupToggle.Location = new Point(setPanel.Width - 54, 20);
            startupToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            startupToggle.Checked = IsStartupEnabled();
            startupToggle.CheckedChanged += delegate { startupLabel.Text = startupToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt"); };
            setPanel.Controls.Add(startupToggle);

            startupLabel = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(setPanel.Width - 96, 21) };
            startupLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            startupLabel.Text = startupToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
            setPanel.Controls.Add(startupLabel);

            lblShowStart = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(14, 58) };
            lblShowStartSub = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(14, 80) };
            setPanel.Controls.Add(lblShowStart);
            setPanel.Controls.Add(lblShowStartSub);

            showStartToggle = new FluentToggle();
            showStartToggle.Location = new Point(setPanel.Width - 54, 62);
            showStartToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            showStartToggle.Checked = LoadShowStartSetting();
            showStartToggle.CheckedChanged += delegate { 
                showStartLabel.Text = showStartToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt"); 
                SaveConfigValue("ShowStart", showStartToggle.Checked.ToString());
            };
            setPanel.Controls.Add(showStartToggle);

            showStartLabel = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(setPanel.Width - 96, 63) };
            showStartLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            showStartLabel.Text = showStartToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
            setPanel.Controls.Add(showStartLabel);

            lblMute = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(14, 100) };
            lblMuteSub = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(14, 122) };
            setPanel.Controls.Add(lblMute);
            setPanel.Controls.Add(lblMuteSub);

            muteToggle = new FluentToggle();
            muteToggle.Location = new Point(setPanel.Width - 54, 104);
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

            muteLabel = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(setPanel.Width - 96, 105) };
            muteLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            muteLabel.Text = muteToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
            setPanel.Controls.Add(muteLabel);

            lblSchedule = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(14, 144) };
            lblScheduleSub = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(14, 166) };
            setPanel.Controls.Add(lblSchedule);
            setPanel.Controls.Add(lblScheduleSub);

            scheduleToggle = new FluentToggle();
            scheduleToggle.Location = new Point(setPanel.Width - 54, 148);
            scheduleToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            scheduleToggle.Checked = ScheduleEnabled;
            scheduleToggle.CheckedChanged += delegate {
                ScheduleEnabled = scheduleToggle.Checked;
                scheduleLabel.Text = scheduleToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
                UpdateScheduleUIState();
                SaveConfigValue("ScheduleEnabled", ScheduleEnabled.ToString());
            };
            setPanel.Controls.Add(scheduleToggle);

            scheduleLabel = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(setPanel.Width - 96, 149) };
            scheduleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            scheduleLabel.Text = scheduleToggle.Checked ? (Lang.IsEng ? "On" : "Bật") : (Lang.IsEng ? "Off" : "Tắt");
            setPanel.Controls.Add(scheduleLabel);

            lblScheduleFrom = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(14, 191), Enabled = ScheduleEnabled };
            setPanel.Controls.Add(lblScheduleFrom);

            DateTime today = DateTime.Today;

            dtpScheduleFrom = new DarkTimePicker();
            dtpScheduleFrom.Location = new Point(48, 187);
            dtpScheduleFrom.Size = new Size(66, 26);
            dtpScheduleFrom.Value = today.Add(ScheduleFrom);
            dtpScheduleFrom.Enabled = ScheduleEnabled && !ScheduleCustomPerDay;
            dtpScheduleFrom.ValueChanged += delegate {
                ScheduleFrom = dtpScheduleFrom.Value.TimeOfDay;
                SaveConfigValue("ScheduleFrom", ScheduleFrom.ToString(@"hh\:mm"));
                if (!ScheduleCustomPerDay)
                {
                    foreach (DayOfWeek d in OrderedDaysOfWeek)
                    {
                        if (CustomDaySchedules.ContainsKey(d)) CustomDaySchedules[d].From = ScheduleFrom;
                    }
                }
            };
            setPanel.Controls.Add(dtpScheduleFrom);

            lblScheduleTo = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(118, 191), Enabled = ScheduleEnabled };
            setPanel.Controls.Add(lblScheduleTo);

            dtpScheduleTo = new DarkTimePicker();
            dtpScheduleTo.Location = new Point(152, 187);
            dtpScheduleTo.Size = new Size(66, 26);
            dtpScheduleTo.Value = today.Add(ScheduleTo);
            dtpScheduleTo.Enabled = ScheduleEnabled && !ScheduleCustomPerDay;
            dtpScheduleTo.ValueChanged += delegate {
                ScheduleTo = dtpScheduleTo.Value.TimeOfDay;
                SaveConfigValue("ScheduleTo", ScheduleTo.ToString(@"hh\:mm"));
                if (!ScheduleCustomPerDay)
                {
                    foreach (DayOfWeek d in OrderedDaysOfWeek)
                    {
                        if (CustomDaySchedules.ContainsKey(d)) CustomDaySchedules[d].To = ScheduleTo;
                    }
                }
            };
            setPanel.Controls.Add(dtpScheduleTo);

            lblScheduleDays = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(14, 225), Enabled = ScheduleEnabled, Visible = true };
            setPanel.Controls.Add(lblScheduleDays);

            dayPillButtons = new DayPillButton[OrderedDaysOfWeek.Length];
            for (int i = 0; i < OrderedDaysOfWeek.Length; i++)
            {
                DayOfWeek d = OrderedDaysOfWeek[i];
                string text = Lang.IsEng ? Lang.ShortDaysEng[i] : Lang.ShortDaysVi[i];
                bool isSel = ScheduleDays.Contains(d);
                DayPillButton pill = new DayPillButton(d, text, isSel);
                pill.Location = new Point(54 + i * 34, 222);
                pill.Enabled = ScheduleEnabled;
                pill.SelectedChanged += delegate(object s, EventArgs ev) {
                    DayPillButton p = (DayPillButton)s;
                    if (p.Selected)
                    {
                        ScheduleDays.Add(p.Day);
                        if (CustomDaySchedules.ContainsKey(p.Day)) CustomDaySchedules[p.Day].Enabled = true;
                    }
                    else
                    {
                        ScheduleDays.Remove(p.Day);
                        if (CustomDaySchedules.ContainsKey(p.Day)) CustomDaySchedules[p.Day].Enabled = false;
                    }
                    SaveConfigValue("ScheduleDays", string.Join(",", GetDayKeys(ScheduleDays)));
                };
                setPanel.Controls.Add(pill);
                dayPillButtons[i] = pill;
            }

            btnCustomSchedule = new Button();
            btnCustomSchedule.Text = Lang.CustomScheduleBtn;
            btnCustomSchedule.Font = Theme.FontBodySmall;
            btnCustomSchedule.Location = new Point(298, 221);
            btnCustomSchedule.Size = new Size(122, 26);
            btnCustomSchedule.FlatStyle = FlatStyle.Flat;
            btnCustomSchedule.Cursor = Cursors.Hand;
            btnCustomSchedule.Enabled = ScheduleEnabled;
            btnCustomSchedule.Click += delegate {
                OpenCustomScheduleDialog();
            };
            setPanel.Controls.Add(btnCustomSchedule);

            lblCustomIndicator = new Label();
            lblCustomIndicator.Font = Theme.FontBodySmall;
            lblCustomIndicator.AutoSize = true;
            lblCustomIndicator.Location = new Point(426, 225);
            lblCustomIndicator.Text = Lang.CustomScheduleActive;
            lblCustomIndicator.Visible = ScheduleEnabled && ScheduleCustomPerDay;
            setPanel.Controls.Add(lblCustomIndicator);

            lblHotkey = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(14, 260) };
            lblHotkeySub = new Label() { Font = Theme.FontBodySmall, AutoSize = true, Location = new Point(14, 282) };
            setPanel.Controls.Add(lblHotkey);
            setPanel.Controls.Add(lblHotkeySub);

            hotkeyBox = new TextBox();
            hotkeyBox.ReadOnly = true;
            hotkeyBox.Font = Theme.FontBody;
            hotkeyBox.TextAlign = HorizontalAlignment.Center;
            hotkeyBox.Location = new Point(setPanel.Width - 134, 262);
            hotkeyBox.Width = 120;
            hotkeyBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            hotkeyBox.Cursor = Cursors.Hand;
            hotkeyBox.Text = GetHotkeyText(HotkeyMods, HotkeyVK);
            hotkeyBox.GotFocus += delegate { hotkeyBox.Text = Lang.IsEng ? "Press keys..." : "Bấm phím..."; hotkeyBox.SelectAll(); };
            hotkeyBox.LostFocus += delegate { hotkeyBox.Text = GetHotkeyText(HotkeyMods, HotkeyVK); };
            hotkeyBox.KeyDown += HotkeyBox_KeyDown;
            setPanel.Controls.Add(hotkeyBox);

            lblTheme = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(14, 306) };
            setPanel.Controls.Add(lblTheme);

            themeCombo = new NoScrollComboBox();
            themeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            themeCombo.Location = new Point(setPanel.Width - 134, 303);
            themeCombo.Width = 120;
            themeCombo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            themeCombo.SelectedIndexChanged += ThemeCombo_SelectedIndexChanged;
            setPanel.Controls.Add(themeCombo);

            lblLang = new Label() { Font = Theme.FontBody, AutoSize = true, Location = new Point(14, 344) };
            setPanel.Controls.Add(lblLang);

            langCombo = new NoScrollComboBox();
            langCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            langCombo.Items.AddRange(Lang.Languages);
            langCombo.Location = new Point(setPanel.Width - 134, 341);
            langCombo.Width = 120;
            langCombo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            langCombo.SelectedIndex = Lang.IsEng ? 1 : 0;
            langCombo.SelectedIndexChanged += LangCombo_SelectedIndexChanged;
            setPanel.Controls.Add(langCombo);

            saveButton = new Button();
            saveButton.Size = new Size(this.ClientSize.Width - 40, 36);
            saveButton.Location = new Point(20, this.ClientSize.Height - 65);
            saveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            saveButton.FlatStyle = FlatStyle.Flat;
            saveButton.Cursor = Cursors.Hand;
            saveButton.Font = new Font("Segoe UI Semibold", 10F);
            saveButton.Click += SaveButton_Click;
            this.Controls.Add(saveButton);

            lblCredits = new Label();
            lblCredits.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            lblCredits.AutoSize = true;
            lblCredits.Location = new Point(this.ClientSize.Width - 120, this.ClientSize.Height - 22);
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
            if (lblScheduleDays != null) lblScheduleDays.Text = Lang.ScheduleDaysLabel;
            if (btnCustomSchedule != null) btnCustomSchedule.Text = Lang.CustomScheduleBtn;
            if (lblCustomIndicator != null) lblCustomIndicator.Text = Lang.CustomScheduleActive;
            if (dayPillButtons != null)
            {
                for (int i = 0; i < OrderedDaysOfWeek.Length; i++)
                {
                    if (dayPillButtons[i] != null)
                    {
                        dayPillButtons[i].DisplayText = Lang.IsEng ? Lang.ShortDaysEng[i] : Lang.ShortDaysVi[i];
                        dayPillButtons[i].Invalidate();
                    }
                }
            }
            UpdateScheduleUIState();
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
            lblCredits.Left = this.ClientSize.Width - 20 - lblCredits.PreferredWidth;

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
            try {
                SetPreferredAppMode(Theme.IsDark ? 2 : 3);
                AllowDarkModeForWindow(this.Handle, Theme.IsDark);
                SetWindowTheme(this.Handle, Theme.IsDark ? "DarkMode_Explorer" : "Explorer", null);
                if (listPanel != null && listPanel.IsHandleCreated) {
                    AllowDarkModeForWindow(listPanel.Handle, Theme.IsDark);
                    SetWindowTheme(listPanel.Handle, Theme.IsDark ? "DarkMode_Explorer" : "Explorer", null);
                    ShowScrollBar(listPanel.Handle, SB_HORZ, false);
                    RedrawWindow(listPanel.Handle, IntPtr.Zero, IntPtr.Zero, RDW_FRAME | RDW_INVALIDATE | RDW_UPDATENOW);
                }
                if (searchBox != null && searchBox.IsHandleCreated)
                    SetWindowTheme(searchBox.Handle, Theme.IsDark ? "DarkMode_CFD" : "CFD", null);
                if (hotkeyBox != null && hotkeyBox.IsHandleCreated)
                    SetWindowTheme(hotkeyBox.Handle, Theme.IsDark ? "DarkMode_CFD" : "CFD", null);
            } catch { }

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
            if (lblScheduleDays != null) lblScheduleDays.ForeColor = Theme.TextSecondary;
            UpdateScheduleUIState();
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

            if (themeCombo != null) themeCombo.ApplyTheme();
            if (langCombo != null) langCombo.ApplyTheme();

            if (dtpScheduleFrom != null) dtpScheduleFrom.Invalidate();
            if (dtpScheduleTo != null) dtpScheduleTo.Invalidate();

            if (btnCustomSchedule != null)
            {
                btnCustomSchedule.BackColor = Theme.ControlFill;
                btnCustomSchedule.ForeColor = Theme.TextPrimary;
                btnCustomSchedule.FlatAppearance.BorderColor = Theme.Stroke;
            }
            if (lblCustomIndicator != null)
            {
                lblCustomIndicator.ForeColor = Theme.Accent;
            }
            if (dayPillButtons != null)
            {
                foreach (DayPillButton pill in dayPillButtons)
                {
                    if (pill != null) pill.Invalidate();
                }
            }
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
                        } else if (key.Equals("ScheduleDays", StringComparison.OrdinalIgnoreCase)) {
                            ScheduleDays.Clear();
                            foreach (string dStr in val.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                                DayOfWeek dow;
                                if (TryParseDayOfWeek(dStr.Trim(), out dow)) ScheduleDays.Add(dow);
                            }
                        } else if (key.Equals("ScheduleCustomPerDay", StringComparison.OrdinalIgnoreCase)) {
                            bool res;
                            if (bool.TryParse(val, out res)) ScheduleCustomPerDay = res;
                        } else if (key.StartsWith("Schedule_", StringComparison.OrdinalIgnoreCase)) {
                            string dStr = key.Substring("Schedule_".Length).Trim();
                            DayOfWeek dow;
                            if (TryParseDayOfWeek(dStr, out dow)) {
                                string[] parts = val.Split(new char[] { ',', '|' });
                                if (parts.Length >= 3) {
                                    bool en = parts[0] == "1" || parts[0].Equals("true", StringComparison.OrdinalIgnoreCase);
                                    TimeSpan f, t;
                                    if (TimeSpan.TryParse(parts[1], out f) && TimeSpan.TryParse(parts[2], out t)) {
                                        CustomDaySchedules[dow] = new DaySchedule(dow, en, f, t);
                                    }
                                }
                            }
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
            dict["ScheduleDays"] = string.Join(",", GetDayKeys(ScheduleDays));
            dict["ScheduleCustomPerDay"] = ScheduleCustomPerDay.ToString();
            foreach (DayOfWeek d in OrderedDaysOfWeek)
            {
                DaySchedule ds;
                if (!CustomDaySchedules.TryGetValue(d, out ds))
                {
                    ds = new DaySchedule(d, ScheduleDays.Contains(d), ScheduleFrom, ScheduleTo);
                }
                dict["Schedule_" + GetShortDayName(d)] = (ds.Enabled ? "1" : "0") + "," + ds.From.ToString(@"hh\:mm") + "," + ds.To.ToString(@"hh\:mm");
            }
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

            if (dayPillButtons != null)
            {
                ScheduleDays.Clear();
                foreach (var pill in dayPillButtons)
                {
                    if (pill.Selected)
                    {
                        ScheduleDays.Add(pill.Day);
                        if (CustomDaySchedules.ContainsKey(pill.Day))
                        {
                            CustomDaySchedules[pill.Day].Enabled = true;
                        }
                    }
                    else
                    {
                        if (CustomDaySchedules.ContainsKey(pill.Day))
                        {
                            CustomDaySchedules[pill.Day].Enabled = false;
                        }
                    }
                }
            }

            SaveSettingsToFile();
        }

        private void RefreshWindowsList()
        {
            listPanel.SuspendLayout();
            try
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
                                        row.Width = listPanel.ClientSize.Width;
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
                        row.Width = listPanel.ClientSize.Width;
                        row.ApplyTheme();
                        listPanel.Controls.Add(row);
                        currentProcesses.Add(p);
                    }
                }
                foreach(string p in HideFromTaskbarApps)
                {
                    if (!currentProcesses.Contains(p)) {
                        AppRowControl row = new AppRowControl(this, p, p, 2, false);
                        row.Width = listPanel.ClientSize.Width;
                        row.ApplyTheme();
                        listPanel.Controls.Add(row);
                        currentProcesses.Add(p);
                    }
                }
            }
            finally
            {
                listPanel.ResumeLayout(true);
                try {
                    listPanel.HorizontalScroll.Maximum = 0;
                    listPanel.HorizontalScroll.Visible = false;
                    ShowScrollBar(listPanel.Handle, SB_HORZ, false);
                } catch { }
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
            try { SetPreferredAppMode(2); } catch { }
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e) {
                try {
                    File.WriteAllText(Path.Combine(Application.StartupPath, "app_error.log"), e.Exception.ToString());
                } catch { }
            };
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) {
                try {
                    File.WriteAllText(Path.Combine(Application.StartupPath, "app_error.log"), (e.ExceptionObject != null ? e.ExceptionObject.ToString() : "Unknown"));
                } catch { }
            };

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
                try {
                    File.WriteAllText(Path.Combine(Application.StartupPath, "app_error.log"), ex.ToString());
                } catch { }
            }
        }
    }
}
