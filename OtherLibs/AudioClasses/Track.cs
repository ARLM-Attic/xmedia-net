using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.ComponentModel;

#if !MONO
using System.Windows.Threading;
#endif

using System.IO;

namespace AudioClasses
{
    /// <summary>
    ///  Some of this taken from http://stackoverflow.com/questions/68283/view-edit-id3-data-for-mp3-files
    /// </summary>
    class MusicID3Tag 
    { 
 
        public byte[] TAGID = new byte[3];      //  3 
        public byte[] Title = new byte[30];     //  30 
        public byte[] Artist = new byte[30];    //  30  
        public byte[] Album = new byte[30];     //  30  
        public byte[] Year = new byte[4];       //  4  
        public byte[] Comment = new byte[30];   //  30  
        public byte[] Genre = new byte[1];      //  1 
 
    } 
 
 

    [DataContract]
    public class Track : INotifyPropertyChanged
    {
        public Track()
        {
        }
        public Track(string strFileName)
        {
            FileName = strFileName;
            Name = System.IO.Path.GetFileName(FileName);
        }

        public override string ToString()
        {
            return Name;
        }

        private string m_strFileName = "";

        [DataMember]
        public string FileName
        {
            get { return m_strFileName; }
            set { m_strFileName = value; }
        }

        private string m_strName = "";
        [DataMember]
        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        private string m_strArtist = "";
        [DataMember]
        public string Artist
        {
            get { return m_strArtist; }
            set { m_strArtist = value; }
        }

        private TimeSpan m_tsDuration = TimeSpan.Zero;
        [DataMember]
        public TimeSpan Duration
        {
            get { return m_tsDuration; }
            set { m_tsDuration = value; }
        }

        private string m_strAlbum = "";
        [DataMember]
        public string Album
        {
            get { return m_strAlbum; }
            set { m_strAlbum = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj is Track)
            {
                return ((Track)obj).FileName.Equals(this.FileName);
            }
            
            return false;
        }

        public int RandomIndex = 0;

        bool m_bHasGotTagData = false;

        public void GetTagData()
        {
            if (m_bHasGotTagData == true)
                return;
            m_bHasGotTagData = true;

            using (FileStream fs = File.OpenRead(this.FileName))
            {
                if (fs.Length >= 128)
                {
                    MusicID3Tag tag = new MusicID3Tag();
                    fs.Seek(-128, SeekOrigin.End);
                    fs.Read(tag.TAGID, 0, tag.TAGID.Length);
                    fs.Read(tag.Title, 0, tag.Title.Length);
                    fs.Read(tag.Artist, 0, tag.Artist.Length);
                    fs.Read(tag.Album, 0, tag.Album.Length);
                    fs.Read(tag.Year, 0, tag.Year.Length);
                    fs.Read(tag.Comment, 0, tag.Comment.Length);
                    fs.Read(tag.Genre, 0, tag.Genre.Length);
                    string theTAGID = Encoding.Default.GetString(tag.TAGID);

                    if (theTAGID.Equals("TAG"))
                    {
                        this.Name = TrimString(System.Text.ASCIIEncoding.ASCII.GetString(tag.Title));
                        this.Artist = TrimString(System.Text.ASCIIEncoding.ASCII.GetString(tag.Artist));
                        this.Album = TrimString(System.Text.ASCIIEncoding.ASCII.GetString(tag.Album));
                        //string Year = Encoding.Default.GetString(tag.Year);
                        //string Comment = Encoding.Default.GetString(tag.Comment);
                        //string Genre = Encoding.Default.GetString(tag.Genre);
                     
                    }
                }
            }
        }

        string TrimString(string strValue)
        {
            strValue = strValue.Trim();
            int nIndex = strValue.IndexOf('\0');
            if (nIndex > 0)
                return strValue.Substring(0, nIndex);

            return strValue;
        }


        #region INotifyPropertyChanged Members

