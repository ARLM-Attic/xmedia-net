using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioClasses;
using System.Runtime.Serialization;
using ImageAquisition;
using System.Xml;

namespace USBMotionJpegServer
{
    [DataContract]
    public class CameraConfig
    {
        public CameraConfig()
        {
        }

        public CameraConfig(string strUniqueName)
        {
            UniqueName = strUniqueName;
        }

        public CameraConfig(string strUniqueName, VideoCaptureRate [] rates)
        {
            UniqueName = strUniqueName;
            VideoCaptureRates = rates;
        }

        [OnDeserializing]
        void OnDeserializing(StreamingContext context)
        {
            this.DoMotionAnalysis = true;
            this.ShowMotionImages = true;
            this.RecordMotion = true;
            this.PreRecordFrames = 30;
            this.PostRecordFrames = 30;
            this.MaxEncodingQueueSize = 90;
            this.MotionLevel = 15.0f;
            this.MinimumBlobSizeTriggerMotion = 5.0f;
            this.ShowText = true;
            this.MaskFileName = "";
            this.MaxFrameRateServed = 0;
            this.MaxFramesAnalyzed = 0;
        }

        private bool m_bDoMotionAnalysis = true;
        [DataMember]
        public bool DoMotionAnalysis
        {
            get { return m_bDoMotionAnalysis; }
            set { m_bDoMotionAnalysis = value; }
        }

        private bool m_bShowMotionImages = true;
        [DataMember]
        public bool ShowMotionImages
        {
            get { return m_bShowMotionImages; }
            set { m_bShowMotionImages = value; }
        }

        private bool m_bRecordMotion = true;
        [DataMember]
        public bool RecordMotion
        {
            get { return m_bRecordMotion; }
            set { m_bRecordMotion = value; }
        }

        private int m_nPreRecordFrames = 30;
        [DataMember]
        public int PreRecordFrames
        {
            get { return m_nPreRecordFrames; }
            set { m_nPreRecordFrames = value; }
        }
        
        private int m_nPostRecordFrames = 30;
        [DataMember]
        public int PostRecordFrames
        {
            get { return m_nPostRecordFrames; }
            set { m_nPostRecordFrames = value; }
        }

        private int m_nMaxEncodingQueueSize = 90;
        [DataMember]
        public int MaxEncodingQueueSize
        {
            get { return m_nMaxEncodingQueueSize; }
            set { m_nMaxEncodingQueueSize = value; }
        }

        private double m_fMotionLevel = 30.0f;
        [DataMember]
        public double MotionLevel
        {
            get { return m_fMotionLevel; }
            set { m_fMotionLevel = value; }
        }

        private double m_fMinimumBlobSizeTriggerMotion = 0.0f;
        [DataMember]
        public double MinimumBlobSizeTriggerMotion
        {
            get { return m_fMinimumBlobSizeTriggerMotion; }
            set { m_fMinimumBlobSizeTriggerMotion = value; }
        }

        private bool m_bShowText = false;
        [DataMember]
        public bool ShowText
        {
            get { return m_bShowText; }
            set { m_bShowText = value; }
        }

        private string m_strUniqueName = "";
        [DataMember]
        public string UniqueName
        {
            get { return m_strUniqueName; }
            set { m_strUniqueName = value; }
        }

        private string m_strName = "";
        [DataMember]
        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        private string m_strMaskFileName = "";
        [DataMember]
        public string MaskFileName
        {
            get { return m_strMaskFileName; }
            set { m_strMaskFileName = value; }
        }

        private VideoCaptureRate [] m_objVideoCaptureRates = new VideoCaptureRate[]{};
        [DataMember]
        public VideoCaptureRate [] VideoCaptureRates
        {
            get { return m_objVideoCaptureRates; }
            set { m_objVideoCaptureRates = value; }
        }

        private int m_nMaxFrameRateServed = 0;
        [DataMember]
        public int MaxFrameRateServed
        {
            get { return m_nMaxFrameRateServed; }
            set { m_nMaxFrameRateServed = value; }
        }

        private int m_nMaxFramesAnalyzed = 0;
        [DataMember]
        public int MaxFramesAnalyzed
        {
            get { return m_nMaxFramesAnalyzed; }
            set { m_nMaxFramesAnalyzed = value; }
        }

        public static void CreateDefault(string strFileName)
        {
            MFVideoCaptureDevice [] Devices = MFVideoCaptureDevice.GetCaptureDevices();

            List<CameraConfig> cameras = new List<CameraConfig>();
            foreach (MFVideoCaptureDevice dev in Devices)
            {
                List<VideoCaptureRate> rates = new List<VideoCaptureRate>();
                foreach (VideoCaptureRate rate in dev.VideoFormats)
                {
                    if ((rate.CompressedFormat == VideoDataFormat.MJPEG) || (rate.CompressedFormat == VideoDataFormat.RGB24) || (rate.CompressedFormat == VideoDataFormat.RGB32))
                        rates.Add(rate);
                }

                CameraConfig config = new CameraConfig(dev.UniqueName, rates.ToArray());
                config.Name = dev.Name;
                cameras.Add(config);
            }

            SaveCameras(strFileName, cameras.ToArray());
        }

        public static void SaveCameras(string strFileName, CameraConfig [] cameras)
        {
            /// See if there are any new cameras first
            /// 
            List<CameraConfig> allcameras = new List<CameraConfig>(cameras);
            MFVideoCaptureDevice[] Devices = MFVideoCaptureDevice.GetCaptureDevices();
            foreach (MFVideoCaptureDevice dev in Devices)
            {
                bool bFound = false;
                foreach (CameraConfig camera in cameras)
                {
                    if (dev.UniqueName == camera.UniqueName)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (bFound == false)
                {
                    List<VideoCaptureRate> rates = new List<VideoCaptureRate>();
                    foreach (VideoCaptureRate rate in dev.VideoFormats)
                    {
                        if ((rate.CompressedFormat == VideoDataFormat.MJPEG) || (rate.CompressedFormat == VideoDataFormat.RGB24) || (rate.CompressedFormat == VideoDataFormat.RGB32))
                            rates.Add(rate);
                    }
                    CameraConfig config = new CameraConfig(dev.UniqueName, rates.ToArray());
                    config.Name = dev.Name;
                    allcameras.Add(config);
                }
            }

            DataContractSerializer serializer = new DataContractSerializer(typeof(CameraConfig[]));
            //System.IO.FileStream stream = new System.IO.FileStream(strFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            //serializer.WriteObject(stream, cameras);

            var settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "\t"
            };

            using (var writer = XmlWriter.Create(strFileName, settings))
            {
                serializer.WriteObject(writer, allcameras.ToArray());
            }


            //stream.Close();
        }


        public static CameraConfig[] LoadCameras(string strFileName)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(CameraConfig[]));
            try
            {
                System.IO.FileStream stream = new System.IO.FileStream(strFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                CameraConfig[] cameras = (CameraConfig[])serializer.ReadObject(stream);

                stream.Close();
                return cameras;
            }
            catch (Exception ex)
            {
            }
            return null;
        }
    }
}
