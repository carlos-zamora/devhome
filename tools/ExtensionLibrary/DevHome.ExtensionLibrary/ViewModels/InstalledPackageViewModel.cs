﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel;
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
    private string _title;

    [ObservableProperty]
    private string _publisher;

    [ObservableProperty]
    private string _packageFamilyName;

    [ObservableProperty]
    private DateTimeOffset _installedDate;

    [ObservableProperty]
    private PackageVersion _version;

    [ObservableProperty]
    private ObservableCollection<InstalledExtensionViewModel> _installedExtensionsList = new ();

    public InstalledPackageViewModel(string title, string publisher, string packageFamilyName, DateTimeOffset installedDate, PackageVersion version)
    {
        _title = title;
        _publisher = publisher;
        _packageFamilyName = packageFamilyName;
        _installedDate = installedDate;
        _version = version;
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

    public string GeneratePackageDetails(PackageVersion version, string publisher, DateTimeOffset installedDate)
    {
        var resourceLoader = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader("DevHome.ExtensionLibrary.pri", "DevHome.ExtensionLibrary/Resources");
        var versionLabel = resourceLoader.GetString("Version");
        var lastUpdatedLabel = resourceLoader.GetString("LastUpdated");

        var versionString = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

        return $"{versionLabel} {versionString} | {publisher} | {lastUpdatedLabel} {installedDate.LocalDateTime}";
    }
}
