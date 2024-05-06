// Copyright (c) Stéphane ANDRE. All Right Reserved.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using DynamicData;
using MyNet.Humanizer;
using MyNet.IconCreator.Wpf.Properties;
using MyNet.IconCreator.Wpf.Resources;
using MyNet.Observable;
using MyNet.UI.Busy;
using MyNet.UI.Commands;
using MyNet.UI.Dialogs;
using MyNet.UI.Theming;
using MyNet.UI.Toasting;
using MyNet.Utilities.Localization;
using PropertyChanged;

namespace MyNet.IconCreator.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public bool IsDark { get; set; }

        public CultureInfo? SelectedCulture { get; set; }

        public ObservableCollection<CultureInfo?> Cultures { get; private set; } = [];

        public ICommand IsDarkCommand { get; }

        public ICommand IsLightCommand { get; }

        public ICommand ExitCommand { get; }

        public string Title { get; }

        public bool IsDebug { get; }

        public IBusyService BusyService { get; }

        public IDialogService DialogService { get; }

        public MainWindowViewModel(IBusyService busyService, IDialogService dialogService)
        {
            BusyService = busyService;
            DialogService = dialogService;
            var assembly = Assembly.GetEntryAssembly();
            Title = assembly?.GetName().Name?.Humanize().ToTitle() ?? string.Empty;

#if DEBUG
            IsDebug = true;
#endif

            ExitCommand = CommandsManager.Create(() => Application.Current.Shutdown(), () => !DialogManager.HasOpenedDialogs);
            IsDarkCommand = CommandsManager.Create(() => IsDark = true);
            IsLightCommand = CommandsManager.Create(() => IsDark = false);

            using (PropertyChangedSuspender.Suspend())
            {
                Cultures.AddRange(CultureInfoService.Current.SupportedCultures);
                IsDark = ThemeManager.CurrentTheme?.Base == ThemeBase.Dark;
                SelectedCulture = string.IsNullOrEmpty(TranslationService.Current.Culture.Name) ? null : GetSelectedCulture(CultureInfo.GetCultureInfo(TranslationService.Current.Culture.Name));
            }

            ThemeManager.ThemeChanged += OnThemeChanged;
            CultureInfoService.Current.CultureChanged += OnCultureChanged;
        }

        [SuppressPropertyChangedWarnings]
        protected void OnIsDarkChanged()
        {
            if (PropertyChangedSuspender.IsSuspended) return;

            var theme = IsDark ? ThemeBase.Dark : ThemeBase.Light;
            ThemeManager.ApplyBase(theme);
            PreferencesSettings.Default.ThemeBase = theme.ToString();

            PreferencesSettings.Default.Save();

            ToasterManager.ShowSuccess(IconCreatorResources.SettingsHaveBeenSaved);
        }

        private CultureInfo? GetSelectedCulture(CultureInfo culture) => Cultures.Contains(culture) ? culture : culture.Parent is not null ? GetSelectedCulture(culture.Parent) : null;

        protected virtual void OnSelectedCultureChanged()
        {
            if (PropertyChangedSuspender.IsSuspended) return;

            var cultureInfo = SelectedCulture ?? CultureInfo.InstalledUICulture;
            CultureInfoService.Current.SetCulture(cultureInfo);
            PreferencesSettings.Default.Language = cultureInfo.ToString();

            PreferencesSettings.Default.Save();

            ToasterManager.ShowSuccess(IconCreatorResources.SettingsHaveBeenSaved);
        }

        [SuppressPropertyChangedWarnings]
        private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            using (PropertyChangedSuspender.Suspend())
                IsDark = e.CurrentTheme?.Base == ThemeBase.Dark;
        }

        [SuppressPropertyChangedWarnings]
        private void OnCultureChanged(object? sender, EventArgs e)
        {
            using (PropertyChangedSuspender.Suspend())
                SelectedCulture = GetSelectedCulture(CultureInfo.CurrentCulture);
        }

        protected override void Cleanup()
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;
            CultureInfoService.Current.CultureChanged -= OnCultureChanged;
            base.Cleanup();
        }
    }
}
