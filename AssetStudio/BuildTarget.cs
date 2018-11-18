using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public enum BuildTarget
    {
        UnknownPlatform = 3716,
        DashboardWidget = 1,
        StandaloneOSX = 2,
        StandaloneOSXPPC = 3,
        StandaloneOSXIntel = 4,
        StandaloneWindows,
        WebPlayer,
        WebPlayerStreamed,
        Wii = 8,
        iOS = 9,
        PS3,
        XBOX360,
        Android = 13,
        StandaloneGLESEmu = 14,
        NaCl = 16,
        StandaloneLinux = 17,
        FlashPlayer = 18,
        StandaloneWindows64 = 19,
        WebGL,
        WSAPlayer,
        StandaloneLinux64 = 24,
        StandaloneLinuxUniversal,
        WP8Player,
        StandaloneOSXIntel64,
        BlackBerry,
        Tizen,
        PSP2,
        PS4,
        PSM,
        XboxOne,
        SamsungTV,
        N3DS,
        WiiU,
        tvOS,
        Switch,
        NoTarget = -2
    }
}
