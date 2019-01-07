using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using static mi360.Win32.Native.SetupApi;

namespace mi360.Win32
{
    // Source: https://stackoverflow.com/questions/4097000/how-do-i-disable-a-system-device-programatically
    public static class DeviceStateManager
    {
        // returns true for real gamepad; exit for tre == disable
        public static bool ChangeDeviceState(string filter, ref bool disable)
        {
            IntPtr info = IntPtr.Zero;
            Guid NullGuid = Guid.Empty;
            bool real = false;

            try
            {
                info = SetupDiGetClassDevsW(ref NullGuid, null, IntPtr.Zero, DIGCF_ALLCLASSES);
                CheckError("SetupDiGetClassDevs");

                SP_DEVINFO_DATA devdata = new SP_DEVINFO_DATA();
                devdata.cbSize = (UInt32) Marshal.SizeOf(devdata);

                // Get first device matching device criterion.
                for (uint i = 0;; i++)
                {
                    string devicepath = null;

                    // SetupDiEnumDeviceInfo() is used ONLY here
                    // 3 states: real = true, real = false and continue, real = false and quit
                    // quit if 0 == devdata.cbSize
                    real = SetupDiEnumDeviceInfo(info, i, out devdata);

                    if (0 < devdata.cbSize)
                        devicepath = GetStringPropertyForDevice(info, devdata, 1); // SPDRP_HARDWAREID

                    if (! real)
                        break;

                    if (devicepath != null && devicepath.Contains(filter))
                        break;
                }

                if (0 < devdata.cbSize)
                {
                    disable = false;  // for second invocation, if real

                    SP_CLASSINSTALL_HEADER header = new SP_CLASSINSTALL_HEADER();
                    header.cbSize = (UInt32)Marshal.SizeOf(header);
                    header.InstallFunction = DIF_PROPERTYCHANGE;

                    SP_PROPCHANGE_PARAMS propchangeparams = new SP_PROPCHANGE_PARAMS
                    {
                        ClassInstallHeader = header,
                        StateChange = disable ? DICS_DISABLE : DICS_ENABLE,
                        Scope = DICS_FLAG_GLOBAL,
                        HwProfile = 0
                    };

                    SetupDiSetClassInstallParams(info, ref devdata, ref propchangeparams, (UInt32)Marshal.SizeOf(propchangeparams));
                    CheckError("SetupDiSetClassInstallParams");

                    SetupDiChangeState(info, ref devdata);
                    //SetupDiCallClassInstaller(DIF_PROPERTYCHANGE, info, ref devdata);
                    CheckError("SetupDiChangeState");
                }
            }
            finally
            {
                if (info != IntPtr.Zero)
                    SetupDiDestroyDeviceInfoList(info);
            }

            return real;
        }

        private static void CheckError(string message, int lasterror = -1)
        {
            int code = lasterror == -1 ? Marshal.GetLastWin32Error() : lasterror;
            if (code != 0)
                throw new Win32Exception(code, $"An API call returned an error: {message}");
        }

        private static string GetStringPropertyForDevice(IntPtr info, SP_DEVINFO_DATA devdata, uint propId)
        {
            IntPtr buffer = IntPtr.Zero;
            try
            {
                uint buflen = 1024;
                buffer = Marshal.AllocHGlobal((int) buflen);
                uint outsize = 0;

                SetupDiGetDeviceRegistryPropertyW(info, ref devdata, propId, out uint proptype, buffer, buflen, ref outsize);

                if (0 == outsize)
                    return null;

                byte[] lbuffer = new byte[outsize];
                Marshal.Copy(buffer, lbuffer, 0, (int) outsize);
                return Encoding.Unicode.GetString(lbuffer);
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(buffer);
            }
        }

        // returns true for real gamepads;  null filter if quit
        public static bool DisableReEnableDevice(ref string filter)
        {
            Win32Exception ex = null;
            bool disable = true, real = false;

            // returns true for real gamepad; exit for tre == disable
            try { real = ChangeDeviceState(filter, ref disable); }
            catch(Win32Exception e) { ex = e; }

            if (! real)
            {
                if (disable)
                    filter = null;
                return real;
            }

            try { ChangeDeviceState(filter, ref disable); }
            catch (Win32Exception e) { ex = e; }

            return (ex != null);
        }
    }
}
