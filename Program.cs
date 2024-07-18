using System.Runtime.InteropServices;

namespace Win32ApiShutdown
{
    internal class Program
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1LuId
        {
            internal int Count;
            internal long LuId;
            internal int Attr;
        }

        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluId);

        [DllImport("advapi32.dll", ExactSpelling = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, [MarshalAs(UnmanagedType.Bool)] bool disall, ref TokPriv1LuId newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("user32.dll", ExactSpelling = true)]
        internal static extern bool ExitWindowsEx(int flg, int rea);

        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        [Obsolete("Will corrupt account that this app is run with", true)]
        internal const int EWX_LOGOFF = 0x00000000;
        internal const int EWX_SHUTDOWN = 0x00000001;
        internal const int EWX_REBOOT = 0x00000002;
        internal const int EWX_FORCE = 0x00000004;
        internal const int EWX_POWEROFF = 0x00000008;
        internal const int EWX_FORCEIFHUNG = 0x00000010;

        internal static void Main()
        {
            DoExitWin(EWX_REBOOT | EWX_FORCEIFHUNG);
        }

        internal static void DoExitWin(int flg)
        {
            TokPriv1LuId tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            tp.Count = 1;
            tp.LuId = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            LookupPrivilegeValue(null, "SeShutdownPrivilege", ref tp.LuId);
            AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            ExitWindowsEx(flg, 0);
        }
    }
}
