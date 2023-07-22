using Nashet.Map.Utils;
using System;
using UnityEngine;

namespace Nashet.FlagGeneration
{
	enum StripesDirection { horizontal, vertical }
	public class FlagGenerator
	{
		public static Texture2D Generate(int textureWidth, int textureHeight)
		{
			var stripes = new ChanceBox<int>();
			stripes.Add(1, 0.08f);
			stripes.Add(2, 0.3f);
			stripes.Add(3, 0.5f);
			stripes.Add(4, 0.05f);
			//stripes.Add(8, 0.01f);
			stripes.Initiate();

			int stripesAmount = stripes.GetRandom();
			int stripeSize = textureWidth / stripesAmount;
			var res = new Texture2D(textureWidth, textureHeight);


			int stripeNumber = 0;
			var color = ColorExtensions.getRandomColor();

			Array values = Enum.GetValues(typeof(StripesDirection));

			StripesDirection stripeDirection = (StripesDirection)values.GetValue(Rand.Get.Next(values.Length));


			if (stripeDirection == StripesDirection.vertical)
			//Vertical stripes
			{
				for (int x = 0; x < textureWidth; x++)
				{
					if (x > stripeSize * stripeNumber)
					{
						stripeNumber++;
						color = ColorExtensions.getRandomColor();
					}
					for (int y = 0; y < textureHeight; y++)
					{
						res.SetPixel(x, y, color);
					}
				}
			}
			else
			{
				for (int y = 0; y < textureHeight; y++)
				{
					if (y > stripeSize * stripeNumber)
					{
						stripeNumber++;
						color = ColorExtensions.getRandomColor();
					}
					for (int x = 0; x < textureHeight; x++)
					{
						res.SetPixel(x, y, color);
					}
				}
			}
			res.Apply();
			return res;
		}
	}
}
