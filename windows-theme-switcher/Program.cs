using System.Reflection;
using Microsoft.Win32;

namespace WindowsThemeSwitcher;

class Program : ApplicationContext
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Program());
    }

    private readonly Icon moonIcon, sunIcon;
    private readonly NotifyIcon notifyIcon;

    private Program()
    {
        moonIcon = new Icon(GetResourceStream("moon.ico"));
        sunIcon = new Icon(GetResourceStream("sun.ico"));

        notifyIcon = new NotifyIcon();
        UpdateIcon();
        notifyIcon.DoubleClick += (s, e) => SwitchTheme();
        notifyIcon.Visible = true;

        SystemEvents.UserPreferenceChanged += (s, e) => UpdateIcon();
    }

    private static Stream GetResourceStream(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream($"WindowsThemeSwitcher.Resources.{name}");
        return stream ?? throw new Exception($"Failed to load resource '{name}'.");
    }

    private static void SwitchTheme()
    {
        var currentAppsTheme = Theme.AppsTheme;
        var newTheme = currentAppsTheme == Theme.Light ? Theme.Dark : Theme.Light;
        Theme.AppsTheme = newTheme;
        Theme.SystemTheme = newTheme;
        Theme.NotifyThemeChange();
    }

    private void UpdateIcon()
    {
        var currentSystemTheme = Theme.SystemTheme;
        notifyIcon.Icon = currentSystemTheme == Theme.Light ? sunIcon : moonIcon;
    }
}
