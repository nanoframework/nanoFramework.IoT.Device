# DHCP Server

This binding offers simple and efficient DHCP capabilities. It is following the [RFC2131](https://www.rfc-editor.org/rfc/rfc2131.html#ref-19) and does implement the minimum set to offer decent DHCP capabilities.

## Reference

* DHCP referent in [RFC2131](https://www.rfc-editor.org/rfc/rfc2131.html#ref-19).
* Options definitions in [Wikipedia](https://en.wikipedia.org/wiki/Dynamic_Host_Configuration_Protocol) and [IBM](https://www.ibm.com/docs/en/i/7.2?topic=concepts-dhcp-options-lookup).
* Captive Portal option is supported from the latest [RFC8910](https://datatracker.ietf.org/doc/html/rfc8910).

## Usage

You simply create a server, set the DHCP address and the mask.

```csharp
DhcpServer dhcpserver = new DhcpServer();
// Give the captive portal URL. Note: this is experimental and as RFC is new, only works on a limited number of devices.
dhcpserver.CaptivePortalUrl = "http://192.168.4.1";
// Starts the serveur with the DHCP server address (should be the device address) and the mask.
dhcpserver.Start(IPAddress.Parse(new IPAddress(new byte[] {192, 168, 4, 1}), new IPAddress(new byte[] { 255, 255, 255, 0 })));
```

By default the time to leave is set to 1200 seconds, you adjust it.

Also note that the server will smartly manage the IP addresses and will give the preference to any device if it's available or if the device had it before. It will also clean the bails to make sure there is always enough space available.

While you can use this simple and efficient server in a real network, it remains for simple usage and is **not** recommended in production.

## Information on the sample

The sample is a complete example on how to use this DHCP server to provide IP addresses to a phone or a PC you'll connect to the .NET nanoFramework device to setup the Wireless configuration it has to connect to.

## Limitations

* This server does only support the basic flow. It does not support advanced flow with Renew and Release.
* While you can specify the mask you want, internally, the server only support mask C. So it will allocate 253 addresses maximum.
* While the Captive Portal option is supported from the latest [RFC8910](https://datatracker.ietf.org/doc/html/rfc8910), only few OS and mobile phones supports it today. It is still experimental.
