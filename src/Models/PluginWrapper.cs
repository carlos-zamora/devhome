// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System.Runtime.InteropServices;
using DevHome.Common.Services;
using Microsoft.Windows.DevHome.SDK;
using Windows.ApplicationModel;
using Windows.Win32;
using Windows.Win32.System.Com;
using WinRT;

namespace DevHome.Models;

public class PluginWrapper : IPluginWrapper
{
    private const int HResultRpcServerNotRunning = -2147023174;

    private readonly object _lock = new ();
    private readonly List<ProviderType> _providerTypes = new ();

    private readonly Dictionary<Type, ProviderType> _providerTypeMap = new ()
    {
        [typeof(IDeveloperIdProvider)] = ProviderType.DeveloperId,
        [typeof(IRepositoryProvider)] = ProviderType.Repository,
        [typeof(INotificationsProvider)] = ProviderType.Notifications,
        [typeof(IWidgetProvider)] = ProviderType.Widget,
        [typeof(ISettingsProvider)] = ProviderType.Settings,
        [typeof(IDevDoctorProvider)] = ProviderType.DevDoctor,
        [typeof(ISetupFlowProvider)] = ProviderType.SetupFlow,
    };

    private IPlugin? _pluginObject;

    public PluginWrapper(string name, string packageFullName, string packageFamilyName, string publisher, string classId, DateTimeOffset installedDate, PackageVersion version)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        PackageFullName = packageFullName ?? throw new ArgumentNullException(nameof(packageFullName));
        PackageFamilyName = packageFamilyName ?? throw new ArgumentNullException(nameof(packageFamilyName));
        Publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        PluginClassId = classId ?? throw new ArgumentNullException(nameof(classId));
        InstalledDate = installedDate;
        Version = version;
    }

    public string Name
    {
        get;
    }

    public string PackageFullName
    {
        get;
    }

    public string PackageFamilyName
    {
        get;
    }

    public string Publisher
    {
        get;
    }

    public string PluginClassId
    {
        get;
    }

    public DateTimeOffset InstalledDate
    {
        get;
    }

    public PackageVersion Version
    {
        get;
    }

    public bool IsRunning()
    {
        if (_pluginObject is null)
        {
            return false;
        }

        try
        {
            _pluginObject.As<IInspectable>().GetRuntimeClassName();
        }
        catch (COMException e)
        {
            if (e.ErrorCode == HResultRpcServerNotRunning)
            {
                return false;
            }

            throw;
        }

        return true;
    }

    public async Task StartPluginAsync()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (!IsRunning())
                {
                    var pluginPtr = IntPtr.Zero;
                    try
                    {
                        var hr = PInvoke.CoCreateInstance(Guid.Parse(PluginClassId), null, CLSCTX.CLSCTX_LOCAL_SERVER, typeof(IPlugin).GUID, out var pluginObj);
                        pluginPtr = Marshal.GetIUnknownForObject(pluginObj);
                        if (hr < 0)
                        {
                            Marshal.ThrowExceptionForHR(hr);
                        }

                        _pluginObject = MarshalInterface<IPlugin>.FromAbi(pluginPtr);
                    }
                    finally
                    {
                        if (pluginPtr != IntPtr.Zero)
                        {
                            Marshal.Release(pluginPtr);
                        }
                    }
                }
            }
        });
    }

    public void SignalDispose()
    {
        lock (_lock)
        {
            if (IsRunning())
            {
                _pluginObject?.Dispose();
            }

            _pluginObject = null;
        }
    }

    public IPlugin? GetPluginObject()
    {
        lock (_lock)
        {
            if (IsRunning())
            {
                return _pluginObject;
            }
            else
            {
                return null;
            }
        }
    }

    public async Task<T?> GetProviderAsync<T>()
        where T : class
    {
        await StartPluginAsync();

        return GetPluginObject()?.GetProvider(_providerTypeMap[typeof(T)]) as T;
    }

    public void AddProviderType(ProviderType providerType)
    {
        _providerTypes.Add(providerType);
    }

    public bool HasProviderType(ProviderType providerType)
    {
        return _providerTypes.Contains(providerType);
    }
}
