// -----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Stéphane ANDRE">
// Copyright (c) Stéphane ANDRE. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyNet.IconCreator.Wpf.Services;
using MyNet.IconCreator.Wpf.ViewModels;
using MyNet.UI.Commands;
using MyNet.UI.Dialogs;
using MyNet.UI.Dialogs.ContentDialogs;
using MyNet.UI.Dialogs.FileDialogs;
using MyNet.UI.Dialogs.MessageBox;
using MyNet.UI.Loading;
using MyNet.UI.Locators;
using MyNet.UI.Resources;
using MyNet.UI.Theming;
using MyNet.UI.Toasting;
using MyNet.Utilities;
using MyNet.Utilities.Logging;
using MyNet.Utilities.Logging.NLog;
using MyNet.Wpf.Busy;
using MyNet.Wpf.Commands;
using MyNet.Wpf.Dialogs;
using MyNet.Wpf.Schedulers;
using MyNet.Wpf.Theming;
using MyNet.Wpf.Toasting;

namespace MyNet.IconCreator;

/// <summary>
/// Interaction logic for App.xaml.
/// </summary>
public partial class App : Application
{
    private static readonly IHost Host = Microsoft.Extensions.Hosting.Host
        .CreateDefaultBuilder()
        .ConfigureLogging((_, logging) =>
        {
            logging.ClearProviders();

            Logger.LoadConfiguration($"{Directory.GetCurrentDirectory()}/config/NLog.config");
            logging.AddProvider(new LoggerProvider());
        })
        .ConfigureServices((__, services) => services

        // App Host
        .AddHostedService<ApplicationHostService>()

        // Services
        .AddSingleton<Utilities.Logging.ILogger, Logger>()
        .AddSingleton<IViewModelResolver, ViewModelResolver>()
        .AddSingleton<IViewModelLocator, ViewModelLocator>(x => new ViewModelLocator(x))
        .AddSingleton<IViewLocator, ViewLocator>()
        .AddSingleton<IViewResolver, ViewResolver>()
        .AddSingleton<IThemeService, ThemeService>()
        .AddSingleton<IToasterService, ToasterService>()
        .AddSingleton<IContentDialogService, OverlayDialogService>()
        .AddSingleton<IMessageBoxService, OverlayDialogService>()
        .AddSingleton<IFileDialogService, FileDialogService>()
        .AddSingleton<IDialogService, DialogService>()
        .AddSingleton(_ => BusyManager.Create())
        .AddScoped<IBusyServiceFactory, BusyServiceFactory>()
        .AddScoped<IMessageBoxFactory, MessageBoxFactory>()
        .AddScoped<IScheduler, WpfScheduler>(_ => WpfScheduler.Current)
        .AddScoped<ICommandFactory, WpfCommandFactory>()

        // ViewModels
        .AddSingleton<MainWindowViewModel>()
        .AddSingleton<MainViewModel>()).Build();

    static App() => AppDomain.CurrentDomain.UnhandledException += async (sender, e) => await ShowExceptionAsync((Exception)e.ExceptionObject).ConfigureAwait(false);

    protected App() => DispatcherUnhandledException += async (sender, e) =>
    {
        e.Handled = true;
        await ShowExceptionAsync(e.Exception).ConfigureAwait(false);
    };

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        await Host.StartAsync().ConfigureAwait(false);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        await Host.StopAsync().ConfigureAwait(false);

        Host.Dispose();
    }

    private static async Task ShowExceptionAsync(Exception e)
    {
        var exception = e.InnerException ?? e;
        LogManager.Fatal(exception);

        // If Binding error
        if (exception is ResourceReferenceKeyNotFoundException or XamlParseException) return;

        if (await DialogManager.ShowErrorAsync(MessageResources.UnexpectedXError.FormatWith(exception.Message), buttons: MessageBoxResultOption.OkCancel).ConfigureAwait(false) == UI.Dialogs.MessageBox.MessageBoxResult.Cancel)
            Current.Shutdown();
    }
}
