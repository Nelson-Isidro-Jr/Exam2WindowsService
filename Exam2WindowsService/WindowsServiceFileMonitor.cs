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
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.*"
            };
            folder1Watcher.Created += OnFileCreated;

        }

        protected override void OnStart(string[] args)
        {
            SetupFileSystemWatcher();
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                
                System.Threading.Thread.Sleep(4000);

                // Construct the destination file path
                string destinationFilePath = Path.Combine(destinationFolderPath, Path.GetFileName(e.FullPath));

                // Check if the file already exists and rename if necessary
                int count = 1;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                string extension = Path.GetExtension(e.FullPath);

                while (File.Exists(destinationFilePath))
                {
                    string tempFileName = $"{fileNameWithoutExtension}_{count}{extension}";
                    destinationFilePath = Path.Combine(destinationFolderPath, tempFileName);
                    count++;
                }

               
                File.Move(e.FullPath, destinationFilePath);
            }
            catch (Exception ex)
            {
               
                EventLog.WriteEntry("WindowsServiceFileMonitor", $"Error moving file: {ex.Message}", EventLogEntryType.Error);
            }
        }

        protected override void OnStop()
        {
            folder1Watcher.Dispose();
        }
    }
}
