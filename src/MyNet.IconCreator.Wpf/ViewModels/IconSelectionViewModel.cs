// Copyright (c) Stéphane ANDRE. All Right Reserved.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Binding;
using MaterialDesignThemes.Wpf;
using MyNet.Observable;

namespace MyNet.IconCreator.ViewModels
{
    internal class IconSelectionViewModel : ObservableObject
    {
        private static readonly IList<IconData> PackIconGroups = Enum.GetNames<PackIconKind>()
            .GroupBy(k => (PackIconKind)Enum.Parse(typeof(PackIconKind), k))
            .Select(g =>
            {
                PackIcon buildIcon() => new() { Kind = g.Key };

                return new IconData((Func<PackIcon>)buildIcon, g.Key.ToString(), [.. g.OrderBy(x => x)], buildIcon().Data);
            })
            .ToList();
        private readonly IDisposable _disposable;

        public IconsListViewModel IconsListViewModel { get; } = new(PackIconGroups);

        public bool ShowOtherData { get; set; }

        public string? OtherData { get; set; }

        public IconSelectionViewModel() => _disposable = IconsListViewModel.WhenPropertyChanged(x => x.SelectedItem).Subscribe(_ => RaisePropertyChanged(nameof(SelectedData)));

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

        protected virtual void OnShowOtherDataChanged() => RaisePropertyChanged(nameof(SelectedData));

        protected virtual void OnOtherDataChanged() => RaisePropertyChanged(nameof(SelectedData));

        protected override void Cleanup()
        {
            base.Cleanup();
            _disposable.Dispose();
        }
    }
}
