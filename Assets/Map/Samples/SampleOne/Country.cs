using Nashet.Map.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Nashet.Map.Examples
{
	public class Country
	{
		static public readonly HashSet<Country> AllCountries = new HashSet<Country>();

		public Color NationalColor { get; protected set; }
		public Material borderMaterial;
		private string name;
		public Province Capital;

		public Country(Color nationalColor, string name, Material defaultCountryBorderMaterial)
		{
			AllCountries.Add(this);
			this.name = name;
			NationalColor = nationalColor;
			borderMaterial = new Material(defaultCountryBorderMaterial) { color = NationalColor.getNegative() };
		}

		public override string ToString()
		{
			return name;
		}
	}
}