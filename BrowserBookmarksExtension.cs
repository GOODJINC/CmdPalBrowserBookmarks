using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CommandPalette.Extensions;

namespace CmdPalBrowserBookmarks;

[ComVisible(true)]
[Guid("F9BBE047-9AE1-4F40-A35D-5B8F89133E75")]
[ComDefaultInterface(typeof(IExtension))]
public sealed partial class BrowserBookmarksExtension : IExtension, IDisposable
{
    private readonly ManualResetEvent _extensionDisposedEvent;
    private readonly BrowserBookmarksCommandsProvider _provider = new();

    public BrowserBookmarksExtension(ManualResetEvent extensionDisposedEvent)
    {
        _extensionDisposedEvent = extensionDisposedEvent;
    }

    public object? GetProvider(ProviderType providerType)
    {
        return providerType == ProviderType.Commands ? _provider : null;
    }

    public void Dispose()
    {
        _extensionDisposedEvent.Set();
    }
}
