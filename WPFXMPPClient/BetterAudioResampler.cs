using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AudioClasses;
using ImageAquisition;

namespace WPFXMPPClient
{
    public class BetterAudioResampler : AudioResampler
    {
        public BetterAudioResampler() : base()
        {
        }

        ImageAquisition.SampleConvertor Converter = null;

        public override MediaSample Resample(MediaSample sample, AudioFormat outformat)
        {
            if (outformat == sample.AudioFormat)
                return sample;

            short [] sData = sample.GetShortData();
            if (Converter == null)
            {
                /// Assume we will always get the same size data to resample, and the same in/out formats
                /// 
                //Converter = new SampleConvertor(((int)sample.AudioFormat.AudioSamplingRate) / 100, ((int)outformat.AudioSamplingRate) / 100, sData.Length);
                //Converter = new SampleConvertor(((int)sample.AudioFormat.AudioSamplingRate) / 1000, ((int)outformat.AudioSamplingRate) / 1000, sData.Length, .25f, 1.0f);

                /// 16000 to 8000
                if ( (sample.AudioFormat.AudioSamplingRate == AudioSamplingRate.sr16000) && (outformat.AudioSamplingRate == AudioSamplingRate.sr8000) )
                    Converter = new SampleConvertor(16, 8, sData.Length, .25f, 0.5f);
                else if ((sample.AudioFormat.AudioSamplingRate == AudioSamplingRate.sr16000) && (outformat.AudioSamplingRate == AudioSamplingRate.sr8000))
                    Converter = new SampleConvertor(8, 16, sData.Length, .50f, .25f);
                else
                    Converter = new SampleConvertor(((int)sample.AudioFormat.AudioSamplingRate) / 100, ((int)outformat.AudioSamplingRate) / 100, sData.Length);
            }

            short[] sConverted = Converter.Convert(sData);

            return new MediaSample(sConverted, outformat);
        }

    }
}
