using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
	internal class StripesMaterialHelper : SingletonMonoBehaviour<StripesMaterialHelper>
	{
		[SerializeField] Material stripesPrefab;

		private Dictionary<Color, Dictionary<Color, Material>> stripesMaterials = new Dictionary<Color, Dictionary<Color, Material>> { };
		internal Material Get(Color mainColor, Color secondColor)
		{
			var hasMaincolorInDictionary = false;
			if (stripesMaterials.TryGetValue(mainColor, out var pairs))
			{
				hasMaincolorInDictionary = true;
				if (pairs.TryGetValue(secondColor, out var material))
				{
					return material;
				}
			}
			var newMaterial = new Material(stripesPrefab);
			newMaterial.SetColor("_StripeColor1", mainColor);
			newMaterial.SetColor("_StripeColor2", secondColor);
			if (hasMaincolorInDictionary)
			{
				pairs.Add(secondColor, newMaterial);
			}
			else
			{
				stripesMaterials.Add(mainColor, new Dictionary<Color, Material> { { secondColor, newMaterial } });
			}
			
			return newMaterial;
		}
	}
}