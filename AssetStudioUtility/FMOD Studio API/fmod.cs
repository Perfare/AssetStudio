/* ========================================================================================== */
/*                                                                                            */
/* FMOD Studio - C# Wrapper . Copyright (c), Firelight Technologies Pty, Ltd. 2004-2016.      */
/*                                                                                            */
/* ========================================================================================== */

using System;
using System.Text;
using System.Runtime.InteropServices;
using AssetStudio.PInvoke;

namespace FMOD
{
    /*
        FMOD version number.  Check this against FMOD::System::getVersion / System_GetVersion
        0xaaaabbcc -> aaaa = major version number.  bb = minor version number.  cc = development version number.
    */
    public class VERSION
    {
        public const int    number = 0x00010716;
#if WIN64
        public const string dll    = "fmod64";
#else
        public const string dll    = "fmod";
#endif
    }

    public class CONSTANTS
    {
        public const int MAX_CHANNEL_WIDTH = 32;
        public const int MAX_LISTENERS = 8;
    }

    /*
        FMOD types
    */

    /*
    [ENUM]
    [
        [DESCRIPTION]
        error codes.  Returned from every function.

        [REMARKS]

        [SEE_ALSO]
    ]
    */
    public enum RESULT : int
    {
        OK,                        /* No errors. */
        ERR_BADCOMMAND,            /* Tried to call a function on a data type that does not allow this type of functionality (ie calling Sound::lock on a streaming sound). */
        ERR_CHANNEL_ALLOC,         /* Error trying to allocate a channel. */
        ERR_CHANNEL_STOLEN,        /* The specified channel has been reused to play another sound. */
        ERR_DMA,                   /* DMA Failure.  See debug output for more information. */
        ERR_DSP_CONNECTION,        /* DSP connection error.  Connection possibly caused a cyclic dependency or connected dsps with incompatible buffer counts. */
        ERR_DSP_DONTPROCESS,       /* DSP return code from a DSP process query callback.  Tells mixer not to call the process callback and therefore not consume CPU.  Use this to optimize the DSP graph. */
        ERR_DSP_FORMAT,            /* DSP Format error.  A DSP unit may have attempted to connect to this network with the wrong format, or a matrix may have been set with the wrong size if the target unit has a specified channel map. */
        ERR_DSP_INUSE,             /* DSP is already in the mixer's DSP network. It must be removed before being reinserted or released. */
        ERR_DSP_NOTFOUND,          /* DSP connection error.  Couldn't find the DSP unit specified. */
        ERR_DSP_RESERVED,          /* DSP operation error.  Cannot perform operation on this DSP as it is reserved by the system. */
        ERR_DSP_SILENCE,           /* DSP return code from a DSP process query callback.  Tells mixer silence would be produced from read, so go idle and not consume CPU.  Use this to optimize the DSP graph. */
        ERR_DSP_TYPE,              /* DSP operation cannot be performed on a DSP of this type. */
        ERR_FILE_BAD,              /* Error loading file. */
        ERR_FILE_COULDNOTSEEK,     /* Couldn't perform seek operation.  This is a limitation of the medium (ie netstreams) or the file format. */
        ERR_FILE_DISKEJECTED,      /* Media was ejected while reading. */
        ERR_FILE_EOF,              /* End of file unexpectedly reached while trying to read essential data (truncated?). */
        ERR_FILE_ENDOFDATA,        /* End of current chunk reached while trying to read data. */
        ERR_FILE_NOTFOUND,         /* File not found. */
        ERR_FORMAT,                /* Unsupported file or audio format. */
        ERR_HEADER_MISMATCH,       /* There is a version mismatch between the FMOD header and either the FMOD Studio library or the FMOD Low Level library. */
        ERR_HTTP,                  /* A HTTP error occurred. This is a catch-all for HTTP errors not listed elsewhere. */
        ERR_HTTP_ACCESS,           /* The specified resource requires authentication or is forbidden. */
        ERR_HTTP_PROXY_AUTH,       /* Proxy authentication is required to access the specified resource. */
        ERR_HTTP_SERVER_ERROR,     /* A HTTP server error occurred. */
        ERR_HTTP_TIMEOUT,          /* The HTTP request timed out. */
        ERR_INITIALIZATION,        /* FMOD was not initialized correctly to support this function. */
        ERR_INITIALIZED,           /* Cannot call this command after System::init. */
        ERR_INTERNAL,              /* An error occurred that wasn't supposed to.  Contact support. */
        ERR_INVALID_FLOAT,         /* Value passed in was a NaN, Inf or denormalized float. */
        ERR_INVALID_HANDLE,        /* An invalid object handle was used. */
        ERR_INVALID_PARAM,         /* An invalid parameter was passed to this function. */
        ERR_INVALID_POSITION,      /* An invalid seek position was passed to this function. */
        ERR_INVALID_SPEAKER,       /* An invalid speaker was passed to this function based on the current speaker mode. */
        ERR_INVALID_SYNCPOINT,     /* The syncpoint did not come from this sound handle. */
        ERR_INVALID_THREAD,        /* Tried to call a function on a thread that is not supported. */
        ERR_INVALID_VECTOR,        /* The vectors passed in are not unit length, or perpendicular. */
        ERR_MAXAUDIBLE,            /* Reached maximum audible playback count for this sound's soundgroup. */
        ERR_MEMORY,                /* Not enough memory or resources. */
        ERR_MEMORY_CANTPOINT,      /* Can't use FMOD_OPENMEMORY_POINT on non PCM source data, or non mp3/xma/adpcm data if FMOD_CREATECOMPRESSEDSAMPLE was used. */
        ERR_NEEDS3D,               /* Tried to call a command on a 2d sound when the command was meant for 3d sound. */
        ERR_NEEDSHARDWARE,         /* Tried to use a feature that requires hardware support. */
        ERR_NET_CONNECT,           /* Couldn't connect to the specified host. */
        ERR_NET_SOCKET_ERROR,      /* A socket error occurred.  This is a catch-all for socket-related errors not listed elsewhere. */
        ERR_NET_URL,               /* The specified URL couldn't be resolved. */
        ERR_NET_WOULD_BLOCK,       /* Operation on a non-blocking socket could not complete immediately. */
        ERR_NOTREADY,              /* Operation could not be performed because specified sound/DSP connection is not ready. */
        ERR_OUTPUT_ALLOCATED,      /* Error initializing output device, but more specifically, the output device is already in use and cannot be reused. */
        ERR_OUTPUT_CREATEBUFFER,   /* Error creating hardware sound buffer. */
        ERR_OUTPUT_DRIVERCALL,     /* A call to a standard soundcard driver failed, which could possibly mean a bug in the driver or resources were missing or exhausted. */
        ERR_OUTPUT_FORMAT,         /* Soundcard does not support the specified format. */
        ERR_OUTPUT_INIT,           /* Error initializing output device. */
        ERR_OUTPUT_NODRIVERS,      /* The output device has no drivers installed.  If pre-init, FMOD_OUTPUT_NOSOUND is selected as the output mode.  If post-init, the function just fails. */
        ERR_PLUGIN,                /* An unspecified error has been returned from a plugin. */
        ERR_PLUGIN_MISSING,        /* A requested output, dsp unit type or codec was not available. */
        ERR_PLUGIN_RESOURCE,       /* A resource that the plugin requires cannot be found. (ie the DLS file for MIDI playback) */
        ERR_PLUGIN_VERSION,        /* A plugin was built with an unsupported SDK version. */
        ERR_RECORD,                /* An error occurred trying to initialize the recording device. */
        ERR_REVERB_CHANNELGROUP,   /* Reverb properties cannot be set on this channel because a parent channelgroup owns the reverb connection. */
        ERR_REVERB_INSTANCE,       /* Specified instance in FMOD_REVERB_PROPERTIES couldn't be set. Most likely because it is an invalid instance number or the reverb doesn't exist. */
        ERR_SUBSOUNDS,             /* The error occurred because the sound referenced contains subsounds when it shouldn't have, or it doesn't contain subsounds when it should have.  The operation may also not be able to be performed on a parent sound. */
        ERR_SUBSOUND_ALLOCATED,    /* This subsound is already being used by another sound, you cannot have more than one parent to a sound.  Null out the other parent's entry first. */
        ERR_SUBSOUND_CANTMOVE,     /* Shared subsounds cannot be replaced or moved from their parent stream, such as when the parent stream is an FSB file. */
        ERR_TAGNOTFOUND,           /* The specified tag could not be found or there are no tags. */
        ERR_TOOMANYCHANNELS,       /* The sound created exceeds the allowable input channel count.  This can be increased using the 'maxinputchannels' parameter in System::setSoftwareFormat. */
        ERR_TRUNCATED,             /* The retrieved string is too long to fit in the supplied buffer and has been truncated. */
        ERR_UNIMPLEMENTED,         /* Something in FMOD hasn't been implemented when it should be! contact support! */
        ERR_UNINITIALIZED,         /* This command failed because System::init or System::setDriver was not called. */
        ERR_UNSUPPORTED,           /* A command issued was not supported by this object.  Possibly a plugin without certain callbacks specified. */
        ERR_VERSION,               /* The version number of this file format is not supported. */
        ERR_EVENT_ALREADY_LOADED,  /* The specified bank has already been loaded. */
        ERR_EVENT_LIVEUPDATE_BUSY, /* The live update connection failed due to the game already being connected. */
        ERR_EVENT_LIVEUPDATE_MISMATCH, /* The live update connection failed due to the game data being out of sync with the tool. */
        ERR_EVENT_LIVEUPDATE_TIMEOUT, /* The live update connection timed out. */
        ERR_EVENT_NOTFOUND,        /* The requested event, bus or vca could not be found. */
        ERR_STUDIO_UNINITIALIZED,  /* The Studio::System object is not yet initialized. */
        ERR_STUDIO_NOT_LOADED,     /* The specified resource is not loaded, so it can't be unloaded. */
        ERR_INVALID_STRING,        /* An invalid string was passed to this function. */
        ERR_ALREADY_LOCKED,        /* The specified resource is already locked. */
        ERR_NOT_LOCKED,            /* The specified resource is not locked, so it can't be unlocked. */
        ERR_RECORD_DISCONNECTED,   /* The specified recording driver has been disconnected. */
        ERR_TOOMANYSAMPLES,        /* The length provided exceed the allowable limit. */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]
        Used to distinguish if a FMOD_CHANNELCONTROL parameter is actually a channel or a channelgroup.

        [REMARKS]
        Cast the FMOD_CHANNELCONTROL to an FMOD_CHANNEL/FMOD::Channel, or FMOD_CHANNELGROUP/FMOD::ChannelGroup if specific functionality is needed for either class.
        Otherwise use as FMOD_CHANNELCONTROL/FMOD::ChannelControl and use that API.

