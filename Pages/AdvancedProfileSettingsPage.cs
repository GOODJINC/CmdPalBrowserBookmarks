using CmdPalBrowserBookmarks.Settings;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks.Pages;

internal sealed partial class AdvancedProfileSettingsPage : ContentPage
{
    private readonly SettingsManager _settings;

    public AdvancedProfileSettingsPage(SettingsManager settings)
    {
        _settings = settings;
        Icon = Icons.Settings;
        Title = settings.Strings.AdvancedProfileSettings;
        Name = settings.Strings.AdvancedProfileSettings;
    }

    public override IContent[] GetContent()
    {
        return
        [
            new MarkdownContent
            {
                Body = _settings.Strings.AdvancedProfileSettingsNotice,
            },
            .. _settings.CreateProfileSettings().ToContent(),
        ];
    }
}
