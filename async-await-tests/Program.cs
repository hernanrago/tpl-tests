using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace async_await_tests
{
    class Program
    {
        static bool done;
        static readonly string ImageResourcesPath = "/home/hrago/Pictures";
        static readonly HttpClient client = new HttpClient();
        static Random random = new Random();

        static void Main(string[] args)
        {
            Test1();

            while (!done)
            {
                Console.CursorLeft = 0;
                Console.Write(DateTime.Now.ToString("HH:mm:ss.fff"));
                Thread.Sleep(50);
            }

            Console.WriteLine();
            Console.WriteLine("Test 1 Done!");

            // Test2();

            // while (!done)
            // {
            //     Console.CursorLeft = 0;
            //     Console.Write(DateTime.Now.ToString("HH:mm:ss.fff"));
            //     Thread.Sleep(50);
            // }

            // Console.WriteLine();
            // Console.WriteLine("Test 2 Done!");
        }

        static Task<byte[]> DownloadImage(string url)
        {
            return client.GetAsync(url)
            .ContinueWith(t1 =>
            {
                var result = t1.Result;
                result.EnsureSuccessStatusCode();
                return result.Content.ReadAsByteArrayAsync().ContinueWith(t2 => { return t2.Result; });
            })
            .Result;
        }

        static Task<byte[]> CopyImage(string imagePath)
        {
            return File.ReadAllBytesAsync(imagePath);
        }

        static Task SaveImage(byte[] bytes, string imagePath)
        {
            File.WriteAllBytes(imagePath, bytes);
            return Task.CompletedTask;
        }

        static async Task Test1()
        {
            done = false;
            string url = $"https://assets.pokemon.com/assets/cms2/img/pokedex/detail/{random.Next(001, 898).ToString("D3")}.png";
            string fileName = Path.GetFileName(url);
            byte[] originalImageBytes = await DownloadImage(url);
            string originalImagePath = Path.Combine(ImageResourcesPath, fileName);
            await SaveImage(originalImageBytes, originalImagePath);
            byte[] copyImageBytes = await CopyImage(originalImagePath);
            string copyFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_copy.png";
            string copyImagePath = Path.Combine(ImageResourcesPath, copyFileName);
            await SaveImage(copyImageBytes, copyImagePath);
            done = true;
        }

        static Task Test2()
        {
            done = false;
            var saveCopyImageTasks = new List<Task>();
            var images = random.Next(1, 10);

            for (int i = images; i > 0; i--)
            {
                var url = $"https://assets.pokemon.com/assets/cms2/img/pokedex/detail/{random.Next(001, 898).ToString("D3")}.png";
                var fileName = Path.GetFileName(url);
                DownloadImage(url).ContinueWith(task1 =>
                {
                    var originalImageBytes = task1.Result;
                    var originalImagePath = Path.Combine("/home/hrago/Pictures", fileName);
                    SaveImage(originalImageBytes, originalImagePath).ContinueWith(task2 =>
      {
          CopyImage(originalImagePath).ContinueWith(task3 =>
{
    var copyImageBytes = task3.Result;
    var copyFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_copy.png";
    var copyImagePath = Path.Combine("/home/hrago/Pictures", copyFileName);
    var saveCopyImageTask = SaveImage(copyImageBytes, copyImagePath);
    saveCopyImageTasks.Add(saveCopyImageTask);
    if (saveCopyImageTasks.Count == images)
    {
        Task.WhenAll(saveCopyImageTasks).ContinueWith(finalTask =>
      {
          done = true;
      });
    }
});
      });
                });
            }
            return Task.CompletedTask;
        }
    }
}