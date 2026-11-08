using System.Net.Sockets;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement.JPake;

namespace DataDrop.Core;

public class SecureStream(NetworkStream networkStream)
{
    private NetworkStream? _networkStream = networkStream;

    public bool IsReady()
    {
        return _networkStream is { CanRead: true, CanWrite: true };
    }

    public async Task PairAsync(string password)
    {
        var participnat = new JPakeParticipant(
            participantId: "client",
            password: password.ToCharArray()
            );

        var r1Payload = participnat.CreateRound1PayloadToSend();
        
    }
}