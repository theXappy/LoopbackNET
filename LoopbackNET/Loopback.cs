using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using LoopbackNET.Util;

namespace LoopbackNET
{
    public static class Loopback
    {
        static Random rand = new Random();

        private static readonly string _MSLOOP_DEFAULT_HWID = "*MSLOOP";
        private static readonly string _LPNET_TEMP_HWID = "LOOPBACKNETTEMP";
        private static readonly string _LPNET_TOKEN_PREFIX = "LOOPBACKNETTOKEN_";

        public static LoopbackInterface Create(string name = null)
        {
            string removalToken;

            // Getting current defined interfaces, so we can deduce which new interface was created
            NetworkInterface[] oldInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            // Check if an interface already exists with this name
            NetworkInterface candidate = oldInterfaces.SingleOrDefault(iface => iface.Name == name);
            if (candidate != null)
            {
                removalToken = _LPNET_TOKEN_PREFIX + candidate.Id;
                return new LoopbackInterface(candidate,removalToken);
            }


            // This process required admin privileges. If we don't have those, there's no point trying
            AssertAdminPriv();


            // Mark existing interfaces
            DevconHelper.AddHWID(_MSLOOP_DEFAULT_HWID, _LPNET_TEMP_HWID);
            DevconHelper.RemoveHWID(_MSLOOP_DEFAULT_HWID, _MSLOOP_DEFAULT_HWID);


            // Running DEVCON util to create a new (unnamed) loopback device
            DevconHelper.Install(@"%WINDIR%\Inf\Netloop.inf", _MSLOOP_DEFAULT_HWID);


            // Getting interfaces after the loop back addition
            NetworkInterface[] newInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            // Filtering previous interfaces from current interfaces, trying to narrow down the options for the new interface
            NetworkInterface[] diffInterfaces = newInterfaces.Except(oldInterfaces,new NetInterfacesComparer()).ToArray();
            if (diffInterfaces.Length == 0)
            {
                // Restore previous interfaces' hwid before throwing
                DevconHelper.AddHWID(_LPNET_TEMP_HWID, _MSLOOP_DEFAULT_HWID);
                DevconHelper.RemoveHWID(_LPNET_TEMP_HWID, _LPNET_TEMP_HWID);
                throw new Exception("No new interface could be found after devcon finished.");
            }

            if (diffInterfaces.Length > 1)
            {
                // Restore previous interfaces' hwid before throwing
                DevconHelper.AddHWID(_LPNET_TEMP_HWID, _MSLOOP_DEFAULT_HWID);
                DevconHelper.RemoveHWID(_LPNET_TEMP_HWID, _LPNET_TEMP_HWID);
                throw new Exception(
                    $"Expected exactly 1 new interfaces, found {diffInterfaces.Length}. New interfaces: {string.Join(",", diffInterfaces.Select(iface => iface.Name).ToArray())}");
            }


            // We found our new loopback device!
            NetworkInterface loopbackDevice = diffInterfaces.Single();


            // Mark newly creating device with special removal token
            removalToken = _LPNET_TOKEN_PREFIX + loopbackDevice.Id;
            DevconHelper.AddHWID("*MSLOOP", removalToken);


            // Restore previous interfaces' hwid
            DevconHelper.AddHWID(_LPNET_TEMP_HWID, _MSLOOP_DEFAULT_HWID);
            DevconHelper.RemoveHWID(_LPNET_TEMP_HWID, _LPNET_TEMP_HWID);


            // Check if rename required
            if (name != null)
            {
                // Specific name requested, call sub-function
                RenameInterface(loopbackDevice, name);

                // Retrieve new, updated, NetworkInterface object with the new name
                loopbackDevice = NetworkInterface.GetAllNetworkInterfaces().Single(iface => iface.Id == loopbackDevice.Id);
            }

            return new LoopbackInterface(loopbackDevice,removalToken);
        }

        /// <summary>
        /// Remove a specific Loopback device
        /// </summary>
        /// <param name="loopback">Device to remove</param>
        public static void Remove(LoopbackInterface loopback)
        {
            DevconHelper.Remove(loopback.RemovalToken);
        }


        /// <summary>
        /// Remove all loopback interfaces. By default only removes interfaces created with
        /// Loopback.NET but can also remove other loop back interfaces
        /// </summary>
        /// <param name="onlyLoopbackNetIfaces">True to remove only interfaces created with Loopback.NET, false to remove all</param>
        public static void RemoveAll(bool onlyLoopbackNetIfaces = true)
        {
            string idToRemove;
            if (onlyLoopbackNetIfaces)
            {
                // Adding wildcard (*) to capture all matchinv devices
                idToRemove = _LPNET_TOKEN_PREFIX+"*";
            }
            else
            {
                // Adding wildcard (*) to capture all matchinv devices
                idToRemove = _MSLOOP_DEFAULT_HWID+"*";
            }
            DevconHelper.Remove(idToRemove);
        }

        private static void RenameInterface(NetworkInterface iface, string newName)
        {
            // Using netsh to rename the interface. we need to tell it the current name and the new name
            string oldName = iface.Name;
            ProcessStartInfo psi = new ProcessStartInfo("cmd", $@"/C netsh interface set interface name = ""{oldName}"" newname = ""{newName}""");
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process netsh = Process.Start(psi);
            netsh.WaitForExit();
        }
        private static void AssertAdminPriv()
        {
            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            if (!isElevated)
                throw new UnauthorizedAccessException("Loopback creations requires Administrator privileges.");
        }
    }
}
