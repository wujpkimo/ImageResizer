using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{
    public class ImageProcess
    {
        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var allImageFiles = Directory.GetFiles(destPath, "*", SearchOption.AllDirectories);

                foreach (var item in allImageFiles)
                {
                    File.Delete(item);
                }
            }
        }

        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("AsyncUsage", "AsyncFixer01:Unnecessary async/await usage", Justification = "<暫止>")]
        public async Task ResizeImagesAsync(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            // New出tasks來存放每筆作業
            List<Task> tasks = new List<Task>();
            foreach (var filePath in allFiles)
            {
                // 每筆作業都使用非用步作法執行
                tasks.Add(Task.Run(() =>
                  {
                      Image imgPhoto = Image.FromFile(filePath);
                      string imgName = Path.GetFileNameWithoutExtension(filePath);
                      System.Console.WriteLine($"Start process {imgName} and Thread ID = {Thread.CurrentThread.ManagedThreadId.ToString("00")} and time is {DateTime.Now}");

                      int sourceWidth = imgPhoto.Width;
                      int sourceHeight = imgPhoto.Height;

                      int destionatonWidth = (int)(sourceWidth * scale);
                      int destionatonHeight = (int)(sourceHeight * scale);

                      Bitmap processedImage = processBitmap((Bitmap)imgPhoto,
                        sourceWidth, sourceHeight,
                        destionatonWidth, destionatonHeight);

                      string destFile = Path.Combine(destPath, imgName + ".jpg");
                      processedImage.Save(destFile, ImageFormat.Jpeg);
                      System.Console.WriteLine($"Processed {imgName} and Thread ID = {Thread.CurrentThread.ManagedThreadId.ToString("00")} and time is {DateTime.Now}");
                  }));
            }
            // 待全數完成
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("AsyncUsage", "AsyncFixer01:Unnecessary async/await usage", Justification = "<暫止>")]
        public async Task ResizeImagesStopOver50msAsync(string sourcePath, string destPath, double scale, CancellationTokenSource cts = default)
        {
            var allFiles = FindImages(sourcePath);

            // New出tasks來存放每筆作業
            List<Task> tasks = new List<Task>();

            foreach (var filePath in allFiles)
            {
                // 每筆作業都使用非用步作法執行
                tasks.Add(Task.Run(() =>
                {
                    if (cts.Token.IsCancellationRequested == true)
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        return;
                    }
                    Image imgPhoto = Image.FromFile(filePath);
                    string imgName = Path.GetFileNameWithoutExtension(filePath);
                    System.Console.WriteLine($"Start process {imgName} and Thread ID = {Thread.CurrentThread.ManagedThreadId.ToString("00")} and time is {DateTime.Now}");

                    int sourceWidth = imgPhoto.Width;
                    int sourceHeight = imgPhoto.Height;

                    int destionatonWidth = (int)(sourceWidth * scale);
                    int destionatonHeight = (int)(sourceHeight * scale);
                    if (cts.Token.IsCancellationRequested == true)
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        return;
                    }

                    if (imgName == "6CxiNWJceow")
                    {
                        Console.WriteLine($"Cancel in processing filename {imgName}");
                        cts.Cancel();
                    }
                    Bitmap processedImage = processBitmap((Bitmap)imgPhoto,
                      sourceWidth, sourceHeight,
                      destionatonWidth, destionatonHeight);

                    string destFile = Path.Combine(destPath, imgName + ".jpg");
                    processedImage.Save(destFile, ImageFormat.Jpeg);
                    System.Console.WriteLine($"Processed {imgName} and Thread ID = {Thread.CurrentThread.ManagedThreadId.ToString("00")} and time is {DateTime.Now}");
                }));
            }
            //ConsoleKeyInfo key = Console.ReadKey();
            //if (key.Key == ConsoleKey.C)
            //{
            //    cts.Cancel();
            //}
            // 待全數完成
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Executor had been canceled!!");
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public List<string> FindImages(string srcPath)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(srcPath, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpeg", SearchOption.AllDirectories));
            return files;
        }

        /// <summary>
        /// 針對指定圖片進行縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        private Bitmap processBitmap(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(resizedbitmap);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(img,
                new Rectangle(0, 0, newWidth, newHeight),
                new Rectangle(0, 0, srcWidth, srcHeight),
                GraphicsUnit.Pixel);
            return resizedbitmap;
        }
    }
}