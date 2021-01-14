using FMOD;
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
            var m_AudioData = m_AudioClip.m_AudioData.GetData();
            if (m_AudioData == null || m_AudioData.Length == 0)
                return null;
            var exinfo = new CREATESOUNDEXINFO();
            var result = Factory.System_Create(out var system);
            if (result != RESULT.OK)
                return null;
            result = system.init(1, INITFLAGS.NORMAL, IntPtr.Zero);
            if (result != RESULT.OK)
                return null;
            exinfo.cbsize = Marshal.SizeOf(exinfo);
            exinfo.length = (uint)m_AudioClip.m_Size;
            result = system.createSound(m_AudioData, MODE.OPENMEMORY, ref exinfo, out var sound);
            if (result != RESULT.OK)
                return null;
            result = sound.getNumSubSounds(out var numsubsounds);
            if (result != RESULT.OK)
                return null;
            byte[] buff;
            if (numsubsounds > 0)
            {
                result = sound.getSubSound(0, out var subsound);
                if (result != RESULT.OK)
                    return null;
                buff = SoundToWav(subsound);
                subsound.release();
            }
            else
            {
                buff = SoundToWav(sound);
            }
            sound.release();
            system.release();
            return buff;
        }

        public byte[] SoundToWav(Sound sound)
        {
            var result = sound.getFormat(out _, out _, out int channels, out int bits);
            if (result != RESULT.OK)
                return null;
            result = sound.getDefaults(out var frequency, out _);
            if (result != RESULT.OK)
                return null;
            var sampleRate = (int)frequency;
            result = sound.getLength(out var length, TIMEUNIT.PCMBYTES);
            if (result != RESULT.OK)
                return null;
            result = sound.@lock(0, length, out var ptr1, out var ptr2, out var len1, out var len2);
            if (result != RESULT.OK)
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
            result = sound.unlock(ptr1, ptr2, len1, len2);
            if (result != RESULT.OK)
                return null;
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
                        return ".fsb";
                    case AudioCompressionFormat.HEVAG:
                        return ".fsb";
                    case AudioCompressionFormat.XMA:
                        return ".fsb";
                    case AudioCompressionFormat.AAC:
                        return ".m4a";
                    case AudioCompressionFormat.GCADPCM:
                        return ".fsb";
                    case AudioCompressionFormat.ATRAC9:
                        return ".fsb";
                }
            }

            return ".AudioClip";
        }

        public bool IsSupport
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
                        case AudioCompressionFormat.XMA:
                            return true;
                        default:
                            return false;
                    }
                }
            }
        }
    }
}
