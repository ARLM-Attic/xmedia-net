using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic;

namespace SocketServer
{
   public interface IMediaTimer
   {
      void Cancel();
      double RemainingTimeSeconds
      {
         get;
      }
   }

   /// <summary>
   /// What our delegate should look like, otherwise we'll get an exception because the parameters we pass
   /// back won't match.
   /// </summary>
   public delegate void DelegateTimerFired(IMediaTimer objTimer);


   /// <summary>
   /// A timer class that should be highly accurate
   /// </summary>
   public class MediaTimer : IComparable, IMediaTimer
   {
       private MediaTimer(System.DateTime dtDue, DelegateTimerFired del, string strGuid, ILogInterface logmgr)
      {
         DueTime = dtDue;
         CallBack = del;
         Id = Interlocked.Increment(ref BaseTimerId);
         Guid = strGuid;
         m_logmgr = logmgr;
      }

      private MediaTimer(System.DateTime dtDue, DelegateTimerFired del, string strGuid, object objTag)
      {
         DueTime = dtDue;
         CallBack = del;
         Id = Interlocked.Increment(ref BaseTimerId);
         Guid = strGuid;
         m_logmgr = null;
         Tag = objTag;
      }

      public readonly int Id;
      public readonly System.DateTime DueTime;
      public readonly string Guid;
      private DelegateTimerFired CallBack;
      public object Tag = null;

      public double RemainingTimeSeconds
      {
         get
         {
            TimeSpan tsRemaining = DueTime - DateTime.Now;
            return tsRemaining.TotalSeconds;
         }
      }

      #region IComparable Members

      public int CompareTo(object obj)
      {
         MediaTimer objTimer2 = obj as MediaTimer;
         if (obj == null)
            return 0;
         return DueTime.CompareTo(objTimer2.DueTime);
      }

      #endregion


      public bool Expired
      {
         get
         {
            TimeSpan tsDif = DueTime - DateTime.Now;
            if (Convert.ToInt32(Math.Ceiling(tsDif.TotalMilliseconds)) <= AccuracyAndLag) /// if we are within 5 ms, we're expired
               return true;
            else
               return false;
         }
      }

      public void Fire()
      {
         try
         {
            if (CallBack != null)
            {
               CallBack.DynamicInvoke(new object[] { this }); //invoke our self if we have no host
               CallBack = null;
            }
         }
         catch (System.NullReferenceException e)
         {
            if (m_logmgr != null)
               m_logmgr.LogError(Guid, MessageImportance.Highest, string.Format("Exception in timer thread: {0}", e));

         }
      }

      public void Cancel()
      {
         lock (TimerLock)
         {
            if (SortedTimers.Contains(this))
            {
               SortedTimers.Remove(this);
               EventNewTimer.Set();
            }
         }

         this.CallBack = null;
      }


      protected ILogInterface m_logmgr = null;
      private static List<MediaTimer> SortedTimers = new List<MediaTimer>();
      private static object TimerLock = new object();

      private static int BaseTimerId = 1;
      private static bool Initialized = false;
      private static object LockInit = new object();

      /// <summary>
      /// The default timeout to check all timers even if none have fired
      /// </summary>
      public static int TimerCheck = 10000;
      public static int AccuracyAndLag = 1; /// account for cpu... fire within 5 ms

      public static IMediaTimer CreateTimer(int nMilliseconds, DelegateTimerFired del, string strGuid, ILogInterface logmgr)
      {
         lock (LockInit)
         {
            if (Initialized == false)
            {
               PrepareStuff();
               Initialized = true;
            }
         }

         System.DateTime dtDue = DateTime.Now.AddMilliseconds(Convert.ToDouble(nMilliseconds));

         MediaTimer objNewTimer = new MediaTimer(dtDue, del, strGuid, logmgr);
         AddSorted(objNewTimer);

         return objNewTimer;
      }


      public static IMediaTimer CreateTimer(int nMilliseconds, DelegateTimerFired del, string strGuid, object objTag)
      {
         lock (LockInit)
         {
            if (Initialized == false)
            {
               PrepareStuff();
               Initialized = true;
            }
         }

         System.DateTime dtDue = DateTime.Now.AddMilliseconds(Convert.ToDouble(nMilliseconds));

         MediaTimer objNewTimer = new MediaTimer(dtDue, del, strGuid, objTag);
         AddSorted(objNewTimer);

         return objNewTimer;
      }

