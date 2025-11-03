# DNS Server

This binding offers a simple and minimal DNS server. It follows the [RFC1034](https://www.rfc-editor.org/rfc/rfc1034) and [RFC1035](https://www.rfc-editor.org/rfc/rfc1035) specifications.

## Reference

* DNS concepts and facilities: [RFC1034](https://www.rfc-editor.org/rfc/rfc1034)
* DNS implementation and specification: [RFC1035](https://www.rfc-editor.org/rfc/rfc1035)

## Usage

To use the DNS server, you need to add a reference to the `nanoFramework.Iot.Device.DnsServer` package to your project.

### Basic Implementation

The DNS server allows you to create a simple DNS service that responds to DNS queries based on a predefined list of DNS entries. Here's a basic example:

```csharp
using Iot.Device.DnsServer;
using System.Net;
using System.Threading;

// Create an IP address for the server to bind to
IPAddress serverAddress = IPAddress.Parse("192.168.1.1");

// Create DNS entries (domain name to IP address mappings)
DnsEntry[] dnsEntries = new DnsEntry[]
{
    // Use specific domain name mappings
    new("example.com", serverAddress),
    new("test.local", IPAddress.Parse("192.168.1.10")),
    
    // Use wildcard to catch all other domains
    new("*", serverAddress)  // Wildcard entry redirects all other DNS requests to the server address
};

// Create the DNS server with the server address and entries
DnsServer dnsServer = new DnsServer(serverAddress, dnsEntries);

// Start the DNS server
if (dnsServer.Start())
{
    // Server started successfully
    Debug.WriteLine("DNS Server started successfully");
}
else
{
    // Server failed to start
    Debug.WriteLine("Failed to start DNS Server");
}

// Keep the application running
Thread.Sleep(Timeout.Infinite);

// To stop the server:
// dnsServer.Stop();
// dnsServer.Dispose();
```

### Advanced Usage

You can also configure the DNS server with logging:

```csharp
using Iot.Device.DnsServer;
using Microsoft.Extensions.Logging;
using System.Net;

// Create a logger (implementation depends on your logging framework)
ILogger logger = /* your logger implementation */;

// Create DNS entries
DnsEntry[] dnsEntries = new DnsEntry[]
{
    new("example.com", IPAddress.Parse("192.168.1.1"))
};

// Create the DNS server with logging enabled
DnsServer dnsServer = new DnsServer(
    IPAddress.Parse("192.168.1.1"),
    dnsEntries,
    logger);

// Start the DNS server
dnsServer.Start();
```

## Information on the sample

The included [sample](samples) demonstrates how to create a simple DNS server that redirects all DNS queries to a specified IP address. This is useful for scenarios like captive portals or local development environments.

The sample:

1. Creates a DNS server bound to a specific IP address
2. Sets up a wildcard DNS entry that redirects all DNS requests to that address
3. Starts the DNS server and keeps it running indefinitely

To run the sample, make sure your device is connected to a network where it can receive DNS queries (port 53).

## Limitations

- The DNS server only supports IPv4 addresses (A records)
- Only supports basic DNS queries with a single question
- Limited error handling and DNS record types
- No support for recursive queries or forwarding
- No authentication or security features
- No caching mechanism for DNS responses
