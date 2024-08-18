using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Exam2WindowsService
{
    public partial class WindowsServiceFileMonitor : ServiceBase
    {
        private FileSystemWatcher folder1Watcher;
        private readonly string sourceFolderPath = @"C:\Folder1";
        private readonly string destinationFolderPath = @"C:\Folder2";

        public WindowsServiceFileMonitor()
        {
            InitializeComponent();
            
        }

        private void SetupFileSystemWatcher()
        {
            folder1Watcher = new FileSystemWatcher(sourceFolderPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.*"
            };
            folder1Watcher.Created += OnFileCreated;


        }

        protected override void OnStart(string[] args)
        {
            folder1Watcher.EnableRaisingEvents = true;
            SetupFileSystemWatcher();
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            // Move the file to Folder2 when created
            string destinationFilePath = Path.Combine(destinationFolderPath, Path.GetFileName(e.FullPath));
            File.Move(e.FullPath, destinationFilePath);
        }

        protected override void OnStop()
        {
            folder1Watcher.Dispose();
        }
    }
}
