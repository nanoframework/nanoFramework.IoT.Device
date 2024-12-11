# Multicast DNS

Multicast DNS (mDNS) is a computer networking protocol that acts as a DNS server on a local network. The main use case is to resolve hostnames to IP addresses within local networks. Modern browsers send a Multicast DNS question over UDP whenever requesting a domain with .local as TLD. For instance: http://nanodevice.local. 
This binding allows to resolve these DNS requests and return an IP address.
The implementation is complete so any other DNS queries can be treated and any of the standard DNS records can be returned.

## Reference

* Some information in [Wikipedia](https://en.wikipedia.org/wiki/Multicast_DNS).
* mDNS is defined in [RFC6762](https://datatracker.ietf.org/doc/html/rfc6762).

## Use cases

The primary use case is obviously resolving a .local domain to the IP address of the nanoFramework device which facilitates:

- A new IoT device lacking config starts in AP mode, mDNS allows to connect to it without knowing its IP address.
- Once configured, a device typically gets its IP address from DHCP, mDNS again allows to connect to the device through a domain name which the user even could have chosen during configuration.
- More exotic uses are also possible, think about P2P communication between IoT devices who are on the same local network. These devices can discover eachother through mDNS querying and monitoring without a need for internet, for a broker and even without configuring the devices.

## Usage

You simply create an instance of the MulticastDNSService, register an event handler and you're set to go. In the event handler you can respond to DNS queries or monitor what's passing on the network. 

```csharp
using MulticastDNSService multicastDNSService = new();
multicastDNSService.MessageReceived += MulticastDNSService_MessageReceived;
multicastDNSService.Start();

private static void MulticastDNSService_MessageReceived(object sender, MessageReceivedEventArgs e)
{
    // Treat e.Message
}
```

To allow sending out messages yourself a Send method is available on the service.

## Information on the sample

The sample is offering the resolution of a domain to an IP address (the first two use cases mentioned above). Change the WiFi credentials and run the program on some hardware.
It will output a Debug message to signal when ready and then you can use a browser to surf to [your device](http://nanodevice.local).

## Limitations

Be mindful that the messages are transmitted over UDP. There is hence no guarantee that a message will arrive nor be treated by a device listening to mDNS. This is utterly
important when implementing the third use case (sending mDNS messages to discover other devices on the network). The discovery should be continuous and as per description in the RFC, a random delay should be implemented. This delay enlarges the chance that a device listening for your message will catch messages regularly since when that device is treating a message, it could skip an incoming message it was supposed to treat.
