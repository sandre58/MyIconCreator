// Copyright (c) Stéphane ANDRE. All Right Reserved.
// See the LICENSE file in the project root for more information.

using System;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Hosting;
using MyNet.Humanizer;
using MyNet.IconCreator.ViewModels;
using MyNet.IconCreator.Views;
using MyNet.IconCreator.Wpf.Properties;
using MyNet.IconCreator.Wpf.Resources;
using MyNet.UI.Busy;
using MyNet.UI.Commands;
using MyNet.UI.Dialogs;
using MyNet.UI.Locators;
using MyNet.UI.Theming;
using MyNet.UI.Toasting;
using MyNet.Utilities.Localization;
using MyNet.Utilities.Logging;

namespace MyNet.IconCreator.Services;

/// <summary>
/// Managed host of the application.
/// </summary>
internal class ApplicationHostService : IHostedService
{
    public ApplicationHostService(
        IThemeService themeService,
        IToasterService toasterService,
        IDialogService dialogService,
        IViewModelResolver viewModelResolver,
        IViewModelLocator viewModelLocator,
        IViewResolver viewResolver,
        IViewLocator viewLocator,
        IBusyServiceFactory busyServiceFactory,
        IMessageBoxFactory messageBoxFactory,
        ICommandFactory commandFactory,
        IScheduler uiScheduler,
        ILogger logger)
    {
        LogManager.Initialize(logger);
        ViewModelManager.Initialize(viewModelResolver, viewModelLocator);
        ViewManager.Initialize(viewResolver, viewLocator);
        ThemeManager.Initialize(themeService);
        ToasterManager.Initialize(toasterService);
        DialogManager.Initialize(dialogService, messageBoxFactory, viewResolver, viewLocator, viewModelLocator);
        BusyManager.Initialize(busyServiceFactory);
        CommandsManager.Initialize(commandFactory);
        Observable.Threading.Scheduler.Initialize(uiScheduler);

        TranslationService.RegisterResources(nameof(IconCreatorResources), IconCreatorResources.ResourceManager);
    }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        LogStartApplication();

        InitializeAplication();

        using (LogManager.MeasureTime("Initializing application", TraceLevel.Info))
        {
            var window = new MainWindow();
            Application.Current.MainWindow = window;
            window.Closed += OnWindowsClosed;
            window.Show();
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        ViewModelManager.Get<MainViewModel>()!.SaveSettings();

        await Task.CompletedTask;
    }

    private static void InitializeAplication()
    {
        if (!string.IsNullOrEmpty(PreferencesSettings.Default.Language))
            CultureInfoService.Current.SetCulture(PreferencesSettings.Default.Language);

        ThemeManager.ApplyTheme(new Theme
        {
            Base = PreferencesSettings.Default.ThemeBase.DehumanizeTo<ThemeBase>(),
            PrimaryColor = PreferencesSettings.Default.ThemePrimaryColor,
            AccentColor = PreferencesSettings.Default.ThemeAccentColor
        });
    }

    private void OnWindowsClosed(object? sender, EventArgs e) => Application.Current.Shutdown();

    private static void LogStartApplication()
    {
        var assembly = Assembly.GetEntryAssembly();

        var productAttr = assembly?.GetCustomAttribute<AssemblyProductAttribute>();
        LogManager.Info(new string('*', 80));
        LogManager.Info($"Start Application {productAttr?.Product} - Version {assembly?.GetName().Version?.ToString()}");
        LogManager.Info(new string('*', 80));
    }
}
