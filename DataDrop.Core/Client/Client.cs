using System.Net.Sockets;
using System.Text;

namespace DataDrop.Core.Client;

public class Client : IDisposable
{
    private TcpClient? _client;

    public delegate Task ConnectedToServer();
    public event ConnectedToServer? OnConnectedToServer;
    
    public async Task ConnectToServerAsync(string hostname, int port)
    {
        _client = new TcpClient();
        await _client.ConnectAsync(hostname, port);
        if(OnConnectedToServer != null) await OnConnectedToServer();
    }

    public async Task<IMessage> GetTextMessageAsync()
    {
        if(_client is not { Connected: true }) throw new Exception("Client not connected"); 
        await using var stream = _client.GetStream();
        byte[] buffer = new byte[1024];
        await stream.ReadAsync(new Memory<byte>(buffer, 0, 1024));
        return new TextMessage(Encoding.UTF8.GetString(buffer));
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}