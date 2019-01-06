using System;
using System.Runtime.InteropServices;

namespace mi360.Win32.Native
{
    public static class SetupApi
    {
        #region Constants

        public const uint DIF_PROPERTYCHANGE = 0x12;
        public const uint DICS_ENABLE = 1;
        public const uint DICS_DISABLE = 2; // disable device
        public const uint DICS_FLAG_GLOBAL = 1; // not profile-specific
        public const uint DIGCF_ALLCLASSES = 4;
        public const uint ERROR_INVALID_DATA = 13;
        public const uint ERROR_NO_MORE_ITEMS = 259;

        #endregion

        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_CLASSINSTALL_HEADER
        {
            public UInt32 cbSize;
            public UInt32 InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_PROPCHANGE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader;
            public UInt32 StateChange;
            public UInt32 Scope;
            public UInt32 HwProfile;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public UInt32 cbSize;
            public Guid classGuid;
            public UInt32 devInst;
            private readonly IntPtr reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVPROPKEY
        {
            public Guid fmtid;
            public UInt32 pid;
        }

        #endregion

        #region Methods

        private static void CheckError(string message, int lasterror = -1)
        {
            int code = lasterror == -1 ? Marshal.GetLastWin32Error() : lasterror;
            if (code != 0)
                throw new System.ComponentModel.Win32Exception(code, $"An API call returned an error: {message}");
        }

        public static IntPtr SetupDiGetClassDevsW(
            [In] ref Guid ClassGuid,
            [MarshalAs(UnmanagedType.LPWStr)] string Enumerator,
            IntPtr parent,
            UInt32 flagsa)
        {
            return SetupDiGetClassDevsW(ref ClassGuid, Enumerator, parent, flagsa);
        }

        public static bool SetupDiDestroyDeviceInfoList(IntPtr handle)
        {
            return SetupDiDestroyDeviceInfoList(handle);
        }

        public static bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet,
            UInt32 memberIndex,
            [Out] out SP_DEVINFO_DATA deviceInfoData)
        {
            bool foo = SetupDiEnumDeviceInfo(deviceInfoSet, memberIndex, out deviceInfoData);
            if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
                CheckError("No device found matching filter.", 0xcffff);
            CheckError("SetupDiEnumDeviceInfo");
            return foo;
        }

        public static bool SetupDiSetClassInstallParams(
            IntPtr deviceInfoSet,
            [In] ref SP_DEVINFO_DATA deviceInfoData,
            [In] ref SP_PROPCHANGE_PARAMS classInstallParams,
            UInt32 ClassInstallParamsSize)
        {
            return SetupDiSetClassInstallParams(deviceInfoSet, ref deviceInfoData,
                  ref classInstallParams, ClassInstallParamsSize);
        }

        public static bool SetupDiChangeState(
            IntPtr deviceInfoSet,
            [In] ref SP_DEVINFO_DATA deviceInfoData)
        {
            return SetupDiChangeState(deviceInfoSet, ref deviceInfoData);
        }

        public static bool SetupDiGetDevicePropertyW(
            IntPtr deviceInfoSet,
            [In] ref SP_DEVINFO_DATA DeviceInfoData,
            [In] ref DEVPROPKEY propertyKey,
            [Out] out UInt32 propertyType,
            IntPtr propertyBuffer,
            UInt32 propertyBufferSize,
            out UInt32 requiredSize,
            UInt32 flags)
        {
            return SetupDiGetDevicePropertyW(deviceInfoSet,
                ref DeviceInfoData, ref propertyKey,
                  out propertyType, propertyBuffer, propertyBufferSize,
                  out requiredSize, flags);              
        }

        public static bool SetupDiGetDeviceRegistryPropertyW(
            IntPtr DeviceInfoSet,
            [In] ref SP_DEVINFO_DATA DeviceInfoData,
            UInt32 Property,
            [Out] out UInt32 PropertyRegDataType,
            IntPtr PropertyBuffer,
            UInt32 PropertyBufferSize,
            [In, Out] ref UInt32 RequiredSize)
        {
            bool foo = SetupDiGetDeviceRegistryPropertyW(DeviceInfoSet,
                ref DeviceInfoData,
                Property, out PropertyRegDataType, PropertyBuffer,
                PropertyBufferSize, ref RequiredSize);

            int errcode = Marshal.GetLastWin32Error();

            if (errcode == ERROR_INVALID_DATA)
                RequiredSize = 0;
            else
                CheckError("SetupDiGetDeviceProperty", errcode);

            return foo;
        }

        public static bool SetupDiCallClassInstaller(
            UInt32 InstallFunction,
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData
        )
        {
            return SetupDiCallClassInstaller(InstallFunction,
                DeviceInfoSet, ref DeviceInfoData);
        }
        #endregion
    }

        #region Methods imports
    internal static class SetupApiNativeMethods
    {
        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_CLASSINSTALL_HEADER
        {
            public UInt32 cbSize;
            public UInt32 InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_PROPCHANGE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader;
            public UInt32 StateChange;
            public UInt32 Scope;
            public UInt32 HwProfile;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public UInt32 cbSize;
            public Guid classGuid;
            public UInt32 devInst;
            private readonly IntPtr reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVPROPKEY
        {
            public Guid fmtid;
            public UInt32 pid;
        }

        #endregion

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevsW(
            [In] ref Guid ClassGuid,
            [MarshalAs(UnmanagedType.LPWStr)] string Enumerator,
            IntPtr parent,
            UInt32 flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr handle);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet,
            UInt32 memberIndex,
            [Out] out SP_DEVINFO_DATA deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiSetClassInstallParams(
            IntPtr deviceInfoSet,
            [In] ref SP_DEVINFO_DATA deviceInfoData,
            [In] ref SP_PROPCHANGE_PARAMS classInstallParams,
            UInt32 ClassInstallParamsSize);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiChangeState(
            IntPtr deviceInfoSet,
            [In] ref SP_DEVINFO_DATA deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiGetDevicePropertyW(
            IntPtr deviceInfoSet,
            [In] ref SP_DEVINFO_DATA DeviceInfoData,
            [In] ref DEVPROPKEY propertyKey,
            [Out] out UInt32 propertyType,
            IntPtr propertyBuffer,
            UInt32 propertyBufferSize,
            out UInt32 requiredSize,
            UInt32 flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryPropertyW(
            IntPtr DeviceInfoSet,
            [In] ref SP_DEVINFO_DATA DeviceInfoData,
            UInt32 Property,
            [Out] out UInt32 PropertyRegDataType,
            IntPtr PropertyBuffer,
            UInt32 PropertyBufferSize,
            [In, Out] ref UInt32 RequiredSize);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiCallClassInstaller(
            UInt32 InstallFunction,
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData
        );
        #endregion
    }
}
