# LoopbackNET
Easy way to create and remove loopback network interfaces on Windows (.NET Framework)

Utilizes devcon and netsh.

## Privileges
An important not about this library - To use devcon it must be run as an **elevated process**.
This is currently enforced by making a check in LoopbackNET that the current process is elevated when trying
to create new loopbacks.

## Usage
Create new unnamed/name interface:
```C#
LoopbackNET.Loopback.Create();
LoopbackNET.Loopback.Create("myLoopback");
```

If you want to remove the interface when you are done, keep the result:
```C#
LoopbackInterface iface = LoopbackNET.Loopback.Create();
... more code ...
iface.Remove();
```

Another option is to use the 'using' keyword since `LoopbackInterface` implements `IDisposable`:
```C#
using(LoopbackInterface iface = LoopbackNET.Loopback.Create())
{
  ... more code ...
}
```

### Working with SharpPcap ( + Npcap )
While SharpPcap is not a requirement for this library, you might want to use your
newly created loopback interface with a SharpPcap device object.

You can use the NetworkInterface ID of the created loopback to find the right device like so:
```C#
LoopbackInterface iface = LoopbackNET.Loopback.Create();
string interfaceId = iface.NetworkInterface.Id;
ICaptureDevice dev = SharpPcap.CaptureDeviceList.Instance.Single(device => device.Name.Contains(interfaceId));
```
