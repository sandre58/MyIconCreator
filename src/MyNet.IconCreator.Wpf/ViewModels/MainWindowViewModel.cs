// -----------------------------------------------------------------------
// <copyright file="MainWindowViewModel.cs" company="Stéphane ANDRE">
// Copyright (c) Stéphane ANDRE. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using DynamicData;
using MyNet.Humanizer;
using MyNet.IconCreator.Wpf.Properties;
using MyNet.IconCreator.Wpf.Resources;
using MyNet.Observable;
using MyNet.UI.Commands;
using MyNet.UI.Dialogs;
using MyNet.UI.Dialogs.ContentDialogs;
using MyNet.UI.Loading;
using MyNet.UI.Theming;
using MyNet.UI.Toasting;
using MyNet.Utilities.Localization;
using PropertyChanged;

namespace MyNet.IconCreator.Wpf.ViewModels;

internal sealed class MainWindowViewModel : ObservableObject
{
    public bool IsDark { get; set; }

    public CultureInfo? SelectedCulture { get; set; }

    public ObservableCollection<CultureInfo?> Cultures { get; } = [];

    public ICommand IsDarkCommand { get; }

    public ICommand IsLightCommand { get; }

    public ICommand ExitCommand { get; }

    public string Title { get; }

    public bool IsDebug { get; }

    public IBusyService BusyService { get; }

    public IContentDialogService DialogService { get; }

    public MainWindowViewModel(IBusyService busyService, IContentDialogService dialogService)
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
            Cultures.AddRange(GlobalizationService.Current.SupportedCultures);
            IsDark = ThemeManager.CurrentTheme?.Base == ThemeBase.Dark;
            SelectedCulture = string.IsNullOrEmpty(TranslationService.Current.Culture.Name) ? null : GetSelectedCulture(CultureInfo.GetCultureInfo(TranslationService.Current.Culture.Name));
        }

        ThemeManager.ThemeChanged += OnThemeChanged;
        GlobalizationService.Current.CultureChanged += OnCultureChanged;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration", Justification = "Used by Fody")]
    private void OnIsDarkChanged()
    {
        if (PropertyChangedSuspender.IsSuspended) return;

        var theme = IsDark ? ThemeBase.Dark : ThemeBase.Light;
        ThemeManager.ApplyBase(theme);
        PreferencesSettings.Default.ThemeBase = theme.ToString();

        PreferencesSettings.Default.Save();

        ToasterManager.ShowSuccess(IconCreatorResources.SettingsHaveBeenSaved);
    }

    private CultureInfo? GetSelectedCulture(CultureInfo culture) => Cultures.Contains(culture) ? culture : culture.Parent is not null ? GetSelectedCulture(culture.Parent) : null;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration", Justification = "Used by Fody")]
    private void OnSelectedCultureChanged()
    {
        if (PropertyChangedSuspender.IsSuspended) return;

        var cultureInfo = SelectedCulture ?? CultureInfo.InstalledUICulture;
        GlobalizationService.Current.SetCulture(cultureInfo);
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
        GlobalizationService.Current.CultureChanged -= OnCultureChanged;
        base.Cleanup();
    }
}
