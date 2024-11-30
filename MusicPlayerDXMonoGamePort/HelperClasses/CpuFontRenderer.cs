using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MusicPlayerDXMonoGamePort.HelperClasses
{
    public class CpuFontRenderer(Font font)
    {
        Bitmap tmpTextBitmap = null, textBitmap = null;

        private const int superResoultion = 2;

        public void RenderToTexture(Texture2D tex, string textToDraw, int startPosX = 0, int startPosY = 0, SolidBrush color = null)
        {
            color ??= new(Color.White);

            tmpTextBitmap?.Dispose();
            tmpTextBitmap = new Bitmap(tex.Width * superResoultion, tex.Height * superResoultion, PixelFormat.Format64bppArgb);
            using Graphics g = Graphics.FromImage(tmpTextBitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            using Font tmpFont = new(font.FontFamily, font.Size * superResoultion * (96 / g.DpiX));
            g.DrawString(textToDraw, tmpFont, color, startPosX * superResoultion, startPosY * superResoultion);
            g.Flush();
            textBitmap?.Dispose();
            textBitmap = ResizeImage(tmpTextBitmap, tmpTextBitmap.Width / superResoultion, tmpTextBitmap.Height / superResoultion);

            DrawBitmapToTexture(tex, textBitmap);
        }

        public SizeF MeasureString(string textToMeasure)
        {
            using Bitmap tmp = new(1, 1);
            using Graphics g = Graphics.FromImage(tmp);
            var size = g.MeasureString(textToMeasure, font);
            return new SizeF(size.Width / superResoultion, size.Height / superResoultion);
        }

        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using var graphics = Graphics.FromImage(destImage);
            
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var wrapMode = new ImageAttributes();
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            
            return destImage;
        }

        private static void DrawBitmapToTexture(Texture2D tex, Bitmap bitmap)
        {
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            int bufferSize = data.Height * data.Stride;
            byte[] bytes = new byte[bufferSize];

            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            try { tex.SetData(bytes); } catch { }

            bitmap.UnlockBits(data);
        }
    }
}
