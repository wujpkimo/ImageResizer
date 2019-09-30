using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;

            ImageProcess imageProcess = new ImageProcess();

            imageProcess.Clean(destinationPath);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            System.Console.WriteLine($"Start process and Thread ID = {Thread.CurrentThread.ManagedThreadId.ToString("00")} and time is {DateTime.Now}");
            await imageProcess.ResizeImagesAsync(sourcePath, destinationPath, 2.0);
            System.Console.WriteLine($"Finished all file and Thread ID = {Thread.CurrentThread.ManagedThreadId.ToString("00")} and time is {DateTime.Now}");
            sw.Stop();

            Console.WriteLine($"花費時間: {sw.ElapsedMilliseconds} ms");
        }
    }
}