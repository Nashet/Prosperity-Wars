using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Nashet.Utils
{
    //public static class TextureExtensions
    //{
    //    public static bool IsEmpty(Texture2D original)
    //    {
    //    }
    //}
    public static class Texture2DExtensions
    {
        public static List<Color> AllUniqueColors(this Texture2D image)
        {
            var res = new List<Color>();
            
            Color currentProvinceColor = image.GetPixel(0, 0);
            
            for (int y = 0; y < image.height; y++) // circle by province
                for (int x = 0; x < image.width; x++)
                {
                    if (currentProvinceColor != image.GetPixel(x, y)                        
                        && !res.Contains( currentProvinceColor))
                    {
                            res.Add(currentProvinceColor);            
                    }
                    currentProvinceColor = image.GetPixel(x, y);
                    
                }
            return res;
        }
        public static Texture2D LoadPNG(string filePath)
        {

            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2, TextureFormat.RGBAFloat, false);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            return tex;
        }
        public static Texture2D FlipTexture(Texture2D original)
        {
            Texture2D flipped = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;

            for (int i = 0; i < xN; i++)
            {
                for (int j = 0; j < yN; j++)
                {
                    flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
                }
            }
            flipped.Apply();

            return flipped;
        }

        public static bool isDifferentColor(this Texture2D image, int thisx, int thisy, int x, int y)
        {
            if (image.GetPixel(thisx, thisy) != image.GetPixel(x, y))
                return true;
            else
                return false;
        }

        public static void setColor(this Texture2D image, Color color)
        {
            for (int j = 0; j < image.height; j++) // cicle by province
                for (int i = 0; i < image.width; i++)
                    image.SetPixel(i, j, color);
        }

        public static void setAlphaToMax(this Texture2D image)
        {
            for (int j = 0; j < image.height; j++) // cicle by province
                for (int i = 0; i < image.width; i++)
                    // if (image.GetPixel(i, j) != Color.black)
                    image.SetPixel(i, j, image.GetPixel(i, j).setAlphaToMax());
        }

        private static void drawSpot(Texture2D image, int x, int y, Color color)
        {
            int straightBorderChance = 4;// 5;
                                         //if (x >= 0 && x < image.width && y >= 0 && y < image.height)

            if (image.coordinatesExist(x, y))
                if (Rand.Get.Next(straightBorderChance) != 1)
                    //if (image.GetPixel(x, y).a != 1f || image.GetPixel(x, y) == Color.black)
                    if (image.GetPixel(x, y) == Color.black)
                        image.SetPixel(x, y, color.setAlphaToZero());
        }

        public static bool coordinatesExist(this Texture2D image, int x, int y)
        {
            return (x >= 0 && x < image.width && y >= 0 && y < image.height);
        }

        public static bool isRightTopCorner(this Texture2D image, int x, int y)
        {
            if (image.coordinatesExist(x + 1, y) && image.GetPixel(x + 1, y) != image.GetPixel(x, y)
                && image.coordinatesExist(x, y + 1) && image.GetPixel(x, y + 1) != image.GetPixel(x, y)
                && image.coordinatesExist(x, y - 1) && image.GetPixel(x, y - 1) == image.GetPixel(x, y)
                && image.coordinatesExist(x - 1, y) && image.GetPixel(x - 1, y) == image.GetPixel(x, y)
                )
                return true;
            else
                return false;
        }

        public static bool isRightBottomCorner(this Texture2D image, int x, int y)
        {
            if (image.coordinatesExist(x + 1, y) && image.GetPixel(x + 1, y) != image.GetPixel(x, y)
                && image.coordinatesExist(x, y - 1) && image.GetPixel(x, y - 1) != image.GetPixel(x, y)
                && image.coordinatesExist(x, y + 1) && image.GetPixel(x, y + 1) == image.GetPixel(x, y)
                && image.coordinatesExist(x - 1, y) && image.GetPixel(x - 1, y) == image.GetPixel(x, y)
                )
                return true;
            else
                return false;
        }

        public static bool isLeftTopCorner(this Texture2D image, int x, int y)
        {
            if (image.coordinatesExist(x - 1, y) && image.GetPixel(x - 1, y) != image.GetPixel(x, y)
                && image.coordinatesExist(x, y + 1) && image.GetPixel(x, y + 1) != image.GetPixel(x, y)
                && image.coordinatesExist(x, y - 1) && image.GetPixel(x, y - 1) == image.GetPixel(x, y)
                && image.coordinatesExist(x + 1, y) && image.GetPixel(x + 1, y) == image.GetPixel(x, y)
                )
                return true;
            else
                return false;
        }

        public static bool isLeftBottomCorner(this Texture2D image, int x, int y)
        {
            if (image.coordinatesExist(x - 1, y) && image.GetPixel(x - 1, y) != image.GetPixel(x, y)
                && image.coordinatesExist(x, y - 1) && image.GetPixel(x, y - 1) != image.GetPixel(x, y)
                && image.coordinatesExist(x, y + 1) && image.GetPixel(x, y + 1) == image.GetPixel(x, y)
                && image.coordinatesExist(x + 1, y) && image.GetPixel(x + 1, y) == image.GetPixel(x, y)
                )
                return true;
            else
                return false;
        }

        public static void drawRandomSpot(this Texture2D image, int x, int y, Color color)
        {
            //draw 4 points around x, y
            //int chance = 90;
            drawSpot(image, x - 1, y, color);
            drawSpot(image, x + 1, y, color);
            drawSpot(image, x, y - 1, color);
            drawSpot(image, x, y + 1, color);
        }

        public static int getRandomX(this Texture2D image)
        {
            return Rand.Get.Next(0, image.width);
        }

        public static Color getRandomPixel(this Texture2D image)
        {
            return image.GetPixel(image.getRandomX(), image.getRandomY());
        }

        public static int getRandomY(this Texture2D image)
        {
            return Rand.Get.Next(0, image.height);
        }
    }
}