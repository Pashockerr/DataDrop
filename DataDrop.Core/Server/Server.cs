using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DataDrop.Core.Server;

public class Server : IDisposable
{
    private TcpListener? _listener;
    private TcpClient? _currentClient;

    public delegate Task ClientConnected();
    public event ClientConnected? OnClientConnected;
    
    public void StartServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
    }

    public async Task AcceptClientAsync()
    {
        if (_listener == null) return;
        _currentClient = await _listener.AcceptTcpClientAsync();
        if(OnClientConnected != null) await OnClientConnected();
    }

    public async Task SendMessageAsync(IMessage message)
    {
        if (_currentClient == null) throw new Exception("No active client connection!");
        await using var stream = _currentClient.GetStream();
        if(message is TextMessage textMessage)
            await stream.WriteAsync(Encoding.UTF8.GetBytes(textMessage.Text ?? ""));
    }

    public async Task ReceiveMessageAsync()
    {
        if (_currentClient == null) throw new Exception("No active client connection!");
        await using var stream = _currentClient.GetStream();
        if (_currentClient.Available > 0)
        {
            // stream.ReadAsync();     // TODO : Add encryption
        }
            
    }

    public void Dispose()
    {
        _listener?.Dispose();
        _currentClient?.Dispose();
    }
}