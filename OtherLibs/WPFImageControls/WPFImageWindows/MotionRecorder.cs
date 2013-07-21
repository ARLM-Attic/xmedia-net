
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;

using System.Runtime.Serialization;
using AudioClasses;

namespace WPFImageWindows
{
    public enum FrameType
    {
        FirstFrame,
        MiddleFrame,
        LastFrame,
    }

    public class FrameToBeEncoded : IDisposable
    {
        public FrameToBeEncoded()
        {
        }

        public FrameToBeEncoded(byte []bytes, int nWidth, int nHeight)
        {
            FrameBytes = new byte[bytes.Length];
            Array.Copy(bytes, FrameBytes, bytes.Length);
            Width = nWidth;
            Height = nHeight;
        }


        private int m_nBitmapWidth = 0;

        public int Width
        {
            get { return m_nBitmapWidth; }
            set { m_nBitmapWidth = value; }
        }
        private int m_nBitmapHeight = 0;

        public int Height
        {
            get { return m_nBitmapHeight; }
            set { m_nBitmapHeight = value; }
        }

        private byte[] m_bFrameBytes = new byte[] { };
        public byte[] FrameBytes
        {
            get { return m_bFrameBytes; }
            set { m_bFrameBytes = value; }
        }

        private byte[] m_bAudioBytes = null;

        public byte[] AudioBytes
        {
            get { return m_bAudioBytes; }
            set { m_bAudioBytes = value; }
        }

        /// Can't use bitmap because it get's locked from a different thread
        //private System.Drawing.Bitmap m_bmpFrame = null;

        //public System.Drawing.Bitmap Frame
        //{
        //    get { return m_bmpFrame; }
        //    set { m_bmpFrame = value; }
        //}
        private DateTime m_dtTimeStamp = DateTime.Now;

        public DateTime TimeStamp
        {
            get { return m_dtTimeStamp; }
            set { m_dtTimeStamp = value; }
        }
        private FrameType m_eFrameType = FrameType.MiddleFrame;

        public FrameType FrameType
        {
            get { return m_eFrameType; }
            set { m_eFrameType = value; }
        }

        #region IDisposable Members

        bool m_bDisposed = false;
        public void Dispose()
        {
            if (m_bDisposed == true)
                return;
            m_bDisposed = true;
            AudioBytes = null;
            FrameBytes = null;

        }

        #endregion
    }


    /// <summary>
    /// Hooks up to a USBSecurityCamera and saves compressed recorded frames to a file
    /// if motion is detected
    /// </summary>
    [DataContract]
    public class MutlitFormatMotionRecorder : System.ComponentModel.INotifyPropertyChanged, IVideoRecorder
    {
        public MutlitFormatMotionRecorder(IVideoSource camera, bool bHookEvents)
        {
            RecordingDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyVideos);

            Camera = camera;
            if (bHookEvents == true)
                Camera.OnNewFrame += Camera_OnNewFrame;

