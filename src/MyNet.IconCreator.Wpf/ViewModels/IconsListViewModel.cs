// -----------------------------------------------------------------------
// <copyright file="IconsListViewModel.cs" company="Stéphane ANDRE">
// Copyright (c) Stéphane ANDRE. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using MyNet.UI.ViewModels.List;
using MyNet.UI.ViewModels.List.Filtering;
using MyNet.UI.ViewModels.List.Filtering.Filters;

namespace MyNet.IconCreator.Wpf.ViewModels;

internal sealed class IconsListViewModel(ICollection<IconData> collection) : SelectionListViewModel<IconData>(collection, new IconsControllerProvider(), selectionMode: UI.Selection.SelectionMode.Single);

internal sealed class IconsControllerProvider : ListParametersProvider
{
    public IconsControllerProvider()
        : base(nameof(IconData.Name)) { }

    public override IFiltersViewModel ProvideFilters() => new StringFilterViewModel(nameof(IconData.Aliases));
}

internal sealed class IconData(Func<object> buildIcon, string name, string[] aliases, string? data)
{
    private readonly Func<object> _buildIcon = buildIcon;

    public object Icon => _buildIcon();

    public string Name { get; } = name;

    public string[] Aliases { get; } = aliases;

    public string? Data { get; } = data;
}
