using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoozyPlants.App.Model;

internal static class DeviceState
{
    private static bool _softKeyboardOpen;
    private static int _screenMarginBottom;

    public static bool SoftKeyboardOpen 
    {
        get
        {
            return _softKeyboardOpen;
        }
        set
        {
            if (_softKeyboardOpen == value) return;

            _softKeyboardOpen = value;

            OnSoftKeyboardOpenChange?.Invoke();
        }
    }

    public static int ScreenMarginBottom
    {
        get
        {
            return _screenMarginBottom;
        }
        set
        {
            if (_screenMarginBottom == value) return;

            _screenMarginBottom = value;

            OnScreenMarginChange?.Invoke();
        }
    }

    public static float DevicePixelRatio { get; set; } = 1;

    public delegate void DeviceStateChangedEvent();

    public static event DeviceStateChangedEvent? OnSoftKeyboardOpenChange;
    public static event DeviceStateChangedEvent? OnScreenMarginChange;
}
