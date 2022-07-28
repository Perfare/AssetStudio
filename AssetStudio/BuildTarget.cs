using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public enum BuildTarget
    {
        NoTarget = -2,
        AnyPlayer = -1,
        ValidPlayer = 1,
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
        Broadcom = 12,
        Android = 13,
        StandaloneGLESEmu = 14,
        StandaloneGLES20Emu = 15,
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
        Lumin,
        Stadia,
        CloudRendering,
        GameCoreXboxSeries,
        GameCoreXboxOne,
        PS5,
        EmbeddedLinux,
        QNX,
        UnknownPlatform = 9999
    }
}
