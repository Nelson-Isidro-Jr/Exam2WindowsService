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
            SetupEventLog();
        }

        private void SetupEventLog()
        {
            if (!EventLog.SourceExists("WindowsServiceFileMonitor"))
            {
                EventLog.CreateEventSource("WindowsServiceFileMonitor", "Application");
            }

            EventLog.Source = "WindowsServiceFileMonitor";
            EventLog.Log = "Application";
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
            LogEvent("File system watcher initialized and started.", EventLogEntryType.Information);
        }

        protected override void OnStart(string[] args)
        {
            SetupFileSystemWatcher();
            LogEvent("Service started successfully.", EventLogEntryType.Information);
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                LogEvent($"File created: {e.FullPath}. Waiting before processing...", EventLogEntryType.Information);

                System.Threading.Thread.Sleep(4000);

                string destinationFilePath = GenerateUniqueFilePath(e.FullPath);

                File.Move(e.FullPath, destinationFilePath);

                LogEvent($"File moved from {e.FullPath} to {destinationFilePath}.", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                LogEvent($"Error moving file: {ex.Message}", EventLogEntryType.Error);
            }
        }

        private string GenerateUniqueFilePath(string sourceFilePath)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath);
            string extension = Path.GetExtension(sourceFilePath);
            string destinationFilePath = Path.Combine(destinationFolderPath, Path.GetFileName(sourceFilePath));

            int count = 1;
            while (File.Exists(destinationFilePath))
            {
                string tempFileName = $"{fileNameWithoutExtension}_{count}{extension}";
                destinationFilePath = Path.Combine(destinationFolderPath, tempFileName);
                count++;
            }

            return destinationFilePath;
        }

        private void LogEvent(string message, EventLogEntryType type)
        {
            EventLog.WriteEntry("WindowsServiceFileMonitor", message, type);
        }

        protected override void OnStop()
        {
            folder1Watcher.Dispose();
            LogEvent("Service stopped successfully.", EventLogEntryType.Information);
        }
    }
}
