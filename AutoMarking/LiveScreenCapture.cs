using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

public class LiveScreenCapture
{
    private static Thread? captureThread;
    private static bool isCapturing = false;

    public static void StartLiveScreenCapture(int screenIndex)
    {
        if (isCapturing) return;

        isCapturing = true;
        captureThread = new Thread(() =>
        {
            while (isCapturing)
            {
                Bitmap screenshot = CaptureScreen(screenIndex);

                Console.WriteLine("Captured screen at: " + DateTime.Now);

                // You can save the screenshot or process it here

                Thread.Sleep(1000); // capture every second
            }
        });

        captureThread.Start();
    }

    public static void StopLiveScreenCapture()
    {
        if (captureThread != null && captureThread.IsAlive)
        {
            isCapturing = false;
            captureThread.Join(); // Wait for the capture thread to finish
        }
    }

    public static Bitmap CaptureScreen(int screenIndex)
    {
        if (screenIndex < 0 || screenIndex >= Screen.AllScreens.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(screenIndex), "Invalid screen index.");
        }

        Rectangle bounds = Screen.AllScreens[screenIndex].Bounds;

        Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
        }

        return bitmap;
    }

}
