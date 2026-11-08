using DataDrop.Core;

namespace DataDrop.Desktop;

public class StateManager
{
    public const int ConnectTimeout = 10_000;
    
    public enum State
    {
        Idle,
        Listening,
        Connecting,
        Sending,
        Receiving,
        GeneratingCertificate
    }
    
    private static Peer? _peer;
    public static State CurrentState { get; private set; }

    // public static async Task Connect(string hostname, int port)
    // {
    //     if (CurrentState != State.Idle) // TODO: Maybe add exception??
    //         return;
    //     using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(ConnectTimeout));
    //     _peer = new Peer()
    //     await _peer.ConnectAsync(hostname, port, cts.Token);
    // }
}