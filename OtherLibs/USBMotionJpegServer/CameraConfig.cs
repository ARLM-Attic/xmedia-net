using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioClasses;
using System.Runtime.Serialization;
using ImageAquisition;

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

        private bool m_bRecordMotion = true;
        [DataMember]
        public bool RecordMotion
        {
            get { return m_bRecordMotion; }
            set { m_bRecordMotion = value; }
        }

        private double m_fMotionLevel = 30.0f;
        [DataMember]
        public double MotionLevel
        {
            get { return m_fMotionLevel; }
            set { m_fMotionLevel = value; }
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

        private string m_strMaskFileName = null;
        [DataMember]
        public string MaskFileName
        {
            get { return m_strMaskFileName; }
            set { m_strMaskFileName = value; }
        }

        private int m_nMaxMotionDetectionFramesPerSecond = 5;
        [DataMember(EmitDefaultValue=true)]
        public int MaxMotionDetectionFramesPerSecond
        {
            get { return m_nMaxMotionDetectionFramesPerSecond; }
            set { m_nMaxMotionDetectionFramesPerSecond = value; }
        }

        private VideoCaptureRate [] m_objVideoCaptureRates = new VideoCaptureRate[]{};
        [DataMember]
        public VideoCaptureRate [] VideoCaptureRates
        {
            get { return m_objVideoCaptureRates; }
            set { m_objVideoCaptureRates = value; }
        }

        private int m_nMaxFrameRateServed = -1;
        [DataMember]
        public int MaxFrameRateServed
        {
            get { return m_nMaxFrameRateServed; }
            set { m_nMaxFrameRateServed = value; }
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
            DataContractSerializer serializer = new DataContractSerializer(typeof(CameraConfig[]));
            System.IO.FileStream stream = new System.IO.FileStream(strFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            serializer.WriteObject(stream, cameras);
            stream.Close();
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
