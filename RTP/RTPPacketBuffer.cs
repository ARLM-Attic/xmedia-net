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
         InitialPacketQueueMinimumSize = nPacketQueueMinimumSize;
         InitialMaxQueueSize = nMaxQueueSize;
         if (InitialPacketQueueMinimumSize < 0)
            throw new Exception("Packet queue size must be greater than zero");
      }

      public override string ToString()
      {
         return string.Format("RTPPacketBuffer, Current Size {0}, Last Received Sequence: {1}", m_nCurrentQueueSize, m_nLastReceivedSequence);
      }

      private int m_nInitialPacketQueueMinimumSize = 2;
      public int InitialPacketQueueMinimumSize
      {
          get { return m_nInitialPacketQueueMinimumSize; }
          set { m_nInitialPacketQueueMinimumSize = value; }
      }

      private int m_nPacketQueueMinimumSize = 2;
      public int CurrentPacketQueueMinimumSize
      {
        get { return m_nPacketQueueMinimumSize; }
        set { m_nPacketQueueMinimumSize = value; }
      }

      public int PacketSizeShiftMax = 4; // allow the PacketQueueMinimumSize and MaxQueueSize to grow by 4 if needed

      private int m_nInitialMaxQueueSize = 4;
      public int InitialMaxQueueSize
      {
          get { return m_nInitialMaxQueueSize; }
          set { m_nInitialMaxQueueSize = value; }
      }

      private int m_nMaxQueueSize = 4;
      public int CurrentMaxQueueSize
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
          CurrentPacketQueueMinimumSize = InitialPacketQueueMinimumSize;
          CurrentMaxQueueSize = InitialMaxQueueSize;
          m_nTotalPackets = 0;
          FirstPacketTime = DateTime.Now;
          lock (PacketLock)
          {
              Packets.Clear();
          }
      }

      bool m_bHaveSetInitialSequence = false;
      ushort m_nNextExpectedSequence = 0xFFFF;
      ushort m_nLastReceivedSequence = 0xFFFF;
      private uint m_nTotalPackets = 0;
      public uint TotalPackets
      {
          get { return m_nTotalPackets; }
          set { m_nTotalPackets = value; }
      }

      private DateTime m_dtFirstPacket = DateTime.MinValue;

      public DateTime FirstPacketTime
      {
          get { return m_dtFirstPacket; }
          set { m_dtFirstPacket = value; }
      }

      public TimeSpan Duration
      {
          get
          {
              if (m_dtFirstPacket == DateTime.MinValue)
                  return TimeSpan.Zero;

              return DateTime.Now - m_dtFirstPacket;
          }
      }

      public double AverageInterPacketTimeMs
      {
          get
          {
              if (m_nTotalPackets <= 1)
                  return 0.0f;
              TimeSpan duration = Duration;
              return duration.TotalMilliseconds / m_nTotalPackets;
          }
      }

      public string Statistics
      {
          get
          {
              if (m_nTotalPackets == 0)
                  return "none";

              double fPercent = ((double)(DiscardedPackets*100.0f)) / (double)m_nTotalPackets;
              return string.Format("Loss: {0}, Total: {1}, Discarded: {2}, NA: {3}, Size: {4}", fPercent.ToString("N2"), m_nTotalPackets, DiscardedPackets, UnavailablePackets, CurrentQueueSize);

          }
      }
      /// <summary>
      /// Adds a packet to our buffer
      /// </summary>
      /// <param name="packet"></param>
      public void AddPacket(RTPPacket packet)
      {
          m_nTotalPackets++;
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
            /// 
            int nSequenceCompare = CompareSequence(packet.SequenceNumber, m_nNextExpectedSequence);
            if (nSequenceCompare < 0)
            {
               DiscardedPackets++;
               return;
            }

            Packets.Add(packet);
            Packets.Sort(this);

            while (Packets.Count > CurrentMaxQueueSize)
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
              if (Packets.Count < CurrentPacketQueueMinimumSize)  /// If packet unavailability becomes common, grow our queue so it stops happenning
              {
                  UnavailablePackets++;
                  if (UnavailablePackets > 10)
                  {
                      if (CurrentPacketQueueMinimumSize < (InitialPacketQueueMinimumSize+PacketSizeShiftMax) )
                      {
                        UnavailablePackets = 0;
                        CurrentPacketQueueMinimumSize++;
                        CurrentMaxQueueSize++;
                      }
                  }
                  return null;
              }

              packetret = Packets[0];
              int nSequenceCompare = CompareSequence(packetret.SequenceNumber, m_nNextExpectedSequence);

              if (nSequenceCompare > 0) //(packetret.SequenceNumber > m_nNextExpectedSequence)
              {
                  if (Packets.Count >= CurrentMaxQueueSize)
                  {
                      // May have lost the desired packet.  We've waited all we can, get the lowest packet number
                      Packets.RemoveAt(0);
                      m_nNextExpectedSequence = RTPPacket.GetNextSequence(packetret.SequenceNumber);
                  }
                  else
                  {
                      /// we haven't filled our queue, so we can wait a little longer
                      packetret = null;
                  }
              }
              else if (nSequenceCompare == 0) //(packetret.SequenceNumber == m_nNextExpectedSequence)
              {
                  Packets.RemoveAt(0);
                  m_nNextExpectedSequence = RTPPacket.GetNextSequence(packetret.SequenceNumber);
              }
              else
              {
                  /// packet sequence is before the expected value... should never happen
                  packetret = null;
              }

              //if (packetret.SequenceNumber <= m_nNextExpectedSequence) /// Packet is the expected one, or before the expected one... should never happen since we don't add packets before the next expected one
              //{
              //    Packets.RemoveAt(0);
              //    m_nNextExpectedSequence = (ushort)(packetret.SequenceNumber + 1);
              //}
              //else if (Packets.Count == MaxQueueSize) // May have lost the desired packet.  We've waited all we can, get the lowest packet number
              //{
              //    Packets.RemoveAt(0);
              //    m_nNextExpectedSequence = (ushort)(packetret.SequenceNumber + 1);
              //}

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
