using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Built.Grpcc.Utils
{
    public delegate void FileSystemEvent(String path);

    public interface IDirectoryMonitor
    {
        event FileSystemEvent Change;

        void Start();
    }

    public class DirectoryMonitor : IDirectoryMonitor
    {
        private readonly FileSystemWatcher m_fileSystemWatcher = new FileSystemWatcher();
        private readonly Dictionary<string, DateTime> m_pendingEvents = new Dictionary<string, DateTime>();
        private readonly Timer m_timer;
        private bool m_timerStarted = false;

        public string MonitorPath { get; }
        public string Filter { get; }

        public DirectoryMonitor(string dirPath, string filter)
        {
            MonitorPath = dirPath;
            Filter = filter;
            m_fileSystemWatcher.Path = dirPath;
            m_fileSystemWatcher.Filter = filter;// "*.dll";
            m_fileSystemWatcher.IncludeSubdirectories = false;
            m_fileSystemWatcher.Created += new FileSystemEventHandler(OnChange);
            m_fileSystemWatcher.Changed += new FileSystemEventHandler(OnChange);
            m_fileSystemWatcher.Deleted += new FileSystemEventHandler(OnDelete);
            /*
              NotifyFilter = NotifyFilters.Attributes |
                                   NotifyFilters.CreationTime |
                                   NotifyFilters.DirectoryName |
                                   NotifyFilters.FileName |
                                   NotifyFilters.LastAccess |
                                   NotifyFilters.LastWrite |
                                   NotifyFilters.Security |
                                   NotifyFilters.Size,
             */
            m_timer = new Timer(OnTimeout, null, Timeout.Infinite, Timeout.Infinite);
        }

        public event FileSystemEvent Change;

        public event FileSystemEvent Delete;

        public void Start()
        {
            m_fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void OnChange(object sender, FileSystemEventArgs e)
        {
            // Don't want other threads messing with the pending events right now
            lock (m_pendingEvents)
            {
                // Save a timestamp for the most recent event for this path
                m_pendingEvents[e.FullPath] = DateTime.Now;

                // Start a timer if not already started
                if (!m_timerStarted)
                {
                    m_timer.Change(100, 100);
                    m_timerStarted = true;
                }
            }
        }

        private void OnDelete(object sender, FileSystemEventArgs e)
        {
            Delete?.Invoke(e.FullPath);
        }

        private void OnTimeout(object state)
        {
            List<string> paths;

            // Don't want other threads messing with the pending events right now
            lock (m_pendingEvents)
            {
                // Get a list of all paths that should have events thrown
                paths = FindReadyPaths(m_pendingEvents);

                // Remove paths that are going to be used now
                paths.ForEach(delegate (string path)
                {
                    m_pendingEvents.Remove(path);
                });

                // Stop the timer if there are no more events pending
                if (m_pendingEvents.Count == 0)
                {
                    m_timer.Change(Timeout.Infinite, Timeout.Infinite);
                    m_timerStarted = false;
                }
            }

            // Fire an event for each path that has changed
            paths.ForEach(delegate (string path)
            {
                FireEvent(path);
            });
        }

        private List<string> FindReadyPaths(Dictionary<string, DateTime> events)
        {
            List<string> results = new List<string>();
            DateTime now = DateTime.Now;

            foreach (KeyValuePair<string, DateTime> entry in events)
            {
                // If the path has not received a new event in the last 75ms
                // an event for the path should be fired
                double diff = now.Subtract(entry.Value).TotalMilliseconds;
                if (diff >= 75)
                {
                    results.Add(entry.Key);
                }
            }

            return results;
        }

        private void FireEvent(string path)
        {
            Change?.Invoke(path);
        }
    }
}