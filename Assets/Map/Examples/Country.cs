using UnityEngine;

namespace Nashet.Map.Examples
{
	public class Country
	{
		public Color NationalColor { get; protected set; }
		private Material borderMaterial;
		private string name;

		public Country(Color nationalColor, string name)
		{
			NationalColor = nationalColor;
			this.name = name;
		}
	}
}