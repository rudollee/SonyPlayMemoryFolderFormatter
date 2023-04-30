using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace SonyPlayMemoryFolderFormatter
{
    public partial class ServiceDefault : ServiceBase
    {
        private List<DirectoryInfo> _directories = new List<DirectoryInfo>();
        private System.Timers.Timer _timer = new System.Timers.Timer();
        private readonly string _folderToWatch;
        private FileSystemWatcher _sonyWatcher = new FileSystemWatcher();
        public ServiceDefault()
        {
            InitializeComponent();

            _folderToWatch = Environment.GetEnvironmentVariable("Sony");
            _sonyWatcher.Path = _folderToWatch;
            _sonyWatcher.Created +=_sonyWatcher_Created;

            _timer.Interval = 3500;
            _timer.Elapsed +=_timer_Elapsed;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _directories.ForEach(f => {
                while (Locked(f.FullName))
                {
                    Thread.Sleep(500);
                }
                f.MoveTo(f.FullName.Replace("-", ""));
            });
            
            _timer.Stop();
        }

        private void _sonyWatcher_Created(object sender, FileSystemEventArgs e)
        {
            _timer.Stop();
            if (Directory.Exists(e.FullPath))
            {
                _directories.Add(new DirectoryInfo(e.FullPath));
            }

            _timer.Start();
        }

        private bool Locked(string filename)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            return false;
        }

        protected override void OnStart(string[] args)
        {
            _sonyWatcher.EnableRaisingEvents = true;
            _timer.Stop();
        }

        protected override void OnStop()
        {
            _sonyWatcher.EnableRaisingEvents = false;
            _timer.Stop();
        }
    }
}
