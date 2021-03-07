using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace task_run_tests
{
    class Program
    {
        static bool done;
        static readonly string ImageResourcesPath = "/home/hrago/Pictures";
        static readonly HttpClient client = new HttpClient();
        static Random random = new Random();
        static Stopwatch stopWatch = new Stopwatch();


        static void Main(string[] args)
        {
            var task = Test();
            
            while(!done){}

            Console.CursorLeft = 0;
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff"));
            Thread.Sleep(50);

            Console.WriteLine();
            Console.WriteLine("Test Done!");
        }

        static Task<byte[]> DownloadImage(string url)
        {
            return client.GetByteArrayAsync(url);
        }

        static async Task<byte[]> BlurImage(string imagePath)
        {
        return await Task.Run(() =>
        {
            var image = Image.Load(imagePath);
            image.Mutate(ctx => ctx.GaussianBlur());
            using (var memoryStream = new MemoryStream())
            {
            image.SaveAsPng(memoryStream);
            return memoryStream.ToArray();
            }
        });
        }

        static async Task<byte[]> BlurImage2(string imagePath)
        {

            var image = await Image.LoadAsync(imagePath);
            image.Mutate(ctx => ctx.GaussianBlur());
            using (var memoryStream = new MemoryStream())
            {
                image.SaveAsPng(memoryStream);
                return memoryStream.ToArray();
            }
        }

        static Task SaveImage(byte[] bytes, string imagePath)
        {
            return File.WriteAllBytesAsync(imagePath, bytes);
        }

        static async Task Test()
        {
            done = false;
            string url = $"https://assets.pokemon.com/assets/cms2/img/pokedex/detail/{random.Next(001, 898).ToString("D3")}.png";
            string fileName = Path.GetFileName(url);
            Console.WriteLine("Downloading...");
            byte[] originalImageBytes = await DownloadImage(url);
            string originalImagePath = Path.Combine(ImageResourcesPath, fileName);

            Console.WriteLine("Saving...");
            await SaveImage(originalImageBytes, originalImagePath);
            
            Console.WriteLine("Blurring 1...");
            Func<string, Task<byte[]>> blur = BlurImage;
            byte[] blurredImageBytes = await MeasureTime(blur, originalImagePath);
            string blurredFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_blurred.png";
            string blurredImagePath = Path.Combine(ImageResourcesPath, blurredFileName);

            Console.WriteLine("Saving blurred...");
            await SaveImage(blurredImageBytes, blurredImagePath);

            Console.WriteLine("Blurring 2...");
            blur = BlurImage2;
            blurredImageBytes = await MeasureTime(blur, originalImagePath);
            blurredFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_blurred2.png";
            blurredImagePath = Path.Combine(ImageResourcesPath, blurredFileName);

            Console.WriteLine("Saving blurred 2...");
            await SaveImage(blurredImageBytes, blurredImagePath);

            done = true;
        }

        static async Task<byte[]> MeasureTime(Func<string, Task<byte[]>> selector, string path)
        {
            stopWatch.Start();
            var task = await selector.Invoke(path);
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);

            return task;
        }
    }
}