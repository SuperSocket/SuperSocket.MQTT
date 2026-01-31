# SuperSocket.MQTT

[![build](https://github.com/SuperSocket/SuperSocket.MQTT/actions/workflows/build.yml/badge.svg)](https://github.com/SuperSocket/SuperSocket.MQTT/actions/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.MQTT.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.MQTT/)
[![NuGet](https://img.shields.io/nuget/dt/SuperSocket.MQTT.svg)](https://www.nuget.org/packages/SuperSocket.MQTT)

**SuperSocket.MQTT** is an MQTT protocol implementation built on top of [SuperSocket](https://github.com/kerryjiang/SuperSocket), a high-performance, extensible socket server application framework for .NET.

- **License**: [Apache 2.0](https://www.apache.org/licenses/LICENSE-2.0)

## Key Features

- **MQTT 3.1.1 Protocol Support**: Implementation based on the OASIS MQTT 3.1.1 specification
- **Built on SuperSocket**: Leverages SuperSocket's powerful pipeline architecture and session management
- **Server & Client**: Includes both server-side and client-side components
- **Command-based Architecture**: Uses SuperSocket's command pattern for handling MQTT control packets
- **Topic Management**: Built-in topic subscription and message routing with wildcard support (`+` and `#`)
- **Cross-Platform**: Supports .NET 6.0, .NET 7.0, .NET 8.0, and .NET 9.0

## Installation

Install via NuGet:

```bash
# Core MQTT package
dotnet add package SuperSocket.MQTT

# For MQTT Server
dotnet add package SuperSocket.MQTT.Server

# For MQTT Client
dotnet add package SuperSocket.MQTT.Client
```

## Quick Start

### MQTT Server

```csharp
using SuperSocket.MQTT;
using SuperSocket.MQTT.Server;
using SuperSocket.Server.Host;

var host = SuperSocketHostBuilder
    .Create<MQTTPacket>()
    .UseMQTT()
    .UseInProcSessionContainer()
    .ConfigureAppConfiguration((hostCtx, configApp) =>
    {
        configApp.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "serverOptions:name", "MQTTServer" },
            { "serverOptions:listeners:0:ip", "Any" },
            { "serverOptions:listeners:0:port", "1883" }
        });
    })
    .Build();

await host.RunAsync();
```

### MQTT Client

```csharp
using SuperSocket.MQTT.Client;
using System.Net;
using System.Text;

await using var client = new MQTTClient();

// Connect to the broker
await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 1883));

// Send MQTT CONNECT and receive CONNACK
var connAck = await client.SendConnectAsync("MyClient", keepAlive: 60);

// Subscribe to a topic
var subAck = await client.SendSubscribeAsync("home/temperature", qos: 0);

// Publish a message
await client.SendPublishAsync("home/temperature", Encoding.UTF8.GetBytes("25.5"));

// Disconnect
await client.SendDisconnectAsync();
```

## Project Structure

| Package | Description |
|---------|-------------|
| **SuperSocket.MQTT** | Core MQTT protocol types, packet definitions, and pipeline filter |
| **SuperSocket.MQTT.Server** | MQTT server implementation with command handlers and topic management |
| **SuperSocket.MQTT.Client** | MQTT client implementation for connecting to MQTT brokers |

## References

- [MQTT Version 3.1.1 Specification](https://docs.oasis-open.org/mqtt/mqtt/v3.1.1/mqtt-v3.1.1.html)
- [MQTT Version 5.0 Specification](https://docs.oasis-open.org/mqtt/mqtt/v5.0/mqtt-v5.0.html)
- [SuperSocket Documentation](https://docs.supersocket.net/)

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.