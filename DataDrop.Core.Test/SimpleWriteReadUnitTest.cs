using System.Security.Authentication;

namespace DataDrop.Core.Test;

using Core;

[TestFixture]
public class SimpleWriteReadUnitTest
{
    [Test]
    public async Task RejectsUntrustedServerAsync()
    {
        var serverCerts = SecurityManager.GenerateCertificate("server-password");
        var clientCerts = SecurityManager.GenerateCertificate("client-password");
        using Peer server = new(serverCerts.PrivateCert, "server-password", 1337);
        using Peer client = new(clientCerts.PrivateCert, "client-password", 1337);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        Task listenTask = server.ListenAsync(timeout.Token);
        Task connectTask = client.ConnectAsync("localhost", 1337, timeout.Token);

        Assert.ThrowsAsync<AuthenticationException>(async () => await connectTask);
        // Observe both tasks: the server can finish TLS before learning of rejection.
        try
        {
            await Task.WhenAll(listenTask, connectTask);
        }
        catch (AuthenticationException)
        {
        }
        Assert.That(client.IsReady(), Is.False);
    }

    [Test]
    public async Task ListenCanBeCancelledAsync()
    {
        var certs = SecurityManager.GenerateCertificate("password");
        using Peer server = new(certs.PrivateCert, "password", 1337);
        using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        Assert.CatchAsync<OperationCanceledException>(async () =>
            await server.ListenAsync(timeout.Token));
        Assert.That(server.IsReady(), Is.False);
    }
}
