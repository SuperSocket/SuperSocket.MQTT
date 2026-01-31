using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Command;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions.Host;

namespace SuperSocket.MQTT.Server
{
    /// <summary>
    /// Extension methods for configuring MQTT server with SuperSocket.
    /// </summary>
    /// <example>
    /// Example usage:
    /// <code>
    /// var host = SuperSocketHostBuilder.Create&lt;MQTTPacket&gt;()
    ///     .UseMQTT()
    ///     .UseInProcSessionContainer()
    ///     .Build();
    /// await host.RunAsync();
    /// </code>
    /// </example>
    public static class SuperSocketHostBuilderExtensions
    {
        /// <summary>
        /// Configures the SuperSocket host builder to use MQTT protocol with MQTTPacket, MQTTSession, command middleware, and topic management.
        /// </summary>
        /// <param name="builder">The SuperSocket host builder.</param>
        /// <returns>The configured SuperSocket host builder for method chaining.</returns>
        public static ISuperSocketHostBuilder<MQTTPacket> UseMQTT(this ISuperSocketHostBuilder<MQTTPacket> builder)
        {
            return builder
                .UsePipelineFilter<MQTTPipelineFilter>()
                .UseSession<MQTTSession>()
                .UseCommand<ControlPacketType, MQTTPacket>(options => 
                {
                    // Add all command classes from the current executing assembly
                    options.AddCommandAssembly(Assembly.GetExecutingAssembly());
                })
                .UseMiddleware<TopicMiddleware>(sp => sp.GetRequiredService<TopicMiddleware>())
                .ConfigureServices((ctx, services) =>
                {
                    // Register TopicMiddleware as a singleton
                    services.AddSingleton<TopicMiddleware>();
                    
                    // Register ITopicManager interface pointing to the same TopicMiddleware instance
                    services.AddSingleton<ITopicManager>(serviceProvider => serviceProvider.GetRequiredService<TopicMiddleware>());
                }) as ISuperSocketHostBuilder<MQTTPacket>;
        }
    }
}
