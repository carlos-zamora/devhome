// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using DevHome.Common.Extensions;
using DevHome.Common.Services;
using DevHome.Dashboard.Helpers;
using DevHome.ExtensionLibrary.Models;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using Windows.Data.Json;
using Windows.Storage;
using Windows.System;

namespace DevHome.ExtensionLibrary.ViewModels;

public partial class ExtensionLibraryViewModel : ObservableObject
{
    private readonly string devHomeProductId = "9N8MHTPHNGVV";

    private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcher;

    [ObservableProperty]
    private ObservableCollection<StorePackageViewModel> _storePackagesList = new ();

    [ObservableProperty]
    private ObservableCollection<InstalledPackageViewModel> _installedPackagesList = new ();

    public ExtensionLibraryViewModel()
    {
        _dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        var pluginService = Application.Current.GetService<IPluginService>();
        pluginService.OnPluginsChanged -= OnPluginsChanged;
        pluginService.OnPluginsChanged += OnPluginsChanged;

        GetInstalledExtensions();
        GetAvailablePackages();
    }

    private async void OnPluginsChanged(object? sender, EventArgs e)
    {
        await _dispatcher.EnqueueAsync(() =>
        {
            GetInstalledExtensions();
            GetAvailablePackages();
        });
    }

    private void GetInstalledExtensions()
    {
        var extensionWrappers = Task.Run(async () =>
        {
            var pluginService = Application.Current.GetService<IPluginService>();
            return await pluginService.GetInstalledPluginsAsync(true);
        }).Result;

        InstalledPackagesList.Clear();

        foreach (var extensionWrapper in extensionWrappers)
        {
            // Don't show self as an extension.
            if (Package.Current.Id.FullName == extensionWrapper.PackageFullName)
            {
                ////continue;
            }

            var extension = new InstalledExtensionViewModel(extensionWrapper.Name, extensionWrapper.PackageFamilyName, true /*TODO*/);

            var foundPackage = false;
            foreach (var installedPackage in InstalledPackagesList)
            {
                if (installedPackage.PackageFamilyName == extensionWrapper.PackageFamilyName)
                {
                    foundPackage = true;
                    installedPackage.InstalledExtensionsList.Add(extension);
                    break;
                }
            }

            if (!foundPackage)
            {
                var installedPackage = new InstalledPackageViewModel("x", extensionWrapper.Name, extensionWrapper.Publisher, extensionWrapper.PackageFamilyName);
                installedPackage.InstalledExtensionsList.Add(extension);
                InstalledPackagesList.Add(installedPackage);
            }
        }
    }

    private async Task<string> GetStoreData()
    {
        var packagesFileContents = string.Empty;
        var packagesFileName = "extensionResult.json";
        try
        {
            Log.Logger()?.ReportInfo("ExtensionLibraryViewModel", $"Get packages file '{packagesFileName}'");
            var uri = new Uri($"ms-appx:///DevHome.ExtensionLibrary/Assets/{packagesFileName}");
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri).AsTask().ConfigureAwait(false);
            packagesFileContents = await FileIO.ReadTextAsync(file);
        }
        catch (Exception ex)
        {
            Log.Logger()?.ReportError("ExtensionLibraryViewModel", "Error retrieving packages", ex);
        }

        return packagesFileContents;
    }

    private async void GetAvailablePackages()
    {
        StorePackagesList.Clear();

        var storeData = await GetStoreData();
        if (string.IsNullOrEmpty(storeData))
        {
            return;
        }

        var jsonObj = JsonObject.Parse(storeData);
        if (jsonObj != null)
        {
            var products = jsonObj.GetNamedArray("Products");
            foreach (var product in products)
            {
                var productObj = product.GetObject();
                var productId = productObj.GetNamedString("ProductId");

                // Don't show self as available, and don't show packages of already installed extensions as available.
                if (productId == devHomeProductId || IsAlreadyInstalled(productId))
                {
                    ////continue;
                }

                var title = string.Empty;
                var publisher = string.Empty;

                var localizedProperties = productObj.GetNamedArray("LocalizedProperties");
                foreach (var localizedProperty in localizedProperties)
                {
                    var propertyObject = localizedProperty.GetObject();
                    title = propertyObject.GetNamedValue("ProductTitle").GetString();
                    publisher = propertyObject.GetNamedValue("PublisherName").GetString();
                }

                var properties = productObj.GetNamedObject("Properties");
                var packageFamilyName = properties.GetNamedString("PackageFamilyName");

                var storePackage = new StorePackageViewModel(productId, title, publisher, packageFamilyName);
                StorePackagesList.Add(storePackage);
            }
        }
    }

    private bool IsAlreadyInstalled(string productId)
    {
        // PackageFullName = Microsoft.Windows.DevHome.Dev_0.0.0.0_x64__8wekyb3d8bbwe
        // PackageFamilyName = Microsoft.Windows.DevHomeGitHubExtension_8wekyb3d8bbwe
        return InstalledPackagesList.Any(package => productId == package.ProductId);
    }

    [RelayCommand]
    public async Task GetUpdatesButtonAsync()
    {
        await Launcher.LaunchUriAsync(new ("ms-windows-store://downloadsandupdates"));
    }
}