        [SEE_ALSO]
        Channel::setCallback
        ChannelGroup::setCallback
    ]
    */
    public enum CHANNELCONTROL_TYPE : int
    {
        CHANNEL,
        CHANNELGROUP
    }

    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure describing a point in 3D space.

        [REMARKS]
        FMOD uses a left handed co-ordinate system by default.
        To use a right handed co-ordinate system specify FMOD_INIT_3D_RIGHTHANDED from FMOD_INITFLAGS in System::init.

        [SEE_ALSO]
        System::set3DListenerAttributes
        System::get3DListenerAttributes
        Channel::set3DAttributes
        Channel::get3DAttributes
        Geometry::addPolygon
        Geometry::setPolygonVertex
        Geometry::getPolygonVertex
        Geometry::setRotation
        Geometry::getRotation
        Geometry::setPosition
        Geometry::getPosition
        Geometry::setScale
        Geometry::getScale
        FMOD_INITFLAGS
    ]
    */
    [StructLayout(LayoutKind.Sequential)]
    public struct VECTOR
    {
        public float x;        /* X co-ordinate in 3D space. */
        public float y;        /* Y co-ordinate in 3D space. */
        public float z;        /* Z co-ordinate in 3D space. */
    }

    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure describing a position, velocity and orientation.

        [REMARKS]

        [SEE_ALSO]
        FMOD_VECTOR
        FMOD_DSP_PARAMETER_3DATTRIBUTES
    ]
    */
    [StructLayout(LayoutKind.Sequential)]
    public struct _3D_ATTRIBUTES
    {
        VECTOR position;
        VECTOR velocity;
        VECTOR forward;
        VECTOR up;
    }

    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure that is passed into FMOD_FILE_ASYNCREAD_CALLBACK.  Use the information in this structure to perform

        [REMARKS]
        Members marked with [r] mean the variable is modified by FMOD and is for reading purposes only.  Do not change this value.<br>
        Members marked with [w] mean the variable can be written to.  The user can set the value.<br>
        <br>
        Instructions: write to 'buffer', and 'bytesread' <b>BEFORE</b> setting 'result'.<br>
        As soon as result is set, FMOD will asynchronously continue internally using the data provided in this structure.<br>
        <br>
        Set 'result' to the result expected from a normal file read callback.<br>
        If the read was successful, set it to FMOD_OK.<br>
        If it read some data but hit the end of the file, set it to FMOD_ERR_FILE_EOF.<br>
        If a bad error occurred, return FMOD_ERR_FILE_BAD<br>
        If a disk was ejected, return FMOD_ERR_FILE_DISKEJECTED.<br>

        [SEE_ALSO]
        FMOD_FILE_ASYNCREAD_CALLBACK
        FMOD_FILE_ASYNCCANCEL_CALLBACK
    ]
    */
    [StructLayout(LayoutKind.Sequential)]
    public struct ASYNCREADINFO
    {
        public IntPtr   handle;                     /* [r] The file handle that was filled out in the open callback. */
        public uint     offset;                     /* [r] Seek position, make sure you read from this file offset. */
        public uint     sizebytes;                  /* [r] how many bytes requested for read. */
        public int      priority;                   /* [r] 0 = low importance.  100 = extremely important (ie 'must read now or stuttering may occur') */

        public IntPtr   userdata;                   /* [r] User data pointer. */
        public IntPtr   buffer;                     /* [w] Buffer to read file data into. */
        public uint     bytesread;                  /* [w] Fill this in before setting result code to tell FMOD how many bytes were read. */
        public ASYNCREADINFO_DONE_CALLBACK   done;  /* [r] FMOD file system wake up function.  Call this when user file read is finished.  Pass result of file read as a parameter. */

    }

    /*
    [ENUM]
    [
        [DESCRIPTION]
        These output types are used with System::setOutput / System::getOutput, to choose which output method to use.

        [REMARKS]
        To pass information to the driver when initializing fmod use the *extradriverdata* parameter in System::init for the following reasons.

        - FMOD_OUTPUTTYPE_WAVWRITER     - extradriverdata is a pointer to a char * file name that the wav writer will output to.
        - FMOD_OUTPUTTYPE_WAVWRITER_NRT - extradriverdata is a pointer to a char * file name that the wav writer will output to.
        - FMOD_OUTPUTTYPE_DSOUND        - extradriverdata is cast to a HWND type, so that FMOD can set the focus on the audio for a particular window.
        - FMOD_OUTPUTTYPE_PS3           - extradriverdata is a pointer to a FMOD_PS3_EXTRADRIVERDATA struct. This can be found in fmodps3.h.
        - FMOD_OUTPUTTYPE_XBOX360       - extradriverdata is a pointer to a FMOD_360_EXTRADRIVERDATA struct. This can be found in fmodxbox360.h.

        Currently these are the only FMOD drivers that take extra information.  Other unknown plugins may have different requirements.
    
        Note! If FMOD_OUTPUTTYPE_WAVWRITER_NRT or FMOD_OUTPUTTYPE_NOSOUND_NRT are used, and if the System::update function is being called
        very quickly (ie for a non realtime decode) it may be being called too quickly for the FMOD streamer thread to respond to.
        The result will be a skipping/stuttering output in the captured audio.
    
        To remedy this, disable the FMOD streamer thread, and use FMOD_INIT_STREAM_FROM_UPDATE to avoid skipping in the output stream,
        as it will lock the mixer and the streamer together in the same thread.
    
        [SEE_ALSO]
            System::setOutput
            System::getOutput
            System::setSoftwareFormat
            System::getSoftwareFormat
            System::init
            System::update
            FMOD_INITFLAGS
    ]
    */
    public enum OUTPUTTYPE : int
    {
        AUTODETECT,      /* Picks the best output mode for the platform. This is the default. */

        UNKNOWN,         /* All - 3rd party plugin, unknown. This is for use with System::getOutput only. */
        NOSOUND,         /* All - Perform all mixing but discard the final output. */
        WAVWRITER,       /* All - Writes output to a .wav file. */
        NOSOUND_NRT,     /* All - Non-realtime version of FMOD_OUTPUTTYPE_NOSOUND. User can drive mixer with System::update at whatever rate they want. */
        WAVWRITER_NRT,   /* All - Non-realtime version of FMOD_OUTPUTTYPE_WAVWRITER. User can drive mixer with System::update at whatever rate they want. */

        DSOUND,          /* Win                  - Direct Sound.                        (Default on Windows XP and below) */
        WINMM,           /* Win                  - Windows Multimedia. */
        WASAPI,          /* Win/WinStore/XboxOne - Windows Audio Session API.           (Default on Windows Vista and above, Xbox One and Windows Store Applications) */
        ASIO,            /* Win                  - Low latency ASIO 2.0. */
        PULSEAUDIO,      /* Linux                - Pulse Audio.                         (Default on Linux if available) */
        ALSA,            /* Linux                - Advanced Linux Sound Architecture.   (Default on Linux if PulseAudio isn't available) */
        COREAUDIO,       /* Mac/iOS              - Core Audio.                          (Default on Mac and iOS) */
        XBOX360,         /* Xbox 360             - XAudio.                              (Default on Xbox 360) */
        PS3,             /* PS3                  - Audio Out.                           (Default on PS3) */
        AUDIOTRACK,      /* Android              - Java Audio Track.                    (Default on Android 2.2 and below) */
        OPENSL,          /* Android              - OpenSL ES.                           (Default on Android 2.3 and above) */
        WIIU,            /* Wii U                - AX.                                  (Default on Wii U) */
        AUDIOOUT,        /* PS4/PSVita           - Audio Out.                           (Default on PS4 and PS Vita) */

        MAX,             /* Maximum number of output types supported. */
    }

    /*
    [ENUM]
    [
        [DESCRIPTION]
        Specify the destination of log output when using the logging version of FMOD.

        [REMARKS]
        TTY destination can vary depending on platform, common examples include the
        Visual Studio / Xcode output window, stderr and LogCat.

        [SEE_ALSO]
        FMOD_Debug_Initialize
    ]
    */
    public enum DEBUG_MODE : int
    {
        TTY,        /* Default log location per platform, i.e. Visual Studio output window, stderr, LogCat, etc */
        FILE,       /* Write log to specified file path */
        CALLBACK,   /* Call specified callback with log information */
    }

    /*
    [DEFINE]
    [
        [NAME]
        FMOD_DEBUG_FLAGS

        [DESCRIPTION]
        Specify the requested information to be output when using the logging version of FMOD.

        [REMARKS]

        [SEE_ALSO]
        FMOD_Debug_Initialize
    ]
    */
    [Flags]
    public enum DEBUG_FLAGS : uint
    {
        NONE                    = 0x00000000,   /* Disable all messages */
        ERROR                   = 0x00000001,   /* Enable only error messages. */
        WARNING                 = 0x00000002,   /* Enable warning and error messages. */
        LOG                     = 0x00000004,   /* Enable informational, warning and error messages (default). */

        TYPE_MEMORY             = 0x00000100,   /* Verbose logging for memory operations, only use this if you are debugging a memory related issue. */
        TYPE_FILE               = 0x00000200,   /* Verbose logging for file access, only use this if you are debugging a file related issue. */
        TYPE_CODEC              = 0x00000400,   /* Verbose logging for codec initialization, only use this if you are debugging a codec related issue. */
        TYPE_TRACE              = 0x00000800,   /* Verbose logging for internal errors, use this for tracking the origin of error codes. */

        DISPLAY_TIMESTAMPS      = 0x00010000,   /* Display the time stamp of the log message in milliseconds. */
        DISPLAY_LINENUMBERS     = 0x00020000,   /* Display the source code file and line number for where the message originated. */
        DISPLAY_THREAD          = 0x00040000,   /* Display the thread ID of the calling function that generated the message. */
    }

    /*
    [DEFINE]
    [
        [NAME]
        FMOD_MEMORY_TYPE

        [DESCRIPTION]
        Bit fields for memory allocation type being passed into FMOD memory callbacks.

        [REMARKS]
        Remember this is a bitfield.  You may get more than 1 bit set (ie physical + persistent) so do not simply switch on the types!  You must check each bit individually or clear out the bits that you do not want within the callback.<br>
        Bits can be excluded if you want during Memory_Initialize so that you never get them.

        [SEE_ALSO]
        FMOD_MEMORY_ALLOC_CALLBACK
        FMOD_MEMORY_REALLOC_CALLBACK
        FMOD_MEMORY_FREE_CALLBACK
        Memory_Initialize
    ]
    */
    [Flags]
    public enum MEMORY_TYPE : uint
    {
        NORMAL             = 0x00000000,       /* Standard memory. */
        STREAM_FILE        = 0x00000001,       /* Stream file buffer, size controllable with System::setStreamBufferSize. */
        STREAM_DECODE      = 0x00000002,       /* Stream decode buffer, size controllable with FMOD_CREATESOUNDEXINFO::decodebuffersize. */
        SAMPLEDATA         = 0x00000004,       /* Sample data buffer.  Raw audio data, usually PCM/MPEG/ADPCM/XMA data. */
        DSP_BUFFER         = 0x00000008,       /* DSP memory block allocated when more than 1 output exists on a DSP node. */
        PLUGIN             = 0x00000010,       /* Memory allocated by a third party plugin. */
        XBOX360_PHYSICAL   = 0x00100000,       /* Requires XPhysicalAlloc / XPhysicalFree. */
        PERSISTENT         = 0x00200000,       /* Persistent memory. Memory will be freed when System::release is called. */
        SECONDARY          = 0x00400000,       /* Secondary memory. Allocation should be in secondary memory. For example RSX on the PS3. */
        ALL                = 0xFFFFFFFF
    }

    /*
    [ENUM]
    [
        [DESCRIPTION]
        These are speaker types defined for use with the System::setSoftwareFormat command.

        [REMARKS]
        Note below the phrase 'sound channels' is used.  These are the subchannels inside a sound, they are not related and
        have nothing to do with the FMOD class "Channel".<br>
        For example a mono sound has 1 sound channel, a stereo sound has 2 sound channels, and an AC3 or 6 channel wav file have 6 "sound channels".<br>
        <br>
        FMOD_SPEAKERMODE_RAW<br>
        ---------------------<br>
        This mode is for output devices that are not specifically mono/stereo/quad/surround/5.1 or 7.1, but are multichannel.<br>
        Use System::setSoftwareFormat to specify the number of speakers you want to address, otherwise it will default to 2 (stereo).<br>
        Sound channels map to speakers sequentially, so a mono sound maps to output speaker 0, stereo sound maps to output speaker 0 & 1.<br>
        The user assumes knowledge of the speaker order.  FMOD_SPEAKER enumerations may not apply, so raw channel indices should be used.<br>
        Multichannel sounds map input channels to output channels 1:1. <br>
        Channel::setPan and Channel::setPanLevels do not work.<br>
        Speaker levels must be manually set with Channel::setPanMatrix.<br>
        <br>
        FMOD_SPEAKERMODE_MONO<br>
        ---------------------<br>
        This mode is for a 1 speaker arrangement.<br>
        Panning does not work in this speaker mode.<br>
        Mono, stereo and multichannel sounds have each sound channel played on the one speaker unity.<br>
        Mix behavior for multichannel sounds can be set with Channel::setPanMatrix.<br>
        Channel::setPanLevels does not work.<br>
        <br>
        FMOD_SPEAKERMODE_STEREO<br>
        -----------------------<br>
        This mode is for 2 speaker arrangements that have a left and right speaker.<br>
        <li>Mono sounds default to an even distribution between left and right.  They can be panned with Channel::setPan.<br>
        <li>Stereo sounds default to the middle, or full left in the left speaker and full right in the right speaker.
        <li>They can be cross faded with Channel::setPan.<br>
        <li>Multichannel sounds have each sound channel played on each speaker at unity.<br>
        <li>Mix behavior for multichannel sounds can be set with Channel::setPanMatrix.<br>
        <li>Channel::setPanLevels works but only front left and right parameters are used, the rest are ignored.<br>
        <br>
        FMOD_SPEAKERMODE_QUAD<br>
        ------------------------<br>
        This mode is for 4 speaker arrangements that have a front left, front right, surround left and a surround right speaker.<br>
        <li>Mono sounds default to an even distribution between front left and front right.  They can be panned with Channel::setPan.<br>
        <li>Stereo sounds default to the left sound channel played on the front left, and the right sound channel played on the front right.<br>
        <li>They can be cross faded with Channel::setPan.<br>
        <li>Multichannel sounds default to all of their sound channels being played on each speaker in order of input.<br>
        <li>Mix behavior for multichannel sounds can be set with Channel::setPanMatrix.<br>
        <li>Channel::setPanLevels works but rear left, rear right, center and lfe are ignored.<br>
        <br>
        FMOD_SPEAKERMODE_SURROUND<br>
        ------------------------<br>
        This mode is for 5 speaker arrangements that have a left/right/center/surround left/surround right.<br>
        <li>Mono sounds default to the center speaker.  They can be panned with Channel::setPan.<br>
        <li>Stereo sounds default to the left sound channel played on the front left, and the right sound channel played on the front right.
        <li>They can be cross faded with Channel::setPan.<br>
        <li>Multichannel sounds default to all of their sound channels being played on each speaker in order of input.
        <li>Mix behavior for multichannel sounds can be set with Channel::setPanMatrix.<br>
        <li>Channel::setPanLevels works but rear left / rear right are ignored.<br>
        <br>
        FMOD_SPEAKERMODE_5POINT1<br>
        ---------------------------------------------------------<br>
        This mode is for 5.1 speaker arrangements that have a left/right/center/surround left/surround right and a subwoofer speaker.<br>
        <li>Mono sounds default to the center speaker.  They can be panned with Channel::setPan.<br>
        <li>Stereo sounds default to the left sound channel played on the front left, and the right sound channel played on the front right.
        <li>They can be cross faded with Channel::setPan.<br>
        <li>Multichannel sounds default to all of their sound channels being played on each speaker in order of input.
        <li>Mix behavior for multichannel sounds can be set with Channel::setPanMatrix.<br>
        <li>Channel::setPanLevels works but rear left / rear right are ignored.<br>
        <br>
        FMOD_SPEAKERMODE_7POINT1<br>
        ------------------------<br>
        This mode is for 7.1 speaker arrangements that have a left/right/center/surround left/surround right/rear left/rear right
        and a subwoofer speaker.<br>
        <li>Mono sounds default to the center speaker.  They can be panned with Channel::setPan.<br>
        <li>Stereo sounds default to the left sound channel played on the front left, and the right sound channel played on the front right.
        <li>They can be cross faded with Channel::setPan.<br>
        <li>Multichannel sounds default to all of their sound channels being played on each speaker in order of input.
        <li>Mix behavior for multichannel sounds can be set with Channel::setPanMatrix.<br>
        <li>Channel::setPanLevels works and every parameter is used to set the balance of a sound in any speaker.<br>
        <br>

        [SEE_ALSO]
        System::setSoftwareFormat
        System::getSoftwareFormat
        DSP::setChannelFormat
    ]
    */
    public enum SPEAKERMODE : int
    {
        DEFAULT,          /* Default speaker mode based on operating system/output mode.  Windows = control panel setting, Xbox = 5.1, PS3 = 7.1 etc. */
        RAW,              /* There is no specific speakermode.  Sound channels are mapped in order of input to output.  Use System::setSoftwareFormat to specify speaker count. See remarks for more information. */
        MONO,             /* The speakers are monaural. */
        STEREO,           /* The speakers are stereo. */
        QUAD,             /* 4 speaker setup.  This includes front left, front right, surround left, surround right.  */
        SURROUND,         /* 5 speaker setup.  This includes front left, front right, center, surround left, surround right. */
        _5POINT1,         /* 5.1 speaker setup.  This includes front left, front right, center, surround left, surround right and an LFE speaker. */
        _7POINT1,         /* 7.1 speaker setup.  This includes front left, front right, center, surround left, surround right, back left, back right and an LFE speaker. */

        MAX,              /* Maximum number of speaker modes supported. */
    }

    /*
    [ENUM]
    [
        [DESCRIPTION]
        Assigns an enumeration for a speaker index.

        [REMARKS]

        [SEE_ALSO]
        System::setSpeakerPosition
        System::getSpeakerPosition
    ]
    */
    public enum SPEAKER : int
    {
        FRONT_LEFT,
        FRONT_RIGHT,
        FRONT_CENTER,
        LOW_FREQUENCY,
        SURROUND_LEFT,
        SURROUND_RIGHT,
        BACK_LEFT,
        BACK_RIGHT,

        MAX,               /* Maximum number of speaker types supported. */
    }

    /*
    [DEFINE]
    [
        [NAME]
        FMOD_CHANNELMASK

        [DESCRIPTION]
        These are bitfields to describe for a certain number of channels in a signal, which channels are being represented.<br>
        For example, a signal could be 1 channel, but contain the LFE channel only.<br>

        [REMARKS]
        FMOD_CHANNELMASK_BACK_CENTER is not represented as an output speaker in fmod - but it is encountered in input formats and is down or upmixed appropriately to the nearest speakers.<br>

        [SEE_ALSO]
        DSP::setChannelFormat
        DSP::getChannelFormat
        FMOD_SPEAKERMODE
    ]
    */
    [Flags]
    public enum CHANNELMASK : uint
    {
        FRONT_LEFT             = 0x00000001,
        FRONT_RIGHT            = 0x00000002,
        FRONT_CENTER           = 0x00000004,
        LOW_FREQUENCY          = 0x00000008,
        SURROUND_LEFT          = 0x00000010,
        SURROUND_RIGHT         = 0x00000020,
        BACK_LEFT              = 0x00000040,
        BACK_RIGHT             = 0x00000080,
        BACK_CENTER            = 0x00000100,

        MONO                   = (FRONT_LEFT),
        STEREO                 = (FRONT_LEFT | FRONT_RIGHT),
        LRC                    = (FRONT_LEFT | FRONT_RIGHT | FRONT_CENTER),
        QUAD                   = (FRONT_LEFT | FRONT_RIGHT | SURROUND_LEFT | SURROUND_RIGHT),
        SURROUND               = (FRONT_LEFT | FRONT_RIGHT | FRONT_CENTER | SURROUND_LEFT | SURROUND_RIGHT),
        _5POINT1               = (FRONT_LEFT | FRONT_RIGHT | FRONT_CENTER | LOW_FREQUENCY | SURROUND_LEFT | SURROUND_RIGHT),
        _5POINT1_REARS         = (FRONT_LEFT | FRONT_RIGHT | FRONT_CENTER | LOW_FREQUENCY | BACK_LEFT | BACK_RIGHT),
        _7POINT0               = (FRONT_LEFT | FRONT_RIGHT | FRONT_CENTER | SURROUND_LEFT | SURROUND_RIGHT | BACK_LEFT | BACK_RIGHT),
        _7POINT1               = (FRONT_LEFT | FRONT_RIGHT | FRONT_CENTER | LOW_FREQUENCY | SURROUND_LEFT | SURROUND_RIGHT | BACK_LEFT | BACK_RIGHT)
    }

    /*
    [ENUM]
    [
        [DESCRIPTION]
        When creating a multichannel sound, FMOD will pan them to their default speaker locations, for example a 6 channel sound will default to one channel per 5.1 output speaker.<br>
        Another example is a stereo sound.  It will default to left = front left, right = front right.<br>
        <br>
        This is for sounds that are not 'default'.  For example you might have a sound that is 6 channels but actually made up of 3 stereo pairs, that should all be located in front left, front right only.

        [REMARKS]

        [SEE_ALSO]
        FMOD_CREATESOUNDEXINFO
    ]
    */
    public enum CHANNELORDER : int
    {
        DEFAULT,              /* Left, Right, Center, LFE, Surround Left, Surround Right, Back Left, Back Right (see FMOD_SPEAKER enumeration)   */
        WAVEFORMAT,           /* Left, Right, Center, LFE, Back Left, Back Right, Surround Left, Surround Right (as per Microsoft .wav WAVEFORMAT structure master order) */
        PROTOOLS,             /* Left, Center, Right, Surround Left, Surround Right, LFE */
        ALLMONO,              /* Mono, Mono, Mono, Mono, Mono, Mono, ... (each channel all the way up to 32 channels are treated as if they were mono) */
        ALLSTEREO,            /* Left, Right, Left, Right, Left, Right, ... (each pair of channels is treated as stereo all the way up to 32 channels) */
        ALSA,                 /* Left, Right, Surround Left, Surround Right, Center, LFE (as per Linux ALSA channel order) */

        MAX,                  /* Maximum number of channel orderings supported. */
    }

    /*
    [ENUM]
    [
        [DESCRIPTION]
        These are plugin types defined for use with the System::getNumPlugins,
        System::getPluginInfo and System::unloadPlugin functions.

        [REMARKS]

        [SEE_ALSO]
        System::getNumPlugins
        System::getPluginInfo
        System::unloadPlugin
    ]
    */
    public enum PLUGINTYPE : int
    {
        OUTPUT,          /* The plugin type is an output module.  FMOD mixed audio will play through one of these devices */
        CODEC,           /* The plugin type is a file format codec.  FMOD will use these codecs to load file formats for playback. */
        DSP,             /* The plugin type is a DSP unit.  FMOD will use these plugins as part of its DSP network to apply effects to output or generate sound in realtime. */

        MAX,             /* Maximum number of plugin types supported. */
    }



    /*
    [DEFINE]
    [
        [NAME]
        FMOD_INITFLAGS

        [DESCRIPTION]
        Initialization flags.  Use them with System::init in the *flags* parameter to change various behavior.

        [REMARKS]
        Use System::setAdvancedSettings to adjust settings for some of the features that are enabled by these flags.

        [SEE_ALSO]
        System::init
        System::update
        System::setAdvancedSettings
        Channel::set3DOcclusion
    ]
    */
    [Flags]
    public enum INITFLAGS : uint
    {
        NORMAL                    = 0x00000000, /* Initialize normally */
        STREAM_FROM_UPDATE        = 0x00000001, /* No stream thread is created internally.  Streams are driven from System::update.  Mainly used with non-realtime outputs. */
        MIX_FROM_UPDATE           = 0x00000002, /* Win/Wii/PS3/Xbox/Xbox 360 Only - FMOD Mixer thread is woken up to do a mix when System::update is called rather than waking periodically on its own timer. */
        _3D_RIGHTHANDED           = 0x00000004, /* FMOD will treat +X as right, +Y as up and +Z as backwards (towards you). */
        CHANNEL_LOWPASS           = 0x00000100, /* All FMOD_3D based voices will add a software lowpass filter effect into the DSP chain which is automatically used when Channel::set3DOcclusion is used or the geometry API.   This also causes sounds to sound duller when the sound goes behind the listener, as a fake HRTF style effect.  Use System::setAdvancedSettings to disable or adjust cutoff frequency for this feature. */
        CHANNEL_DISTANCEFILTER    = 0x00000200, /* All FMOD_3D based voices will add a software lowpass and highpass filter effect into the DSP chain which will act as a distance-automated bandpass filter. Use System::setAdvancedSettings to adjust the center frequency. */
        PROFILE_ENABLE            = 0x00010000, /* Enable TCP/IP based host which allows FMOD Designer or FMOD Profiler to connect to it, and view memory, CPU and the DSP network graph in real-time. */
        VOL0_BECOMES_VIRTUAL      = 0x00020000, /* Any sounds that are 0 volume will go virtual and not be processed except for having their positions updated virtually.  Use System::setAdvancedSettings to adjust what volume besides zero to switch to virtual at. */
        GEOMETRY_USECLOSEST       = 0x00040000, /* With the geometry engine, only process the closest polygon rather than accumulating all polygons the sound to listener line intersects. */
        PREFER_DOLBY_DOWNMIX      = 0x00080000, /* When using FMOD_SPEAKERMODE_5POINT1 with a stereo output device, use the Dolby Pro Logic II downmix algorithm instead of the SRS Circle Surround algorithm. */
        THREAD_UNSAFE             = 0x00100000, /* Disables thread safety for API calls. Only use this if FMOD low level is being called from a single thread, and if Studio API is not being used! */
        PROFILE_METER_ALL         = 0x00200000  /* Slower, but adds level metering for every single DSP unit in the graph.  Use DSP::setMeteringEnabled to turn meters off individually. */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]
        These definitions describe the type of song being played.

        [REMARKS]

        [SEE_ALSO]
        Sound::getFormat
    ]
    */
    public enum SOUND_TYPE
    {
        UNKNOWN,         /* 3rd party / unknown plugin format. */
        AIFF,            /* AIFF. */
        ASF,             /* Microsoft Advanced Systems Format (ie WMA/ASF/WMV). */
        DLS,             /* Sound font / downloadable sound bank. */
        FLAC,            /* FLAC lossless codec. */
        FSB,             /* FMOD Sample Bank. */
        IT,              /* Impulse Tracker. */
        MIDI,            /* MIDI. extracodecdata is a pointer to an FMOD_MIDI_EXTRACODECDATA structure. */
        MOD,             /* Protracker / Fasttracker MOD. */
        MPEG,            /* MP2/MP3 MPEG. */
        OGGVORBIS,       /* Ogg vorbis. */
        PLAYLIST,        /* Information only from ASX/PLS/M3U/WAX playlists */
        RAW,             /* Raw PCM data. */
        S3M,             /* ScreamTracker 3. */
        USER,            /* User created sound. */
        WAV,             /* Microsoft WAV. */
        XM,              /* FastTracker 2 XM. */
        XMA,             /* Xbox360 XMA */
        AUDIOQUEUE,      /* iPhone hardware decoder, supports AAC, ALAC and MP3. extracodecdata is a pointer to an FMOD_AUDIOQUEUE_EXTRACODECDATA structure. */
        AT9,             /* PS4 / PSVita ATRAC 9 format */
        VORBIS,          /* Vorbis */
        MEDIA_FOUNDATION,/* Windows Store Application built in system codecs */
        MEDIACODEC,      /* Android MediaCodec */
        FADPCM,          /* FMOD Adaptive Differential Pulse Code Modulation */

        MAX,             /* Maximum number of sound types supported. */
    }

    /*
    [ENUM]
    [
        [DESCRIPTION]
        These definitions describe the native format of the hardware or software buffer that will be used.

        [REMARKS]
        This is the format the native hardware or software buffer will be or is created in.

        [SEE_ALSO]
        System::createSoundEx
        Sound::getFormat
    ]
    */
    public enum SOUND_FORMAT : int
    {
        NONE,       /* Unitialized / unknown */
        PCM8,       /* 8bit integer PCM data */
        PCM16,      /* 16bit integer PCM data  */
        PCM24,      /* 24bit integer PCM data  */
        PCM32,      /* 32bit integer PCM data  */
        PCMFLOAT,   /* 32bit floating point PCM data  */
        BITSTREAM,  /* Sound data is in its native compressed format. */

        MAX         /* Maximum number of sound formats supported. */
    }


    /*
    [DEFINE]
    [
        [NAME]
        FMOD_MODE

        [DESCRIPTION]
        Sound description bitfields, bitwise OR them together for loading and describing sounds.

        [REMARKS]
        By default a sound will open as a static sound that is decompressed fully into memory to PCM. (ie equivalent of FMOD_CREATESAMPLE)<br>
        To have a sound stream instead, use FMOD_CREATESTREAM, or use the wrapper function System::createStream.<br>
        Some opening modes (ie FMOD_OPENUSER, FMOD_OPENMEMORY, FMOD_OPENMEMORY_POINT, FMOD_OPENRAW) will need extra information.<br>
        This can be provided using the FMOD_CREATESOUNDEXINFO structure.
        <br>
        Specifying FMOD_OPENMEMORY_POINT will POINT to your memory rather allocating its own sound buffers and duplicating it internally.<br>
        <b><u>This means you cannot free the memory while FMOD is using it, until after Sound::release is called.</b></u>
        With FMOD_OPENMEMORY_POINT, for PCM formats, only WAV, FSB, and RAW are supported.  For compressed formats, only those formats supported by FMOD_CREATECOMPRESSEDSAMPLE are supported.<br>
        With FMOD_OPENMEMORY_POINT and FMOD_OPENRAW or PCM, if using them together, note that you must pad the data on each side by 16 bytes.  This is so fmod can modify the ends of the data for looping/interpolation/mixing purposes.  If a wav file, you will need to insert silence, and then reset loop points to stop the playback from playing that silence.<br>
        <br>
        <b>Xbox 360 memory</b> On Xbox 360 Specifying FMOD_OPENMEMORY_POINT to a virtual memory address will cause FMOD_ERR_INVALID_ADDRESS
        to be returned.  Use physical memory only for this functionality.<br>
        <br>
        FMOD_LOWMEM is used on a sound if you want to minimize the memory overhead, by having FMOD not allocate memory for certain
        features that are not likely to be used in a game environment.  These are :<br>
        1. Sound::getName functionality is removed.  256 bytes per sound is saved.<br>

        [SEE_ALSO]
        System::createSound
        System::createStream
        Sound::setMode
        Sound::getMode
        Channel::setMode
        Channel::getMode
        Sound::set3DCustomRolloff
        Channel::set3DCustomRolloff
        Sound::getOpenState
    ]
    */
    [Flags]
    public enum MODE : uint
    {
        DEFAULT                = 0x00000000,  /* Default for all modes listed below. FMOD_LOOP_OFF, FMOD_2D, FMOD_3D_WORLDRELATIVE, FMOD_3D_INVERSEROLLOFF */
        LOOP_OFF               = 0x00000001,  /* For non looping sounds. (default).  Overrides FMOD_LOOP_NORMAL / FMOD_LOOP_BIDI. */
        LOOP_NORMAL            = 0x00000002,  /* For forward looping sounds. */
        LOOP_BIDI              = 0x00000004,  /* For bidirectional looping sounds. (only works on software mixed static sounds). */
        _2D                    = 0x00000008,  /* Ignores any 3d processing. (default). */
        _3D                    = 0x00000010,  /* Makes the sound positionable in 3D.  Overrides FMOD_2D. */
        CREATESTREAM           = 0x00000080,  /* Decompress at runtime, streaming from the source provided (standard stream).  Overrides FMOD_CREATESAMPLE. */
        CREATESAMPLE           = 0x00000100,  /* Decompress at loadtime, decompressing or decoding whole file into memory as the target sample format. (standard sample). */
        CREATECOMPRESSEDSAMPLE = 0x00000200,  /* Load MP2, MP3, IMAADPCM or XMA into memory and leave it compressed.  During playback the FMOD software mixer will decode it in realtime as a 'compressed sample'.  Can only be used in combination with FMOD_SOFTWARE. */
        OPENUSER               = 0x00000400,  /* Opens a user created static sample or stream. Use FMOD_CREATESOUNDEXINFO to specify format and/or read callbacks.  If a user created 'sample' is created with no read callback, the sample will be empty.  Use FMOD_Sound_Lock and FMOD_Sound_Unlock to place sound data into the sound if this is the case. */
        OPENMEMORY             = 0x00000800,  /* "name_or_data" will be interpreted as a pointer to memory instead of filename for creating sounds. */
        OPENMEMORY_POINT       = 0x10000000,  /* "name_or_data" will be interpreted as a pointer to memory instead of filename for creating sounds.  Use FMOD_CREATESOUNDEXINFO to specify length.  This differs to FMOD_OPENMEMORY in that it uses the memory as is, without duplicating the memory into its own buffers.  Cannot be freed after open, only after Sound::release.   Will not work if the data is compressed and FMOD_CREATECOMPRESSEDSAMPLE is not used. */
        OPENRAW                = 0x00001000,  /* Will ignore file format and treat as raw pcm.  User may need to declare if data is FMOD_SIGNED or FMOD_UNSIGNED */
        OPENONLY               = 0x00002000,  /* Just open the file, dont prebuffer or read.  Good for fast opens for info, or when sound::readData is to be used. */
        ACCURATETIME           = 0x00004000,  /* For FMOD_CreateSound - for accurate FMOD_Sound_GetLength / FMOD_Channel_SetPosition on VBR MP3, AAC and MOD/S3M/XM/IT/MIDI files.  Scans file first, so takes longer to open. FMOD_OPENONLY does not affect this. */
        MPEGSEARCH             = 0x00008000,  /* For corrupted / bad MP3 files.  This will search all the way through the file until it hits a valid MPEG header.  Normally only searches for 4k. */
        NONBLOCKING            = 0x00010000,  /* For opening sounds and getting streamed subsounds (seeking) asyncronously.  Use Sound::getOpenState to poll the state of the sound as it opens or retrieves the subsound in the background. */
        UNIQUE                 = 0x00020000,  /* Unique sound, can only be played one at a time */
        _3D_HEADRELATIVE       = 0x00040000,  /* Make the sound's position, velocity and orientation relative to the listener. */
        _3D_WORLDRELATIVE      = 0x00080000,  /* Make the sound's position, velocity and orientation absolute (relative to the world). (DEFAULT) */
        _3D_INVERSEROLLOFF     = 0x00100000,  /* This sound will follow the inverse rolloff model where mindistance = full volume, maxdistance = where sound stops attenuating, and rolloff is fixed according to the global rolloff factor.  (DEFAULT) */
        _3D_LINEARROLLOFF      = 0x00200000,  /* This sound will follow a linear rolloff model where mindistance = full volume, maxdistance = silence.  */
        _3D_LINEARSQUAREROLLOFF= 0x00400000,  /* This sound will follow a linear-square rolloff model where mindistance = full volume, maxdistance = silence.  Rolloffscale is ignored. */
        _3D_INVERSETAPEREDROLLOFF = 0x00800000,  /* This sound will follow the inverse rolloff model at distances close to mindistance and a linear-square rolloff close to maxdistance. */
        _3D_CUSTOMROLLOFF      = 0x04000000,  /* This sound will follow a rolloff model defined by Sound::set3DCustomRolloff / Channel::set3DCustomRolloff.  */
        _3D_IGNOREGEOMETRY     = 0x40000000,  /* Is not affect by geometry occlusion.  If not specified in Sound::setMode, or Channel::setMode, the flag is cleared and it is affected by geometry again. */
        IGNORETAGS             = 0x02000000,  /* Skips id3v2/asf/etc tag checks when opening a sound, to reduce seek/read overhead when opening files (helps with CD performance). */
        LOWMEM                 = 0x08000000,  /* Removes some features from samples to give a lower memory overhead, like Sound::getName. */
        LOADSECONDARYRAM       = 0x20000000,  /* Load sound into the secondary RAM of supported platform.  On PS3, sounds will be loaded into RSX/VRAM. */
        VIRTUAL_PLAYFROMSTART  = 0x80000000   /* For sounds that start virtual (due to being quiet or low importance), instead of swapping back to audible, and playing at the correct offset according to time, this flag makes the sound play from the start. */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]
        These values describe what state a sound is in after FMOD_NONBLOCKING has been used to open it.

        [REMARKS]
        With streams, if you are using FMOD_NONBLOCKING, note that if the user calls Sound::getSubSound, a stream will go into FMOD_OPENSTATE_SEEKING state and sound related commands will return FMOD_ERR_NOTREADY.<br>
        With streams, if you are using FMOD_NONBLOCKING, note that if the user calls Channel::getPosition, a stream will go into FMOD_OPENSTATE_SETPOSITION state and sound related commands will return FMOD_ERR_NOTREADY.<br>

        [SEE_ALSO]
        Sound::getOpenState
        FMOD_MODE
    ]
    */
    public enum OPENSTATE : int
    {
        READY = 0,       /* Opened and ready to play */
        LOADING,         /* Initial load in progress */
        ERROR,           /* Failed to open - file not found, out of memory etc.  See return value of Sound::getOpenState for what happened. */
        CONNECTING,      /* Connecting to remote host (internet sounds only) */
        BUFFERING,       /* Buffering data */
        SEEKING,         /* Seeking to subsound and re-flushing stream buffer. */
        PLAYING,         /* Ready and playing, but not possible to release at this time without stalling the main thread. */
        SETPOSITION,     /* Seeking within a stream to a different position. */

        MAX,             /* Maximum number of open state types. */
    }

    /*
    [ENUM]
    [
        [DESCRIPTION]
        These flags are used with SoundGroup::setMaxAudibleBehavior to determine what happens when more sounds
        are played than are specified with SoundGroup::setMaxAudible.

        [REMARKS]
        When using FMOD_SOUNDGROUP_BEHAVIOR_MUTE, SoundGroup::setMuteFadeSpeed can be used to stop a sudden transition.
        Instead, the time specified will be used to cross fade between the sounds that go silent and the ones that become audible.

        [SEE_ALSO]
        SoundGroup::setMaxAudibleBehavior
        SoundGroup::getMaxAudibleBehavior
        SoundGroup::setMaxAudible
        SoundGroup::getMaxAudible
        SoundGroup::setMuteFadeSpeed
        SoundGroup::getMuteFadeSpeed
    ]
    */
    public enum SOUNDGROUP_BEHAVIOR : int
    {
        BEHAVIOR_FAIL,              /* Any sound played that puts the sound count over the SoundGroup::setMaxAudible setting, will simply fail during System::playSound. */
        BEHAVIOR_MUTE,              /* Any sound played that puts the sound count over the SoundGroup::setMaxAudible setting, will be silent, then if another sound in the group stops the sound that was silent before becomes audible again. */
        BEHAVIOR_STEALLOWEST,       /* Any sound played that puts the sound count over the SoundGroup::setMaxAudible setting, will steal the quietest / least important sound playing in the group. */

        MAX,               /* Maximum number of sound group behaviors. */
    }

    /*
    [ENUM]
    [
        [DESCRIPTION]
        These callback types are used with Channel::setCallback.

        [REMARKS]
        Each callback has commanddata parameters passed as int unique to the type of callback.<br>
        See reference to FMOD_CHANNELCONTROL_CALLBACK to determine what they might mean for each type of callback.<br>
        <br>
        <b>Note!</b>  Currently the user must call System::update for these callbacks to trigger!

        [SEE_ALSO]
        Channel::setCallback
        ChannelGroup::setCallback
        FMOD_CHANNELCONTROL_CALLBACK
        System::update
    ]
    */
    public enum CHANNELCONTROL_CALLBACK_TYPE : int
    {
        END,                  /* Called when a sound ends. */
        VIRTUALVOICE,         /* Called when a voice is swapped out or swapped in. */
        SYNCPOINT,            /* Called when a syncpoint is encountered.  Can be from wav file markers. */
        OCCLUSION,            /* Called when the channel has its geometry occlusion value calculated.  Can be used to clamp or change the value. */

        MAX,                  /* Maximum number of callback types supported. */
    }

    /*
    [ENUM]
    [
        [DESCRIPTION]
        These enums denote special types of node within a DSP chain.

        [REMARKS]

        [SEE_ALSO]
        Channel::getDSP
        ChannelGroup::getDSP
    ]
    */
    public struct CHANNELCONTROL_DSP_INDEX
    {
        public const int HEAD    = -1;         /* Head of the DSP chain. */
        public const int FADER   = -2;         /* Built in fader DSP. */
        public const int PANNER  = -3;         /* Built in panner DSP. */
        public const int TAIL    = -4;         /* Tail of the DSP chain. */
    }

    /*
    [ENUM]
    [
        [DESCRIPTION]
        Used to distinguish the instance type passed into FMOD_ERROR_CALLBACK.

        [REMARKS]
        Cast the instance of FMOD_ERROR_CALLBACK to the appropriate class indicated by this enum.

        [SEE_ALSO]
    ]
    */
    public enum ERRORCALLBACK_INSTANCETYPE
    {
        NONE,
        SYSTEM,
        CHANNEL,
        CHANNELGROUP,
        CHANNELCONTROL,
        SOUND,
        SOUNDGROUP,
        DSP,
        DSPCONNECTION,
        GEOMETRY,
        REVERB3D,
        STUDIO_SYSTEM,
        STUDIO_EVENTDESCRIPTION,
        STUDIO_EVENTINSTANCE,
        STUDIO_PARAMETERINSTANCE,
        STUDIO_CUEINSTANCE,
        STUDIO_BUS,
        STUDIO_VCA,
        STUDIO_BANK,
        STUDIO_COMMANDREPLAY
    }

    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure that is passed into FMOD_SYSTEM_CALLBACK for the FMOD_SYSTEM_CALLBACK_ERROR callback type.

        [REMARKS]
        The instance pointer will be a type corresponding to the instanceType enum.

        [SEE_ALSO]
        FMOD_ERRORCALLBACK_INSTANCETYPE
    ]
    */
    [StructLayout(LayoutKind.Sequential)]
    public struct ERRORCALLBACK_INFO
    {
        public  RESULT                      result;                     /* Error code result */
        public  ERRORCALLBACK_INSTANCETYPE  instancetype;               /* Type of instance the error occurred on */
        public  IntPtr                      instance;                   /* Instance pointer */
        private IntPtr                      functionname_internal;      /* Function that the error occurred on */
        private IntPtr                      functionparams_internal;    /* Function parameters that the error ocurred on */

        public string functionname   { get { return Marshal.PtrToStringAnsi(functionname_internal); } }
        public string functionparams { get { return Marshal.PtrToStringAnsi(functionparams_internal); } }
    }

    /*
    [DEFINE]
    [
        [NAME]
        FMOD_SYSTEM_CALLBACK_TYPE

        [DESCRIPTION]
        These callback types are used with System::setCallback.

        [REMARKS]
        Each callback has commanddata parameters passed as void* unique to the type of callback.<br>
        See reference to FMOD_SYSTEM_CALLBACK to determine what they might mean for each type of callback.<br>
        <br>
        <b>Note!</b> Using FMOD_SYSTEM_CALLBACK_DEVICELISTCHANGED (on Mac only) requires the application to be running an event loop which will allow external changes to device list to be detected by FMOD.<br>
        <br>
        <b>Note!</b> The 'system' object pointer will be null for FMOD_SYSTEM_CALLBACK_THREADCREATED and FMOD_SYSTEM_CALLBACK_MEMORYALLOCATIONFAILED callbacks.

        [SEE_ALSO]
        System::setCallback
        System::update
        DSP::addInput
    ]
    */
    [Flags]
    public enum SYSTEM_CALLBACK_TYPE : uint
    {
        DEVICELISTCHANGED      = 0x00000001,  /* Called from System::update when the enumerated list of devices has changed. */
        DEVICELOST             = 0x00000002,  /* Called from System::update when an output device has been lost due to control panel parameter changes and FMOD cannot automatically recover. */
        MEMORYALLOCATIONFAILED = 0x00000004,  /* Called directly when a memory allocation fails somewhere in FMOD.  (NOTE - 'system' will be NULL in this callback type.)*/
        THREADCREATED          = 0x00000008,  /* Called directly when a thread is created. (NOTE - 'system' will be NULL in this callback type.) */
        BADDSPCONNECTION       = 0x00000010,  /* Called when a bad connection was made with DSP::addInput. Usually called from mixer thread because that is where the connections are made.  */
        PREMIX                 = 0x00000020,  /* Called each tick before a mix update happens. */
        POSTMIX                = 0x00000040,  /* Called each tick after a mix update happens. */
        ERROR                  = 0x00000080,  /* Called when each API function returns an error code, including delayed async functions. */
        MIDMIX                 = 0x00000100,  /* Called each tick in mix update after clocks have been updated before the main mix occurs. */
        THREADDESTROYED        = 0x00000200,  /* Called directly when a thread is destroyed. */
        PREUPDATE              = 0x00000400,  /* Called at start of System::update function. */
        POSTUPDATE             = 0x00000800,  /* Called at end of System::update function. */
        RECORDLISTCHANGED      = 0x00001000,  /* Called from System::update when the enumerated list of recording devices has changed. */
        ALL                    = 0xFFFFFFFF,  /* Pass this mask to System::setCallback to receive all callback types.  */
    }
	
    #region wrapperinternal
    [StructLayout(LayoutKind.Sequential)]
    public struct StringWrapper
    {
        IntPtr nativeUtf8Ptr;

        public static implicit operator string(StringWrapper fstring)
        {
            if (fstring.nativeUtf8Ptr == IntPtr.Zero)
            {
                return "";
            }

            int strlen = 0;
            while (Marshal.ReadByte(fstring.nativeUtf8Ptr, strlen) != 0)
            {
                strlen++;
            }
            if (strlen > 0)
            {
                byte[] bytes = new byte[strlen];
                Marshal.Copy(fstring.nativeUtf8Ptr, bytes, 0, strlen);
                return Encoding.UTF8.GetString(bytes, 0, strlen);
            }
            else
            {
                return "";
            }
        }
    }
    #endregion

    /*
        FMOD Callbacks
    */
    public delegate RESULT ASYNCREADINFO_DONE_CALLBACK(IntPtr info, RESULT result);

    public delegate RESULT DEBUG_CALLBACK           (DEBUG_FLAGS flags, string file, int line, string func, string message);

    public delegate RESULT SYSTEM_CALLBACK          (IntPtr systemraw, SYSTEM_CALLBACK_TYPE type, IntPtr commanddata1, IntPtr commanddata2, IntPtr userdata);

    public delegate RESULT CHANNEL_CALLBACK         (IntPtr channelraw, CHANNELCONTROL_TYPE controltype, CHANNELCONTROL_CALLBACK_TYPE type, IntPtr commanddata1, IntPtr commanddata2);

    public delegate RESULT SOUND_NONBLOCKCALLBACK   (IntPtr soundraw, RESULT result);
    public delegate RESULT SOUND_PCMREADCALLBACK    (IntPtr soundraw, IntPtr data, uint datalen);
    public delegate RESULT SOUND_PCMSETPOSCALLBACK  (IntPtr soundraw, int subsound, uint position, TIMEUNIT postype);

    public delegate RESULT FILE_OPENCALLBACK        (StringWrapper name, ref uint filesize, ref IntPtr handle, IntPtr userdata);
    public delegate RESULT FILE_CLOSECALLBACK       (IntPtr handle, IntPtr userdata);
    public delegate RESULT FILE_READCALLBACK        (IntPtr handle, IntPtr buffer, uint sizebytes, ref uint bytesread, IntPtr userdata);
    public delegate RESULT FILE_SEEKCALLBACK        (IntPtr handle, uint pos, IntPtr userdata);
    public delegate RESULT FILE_ASYNCREADCALLBACK   (IntPtr handle, IntPtr info, IntPtr userdata);
    public delegate RESULT FILE_ASYNCCANCELCALLBACK (IntPtr handle, IntPtr userdata);

    public delegate IntPtr MEMORY_ALLOC_CALLBACK    (uint size, MEMORY_TYPE type, StringWrapper sourcestr);
    public delegate IntPtr MEMORY_REALLOC_CALLBACK  (IntPtr ptr, uint size, MEMORY_TYPE type, StringWrapper sourcestr);
    public delegate void   MEMORY_FREE_CALLBACK     (IntPtr ptr, MEMORY_TYPE type, StringWrapper sourcestr);

    public delegate float  CB_3D_ROLLOFFCALLBACK    (IntPtr channelraw, float distance);

    /*
    [ENUM]
    [
        [DESCRIPTION]
        List of interpolation types that the FMOD Ex software mixer supports.

        [REMARKS]
        The default resampler type is FMOD_DSP_RESAMPLER_LINEAR.<br>
        Use System::setSoftwareFormat to tell FMOD the resampling quality you require for FMOD_SOFTWARE based sounds.

        [SEE_ALSO]
        System::setSoftwareFormat
        System::getSoftwareFormat
    ]
    */
    public enum DSP_RESAMPLER : int
    {
        DEFAULT,         /* Default interpolation method.  Currently equal to FMOD_DSP_RESAMPLER_LINEAR. */
        NOINTERP,        /* No interpolation.  High frequency aliasing hiss will be audible depending on the sample rate of the sound. */
        LINEAR,          /* Linear interpolation (default method).  Fast and good quality, causes very slight lowpass effect on low frequency sounds. */
        CUBIC,           /* Cubic interpolation.  Slower than linear interpolation but better quality. */
        SPLINE,          /* 5 point spline interpolation.  Slowest resampling method but best quality. */

        MAX,             /* Maximum number of resample methods supported. */
    }

    /*
    [ENUM]
    [
        [DESCRIPTION]
        List of connection types between 2 DSP nodes.

        [REMARKS]
        FMOD_DSP_CONNECTION_TYPE_STANDARD<br>
        ----------------------------------<br>
        Default DSPConnection type.  Audio is mixed from the input to the output DSP's audible buffer, meaning it will be part of the audible signal.  A standard connection will execute its input DSP if it has not been executed before.<br>
        <br>
        FMOD_DSP_CONNECTION_TYPE_SIDECHAIN<br>
        ----------------------------------<br>
        Sidechain DSPConnection type.  Audio is mixed from the input to the output DSP's sidechain buffer, meaning it will NOT be part of the audible signal.  A sidechain connection will execute its input DSP if it has not been executed before.<br>
        The purpose of the seperate sidechain buffer in a DSP, is so that the DSP effect can privately access for analysis purposes.  An example of use in this case, could be a compressor which analyzes the signal, to control its own effect parameters (ie a compression level or gain).<br>
        <br>
        For the effect developer, to accept sidechain data, the sidechain data will appear in the FMOD_DSP_STATE struct which is passed into the read callback of a DSP unit.<br>
        FMOD_DSP_STATE::sidechaindata and FMOD_DSP::sidechainchannels will hold the mixed result of any sidechain data flowing into it.<br>
        <br>
        FMOD_DSP_CONNECTION_TYPE_SEND<br>
        -----------------------------<br>
        Send DSPConnection type.  Audio is mixed from the input to the output DSP's audible buffer, meaning it will be part of the audible signal.  A send connection will NOT execute its input DSP if it has not been executed before.<br>
        A send connection will only read what exists at the input's buffer at the time of executing the output DSP unit (which can be considered the 'return')<br>
        <br>
        FMOD_DSP_CONNECTION_TYPE_SEND_SIDECHAIN<br>
        ---------------------------------------<br>
        Send sidechain DSPConnection type.  Audio is mixed from the input to the output DSP's sidechain buffer, meaning it will NOT be part of the audible signal.  A send sidechain connection will NOT execute its input DSP if it has not been executed before.<br>
        A send sidechain connection will only read what exists at the input's buffer at the time of executing the output DSP unit (which can be considered the 'sidechain return').
        <br>
        For the effect developer, to accept sidechain data, the sidechain data will appear in the FMOD_DSP_STATE struct which is passed into the read callback of a DSP unit.<br>
        FMOD_DSP_STATE::sidechaindata and FMOD_DSP::sidechainchannels will hold the mixed result of any sidechain data flowing into it.

        [SEE_ALSO]
        DSP::addInput
        DSPConnection::getType
    ]
    */
    public enum DSPCONNECTION_TYPE : int
    {
        STANDARD,          /* Default connection type.         Audio is mixed from the input to the output DSP's audible buffer.  */
        SIDECHAIN,         /* Sidechain connection type.       Audio is mixed from the input to the output DSP's sidechain buffer.  */
        SEND,              /* Send connection type.            Audio is mixed from the input to the output DSP's audible buffer, but the input is NOT executed, only copied from.  A standard connection or sidechain needs to make an input execute to generate data. */
        SEND_SIDECHAIN,    /* Send sidechain connection type.  Audio is mixed from the input to the output DSP's sidechain buffer, but the input is NOT executed, only copied from.  A standard connection or sidechain needs to make an input execute to generate data. */

        MAX,               /* Maximum number of DSP connection types supported. */
    }

    /*
    [ENUM]
    [
        [DESCRIPTION]
        List of tag types that could be stored within a sound.  These include id3 tags, metadata from netstreams and vorbis/asf data.

        [REMARKS]

        [SEE_ALSO]
        Sound::getTag
    ]
    */
    public enum TAGTYPE : int
    {
        UNKNOWN = 0,
        ID3V1,
        ID3V2,
        VORBISCOMMENT,
        SHOUTCAST,
        ICECAST,
        ASF,
        MIDI,
        PLAYLIST,
        FMOD,
        USER,

        MAX                /* Maximum number of tag types supported. */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]
        List of data types that can be returned by Sound::getTag

        [REMARKS]

        [SEE_ALSO]
        Sound::getTag
    ]
    */
    public enum TAGDATATYPE : int
    {
        BINARY = 0,
        INT,
        FLOAT,
        STRING,
        STRING_UTF16,
        STRING_UTF16BE,
        STRING_UTF8,
        CDTOC,

        MAX                /* Maximum number of tag datatypes supported. */
    }

    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure describing a piece of tag data.

        [REMARKS]
        Members marked with [w] mean the user sets the value before passing it to the function.
        Members marked with [r] mean FMOD sets the value to be used after the function exits.

        [SEE_ALSO]
        Sound::getTag
        TAGTYPE
        TAGDATATYPE
    ]
    */
    [StructLayout(LayoutKind.Sequential)]
    public struct TAG
    {
        public  TAGTYPE           type;         /* [r] The type of this tag. */
        public  TAGDATATYPE       datatype;     /* [r] The type of data that this tag contains */
        private IntPtr            name_internal;/* [r] The name of this tag i.e. "TITLE", "ARTIST" etc. */
        public  IntPtr            data;         /* [r] Pointer to the tag data - its format is determined by the datatype member */
        public  uint              datalen;      /* [r] Length of the data contained in this tag */
        public  bool              updated;      /* [r] True if this tag has been updated since last being accessed with Sound::getTag */

        public string name { get { return Marshal.PtrToStringAnsi(name_internal); } }
    }


    /*
    [DEFINE]
    [
        [NAME]
        FMOD_TIMEUNIT

        [DESCRIPTION]
        List of time types that can be returned by Sound::getLength and used with Channel::setPosition or Channel::getPosition.

        [REMARKS]
        Do not combine flags except FMOD_TIMEUNIT_BUFFERED.

        [SEE_ALSO]
        Sound::getLength
        Channel::setPosition
        Channel::getPosition
    ]
    */
    [Flags]
    public enum TIMEUNIT : uint
    {
        MS                = 0x00000001,  /* Milliseconds. */
        PCM               = 0x00000002,  /* PCM Samples, related to milliseconds * samplerate / 1000. */
        PCMBYTES          = 0x00000004,  /* Bytes, related to PCM samples * channels * datawidth (ie 16bit = 2 bytes). */
        RAWBYTES          = 0x00000008,  /* Raw file bytes of (compressed) sound data (does not include headers).  Only used by Sound::getLength and Channel::getPosition. */
        PCMFRACTION       = 0x00000010,  /* Fractions of 1 PCM sample.  Unsigned int range 0 to 0xFFFFFFFF.  Used for sub-sample granularity for DSP purposes. */
        MODORDER          = 0x00000100,  /* MOD/S3M/XM/IT.  Order in a sequenced module format.  Use Sound::getFormat to determine the format. */
        MODROW            = 0x00000200,  /* MOD/S3M/XM/IT.  Current row in a sequenced module format.  Sound::getLength will return the number if rows in the currently playing or seeked to pattern. */
        MODPATTERN        = 0x00000400,  /* MOD/S3M/XM/IT.  Current pattern in a sequenced module format.  Sound::getLength will return the number of patterns in the song and Channel::getPosition will return the currently playing pattern. */
        BUFFERED          = 0x10000000,  /* Time value as seen by buffered stream.  This is always ahead of audible time, and is only used for processing. */
    }

    /*
    [DEFINE]
    [
        [NAME]
        FMOD_PORT_INDEX

        [DESCRIPTION]

        [REMARKS]

        [SEE_ALSO]
        System::AttachChannelGroupToPort
    ]
    */
    public struct PORT_INDEX
    {
        public const ulong NONE = 0xFFFFFFFFFFFFFFFF;
    }

    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Use this structure with System::createSound when more control is needed over loading.
        The possible reasons to use this with System::createSound are:

        - Loading a file from memory.
        - Loading a file from within another larger (possibly wad/pak) file, by giving the loader an offset and length.
        - To create a user created / non file based sound.
        - To specify a starting subsound to seek to within a multi-sample sounds (ie FSB/DLS) when created as a stream.
        - To specify which subsounds to load for multi-sample sounds (ie FSB/DLS) so that memory is saved and only a subset is actually loaded/read from disk.
        - To specify 'piggyback' read and seek callbacks for capture of sound data as fmod reads and decodes it.  Useful for ripping decoded PCM data from sounds as they are loaded / played.
        - To specify a MIDI DLS sample set file to load when opening a MIDI file.

        See below on what members to fill for each of the above types of sound you want to create.

        [REMARKS]
        This structure is optional!  Specify 0 or NULL in System::createSound if you don't need it!

        <u>Loading a file from memory.</u>

        - Create the sound using the FMOD_OPENMEMORY flag.
        - Mandatory.  Specify 'length' for the size of the memory block in bytes.
        - Other flags are optional.

        <u>Loading a file from within another larger (possibly wad/pak) file, by giving the loader an offset and length.</u>

        - Mandatory.  Specify 'fileoffset' and 'length'.
        - Other flags are optional.

        <u>To create a user created / non file based sound.</u>

        - Create the sound using the FMOD_OPENUSER flag.
        - Mandatory.  Specify 'defaultfrequency, 'numchannels' and 'format'.
        - Other flags are optional.

        <u>To specify a starting subsound to seek to and flush with, within a multi-sample stream (ie FSB/DLS).</u>

        - Mandatory.  Specify 'initialsubsound'.

        <u>To specify which subsounds to load for multi-sample sounds (ie FSB/DLS) so that memory is saved and only a subset is actually loaded/read from disk.</u>

        - Mandatory.  Specify 'inclusionlist' and 'inclusionlistnum'.

        <u>To specify 'piggyback' read and seek callbacks for capture of sound data as fmod reads and decodes it.  Useful for ripping decoded PCM data from sounds as they are loaded / played.</u>

        - Mandatory.  Specify 'pcmreadcallback' and 'pcmseekcallback'.

        <u>To specify a MIDI DLS sample set file to load when opening a MIDI file.</u>

        - Mandatory.  Specify 'dlsname'.

        Setting the 'decodebuffersize' is for cpu intensive codecs that may be causing stuttering, not file intensive codecs (ie those from CD or netstreams) which are normally
        altered with System::setStreamBufferSize.  As an example of cpu intensive codecs, an mp3 file will take more cpu to decode than a PCM wav file.

        If you have a stuttering effect, then it is using more cpu than the decode buffer playback rate can keep up with.  Increasing the decode buffersize will most likely solve this problem.

        FSB codec.  If inclusionlist and numsubsounds are used together, this will trigger a special mode where subsounds are shuffled down to save memory.  (useful for large FSB
        files where you only want to load 1 sound).  There will be no gaps, ie no null subsounds.  As an example, if there are 10,000 subsounds and there is an inclusionlist with only 1 entry,
        and numsubsounds = 1, then subsound 0 will be that entry, and there will only be the memory allocated for 1 subsound.  Previously there would still be 10,000 subsound pointers and other
        associated codec entries allocated along with it multiplied by 10,000.

        Members marked with [r] mean the variable is modified by FMOD and is for reading purposes only.  Do not change this value.<br>
        Members marked with [w] mean the variable can be written to.  The user can set the value.

        [SEE_ALSO]
        System::createSound
        System::setStreamBufferSize
        FMOD_MODE
        FMOD_SOUND_FORMAT
        FMOD_SOUND_TYPE
        FMOD_CHANNELMASK
        FMOD_CHANNELORDER
    ]
    */
    [StructLayout(LayoutKind.Sequential)]
    public struct CREATESOUNDEXINFO
    {
        public int                         cbsize;                 /* [w] Size of this structure.  This is used so the structure can be expanded in the future and still work on older versions of FMOD Ex. */
        public uint                        length;                 /* [w] Optional. Specify 0 to ignore. Size in bytes of file to load, or sound to create (in this case only if FMOD_OPENUSER is used).  Required if loading from memory.  If 0 is specified, then it will use the size of the file (unless loading from memory then an error will be returned). */
        public uint                        fileoffset;             /* [w] Optional. Specify 0 to ignore. Offset from start of the file to start loading from.  This is useful for loading files from inside big data files. */
        public int                         numchannels;            /* [w] Optional. Specify 0 to ignore. Number of channels in a sound specified only if OPENUSER is used. */
        public int                         defaultfrequency;       /* [w] Optional. Specify 0 to ignore. Default frequency of sound in a sound specified only if OPENUSER is used.  Other formats use the frequency determined by the file format. */
        public SOUND_FORMAT                format;                 /* [w] Optional. Specify 0 or SOUND_FORMAT_NONE to ignore. Format of the sound specified only if OPENUSER is used.  Other formats use the format determined by the file format.   */
        public uint                        decodebuffersize;       /* [w] Optional. Specify 0 to ignore. For streams.  This determines the size of the double buffer (in PCM samples) that a stream uses.  Use this for user created streams if you want to determine the size of the callback buffer passed to you.  Specify 0 to use FMOD's default size which is currently equivalent to 400ms of the sound format created/loaded. */
        public int                         initialsubsound;        /* [w] Optional. Specify 0 to ignore. In a multi-sample file format such as .FSB/.DLS/.SF2, specify the initial subsound to seek to, only if CREATESTREAM is used. */
        public int                         numsubsounds;           /* [w] Optional. Specify 0 to ignore or have no subsounds.  In a user created multi-sample sound, specify the number of subsounds within the sound that are accessable with Sound::getSubSound / SoundGetSubSound. */
        public IntPtr                      inclusionlist;          /* [w] Optional. Specify 0 to ignore. In a multi-sample format such as .FSB/.DLS/.SF2 it may be desirable to specify only a subset of sounds to be loaded out of the whole file.  This is an array of subsound indicies to load into memory when created. */
        public int                         inclusionlistnum;       /* [w] Optional. Specify 0 to ignore. This is the number of integers contained within the */
        public SOUND_PCMREADCALLBACK       pcmreadcallback;        /* [w] Optional. Specify 0 to ignore. Callback to 'piggyback' on FMOD's read functions and accept or even write PCM data while FMOD is opening the sound.  Used for user sounds created with OPENUSER or for capturing decoded data as FMOD reads it. */
        public SOUND_PCMSETPOSCALLBACK     pcmsetposcallback;      /* [w] Optional. Specify 0 to ignore. Callback for when the user calls a seeking function such as Channel::setPosition within a multi-sample sound, and for when it is opened.*/
        public SOUND_NONBLOCKCALLBACK      nonblockcallback;       /* [w] Optional. Specify 0 to ignore. Callback for successful completion, or error while loading a sound that used the FMOD_NONBLOCKING flag.*/
        public IntPtr                      dlsname;                /* [w] Optional. Specify 0 to ignore. Filename for a DLS or SF2 sample set when loading a MIDI file.   If not specified, on windows it will attempt to open /windows/system32/drivers/gm.dls, otherwise the MIDI will fail to open.  */
        public IntPtr                      encryptionkey;          /* [w] Optional. Specify 0 to ignore. Key for encrypted FSB file.  Without this key an encrypted FSB file will not load. */
        public int                         maxpolyphony;           /* [w] Optional. Specify 0 to ingore. For sequenced formats with dynamic channel allocation such as .MID and .IT, this specifies the maximum voice count allowed while playing.  .IT defaults to 64.  .MID defaults to 32. */
        public IntPtr                      userdata;               /* [w] Optional. Specify 0 to ignore. This is user data to be attached to the sound during creation.  Access via Sound::getUserData. */
        public SOUND_TYPE                  suggestedsoundtype;     /* [w] Optional. Specify 0 or FMOD_SOUND_TYPE_UNKNOWN to ignore.  Instead of scanning all codec types, use this to speed up loading by making it jump straight to this codec. */
        public FILE_OPENCALLBACK           fileuseropen;           /* [w] Optional. Specify 0 to ignore. Callback for opening this file. */
        public FILE_CLOSECALLBACK          fileuserclose;          /* [w] Optional. Specify 0 to ignore. Callback for closing this file. */
        public FILE_READCALLBACK           fileuserread;           /* [w] Optional. Specify 0 to ignore. Callback for reading from this file. */
        public FILE_SEEKCALLBACK           fileuserseek;           /* [w] Optional. Specify 0 to ignore. Callback for seeking within this file. */
        public FILE_ASYNCREADCALLBACK      fileuserasyncread;      /* [w] Optional. Specify 0 to ignore. Callback for asyncronously reading from this file. */
        public FILE_ASYNCCANCELCALLBACK    fileuserasynccancel;    /* [w] Optional. Specify 0 to ignore. Callback for cancelling an asyncronous read. */
        public IntPtr                      fileuserdata;           /* [w] Optional. Specify 0 to ignore. User data to be passed into the file callbacks. */
        public CHANNELORDER                channelorder;           /* [w] Optional. Specify 0 to ignore. Use this to differ the way fmod maps multichannel sounds to speakers.  See FMOD_CHANNELORDER for more. */
        public CHANNELMASK                 channelmask;            /* [w] Optional. Specify 0 to ignore. Use this to differ the way fmod maps multichannel sounds to speakers.  See FMOD_CHANNELMASK for more. */
        public IntPtr                      initialsoundgroup;      /* [w] Optional. Specify 0 to ignore. Specify a sound group if required, to put sound in as it is created. */
        public uint                        initialseekposition;    /* [w] Optional. Specify 0 to ignore. For streams. Specify an initial position to seek the stream to. */
        public TIMEUNIT                    initialseekpostype;     /* [w] Optional. Specify 0 to ignore. For streams. Specify the time unit for the position set in initialseekposition. */
        public int                         ignoresetfilesystem;    /* [w] Optional. Specify 0 to ignore. Set to 1 to use fmod's built in file system. Ignores setFileSystem callbacks and also FMOD_CREATESOUNEXINFO file callbacks.  Useful for specific cases where you don't want to use your own file system but want to use fmod's file system (ie net streaming). */
        public uint                        audioqueuepolicy;       /* [w] Optional. Specify 0 or FMOD_AUDIOQUEUE_CODECPOLICY_DEFAULT to ignore. Policy used to determine whether hardware or software is used for decoding, see FMOD_AUDIOQUEUE_CODECPOLICY for options (iOS >= 3.0 required, otherwise only hardware is available) */
        public uint                        minmidigranularity;     /* [w] Optional. Specify 0 to ignore. Allows you to set a minimum desired MIDI mixer granularity. Values smaller than 512 give greater than default accuracy at the cost of more CPU and vise versa. Specify 0 for default (512 samples). */
        public int                         nonblockthreadid;       /* [w] Optional. Specify 0 to ignore. Specifies a thread index to execute non blocking load on.  Allows for up to 5 threads to be used for loading at once.  This is to avoid one load blocking another.  Maximum value = 4. */
    }
    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Structure defining a reverb environment for FMOD_SOFTWARE based sounds only.<br>

        [REMARKS]
        Note the default reverb properties are the same as the FMOD_PRESET_GENERIC preset.<br>
        Note that integer values that typically range from -10,000 to 1000 are represented in decibels,
        and are of a logarithmic scale, not linear, wheras float values are always linear.<br>
        <br>
        The numerical values listed below are the maximum, minimum and default values for each variable respectively.<br>
        <br>
        Hardware voice / Platform Specific reverb support.<br>
        WII   See FMODWII.H for hardware specific reverb functionality.<br>
        3DS   See FMOD3DS.H for hardware specific reverb functionality.<br>
        PSP   See FMODWII.H for hardware specific reverb functionality.<br>
        <br>
        Members marked with [r] mean the variable is modified by FMOD and is for reading purposes only.  Do not change this value.<br>
        Members marked with [w] mean the variable can be written to.  The user can set the value.<br>
        Members marked with [r/w] are either read or write depending on if you are using System::setReverbProperties (w) or System::getReverbProperties (r).

        [SEE_ALSO]
        System::setReverbProperties
        System::getReverbProperties
        FMOD_REVERB_PRESETS
    ]
    */