            ThreadEncoding = new System.Threading.Thread(new System.Threading.ThreadStart(EncodingThread));
            ThreadEncoding.IsBackground = true;
            ThreadEncoding.Priority = System.Threading.ThreadPriority.Highest;
            ThreadEncoding.Name = "Motion Recording Encoding Thread";
            ThreadEncoding.Start();
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strProp)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                (Action<string>)((prop) =>
                        {
                            if (PropertyChanged != null)
                                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(prop));
                        }
                       )
                    , strProp);
        }

        #endregion


        private IVideoSource m_objCamera = null;
        public IVideoSource Camera
        {
            get { return m_objCamera; }
            protected set { m_objCamera = value; }
        }

        private string m_strDirectory = System.Environment.CurrentDirectory;

        [DataMember]
        public string Directory
        {
            get { return m_strDirectory; }
            set { m_strDirectory = value; }
        }


        /// <summary>
        /// Holds the last m_nPreviousFrames of video before motion was detected
        /// </summary>
        xmedianet.socketserver.EventQueueWithNotification<FrameToBeEncoded> PreviousFrameBuffer = new xmedianet.socketserver.EventQueueWithNotification<FrameToBeEncoded>();

        /// <summary>
        /// Holds the list of frames to be encoded to a file
        /// </summary>
        xmedianet.socketserver.EventQueueWithNotification<FrameToBeEncoded> EncodingQueue = new xmedianet.socketserver.EventQueueWithNotification<FrameToBeEncoded>();


        System.Threading.Thread ThreadEncoding = null;

        DateTime m_dtStart = DateTime.MinValue;


        ImageAquisition.MFVideoEncoder Recorder = null;
        AudioClasses.VideoCaptureRate rate = null;

        private string m_strLastRecordedFileName = "";
        public string LastRecordedFileName
        {
            get { return m_strLastRecordedFileName; }
            set { m_strLastRecordedFileName = value; }
        }

        private int m_nMaxRecordingFrames = -1;
        
        /// <summary>
        /// The maximum number of frames to record in each video session before stopping the recording
        /// </summary>
        public int MaxRecordingFrames
        {
            get { return m_nMaxRecordingFrames; }
            set { m_nMaxRecordingFrames = value; }
        }

        bool m_bRecording = false;
        public bool Recording
        {
            get { return m_bRecording; }
            set
            {
                m_bRecording = true;
            }
        }
        
        int m_nRecordedFrames = 0;
        object objRecordingLock = new object();
        public bool StartRecording()
        {
            if (m_bRecording == false)
            {
                m_bRecording = true;
                return true;
            }
            return false;
        }

        public string StopRecording()
        {
            m_bRecording = false;
            return LastRecordedFileName;
        }
        private string m_strRecordingDirectory = "";

        public string RecordingDirectory
        {
            get { return m_strRecordingDirectory; }
            set { m_strRecordingDirectory = value; }
        }

       



        /// <summary>
        /// Encodes motion to an H.264 compressed file when triggered.  Hopefully works fast enough so the queue doesn't get
        /// too big
        /// </summary>
        void EncodingThread()
        {
            while (true)
            {
                FrameToBeEncoded frame = EncodingQueue.WaitNext(1000);
                if (frame != null)
                {
                    lock (objRecordingLock)
                    {

                        if ((this.MaxRecordingFrames > 0) && (m_nRecordedFrames >= this.MaxRecordingFrames))
                        {
                            if (Recorder != null)
                            {
                                Recorder.Stop();
                                Recorder = null;
                                m_bRecording = false;
                            }
                            continue;
                        }


                        if (Recorder == null)
                        {
                            ImageAquisition.MFVideoEncoder tempRecorder = new ImageAquisition.MFVideoEncoder();
                            string strName = Camera.Name.Replace("/", "");
                            strName = strName.Replace("\\", "");
                            strName = strName.Replace("?", "");

                            LastRecordedFileName = string.Format("{0}/{1}-{2}.mp4", RecordingDirectory, strName, Guid.NewGuid().ToString());
                            rate = new VideoCaptureRate(frame.Width, frame.Height, CurrentFrameRate, 3000000);
                            rate.CompressedFormat = AudioClasses.VideoDataFormat.MP4;
                            tempRecorder.Start(LastRecordedFileName, rate, frame.TimeStamp, false);
                            System.Diagnostics.Debug.WriteLine(string.Format("Starting recording {0}, start time {1}.{2}", LastRecordedFileName, frame.TimeStamp, frame.TimeStamp.Millisecond));
                            Recorder = tempRecorder;
                        }

                        if (Recorder != null)
                        {
                            if ((frame.Width == rate.Width) && (frame.Height == rate.Height) && (frame.FrameBytes != null))
                            {

                                //byte[] bRGB32Data = ImageUtils.Utils.Convert24BitImageTo32BitImage(bRGBData, nWidth, nHeight);
                                m_nRecordedFrames++;
                                System.Diagnostics.Debug.WriteLine(string.Format("Adding Frame{0}, time {1}.{2}", m_nRecordedFrames, frame.TimeStamp, frame.TimeStamp.Millisecond));
                                Recorder.AddVideoFrame(frame.FrameBytes, frame.TimeStamp);
                            }
                            else
                            {
                                if (Recorder != null)
                                {
                                    Recorder.Stop();
                                    Recorder = null;
                                }

                            }
                        }

                        if (frame.FrameType == FrameType.LastFrame)
                        {
                            if (Recorder != null)
                            {
                                Recorder.Stop();
                                Recorder = null;
                            }

                        }
                    }

                }
            }

        }

        DateTime m_dtFirstFrameReceived = DateTime.MinValue;
        int m_nTotalFrames = 0;

        public int CurrentFrameRate
        {
            get 
            {
                if ((m_dtFirstFrameReceived == DateTime.MinValue) || (m_nTotalFrames == 0))
                    return 30;
                TimeSpan tsTotal = DateTime.Now - m_dtFirstFrameReceived;
                double fFramesPerSec = m_nTotalFrames / tsTotal.TotalSeconds;
                return (int)fFramesPerSec;
            }
            
        }


        public int EncodingQueueCount
        {
            get
            {
                return EncodingQueue.Count;
            }
        }
        /// <summary>
        /// The number of frames before motion detection occurred to store to the file
        /// </summary>
        /// 
        private int m_nPreviousFrames = 10;
        [DataMember]
        public int PreviousFrames
        {
            get { return m_nPreviousFrames; }
            set { m_nPreviousFrames = value; }
        }

        private int m_nSubsequentFrames = 20;

        /// <summary>
        /// The number of frames after motion detection occurred to store to the file
        /// </summary>
        /// 
        [DataMember]
        public int SubsequentFrames
        {
            get { return m_nSubsequentFrames; }
            set { m_nSubsequentFrames = value; }
        }
   

        private bool m_bIsRecordingMotion = false;

        public bool IsRecordingMotion
        {
            get { return m_bIsRecordingMotion; }
            set { m_bIsRecordingMotion = value; }
        }


        void ClearAllPreviousFrames()
        {
            FrameToBeEncoded[] allprevframe = PreviousFrameBuffer.WaitAll(0);
            if (allprevframe != null)
            {
                foreach (FrameToBeEncoded prevfram in allprevframe)
                {
                    prevfram.Dispose();
                }
            }
            PreviousFrameBuffer.Clear();
        }


        bool m_bCurrentMotion = false;
        int m_nFramesLeftToRecord = 0;

       
        /// <summary>
        /// The maximum number of frames that the encoder will queue before dropping frames
        /// </summary>
        private int m_nMaxEncodingQueueSize = 60+20;

        [DataMember]
        public int MaxEncodingQueueSize
        {
            get { return m_nMaxEncodingQueueSize; }
            set { m_nMaxEncodingQueueSize = value; }
        }

        int nFrameNumber = 0;

        private AudioClasses.IMotionDetector m_objMotionDetector = null;
        public AudioClasses.IMotionDetector MotionDetector
        {
            get { return m_objMotionDetector; }
            set { m_objMotionDetector = value; }
        }

        private bool m_bShowMotionImages = false;
        public bool ShowMotionImages
        {
            get { return m_bShowMotionImages; }
            set { m_bShowMotionImages = value; }
        }

        DateTime m_dtLastFrameProcessed = DateTime.MinValue;



        public byte [] SetNewFrame(byte[] bRawData, VideoCaptureRate format, object objSource)
        {
            if (m_dtFirstFrameReceived == DateTime.MinValue)
                m_dtFirstFrameReceived = DateTime.Now;

            m_nTotalFrames++;

            if (EncodingQueue.Count > MaxEncodingQueueSize)
                return bRawData; /// Not enough memory or CPU to handle this request



            bool bMotionDetected = false;
            if ((IsRecordingMotion == true) && (MotionDetector != null))
            {
                bMotionDetected = MotionDetector.Detect(ref bRawData, format.Width, format.Height, ShowMotionImages);
                m_dtLastFrameProcessed = DateTime.Now;
            }

            /// 

            FrameToBeEncoded frame = new FrameToBeEncoded(bRawData, format.Width, format.Height);

            if (Recording == true)
            {
                if (m_bCurrentlyRecording == false)
                {

                    FrameToBeEncoded[] frames = PreviousFrameBuffer.WaitAll(0);
                    if ((frames != null) && (frames.Length > 0))
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("Starting new encoding, enqueing {0} previous frames", frames.Length));

                        frames[0].FrameType = FrameType.FirstFrame;
                        EncodingQueue.Enqueue(frames);
                    }
                    else
                        frame.FrameType = FrameType.FirstFrame;

                    EncodingQueue.Enqueue(frame);

                    m_bCurrentlyRecording = true;
                }
                else
                {
                    EncodingQueue.Enqueue(frame);
                }
            }
            else if ((Recording == false) && (m_bCurrentlyRecording == true))
            {
                frame.FrameType = FrameType.LastFrame;
                EncodingQueue.Enqueue(frame);
                m_bCurrentlyRecording = false;
            }

            else if (IsRecordingMotion == true)
            {
                if (bMotionDetected == true) /// We have motion.  We always record an addtion SubsequentFrames after any motion
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Got a frame with motion, Time {0}.{1}", frame.TimeStamp, frame.TimeStamp.Millisecond));
                    m_nFramesLeftToRecord = SubsequentFrames;
                    if (m_bCurrentMotion == false)
                    {
                        FrameToBeEncoded[] frames = PreviousFrameBuffer.WaitAll(0);
                        if ((frames != null) && (frames.Length > 0))
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format("Starting new encoding, enqueing {0} previous frames", frames.Length));

                            frames[0].FrameType = FrameType.FirstFrame;
                            EncodingQueue.Enqueue(frames);
                        }
                        else
                            frame.FrameType = FrameType.FirstFrame;


                        m_bCurrentMotion = true;
                    }
                    EncodingQueue.Enqueue(frame);

                }
                else if (m_nFramesLeftToRecord > 0)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Got a frame after motion... {0} left to write", m_nFramesLeftToRecord));

                    /// No motion, but we have to record the trailing end
                    m_nFramesLeftToRecord--;

                    if (m_nFramesLeftToRecord <= 0)
                    {
                        frame.FrameType = FrameType.LastFrame;
                        m_nFramesLeftToRecord = 0;
                        //ClearAllPreviousFrames();
                    }
                    EncodingQueue.Enqueue(frame);

                }
                else
                {
                    m_bCurrentMotion = false;
                    m_nFramesLeftToRecord = 0;


                    PreviousFrameBuffer.Enqueue(frame);
                    if (PreviousFrameBuffer.Count > PreviousFrames)
                    {
                        FrameToBeEncoded prvframe = PreviousFrameBuffer.WaitNext(1);
                        prvframe.Dispose();
                    }

                    System.Diagnostics.Debug.WriteLine(string.Format("No motion, enqueing frame... count is now {0}", PreviousFrameBuffer.Count));

                }
            }
            else if (IsRecordingMotion == false) /// No recording... no need to buffer anything
            {
                m_bCurrentMotion = false;
                m_nFramesLeftToRecord = 0;

                /// Keep previous frames for recordings too, just in case they're too late
                PreviousFrameBuffer.Enqueue(frame);
                if (PreviousFrameBuffer.Count > PreviousFrames)
                {
                    FrameToBeEncoded prvframe = PreviousFrameBuffer.WaitNext(1);
                    prvframe.Dispose();
                }


            }
            return bRawData;

        }

        private bool m_bCurrentlyRecording = false;
        protected void Camera_OnNewFrame(byte[] bRawData, VideoCaptureRate format, object objSource)
        {
            SetNewFrame(bRawData, format, objSource);
        }


    }
}
