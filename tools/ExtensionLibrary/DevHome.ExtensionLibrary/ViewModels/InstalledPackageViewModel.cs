// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHome.Common.Extensions;
using DevHome.Common.Services;
using Microsoft.UI.Xaml;
using Windows.System;

namespace DevHome.ExtensionLibrary.ViewModels;
public partial class InstalledExtensionViewModel : ObservableObject
{
    [ObservableProperty]
    private string _displayName;

    [ObservableProperty]
    private string _packageFamilyName;

    [ObservableProperty]
    private bool _hasSettingsProvider;

    public InstalledExtensionViewModel(string displayName, string packageFamilyName, bool hasSettingsProvider)
    {
        _displayName = displayName;
        _packageFamilyName = packageFamilyName;
        _hasSettingsProvider = hasSettingsProvider;
    }

    [RelayCommand]
    private void NavigateSettings()
    {
        ////var navigationService = Application.Current.GetService<INavigationService>();
        ////var segments = path.Split("/");
        ////navigationService.NavigateTo(typeof(ExtensionSettingsViewModel).FullName!, segments[1]);
        ////_extensionsViewModel.Navigate(_setting.Path);
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
    private ObservableCollection<InstalledExtensionViewModel> _installedExtensionsList = new ();

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
        var linkString = $"ms-windows-store://pdp/?ProductId={packageId}&mode=mini";
        await Launcher.LaunchUriAsync(new (linkString));
    }

    [RelayCommand]
    public async Task UninstallButton()
    {
        await Launcher.LaunchUriAsync(new ("ms-settings:appsfeatures"));
    }
}
