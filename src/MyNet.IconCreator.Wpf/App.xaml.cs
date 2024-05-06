// Copyright (c) Stéphane ANDRE. All Right Reserved.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyNet.IconCreator.Services;
using MyNet.IconCreator.ViewModels;
using MyNet.UI.Busy;
using MyNet.UI.Commands;
using MyNet.UI.Dialogs;
using MyNet.UI.Dialogs.Settings;
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

namespace MyNet.IconCreator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly IHost Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();

                Logger.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/config/NLog.config"));
                logging.AddProvider(new LoggerProvider());
            })
            .ConfigureServices((context, services) => services

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
            .AddSingleton<IDialogService, OverlayDialogService>()
            .AddSingleton(x => BusyManager.Create())
            .AddScoped<IBusyServiceFactory, BusyServiceFactory>()
            .AddScoped<IMessageBoxFactory, MessageBoxFactory>()
            .AddScoped<IScheduler, WpfScheduler>(_ => WpfScheduler.Current)
            .AddScoped<ICommandFactory, WpfCommandFactory>()

            // ViewModels
            .AddSingleton<MainWindowViewModel>()
            .AddSingleton<MainViewModel>()

            ).Build();

        static App() => AppDomain.CurrentDomain.UnhandledException += async (sender, e) => await ShowExceptionAsync((Exception)e.ExceptionObject).ConfigureAwait(false);

        protected App() => DispatcherUnhandledException += async (sender, e) =>
        {
            e.Handled = true;
            await ShowExceptionAsync(e.Exception).ConfigureAwait(false);
        };

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            await Host.StartAsync();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            await Host.StopAsync();

            Host.Dispose();
        }

        private static async Task ShowExceptionAsync(Exception e)
        {
            var exception = e.InnerException ?? e;
            LogManager.Fatal(exception);

            // If Binding error
            if (exception is ResourceReferenceKeyNotFoundException or XamlParseException) return;

            if (await DialogManager.ShowErrorAsync(MessageResources.UnexpectedXError.FormatWith(exception.Message), buttons: MessageBoxResultOption.OkCancel).ConfigureAwait(false) == MyNet.UI.Dialogs.MessageBoxResult.Cancel)
                Current.Shutdown();
        }
    }
}
