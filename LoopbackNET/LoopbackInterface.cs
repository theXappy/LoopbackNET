using System;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace LoopbackNET
{
    public class LoopbackInterface : IDisposable
    {
        public NetworkInterface NetworkInterface { get; private set; }
        internal string RemovalToken;
        internal LoopbackInterface(NetworkInterface iface,string removalToken)
        {
            NetworkInterface = iface;
            RemovalToken = removalToken;
        }

        /// <summary>
        /// Removes the Loopback interface from the system
        /// </summary>
        public void Remove()
        {
            Loopback.Remove(this);
        }

        // Implementing IDisposable so we can use loopbacks in 'using' statements
        public void Dispose()
        {
            Remove();
        }
    }
}