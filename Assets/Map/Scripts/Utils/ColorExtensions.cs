using UnityEngine;

namespace Nashet.Map.Utils
{
    public static class ColorExtensions
    {
        public static Color getNegative(this Color color)
        {
            return new Color(1.0f - color.r, 1.0f - color.g, 1.0f - color.b);
        }

        public static Color getRandomColor()
        {
            return new Color((float)Rand.Get.NextDouble(), (float)Rand.Get.NextDouble(), (float)Rand.Get.NextDouble(), 1f);
        }

        public static Color setAlphaToZero(this Color color)
        {
            color.a = 0f;
            return color;
        }

        public static Color getAlmostSameColor(this Color color)
        {
            float maxDeviation = 0.02f;//not including

            var result = new Color();
            float deviation = maxDeviation - Rand.getFloat(0f, maxDeviation * 2);
            result.r = color.r + deviation;
            result.g = color.g + deviation;
            result.b = color.b + deviation;

            return result;
        }

        public static bool isSameColorsWithoutAlpha(this Color colorA, Color colorB)
        {
            if (colorA.b == colorB.b && colorA.g == colorB.g && colorA.r == colorB.r)
                return true;
            else
                return false;
        }

        public static Color setAlphaToMax(this Color color)
        {
            color.a = 1f;
            return color;
        }

        public static int ToInt(this Color color)
        { 
        // Pack the color components into a single 32-bit integer
            int packedColor = ((int)(color.r * 255.0f) << 24) | ((int)(color.g * 255.0f) << 16) | ((int)(color.b * 255.0f) << 8) | (int)(color.a * 255.0f);
            return packedColor;
        }

        public static Color ToColor(this int packedColor)
        {
			// Unpack the color components from the packed integer
			float r = (packedColor >> 24) / 255.0f;
			float g = ((packedColor >> 16) & 0xFF) / 255.0f;
			float b = ((packedColor >> 8) & 0xFF) / 255.0f;
			float a = (packedColor & 0xFF) / 255.0f;

			// Create a new Color object from the unpacked components
			Color unpackedColor = new Color(r, g, b, a);
            return unpackedColor;
		}
    }
}