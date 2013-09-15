/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if !MONO
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Collections.Specialized;

namespace System.Net.XMPP
{
#if WINDOWS_PHONE
    [CollectionDataContract()]
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        protected override void ClearItems()
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                base.ClearItems();
            });
        }

        protected override void InsertItem(int index, T item)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                base.InsertItem(index, item);
            });
        }


        public void AddNoNotify(T item)
        {
           base.InsertItem(this.Count, item);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                base.OnCollectionChanged(e);
            });
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                base.OnPropertyChanged(e);
            });
        }

        protected override void RemoveItem(int index)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                base.RemoveItem(index);
            });
        }

        protected override void SetItem(int index, T item)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                base.SetItem(index, item);
            });
        }

  
    }
#else
   [CollectionDataContract()]
   public class ObservableCollectionEx<T> : ObservableCollection<T>
   {
      // Override the event so this class can access it
      public override event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;

      protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
      {
         // Be nice - use BlockReentrancy like MSDN said
         using (BlockReentrancy())
         {
            System.Collections.Specialized.NotifyCollectionChangedEventHandler eventHandler = CollectionChanged;
            if (eventHandler == null)
               return;

            Delegate[] delegates = eventHandler.GetInvocationList();
            // Walk thru invocation list
            foreach (System.Collections.Specialized.NotifyCollectionChangedEventHandler handler in delegates)
            {
               DispatcherObject dispatcherObject = handler.Target as DispatcherObject;
               // If the subscriber is a DispatcherObject and different thread
               if (dispatcherObject != null && dispatcherObject.CheckAccess() == false)
               {
                  // Invoke handler in the target dispatcher's thread
                  dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, e);
               }
               else // Execute handler as is
                  handler(this, e);
            }
         }
      }

      public T[] ToArray()
      {
          List<T> Returnlist = new List<T>();
          foreach (T item in this)
          {
              Returnlist.Add(item);
          }

          return Returnlist.ToArray();
      }
   }
#endif

}
#endif
