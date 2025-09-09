// -----------------------------------------------------------------------
// <copyright file="ApplicationHostService.cs" company="Stéphane ANDRE">
// Copyright (c) Stéphane ANDRE. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Hosting;
using MyNet.Humanizer;
using MyNet.IconCreator.Wpf.Properties;
using MyNet.IconCreator.Wpf.Resources;
using MyNet.IconCreator.Wpf.ViewModels;
using MyNet.IconCreator.Wpf.Views;
using MyNet.UI.Commands;
using MyNet.UI.Dialogs;
using MyNet.UI.Dialogs.CustomDialogs;
using MyNet.UI.Dialogs.FileDialogs;
using MyNet.UI.Dialogs.MessageBox;
using MyNet.UI.Loading;
using MyNet.UI.Locators;
using MyNet.UI.Theming;
using MyNet.UI.Toasting;
using MyNet.Utilities.Localization;
using MyNet.Utilities.Logging;

namespace MyNet.IconCreator.Wpf.Services;

/// <summary>
/// Managed host of the application.
/// </summary>
internal sealed class ApplicationHostService : IHostedService
{
    public ApplicationHostService(
        IThemeService themeService,
        IToasterService toasterService,
        ICustomDialogService dialogService,
        IMessageBoxService messageBoxService,
        IFileDialogService fileDialogService,
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
        DialogManager.Initialize(dialogService, messageBoxService, fileDialogService, messageBoxFactory, viewResolver, viewLocator, viewModelLocator);
        BusyManager.Initialize(busyServiceFactory);
        CommandsManager.Initialize(commandFactory);
        UI.Threading.Scheduler.Initialize(uiScheduler);

        TranslationService.RegisterResources(nameof(IconCreatorResources), IconCreatorResources.ResourceManager);
    }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask.ConfigureAwait(false);

        LogStartApplication();

        InitializeAplication();

        using (LogManager.MeasureTime("Initializing application", TraceLevel.Info))
        {
            var window = new MainWindow();
            Application.Current.MainWindow = window;
            window.Closed += OnWindowsClosed;
            window.Show();
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        ViewModelManager.Get<MainViewModel>()!.SaveSettings();

        await Task.CompletedTask.ConfigureAwait(false);
    }

    private static void LogStartApplication()
    {
        var assembly = Assembly.GetEntryAssembly();

        var productAttr = assembly?.GetCustomAttribute<AssemblyProductAttribute>();
        LogManager.Info(new string('*', 80));
        LogManager.Info($"Start Application {productAttr?.Product} - Version {assembly?.GetName().Version?.ToString()}");
        LogManager.Info(new string('*', 80));
    }

    private static void InitializeAplication()
    {
        if (!string.IsNullOrEmpty(PreferencesSettings.Default.Language))
            GlobalizationService.Current.SetCulture(PreferencesSettings.Default.Language);

        ThemeManager.ApplyTheme(new Theme
        {
            Base = PreferencesSettings.Default.ThemeBase.DehumanizeTo<ThemeBase>(),
            PrimaryColor = PreferencesSettings.Default.ThemePrimaryColor,
            AccentColor = PreferencesSettings.Default.ThemeAccentColor
        });
    }

    private void OnWindowsClosed(object? sender, EventArgs e) => Application.Current.Shutdown();
}
