using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;

namespace RTP
{
   public class RTPPacketBuffer : IComparer<RTPPacket>, INotifyPropertyChanged
   {
      public RTPPacketBuffer()
      {
      }
      public RTPPacketBuffer(int nPacketQueueMinimumSize, int nMaxQueueSize)
      {
         PacketQueueMinimumSize = nPacketQueueMinimumSize;
         MaxQueueSize = nMaxQueueSize;
         if (PacketQueueMinimumSize < 0)
            throw new Exception("Packet queue size must be greater than zero");
      }

      public override string ToString()
      {
         return string.Format("RTPPacketBuffer, Current Size {0}, Last Received Sequence: {1}", m_nCurrentQueueSize, m_nLastReceivedSequence);
      }

      private int m_nPacketQueueMinimumSize = 2;

      public int PacketQueueMinimumSize
      {
        get { return m_nPacketQueueMinimumSize; }
        set { m_nPacketQueueMinimumSize = value; }
      }
      private int m_nMaxQueueSize = 4;

      public int MaxQueueSize
      {
        get { return m_nMaxQueueSize; }
        set { m_nMaxQueueSize = value; }
      }

      List<RTPPacket> Packets = new List<RTPPacket>();
      object PacketLock = new object();

      public void Clear()
      {
         lock (PacketLock)
         {
            Packets.Clear();
         }
      }

      private int m_nCurrentQueueSize = 0;

      public int CurrentQueueSize
      {
         get { return m_nCurrentQueueSize; }
         protected set 
         { 
            if (m_nCurrentQueueSize != value)
            {
              m_nCurrentQueueSize = value; 
               FirePropertyChanged("CurrentQueueSize");
            }
         }
      }

      private int m_nUnavailablePackets = 0;

      /// <summary>
      /// This doesn't really work because we don't push packets on a timer in our audio graph.  If a packet is unavailble, a filter down the line just generates silence
      /// </summary>
      public int UnavailablePackets
      {
         get { return m_nUnavailablePackets; }
         protected set { m_nUnavailablePackets = value; }
      }


      private int m_nDiscardedPackets = 0;

      public int DiscardedPackets
      {
         get { return m_nDiscardedPackets; }
         protected set { m_nDiscardedPackets = value; }
      }


      public void ResetStats()
      {
         UnavailablePackets = 0;
         DiscardedPackets = 0;
      }

      public void Reset()
      {
          ResetStats();
          m_bHaveSetInitialSequence = false;
          m_nNextExpectedSequence = 0xFFFF;
          m_nLastReceivedSequence = 0xFFFF;
          lock (PacketLock)
          {
              Packets.Clear();
          }
      }

      bool m_bHaveSetInitialSequence = false;
      ushort m_nNextExpectedSequence = 0xFFFF;

      ushort m_nLastReceivedSequence = 0xFFFF;
      /// <summary>
      /// Adds a packet to our buffer
      /// </summary>
      /// <param name="packet"></param>
      public void AddPacket(RTPPacket packet)
      {
         m_nLastReceivedSequence = packet.SequenceNumber;
         if (m_bHaveSetInitialSequence == false)
         {
             m_nNextExpectedSequence = packet.SequenceNumber;
             m_bHaveSetInitialSequence = true;
         }

         int nNewSize = 0;
         lock (PacketLock)
         {
            /// If packet is before the last one we've given out, discard it, we already assumed it was lost
            if (CompareSequence(packet.SequenceNumber, m_nNextExpectedSequence) < 0)
            {
               DiscardedPackets++;
               return;
            }



            Packets.Add(packet);
            Packets.Sort(this);

            while (Packets.Count > MaxQueueSize)
            {
               Packets.RemoveAt(0);
               DiscardedPackets++;
            }

            nNewSize = Packets.Count;
         }
         CurrentQueueSize = nNewSize;
      }
      
      /// <summary>
      /// Gets the next ordered packet from our buffer, or null if none are available
      /// </summary>
      /// <returns></returns>
      public RTPPacket GetPacket()
      {
          RTPPacket packetret = null;
          int nNewSize = 0;
          lock (PacketLock)
          {
              if (Packets.Count < PacketQueueMinimumSize)
              {
                  UnavailablePackets++;
                  return null;
              }

              packetret = Packets[0];
              if (packetret.SequenceNumber <= m_nNextExpectedSequence) /// Packet is the expected one, or before the expected one
              {
                  Packets.RemoveAt(0);
                  m_nNextExpectedSequence = (ushort)(packetret.SequenceNumber + 1);
              }
              else if (Packets.Count == MaxQueueSize) // May have lost the desired packet.  We've waited all we can, get the lowest packet number
              {
                  Packets.RemoveAt(0);
                  m_nNextExpectedSequence = (ushort)(packetret.SequenceNumber + 1);
              }
              else
                  packetret = null;

              nNewSize = Packets.Count;
          }
          CurrentQueueSize = nNewSize;
          return packetret;
      }


      #region IComparer<RTPPacket> Members

      public int Compare(RTPPacket x, RTPPacket y)
      {
         int nXSeq = x.SequenceNumber;
         int nYSeq = y.SequenceNumber;
         return CompareSequence(nXSeq, nYSeq);
      }

      public int CompareSequence(int nXSeq, int nYSeq)
      {
         if (Math.Abs(nXSeq - nYSeq) > 60000) // we rolled around, add to our zero index so it appears in order
         {
            if (nXSeq < 5000)
               nXSeq += ushort.MaxValue;
            if (nYSeq < 5000)
               nYSeq += ushort.MaxValue;
         }
         return nXSeq.CompareTo(nYSeq);
      }

      #endregion

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
