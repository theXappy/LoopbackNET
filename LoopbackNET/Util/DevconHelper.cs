using System.Diagnostics;

namespace LoopbackNET.Util
{
    public static class DevconHelper
    {
        public static void RunCommand(string devConCommand)
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd", $@"/C devcon.exe {devConCommand}");
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process devconProc = Process.Start(psi);
            devconProc.WaitForExit();
        }

        /// <summary>
        /// Adds a specific HWID to a subset of the devices in the system
        /// </summary>
        /// <param name="filter">Filter to use to select the devices</param>
        /// <param name="newId">The new HWID to add to the filtered devices</param>
        public static void AddHWID(string filter, string newId)
        {
            RunCommand($"sethwid {filter} := {newId}");
        }

        /// <summary>
        /// Removes a specific HWID to a subset of the devices in the system
        /// </summary>
        /// <param name="filter">Filter to use to select the devices</param>
        /// <param name="newId">The HWID to remove from the filtered devices</param>
        public static void RemoveHWID(string filter, string newId)
        {
            RunCommand($"sethwid {filter} := !{newId}");
        }

        /// <summary>
        /// Installs a new device
        /// </summary>
        /// <param name="infFile">.inf file for the device</param>
        /// <param name="hwid">HWID value for the device</param>
        public static void Install(string infFile, string hwid)
        {
            RunCommand($"install {infFile} {hwid}");
        }

        /// <summary>
        /// Removes subset of the devices in the system according to a filter
        /// </summary>
        /// <param name="filter">Filter to use to select the devices</param>
        public static void Remove(string filter)
        {
            RunCommand($"remove {filter}");
        }
    }
}
