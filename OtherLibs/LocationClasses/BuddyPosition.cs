using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.XMPP;
using System.ComponentModel;

namespace LocationClasses
{
    public class BuddyPosition : INotifyPropertyChanged
    {
        public BuddyPosition(RosterItem item)
        {
            RosterItem = item;
            ((INotifyPropertyChanged)item).PropertyChanged += new PropertyChangedEventHandler(BuddyPosition_PropertyChanged);
        }

        void BuddyPosition_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "GeoLoc")
            {
                /// New geolocation, add it to our list
                /// 
                GeoCoordinate coord = new GeoCoordinate(RosterItem.GeoLoc.lon, RosterItem.GeoLoc.lat, RosterItem.GeoLoc.TimeStamp);
                CoordinateList.Add(coord);
                FirePropertyChanged("Count");

                if (ShowOnMap)
                {
                    IsDirty = true;
                    // It will fire itself
                    //FirePropertyChanged("ShowOnMap");
                }

                //Window1 win1 = new Window1();
                //win1.Activate();
                //win1.Show();
            }
        }

        private string m_LocalAvatarPath = @"C:\Users\Jaime\Pictures\New folder (2)\IMG_20120623_153041.jpg";

        public string LocalAvatarPath
        {
            get { return m_LocalAvatarPath; }
            set
            {
                m_LocalAvatarPath = value;
                FirePropertyChanged("LocalAvatarPath");
            }
        }


        private bool m_bShowOnMap = false;

        public bool ShowOnMap
        {
            get { return m_bShowOnMap; }
            set
            {
                m_bShowOnMap = value;
                FirePropertyChanged("ShowOnMap");
            }
        }

        private RosterItem m_objRosterItem = null;

        public RosterItem RosterItem
        {
            get { return m_objRosterItem; }
            set
            {
                m_objRosterItem = value;
                FirePropertyChanged("RosterItem");
            }
        }

        public void ClearCoordinates()
        {
            m_listCoordinateList.Clear();
            FirePropertyChanged("Count");
        }

        private List<GeoCoordinate> m_listCoordinateList = new List<GeoCoordinate>();

        public List<GeoCoordinate> CoordinateList
        {
            get { return m_listCoordinateList; }
            set { m_listCoordinateList = value; }
        }

        public int Count
        {
            get
            {
                return CoordinateList.Count;
            }
            set
            {
            }
        }

        private bool m_bIsDirty = false;

        public bool IsDirty
        {
            get { return m_bIsDirty; }
            set
            {
                m_bIsDirty = value;
                FirePropertyChanged("IsDirty");
            }
        }



        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(strName));
        }

        #endregion
    }
}
