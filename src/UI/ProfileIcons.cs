using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RiotSwitcherAccount;

internal static class ProfileIcons
{
    private static readonly string Folder = Path.Combine(AppPaths.DataRoot, "icons");

    private static string ImagePath(string id)
    {
        if (!Guid.TryParseExact(id, "N", out _))
            throw new InvalidOperationException("Invalid account identifier.");
        return Path.Combine(Folder, id + ".png");
    }

    public static ImageSource? Load(string id)
    {
        try
        {
            var path = ImagePath(id);
            if (!File.Exists(path)) return null;
            byte[] image = File.ReadAllBytes(path);
            if (image.Length > 1_000_000) return null;
            using var stream = new MemoryStream(image);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = 256;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch { return null; }
    }

    public static void Import(string id, string source)
    {
        var extension = Path.GetExtension(source).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png")) throw new InvalidDataException("Only PNG and JPEG images are supported.");
        var info = new FileInfo(source);
        if (info.Length is < 1 or > 1_000_000) throw new InvalidDataException("Image too large (max 1 MB).");
        var buffer = File.ReadAllBytes(source);
        ImportDownloaded(id, buffer);
    }

    public static void ImportDownloaded(string id, byte[] buffer)
    {
        if (buffer.Length is < 1 or > 1_500_000)
            throw new InvalidDataException("Image exceeds size limit.");
        using var stream = new MemoryStream(buffer, false);
        var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnDemand);
        if (decoder is not (PngBitmapDecoder or JpegBitmapDecoder)) throw new InvalidDataException("PNG or JPEG required.");
        if (decoder.Frames.Count == 0) throw new InvalidDataException("No image frame found.");
        BitmapFrame frame = decoder.Frames[0];
        if (frame.PixelWidth < 1 || frame.PixelHeight < 1 ||
            frame.PixelWidth > 2048 || frame.PixelHeight > 2048 ||
            (long)frame.PixelWidth * frame.PixelHeight > 4_000_000)
            throw new InvalidDataException("Image dimensions exceed 2048 × 2048.");

        const int targetSize = 256;
        double scale = Math.Min((double)targetSize / frame.PixelWidth, (double)targetSize / frame.PixelHeight);
        double width = frame.PixelWidth * scale;
        double height = frame.PixelHeight * scale;
        var visual = new DrawingVisual();
        using (var ctx = visual.RenderOpen())
            ctx.DrawImage(frame, new Rect((targetSize - width) / 2, (targetSize - height) / 2, width, height));
        var resized = new RenderTargetBitmap(targetSize, targetSize, 96, 96, PixelFormats.Pbgra32);
        resized.Render(visual);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(resized));
        Directory.CreateDirectory(Folder);
        var output = ImagePath(id);
        var temp = output + ".tmp";
        try
        {
            using (var file = File.Create(temp)) encoder.Save(file);
            File.Move(temp, output, true);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }

    public static void Remove(string id)
    {
        var path = ImagePath(id);
        if (File.Exists(path)) File.Delete(path);
    }
}
