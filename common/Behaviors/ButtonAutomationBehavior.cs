﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using DevHome.Common.Extensions;
using DevHome.Common.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Xaml.Interactivity;

namespace DevHome.Common.Behaviors;

/// <summary>
/// Behavior class for <see cref="Button"/> automation
/// </summary>
public class ButtonAutomationBehavior : Behavior<ButtonBase>
{
    public string InvokeAnnouncementText
    {
        get => (string)GetValue(InvokeAnnouncementTextProperty);
        set => SetValue(InvokeAnnouncementTextProperty, value);
    }

    public string Name
    {
        get => (string)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject.Click += OnButtonClick;
        UpdateAutomationName();

        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Click -= OnButtonClick;

        base.OnDetaching();
    }

    // Dependency property registration
    public static readonly DependencyProperty InvokeAnnouncementTextProperty = DependencyProperty.Register(nameof(InvokeAnnouncementText), typeof(string), typeof(TextBlockAutomationBehavior), new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty NameProperty = DependencyProperty.Register(nameof(Name), typeof(string), typeof(TextBlockAutomationBehavior), new PropertyMetadata(string.Empty, (d, e) => ((ButtonAutomationBehavior)d).UpdateAutomationName()));

    private void OnButtonClick(object sender, RoutedEventArgs args) => Announce();

    /// <summary>
    /// Announce <see cref="InvokeAnnouncementText"/>
    /// </summary>
    private void Announce()
    {
        if (!string.IsNullOrEmpty(InvokeAnnouncementText))
        {
            Application.Current.GetService<IScreenReaderService>().Announce(InvokeAnnouncementText);
        }
    }

    private void UpdateAutomationName()
    {
        if (AssociatedObject != null)
        {
            AutomationProperties.SetName(AssociatedObject, Name);
        }
    }
}
