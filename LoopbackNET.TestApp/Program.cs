using System;
using System.Net.NetworkInformation;

namespace LoopbackNET.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Setup: Removing all previous Loopback interfaces");
            Loopback.RemoveAll(onlyLoopbackNetIfaces: false);
            Console.WriteLine($"Done.");


            // 1. Manual creation and removal
            string loopbackName = "loopy";
            Console.WriteLine($"Creating Loopback named '{loopbackName}'");
            var loop1 = LoopbackNET.Loopback.Create(loopbackName);
            Console.WriteLine($"Done! New interface ID: {loop1.NetworkInterface.Id}");
            Console.WriteLine($"Removing Loopback named '{loopbackName}'");
            loop1.Remove();
            Console.WriteLine($"Done.");



            // 2. Creation and removal in a 'Using' scope
            using (LoopbackInterface loop2 = Loopback.Create())
            {
                Console.WriteLine($"Temporary loopback created. Name: '{loop2.NetworkInterface.Name}'");
                Console.WriteLine($"Disposing of temporary loopback.");
            } // Dispose removes the interface
            Console.WriteLine($"Done.");



            Console.WriteLine("Clean up: Making sure no Loopback interfaces are left behind");
            Loopback.RemoveAll(onlyLoopbackNetIfaces: false);
            Console.ReadLine();
        }
    }
}
