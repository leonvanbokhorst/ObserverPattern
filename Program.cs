using System;
using System.Collections.Generic;

namespace Observer
{
    internal class Program
    {
        private static void Main()
        {
            var newsFeedNull = new NewsFeed();
            var newsFeedRemondo = new NewsFeed();
            var newsFeedMonitor = new NewsFeedMonitor();

            IDisposable unsubscriberNYT =
                newsFeedNull.Subscribe(newsFeedMonitor);
            IDisposable unsubscriberRemondo =
                newsFeedRemondo.Subscribe(newsFeedMonitor);

            newsFeedNull.Uri = null; // to force an error
            newsFeedRemondo.Uri = new Uri("http://remondo.net/feed");

            Console.Read();

            unsubscriberNYT.Dispose();
            unsubscriberRemondo.Dispose();
        }
    }

    internal class NewsFeed : IObservable<NewsFeed>
    {
        private readonly List<IObserver<NewsFeed>> observers =
            new List<IObserver<NewsFeed>>();

        private Uri _uri;

        public Uri Uri
        {
            get { return _uri; }
            set
            {
                _uri = value;
                Notify(this);
            }
        }

        #region IObservable<NewsFeed> Members

        public IDisposable Subscribe(IObserver<NewsFeed> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }

            return new Unsubscriber(observers, observer);
        }

        #endregion

        private void Notify(NewsFeed newsFeed)
        {
            foreach (var observer in observers)
            {
                if (_uri == null)
                {
                    observer.OnError(new Exception("Sumtinwong"));
                }
                else
                {
                    observer.OnNext(this);
                }
            }
        }

        private void StopReading()
        {
            foreach (var observer in observers)
            {
                if (observers.Contains(observer))
                {
                    observer.OnCompleted();
                }

                observers.Clear(); // No more dangling events!
            }
        }

        #region Nested type: Unsubscriber

        private class Unsubscriber : IDisposable
        {
            private readonly IObserver<NewsFeed> _observer;
            private readonly List<IObserver<NewsFeed>> _observers;

            public Unsubscriber(List<IObserver<NewsFeed>> observers,
                                IObserver<NewsFeed> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            #region IDisposable Members

            public void Dispose()
            {
                if (_observer != null
                    && _observers.Contains(_observer))
                {
                    _observers.Remove(_observer);
                }
            }

            #endregion
        }

        #endregion
    }

    internal class NewsFeedMonitor : IObserver<NewsFeed>
    {
        #region IObserver<NewsFeed> Members

        public void OnCompleted()
        {
            Console.WriteLine("No more news today...");
        }

        public void OnError(Exception error)
        {
            Console.WriteLine("Error in News Feed... "
                              + error.Message);
        }

        public void OnNext(NewsFeed value)
        {
            Console.WriteLine("There's a new News Item... "
                              + value.Uri.AbsoluteUri);
        }

        #endregion
    }
}