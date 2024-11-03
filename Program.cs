using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DZ_9
{
    public class ImageDownloader
    {
        public event EventHandler? ImageStarted;
        public event EventHandler? ImageCompleted;

        public async Task DownloadAsync(string remoteUri, string fileName, CancellationToken cancellationToken)
        {
            OnImageStarted();

            using (var myWebClient = new WebClient())
            {
                cancellationToken.ThrowIfCancellationRequested();
                cancellationToken.Register(myWebClient.CancelAsync);
                Console.WriteLine($"Качаю: \"{fileName}\" from \"{remoteUri}\" .......\n\n");
                try
                {
                    await myWebClient.DownloadFileTaskAsync(remoteUri, fileName);
                    Console.WriteLine($"Успешно скачал \"{fileName}\" from \"{remoteUri}\"");
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"Загрузка отменена: {fileName}");
                }
            }

            OnImageCompleted();
        }

        protected virtual void OnImageStarted()
        {
            ImageStarted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnImageCompleted()
        {
            ImageCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    class Program
    {
        static async Task Main()
        {
            string[] remoteUris = new string[]
            {
            "https://webneel.com/daily/sites/default/files/images/daily/09-2014/1-rose-drawing-stephen-ainsworth.jpg",
            "https://webneel.com/daily/sites/default/files/images/daily/09-2014/6-rose-watercolor-painting.jpg",
            "https://webneel.com/daily/sites/default/files/images/daily/01-2014/14-drawings-of-flowers-hibiscus.jpg",
            "https://webneel.com/daily/sites/default/files/images/daily/01-2014/1-flower-drawings-rose.jpg",
            "https://webneel.com/daily/sites/default/files/images/daily/09-2014/7-rose-color-pencil-drawing.jpg",
            "https://webneel.com/daily/sites/default/files/images/daily/09-2014/12-rose-painting.jpg",
            "https://webneel.com/daily/sites/default/files/images/daily/09-2014/14-rose-painting.jpg",
            "https://webneel.com/daily/sites/default/files/images/daily/01-2014/20-drawings-of-flowers.jpg",
            "https://webneel.com/daily/sites/default/files/images/daily/11-2013/10-flower-paintings.jpg",
            "https://webneel.com/daily/sites/default/files/images/daily/11-2013/17-flower-painting-rose.jpg"
            };

            List<Task> downloadTasks = new List<Task>();
            Dictionary<Task, string> taskFileNames = new Dictionary<Task, string>();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;
            ImageDownloader downloader = new ImageDownloader();
            downloader.ImageStarted += (sender, e) => Console.WriteLine("Скачивание файла началось");
            downloader.ImageCompleted += (sender, e) => Console.WriteLine("Скачивание файла закончилось");

            foreach (var uri in remoteUris)
            {
                string fileName = $"bigimage{Array.IndexOf(remoteUris, uri)+1}.jpg";
                var downloadTask = downloader.DownloadAsync(uri, fileName, token);
                downloadTasks.Add(downloadTask);
                taskFileNames[downloadTask] = fileName;
            }

            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine("Нажмите клавишу A для выхода или любую другую клавишу для проверки статуса скачивания");
            Console.WriteLine("-------------------------------------------------------------------------------------");
            bool isCancelled = false;
            while (downloadTasks.Count>0)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.A)
                    {
                        cancellationTokenSource.Cancel();
                        Console.WriteLine("-------------------------------------------------------------------------------------");
                        Console.WriteLine("Отмена всех загрузок");
                        Console.WriteLine("-------------------------------------------------------------------------------------");
                        isCancelled = true;
                        break;
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("-------------------------------------------------------------------------------------");
                        Console.WriteLine("Текущий статус загрузок:");
                        Console.WriteLine("-------------------------------------------------------------------------------------");
                        foreach (var kvp in taskFileNames)
                        {
                            Console.WriteLine($"Файл {kvp.Value}, Статус загрузки: {kvp.Key.IsCompleted}");
                        }
                        Console.WriteLine("-------------------------------------------------------------------------------------");
                    }
                }

                var completedTask = await Task.WhenAny(downloadTasks);
                downloadTasks.Remove(completedTask);
            }

            cancellationTokenSource.Dispose();
            if (isCancelled)
            {
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("-------------------------------------------------------------------------------------");
                Console.WriteLine("Скачивание всех файлов завершено.");
                Console.WriteLine("-------------------------------------------------------------------------------------");
            }
        } 
    }
}