      private static System.Threading.AutoResetEvent EventNewTimer = new AutoResetEvent(false);
      private static Thread WorkerThread;
      private static void PrepareStuff()
      {
         WorkerThread = new Thread(new ThreadStart(CheckTimerThread));
         WorkerThread.IsBackground = true;
         WorkerThread.Name = "Script Timer";
         WorkerThread.Priority = ThreadPriority.AboveNormal;
         WorkerThread.Start();
      }

      public static ThreadState QueryThreadState
      {
         get
         {
            return WorkerThread.ThreadState;
         }
      }

      private static void AddSorted(MediaTimer objTimer)
      {
         lock (TimerLock)
         {
            int nIndexInsert = 0;
            foreach (MediaTimer nextTimer in SortedTimers)
            {
               if (objTimer.CompareTo(nextTimer) < 0) // this timer is less than this timer, insert here
               {
                  break;
               }
               nIndexInsert++;
            }

            SortedTimers.Insert(nIndexInsert, objTimer);
            EventNewTimer.Set();
         }
      }

      private static void CheckTimerThread()
      {
         //if (m_logmgr != null)
         // m_logmgr.LogError("GENERAL", "TIMER", LogInterfaces.MessageImportance.Highest, 0, string.Format("Starting Timer Thread: Id {0}", AppDomain.GetCurrentThreadId()));

         System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
         System.DateTime dtNextDue = DateTime.Now.AddMilliseconds(Convert.ToDouble(TimerCheck));
         while (true)
         {
            int nHandle = WaitHandle.WaitTimeout;

            /// See how long until the next timer is due
            TimeSpan tsNextTimer = dtNextDue - DateTime.Now;
            int nNextTimeOut = Convert.ToInt32(Math.Ceiling(tsNextTimer.TotalMilliseconds));
            if (nNextTimeOut < 0)
            {
               /// need to find out if this is happening
               nNextTimeOut = 0;
            }

            if (nNextTimeOut > AccuracyAndLag)
               nHandle = WaitHandle.WaitAny(new WaitHandle[] { EventNewTimer }, nNextTimeOut, true);

            try
            {

               if (nHandle == WaitHandle.WaitTimeout)  // Timer elapsed
               {
                  ArrayList alTimersRemoveAndFire = new ArrayList();
                  lock (TimerLock)
                  {
                     int nIndexInsert = 0;
                     if (SortedTimers.Count == 0)  /// no timers, no need to check until we get signaled
                     {
                        dtNextDue = DateTime.Now.AddMilliseconds(Convert.ToDouble(TimerCheck));
                        continue;
                     }

                     foreach (MediaTimer nextTimer in SortedTimers)
                     {
                        if (nextTimer.Expired == true)
                        {
                           alTimersRemoveAndFire.Add(nextTimer);
                        }
                        else
                        {
                           dtNextDue = nextTimer.DueTime;
                           break;
                        }
                     }

                     foreach (MediaTimer nextTimer in alTimersRemoveAndFire)
                     {
                        SortedTimers.Remove(nextTimer);
                     }
                  }


                  foreach (MediaTimer nextTimer in alTimersRemoveAndFire)
                  {
                     nextTimer.Fire();
                  }
               }
               else if (nHandle == 0) /// new item added or removed
               {
                  lock (TimerLock)
                  {
                     if (SortedTimers.Count > 0)
                     {
                        dtNextDue = ((MediaTimer)SortedTimers[0]).DueTime;
                     }
                     else
                     {
                        dtNextDue = DateTime.Now.AddMilliseconds(Convert.ToDouble(TimerCheck));
                     }

                  }
               }

            }
            catch (System.Exception e2)
            {
               /// should not have any exceptions here
               /// 
               //if (m_logmgr != null)
               // m_logmgr.LogError("GENERAL", "TIMER", LogInterfaces.MessageImportance.Highest, 0, string.Format("Exception in timer thread: {0}", e2));
            }


         }
      }

   }

}
