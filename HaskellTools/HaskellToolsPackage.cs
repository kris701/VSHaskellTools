using EnvDTE;
using HaskellTools.Checkers;
using HaskellTools.Commands;
using HaskellTools.ErrorList;
using HaskellTools.Options;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.IO.Packaging;
using System.Management.Instrumentation;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace HaskellTools
{
    [ProvideService(typeof(HaskellLanguageFactory), ServiceName = nameof(HaskellLanguageFactory))]
    [ProvideLanguageService(typeof(HaskellLanguageFactory), Constants.HaskellLanguageName, Constants.HaskellLanguageID,
        ShowHotURLs = false, DefaultToNonHotURLs = true, EnableLineNumbers = true,
        EnableAsyncCompletion = true, EnableCommenting = true, ShowCompletion = true,
        AutoOutlining = true, CodeSense = true, RequestStockColors = true, EnableFormatSelection = true,
        QuickInfo = true, ShowDropDownOptions = true, ShowMatchingBrace = true
        )]
    [ProvideLanguageExtension(typeof(HaskellLanguageFactory), Constants.HaskellExt)]

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(Constants.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionPageGrid),
    "Haskell Tools", "Options", 0, 0, true)]
    [ProvideToolWindow(typeof(HaskellInteractiveWindow),Transient = true, Style = VsDockStyle.MDI, Width = 1200, Height = 800, Orientation = ToolWindowOrientation.Bottom)]
    [ProvideToolWindow(typeof(GHCiDebuggerWindow), Transient = true, Style = VsDockStyle.MDI, Width = 1200, Height = 800)]
    [ProvideToolWindow(typeof(InstallGHCiWindow), Transient = true, Style = VsDockStyle.MDI, Width = 1200, Height = 800)]
    [ProvideToolWindow(typeof(WelcomeWindow), Transient = true, Style = VsDockStyle.MDI, Width = 1200, Height = 800)]
    public sealed class HaskellToolsPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
            OptionsAccessor.Instance = page;

            await WelcomeWindowCommand.InitializeAsync(this);
            await GitHubCommand.InitializeAsync(this);

            if (OptionsAccessor.CheckForGHCiAtStartup || !OptionsAccessor.GHCiFound)
            {
                var checker = new GHCiChecker(this);
                await checker.CheckForGHCiAsync();
            }
            if (OptionsAccessor.GHCiFound)
            {
                if (OptionsAccessor.IsFirstStart)
                    WelcomeWindowCommand.Instance.Execute(null, null);

                await RunHaskellFileCommand.InitializeAsync(this);
                await RunSelectedFunctionCommand.InitializeAsync(this);
                await HaskellInteractiveWindowCommand.InitializeAsync(this);
                await GHCiDebuggerWindowCommand.InitializeAsync(this);
                new GHCiErrorManager(this);
            }
        }
    }
}
