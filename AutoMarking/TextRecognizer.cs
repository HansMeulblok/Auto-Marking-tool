using System;
using System.Drawing;
using System.IO;
using Tesseract;

public static class TextRecognizer
{
    private static string TesseractDataPath = @"Resources/tessdata-main";

    public static string ExtractTextFromImage(Bitmap image)
    {
        try
        {
            // Convert Bitmap to Pix
            using (Pix pix = BitmapToPix(image))
            {
                using (var engine = new TesseractEngine(TesseractDataPath, "eng", EngineMode.Default))
                {
                    using (var page = engine.Process(pix))
                    {
                        string text = page.GetText();
                        Console.WriteLine($"Confidence: {page.GetMeanConfidence()}");
                        return text;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during OCR: {ex.Message}");
            return string.Empty;
        }
    }

    private static Pix BitmapToPix(Bitmap image)
    {
        if (image == null) throw new ArgumentNullException(nameof(image));

        // Save the Bitmap as a temporary file
        string tempFilePath = Path.GetTempFileName();
        image.Save(tempFilePath, System.Drawing.Imaging.ImageFormat.Png);

        // Load the image from the temporary file as a Pix object
        Pix pix = Pix.LoadFromFile(tempFilePath);

        // Delete the temporary file
        File.Delete(tempFilePath);

        return pix;
    }
}
