namespace WindowsThemeSwitcher;

using System.Runtime.InteropServices;
using Microsoft.Win32;
using Windows.Win32;
using Windows.Win32.Foundation;

class Theme
{
    public static readonly Theme Light = new();
    public static readonly Theme Dark = new();

    private const string AppsUseLightThemeValueName = "AppsUseLightTheme";
    private const string SystemUsesLightThemeValueName = "SystemUsesLightTheme";

    public static Theme AppsTheme
    {
        get => GetTheme(AppsUseLightThemeValueName);
        set => SetTheme(AppsUseLightThemeValueName, value);
    }

    public static Theme SystemTheme
    {
        get => GetTheme(SystemUsesLightThemeValueName);
        set => SetTheme(SystemUsesLightThemeValueName, value);
    }

    private static Theme GetTheme(string registryValueName)
    {
        using var key = OpenPersonalizeKey(writable: false);
        var value = key.GetValue(registryValueName) as int?;
        return value != 0 ? Light : Dark;
    }

    private static void SetTheme(string registryValueName, Theme theme)
    {
        using var key = OpenPersonalizeKey(writable: true);
        var value = (theme == Light) ? 1 : 0;
        key.SetValue(registryValueName, value, RegistryValueKind.DWord);
    }

    private static RegistryKey OpenPersonalizeKey(bool writable)
    {
        return Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", writable)
            ?? throw new Exception("Failed to open registry key.");
    }

    public static void NotifyThemeChange()
    {
        var result = PInvoke.SendNotifyMessage(
            hWnd: HWND.HWND_BROADCAST,
            Msg: PInvoke.WM_SETTINGCHANGE,
            wParam: 0,
            lParam: Marshal.StringToHGlobalUni("ImmersiveColorSet"));
        if (result == 0)
        {
            var error = Marshal.GetLastPInvokeErrorMessage();
            throw new Exception($"Failed to notify theme change: {error}");
        }
    }

    private Theme() { }
}
