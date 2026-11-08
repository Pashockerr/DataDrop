using DataDrop.Core;
using Gtk;

namespace DataDrop.Desktop;

public class MainWindowEventHandler
{
    public static void ConnectEvents(Builder builder)
    {
        var connectEntry = builder.GetObject("connect_entry") as Gtk.Entry
                           ?? throw new InvalidOperationException("MainWindow.ui must define connect_entry as a GtkEntry.");
        var serverEntry = builder.GetObject("server_entry") as Gtk.Entry
                          ?? throw new InvalidOperationException("MainWindow.ui must define server_entry as a GtkEntry.");
        var connectButton = builder.GetObject("connect_button") as Gtk.Button
                            ?? throw new InvalidOperationException("MainWindow.ui must define connect_button as a GtkButton.");
        var serverButton = builder.GetObject("server_button") as Gtk.Button
                           ?? throw new InvalidOperationException("MainWindow.ui must define server_button as a GtkButton.");

        connectButton.OnClicked += (_, _) => OnConnectClicked(connectEntry.GetText());
        serverButton.OnClicked += (_, _) => OnStartServerClicked(serverEntry.GetText());
    }
    
    
    private static void OnConnectClicked(string port)
    {
        // TODO: Connect to a peer using the entered port.
        Console.WriteLine($"Connect clicked. Port: {port}");
    }

    private static void OnStartServerClicked(string port)
    {
        Console.WriteLine($"Start server clicked. Port: {port}");
    }
}