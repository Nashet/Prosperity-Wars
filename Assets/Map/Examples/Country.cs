using System.Collections.Generic;
using UnityEngine;

namespace Nashet.Map.Examples
{
	public class Country
	{
		static public readonly HashSet<Country> AllCountries = new HashSet<Country>();

		public Color NationalColor { get; protected set; }
		private Material borderMaterial;
		private string name;
		public Province Capital;

		public Country(Color nationalColor, string name)
		{
			NationalColor = nationalColor;
			this.name = name;
			AllCountries.Add(this);
		}

		public override string ToString()
		{
			return name;
		}
	}
}