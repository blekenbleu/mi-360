﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static mi360.Win32.Native.DBT;

namespace mi360.Win32
{
    public class DeviceMonitor : NativeWindow, IMonitor
    {
        #region Constants & Fields

        private const string HIDClassID = "4D1E55B2-F16F-11CF-88CB-001111000030";
        private static CreateParams cp = new CreateParams { Parent = (IntPtr)(-3) };

        public event EventHandler<string> DeviceAttached;
        public event EventHandler<string> DeviceRemoved;

        #endregion

        #region Constructors

        public DeviceMonitor()
        {
            CreateHandle(cp);  // CA2214 
        }

        #endregion

        #region Methods

        public void Start()
        {
            DEV_BROADCAST_DEVICEINTERFACE notificationFilter = new DEV_BROADCAST_DEVICEINTERFACE();
            int size = Marshal.SizeOf(notificationFilter);
            notificationFilter.dbcc_size = size;
            notificationFilter.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
            notificationFilter.dbcc_reserved = 0;
            notificationFilter.dbcc_classguid = new Guid(HIDClassID).ToByteArray();

            IntPtr buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(notificationFilter, buffer, true);
            IntPtr result = RegisterDeviceNotification(Handle, buffer, (int)(DEVICE_NOTIFY.DEVICE_NOTIFY_WINDOW_HANDLE | DEVICE_NOTIFY.DEVICE_NOTIFY_ALL_INTERFACE_CLASSES));
        }

        public void Stop()
        {
            UnregisterDeviceNotification(Handle);
        }

        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == WM_DEVICECHANGE)
            {
                var info = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(msg.LParam, typeof(DEV_BROADCAST_DEVICEINTERFACE));
                var devicePath = new string(info.dbcc_name);

                switch (msg.WParam.ToInt64())
                {
                    case DBT_DEVICEARRIVAL:
                        DeviceAttached?.Invoke(this, devicePath);
                        break;

                    case DBT_DEVICEREMOVECOMPLETE:
                        DeviceRemoved?.Invoke(this, devicePath);
                        break;

                    case DBT_DEVNODES_CHANGED:
                        break;
                }
            }

            base.WndProc(ref msg);
        }

        #endregion

        #region IDisposable pattern

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                DestroyHandle();
        }

        ~DeviceMonitor()
        {
            Dispose(false);
        }
        
        #endregion
    }
}
