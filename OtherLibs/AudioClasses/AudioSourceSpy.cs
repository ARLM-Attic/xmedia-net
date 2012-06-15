using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioClasses
{
    public delegate void DelegatePullSample(MediaSample sample);

    /// <summary>
    /// Allows a host to spy on an audio source
    /// </summary>
    public class AudioSourceSpy : IAudioSource
    {
        public AudioSourceSpy(IAudioSource source)
        {
            NativeSource = source;
        }

        private IAudioSource m_objNativeSource = null;

        public IAudioSource NativeSource
        {
            get { return m_objNativeSource; }
            set { m_objNativeSource = value; }
        }

        public event DelegatePullSample OnPullSample = null;

        #region IAudioSource Members

        public MediaSample PullSample(AudioFormat format, TimeSpan tsDuration)
        {
            MediaSample sample =  NativeSource.PullSample(format, tsDuration);

            if (OnPullSample != null)
                OnPullSample(sample);

            return sample;
        }

        public bool IsSourceActive
        {
            get
            {
                return NativeSource.IsSourceActive;
            }
            set
            {
                NativeSource.IsSourceActive = value;
            }
        }

        public double SourceAmplitudeMultiplier
        {
            get
            {
                return NativeSource.SourceAmplitudeMultiplier;
            }
            set
            {
                NativeSource.SourceAmplitudeMultiplier = value;
            }
        }

        #endregion
    }
}
