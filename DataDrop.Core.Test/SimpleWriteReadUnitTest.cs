namespace DataDrop.Core.Test;

using Core;

[TestFixture]
public class SimpleWriteReadUnitTest
{
    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    public async Task SendAndRecieveAsync()
    {
        Server.Server server = new();
        server.StartServer(1487);
        server.OnClientConnected += async () => await server.SendMessageAsync(new TextMessage("Test luptidoo"));
        Task acceptTask = server.AcceptClientAsync();

        Client.Client client = new();
        client.OnConnectedToServer += async () =>
        {
            var receivedMessage = await client.GetTextMessageAsync();
            if(receivedMessage is TextMessage textMessage)
                Console.WriteLine($"Got message: {textMessage.Text}");
        };
        await client.ConnectToServerAsync("127.0.0.1",  1487);
        await acceptTask;
        
        Assert.Pass();
    }
}