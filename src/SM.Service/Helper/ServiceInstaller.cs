﻿
using System;
using System.Runtime.InteropServices;

namespace SM.Service.Helper
{
    internal class Win32
    {
        // Use DllImportAttribute to inport the Win32 MessageBox
        // function.  Set the SetLastError flag to true to allow
        // the function to set the Win32 error.
        [DllImportAttribute("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hwnd, String text, String caption, uint type);
    }

    internal class ServiceInstaller
    {
        #region DLLImport
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr OpenSCManager(string lpMachineName, string lpSCDB, int scParameter);
        [DllImport("Advapi32.dll", SetLastError = true)]
        public static extern IntPtr CreateService(IntPtr hSCManager, string lpServiceName, string lpDisplayName,
        int dwDesiredAccess, int dwServiceType, int dwStartType, int dwErrorControl, string lpBinaryPathName,
        string lpLoadOrderGroup, int lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword);
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern void CloseServiceHandle(IntPtr SCHANDLE);
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int StartService(IntPtr SVHANDLE, int dwNumServiceArgs, string lpServiceArgVectors);
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr SCHANDLE, string lpSvcName, int dwNumServiceArgs);
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int DeleteService(IntPtr SVHANDLE);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetLastError();
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, string lpBuffer, int nSize, IntPtr Arguments);
        #endregion

        /// <summary>
        /// This method installs and runs the service in the service control manager.
        /// </summary>
        /// <param name="svcPath">The complete path of the service.</param>
        /// <param name="svcName">Name of the service.</param>
        /// <param name="svcDispName">Display name of the service.</param>
        /// <returns>True if the process went thro successfully. False if there was any error.</returns>
        public static Boolean Install(string servicePath, string serviceName, string serviceDisplayName)
        {
            int SC_MANAGER_CREATE_SERVICE = 0x0002;
            int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
            //int SERVICE_DEMAND_START = 0x00000003;
            int SERVICE_ERROR_NORMAL = 0x00000001;
            int STANDARD_RIGHTS_REQUIRED = 0xF0000;
            int SERVICE_QUERY_CONFIG = 0x0001;
            int SERVICE_CHANGE_CONFIG = 0x0002;
            int SERVICE_QUERY_STATUS = 0x0004;
            int SERVICE_ENUMERATE_DEPENDENTS = 0x0008;
            int SERVICE_START = 0x0010;
            int SERVICE_STOP = 0x0020;
            int SERVICE_PAUSE_CONTINUE = 0x0040;
            int SERVICE_INTERROGATE = 0x0080;
            int SERVICE_USER_DEFINED_CONTROL = 0x0100;
            int SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
            SERVICE_QUERY_CONFIG |
            SERVICE_CHANGE_CONFIG |
            SERVICE_QUERY_STATUS |
            SERVICE_ENUMERATE_DEPENDENTS |
            SERVICE_START |
            SERVICE_STOP |
            SERVICE_PAUSE_CONTINUE |
            SERVICE_INTERROGATE |
            SERVICE_USER_DEFINED_CONTROL);
            int SERVICE_AUTO_START = 0x00000002;

            try
            {
                IntPtr sc_handle = OpenSCManager(null, null, SC_MANAGER_CREATE_SERVICE);
                if (sc_handle != IntPtr.Zero)
                {
                    IntPtr sv_handle = CreateService(sc_handle, servicePath, serviceName, SERVICE_ALL_ACCESS, SERVICE_WIN32_OWN_PROCESS, SERVICE_AUTO_START, SERVICE_ERROR_NORMAL, serviceDisplayName, null, 0, null, null, null);
                    if (sv_handle != IntPtr.Zero)
                    {
                        Int32 t = Marshal.GetLastWin32Error();
                        CloseServiceHandle(sc_handle);
                        return false;
                    }
                    else
                    {
                        //now trying to start the service
                        int i = StartService(sv_handle, 0, null);
                        // If the value i is zero, then there was an error starting the service.
                        // note: error may arise if the service is already running or some other problem.
                        if (i == 0)
                        {
                            //Console.WriteLine("Couldnt start service");
                            return false;
                        }
                        //Console.WriteLine("Success");
                        CloseServiceHandle(sc_handle);
                        return true;
                    }
                }
                else
                    //Console.WriteLine("SCM not opened successfully");
                    return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// This method uninstalls the service from the service conrol manager.
        /// </summary>
        /// <param name="svcName">Name of the service to uninstall.</param>
        public static Boolean Uninstall(string serviceName)
        {
            int GENERIC_WRITE = 0x40000000;
            IntPtr sc_hndl = OpenSCManager(null, null, GENERIC_WRITE);
            if (sc_hndl != IntPtr.Zero)
            {
                int DELETE = 0x10000;
                IntPtr svc_hndl = OpenService(sc_hndl, serviceName, DELETE);
                //Console.WriteLine(svc_hndl.ToInt32());
                if (svc_hndl != IntPtr.Zero)
                {
                    int i = DeleteService(svc_hndl);
                    if (i != 0)
                    {
                        CloseServiceHandle(sc_hndl);
                        return true;
                    }
                    else
                    {
                        CloseServiceHandle(sc_hndl);
                        return false;
                    }
                }
                else
                    return false;
            }
            else
                return false;
        }
    }
}
