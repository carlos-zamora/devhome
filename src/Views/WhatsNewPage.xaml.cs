﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using DevHome.Common.Contracts;
using DevHome.Common.Extensions;
using DevHome.Common.Helpers;
using DevHome.Common.Services;
using DevHome.Common.TelemetryEvents;
using DevHome.Models;
using DevHome.SetupFlow.Utilities;
using DevHome.Telemetry;
using DevHome.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace DevHome.Views;

public sealed partial class WhatsNewPage : Page
{
    private readonly Uri _devDrivePageKeyUri = new ("ms-settings:disksandvolumes");
    private readonly Uri _devDriveLearnMoreLinkUri = new ("https://go.microsoft.com/fwlink/?linkid=2236041");
    private const string _devDriveLinkResourceKey = "WhatsNewPage_DevDriveCard/Link";

    public WhatsNewViewModel ViewModel
    {
        get;
    }

    public WhatsNewPage()
    {
        ViewModel = Application.Current.GetService<WhatsNewViewModel>();
        InitializeComponent();
    }

    private async void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await Application.Current.GetService<ILocalSettingsService>().SaveSettingAsync(WellKnownSettingsKeys.IsNotFirstRun, true);

        var whatsNewCards = FeaturesContainer.Resources
            .Where((item) => item.Value.GetType() == typeof(WhatsNewCard))
            .Select(card => card.Value as WhatsNewCard)
            .OrderBy(card => card?.Priority ?? 0);

        foreach (var card in whatsNewCards)
        {
            if (card is null)
            {
                continue;
            }

            // When the Dev Drive feature is not enabled don't show the learn more uri link, but instead move the learn more text into the button content.
            if (string.Equals(card.PageKey, _devDrivePageKeyUri.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
            {
                if (!DevDriveUtil.IsDevDriveFeatureEnabled)
                {
                    card.Button = Application.Current.GetService<IStringResource>().GetLocalized(_devDriveLinkResourceKey);
                    card.ShouldShowLink = false;
                }
            }

            ViewModel.AddCard(card);
        }

        var whatsNewBigCards = BigFeaturesContainer.Resources
            .Where((item) => item.Value.GetType() == typeof(WhatsNewCard))
            .Select(card => card.Value as WhatsNewCard)
            .OrderBy(card => card?.Priority ?? 0);

        foreach (var card in whatsNewBigCards)
        {
            if (card is null)
            {
                continue;
            }

            ViewModel.AddBigCard(card);
        }

        ViewModel.NumberOfBigCards = whatsNewBigCards.Count();

        MoveBigCardsIfNeeded(this.ActualWidth);
    }

    private void MachineConfigButton_Click(object sender, RoutedEventArgs e)
    {
        var navigationService = Application.Current.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(DevHome.SetupFlow.ViewModels.SetupFlowViewModel).FullName!);
    }

    private async void Button_ClickAsync(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;

        if (btn?.DataContext is not string pageKey)
        {
            return;
        }

        if (pageKey.Equals(_devDrivePageKeyUri.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
        {
            // Only launch the disks and volumes settings page when the Dev Drive feature is enabled.
            // Otherwise redirect the user to the Dev Drive support page to learn more about the feature.
            // Critical level approved by subhasan
            TelemetryFactory.Get<ITelemetry>().Log(
                "LaunchDisksAndVolumesSettingsPageTriggered",
                LogLevel.Critical,
                new DisksAndVolumesSettingsPageTriggeredEvent(source: "WhatsNewPageView"));
            await Launcher.LaunchUriAsync(DevDriveUtil.IsDevDriveFeatureEnabled ? _devDrivePageKeyUri : _devDriveLearnMoreLinkUri);
        }
        else
        {
            var navigationService = Application.Current.GetService<INavigationService>();
            navigationService.NavigateTo(pageKey!);
        }
    }

    public void OnSizeChanged(object sender, SizeChangedEventArgs args)
    {
        if ((Page)sender == this)
        {
            MoveBigCardsIfNeeded(args.NewSize.Width);
        }
    }

    private void MoveBigCardsIfNeeded(double newWidth)
    {
        if (newWidth < 760)
        {
            ViewModel.SwitchToSmallerView();
        }
        else
        {
            ViewModel.SwitchToLargerView();
        }
    }

    public static class MyHelpers
    {
        public static Type GetType(object ele)
        {
            return ele.GetType();
        }
    }
}
