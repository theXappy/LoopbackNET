using System;
using System.Diagnostics;
using System.IO;

namespace LoopbackNET.Util
{
    public static class DevconHelper
    {
        /// <summary>
        /// Directory where devcon.exe is searched
        /// </summary>
        public static string DevconDirectory { get; set; }

        static DevconHelper()
        {
            // By default, searching devcon.exe in the current directory
            DevconDirectory = Environment.CurrentDirectory;
        }

        public static void RunCommand(string devConCommand)
        {
            // Make sure devcon.exe exists
            string devconPath = Path.Combine(DevconDirectory, "devcon.exe");
            if(!File.Exists(devconPath))
                throw new FileNotFoundException($"Could not find devcon.exe in the directory: {DevconDirectory}"); 

            ProcessStartInfo psi = new ProcessStartInfo("cmd", $@"/C ""{devconPath}"" {devConCommand}");
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process devconProc = Process.Start(psi);
            devconProc.WaitForExit();

            // Make sure devcon.exe finished with a success
            if(devconProc.ExitCode != 0)
                throw new Exception($"devcon.exe finished with non-zero exit code. Code: {devconProc.ExitCode}");
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
