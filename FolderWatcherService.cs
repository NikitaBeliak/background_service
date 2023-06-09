﻿using Microsoft.Extensions.Logging;
using Quartz;

namespace background_service
{
    public class FolderWatcherService
    {
        private readonly string _folderToWatch;
        private readonly string _watchPeriod;
        private readonly ILogger<FolderWatcherService> _logger;

        public FolderWatcherService(string folderToWatch, string watchPeriod, ILogger<FolderWatcherService> logger)
        {
            _folderToWatch = folderToWatch;
            _watchPeriod = watchPeriod;
            _logger = logger;
        }

        public void StartWatching()
        {
            _logger.LogInformation($"Сервис мониторинга папки {_folderToWatch} запущен");

            //Console.WriteLine(_watchPeriod);
            var cronExpression = new CronExpression(_watchPeriod);
            var nextDateTime = cronExpression.GetNextValidTimeAfter(DateTime.Now);
            var interval = nextDateTime - DateTime.Now;

            if (!Directory.Exists(_folderToWatch))
            {
                Console.WriteLine($"Folder {_folderToWatch} does not exist.");
                return;
            }

            Console.WriteLine($"Watching folder {_folderToWatch}...");

            using var watcher = new FileSystemWatcher(_folderToWatch);
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite |NotifyFilters.DirectoryName;

            watcher.Created += OnChanged;
            watcher.Changed += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;

            watcher.EnableRaisingEvents = true;

            while (true)
            {
                Thread.Sleep(interval.Value);

                nextDateTime = cronExpression.GetNextValidTimeAfter(DateTime.Now);
                interval = nextDateTime - DateTime.Now;

                watcher.EnableRaisingEvents = false;

                if (DateTime.Now.Hour >= 18)
                {
                    break;
                }

                watcher.EnableRaisingEvents = true;
            }

            _logger.LogInformation($"Сервис мониторинга папки {_folderToWatch} завершен");
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            Console.WriteLine($"{e.ChangeType} {e.OldFullPath} -> {e.FullPath}");
            _logger.LogInformation($"Файл {e.OldFullPath} -> {e.FullPath} {e.ChangeType}");
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"{e.ChangeType} {e.FullPath}");
            _logger.LogInformation($"Файл {e.FullPath} {e.ChangeType}");
        }
    }
}
