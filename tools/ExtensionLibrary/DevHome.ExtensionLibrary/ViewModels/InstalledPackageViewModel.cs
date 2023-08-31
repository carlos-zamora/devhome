// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.System;

namespace DevHome.ExtensionLibrary.ViewModels;
public partial class InstalledPluginViewModel : ObservableObject
{
    [ObservableProperty]
    private string _displayName;

    [ObservableProperty]
    private string _packageFamilyName;

    public InstalledPluginViewModel(string displayName, string packageFamilyName)
    {
        _displayName = displayName;
        _packageFamilyName = packageFamilyName;
    }
}

public partial class InstalledPackageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _productId;

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _publisher;

    [ObservableProperty]
    private string _packageFamilyName;

    [ObservableProperty]
    private ObservableCollection<InstalledPluginViewModel> _installedPluginsList = new ();

    public InstalledPackageViewModel(string productId, string title, string publisher, string packageFamilyName)
    {
        _productId = productId;
        _title = title;
        _publisher = publisher;
        _packageFamilyName = packageFamilyName;
    }

    [RelayCommand]
    public async Task LaunchStoreButton(string packageId)
    {
        // Link format is: ms-windows-store://pdp/?ProductId=9N8MHTPHNGVV&mode=mini
        var linkString = $"ms-windows-store://pdp/?ProductId={packageId}&mode=mini";
        await Launcher.LaunchUriAsync(new (linkString));
    }
}