#pragma warning disable 414
    [StructLayout(LayoutKind.Sequential)]
    public struct REVERB_PROPERTIES
    {                            /*        MIN     MAX    DEFAULT   DESCRIPTION */
        public float DecayTime;         /* [r/w]  0.0    20000.0 1500.0  Reverberation decay time in ms                                        */
        public float EarlyDelay;        /* [r/w]  0.0    300.0   7.0     Initial reflection delay time                                         */
        public float LateDelay;         /* [r/w]  0.0    100     11.0    Late reverberation delay time relative to initial reflection          */
        public float HFReference;       /* [r/w]  20.0   20000.0 5000    Reference high frequency (hz)                                         */
        public float HFDecayRatio;      /* [r/w]  10.0   100.0   50.0    High-frequency to mid-frequency decay time ratio                      */
        public float Diffusion;         /* [r/w]  0.0    100.0   100.0   Value that controls the echo density in the late reverberation decay. */
        public float Density;           /* [r/w]  0.0    100.0   100.0   Value that controls the modal density in the late reverberation decay */
        public float LowShelfFrequency; /* [r/w]  20.0   1000.0  250.0   Reference low frequency (hz)                                          */
        public float LowShelfGain;      /* [r/w]  -36.0  12.0    0.0     Relative room effect level at low frequencies                         */
        public float HighCut;           /* [r/w]  20.0   20000.0 20000.0 Relative room effect level at high frequencies                        */
        public float EarlyLateMix;      /* [r/w]  0.0    100.0   50.0    Early reflections level relative to room effect                       */
        public float WetLevel;          /* [r/w]  -80.0  20.0    -6.0    Room effect level (at mid frequencies)
                                  * */
        #region wrapperinternal
        public REVERB_PROPERTIES(float decayTime, float earlyDelay, float lateDelay, float hfReference,
            float hfDecayRatio, float diffusion, float density, float lowShelfFrequency, float lowShelfGain,
            float highCut, float earlyLateMix, float wetLevel)
        {
            DecayTime = decayTime;
            EarlyDelay = earlyDelay;
            LateDelay = lateDelay;
            HFReference = hfReference;
            HFDecayRatio = hfDecayRatio;
            Diffusion = diffusion;
            Density = density;
            LowShelfFrequency = lowShelfFrequency;
            LowShelfGain = lowShelfGain;
            HighCut = highCut;
            EarlyLateMix = earlyLateMix;
            WetLevel = wetLevel;
        }
        #endregion
    }
