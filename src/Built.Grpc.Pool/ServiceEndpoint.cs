using System;

namespace Built.Grpc.Pool
{
    /// <summary>
    /// Settings specifying a service endpoint in the form of a host name and port.
    /// This class is immutable and thread-safe.
    /// </summary>
    public sealed class ServiceEndpoint : IEquatable<ServiceEndpoint>
    {
        /// <summary>
        /// The host name to connect to. Never null or empty.
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// The port to connect to, in the range 1 to 65535 inclusive.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Creates a new endpoint with the given host and port.
        /// </summary>
        /// <param name="host">The host name to connect to. Must not be null or empty.</param>
        /// <param name="port">The port to connect to, in the range 1 to 65535 inclusive.</param>
        public ServiceEndpoint(string host, int port)
        {
            Host = GaxPreconditions.CheckNotNullOrEmpty(host, nameof(host));
            Port = GaxPreconditions.CheckArgumentRange(port, nameof(port), 1, 65535);
        }

        /// <summary>
        /// Creates a new endpoint with the same port but the given host.
        /// </summary>
        /// <param name="host">The host name to connect to. Must not be null or empty.</param>
        /// <returns>A new endpoint with the current port and the specified host.</returns>
        public ServiceEndpoint WithHost(string host) => new ServiceEndpoint(host, Port);

        /// <summary>
        /// Creates a new endpoint with the same host but the given port.
        /// </summary>
        /// <param name="port">The port to connect to, in the range 1 to 65535 inclusive.</param>
        /// <returns>A new endpoint with the current host and the specified port.</returns>
        public ServiceEndpoint WithPort(int port) => new ServiceEndpoint(Host, port);

        /// <summary>
        /// Returns this endpoint's data in the format "host:port".
        /// </summary>
        /// <returns>This endpoint's data in the format "host:port".</returns>
        public override string ToString() => $"{Host}:{Port}";

        /// <summary>
        /// Determines equality between this object and <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">The object to compare with this one.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="ServiceEndpoint"/>
        /// with the same host and port; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj) => Equals(obj as ServiceEndpoint);

        /// <summary>
        /// Returns a hash code for this object, consistent with <see cref="Equals(ServiceEndpoint)"/>.
        /// </summary>
        /// <returns>A hash code for this object.</returns>
        public override int GetHashCode() => unchecked(Host.GetHashCode() * 31 + Port);

        /// <summary>
        /// Determines equality between this endpoint and <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The object to compare with this one.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is a <see cref="ServiceEndpoint"/>
        /// with the same host and port; <c>false</c> otherwise.</returns>
        public bool Equals(ServiceEndpoint other) => other != null && other.Host == Host && other.Port == Port;
    }
}