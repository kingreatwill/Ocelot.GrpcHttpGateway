using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace Built.Grpcc
{
    /*
     Example

Following an example of client to sync get pool :

IEnumerable<string> emptyScopes = Enumerable.Empty<string>();
ServiceEndpoint endpoint = new ServiceEndpoint(TestCredentials.DefaultHostOverride, Port);
var pool = new GrpcPool(emptyScopes);
var channel = pool.GetChannel(endpoint);
var client = new Greeter.GreeterClient(channel);
String user = "you";

var reply = client.SayHello(new HelloRequest { Name = user });
Console.WriteLine("Greeting: " + reply.Message);

channel.ShutdownAsync().Wait();
Following an example of client to sync get SSL pool :

IEnumerable<string> emptyScopes = Enumerable.Empty<string>();
ServiceEndpoint endpoint = new ServiceEndpoint(TestCredentials.DefaultHostOverride, SslPort);
var pool = new GrpcPool(emptyScopes);
var channel = pool.GetChannel(endpoint, clientCredentials);
var client = new Greeter.GreeterClient(channel);
String user = "you";

var reply = client.SayHello(new HelloRequest { Name = user });
Console.WriteLine("Greeting: " + reply.Message);

channel.ShutdownAsync().Wait();
Following an example of client to async get pool :

await Task.Run(async () =>
{
    IEnumerable<string> emptyScopes = Enumerable.Empty<string>();
    ServiceEndpoint endpoint = new ServiceEndpoint(TestCredentials.DefaultHostOverride, Port);
    var pool = new GrpcPool(emptyScopes);
    var channel = await pool.GetChannelAsync(endpoint);
    var client = new Greeter.GreeterClient(channel);
    String user = "you";

    var reply = await client.SayHelloAsync(new HelloRequest { Name = user });
    Console.WriteLine("Greeting: " + reply.Message);

    await channel.ShutdownAsync();
});
Following an example of client to async get SSL pool :

await Task.Run(async () =>
{
    IEnumerable<string> emptyScopes = Enumerable.Empty<string>();
    ServiceEndpoint endpoint = new ServiceEndpoint(TestCredentials.DefaultHostOverride, SslPort);
    var pool = new GrpcPool(emptyScopes);
    var channel = await pool.GetChannelAsync(endpoint, clientCredentials);
    var client = new Greeter.GreeterClient(channel);
    String user = "you";

    var reply = await client.SayHelloAsync(new HelloRequest { Name = user });
    Console.WriteLine("Greeting: " + reply.Message);

    await channel.ShutdownAsync();
});
         */

    public class GrpcPool
    {
        // TODO: See if we could use ConcurrentDictionary instead of locking. I suspect the issue would be making an atomic
        // "clear and fetch values" for shutdown.
        private readonly Dictionary<ServiceEndpoint, Channel> _channels = new Dictionary<ServiceEndpoint, Channel>();

        private readonly object _lock = new object();

        /// <summary>
        /// Shuts down all the currently-allocated channels asynchronously. This does not prevent the channel
        /// pool from being used later on, but the currently-allocated channels will not be reused.
        /// </summary>
        /// <returns></returns>
        public Task ShutdownChannelsAsync()
        {
            try
            {
                List<Channel> channelsToShutdown;
                lock (_lock)
                {
                    channelsToShutdown = _channels.Values.ToList();
                    _channels.Clear();
                }
                var shutdownTasks = channelsToShutdown.Select(c => c.ShutdownAsync());
                return Task.WhenAll(shutdownTasks);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a channel from this pool, creating a new one if there is no channel
        /// already associated with <paramref name="endpoint"/>.
        /// </summary>
        /// <param name="endpoint">The endpoint to connect to. Must not be null.</param>
        /// <returns>A channel for the specified endpoint.</returns>
        public Channel GetChannel(ServiceEndpoint endpoint)
        {
            try
            {
                GaxPreconditions.CheckNotNull(endpoint, nameof(endpoint));
                return GetChannelFromDict(endpoint, ChannelCredentials.Insecure);
            }
            catch (AggregateException e)
            {
                // Unwrap the first exception, a bit like await would.
                // It's very unlikely that we'd ever see an AggregateException without an inner exceptions,
                // but let's handle it relatively gracefully.
                throw e.InnerExceptions.FirstOrDefault() ?? e;
            }
        }

        public Channel GetChannel(ServiceEndpoint endpoint, ChannelCredentials credentials)
        {
            try
            {
                GaxPreconditions.CheckNotNull(endpoint, nameof(endpoint));
                return GetChannelFromDict(endpoint, credentials);
            }
            catch (AggregateException e)
            {
                // Unwrap the first exception, a bit like await would.
                // It's very unlikely that we'd ever see an AggregateException without an inner exceptions,
                // but let's handle it relatively gracefully.
                throw e.InnerExceptions.FirstOrDefault() ?? e;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public async Task<Channel> GetChannelAsync(ServiceEndpoint endpoint, ChannelCredentials credentials)
        {
            try
            {
                return await Task.Run(() =>
                {
                    GaxPreconditions.CheckNotNull(endpoint, nameof(endpoint));
                    GaxPreconditions.CheckNotNull(credentials, nameof(credentials));
                    return GetChannelFromDict(endpoint, credentials);
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Channel> GetChannelAsync(ServiceEndpoint endpoint)
        {
            try
            {
                return await Task.Run(() =>
                {
                    GaxPreconditions.CheckNotNull(endpoint, nameof(endpoint));
                    return GetChannelFromDict(endpoint, ChannelCredentials.Insecure);
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Channel GetChannelFromDict(ServiceEndpoint endpoint, ChannelCredentials credentials)
        {
            try
            {
                lock (_lock)
                {
                    Channel channel;
                    if (!_channels.TryGetValue(endpoint, out channel))
                    {
                        var options = new[]
                       {
                            // "After a duration of this time the client/server pings its peer to see if the
                            // transport is still alive. Int valued, milliseconds."
                            // Required for any channel using a streaming RPC, to ensure an idle stream doesn't
                            // allow the TCP connection to be silently dropped by any intermediary network devices.
                            // 60 second keepalive time is reasonable. This will only add minimal network traffic,
                            // and only if the channel is idle for more than 60 seconds.
                            new ChannelOption("grpc.keepalive_time_ms", 60_000)
                        };
                        channel = new Channel(endpoint.Host, endpoint.Port, credentials, options);
                        _channels[endpoint] = channel;
                    }

                    return channel;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}