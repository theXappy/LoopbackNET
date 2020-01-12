using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace LoopbackNET
{
    internal class NetInterfacesComparer : IEqualityComparer<NetworkInterface>
    {
        public bool Equals(NetworkInterface x, NetworkInterface y)
        {
            return (x == null && y == null) ||
                   ((x != null && y != null) && (x.Id == y.Id));
        }

        public int GetHashCode(NetworkInterface obj)
        {
            return 1;
        }
    }
}