using Nashet.Map.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nashet.MarchingSquares
{
	public class MyTexture : IMyTexture
    {
        private readonly int width, height;
        private readonly Color[] map;
       //public Texture2D Texture { get; private set; }

        public MyTexture(Texture2D image)
        {
            width = image.width;
            height = image.height;
            map = image.GetPixels();
            //Texture = image;
		}

        public int getWidth()
        {
            return width;
        }

        public int getHeight()
        {
            return height;
        }

        public Color GetPixel(int x, int v)
        {
            return map[x + v * width];
        }

        public Color getRandomPixel()
        {
            return map[Rand.Get.Next((width * height) - 1)];
        }
        public List<Color> AllUniqueColorsVictoriaFormat()
        {
            var res = new List<Color>();
            //ProvinceNameGenerator nameGenerator = new ProvinceNameGenerator();
            Color nextColor = map[0];

            for (int i = 0; i < map.Length; i++)
            {
                if (nextColor != map[i]
                    && !res.Contains(nextColor))
                {
                    res.Add(nextColor);
                }
                nextColor = map[i];

            }
            return res;
        }
        
        public HashSet<Color> GetColorsFromBorder()
        {
            var res = new HashSet<Color>();
            Color nextColor = map[0];

            for (int y = 0; y < height; y++)
            {
                if (nextColor != map[y * width])
                {

                    res.Add(nextColor);
                }
                nextColor = map[y * width];
            }

            for (int y = 0; y < height; y++)
            {
                if (nextColor != map[width - 1 + y * width])
                {

                    res.Add(nextColor);
                }
                nextColor = map[width - 1 + y * width];
            }

            for (int x = 0; x < width; x++)
            {
                if (nextColor != map[x])
                {

                    res.Add(nextColor);
                }
                nextColor = map[x];
            }

            for (int x = 0; x < width; x++)
            {
                if (nextColor != map[x + (height - 1) * width])
                {

                    res.Add(nextColor);
                }
                nextColor = map[x + (height - 1) * width];
            }


            return res;
        }

		public HashSet<Color> AllUniqueColors3()
		{			
			var res = new HashSet<Color>();
			Color nextColor = map[0];			

			for (int y = 1; y < height - 1; y++)
				for (int x = 1; x < width - 1; x++)
				{
					if (nextColor != map[x + y * width])
					{
						res.Add(nextColor);
					}
					nextColor = map[x + y * width];
				}
			return res;
		}

		public int CountPixels(Color color)
		{
            int size = 0;
			for (int x = 0; x < map.Length; x++)
                if (map[x] == color)
                    size++;
            return size;
		}
	}
}