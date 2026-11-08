using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace DataDrop.Core;

public class Peer(ReadOnlySpan<byte> certificate, string password, int port) : IDisposable
{
    private TcpClient? _connection;
    private NetworkStream? _networkStream;
    private SslStream? _tls;
    private readonly X509Certificate _tlsServerCertificate = X509CertificateLoader.LoadPkcs12(certificate, password);
    public delegate Task Ready();
    public event Ready? OnReady;

    public async Task ConnectAsync(string hostname, int serverPort,
        CancellationToken cancellationToken = default)
    {
        _connection = new TcpClient();
        try
        {
            await _connection.ConnectAsync(hostname, serverPort, cancellationToken);
            _tls = new SslStream(_connection.GetStream(), false);
            await _tls.AuthenticateAsClientAsync(
                new SslClientAuthenticationOptions
                {
                    TargetHost = hostname,
                    EnabledSslProtocols = SslProtocols.None,
                    CertificateRevocationCheckMode = X509RevocationMode.Online,
                    RemoteCertificateValidationCallback = null
                }, cancellationToken);
        }
        catch
        {
            _tls?.DisposeAsync();
            _connection.Dispose();
            throw;
        }
        
        if (OnReady != null) await OnReady();
    }
    
    public async Task ListenAsync(CancellationToken cancellationToken = default)
    {
        using TcpListener listener = new(IPAddress.Any, port);
        listener.Start();
        _connection = await listener.AcceptTcpClientAsync(cancellationToken);
        _tls = new SslStream(_connection.GetStream(), false);
        try
        {
            await _tls.AuthenticateAsServerAsync(
                new SslServerAuthenticationOptions
                {
                    ServerCertificate = _tlsServerCertificate,
                    ClientCertificateRequired = false,
                    EnabledSslProtocols = SslProtocols.None
                }, cancellationToken
            );
        }
        catch
        {
            _tls.Dispose();
            _connection.Dispose();
            throw;
        }
    }

    public async Task ReadBytesAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (!IsReady()) throw new Exception("No connection established yet!");
        await _tls!.ReadExactlyAsync(buffer, cancellationToken);
    }

    public async Task WriteBytesAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (!IsReady()) throw new Exception("No connection established yet!");
        await _tls!.WriteAsync(buffer, cancellationToken);
    }

    public bool IsReady()
    {
        return _tls is { CanRead: true, CanWrite: true, IsAuthenticated: true };
    }

    public void Dispose()
    {
        _tls?.Dispose();
        _connection?.Dispose();
        _tlsServerCertificate.Dispose();
    }
    
    /* TODO: encryption
    PAKE over TCP(using BouncingCastle lib)
    Generate key from key material from PAKE
    Complete key confirmation
    Encrypt each message via AES-GCM and send over raw tcp stream
    Verify and decode data
    */
}