#pragma warning restore 414

    /*
    [DEFINE]
    [
    [NAME]
    FMOD_REVERB_PRESETS

    [DESCRIPTION]
    A set of predefined environment PARAMETERS, created by Creative Labs
    These are used to initialize an FMOD_REVERB_PROPERTIES structure statically.
    ie
    FMOD_REVERB_PROPERTIES prop = FMOD_PRESET_GENERIC;

    [SEE_ALSO]
    System::setReverbProperties
    ]
    */
    public class PRESET
    {
        /*                                                                                  Instance  Env   Diffus  Room   RoomHF  RmLF DecTm   DecHF  DecLF   Refl  RefDel   Revb  RevDel  ModTm  ModDp   HFRef    LFRef   Diffus  Densty  FLAGS */
        public static REVERB_PROPERTIES OFF()                 { return new REVERB_PROPERTIES(  1000,    7,  11, 5000, 100, 100, 100, 250, 0,    20,  96, -80.0f );}
        public static REVERB_PROPERTIES GENERIC()             { return new REVERB_PROPERTIES(  1500,    7,  11, 5000,  83, 100, 100, 250, 0, 14500,  96,  -8.0f );}
        public static REVERB_PROPERTIES PADDEDCELL()          { return new REVERB_PROPERTIES(   170,    1,   2, 5000,  10, 100, 100, 250, 0,   160,  84,  -7.8f );}
        public static REVERB_PROPERTIES ROOM()                { return new REVERB_PROPERTIES(   400,    2,   3, 5000,  83, 100, 100, 250, 0,  6050,  88,  -9.4f );}
        public static REVERB_PROPERTIES BATHROOM()            { return new REVERB_PROPERTIES(  1500,    7,  11, 5000,  54, 100,  60, 250, 0,  2900,  83,   0.5f );}
        public static REVERB_PROPERTIES LIVINGROOM()          { return new REVERB_PROPERTIES(   500,    3,   4, 5000,  10, 100, 100, 250, 0,   160,  58, -19.0f );}
        public static REVERB_PROPERTIES STONEROOM()           { return new REVERB_PROPERTIES(  2300,   12,  17, 5000,  64, 100, 100, 250, 0,  7800,  71,  -8.5f );}
        public static REVERB_PROPERTIES AUDITORIUM()          { return new REVERB_PROPERTIES(  4300,   20,  30, 5000,  59, 100, 100, 250, 0,  5850,  64, -11.7f );}
        public static REVERB_PROPERTIES CONCERTHALL()         { return new REVERB_PROPERTIES(  3900,   20,  29, 5000,  70, 100, 100, 250, 0,  5650,  80,  -9.8f );}
        public static REVERB_PROPERTIES CAVE()                { return new REVERB_PROPERTIES(  2900,   15,  22, 5000, 100, 100, 100, 250, 0, 20000,  59, -11.3f );}
        public static REVERB_PROPERTIES ARENA()               { return new REVERB_PROPERTIES(  7200,   20,  30, 5000,  33, 100, 100, 250, 0,  4500,  80,  -9.6f );}
        public static REVERB_PROPERTIES HANGAR()              { return new REVERB_PROPERTIES( 10000,   20,  30, 5000,  23, 100, 100, 250, 0,  3400,  72,  -7.4f );}
        public static REVERB_PROPERTIES CARPETTEDHALLWAY()    { return new REVERB_PROPERTIES(   300,    2,  30, 5000,  10, 100, 100, 250, 0,   500,  56, -24.0f );}
        public static REVERB_PROPERTIES HALLWAY()             { return new REVERB_PROPERTIES(  1500,    7,  11, 5000,  59, 100, 100, 250, 0,  7800,  87,  -5.5f );}
        public static REVERB_PROPERTIES STONECORRIDOR()       { return new REVERB_PROPERTIES(   270,   13,  20, 5000,  79, 100, 100, 250, 0,  9000,  86,  -6.0f );}
        public static REVERB_PROPERTIES ALLEY()               { return new REVERB_PROPERTIES(  1500,    7,  11, 5000,  86, 100, 100, 250, 0,  8300,  80,  -9.8f );}
        public static REVERB_PROPERTIES FOREST()              { return new REVERB_PROPERTIES(  1500,  162,  88, 5000,  54,  79, 100, 250, 0,   760,  94, -12.3f );}
        public static REVERB_PROPERTIES CITY()                { return new REVERB_PROPERTIES(  1500,    7,  11, 5000,  67,  50, 100, 250, 0,  4050,  66, -26.0f );}
        public static REVERB_PROPERTIES MOUNTAINS()           { return new REVERB_PROPERTIES(  1500,  300, 100, 5000,  21,  27, 100, 250, 0,  1220,  82, -24.0f );}
        public static REVERB_PROPERTIES QUARRY()              { return new REVERB_PROPERTIES(  1500,   61,  25, 5000,  83, 100, 100, 250, 0,  3400, 100,  -5.0f );}
        public static REVERB_PROPERTIES PLAIN()               { return new REVERB_PROPERTIES(  1500,  179, 100, 5000,  50,  21, 100, 250, 0,  1670,  65, -28.0f );}
        public static REVERB_PROPERTIES PARKINGLOT()          { return new REVERB_PROPERTIES(  1700,    8,  12, 5000, 100, 100, 100, 250, 0, 20000,  56, -19.5f );}
        public static REVERB_PROPERTIES SEWERPIPE()           { return new REVERB_PROPERTIES(  2800,   14,  21, 5000,  14,  80,  60, 250, 0,  3400,  66,   1.2f );}
        public static REVERB_PROPERTIES UNDERWATER()          { return new REVERB_PROPERTIES(  1500,    7,  11, 5000,  10, 100, 100, 250, 0,   500,  92,   7.0f );}
    }

    /*
    [STRUCTURE]
    [
        [DESCRIPTION]
        Settings for advanced features like configuring memory and cpu usage for the FMOD_CREATECOMPRESSEDSAMPLE feature.

        [REMARKS]
        maxMPEGCodecs / maxADPCMCodecs / maxXMACodecs will determine the maximum cpu usage of playing realtime samples.  Use this to lower potential excess cpu usage and also control memory usage.<br>

        [SEE_ALSO]
        System::setAdvancedSettings
        System::getAdvancedSettings
    ]
    */
    [StructLayout(LayoutKind.Sequential)]
    public struct ADVANCEDSETTINGS
    {
        public int                 cbSize;                     /* [w]   Size of this structure.  Use sizeof(FMOD_ADVANCEDSETTINGS) */
        public int                 maxMPEGCodecs;              /* [r/w] Optional. Specify 0 to ignore. For use with FMOD_CREATECOMPRESSEDSAMPLE only.  MPEG   codecs consume 30,528 bytes per instance and this number will determine how many MPEG   channels can be played simultaneously. Default = 32. */
        public int                 maxADPCMCodecs;             /* [r/w] Optional. Specify 0 to ignore. For use with FMOD_CREATECOMPRESSEDSAMPLE only.  ADPCM  codecs consume  3,128 bytes per instance and this number will determine how many ADPCM  channels can be played simultaneously. Default = 32. */
        public int                 maxXMACodecs;               /* [r/w] Optional. Specify 0 to ignore. For use with FMOD_CREATECOMPRESSEDSAMPLE only.  XMA    codecs consume 14,836 bytes per instance and this number will determine how many XMA    channels can be played simultaneously. Default = 32. */
        public int                 maxVorbisCodecs;            /* [r/w] Optional. Specify 0 to ignore. For use with FMOD_CREATECOMPRESSEDSAMPLE only.  Vorbis codecs consume 23,256 bytes per instance and this number will determine how many Vorbis channels can be played simultaneously. Default = 32. */    
        public int                 maxAT9Codecs;               /* [r/w] Optional. Specify 0 to ignore. For use with FMOD_CREATECOMPRESSEDSAMPLE only.  AT9    codecs consume  8,720 bytes per instance and this number will determine how many AT9    channels can be played simultaneously. Default = 32. */    
        public int                 maxFADPCMCodecs;            /* [r/w] Optional. Specify 0 to ignore. For use with FMOD_CREATECOMPRESSEDSAMPLE only.  This number will determine how many FADPCM channels can be played simultaneously. Default = 32. */
        public int                 maxPCMCodecs;               /* [r/w] Optional. Specify 0 to ignore. For use with PS3 only.                          PCM    codecs consume 12,672 bytes per instance and this number will determine how many streams and PCM voices can be played simultaneously. Default = 16. */
        public int                 ASIONumChannels;            /* [r/w] Optional. Specify 0 to ignore. Number of channels available on the ASIO device. */
        public IntPtr              ASIOChannelList;            /* [r/w] Optional. Specify 0 to ignore. Pointer to an array of strings (number of entries defined by ASIONumChannels) with ASIO channel names. */
        public IntPtr              ASIOSpeakerList;            /* [r/w] Optional. Specify 0 to ignore. Pointer to a list of speakers that the ASIO channels map to.  This can be called after System::init to remap ASIO output. */
        public float               HRTFMinAngle;               /* [r/w] Optional.                      For use with FMOD_INIT_HRTF_LOWPASS.  The angle range (0-360) of a 3D sound in relation to the listener, at which the HRTF function begins to have an effect. 0 = in front of the listener. 180 = from 90 degrees to the left of the listener to 90 degrees to the right. 360 = behind the listener. Default = 180.0. */
        public float               HRTFMaxAngle;               /* [r/w] Optional.                      For use with FMOD_INIT_HRTF_LOWPASS.  The angle range (0-360) of a 3D sound in relation to the listener, at which the HRTF function has maximum effect. 0 = front of the listener. 180 = from 90 degrees to the left of the listener to 90 degrees to the right. 360 = behind the listener. Default = 360.0. */
        public float               HRTFFreq;                   /* [r/w] Optional. Specify 0 to ignore. For use with FMOD_INIT_HRTF_LOWPASS.  The cutoff frequency of the HRTF's lowpass filter function when at maximum effect. (i.e. at HRTFMaxAngle).  Default = 4000.0. */
        public float               vol0virtualvol;             /* [r/w] Optional. Specify 0 to ignore. For use with FMOD_INIT_VOL0_BECOMES_VIRTUAL.  If this flag is used, and the volume is below this, then the sound will become virtual.  Use this value to raise the threshold to a different point where a sound goes virtual. */
        public uint                defaultDecodeBufferSize;    /* [r/w] Optional. Specify 0 to ignore. For streams. This determines the default size of the double buffer (in milliseconds) that a stream uses.  Default = 400ms */
        public ushort              profilePort;                /* [r/w] Optional. Specify 0 to ignore. For use with FMOD_INIT_PROFILE_ENABLE.  Specify the port to listen on for connections by the profiler application. */
        public uint                geometryMaxFadeTime;        /* [r/w] Optional. Specify 0 to ignore. The maximum time in miliseconds it takes for a channel to fade to the new level when its occlusion changes. */
        public float               distanceFilterCenterFreq;   /* [r/w] Optional. Specify 0 to ignore. For use with FMOD_INIT_DISTANCE_FILTERING.  The default center frequency in Hz for the distance filtering effect. Default = 1500.0. */
        public int                 reverb3Dinstance;           /* [r/w] Optional. Specify 0 to ignore. Out of 0 to 3, 3d reverb spheres will create a phyical reverb unit on this instance slot.  See FMOD_REVERB_PROPERTIES. */
        public int                 DSPBufferPoolSize;          /* [r/w] Optional. Specify 0 to ignore. Number of buffers in DSP buffer pool.  Each buffer will be DSPBlockSize * sizeof(float) * SpeakerModeChannelCount.  ie 7.1 @ 1024 DSP block size = 8 * 1024 * 4 = 32kb.  Default = 8. */
        public uint                stackSizeStream;            /* [r/w] Optional. Specify 0 to ignore. Specify the stack size for the FMOD Stream thread in bytes.  Useful for custom codecs that use excess stack.  Default 49,152 (48kb) */
        public uint                stackSizeNonBlocking;       /* [r/w] Optional. Specify 0 to ignore. Specify the stack size for the FMOD_NONBLOCKING loading thread.  Useful for custom codecs that use excess stack.  Default 65,536 (64kb) */
        public uint                stackSizeMixer;             /* [r/w] Optional. Specify 0 to ignore. Specify the stack size for the FMOD mixer thread.  Useful for custom dsps that use excess stack.  Default 49,152 (48kb) */
        public DSP_RESAMPLER       resamplerMethod;            /* [r/w] Optional. Specify 0 to ignore. Resampling method used with fmod's software mixer.  See FMOD_DSP_RESAMPLER for details on methods. */
        public uint                commandQueueSize;           /* [r/w] Optional. Specify 0 to ignore. Specify the command queue size for thread safe processing.  Default 2048 (2kb) */
        public uint                randomSeed;                 /* [r/w] Optional. Specify 0 to ignore. Seed value that FMOD will use to initialize its internal random number generators. */
    }

    /*
    [DEFINE]
    [
        [NAME]
        FMOD_DRIVER_STATE

        [DESCRIPTION]
        Flags that provide additional information about a particular driver.

        [REMARKS]

        [SEE_ALSO]
        System::getRecordDriverInfo
    ]
    */
    [Flags]
    public enum DRIVER_STATE : uint
    {
        CONNECTED = 0x00000001, /* Device is currently plugged in. */
        DEFAULT   = 0x00000002, /* Device is the users preferred choice. */
    }

    /*
        FMOD System factory functions.  Use this to create an FMOD System Instance.  below you will see System init/close to get started.
    */
    public class Factory
    {

        static Factory()
        {
            DllLoader.PreloadDll(VERSION.dll);
        }

        public static RESULT System_Create(out System system)
        {
            system = null;

            RESULT result   = RESULT.OK;
            IntPtr rawPtr   = new IntPtr();

            result = FMOD_System_Create(out rawPtr);
            if (result != RESULT.OK)
            {
                return result;
            }

            system = new System(rawPtr);

            return result;
        }


        #region importfunctions

        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Create                      (out IntPtr system);

        #endregion
    }

    public class Memory
    {
        public static RESULT Initialize(IntPtr poolmem, int poollen, MEMORY_ALLOC_CALLBACK useralloc, MEMORY_REALLOC_CALLBACK userrealloc, MEMORY_FREE_CALLBACK userfree, MEMORY_TYPE memtypeflags)
        {
            return FMOD_Memory_Initialize(poolmem, poollen, useralloc, userrealloc, userfree, memtypeflags);
        }

        public static RESULT GetStats(out int currentalloced, out int maxalloced)
        {
            return GetStats(out currentalloced, out maxalloced, false);
        }

        public static RESULT GetStats(out int currentalloced, out int maxalloced, bool blocking)
        {
            return FMOD_Memory_GetStats(out currentalloced, out maxalloced, blocking);
        }


        #region importfunctions

        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Memory_Initialize(IntPtr poolmem, int poollen, MEMORY_ALLOC_CALLBACK useralloc, MEMORY_REALLOC_CALLBACK userrealloc, MEMORY_FREE_CALLBACK userfree, MEMORY_TYPE memtypeflags);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Memory_GetStats(out int currentalloced, out int maxalloced, bool blocking);

        #endregion
    }

    public class Debug
    {
        public static RESULT Initialize(DEBUG_FLAGS flags, DEBUG_MODE mode, DEBUG_CALLBACK callback, string filename)
        {
            return FMOD_Debug_Initialize(flags, mode, callback, filename);
        }


        #region importfunctions

        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Debug_Initialize(DEBUG_FLAGS flags, DEBUG_MODE mode, DEBUG_CALLBACK callback, string filename);

        #endregion
    }

    public class HandleBase
    {
        public HandleBase(IntPtr newPtr)
        {
            rawPtr = newPtr;
        }

        public bool isValid()
        {
            return rawPtr != IntPtr.Zero;
        }

        public IntPtr getRaw()
        {
            return rawPtr;
        }

        protected IntPtr rawPtr;

        #region equality

        public override bool Equals(Object obj)
        {
            return Equals(obj as HandleBase);
        }
        public bool Equals(HandleBase p)
        {
            // Equals if p not null and handle is the same
            return ((object)p != null && rawPtr == p.rawPtr);
        }
        public override int GetHashCode()
        {
            return rawPtr.ToInt32();
        }
        public static bool operator ==(HandleBase a, HandleBase b)
        {
            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
            // Return true if the handle matches
            return (a.rawPtr == b.rawPtr);
        }
        public static bool operator !=(HandleBase a, HandleBase b)
        {
            return !(a == b);
        }
        #endregion

    }

    /*
        'System' API.
    */
    public class System : HandleBase
    {
        public RESULT release                ()
        {
            RESULT result = FMOD_System_Release(rawPtr);
            if (result == RESULT.OK)
            {
                rawPtr = IntPtr.Zero;
            }
            return result;
        }


        // Pre-init functions.
        public RESULT setOutput              (OUTPUTTYPE output)
        {
            return FMOD_System_SetOutput(rawPtr, output);
        }
        public RESULT getOutput              (out OUTPUTTYPE output)
        {
            return FMOD_System_GetOutput(rawPtr, out output);
        }
        public RESULT getNumDrivers          (out int numdrivers)
        {
            return FMOD_System_GetNumDrivers(rawPtr, out numdrivers);
        }
        public RESULT getDriverInfo          (int id, StringBuilder name, int namelen, out Guid guid, out int systemrate, out SPEAKERMODE speakermode, out int speakermodechannels)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(name.Capacity);

            RESULT result = FMOD_System_GetDriverInfo(rawPtr, id, stringMem, namelen, out guid, out systemrate, out speakermode, out speakermodechannels);

            StringMarshalHelper.NativeToBuilder(name, stringMem);
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT setDriver              (int driver)
        {
            return FMOD_System_SetDriver(rawPtr, driver);
        }
        public RESULT getDriver              (out int driver)
        {
            return FMOD_System_GetDriver(rawPtr, out driver);
        }
        public RESULT setSoftwareChannels    (int numsoftwarechannels)
        {
            return FMOD_System_SetSoftwareChannels(rawPtr, numsoftwarechannels);
        }
        public RESULT getSoftwareChannels    (out int numsoftwarechannels)
        {
            return FMOD_System_GetSoftwareChannels(rawPtr, out numsoftwarechannels);
        }
        public RESULT setSoftwareFormat      (int samplerate, SPEAKERMODE speakermode, int numrawspeakers)
        {
            return FMOD_System_SetSoftwareFormat(rawPtr, samplerate, speakermode, numrawspeakers);
        }
        public RESULT getSoftwareFormat      (out int samplerate, out SPEAKERMODE speakermode, out int numrawspeakers)
        {
            return FMOD_System_GetSoftwareFormat(rawPtr, out samplerate, out speakermode, out numrawspeakers);
        }
        public RESULT setDSPBufferSize       (uint bufferlength, int numbuffers)
        {
            return FMOD_System_SetDSPBufferSize(rawPtr, bufferlength, numbuffers);
        }
        public RESULT getDSPBufferSize       (out uint bufferlength, out int numbuffers)
        {
            return FMOD_System_GetDSPBufferSize(rawPtr, out bufferlength, out numbuffers);
        }
        public RESULT setFileSystem          (FILE_OPENCALLBACK useropen, FILE_CLOSECALLBACK userclose, FILE_READCALLBACK userread, FILE_SEEKCALLBACK userseek, FILE_ASYNCREADCALLBACK userasyncread, FILE_ASYNCCANCELCALLBACK userasynccancel, int blockalign)
        {
            return FMOD_System_SetFileSystem(rawPtr, useropen, userclose, userread, userseek, userasyncread, userasynccancel, blockalign);
        }
        public RESULT attachFileSystem       (FILE_OPENCALLBACK useropen, FILE_CLOSECALLBACK userclose, FILE_READCALLBACK userread, FILE_SEEKCALLBACK userseek)
        {
            return FMOD_System_AttachFileSystem(rawPtr, useropen, userclose, userread, userseek);
        }
        public RESULT setAdvancedSettings    (ref ADVANCEDSETTINGS settings)
        {
            settings.cbSize = Marshal.SizeOf(settings);
            return FMOD_System_SetAdvancedSettings(rawPtr, ref settings);
        }
        public RESULT getAdvancedSettings    (ref ADVANCEDSETTINGS settings)
        {
            settings.cbSize = Marshal.SizeOf(settings);
            return FMOD_System_GetAdvancedSettings(rawPtr, ref settings);
        }
        public RESULT setCallback            (SYSTEM_CALLBACK callback, SYSTEM_CALLBACK_TYPE callbackmask)
        {
            return FMOD_System_SetCallback(rawPtr, callback, callbackmask);
        }

        // Plug-in support.
        public RESULT setPluginPath          (string path)
        {
            return FMOD_System_SetPluginPath(rawPtr, Encoding.UTF8.GetBytes(path + Char.MinValue));
        }
        public RESULT loadPlugin             (string filename, out uint handle, uint priority)
        {
            return FMOD_System_LoadPlugin(rawPtr, Encoding.UTF8.GetBytes(filename + Char.MinValue), out handle, priority);
        }
        public RESULT loadPlugin             (string filename, out uint handle)
        {
            return loadPlugin(filename, out handle, 0);
        }
        public RESULT unloadPlugin           (uint handle)
        {
            return FMOD_System_UnloadPlugin(rawPtr, handle);
        }
        public RESULT getNumPlugins          (PLUGINTYPE plugintype, out int numplugins)
        {
            return FMOD_System_GetNumPlugins(rawPtr, plugintype, out numplugins);
        }
        public RESULT getPluginHandle        (PLUGINTYPE plugintype, int index, out uint handle)
        {
            return FMOD_System_GetPluginHandle(rawPtr, plugintype, index, out handle);
        }
        public RESULT getPluginInfo          (uint handle, out PLUGINTYPE plugintype, StringBuilder name, int namelen, out uint version)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(name.Capacity);

            RESULT result = FMOD_System_GetPluginInfo(rawPtr, handle, out plugintype, stringMem, namelen, out version);

            StringMarshalHelper.NativeToBuilder(name, stringMem);
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT setOutputByPlugin      (uint handle)
        {
            return FMOD_System_SetOutputByPlugin(rawPtr, handle);
        }
        public RESULT getOutputByPlugin      (out uint handle)
        {
            return FMOD_System_GetOutputByPlugin(rawPtr, out handle);
        }
        public RESULT createDSPByPlugin(uint handle, out DSP dsp)
        {
            dsp = null;

            IntPtr dspraw;
            RESULT result = FMOD_System_CreateDSPByPlugin(rawPtr, handle, out dspraw);
            dsp = new DSP(dspraw);

            return result;
        }
        public RESULT getDSPInfoByPlugin(uint handle, out IntPtr description)
        {
            return FMOD_System_GetDSPInfoByPlugin(rawPtr, handle, out description);
        }
        /*
        public RESULT registerCodec(ref CODEC_DESCRIPTION description, out uint handle, uint priority)
        {
            return FMOD_System_RegisterCodec(rawPtr, ref description, out handle, priority);
        }
        */
        public RESULT registerDSP(ref DSP_DESCRIPTION description, out uint handle)
        {
            return FMOD_System_RegisterDSP(rawPtr, ref description, out handle);
        }
        /*
        public RESULT registerOutput(ref OUTPUT_DESCRIPTION description, out uint handle)
        {
            return FMOD_System_RegisterOutput(rawPtr, ref description, out handle);
        }
        */

        // Init/Close.
        public RESULT init                   (int maxchannels, INITFLAGS flags, IntPtr extradriverdata)
        {
            return FMOD_System_Init(rawPtr, maxchannels, flags, extradriverdata);
        }
        public RESULT close                  ()
        {
            return FMOD_System_Close(rawPtr);
        }


        // General post-init system functions.
        public RESULT update                 ()
        {
            return FMOD_System_Update(rawPtr);
        }

        public RESULT setSpeakerPosition(SPEAKER speaker, float x, float y, bool active)
        {
            return FMOD_System_SetSpeakerPosition(rawPtr, speaker, x, y, active);
        }
        public RESULT getSpeakerPosition(SPEAKER speaker, out float x, out float y, out bool active)
        {
            return FMOD_System_GetSpeakerPosition(rawPtr, speaker, out x, out y, out active);
        }
        public RESULT setStreamBufferSize(uint filebuffersize, TIMEUNIT filebuffersizetype)
        {
            return FMOD_System_SetStreamBufferSize(rawPtr, filebuffersize, filebuffersizetype);
        }
        public RESULT getStreamBufferSize(out uint filebuffersize, out TIMEUNIT filebuffersizetype)
        {
            return FMOD_System_GetStreamBufferSize(rawPtr, out filebuffersize, out filebuffersizetype);
        }
        public RESULT set3DSettings          (float dopplerscale, float distancefactor, float rolloffscale)
        {
            return FMOD_System_Set3DSettings(rawPtr, dopplerscale, distancefactor, rolloffscale);
        }
        public RESULT get3DSettings          (out float dopplerscale, out float distancefactor, out float rolloffscale)
        {
            return FMOD_System_Get3DSettings(rawPtr, out dopplerscale, out distancefactor, out rolloffscale);
        }
        public RESULT set3DNumListeners      (int numlisteners)
        {
            return FMOD_System_Set3DNumListeners(rawPtr, numlisteners);
        }
        public RESULT get3DNumListeners      (out int numlisteners)
        {
            return FMOD_System_Get3DNumListeners(rawPtr, out numlisteners);
        }
        public RESULT set3DListenerAttributes(int listener, ref VECTOR pos, ref VECTOR vel, ref VECTOR forward, ref VECTOR up)
        {
            return FMOD_System_Set3DListenerAttributes(rawPtr, listener, ref pos, ref vel, ref forward, ref up);
        }
        public RESULT get3DListenerAttributes(int listener, out VECTOR pos, out VECTOR vel, out VECTOR forward, out VECTOR up)
        {
            return FMOD_System_Get3DListenerAttributes(rawPtr, listener, out pos, out vel, out forward, out up);
        }
        public RESULT set3DRolloffCallback   (CB_3D_ROLLOFFCALLBACK callback)
        {
            return FMOD_System_Set3DRolloffCallback   (rawPtr, callback);
        }
        public RESULT mixerSuspend           ()
        {
            return FMOD_System_MixerSuspend(rawPtr);
        }
        public RESULT mixerResume            ()
        {
            return FMOD_System_MixerResume(rawPtr);
        }
        public RESULT getDefaultMixMatrix    (SPEAKERMODE sourcespeakermode, SPEAKERMODE targetspeakermode, float[] matrix, int matrixhop)
        {
            return FMOD_System_GetDefaultMixMatrix(rawPtr, sourcespeakermode, targetspeakermode, matrix, matrixhop);
        }
        public RESULT getSpeakerModeChannels (SPEAKERMODE mode, out int channels)
        {
            return FMOD_System_GetSpeakerModeChannels(rawPtr, mode, out channels);
        }

        // System information functions.
        public RESULT getVersion             (out uint version)
        {
            return FMOD_System_GetVersion(rawPtr, out version);
        }
        public RESULT getOutputHandle        (out IntPtr handle)
        {
            return FMOD_System_GetOutputHandle(rawPtr, out handle);
        }
        public RESULT getChannelsPlaying     (out int channels)
        {
            return FMOD_System_GetChannelsPlaying(rawPtr, out channels);
        }
        public RESULT getChannelsReal        (out int channels)
        {
            return FMOD_System_GetChannelsReal(rawPtr, out channels);
        }
        public RESULT getCPUUsage            (out float dsp, out float stream, out float geometry, out float update, out float total)
        {
            return FMOD_System_GetCPUUsage(rawPtr, out dsp, out stream, out geometry, out update, out total);
        }
        public RESULT getSoundRAM            (out int currentalloced, out int maxalloced, out int total)
        {
            return FMOD_System_GetSoundRAM(rawPtr, out currentalloced, out maxalloced, out total);
        }

        // Sound/DSP/Channel/FX creation and retrieval.
        public RESULT createSound            (string name, MODE mode, ref CREATESOUNDEXINFO exinfo, out Sound sound)
        {
            sound = null;

            byte[] stringData;
            stringData = Encoding.UTF8.GetBytes(name + Char.MinValue);
            
            exinfo.cbsize = Marshal.SizeOf(exinfo);

            IntPtr soundraw;
            RESULT result = FMOD_System_CreateSound(rawPtr, stringData, mode, ref exinfo, out soundraw);
            sound = new Sound(soundraw);

            return result;
        }
        public RESULT createSound            (byte[] data, MODE mode, ref CREATESOUNDEXINFO exinfo, out Sound sound)
        {
            sound = null;

            exinfo.cbsize = Marshal.SizeOf(exinfo);

            IntPtr soundraw;
            RESULT result = FMOD_System_CreateSound(rawPtr, data, mode, ref exinfo, out soundraw);
            sound = new Sound(soundraw);

            return result;
        }
        public RESULT createSound            (string name, MODE mode, out Sound sound)
        {
            CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO();
            exinfo.cbsize = Marshal.SizeOf(exinfo);

            return createSound(name, mode, ref exinfo, out sound);
        }
        public RESULT createStream            (string name, MODE mode, ref CREATESOUNDEXINFO exinfo, out Sound sound)
        {
            sound = null;

            byte[] stringData;
            stringData = Encoding.UTF8.GetBytes(name + Char.MinValue);
            
            exinfo.cbsize = Marshal.SizeOf(exinfo);

            IntPtr soundraw;
            RESULT result = FMOD_System_CreateStream(rawPtr, stringData, mode, ref exinfo, out soundraw);
            sound = new Sound(soundraw);

            return result;
        }
        public RESULT createStream            (byte[] data, MODE mode, ref CREATESOUNDEXINFO exinfo, out Sound sound)
        {
            sound = null;

            exinfo.cbsize = Marshal.SizeOf(exinfo);

            IntPtr soundraw;
            RESULT result = FMOD_System_CreateStream(rawPtr, data, mode, ref exinfo, out soundraw);
            sound = new Sound(soundraw);

            return result;
        }
        public RESULT createStream            (string name, MODE mode, out Sound sound)
        {
            CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO();
            exinfo.cbsize = Marshal.SizeOf(exinfo);

            return createStream(name, mode, ref exinfo, out sound);
        }
        public RESULT createDSP              (ref DSP_DESCRIPTION description, out DSP dsp)
        {
            dsp = null;

            IntPtr dspraw;
            RESULT result = FMOD_System_CreateDSP(rawPtr, ref description, out dspraw);
            dsp = new DSP(dspraw);

            return result;
        }
        public RESULT createDSPByType          (DSP_TYPE type, out DSP dsp)
        {
            dsp = null;

            IntPtr dspraw;
            RESULT result = FMOD_System_CreateDSPByType(rawPtr, type, out dspraw);
            dsp = new DSP(dspraw);

            return result;
        }
        public RESULT createChannelGroup     (string name, out ChannelGroup channelgroup)
        {
            channelgroup = null;

            byte[] stringData = Encoding.UTF8.GetBytes(name + Char.MinValue);

            IntPtr channelgroupraw;
            RESULT result = FMOD_System_CreateChannelGroup(rawPtr, stringData, out channelgroupraw);
            channelgroup = new ChannelGroup(channelgroupraw);

            return result;
        }
        public RESULT createSoundGroup       (string name, out SoundGroup soundgroup)
        {
            soundgroup = null;

            byte[] stringData = Encoding.UTF8.GetBytes(name + Char.MinValue);

            IntPtr soundgroupraw;
            RESULT result = FMOD_System_CreateSoundGroup(rawPtr, stringData, out soundgroupraw);
            soundgroup = new SoundGroup(soundgroupraw);

            return result;
        }
        public RESULT createReverb3D         (out Reverb3D reverb)
        {
            IntPtr reverbraw;
            RESULT result = FMOD_System_CreateReverb3D(rawPtr, out reverbraw);
            reverb = new Reverb3D(reverbraw);

            return result;
        }
        public RESULT playSound              (Sound sound, ChannelGroup channelGroup, bool paused, out Channel channel)
        {
            channel = null;

            IntPtr channelGroupRaw = (channelGroup != null) ? channelGroup.getRaw() : IntPtr.Zero;

            IntPtr channelraw;
            RESULT result = FMOD_System_PlaySound(rawPtr, sound.getRaw(), channelGroupRaw, paused, out channelraw);
            channel = new Channel(channelraw);

            return result;
        }
        public RESULT playDSP                (DSP dsp, ChannelGroup channelGroup, bool paused, out Channel channel)
        {
            channel = null;

            IntPtr channelGroupRaw = (channelGroup != null) ? channelGroup.getRaw() : IntPtr.Zero;

            IntPtr channelraw;
            RESULT result = FMOD_System_PlayDSP(rawPtr, dsp.getRaw(), channelGroupRaw, paused, out channelraw);
            channel = new Channel(channelraw);

            return result;
        }
        public RESULT getChannel             (int channelid, out Channel channel)
        {
            channel = null;

            IntPtr channelraw;
            RESULT result = FMOD_System_GetChannel(rawPtr, channelid, out channelraw);
            channel = new Channel(channelraw);

            return result;
        }
        public RESULT getMasterChannelGroup  (out ChannelGroup channelgroup)
        {
            channelgroup = null;

            IntPtr channelgroupraw;
            RESULT result = FMOD_System_GetMasterChannelGroup(rawPtr, out channelgroupraw);
            channelgroup = new ChannelGroup(channelgroupraw);

            return result;
        }
        public RESULT getMasterSoundGroup    (out SoundGroup soundgroup)
        {
            soundgroup = null;

            IntPtr soundgroupraw;
            RESULT result = FMOD_System_GetMasterSoundGroup(rawPtr, out soundgroupraw);
            soundgroup = new SoundGroup(soundgroupraw);

            return result;
        }

        // Routing to ports.
        public RESULT attachChannelGroupToPort(uint portType, ulong portIndex, ChannelGroup channelgroup, bool passThru = false)
        {
            return FMOD_System_AttachChannelGroupToPort(rawPtr, portType, portIndex, channelgroup.getRaw(), passThru);
        }
        public RESULT detachChannelGroupFromPort(ChannelGroup channelgroup)
        {
            return FMOD_System_DetachChannelGroupFromPort(rawPtr, channelgroup.getRaw());
        }

        // Reverb api.
        public RESULT setReverbProperties    (int instance, ref REVERB_PROPERTIES prop)
        {
            return FMOD_System_SetReverbProperties(rawPtr, instance, ref prop);
        }
        public RESULT getReverbProperties    (int instance, out REVERB_PROPERTIES prop)
        {
            return FMOD_System_GetReverbProperties(rawPtr, instance, out prop);
        }

        // System level DSP functionality.
        public RESULT lockDSP            ()
        {
            return FMOD_System_LockDSP(rawPtr);
        }
        public RESULT unlockDSP          ()
        {
            return FMOD_System_UnlockDSP(rawPtr);
        }

        // Recording api
        public RESULT getRecordNumDrivers    (out int numdrivers, out int numconnected)
        {
            return FMOD_System_GetRecordNumDrivers(rawPtr, out numdrivers, out numconnected);
        }
        public RESULT getRecordDriverInfo(int id, StringBuilder name, int namelen, out Guid guid, out int systemrate, out SPEAKERMODE speakermode, out int speakermodechannels, out DRIVER_STATE state)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(name.Capacity);

            RESULT result = FMOD_System_GetRecordDriverInfo(rawPtr, id, stringMem, namelen, out guid, out systemrate, out speakermode, out speakermodechannels, out state);

            StringMarshalHelper.NativeToBuilder(name, stringMem);
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT getRecordPosition      (int id, out uint position)
        {
            return FMOD_System_GetRecordPosition(rawPtr, id, out position);
        }
        public RESULT recordStart            (int id, Sound sound, bool loop)
        {
            return FMOD_System_RecordStart(rawPtr, id, sound.getRaw(), loop);
        }
        public RESULT recordStop             (int id)
        {
            return FMOD_System_RecordStop(rawPtr, id);
        }
        public RESULT isRecording            (int id, out bool recording)
        {
            return FMOD_System_IsRecording(rawPtr, id, out recording);
        }

        // Geometry api
        public RESULT createGeometry         (int maxpolygons, int maxvertices, out Geometry geometry)
        {
            geometry = null;

            IntPtr geometryraw;
            RESULT result = FMOD_System_CreateGeometry(rawPtr, maxpolygons, maxvertices, out geometryraw);
            geometry = new Geometry(geometryraw);

            return result;
        }
        public RESULT setGeometrySettings    (float maxworldsize)
        {
            return FMOD_System_SetGeometrySettings(rawPtr, maxworldsize);
        }
        public RESULT getGeometrySettings    (out float maxworldsize)
        {
            return FMOD_System_GetGeometrySettings(rawPtr, out maxworldsize);
        }
        public RESULT loadGeometry(IntPtr data, int datasize, out Geometry geometry)
        {
            geometry = null;

            IntPtr geometryraw;
            RESULT result = FMOD_System_LoadGeometry(rawPtr, data, datasize, out geometryraw);
            geometry = new Geometry(geometryraw);

            return result;
        }
        public RESULT getGeometryOcclusion    (ref VECTOR listener, ref VECTOR source, out float direct, out float reverb)
        {
            return FMOD_System_GetGeometryOcclusion(rawPtr, ref listener, ref source, out direct, out reverb);
        }

        // Network functions
        public RESULT setNetworkProxy               (string proxy)
        {
            return FMOD_System_SetNetworkProxy(rawPtr, Encoding.UTF8.GetBytes(proxy + Char.MinValue));
        }
        public RESULT getNetworkProxy               (StringBuilder proxy, int proxylen)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(proxy.Capacity);

            RESULT result = FMOD_System_GetNetworkProxy(rawPtr, stringMem, proxylen);

            StringMarshalHelper.NativeToBuilder(proxy, stringMem);
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT setNetworkTimeout      (int timeout)
        {
            return FMOD_System_SetNetworkTimeout(rawPtr, timeout);
        }
        public RESULT getNetworkTimeout(out int timeout)
        {
            return FMOD_System_GetNetworkTimeout(rawPtr, out timeout);
        }

        // Userdata set/get
        public RESULT setUserData            (IntPtr userdata)
        {
            return FMOD_System_SetUserData(rawPtr, userdata);
        }
        public RESULT getUserData            (out IntPtr userdata)
        {
            return FMOD_System_GetUserData(rawPtr, out userdata);
        }


        #region importfunctions
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Release                (IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetOutput              (IntPtr system, OUTPUTTYPE output);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetOutput              (IntPtr system, out OUTPUTTYPE output);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetNumDrivers          (IntPtr system, out int numdrivers);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetDriverInfo          (IntPtr system, int id, IntPtr name, int namelen, out Guid guid, out int systemrate, out SPEAKERMODE speakermode, out int speakermodechannels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetDriver              (IntPtr system, int driver);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetDriver              (IntPtr system, out int driver);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetSoftwareChannels    (IntPtr system, int numsoftwarechannels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetSoftwareChannels    (IntPtr system, out int numsoftwarechannels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetSoftwareFormat      (IntPtr system, int samplerate, SPEAKERMODE speakermode, int numrawspeakers);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetSoftwareFormat      (IntPtr system, out int samplerate, out SPEAKERMODE speakermode, out int numrawspeakers);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetDSPBufferSize       (IntPtr system, uint bufferlength, int numbuffers);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetDSPBufferSize       (IntPtr system, out uint bufferlength, out int numbuffers);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetFileSystem          (IntPtr system, FILE_OPENCALLBACK useropen, FILE_CLOSECALLBACK userclose, FILE_READCALLBACK userread, FILE_SEEKCALLBACK userseek, FILE_ASYNCREADCALLBACK userasyncread, FILE_ASYNCCANCELCALLBACK userasynccancel, int blockalign);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_AttachFileSystem       (IntPtr system, FILE_OPENCALLBACK useropen, FILE_CLOSECALLBACK userclose, FILE_READCALLBACK userread, FILE_SEEKCALLBACK userseek);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetPluginPath          (IntPtr system, byte[] path);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_LoadPlugin             (IntPtr system, byte[] filename, out uint handle, uint priority);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_UnloadPlugin           (IntPtr system, uint handle);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetNumPlugins          (IntPtr system, PLUGINTYPE plugintype, out int numplugins);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetPluginHandle        (IntPtr system, PLUGINTYPE plugintype, int index, out uint handle);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetPluginInfo          (IntPtr system, uint handle, out PLUGINTYPE plugintype, IntPtr name, int namelen, out uint version);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateDSPByPlugin      (IntPtr system, uint handle, out IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetOutputByPlugin      (IntPtr system, uint handle);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetOutputByPlugin      (IntPtr system, out uint handle);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetDSPInfoByPlugin     (IntPtr system, uint handle, out IntPtr description);
        [DllImport(VERSION.dll)]
        //private static extern RESULT FMOD_System_RegisterCodec          (IntPtr system, out CODEC_DESCRIPTION description, out uint handle, uint priority);
        //[DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_RegisterDSP            (IntPtr system, ref DSP_DESCRIPTION description, out uint handle);
        [DllImport(VERSION.dll)]
        //private static extern RESULT FMOD_System_RegisterOutput         (IntPtr system, ref OUTPUT_DESCRIPTION description, out uint handle);
        //[DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Init                   (IntPtr system, int maxchannels, INITFLAGS flags, IntPtr extradriverdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Close                  (IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Update                 (IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetAdvancedSettings    (IntPtr system, ref ADVANCEDSETTINGS settings);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetAdvancedSettings    (IntPtr system, ref ADVANCEDSETTINGS settings);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Set3DRolloffCallback   (IntPtr system, CB_3D_ROLLOFFCALLBACK callback);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_MixerSuspend           (IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_MixerResume            (IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetDefaultMixMatrix    (IntPtr system, SPEAKERMODE sourcespeakermode, SPEAKERMODE targetspeakermode, float[] matrix, int matrixhop);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetSpeakerModeChannels (IntPtr system, SPEAKERMODE mode, out int channels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetCallback            (IntPtr system, SYSTEM_CALLBACK callback, SYSTEM_CALLBACK_TYPE callbackmask);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetSpeakerPosition     (IntPtr system, SPEAKER speaker, float x, float y, bool active);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetSpeakerPosition     (IntPtr system, SPEAKER speaker, out float x, out float y, out bool active);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Set3DSettings          (IntPtr system, float dopplerscale, float distancefactor, float rolloffscale);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Get3DSettings          (IntPtr system, out float dopplerscale, out float distancefactor, out float rolloffscale);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Set3DNumListeners      (IntPtr system, int numlisteners);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Get3DNumListeners      (IntPtr system, out int numlisteners);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Set3DListenerAttributes(IntPtr system, int listener, ref VECTOR pos, ref VECTOR vel, ref VECTOR forward, ref VECTOR up);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Get3DListenerAttributes(IntPtr system, int listener, out VECTOR pos, out VECTOR vel, out VECTOR forward, out VECTOR up);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetStreamBufferSize    (IntPtr system, uint filebuffersize, TIMEUNIT filebuffersizetype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetStreamBufferSize    (IntPtr system, out uint filebuffersize, out TIMEUNIT filebuffersizetype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetVersion             (IntPtr system, out uint version);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetOutputHandle        (IntPtr system, out IntPtr handle);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetChannelsPlaying     (IntPtr system, out int channels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetChannelsReal        (IntPtr system, out int channels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetCPUUsage            (IntPtr system, out float dsp, out float stream, out float geometry, out float update, out float total);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetSoundRAM            (IntPtr system, out int currentalloced, out int maxalloced, out int total);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateSound            (IntPtr system, byte[] name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, out IntPtr sound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateStream           (IntPtr system, byte[] name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, out IntPtr sound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateDSP              (IntPtr system, ref DSP_DESCRIPTION description, out IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateDSPByType        (IntPtr system, DSP_TYPE type, out IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateChannelGroup     (IntPtr system, byte[] name, out IntPtr channelgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateSoundGroup       (IntPtr system, byte[] name, out IntPtr soundgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateReverb3D         (IntPtr system, out IntPtr reverb);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_PlaySound              (IntPtr system, IntPtr sound, IntPtr channelGroup, bool paused, out IntPtr channel);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_PlayDSP                (IntPtr system, IntPtr dsp, IntPtr channelGroup, bool paused, out IntPtr channel);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetChannel             (IntPtr system, int channelid, out IntPtr channel);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetMasterChannelGroup  (IntPtr system, out IntPtr channelgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetMasterSoundGroup    (IntPtr system, out IntPtr soundgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_AttachChannelGroupToPort  (IntPtr system, uint portType, ulong portIndex, IntPtr channelgroup, bool passThru);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_DetachChannelGroupFromPort(IntPtr system, IntPtr channelgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetReverbProperties    (IntPtr system, int instance, ref REVERB_PROPERTIES prop);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetReverbProperties    (IntPtr system, int instance, out REVERB_PROPERTIES prop);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_LockDSP                (IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_UnlockDSP              (IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetRecordNumDrivers    (IntPtr system, out int numdrivers, out int numconnected);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetRecordDriverInfo    (IntPtr system, int id, IntPtr name, int namelen, out Guid guid, out int systemrate, out SPEAKERMODE speakermode, out int speakermodechannels, out DRIVER_STATE state);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetRecordPosition      (IntPtr system, int id, out uint position);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_RecordStart            (IntPtr system, int id, IntPtr sound, bool loop);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_RecordStop             (IntPtr system, int id);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_IsRecording            (IntPtr system, int id, out bool recording);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateGeometry         (IntPtr system, int maxpolygons, int maxvertices, out IntPtr geometry);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetGeometrySettings    (IntPtr system, float maxworldsize);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetGeometrySettings    (IntPtr system, out float maxworldsize);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_LoadGeometry           (IntPtr system, IntPtr data, int datasize, out IntPtr geometry);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetGeometryOcclusion   (IntPtr system, ref VECTOR listener, ref VECTOR source, out float direct, out float reverb);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetNetworkProxy        (IntPtr system, byte[] proxy);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetNetworkProxy        (IntPtr system, IntPtr proxy, int proxylen);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetNetworkTimeout      (IntPtr system, int timeout);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetNetworkTimeout      (IntPtr system, out int timeout);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetUserData            (IntPtr system, IntPtr userdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetUserData            (IntPtr system, out IntPtr userdata);
        #endregion

        #region wrapperinternal

        public System(IntPtr raw)
            : base(raw)
        {
        }

        #endregion
    }


    /*
        'Sound' API.
    */
    public class Sound : HandleBase
    {
        public RESULT release                 ()
        {
            RESULT result = FMOD_Sound_Release(rawPtr);
            if (result == RESULT.OK)
            {
                rawPtr = IntPtr.Zero;
            }
            return result;
        }
        public RESULT getSystemObject         (out System system)
        {
            system = null;

            IntPtr systemraw;
            RESULT result = FMOD_Sound_GetSystemObject(rawPtr, out systemraw);
            system = new System(systemraw);

            return result;
        }

        // Standard sound manipulation functions.
        public RESULT @lock                   (uint offset, uint length, out IntPtr ptr1, out IntPtr ptr2, out uint len1, out uint len2)
        {
            return FMOD_Sound_Lock(rawPtr, offset, length, out ptr1, out ptr2, out len1, out len2);
        }
        public RESULT unlock                  (IntPtr ptr1,  IntPtr ptr2, uint len1, uint len2)
        {
            return FMOD_Sound_Unlock(rawPtr, ptr1, ptr2, len1, len2);
        }
        public RESULT setDefaults             (float frequency, int priority)
        {
            return FMOD_Sound_SetDefaults(rawPtr, frequency, priority);
        }
        public RESULT getDefaults             (out float frequency, out int priority)
        {
            return FMOD_Sound_GetDefaults(rawPtr, out frequency, out priority);
        }
        public RESULT set3DMinMaxDistance     (float min, float max)
        {
            return FMOD_Sound_Set3DMinMaxDistance(rawPtr, min, max);
        }
        public RESULT get3DMinMaxDistance     (out float min, out float max)
        {
            return FMOD_Sound_Get3DMinMaxDistance(rawPtr, out min, out max);
        }
        public RESULT set3DConeSettings       (float insideconeangle, float outsideconeangle, float outsidevolume)
        {
            return FMOD_Sound_Set3DConeSettings(rawPtr, insideconeangle, outsideconeangle, outsidevolume);
        }
        public RESULT get3DConeSettings       (out float insideconeangle, out float outsideconeangle, out float outsidevolume)
        {
            return FMOD_Sound_Get3DConeSettings(rawPtr, out insideconeangle, out outsideconeangle, out outsidevolume);
        }
        public RESULT set3DCustomRolloff      (ref VECTOR points, int numpoints)
        {
            return FMOD_Sound_Set3DCustomRolloff(rawPtr, ref points, numpoints);
        }
        public RESULT get3DCustomRolloff      (out IntPtr points, out int numpoints)
        {
            return FMOD_Sound_Get3DCustomRolloff(rawPtr, out points, out numpoints);
        }
        public RESULT getSubSound             (int index, out Sound subsound)
        {
            subsound = null;

            IntPtr subsoundraw;
            RESULT result = FMOD_Sound_GetSubSound(rawPtr, index, out subsoundraw);
            subsound = new Sound(subsoundraw);

            return result;
        }
        public RESULT getSubSoundParent(out Sound parentsound)
        {
            parentsound = null;

            IntPtr subsoundraw;
            RESULT result = FMOD_Sound_GetSubSoundParent(rawPtr, out subsoundraw);
            parentsound = new Sound(subsoundraw);

            return result;
        }
        public RESULT getName                 (StringBuilder name, int namelen)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(name.Capacity);

            RESULT result = FMOD_Sound_GetName(rawPtr, stringMem, namelen);

            StringMarshalHelper.NativeToBuilder(name, stringMem);
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT getLength               (out uint length, TIMEUNIT lengthtype)
        {
            return FMOD_Sound_GetLength(rawPtr, out length, lengthtype);
        }
        public RESULT getFormat               (out SOUND_TYPE type, out SOUND_FORMAT format, out int channels, out int bits)
        {
            return FMOD_Sound_GetFormat(rawPtr, out type, out format, out channels, out bits);
        }
        public RESULT getNumSubSounds         (out int numsubsounds)
        {
            return FMOD_Sound_GetNumSubSounds(rawPtr, out numsubsounds);
        }
        public RESULT getNumTags              (out int numtags, out int numtagsupdated)
        {
            return FMOD_Sound_GetNumTags(rawPtr, out numtags, out numtagsupdated);
        }
        public RESULT getTag                  (string name, int index, out TAG tag)
        {
            return FMOD_Sound_GetTag(rawPtr, name, index, out tag);
        }
        public RESULT getOpenState            (out OPENSTATE openstate, out uint percentbuffered, out bool starving, out bool diskbusy)
        {
            return FMOD_Sound_GetOpenState(rawPtr, out openstate, out percentbuffered, out starving, out diskbusy);
        }
        public RESULT readData                (IntPtr buffer, uint lenbytes, out uint read)
        {
            return FMOD_Sound_ReadData(rawPtr, buffer, lenbytes, out read);
        }
        public RESULT seekData                (uint pcm)
        {
            return FMOD_Sound_SeekData(rawPtr, pcm);
        }
        public RESULT setSoundGroup           (SoundGroup soundgroup)
        {
            return FMOD_Sound_SetSoundGroup(rawPtr, soundgroup.getRaw());
        }
        public RESULT getSoundGroup           (out SoundGroup soundgroup)
        {
            soundgroup = null;

            IntPtr soundgroupraw;
            RESULT result = FMOD_Sound_GetSoundGroup(rawPtr, out soundgroupraw);
            soundgroup = new SoundGroup(soundgroupraw);

            return result;
        }

        // Synchronization point API.  These points can come from markers embedded in wav files, and can also generate channel callbacks.
        public RESULT getNumSyncPoints        (out int numsyncpoints)
        {
            return FMOD_Sound_GetNumSyncPoints(rawPtr, out numsyncpoints);
        }
        public RESULT getSyncPoint            (int index, out IntPtr point)
        {
            return FMOD_Sound_GetSyncPoint(rawPtr, index, out point);
        }
        public RESULT getSyncPointInfo        (IntPtr point, StringBuilder name, int namelen, out uint offset, TIMEUNIT offsettype)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(name.Capacity);

            RESULT result = FMOD_Sound_GetSyncPointInfo(rawPtr, point, stringMem, namelen, out offset, offsettype);

            StringMarshalHelper.NativeToBuilder(name, stringMem);
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT addSyncPoint            (uint offset, TIMEUNIT offsettype, string name, out IntPtr point)
        {
            return FMOD_Sound_AddSyncPoint(rawPtr, offset, offsettype, name, out point);
        }
        public RESULT deleteSyncPoint         (IntPtr point)
        {
            return FMOD_Sound_DeleteSyncPoint(rawPtr, point);
        }

        // Functions also in Channel class but here they are the 'default' to save having to change it in Channel all the time.
        public RESULT setMode                 (MODE mode)
        {
            return FMOD_Sound_SetMode(rawPtr, mode);
        }
        public RESULT getMode                 (out MODE mode)
        {
            return FMOD_Sound_GetMode(rawPtr, out mode);
        }
        public RESULT setLoopCount            (int loopcount)
        {
            return FMOD_Sound_SetLoopCount(rawPtr, loopcount);
        }
        public RESULT getLoopCount            (out int loopcount)
        {
            return FMOD_Sound_GetLoopCount(rawPtr, out loopcount);
        }
        public RESULT setLoopPoints           (uint loopstart, TIMEUNIT loopstarttype, uint loopend, TIMEUNIT loopendtype)
        {
            return FMOD_Sound_SetLoopPoints(rawPtr, loopstart, loopstarttype, loopend, loopendtype);
        }
        public RESULT getLoopPoints           (out uint loopstart, TIMEUNIT loopstarttype, out uint loopend, TIMEUNIT loopendtype)
        {
            return FMOD_Sound_GetLoopPoints(rawPtr, out loopstart, loopstarttype, out loopend, loopendtype);
        }

        // For MOD/S3M/XM/IT/MID sequenced formats only.
        public RESULT getMusicNumChannels     (out int numchannels)
        {
            return FMOD_Sound_GetMusicNumChannels(rawPtr, out numchannels);
        }
        public RESULT setMusicChannelVolume   (int channel, float volume)
        {
            return FMOD_Sound_SetMusicChannelVolume(rawPtr, channel, volume);
        }
        public RESULT getMusicChannelVolume   (int channel, out float volume)
        {
            return FMOD_Sound_GetMusicChannelVolume(rawPtr, channel, out volume);
        }
        public RESULT setMusicSpeed(float speed)
        {
            return FMOD_Sound_SetMusicSpeed(rawPtr, speed);
        }
        public RESULT getMusicSpeed(out float speed)
        {
            return FMOD_Sound_GetMusicSpeed(rawPtr, out speed);
        }

        // Userdata set/get.
        public RESULT setUserData             (IntPtr userdata)
        {
            return FMOD_Sound_SetUserData(rawPtr, userdata);
        }
        public RESULT getUserData             (out IntPtr userdata)
        {
            return FMOD_Sound_GetUserData(rawPtr, out userdata);
        }


        #region importfunctions
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Release                 (IntPtr sound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetSystemObject         (IntPtr sound, out IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Lock                   (IntPtr sound, uint offset, uint length, out IntPtr ptr1, out IntPtr ptr2, out uint len1, out uint len2);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Unlock                  (IntPtr sound, IntPtr ptr1,  IntPtr ptr2, uint len1, uint len2);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetDefaults             (IntPtr sound, float frequency, int priority);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetDefaults             (IntPtr sound, out float frequency, out int priority);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Set3DMinMaxDistance     (IntPtr sound, float min, float max);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Get3DMinMaxDistance     (IntPtr sound, out float min, out float max);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Set3DConeSettings       (IntPtr sound, float insideconeangle, float outsideconeangle, float outsidevolume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Get3DConeSettings       (IntPtr sound, out float insideconeangle, out float outsideconeangle, out float outsidevolume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Set3DCustomRolloff      (IntPtr sound, ref VECTOR points, int numpoints);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Get3DCustomRolloff      (IntPtr sound, out IntPtr points, out int numpoints);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetSubSound             (IntPtr sound, int index, out IntPtr subsound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetSubSoundParent       (IntPtr sound, out IntPtr parentsound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetName                 (IntPtr sound, IntPtr name, int namelen);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetLength               (IntPtr sound, out uint length, TIMEUNIT lengthtype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetFormat               (IntPtr sound, out SOUND_TYPE type, out SOUND_FORMAT format, out int channels, out int bits);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetNumSubSounds         (IntPtr sound, out int numsubsounds);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetNumTags              (IntPtr sound, out int numtags, out int numtagsupdated);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetTag                  (IntPtr sound, string name, int index, out TAG tag);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetOpenState            (IntPtr sound, out OPENSTATE openstate, out uint percentbuffered, out bool starving, out bool diskbusy);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_ReadData                (IntPtr sound, IntPtr buffer, uint lenbytes, out uint read);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SeekData                (IntPtr sound, uint pcm);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetSoundGroup           (IntPtr sound, IntPtr soundgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetSoundGroup           (IntPtr sound, out IntPtr soundgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetNumSyncPoints        (IntPtr sound, out int numsyncpoints);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetSyncPoint            (IntPtr sound, int index, out IntPtr point);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetSyncPointInfo        (IntPtr sound, IntPtr point, IntPtr name, int namelen, out uint offset, TIMEUNIT offsettype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_AddSyncPoint            (IntPtr sound, uint offset, TIMEUNIT offsettype, string name, out IntPtr point);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_DeleteSyncPoint         (IntPtr sound, IntPtr point);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetMode                 (IntPtr sound, MODE mode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetMode                 (IntPtr sound, out MODE mode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetLoopCount            (IntPtr sound, int loopcount);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetLoopCount            (IntPtr sound, out int loopcount);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetLoopPoints           (IntPtr sound, uint loopstart, TIMEUNIT loopstarttype, uint loopend, TIMEUNIT loopendtype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetLoopPoints           (IntPtr sound, out uint loopstart, TIMEUNIT loopstarttype, out uint loopend, TIMEUNIT loopendtype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetMusicNumChannels     (IntPtr sound, out int numchannels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetMusicChannelVolume   (IntPtr sound, int channel, float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetMusicChannelVolume   (IntPtr sound, int channel, out float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetMusicSpeed           (IntPtr sound, float speed);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetMusicSpeed           (IntPtr sound, out float speed);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetUserData             (IntPtr sound, IntPtr userdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetUserData             (IntPtr sound, out IntPtr userdata);
        #endregion

        #region wrapperinternal

        public Sound(IntPtr raw)
            : base(raw)
        {
        }

        #endregion
    }


    /*
        'ChannelControl' API
    */
    public class ChannelControl : HandleBase
    {
        public RESULT getSystemObject(out System system)
        {
            system = null;

            IntPtr systemraw;
            RESULT result = FMOD_ChannelGroup_GetSystemObject(rawPtr, out systemraw);
            system = new System(systemraw);

            return result;
        }

        // General control functionality for Channels and ChannelGroups.
        public RESULT stop()
        {
            return FMOD_ChannelGroup_Stop(rawPtr);
        }
        public RESULT setPaused(bool paused)
        {
            return FMOD_ChannelGroup_SetPaused(rawPtr, paused);
        }
        public RESULT getPaused(out bool paused)
        {
            return FMOD_ChannelGroup_GetPaused(rawPtr, out paused);
        }
        public RESULT setVolume(float volume)
        {
            return FMOD_ChannelGroup_SetVolume(rawPtr, volume);
        }
        public RESULT getVolume(out float volume)
        {
            return FMOD_ChannelGroup_GetVolume(rawPtr, out volume);
        }
        public RESULT setVolumeRamp(bool ramp)
        {
            return FMOD_ChannelGroup_SetVolumeRamp(rawPtr, ramp);
        }
        public RESULT getVolumeRamp(out bool ramp)
        {
            return FMOD_ChannelGroup_GetVolumeRamp(rawPtr, out ramp);
        }
        public RESULT getAudibility(out float audibility)
        {
            return FMOD_ChannelGroup_GetAudibility(rawPtr, out audibility);
        }
        public RESULT setPitch(float pitch)
        {
            return FMOD_ChannelGroup_SetPitch(rawPtr, pitch);
        }
        public RESULT getPitch(out float pitch)
        {
            return FMOD_ChannelGroup_GetPitch(rawPtr, out pitch);
        }
        public RESULT setMute(bool mute)
        {
            return FMOD_ChannelGroup_SetMute(rawPtr, mute);
        }
        public RESULT getMute(out bool mute)
        {
            return FMOD_ChannelGroup_GetMute(rawPtr, out mute);
        }
        public RESULT setReverbProperties(int instance, float wet)
        {
            return FMOD_ChannelGroup_SetReverbProperties(rawPtr, instance, wet);
        }
        public RESULT getReverbProperties(int instance, out float wet)
        {
            return FMOD_ChannelGroup_GetReverbProperties(rawPtr, instance, out wet);
        }
        public RESULT setLowPassGain(float gain)
        {
            return FMOD_ChannelGroup_SetLowPassGain(rawPtr, gain);
        }
        public RESULT getLowPassGain(out float gain)
        {
            return FMOD_ChannelGroup_GetLowPassGain(rawPtr, out gain);
        }
        public RESULT setMode(MODE mode)
        {
            return FMOD_ChannelGroup_SetMode(rawPtr, mode);
        }
        public RESULT getMode(out MODE mode)
        {
            return FMOD_ChannelGroup_GetMode(rawPtr, out mode);
        }
        public RESULT setCallback(CHANNEL_CALLBACK callback)
        {
            return FMOD_ChannelGroup_SetCallback(rawPtr, callback);
        }
        public RESULT isPlaying(out bool isplaying)
        {
            return FMOD_ChannelGroup_IsPlaying(rawPtr, out isplaying);
        }

        // Panning and level adjustment.
        public RESULT setPan(float pan)
        {
            return FMOD_ChannelGroup_SetPan(rawPtr, pan);
        }
        public RESULT setMixLevelsOutput(float frontleft, float frontright, float center, float lfe, float surroundleft, float surroundright, float backleft, float backright)
        {
            return FMOD_ChannelGroup_SetMixLevelsOutput(rawPtr, frontleft, frontright, center, lfe,
                surroundleft, surroundright, backleft, backright);
        }
        public RESULT setMixLevelsInput(float[] levels, int numlevels)
        {
            return FMOD_ChannelGroup_SetMixLevelsInput(rawPtr, levels, numlevels);
        }
        public RESULT setMixMatrix(float[] matrix, int outchannels, int inchannels, int inchannel_hop)
        {
            return FMOD_ChannelGroup_SetMixMatrix(rawPtr, matrix, outchannels, inchannels, inchannel_hop);
        }
        public RESULT getMixMatrix(float[] matrix, out int outchannels, out int inchannels, int inchannel_hop)
        {
            return FMOD_ChannelGroup_GetMixMatrix(rawPtr, matrix, out outchannels, out inchannels, inchannel_hop);
        }

        // Clock based functionality.
        public RESULT getDSPClock(out ulong dspclock, out ulong parentclock)
        {
            return FMOD_ChannelGroup_GetDSPClock(rawPtr, out dspclock, out parentclock);
        }
        public RESULT setDelay(ulong dspclock_start, ulong dspclock_end, bool stopchannels)
        {
            return FMOD_ChannelGroup_SetDelay(rawPtr, dspclock_start, dspclock_end, stopchannels);
        }
        public RESULT getDelay(out ulong dspclock_start, out ulong dspclock_end, out bool stopchannels)
        {
            return FMOD_ChannelGroup_GetDelay(rawPtr, out dspclock_start, out dspclock_end, out stopchannels);
        }
        public RESULT addFadePoint(ulong dspclock, float volume)
        {
            return FMOD_ChannelGroup_AddFadePoint(rawPtr, dspclock, volume);
        }
        public RESULT setFadePointRamp(ulong dspclock, float volume)
        {
            return FMOD_ChannelGroup_SetFadePointRamp(rawPtr, dspclock, volume);
        }
        public RESULT removeFadePoints(ulong dspclock_start, ulong dspclock_end)
        {
            return FMOD_ChannelGroup_RemoveFadePoints(rawPtr, dspclock_start, dspclock_end);
        }
        public RESULT getFadePoints(ref uint numpoints, ulong[] point_dspclock, float[] point_volume)
        {
            return FMOD_ChannelGroup_GetFadePoints(rawPtr, ref numpoints, point_dspclock, point_volume);
        }

        // DSP effects.
        public RESULT getDSP(int index, out DSP dsp)
        {
            dsp = null;

            IntPtr dspraw;
            RESULT result = FMOD_ChannelGroup_GetDSP(rawPtr, index, out dspraw);
            dsp = new DSP(dspraw);

            return result;
        }
        public RESULT addDSP(int index, DSP dsp)
        {
            return FMOD_ChannelGroup_AddDSP(rawPtr, index, dsp.getRaw());
        }
        public RESULT removeDSP(DSP dsp)
        {
            return FMOD_ChannelGroup_RemoveDSP(rawPtr, dsp.getRaw());
        }
        public RESULT getNumDSPs(out int numdsps)
        {
            return FMOD_ChannelGroup_GetNumDSPs(rawPtr, out numdsps);
        }
        public RESULT setDSPIndex(DSP dsp, int index)
        {
            return FMOD_ChannelGroup_SetDSPIndex(rawPtr, dsp.getRaw(), index);
        }
        public RESULT getDSPIndex(DSP dsp, out int index)
        {
            return FMOD_ChannelGroup_GetDSPIndex(rawPtr, dsp.getRaw(), out index);
        }
        public RESULT overridePanDSP(DSP pan)
        {
            return FMOD_ChannelGroup_OverridePanDSP(rawPtr, pan.getRaw());
        }

        // 3D functionality.
        public RESULT set3DAttributes(ref VECTOR pos, ref VECTOR vel, ref VECTOR alt_pan_pos)
        {
            return FMOD_ChannelGroup_Set3DAttributes(rawPtr, ref pos, ref vel, ref alt_pan_pos);
        }
        public RESULT get3DAttributes(out VECTOR pos, out VECTOR vel, out VECTOR alt_pan_pos)
        {
            return FMOD_ChannelGroup_Get3DAttributes(rawPtr, out pos, out vel, out alt_pan_pos);
        }
        public RESULT set3DMinMaxDistance(float mindistance, float maxdistance)
        {
            return FMOD_ChannelGroup_Set3DMinMaxDistance(rawPtr, mindistance, maxdistance);
        }
        public RESULT get3DMinMaxDistance(out float mindistance, out float maxdistance)
        {
            return FMOD_ChannelGroup_Get3DMinMaxDistance(rawPtr, out mindistance, out maxdistance);
        }
        public RESULT set3DConeSettings(float insideconeangle, float outsideconeangle, float outsidevolume)
        {
            return FMOD_ChannelGroup_Set3DConeSettings(rawPtr, insideconeangle, outsideconeangle, outsidevolume);
        }
        public RESULT get3DConeSettings(out float insideconeangle, out float outsideconeangle, out float outsidevolume)
        {
            return FMOD_ChannelGroup_Get3DConeSettings(rawPtr, out insideconeangle, out outsideconeangle, out outsidevolume);
        }
        public RESULT set3DConeOrientation(ref VECTOR orientation)
        {
            return FMOD_ChannelGroup_Set3DConeOrientation(rawPtr, ref orientation);
        }
        public RESULT get3DConeOrientation(out VECTOR orientation)
        {
            return FMOD_ChannelGroup_Get3DConeOrientation(rawPtr, out orientation);
        }
        public RESULT set3DCustomRolloff(ref VECTOR points, int numpoints)
        {
            return FMOD_ChannelGroup_Set3DCustomRolloff(rawPtr, ref points, numpoints);
        }
        public RESULT get3DCustomRolloff(out IntPtr points, out int numpoints)
        {
            return FMOD_ChannelGroup_Get3DCustomRolloff(rawPtr, out points, out numpoints);
        }
        public RESULT set3DOcclusion(float directocclusion, float reverbocclusion)
        {
            return FMOD_ChannelGroup_Set3DOcclusion(rawPtr, directocclusion, reverbocclusion);
        }
        public RESULT get3DOcclusion(out float directocclusion, out float reverbocclusion)
        {
            return FMOD_ChannelGroup_Get3DOcclusion(rawPtr, out directocclusion, out reverbocclusion);
        }
        public RESULT set3DSpread(float angle)
        {
            return FMOD_ChannelGroup_Set3DSpread(rawPtr, angle);
        }
        public RESULT get3DSpread(out float angle)
        {
            return FMOD_ChannelGroup_Get3DSpread(rawPtr, out angle);
        }
        public RESULT set3DLevel(float level)
        {
            return FMOD_ChannelGroup_Set3DLevel(rawPtr, level);
        }
        public RESULT get3DLevel(out float level)
        {
            return FMOD_ChannelGroup_Get3DLevel(rawPtr, out level);
        }
        public RESULT set3DDopplerLevel(float level)
        {
            return FMOD_ChannelGroup_Set3DDopplerLevel(rawPtr, level);
        }
        public RESULT get3DDopplerLevel(out float level)
        {
            return FMOD_ChannelGroup_Get3DDopplerLevel(rawPtr, out level);
        }
        public RESULT set3DDistanceFilter(bool custom, float customLevel, float centerFreq)
        {
            return FMOD_ChannelGroup_Set3DDistanceFilter(rawPtr, custom, customLevel, centerFreq);
        }
        public RESULT get3DDistanceFilter(out bool custom, out float customLevel, out float centerFreq)
        {
            return FMOD_ChannelGroup_Get3DDistanceFilter(rawPtr, out custom, out customLevel, out centerFreq);
        }

        // Userdata set/get.
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_ChannelGroup_SetUserData(rawPtr, userdata);
        }
        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD_ChannelGroup_GetUserData(rawPtr, out userdata);
        }

        #region importfunctions

        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Stop(IntPtr channelgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetPaused(IntPtr channelgroup, bool paused);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetPaused(IntPtr channelgroup, out bool paused);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetVolume(IntPtr channelgroup, out float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetVolumeRamp(IntPtr channelgroup, bool ramp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetVolumeRamp(IntPtr channelgroup, out bool ramp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetAudibility(IntPtr channelgroup, out float audibility);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetPitch(IntPtr channelgroup, float pitch);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetPitch(IntPtr channelgroup, out float pitch);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetMute(IntPtr channelgroup, bool mute);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetMute(IntPtr channelgroup, out bool mute);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetReverbProperties(IntPtr channelgroup, int instance, float wet);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetReverbProperties(IntPtr channelgroup, int instance, out float wet);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetLowPassGain(IntPtr channelgroup, float gain);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetLowPassGain(IntPtr channelgroup, out float gain);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetMode(IntPtr channelgroup, MODE mode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetMode(IntPtr channelgroup, out MODE mode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetCallback(IntPtr channelgroup, CHANNEL_CALLBACK callback);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_IsPlaying(IntPtr channelgroup, out bool isplaying);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetPan(IntPtr channelgroup, float pan);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetMixLevelsOutput(IntPtr channelgroup, float frontleft, float frontright, float center, float lfe, float surroundleft, float surroundright, float backleft, float backright);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetMixLevelsInput(IntPtr channelgroup, float[] levels, int numlevels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetMixMatrix(IntPtr channelgroup, float[] matrix, int outchannels, int inchannels, int inchannel_hop);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetMixMatrix(IntPtr channelgroup, float[] matrix, out int outchannels, out int inchannels, int inchannel_hop);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetDSPClock(IntPtr channelgroup, out ulong dspclock, out ulong parentclock);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetDelay(IntPtr channelgroup, ulong dspclock_start, ulong dspclock_end, bool stopchannels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetDelay(IntPtr channelgroup, out ulong dspclock_start, out ulong dspclock_end, out bool stopchannels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_AddFadePoint(IntPtr channelgroup, ulong dspclock, float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetFadePointRamp(IntPtr channelgroup, ulong dspclock, float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_RemoveFadePoints(IntPtr channelgroup, ulong dspclock_start, ulong dspclock_end);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetFadePoints(IntPtr channelgroup, ref uint numpoints, ulong[] point_dspclock, float[] point_volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Set3DAttributes(IntPtr channelgroup, ref VECTOR pos, ref VECTOR vel, ref VECTOR alt_pan_pos);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Get3DAttributes(IntPtr channelgroup, out VECTOR pos, out VECTOR vel, out VECTOR alt_pan_pos);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Set3DMinMaxDistance(IntPtr channelgroup, float mindistance, float maxdistance);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Get3DMinMaxDistance(IntPtr channelgroup, out float mindistance, out float maxdistance);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Set3DConeSettings(IntPtr channelgroup, float insideconeangle, float outsideconeangle, float outsidevolume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Get3DConeSettings(IntPtr channelgroup, out float insideconeangle, out float outsideconeangle, out float outsidevolume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Set3DConeOrientation(IntPtr channelgroup, ref VECTOR orientation);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Get3DConeOrientation(IntPtr channelgroup, out VECTOR orientation);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Set3DCustomRolloff(IntPtr channelgroup, ref VECTOR points, int numpoints);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Get3DCustomRolloff(IntPtr channelgroup, out IntPtr points, out int numpoints);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Set3DOcclusion(IntPtr channelgroup, float directocclusion, float reverbocclusion);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Get3DOcclusion(IntPtr channelgroup, out float directocclusion, out float reverbocclusion);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Set3DSpread(IntPtr channelgroup, float angle);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Get3DSpread(IntPtr channelgroup, out float angle);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Set3DLevel(IntPtr channelgroup, float level);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Get3DLevel(IntPtr channelgroup, out float level);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Set3DDopplerLevel(IntPtr channelgroup, float level);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Get3DDopplerLevel(IntPtr channelgroup, out float level);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Set3DDistanceFilter(IntPtr channelgroup, bool custom, float customLevel, float centerFreq);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Get3DDistanceFilter(IntPtr channelgroup, out bool custom, out float customLevel, out float centerFreq);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetSystemObject(IntPtr channelgroup, out IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetVolume(IntPtr channelgroup, float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetDSP(IntPtr channelgroup, int index, out IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_AddDSP(IntPtr channelgroup, int index, IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_RemoveDSP(IntPtr channelgroup, IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetNumDSPs(IntPtr channelgroup, out int numdsps);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetDSPIndex(IntPtr channelgroup, IntPtr dsp, int index);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetDSPIndex(IntPtr channelgroup, IntPtr dsp, out int index);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_OverridePanDSP(IntPtr channelgroup, IntPtr pan);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetUserData(IntPtr channelgroup, IntPtr userdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetUserData(IntPtr channelgroup, out IntPtr userdata);

        #endregion

        #region wrapperinternal

        protected ChannelControl(IntPtr raw)
            : base(raw)
        {
        }

        #endregion
    }


    /*
        'Channel' API
    */
    public class Channel : ChannelControl
    {
        // Channel specific control functionality.
        public RESULT setFrequency          (float frequency)
        {
            return FMOD_Channel_SetFrequency(getRaw(), frequency);
        }
        public RESULT getFrequency          (out float frequency)
        {
            return FMOD_Channel_GetFrequency(getRaw(), out frequency);
        }
        public RESULT setPriority           (int priority)
        {
            return FMOD_Channel_SetPriority(getRaw(), priority);
        }
        public RESULT getPriority           (out int priority)
        {
            return FMOD_Channel_GetPriority(getRaw(), out priority);
        }
        public RESULT setPosition           (uint position, TIMEUNIT postype)
        {
            return FMOD_Channel_SetPosition(getRaw(), position, postype);
        }
        public RESULT getPosition           (out uint position, TIMEUNIT postype)
        {
            return FMOD_Channel_GetPosition(getRaw(), out position, postype);
        }
        public RESULT setChannelGroup       (ChannelGroup channelgroup)
        {
            return FMOD_Channel_SetChannelGroup(getRaw(), channelgroup.getRaw());
        }
        public RESULT getChannelGroup       (out ChannelGroup channelgroup)
        {
            channelgroup = null;

            IntPtr channelgroupraw;
            RESULT result = FMOD_Channel_GetChannelGroup(getRaw(), out channelgroupraw);
            channelgroup = new ChannelGroup(channelgroupraw);

            return result;
        }
        public RESULT setLoopCount(int loopcount)
        {
            return FMOD_Channel_SetLoopCount(getRaw(), loopcount);
        }
        public RESULT getLoopCount(out int loopcount)
        {
            return FMOD_Channel_GetLoopCount(getRaw(), out loopcount);
        }
        public RESULT setLoopPoints(uint loopstart, TIMEUNIT loopstarttype, uint loopend, TIMEUNIT loopendtype)
        {
            return FMOD_Channel_SetLoopPoints(getRaw(), loopstart, loopstarttype, loopend, loopendtype);
        }
        public RESULT getLoopPoints(out uint loopstart, TIMEUNIT loopstarttype, out uint loopend, TIMEUNIT loopendtype)
        {
            return FMOD_Channel_GetLoopPoints(getRaw(), out loopstart, loopstarttype, out loopend, loopendtype);
        }

        // Information only functions.
        public RESULT isVirtual             (out bool isvirtual)
        {
            return FMOD_Channel_IsVirtual(getRaw(), out isvirtual);
        }
        public RESULT getCurrentSound       (out Sound sound)
        {
            sound = null;

            IntPtr soundraw;
            RESULT result = FMOD_Channel_GetCurrentSound(getRaw(), out soundraw);
            sound = new Sound(soundraw);

            return result;
        }
        public RESULT getIndex              (out int index)
        {
            return FMOD_Channel_GetIndex(getRaw(), out index);
        }

        #region importfunctions

        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetFrequency          (IntPtr channel, float frequency);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetFrequency          (IntPtr channel, out float frequency);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetPriority           (IntPtr channel, int priority);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetPriority           (IntPtr channel, out int priority);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetChannelGroup       (IntPtr channel, IntPtr channelgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetChannelGroup       (IntPtr channel, out IntPtr channelgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_IsVirtual             (IntPtr channel, out bool isvirtual);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetCurrentSound       (IntPtr channel, out IntPtr sound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetIndex              (IntPtr channel, out int index);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetPosition           (IntPtr channel, uint position, TIMEUNIT postype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetPosition           (IntPtr channel, out uint position, TIMEUNIT postype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetMode               (IntPtr channel, MODE mode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetMode               (IntPtr channel, out MODE mode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetLoopCount          (IntPtr channel, int loopcount);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetLoopCount          (IntPtr channel, out int loopcount);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetLoopPoints         (IntPtr channel, uint  loopstart, TIMEUNIT loopstarttype, uint  loopend, TIMEUNIT loopendtype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetLoopPoints         (IntPtr channel, out uint loopstart, TIMEUNIT loopstarttype, out uint loopend, TIMEUNIT loopendtype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetUserData           (IntPtr channel, IntPtr userdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetUserData           (IntPtr channel, out IntPtr userdata);
        #endregion

        #region wrapperinternal

        public Channel(IntPtr raw)
            : base(raw)
        {
        }

        #endregion
    }


    /*
        'ChannelGroup' API
    */
    public class ChannelGroup : ChannelControl
    {
        public RESULT release                ()
        {
            RESULT result = FMOD_ChannelGroup_Release(getRaw());
            if (result == RESULT.OK)
            {
                rawPtr = IntPtr.Zero;
            }
            return result;
        }

        // Nested channel groups.
        public RESULT addGroup               (ChannelGroup group, bool propagatedspclock, out DSPConnection connection)
        {
			connection = null;
			
			IntPtr connectionRaw;
            RESULT result = FMOD_ChannelGroup_AddGroup(getRaw(), group.getRaw(), propagatedspclock, out connectionRaw);
			connection = new DSPConnection(connectionRaw);
			
			return result;
        }
        public RESULT getNumGroups           (out int numgroups)
        {
            return FMOD_ChannelGroup_GetNumGroups(getRaw(), out numgroups);
        }
        public RESULT getGroup               (int index, out ChannelGroup group)
        {
            group = null;

            IntPtr groupraw;
            RESULT result = FMOD_ChannelGroup_GetGroup(getRaw(), index, out groupraw);
            group = new ChannelGroup(groupraw);

            return result;
        }
        public RESULT getParentGroup         (out ChannelGroup group)
        {
            group = null;

            IntPtr groupraw;
            RESULT result = FMOD_ChannelGroup_GetParentGroup(getRaw(), out groupraw);
            group = new ChannelGroup(groupraw);

            return result;
        }

        // Information only functions.
        public RESULT getName                (StringBuilder name, int namelen)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(name.Capacity);

            RESULT result = FMOD_ChannelGroup_GetName(getRaw(), stringMem, namelen);

            StringMarshalHelper.NativeToBuilder(name, stringMem);
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT getNumChannels         (out int numchannels)
        {
            return FMOD_ChannelGroup_GetNumChannels(getRaw(), out numchannels);
        }
        public RESULT getChannel             (int index, out Channel channel)
        {
            channel = null;

            IntPtr channelraw;
            RESULT result = FMOD_ChannelGroup_GetChannel(getRaw(), index, out channelraw);
            channel = new Channel(channelraw);

            return result;
        }

        #region importfunctions
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Release          (IntPtr channelgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_AddGroup         (IntPtr channelgroup, IntPtr group, bool propagatedspclock, out IntPtr connection);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetNumGroups     (IntPtr channelgroup, out int numgroups);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetGroup         (IntPtr channelgroup, int index, out IntPtr group);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetParentGroup   (IntPtr channelgroup, out IntPtr group);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetName          (IntPtr channelgroup, IntPtr name, int namelen);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetNumChannels   (IntPtr channelgroup, out int numchannels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetChannel       (IntPtr channelgroup, int index, out IntPtr channel);
        #endregion

        #region wrapperinternal

        public ChannelGroup(IntPtr raw)
            : base(raw)
        {
        }

        #endregion
    }


    /*
        'SoundGroup' API
    */
    public class SoundGroup : HandleBase
    {
        public RESULT release                ()
        {
            RESULT result = FMOD_SoundGroup_Release(getRaw());
            if (result == RESULT.OK)
            {
                rawPtr = IntPtr.Zero;
            }
            return result;
        }

        public RESULT getSystemObject        (out System system)
        {
            system = null;

            IntPtr systemraw;
            RESULT result = FMOD_SoundGroup_GetSystemObject(rawPtr, out systemraw);
            system = new System(systemraw);

            return result;
        }

        // SoundGroup control functions.
        public RESULT setMaxAudible          (int maxaudible)
        {
            return FMOD_SoundGroup_SetMaxAudible(rawPtr, maxaudible);
        }
        public RESULT getMaxAudible          (out int maxaudible)
        {
            return FMOD_SoundGroup_GetMaxAudible(rawPtr, out maxaudible);
        }
        public RESULT setMaxAudibleBehavior  (SOUNDGROUP_BEHAVIOR behavior)
        {
            return FMOD_SoundGroup_SetMaxAudibleBehavior(rawPtr, behavior);
        }
        public RESULT getMaxAudibleBehavior  (out SOUNDGROUP_BEHAVIOR behavior)
        {
            return FMOD_SoundGroup_GetMaxAudibleBehavior(rawPtr, out behavior);
        }
        public RESULT setMuteFadeSpeed       (float speed)
        {
            return FMOD_SoundGroup_SetMuteFadeSpeed(rawPtr, speed);
        }
        public RESULT getMuteFadeSpeed       (out float speed)
        {
            return FMOD_SoundGroup_GetMuteFadeSpeed(rawPtr, out speed);
        }
        public RESULT setVolume       (float volume)
        {
            return FMOD_SoundGroup_SetVolume(rawPtr, volume);
        }
        public RESULT getVolume       (out float volume)
        {
            return FMOD_SoundGroup_GetVolume(rawPtr, out volume);
        }
        public RESULT stop       ()
        {
            return FMOD_SoundGroup_Stop(rawPtr);
        }

        // Information only functions.
        public RESULT getName                (StringBuilder name, int namelen)
        {
            IntPtr stringMem = Marshal.AllocHGlobal(name.Capacity);

            RESULT result = FMOD_SoundGroup_GetName(rawPtr, stringMem, namelen);

            StringMarshalHelper.NativeToBuilder(name, stringMem);
            Marshal.FreeHGlobal(stringMem);

            return result;
        }
        public RESULT getNumSounds           (out int numsounds)
        {
            return FMOD_SoundGroup_GetNumSounds(rawPtr, out numsounds);
        }
        public RESULT getSound               (int index, out Sound sound)
        {
            sound = null;

            IntPtr soundraw;
            RESULT result = FMOD_SoundGroup_GetSound(rawPtr, index, out soundraw);
            sound = new Sound(soundraw);

            return result;
        }
        public RESULT getNumPlaying          (out int numplaying)
        {
            return FMOD_SoundGroup_GetNumPlaying(rawPtr, out numplaying);
        }

        // Userdata set/get.
        public RESULT setUserData            (IntPtr userdata)
        {
            return FMOD_SoundGroup_SetUserData(rawPtr, userdata);
        }
        public RESULT getUserData            (out IntPtr userdata)
        {
            return FMOD_SoundGroup_GetUserData(rawPtr, out userdata);
        }

        #region importfunctions
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_Release            (IntPtr soundgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_GetSystemObject    (IntPtr soundgroup, out IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_SetMaxAudible      (IntPtr soundgroup, int maxaudible);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_GetMaxAudible      (IntPtr soundgroup, out int maxaudible);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_SetMaxAudibleBehavior(IntPtr soundgroup, SOUNDGROUP_BEHAVIOR behavior);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_GetMaxAudibleBehavior(IntPtr soundgroup, out SOUNDGROUP_BEHAVIOR behavior);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_SetMuteFadeSpeed   (IntPtr soundgroup, float speed);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_GetMuteFadeSpeed   (IntPtr soundgroup, out float speed);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_SetVolume          (IntPtr soundgroup, float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_GetVolume          (IntPtr soundgroup, out float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_Stop               (IntPtr soundgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_GetName            (IntPtr soundgroup, IntPtr name, int namelen);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_GetNumSounds       (IntPtr soundgroup, out int numsounds);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_GetSound           (IntPtr soundgroup, int index, out IntPtr sound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_GetNumPlaying      (IntPtr soundgroup, out int numplaying);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_SetUserData        (IntPtr soundgroup, IntPtr userdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_SoundGroup_GetUserData        (IntPtr soundgroup, out IntPtr userdata);
        #endregion

        #region wrapperinternal

        public SoundGroup(IntPtr raw)
            : base(raw)
        {
        }

        #endregion
    }


    /*
        'DSP' API
    */
    public class DSP : HandleBase
    {
        public RESULT release                   ()
        {
            RESULT result = FMOD_DSP_Release(getRaw());
            if (result == RESULT.OK)
            {
                rawPtr = IntPtr.Zero;
            }
            return result;
        }
        public RESULT getSystemObject           (out System system)
        {
            system = null;

            IntPtr systemraw;
            RESULT result = FMOD_DSP_GetSystemObject(rawPtr, out systemraw);
            system = new System(systemraw);

            return result;
        }

        // Connection / disconnection / input and output enumeration.
        public RESULT addInput(DSP target, out DSPConnection connection, DSPCONNECTION_TYPE type)
        {
            connection = null;

            IntPtr dspconnectionraw;
            RESULT result = FMOD_DSP_AddInput(rawPtr, target.getRaw(), out dspconnectionraw, type);
            connection = new DSPConnection(dspconnectionraw);

            return result;
        }
        public RESULT disconnectFrom            (DSP target, DSPConnection connection)
        {
            return FMOD_DSP_DisconnectFrom(rawPtr, target.getRaw(), connection.getRaw());
        }
        public RESULT disconnectAll             (bool inputs, bool outputs)
        {
            return FMOD_DSP_DisconnectAll(rawPtr, inputs, outputs);
        }
        public RESULT getNumInputs              (out int numinputs)
        {
            return FMOD_DSP_GetNumInputs(rawPtr, out numinputs);
        }
        public RESULT getNumOutputs             (out int numoutputs)
        {
            return FMOD_DSP_GetNumOutputs(rawPtr, out numoutputs);
        }
        public RESULT getInput                  (int index, out DSP input, out DSPConnection inputconnection)
        {
            input = null;
            inputconnection = null;

            IntPtr dspinputraw;
            IntPtr dspconnectionraw;
            RESULT result = FMOD_DSP_GetInput(rawPtr, index, out dspinputraw, out dspconnectionraw);
            input = new DSP(dspinputraw);
            inputconnection = new DSPConnection(dspconnectionraw);

            return result;
        }
        public RESULT getOutput                 (int index, out DSP output, out DSPConnection outputconnection)
        {
            output = null;
            outputconnection = null;

            IntPtr dspoutputraw;
            IntPtr dspconnectionraw;
            RESULT result = FMOD_DSP_GetOutput(rawPtr, index, out dspoutputraw, out dspconnectionraw);
            output = new DSP(dspoutputraw);
            outputconnection = new DSPConnection(dspconnectionraw);

            return result;
        }

        // DSP unit control.
        public RESULT setActive                 (bool active)
        {
            return FMOD_DSP_SetActive(rawPtr, active);
        }
        public RESULT getActive                 (out bool active)
        {
            return FMOD_DSP_GetActive(rawPtr, out active);
        }
        public RESULT setBypass(bool bypass)
        {
            return FMOD_DSP_SetBypass(rawPtr, bypass);
        }
        public RESULT getBypass(out bool bypass)
        {
            return FMOD_DSP_GetBypass(rawPtr, out bypass);
        }
        public RESULT setWetDryMix(float prewet, float postwet, float dry)
        {
            return FMOD_DSP_SetWetDryMix(rawPtr, prewet, postwet, dry);
        }
        public RESULT getWetDryMix(out float prewet, out float postwet, out float dry)
        {
            return FMOD_DSP_GetWetDryMix(rawPtr, out prewet, out postwet, out dry);
        }
        public RESULT setChannelFormat(CHANNELMASK channelmask, int numchannels, SPEAKERMODE source_speakermode)
        {
            return FMOD_DSP_SetChannelFormat(rawPtr, channelmask, numchannels, source_speakermode);
        }
        public RESULT getChannelFormat(out CHANNELMASK channelmask, out int numchannels, out SPEAKERMODE source_speakermode)
        {
            return FMOD_DSP_GetChannelFormat(rawPtr, out channelmask, out numchannels, out source_speakermode);
        }
        public RESULT getOutputChannelFormat(CHANNELMASK inmask, int inchannels, SPEAKERMODE inspeakermode, out CHANNELMASK outmask, out int outchannels, out SPEAKERMODE outspeakermode)
        {
            return FMOD_DSP_GetOutputChannelFormat(rawPtr, inmask, inchannels, inspeakermode, out outmask, out outchannels, out outspeakermode);
        }
        public RESULT reset                     ()
        {
            return FMOD_DSP_Reset(rawPtr);
        }

        // DSP parameter control.
        public RESULT setParameterFloat(int index, float value)
        {
            return FMOD_DSP_SetParameterFloat(rawPtr, index, value);
        }
        public RESULT setParameterInt(int index, int value)
        {
            return FMOD_DSP_SetParameterInt(rawPtr, index, value);
        }
        public RESULT setParameterBool(int index, bool value)
        {
            return FMOD_DSP_SetParameterBool(rawPtr, index, value);
        }
        public RESULT setParameterData(int index, byte[] data)
        {
            return FMOD_DSP_SetParameterData(rawPtr, index, Marshal.UnsafeAddrOfPinnedArrayElement(data, 0), (uint)data.Length);
        }
        public RESULT getParameterFloat(int index, out float value)
        {
            IntPtr valuestr = IntPtr.Zero;
            return FMOD_DSP_GetParameterFloat(rawPtr, index, out value, valuestr, 0);
        }
        public RESULT getParameterInt(int index, out int value)
        {
            IntPtr valuestr = IntPtr.Zero;
            return FMOD_DSP_GetParameterInt(rawPtr, index, out value, valuestr, 0);
        }
        public RESULT getParameterBool(int index, out bool value)
        {
            return FMOD_DSP_GetParameterBool(rawPtr, index, out value, IntPtr.Zero, 0);
        }
        public RESULT getParameterData(int index, out IntPtr data, out uint length)
        {
            return FMOD_DSP_GetParameterData(rawPtr, index, out data, out length, IntPtr.Zero, 0);
        }
        public RESULT getNumParameters          (out int numparams)
        {
            return FMOD_DSP_GetNumParameters(rawPtr, out numparams);
        }
        public RESULT getParameterInfo          (int index, out DSP_PARAMETER_DESC desc)
        {
            IntPtr descPtr;
            RESULT result = FMOD_DSP_GetParameterInfo(rawPtr, index, out descPtr);
            if (result == RESULT.OK)
            {
                desc = (DSP_PARAMETER_DESC)Marshal.PtrToStructure(descPtr, typeof(DSP_PARAMETER_DESC));
            }
            else
            {
                desc = new DSP_PARAMETER_DESC();
            }
            return result;
        }
        public RESULT getDataParameterIndex(int datatype, out int index)
        {
            return FMOD_DSP_GetDataParameterIndex     (rawPtr, datatype, out index);
        }
        public RESULT showConfigDialog          (IntPtr hwnd, bool show)
        {
            return FMOD_DSP_ShowConfigDialog          (rawPtr, hwnd, show);
        }

        //  DSP attributes.
        public RESULT getInfo                   (StringBuilder name, out uint version, out int channels, out int configwidth, out int configheight)
        {
            IntPtr nameMem = Marshal.AllocHGlobal(32);
            RESULT result = FMOD_DSP_GetInfo(rawPtr, nameMem, out version, out channels, out configwidth, out configheight);
            StringMarshalHelper.NativeToBuilder(name, nameMem);
            Marshal.FreeHGlobal(nameMem);
            return result;
        }
        public RESULT getType                   (out DSP_TYPE type)
        {
            return FMOD_DSP_GetType(rawPtr, out type);
        }
        public RESULT getIdle                   (out bool idle)
        {
            return FMOD_DSP_GetIdle(rawPtr, out idle);
        }

        // Userdata set/get.
        public RESULT setUserData               (IntPtr userdata)
        {
            return FMOD_DSP_SetUserData(rawPtr, userdata);
        }
        public RESULT getUserData               (out IntPtr userdata)
        {
            return FMOD_DSP_GetUserData(rawPtr, out userdata);
        }

        // Metering.
        public RESULT setMeteringEnabled(bool inputEnabled, bool outputEnabled)
        {
            return FMOD_DSP_SetMeteringEnabled(rawPtr, inputEnabled, outputEnabled);
        }
        public RESULT getMeteringEnabled(out bool inputEnabled, out bool outputEnabled)
        {
            return FMOD_DSP_GetMeteringEnabled(rawPtr, out inputEnabled, out outputEnabled);
        }

        public RESULT getMeteringInfo(DSP_METERING_INFO inputInfo, DSP_METERING_INFO outputInfo)
        {
            return FMOD_DSP_GetMeteringInfo(rawPtr, inputInfo, outputInfo);
        }

        #region importfunctions

        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_Release                   (IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetSystemObject           (IntPtr dsp, out IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_AddInput                  (IntPtr dsp, IntPtr target, out IntPtr connection, DSPCONNECTION_TYPE type);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_DisconnectFrom            (IntPtr dsp, IntPtr target, IntPtr connection);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_DisconnectAll             (IntPtr dsp, bool inputs, bool outputs);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetNumInputs              (IntPtr dsp, out int numinputs);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetNumOutputs             (IntPtr dsp, out int numoutputs);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetInput                  (IntPtr dsp, int index, out IntPtr input, out IntPtr inputconnection);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetOutput                 (IntPtr dsp, int index, out IntPtr output, out IntPtr outputconnection);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetActive                 (IntPtr dsp, bool active);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetActive                 (IntPtr dsp, out bool active);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetBypass                 (IntPtr dsp, bool bypass);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetBypass                 (IntPtr dsp, out bool bypass);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetWetDryMix              (IntPtr dsp, float prewet, float postwet, float dry);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetWetDryMix              (IntPtr dsp, out float prewet, out float postwet, out float dry);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetChannelFormat          (IntPtr dsp, CHANNELMASK channelmask, int numchannels, SPEAKERMODE source_speakermode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetChannelFormat          (IntPtr dsp, out CHANNELMASK channelmask, out int numchannels, out SPEAKERMODE source_speakermode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetOutputChannelFormat    (IntPtr dsp, CHANNELMASK inmask, int inchannels, SPEAKERMODE inspeakermode, out CHANNELMASK outmask, out int outchannels, out SPEAKERMODE outspeakermode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_Reset                     (IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetParameterFloat         (IntPtr dsp, int index, float value);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetParameterInt           (IntPtr dsp, int index, int value);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetParameterBool          (IntPtr dsp, int index, bool value);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetParameterData          (IntPtr dsp, int index, IntPtr data, uint length);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetParameterFloat         (IntPtr dsp, int index, out float value, IntPtr valuestr, int valuestrlen);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetParameterInt           (IntPtr dsp, int index, out int value, IntPtr valuestr, int valuestrlen);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetParameterBool          (IntPtr dsp, int index, out bool value, IntPtr valuestr, int valuestrlen);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetParameterData          (IntPtr dsp, int index, out IntPtr data, out uint length, IntPtr valuestr, int valuestrlen);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetNumParameters          (IntPtr dsp, out int numparams);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetParameterInfo          (IntPtr dsp, int index, out IntPtr desc);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetDataParameterIndex     (IntPtr dsp, int datatype, out int index);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_ShowConfigDialog          (IntPtr dsp, IntPtr hwnd, bool show);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetInfo                   (IntPtr dsp, IntPtr name, out uint version, out int channels, out int configwidth, out int configheight);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetType                   (IntPtr dsp, out DSP_TYPE type);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetIdle                   (IntPtr dsp, out bool idle);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetUserData               (IntPtr dsp, IntPtr userdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetUserData               (IntPtr dsp, out IntPtr userdata);
        [DllImport(VERSION.dll)]
        public static extern RESULT FMOD_DSP_SetMeteringEnabled         (IntPtr dsp, bool inputEnabled, bool outputEnabled);
        [DllImport(VERSION.dll)]
        public static extern RESULT FMOD_DSP_GetMeteringEnabled         (IntPtr dsp, out bool inputEnabled, out bool outputEnabled);
        [DllImport(VERSION.dll)]
        public static extern RESULT FMOD_DSP_GetMeteringInfo            (IntPtr dsp, [Out] DSP_METERING_INFO inputInfo, [Out] DSP_METERING_INFO outputInfo);
        #endregion

        #region wrapperinternal

        public DSP(IntPtr raw)
            : base(raw)
        {
        }

        #endregion
    }


    /*
        'DSPConnection' API
    */
    public class DSPConnection : HandleBase
    {
        public RESULT getInput              (out DSP input)
        {
            input = null;

            IntPtr dspraw;
            RESULT result = FMOD_DSPConnection_GetInput(rawPtr, out dspraw);
            input = new DSP(dspraw);

            return result;
        }
        public RESULT getOutput             (out DSP output)
        {
            output = null;

            IntPtr dspraw;
            RESULT result = FMOD_DSPConnection_GetOutput(rawPtr, out dspraw);
            output = new DSP(dspraw);

            return result;
        }
        public RESULT setMix                (float volume)
        {
            return FMOD_DSPConnection_SetMix(rawPtr, volume);
        }
        public RESULT getMix                (out float volume)
        {
            return FMOD_DSPConnection_GetMix(rawPtr, out volume);
        }
        public RESULT setMixMatrix(float[] matrix, int outchannels, int inchannels, int inchannel_hop)
        {
            return FMOD_DSPConnection_SetMixMatrix(rawPtr, matrix, outchannels, inchannels, inchannel_hop);
        }
        public RESULT getMixMatrix(float[] matrix, out int outchannels, out int inchannels, int inchannel_hop)
        {
            return FMOD_DSPConnection_GetMixMatrix(rawPtr, matrix, out outchannels, out inchannels, inchannel_hop);
        }
        public RESULT getType(out DSPCONNECTION_TYPE type)
        {
            return FMOD_DSPConnection_GetType(rawPtr, out type);
        }

        // Userdata set/get.
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_DSPConnection_SetUserData(rawPtr, userdata);
        }
        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD_DSPConnection_GetUserData(rawPtr, out userdata);
        }

        #region importfunctions
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSPConnection_GetInput        (IntPtr dspconnection, out IntPtr input);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSPConnection_GetOutput       (IntPtr dspconnection, out IntPtr output);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSPConnection_SetMix          (IntPtr dspconnection, float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSPConnection_GetMix          (IntPtr dspconnection, out float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSPConnection_SetMixMatrix    (IntPtr dspconnection, float[] matrix, int outchannels, int inchannels, int inchannel_hop);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSPConnection_GetMixMatrix    (IntPtr dspconnection, float[] matrix, out int outchannels, out int inchannels, int inchannel_hop);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSPConnection_GetType         (IntPtr dspconnection, out DSPCONNECTION_TYPE type);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSPConnection_SetUserData     (IntPtr dspconnection, IntPtr userdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSPConnection_GetUserData     (IntPtr dspconnection, out IntPtr userdata);
        #endregion

        #region wrapperinternal

        public DSPConnection(IntPtr raw)
            : base(raw)
        {
        }

        #endregion
    }

    /*
        'Geometry' API
    */
    public class Geometry : HandleBase
    {
        public RESULT release               ()
        {
            RESULT result = FMOD_Geometry_Release(getRaw());
            if (result == RESULT.OK)
            {
                rawPtr = IntPtr.Zero;
            }
            return result;
        }

        // Polygon manipulation.
        public RESULT addPolygon            (float directocclusion, float reverbocclusion, bool doublesided, int numvertices, VECTOR[] vertices, out int polygonindex)
        {
            return FMOD_Geometry_AddPolygon(rawPtr, directocclusion, reverbocclusion, doublesided, numvertices, vertices, out polygonindex);
        }
        public RESULT getNumPolygons        (out int numpolygons)
        {
            return FMOD_Geometry_GetNumPolygons(rawPtr, out numpolygons);
        }
        public RESULT getMaxPolygons        (out int maxpolygons, out int maxvertices)
        {
            return FMOD_Geometry_GetMaxPolygons(rawPtr, out maxpolygons, out maxvertices);
        }
        public RESULT getPolygonNumVertices (int index, out int numvertices)
        {
            return FMOD_Geometry_GetPolygonNumVertices(rawPtr, index, out numvertices);
        }
        public RESULT setPolygonVertex      (int index, int vertexindex, ref VECTOR vertex)
        {
            return FMOD_Geometry_SetPolygonVertex(rawPtr, index, vertexindex, ref vertex);
        }
        public RESULT getPolygonVertex      (int index, int vertexindex, out VECTOR vertex)
        {
            return FMOD_Geometry_GetPolygonVertex(rawPtr, index, vertexindex, out vertex);
        }
        public RESULT setPolygonAttributes  (int index, float directocclusion, float reverbocclusion, bool doublesided)
        {
            return FMOD_Geometry_SetPolygonAttributes(rawPtr, index, directocclusion, reverbocclusion, doublesided);
        }
        public RESULT getPolygonAttributes  (int index, out float directocclusion, out float reverbocclusion, out bool doublesided)
        {
            return FMOD_Geometry_GetPolygonAttributes(rawPtr, index, out directocclusion, out reverbocclusion, out doublesided);
        }

        // Object manipulation.
        public RESULT setActive             (bool active)
        {
            return FMOD_Geometry_SetActive(rawPtr, active);
        }
        public RESULT getActive             (out bool active)
        {
            return FMOD_Geometry_GetActive(rawPtr, out active);
        }
        public RESULT setRotation           (ref VECTOR forward, ref VECTOR up)
        {
            return FMOD_Geometry_SetRotation(rawPtr, ref forward, ref up);
        }
        public RESULT getRotation           (out VECTOR forward, out VECTOR up)
        {
            return FMOD_Geometry_GetRotation(rawPtr, out forward, out up);
        }
        public RESULT setPosition           (ref VECTOR position)
        {
            return FMOD_Geometry_SetPosition(rawPtr, ref position);
        }
        public RESULT getPosition           (out VECTOR position)
        {
            return FMOD_Geometry_GetPosition(rawPtr, out position);
        }
        public RESULT setScale              (ref VECTOR scale)
        {
            return FMOD_Geometry_SetScale(rawPtr, ref scale);
        }
        public RESULT getScale              (out VECTOR scale)
        {
            return FMOD_Geometry_GetScale(rawPtr, out scale);
        }
        public RESULT save                  (IntPtr data, out int datasize)
        {
            return FMOD_Geometry_Save(rawPtr, data, out datasize);
        }

        // Userdata set/get.
        public RESULT setUserData               (IntPtr userdata)
        {
            return FMOD_Geometry_SetUserData(rawPtr, userdata);
        }
        public RESULT getUserData               (out IntPtr userdata)
        {
            return FMOD_Geometry_GetUserData(rawPtr, out userdata);
        }

        #region importfunctions
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_Release              (IntPtr geometry);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_AddPolygon           (IntPtr geometry, float directocclusion, float reverbocclusion, bool doublesided, int numvertices, VECTOR[] vertices, out int polygonindex);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetNumPolygons       (IntPtr geometry, out int numpolygons);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetMaxPolygons       (IntPtr geometry, out int maxpolygons, out int maxvertices);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetPolygonNumVertices(IntPtr geometry, int index, out int numvertices);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_SetPolygonVertex     (IntPtr geometry, int index, int vertexindex, ref VECTOR vertex);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetPolygonVertex     (IntPtr geometry, int index, int vertexindex, out VECTOR vertex);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_SetPolygonAttributes (IntPtr geometry, int index, float directocclusion, float reverbocclusion, bool doublesided);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetPolygonAttributes (IntPtr geometry, int index, out float directocclusion, out float reverbocclusion, out bool doublesided);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_SetActive            (IntPtr geometry, bool active);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetActive            (IntPtr geometry, out bool active);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_SetRotation          (IntPtr geometry, ref VECTOR forward, ref VECTOR up);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetRotation          (IntPtr geometry, out VECTOR forward, out VECTOR up);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_SetPosition          (IntPtr geometry, ref VECTOR position);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetPosition          (IntPtr geometry, out VECTOR position);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_SetScale             (IntPtr geometry, ref VECTOR scale);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetScale             (IntPtr geometry, out VECTOR scale);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_Save                 (IntPtr geometry, IntPtr data, out int datasize);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_SetUserData          (IntPtr geometry, IntPtr userdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetUserData          (IntPtr geometry, out IntPtr userdata);
        #endregion

        #region wrapperinternal

        public Geometry(IntPtr raw)
            : base(raw)
        {
        }

        #endregion
    }


    /*
        'Reverb3D' API
    */
    public class Reverb3D : HandleBase
    {
        public RESULT release()
        {
            RESULT result = FMOD_Reverb3D_Release(getRaw());
            if (result == RESULT.OK)
            {
                rawPtr = IntPtr.Zero;
            }
            return result;
        }

        // Reverb manipulation.
        public RESULT set3DAttributes(ref VECTOR position, float mindistance, float maxdistance)
        {
            return FMOD_Reverb3D_Set3DAttributes(rawPtr, ref position, mindistance, maxdistance);
        }
        public RESULT get3DAttributes(ref VECTOR position, ref float mindistance, ref float maxdistance)
        {
            return FMOD_Reverb3D_Get3DAttributes(rawPtr, ref position, ref mindistance, ref maxdistance);
        }
        public RESULT setProperties(ref REVERB_PROPERTIES properties)
        {
            return FMOD_Reverb3D_SetProperties(rawPtr, ref properties);
        }
        public RESULT getProperties(ref REVERB_PROPERTIES properties)
        {
            return FMOD_Reverb3D_GetProperties(rawPtr, ref properties);
        }
        public RESULT setActive(bool active)
        {
            return FMOD_Reverb3D_SetActive(rawPtr, active);
        }
        public RESULT getActive(out bool active)
        {
            return FMOD_Reverb3D_GetActive(rawPtr, out active);
        }

        // Userdata set/get.
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_Reverb3D_SetUserData(rawPtr, userdata);
        }
        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD_Reverb3D_GetUserData(rawPtr, out userdata);
        }

        #region importfunctions
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Reverb3D_Release(IntPtr reverb);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Reverb3D_Set3DAttributes(IntPtr reverb, ref VECTOR position, float mindistance, float maxdistance);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Reverb3D_Get3DAttributes(IntPtr reverb, ref VECTOR position, ref float mindistance, ref float maxdistance);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Reverb3D_SetProperties(IntPtr reverb, ref REVERB_PROPERTIES properties);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Reverb3D_GetProperties(IntPtr reverb, ref REVERB_PROPERTIES properties);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Reverb3D_SetActive(IntPtr reverb, bool active);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Reverb3D_GetActive(IntPtr reverb, out bool active);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Reverb3D_SetUserData(IntPtr reverb, IntPtr userdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Reverb3D_GetUserData(IntPtr reverb, out IntPtr userdata);
        #endregion

        #region wrapperinternal

        public Reverb3D(IntPtr raw)
            : base(raw)
        {
        }

        #endregion
    }

    class StringMarshalHelper
    {
        static internal void NativeToBuilder(StringBuilder builder, IntPtr nativeMem)
        {
            byte[] bytes = new byte[builder.Capacity];
            Marshal.Copy(nativeMem, bytes, 0, builder.Capacity);
			int strlen = Array.IndexOf(bytes, (byte)0);
			if (strlen > 0)
			{
				String str = Encoding.UTF8.GetString(bytes, 0, strlen);
				builder.Append(str);
			}
        }
    }
}
