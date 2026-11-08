namespace DataDrop.Desktop;

internal static class MainWindow
{
    public static Gtk.ApplicationWindow Create(Gtk.Application application)
    {
        string layoutPath = Path.Combine(AppContext.BaseDirectory, "ui", "MainWindow.ui");
        using var builder = Gtk.Builder.NewFromFile(layoutPath);
        var window = builder.GetObject("main_window") as Gtk.ApplicationWindow
            ?? throw new InvalidOperationException("MainWindow.ui must define main_window as a GtkApplicationWindow.");


        window.Application = application;
        return window;
    }


}
