using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AssetStudio
{
    public class AudioClipConverter
    {
        private AudioClip m_AudioClip;

        public AudioClipConverter(AudioClip audioClip)
        {
            m_AudioClip = audioClip;
        }

        public byte[] ConvertToWav()
        {
            var m_AudioData = m_AudioClip.m_AudioData.Value;
            if (m_AudioData == null || m_AudioData.Length == 0)
                return null;
            var exinfo = new FMOD.CREATESOUNDEXINFO();
            var result = FMOD.Factory.System_Create(out var system);
            if (result != FMOD.RESULT.OK)
                return null;
            result = system.init(1, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);
            if (result != FMOD.RESULT.OK)
                return null;
            exinfo.cbsize = Marshal.SizeOf(exinfo);
            exinfo.length = (uint)m_AudioClip.m_Size;
            result = system.createSound(m_AudioData, FMOD.MODE.OPENMEMORY, ref exinfo, out var sound);
            if (result != FMOD.RESULT.OK)
                return null;
            result = sound.getSubSound(0, out var subsound);
            if (result != FMOD.RESULT.OK)
                return null;
            result = subsound.getFormat(out var type, out var format, out int channels, out int bits);
            if (result != FMOD.RESULT.OK)
                return null;
            result = subsound.getDefaults(out var frequency, out int priority);
            if (result != FMOD.RESULT.OK)
                return null;
            var sampleRate = (int)frequency;
            result = subsound.getLength(out var length, FMOD.TIMEUNIT.PCMBYTES);
            if (result != FMOD.RESULT.OK)
                return null;
            result = subsound.@lock(0, length, out var ptr1, out var ptr2, out var len1, out var len2);
            if (result != FMOD.RESULT.OK)
                return null;
            byte[] buffer = new byte[len1 + 44];
            //添加wav头
            Encoding.UTF8.GetBytes("RIFF").CopyTo(buffer, 0);
            BitConverter.GetBytes(len1 + 36).CopyTo(buffer, 4);
            Encoding.UTF8.GetBytes("WAVEfmt ").CopyTo(buffer, 8);
            BitConverter.GetBytes(16).CopyTo(buffer, 16);
            BitConverter.GetBytes((short)1).CopyTo(buffer, 20);
            BitConverter.GetBytes((short)channels).CopyTo(buffer, 22);
            BitConverter.GetBytes(sampleRate).CopyTo(buffer, 24);
            BitConverter.GetBytes(sampleRate * channels * bits / 8).CopyTo(buffer, 28);
            BitConverter.GetBytes((short)(channels * bits / 8)).CopyTo(buffer, 32);
            BitConverter.GetBytes((short)bits).CopyTo(buffer, 34);
            Encoding.UTF8.GetBytes("data").CopyTo(buffer, 36);
            BitConverter.GetBytes(len1).CopyTo(buffer, 40);
            Marshal.Copy(ptr1, buffer, 44, (int)len1);
            result = subsound.unlock(ptr1, ptr2, len1, len2);
            if (result != FMOD.RESULT.OK)
                return null;
            subsound.release();
            sound.release();
            system.release();
            return buffer;
        }

        public string GetExtensionName()
        {
            if (m_AudioClip.version[0] < 5)
            {
                switch (m_AudioClip.m_Type)
                {
                    case AudioType.ACC:
                        return ".m4a";
                    case AudioType.AIFF:
                        return ".aif";
                    case AudioType.IT:
                        return ".it";
                    case AudioType.MOD:
                        return ".mod";
                    case AudioType.MPEG:
                        return ".mp3";
                    case AudioType.OGGVORBIS:
                        return ".ogg";
                    case AudioType.S3M:
                        return ".s3m";
                    case AudioType.WAV:
                        return ".wav";
                    case AudioType.XM:
                        return ".xm";
                    case AudioType.XMA:
                        return ".wav";
                    case AudioType.VAG:
                        return ".vag";
                    case AudioType.AUDIOQUEUE:
                        return ".fsb";
                }

            }
            else
            {
                switch (m_AudioClip.m_CompressionFormat)
                {
                    case AudioCompressionFormat.PCM:
                        return ".fsb";
                    case AudioCompressionFormat.Vorbis:
                        return ".fsb";
                    case AudioCompressionFormat.ADPCM:
                        return ".fsb";
                    case AudioCompressionFormat.MP3:
                        return ".fsb";
                    case AudioCompressionFormat.VAG:
                        return ".vag";
                    case AudioCompressionFormat.HEVAG:
                        return ".vag";
                    case AudioCompressionFormat.XMA:
                        return ".wav";
                    case AudioCompressionFormat.AAC:
                        return ".m4a";
                    case AudioCompressionFormat.GCADPCM:
                        return ".fsb";
                    case AudioCompressionFormat.ATRAC9:
                        return ".at9";
                }
            }

            return ".AudioClip";
        }

        public bool IsFMODSupport
        {
            get
            {
                if (m_AudioClip.version[0] < 5)
                {
                    switch (m_AudioClip.m_Type)
                    {
                        case AudioType.AIFF:
                        case AudioType.IT:
                        case AudioType.MOD:
                        case AudioType.S3M:
                        case AudioType.XM:
                        case AudioType.XMA:
                        case AudioType.VAG:
                        case AudioType.AUDIOQUEUE:
                            return true;
                        default:
                            return false;
                    }
                }
                else
                {
                    switch (m_AudioClip.m_CompressionFormat)
                    {
                        case AudioCompressionFormat.PCM:
                        case AudioCompressionFormat.Vorbis:
                        case AudioCompressionFormat.ADPCM:
                        case AudioCompressionFormat.MP3:
                        case AudioCompressionFormat.VAG:
                        case AudioCompressionFormat.HEVAG:
                        case AudioCompressionFormat.XMA:
                        case AudioCompressionFormat.GCADPCM:
                        case AudioCompressionFormat.ATRAC9:
                            return true;
                        default:
                            return false;
                    }
                }
            }
        }
    }
}
