using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DeepLearningDemo.MarioKart
{
    static class ImageUtil
    {
        /// <summary>
        /// Extracts image pixels in CHW using parallelization
        /// </summary>
        /// <param name="image">The bitmap image to extract features from</param>
        /// <returns>A list of pixels in CHW order</returns>
        public static List<float> ParallelExtractCHW(this Bitmap image, bool makeGrayScale = false)
        {
            int channelStride = image.Width * image.Height;
            int imageWidth = image.Width;
            int imageHeight = image.Height;
            int imageDepth = makeGrayScale ? 1 : 3;

            var features = new byte[imageWidth * imageHeight * imageDepth];
            var bitmapData = image.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.ReadOnly, image.PixelFormat);
            IntPtr ptr = bitmapData.Scan0;
            int bytes = Math.Abs(bitmapData.Stride) * bitmapData.Height;
            byte[] rgbValues = new byte[bytes];

            int stride = bitmapData.Stride;

            Marshal.Copy(ptr, rgbValues, 0, bytes);

            Func<int, int, int, int> mapPixel = GetPixelMapper(image.PixelFormat, stride);

            Parallel.For(0, imageHeight, (int h) =>
            {
                Parallel.For(0, imageWidth, (int w) =>
                {
                    if (imageDepth == 1)
                        features[imageWidth * h + w] = rgbValues[mapPixel(h, w, 0)];
                    else
                    {
                        Parallel.For(0, 3, (int c) =>
                        {
                            features[channelStride * c + imageWidth * h + w] = rgbValues[mapPixel(h, w, c)];
                        });
                    }

                });
            });

            image.UnlockBits(bitmapData);

            return features.Select(b => (float)b).ToList();
        }

        /// <summary>
        /// Returns a function for extracting the R-G-B values properly from an image based on its pixel format
        /// </summary>
        /// <param name="pixelFormat">The image's pixel format</param>
        /// <param name="heightStride">The stride (row byte count)</param>
        /// <returns>A function with signature (height, width, channel) returning the corresponding color value</returns>
        private static Func<int, int, int, int> GetPixelMapper(PixelFormat pixelFormat, int heightStride)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    return (h, w, c) => h * heightStride + w * 4 + c;  // bytes are B-G-R-A
                case PixelFormat.Format24bppRgb:
                default:
                    return (h, w, c) => h * heightStride + w * 3 + c;  // bytes are B-G-R
            }
        }

        /// <summary>
        /// Resizes an image
        /// </summary>
        /// <param name="image">The image to resize</param>
        /// <param name="width">New width in pixels</param>
        /// <param name="height">New height in pixesl</param>
        /// <param name="useHighQuality">Resize quality</param>
        /// <returns>The resized image</returns>
        public static Bitmap Resize(this Bitmap image, int width, int height, bool useHighQuality)
        {
            var newImg = new Bitmap(width, height);

            newImg.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var g = Graphics.FromImage(newImg))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                if (useHighQuality)
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                }
                else
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.Default;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;
                }

                var attributes = new ImageAttributes();
                attributes.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                g.DrawImage(image, new System.Drawing.Rectangle(0, 0, width, height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }

            return newImg;
        }

        /// <summary>
        /// Convert to grayscale
        /// https://stackoverflow.com/questions/2265910/convert-an-image-to-grayscale
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Bitmap MakeGrayscale2(this Bitmap original)
        {
            var newBitmap = new Bitmap(original.Width, original.Height);

            using (var g = Graphics.FromImage(newBitmap))
            {
                var colorMatrix = 
                    new ColorMatrix(new float[][]
                    {
                          new float[] {.3f, .3f, .3f, 0, 0},
                          new float[] {.59f, .59f, .59f, 0, 0},
                          new float[] {.11f, .11f, .11f, 0, 0},
                          new float[] {0, 0, 0, 1, 0},
                          new float[] {0, 0, 0, 0, 1}
                    });

                var attributes = new ImageAttributes();

                attributes.SetColorMatrix(colorMatrix);

                g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                   0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            }

            return newBitmap;
        }

        public static Bitmap MakeGrayscale(Bitmap original)
        {
            var newBitmap = new Bitmap(original.Width, original.Height);

            for (int i = 0; i < original.Width; i++)
            {
                for (int j = 0; j < original.Height; j++)
                {
                    var originalColor = original.GetPixel(i, j);

                    int grayScale = (int)((originalColor.R * .3) + (originalColor.G * .59)
                        + (originalColor.B * .11));

                    var newColor = Color.FromArgb(grayScale, grayScale, grayScale);

                    newBitmap.SetPixel(i, j, newColor);
                }
            }

            return newBitmap;
        }
    }
}