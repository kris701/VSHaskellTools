using HaskellTools.Commands;
using HaskellTools.Options;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace HaskellTools
{
    [ProvideService(typeof(HaskellLanguageFactory), ServiceName = nameof(HaskellLanguageFactory))]
    [ProvideLanguageService(typeof(HaskellLanguageFactory), Constants.HaskellLanguageName, 0, 
        ShowHotURLs = false, DefaultToNonHotURLs = true, EnableLineNumbers = true, 
        EnableAsyncCompletion = true, EnableCommenting = true, ShowCompletion = true, 
        AutoOutlining = true, CodeSense = true, RequestStockColors = true, EnableFormatSelection = true
        )]
    [ProvideLanguageExtension(typeof(HaskellLanguageFactory), Constants.HaskellExt)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionPageGrid),
    "Haskell Tools", "Options", 0, 0, true)]
    [ProvideToolWindow(typeof(HaskellInteractiveWindow),Transient = true)]
    [ProvideToolWindow(typeof(GHCiDebuggerWindow), Transient = true)]
    public sealed class HaskellToolsPackage : AsyncPackage
    {
        #region Settings
        public string GHCUPPath
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                return page.GHCUPPath;
            }
        }

        public int HaskellFileExecutionTimeout
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                return page.HaskellFileExecutionTimeout;
            }
        }

        public string DebuggerEntryFunctionName
        {
            get
            {
                OptionPageGrid page = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                return page.DebuggerEntryFunctionName;
            }
        }
        #endregion

        public const string PackageGuidString = "6eaa553c-a41f-487b-99a1-a8383b6d1f74";

        #region Package Members

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            await RunHaskellFileCommand.InitializeAsync(this);
            await RunSelectedFunctionCommand.InitializeAsync(this);
            await GitHubCommand.InitializeAsync(this);
            await HaskellInteractiveWindowCommand.InitializeAsync(this);
            await GHCiDebuggerWindowCommand.InitializeAsync(this);
        }

        #endregion
    }
}
