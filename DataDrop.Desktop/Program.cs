namespace DataDrop.Desktop;

internal static class Program
{
    private static int Main()
    {
        using var application = Gtk.Application.New("com.datadrop.Desktop", Gio.ApplicationFlags.FlagsNone);
        application.OnActivate += (_, _) =>
        {
            var window = application.ActiveWindow ?? MainWindow.Create(application);
            window.Present();
        };

        return application.RunWithSynchronizationContext(null);
    }
}
