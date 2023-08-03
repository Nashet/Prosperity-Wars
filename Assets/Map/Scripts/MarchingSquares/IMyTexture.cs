using System.Collections.Generic;
using UnityEngine;

namespace Nashet.MarchingSquares
{
	public interface IMyTexture
	{
		List<Color> AllUniqueColorsVictoriaFormat();
		HashSet<Color> AllUniqueColors3();
		HashSet<Color> GetColorsFromBorder();
		int getHeight();
		Color GetPixel(int x, int v);
		int getWidth();
	}
}