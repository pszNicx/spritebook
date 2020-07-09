using System;
using System.Diagnostics;
using System.Drawing;
using Windows = System.Windows;

namespace SpriteBook
{
    public static class SpriteGenerator
    {
        public static void GenerateSpriteSheet(string[] paths, string outPath, float scale, bool requireSameSizeImages = true, bool requirePowerOfTwo = false, bool restrictTo2048X2048 = false)
        {
            Bitmap targetImage = null;

            try
            {
                var maxSpriteSize = Size.Empty;
                var horizontalResolution = 0f;
                var verticalResolution = 0f;
                foreach (var path in paths)
                {
                    var spriteSize = GetImageSize(path, out horizontalResolution, out verticalResolution);
                    if (maxSpriteSize != Size.Empty)
                    {
                        if (requireSameSizeImages && maxSpriteSize != spriteSize)
                            throw new SpriteSizeException();
                        maxSpriteSize = new Size(Math.Max(maxSpriteSize.Width, spriteSize.Width), Math.Max(maxSpriteSize.Height, spriteSize.Height));
                    }
                    else
                        maxSpriteSize = spriteSize;
                }

                // Create the target image
                int rows, columns;
                var targetSize = CalculateTargetSize(maxSpriteSize, paths.Length, out columns, out rows);
                targetImage = new Bitmap(targetSize.Width, targetSize.Height);
                targetImage.SetResolution(horizontalResolution, verticalResolution);
                var graphics = Graphics.FromImage(targetImage);

                int row = 0, column = 0;
                foreach (var path in paths)
                {
                    using (var image = Image.FromFile(path))
                    {
                        // Draw the current image to the target
                        graphics.DrawImage(image, column * maxSpriteSize.Width, row * maxSpriteSize.Height);

                        // Move to next row/column
                        if (column >= columns - 1)
                        {
                            column = 0;
                            row++;
                        }
                        else
                            column++;
                    }
                }

                targetImage.Save(outPath);
                Windows.MessageBox.Show("Sprite sheet successfully written.", "Saved");
            }
            finally
            {
                targetImage?.Dispose();
            }
        }

        /// <summary>
        /// Calculates the number of columns in such a way that the the remaining space after adding
        /// power-of-two padding is less than the width of a single frame at the current size and half size
        /// </summary>
        private static int CalculateColumns(int defaultColumnCount, int frameWidth, bool restrictTo2048X2048 = false)
        {
            // Reduce just to attempt to find a less wide match
            if (defaultColumnCount > 2)
                defaultColumnCount -= 1;
            while ((GetNextPowerOfTwo(defaultColumnCount * frameWidth) - (defaultColumnCount * frameWidth) >= frameWidth)
                   || (GetNextPowerOfTwo(defaultColumnCount * Convert.ToInt32(frameWidth * 0.5))
                   - (defaultColumnCount * Convert.ToInt32(frameWidth * 0.5)) >= Convert.ToInt32(frameWidth * 0.5)))
                defaultColumnCount++;
            if (restrictTo2048X2048 && defaultColumnCount * frameWidth > 2048)
                throw new SpriteSizeException("Sprite sheet width greater than 2048!!");
            return defaultColumnCount;
        }

        /// <summary>
        /// Calculates the desired final target size of the sprite sheet.
        /// </summary>
        private static Size CalculateTargetSize(Size spriteSize, int totalImageCount, out int totalColumns, out int totalRows, bool restrictTo2048X2048 = false)
        {
            // Calculate the size of the target image
            totalColumns = SpriteGenerator.CalculateColumns(Convert.ToInt32(Math.Sqrt(totalImageCount)), spriteSize.Width, restrictTo2048X2048);
            totalRows = totalImageCount / totalColumns;
            totalRows += (totalImageCount % totalColumns) > 0 ? 1 : 0;
            return new Size(totalColumns * spriteSize.Width, totalRows * spriteSize.Height);
        }

        /// <summary>
        /// Gets the size and resolution of the given image file.
        /// </summary>
        private static Size GetImageSize(string path, out float horizontalResolution, out float verticalResolution)
        {
            using (var image = Image.FromFile(path))
            {
                horizontalResolution = image.HorizontalResolution;
                verticalResolution = image.VerticalResolution;
                return new Size(image.Width, image.Height);
            }
        }

        private static int GetNextPowerOfTwo(int value)
        {
            Debug.Assert(value > 0);
            var computedValue = 1;
            while (value > computedValue)
                computedValue *= 2;
            return computedValue;
        }
    }
}