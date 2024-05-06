// Copyright (c) Stéphane ANDRE. All Right Reserved.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using MyNet.UI.ViewModels.List;
using MyNet.UI.ViewModels.List.Filtering;
using MyNet.UI.ViewModels.List.Filtering.Filters;

namespace MyNet.IconCreator.ViewModels
{
    internal class IconsListViewModel : SelectionListViewModel<IconData>
    {
        public IconsListViewModel(ICollection<IconData> collection) : base(collection, new IconsControllerProvider(), selectionMode: UI.Selection.SelectionMode.Single) { }
    }

    internal class IconsControllerProvider : ListParametersProvider
    {
        public IconsControllerProvider() : base(nameof(IconData.Name)) { }

        public override IFiltersViewModel ProvideFilters() => new StringFilterViewModel(nameof(IconData.Aliases));
    }

    internal class IconData
    {
        private readonly Func<object> _buildIcon;

        public IconData(Func<object> buildIcon, string name, string[] aliases, string? data)
        {
            _buildIcon = buildIcon;
            Name = name;
            Aliases = aliases;
            Data = data;
        }

        public object Icon => _buildIcon();

        public string Name { get; }

        public string[] Aliases { get; }

        public string? Data { get; }
    }
}
