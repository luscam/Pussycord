using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Pussycord;

public static class Sandbox
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

    [DllImport("kernel32.dll")]
    static extern bool SetInformationJobObject(IntPtr hJob, int JobObjectInfoClass, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

    [StructLayout(LayoutKind.Sequential)]
    struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public uint LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public UIntPtr Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct IO_COUNTERS
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct JOBOBJECT_BASIC_UI_RESTRICTIONS
    {
        public uint UIRestrictionsClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryUsed;
        public UIntPtr PeakJobMemoryUsed;
    }

    private const int JobObjectBasicUIRestrictions = 4;
    private const int JobObjectExtendedLimitInformation = 9;

    private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;
    private const uint JOB_OBJECT_LIMIT_PRIORITY_CLASS = 0x0020;
    private const uint JOB_OBJECT_LIMIT_BREAKAWAY_OK = 0x00000800;
    private const uint JOB_OBJECT_LIMIT_SILENT_BREAKAWAY_OK = 0x00001000;

    private const uint JOB_OBJECT_UILIMIT_SYSTEMPARAMETERS = 0x00000008; 
    private const uint JOB_OBJECT_UILIMIT_EXITWINDOWS = 0x00000080;      

    private static IntPtr _hJob;

    public static void Initialize()
    {
        if (_hJob != IntPtr.Zero) return;

        _hJob = CreateJobObject(IntPtr.Zero, null);

        var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            BasicLimitInformation = new JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE | 
                             JOB_OBJECT_LIMIT_PRIORITY_CLASS |
                             JOB_OBJECT_LIMIT_BREAKAWAY_OK |
                             JOB_OBJECT_LIMIT_SILENT_BREAKAWAY_OK,
                PriorityClass = 0x00008000 
            }
        };

        int lengthInfo = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
        IntPtr pInfo = Marshal.AllocHGlobal(lengthInfo);

        try
        {
            Marshal.StructureToPtr(info, pInfo, false);
            SetInformationJobObject(_hJob, JobObjectExtendedLimitInformation, pInfo, (uint)lengthInfo);
        }
        finally
        {
            Marshal.FreeHGlobal(pInfo);
        }

        var uiRestrictions = new JOBOBJECT_BASIC_UI_RESTRICTIONS
        {
            UIRestrictionsClass = JOB_OBJECT_UILIMIT_SYSTEMPARAMETERS |
                                  JOB_OBJECT_UILIMIT_EXITWINDOWS
        };

        int lengthUi = Marshal.SizeOf(typeof(JOBOBJECT_BASIC_UI_RESTRICTIONS));
        IntPtr pUi = Marshal.AllocHGlobal(lengthUi);

        try
        {
            Marshal.StructureToPtr(uiRestrictions, pUi, false);
            SetInformationJobObject(_hJob, JobObjectBasicUIRestrictions, pUi, (uint)lengthUi);
        }
        finally
        {
            Marshal.FreeHGlobal(pUi);
        }
    }

    public static void Attach(Process process)
    {
        if (_hJob != IntPtr.Zero && process != null && !process.HasExited)
        {
            try
            {
                AssignProcessToJobObject(_hJob, process.Handle);
            }
            catch { }
        }
    }
}