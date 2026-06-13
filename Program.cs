using System.Reflection;
using Microsoft.Win32;

namespace WindowsThemeSwitcher;

class Program : ApplicationContext
{
    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(initiallyOwned: false, "WindowsThemeSwitcher_1b375734-2b71-4099-b371-5abf855a6626");
        if (!mutex.WaitOne(TimeSpan.Zero))
        {
            Environment.FailFast("Application is already running.");
        }

        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Program());
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    private readonly Icon moonIcon, sunIcon;
    private readonly NotifyIcon notifyIcon;

    private Program()
    {
        moonIcon = new Icon(GetResourceStream("moon.ico"));
        sunIcon = new Icon(GetResourceStream("sun.ico"));

        var startWithWindowsItem = new ToolStripMenuItem("Start with Windows")
        {
            Checked = Autostart,
            CheckOnClick = true,
        };
        startWithWindowsItem.CheckedChanged += (s, e) => Autostart = startWithWindowsItem.Checked;

        notifyIcon = new NotifyIcon()
        {
            Text = "Windows Theme Switcher",
            ContextMenuStrip = new ContextMenuStrip()
            {
                Items =
                {
                    startWithWindowsItem,
                    new ToolStripSeparator(),
                    new ToolStripMenuItem("Exit", null, (s, e) => Application.Exit()),
                },
            },
        };
        notifyIcon.DoubleClick += (s, e) => SwitchTheme();

        UpdateIcon();
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

    private const string AutostartRegistryKeyName = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AutostartRegistryValueName = "WindowsThemeSwitcher";

    private static bool Autostart
    {
        get => Registry.GetValue(AutostartRegistryKeyName, AutostartRegistryValueName, null) != null;
        set
        {
            if (value)
            {
                var processPath = Environment.ProcessPath ?? throw new InvalidOperationException("Can not get current process executable path.");
                Registry.SetValue(AutostartRegistryKeyName, AutostartRegistryValueName, processPath);
            }
            else
            {
                Registry.SetValue(AutostartRegistryKeyName, AutostartRegistryValueName, "");
            }
        }
    }
}
