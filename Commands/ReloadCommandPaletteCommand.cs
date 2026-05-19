using System.Diagnostics;
using CmdPalBrowserBookmarks.Settings;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks.Commands;

internal sealed partial class ReloadCommandPaletteCommand : InvokableCommand
{
    private readonly SettingsManager _settings;

    public ReloadCommandPaletteCommand(SettingsManager settings)
    {
        _settings = settings;
        Id = "CmdPalBrowserBookmarks.ReloadCommandPalette";
        Name = settings.Strings.ReloadCommandPalette;
        Icon = Icons.Refresh;
    }

    public override ICommandResult Invoke()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "x-cmdpal://reload",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal,
            });

            return CommandResult.Dismiss();
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast(_settings.Strings.FailedToReloadCommandPalette(ex.Message));
        }
    }
}
