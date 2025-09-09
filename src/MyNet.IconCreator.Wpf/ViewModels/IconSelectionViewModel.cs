// -----------------------------------------------------------------------
// <copyright file="IconSelectionViewModel.cs" company="Stéphane ANDRE">
// Copyright (c) Stéphane ANDRE. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Binding;
using MaterialDesignThemes.Wpf;
using MyNet.Observable;

namespace MyNet.IconCreator.Wpf.ViewModels;

internal sealed class IconSelectionViewModel : ObservableObject
{
    private static readonly IList<IconData> PackIconGroups = [.. Enum.GetNames<PackIconKind>()
        .GroupBy(Enum.Parse<PackIconKind>)
        .Select(g =>
        {
            PackIcon buildIcon() => new() { Kind = g.Key };

            return new IconData((Func<PackIcon>)buildIcon, g.Key.ToString(), [.. g.Order()], buildIcon().Data);
        })];

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Disposed in Cleanup method")]
    private readonly IDisposable _disposable;

    public IconsListViewModel IconsListViewModel { get; } = new(PackIconGroups);

    public bool ShowOtherData { get; set; }

    public string? OtherData { get; set; }

    public IconSelectionViewModel() => _disposable = IconsListViewModel.WhenPropertyChanged(x => x.SelectedItem).Subscribe(_ => OnPropertyChanged(nameof(SelectedData)));

    public string? SelectedData
    {
        get => ShowOtherData ? OtherData : IconsListViewModel.SelectedItem?.Data;
        set
        {
            var iconData = IconsListViewModel.Source.FirstOrDefault(x => x.Data == value);
            OtherData = iconData is null ? value : null;
            ShowOtherData = iconData is null;

            if (iconData is not null)
                IconsListViewModel.UpdateSelection([iconData]);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration", Justification = "Used by Fody")]
    private void OnShowOtherDataChanged() => OnPropertyChanged(nameof(SelectedData));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration", Justification = "Used by Fody")]
    private void OnOtherDataChanged() => OnPropertyChanged(nameof(SelectedData));

    protected override void Cleanup()
    {
        base.Cleanup();
        _disposable.Dispose();
    }
}
