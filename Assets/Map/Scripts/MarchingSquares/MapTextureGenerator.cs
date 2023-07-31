using Nashet.Map.Utils;
using UnityEngine;

namespace Nashet.MarchingSquares
{ 
	public class MapTextureGenerator : IMapTextureGenerator
    {
        public MyTexture generateMapImage(int width, int heght, int amountOfProvince)
        {
            Texture2D mapImage = new Texture2D(width, heght);        // standard for webGL

            Color emptySpaceColor = Color.black;//.setAlphaToZero();
            mapImage.setColor(emptySpaceColor);
           
			for (int i = 0; i < amountOfProvince; i++)
                mapImage.SetPixel(mapImage.getRandomX(), mapImage.getRandomY(), ColorExtensions.getRandomColor());

            int emptyPixels = 1;//non zero
            Color currentColor = mapImage.GetPixel(0, 0);
            int emergencyExit = 0;
            while (emptyPixels != 0 && emergencyExit < 100)
            {
                emergencyExit++;
                emptyPixels = 0;
                for (int j = 0; j < mapImage.height; j++) // circle by province
                    for (int i = 0; i < mapImage.width; i++)
                    {
                        currentColor = mapImage.GetPixel(i, j);
                        if (currentColor == emptySpaceColor)
                            emptyPixels++;
                        else if (currentColor.a == 1f)
                        {
                            mapImage.drawRandomSpot(i, j, currentColor);
                        }
                    }
                mapImage.setAlphaToMax();
            }
            mapImage.Apply();
            MyTexture mapTexture = new MyTexture(mapImage);
            Texture2D.Destroy(mapImage);
            return mapTexture;
        }
    }
}