        protected void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
            {
#if WINDOWS_PHONE
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(PropertyChanged, this, new System.ComponentModel.PropertyChangedEventArgs(strName));
#elif MONO
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(strName));
#else
                System.ComponentModel.PropertyChangedEventArgs args = new System.ComponentModel.PropertyChangedEventArgs(strName);
                System.ComponentModel.PropertyChangedEventHandler eventHandler = PropertyChanged;
                if (eventHandler == null)
                    return;

                Delegate[] delegates = eventHandler.GetInvocationList();
                // Walk thru invocation list
                foreach (System.ComponentModel.PropertyChangedEventHandler handler in delegates)
                {
                    DispatcherObject dispatcherObject = handler.Target as DispatcherObject;
                    // If the subscriber is a DispatcherObject and different thread
                    if (dispatcherObject != null && dispatcherObject.CheckAccess() == false)
                    {
                        // Invoke handler in the target dispatcher's thread
                        dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, args);
                    }
                    else // Execute handler as is
                        handler(this, args);
                }
                //System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(PropertyChanged, this, new System.ComponentModel.PropertyChangedEventArgs(strName));
#endif
            }
        }
        public event PropertyChangedEventHandler PropertyChanged = null;

        #endregion
    }

    [DataContract]
    public class PlayList : IComparer<Track>, INotifyPropertyChanged
    {
        public PlayList()
        {
        }

        public PlayList(string strName)
        {
            Name = strName;
        }

        public void Clone(PlayList list)
        {
            this.Name = list.Name;
            this.Tracks.Clear();
            foreach (Track track in list.Tracks)
            {
                this.Tracks.Add(track);
            }
            if (Tracks.Count > 0)
                CurrentTrack = Tracks[0];
            else
                CurrentTrack = null;
        }

        private string m_strName = "";
        [DataMember]
        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        

        private Track m_objCurrentTrack = null;

        public Track CurrentTrack
        {
            get 
            { 
                return m_objCurrentTrack; 
            }
            set 
            {
                if (m_objCurrentTrack != value)
                {
                    m_objCurrentTrack = value;
                    FirePropertyChanged("CurrentTrack");
                }
            }
        }

        public Track NextTrack()
        {
            if (Tracks.Count <= 0)
                return null;

            int nIndex = -1;
            if (CurrentTrack != null)
            {
                nIndex = Tracks.IndexOf(CurrentTrack);
            }
            nIndex++;
            
            if (nIndex > (Tracks.Count-1))
            {
                if (Loop == true)
                    nIndex = 0;
                else
                    return null;
            }

            CurrentTrack = Tracks[nIndex];
            return CurrentTrack;
        }

        public Track PreviousTrack()
        {
            if (Tracks.Count <= 0)
                return null;

            int nIndex = Tracks.Count;
            if (CurrentTrack != null)
            {
                nIndex = Tracks.IndexOf(CurrentTrack);
            }
            nIndex--;

            if (nIndex < 0)
            {
                if (Loop == true)
                    nIndex = Tracks.Count-1;
                else
                    return null;
            }

            CurrentTrack = Tracks[nIndex];
            return CurrentTrack;
        }

        private bool m_bLoop = true;
        [DataMember]
        public bool Loop
        {
            get { return m_bLoop; }
            set { m_bLoop = value; }
        }


        private ObservableCollection<Track> m_listTracks = new ObservableCollection<Track>();
        [DataMember]
        public ObservableCollection<Track> Tracks
        {
            get { return m_listTracks; }
            set { m_listTracks = value; }
        }

        public void Add(string strFileName)
        {
            Track newtrack = new Track(strFileName);

            if (Tracks.Contains(newtrack) == false)
                Tracks.Add(newtrack);

            if (Tracks.Count == 1)
                CurrentTrack = Tracks[0];
        }
        public void Insert(int nIndex, string strFileName)
        {
            Track newtrack = new Track(strFileName);
            Insert(nIndex, newtrack);

        }

        // Also good for moving a track to a new position
        public void Insert(int nIndex, Track objTrack)
        {
            if (Tracks.Contains(objTrack) == false)
            {
                Tracks.Insert(nIndex, objTrack);
            }
            else
            {
                int nPos = Tracks.IndexOf(objTrack);
                Tracks.Remove(objTrack);
                if (nPos < nIndex)
                    nIndex--;
                Tracks.Insert(nIndex, objTrack);

            }
            if (Tracks.Count == 1)
                CurrentTrack = Tracks[0];
        }

        public void Remove(Track objTrack)
        {
            if (Tracks.Contains(objTrack) == true)
                Tracks.Remove(objTrack);
            if (Tracks.Count == 1)
                CurrentTrack = Tracks[0];
        }

        public Track PickRandomTrack()
        {
            if (Tracks.Count <= 0)
                return null;

            int nIndex = rand.Next(Tracks.Count);
            CurrentTrack = Tracks[nIndex];
            return CurrentTrack;
        }
      
        public void Randomize()
        {
            List<Track> tracks = new List<Track>(this.Tracks);
            foreach (Track objTrac in tracks)
                objTrac.RandomIndex = rand.Next();
            tracks.Sort(this);

            Tracks.Clear();
            foreach (Track objTrac in tracks)
                Tracks.Add(objTrac);            
        }

        public void PopulateFromDirectory(string strDirectory)
        {
            PopulateFromDirectory(strDirectory, -1);
        }
        public void PopulateFromDirectory(string strDirectory, int nCount)
        {
            //string strDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyMusic);
            //MusicFiles = System.IO.Directory.GetFiles(strDir, "*.mp3", System.IO.SearchOption.AllDirectories);

            List<Track> tracks = new List<Track>();
            AddFilesToListRandom(ref tracks, strDirectory, "*.mp3");
            AddFilesToListRandom(ref tracks, strDirectory, "*.wma");
            
            tracks.Sort(this);
            Tracks.Clear();

            int nCountSoFar = 0;
            foreach (Track objTrac in tracks)
            {
                Tracks.Add(objTrac);
                nCountSoFar++;
                if (nCount > 0)
                    if (nCountSoFar >= nCount)
                        break;
            }

            if (Tracks.Count > 0)
                CurrentTrack = Tracks[0];
            else
                CurrentTrack = null;
        }

        void AddFilesToListRandom(ref List<Track> tracks, string strDirectory, string strWildCard)
        {
            string[] MusicFiles = new string[] { };
            MusicFiles = System.IO.Directory.GetFiles(strDirectory, strWildCard, System.IO.SearchOption.AllDirectories);
            foreach (string strFile in MusicFiles)
            {
                Track newtrack = new Track(strFile);
                newtrack.RandomIndex = rand.Next();
                tracks.Add(newtrack);
            }
        }

        public void Clear()
        {
            Tracks.Clear();
            CurrentTrack = null;
        }

        public int Count
        {
            get
            {
                return Tracks.Count;
            }
        }

        #region IComparer<Track> Members

        Random rand = new Random();
        public int Compare(Track x, Track y)
        {
            return x.RandomIndex.CompareTo(y.RandomIndex);
        }

        #endregion
    
        #region INotifyPropertyChanged Members

        protected void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
            {
#if WINDOWS_PHONE
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(PropertyChanged, this, new System.ComponentModel.PropertyChangedEventArgs(strName));
#elif MONO
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(strName));
#else
                System.ComponentModel.PropertyChangedEventArgs args = new System.ComponentModel.PropertyChangedEventArgs(strName);
                System.ComponentModel.PropertyChangedEventHandler eventHandler = PropertyChanged;
                if (eventHandler == null)
                    return;

                Delegate[] delegates = eventHandler.GetInvocationList();
                // Walk thru invocation list
                foreach (System.ComponentModel.PropertyChangedEventHandler handler in delegates)
                {
                    DispatcherObject dispatcherObject = handler.Target as DispatcherObject;
                    // If the subscriber is a DispatcherObject and different thread
                    if (dispatcherObject != null && dispatcherObject.CheckAccess() == false)
                    {
                        // Invoke handler in the target dispatcher's thread
                        dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, args);
                    }
                    else // Execute handler as is
                        handler(this, args);
                }
                //System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(PropertyChanged, this, new System.ComponentModel.PropertyChangedEventArgs(strName));
#endif
            }
        }
        public event PropertyChangedEventHandler  PropertyChanged = null;

        #endregion
}


}
