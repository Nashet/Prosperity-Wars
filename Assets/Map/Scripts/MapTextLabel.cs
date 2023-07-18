using Nashet.EconomicSimulation;
using UnityEngine;

namespace Nashet.MapMeshes
{
	public class MapTextLabel
	{		
		public static void CreateMapTextLabel(GameObject GameObject, string text, Color color, Vector3 _position)
		{
			LODGroup group = GameObject.AddComponent<LODGroup>();

			// Add 4 LOD levels
			LOD[] lods = new LOD[1];
			var txtMeshGl = GameObject.Instantiate(LinksManager.Get.r3DProvinceTextPrefab);
			Transform txtMeshTransform = txtMeshGl.transform;
			txtMeshTransform.SetParent(GameObject.transform, false);
			Renderer[] renderers = new Renderer[1];
			renderers[0] = txtMeshTransform.GetComponent<Renderer>();
			lods[0] = new LOD(0.25F, renderers);

			var position = _position;
			position.z -= 0.003f;
			txtMeshTransform.position = position;

			TextMesh txtMesh = txtMeshTransform.GetComponent<TextMesh>();

			txtMesh.text = text;
			txtMesh.color = color;

			//renderers[0].material.shader = Shader.Find("3DText");


			group.SetLODs(lods);
			//#if UNITY_WEBGL
			group.size = 20;
		}

		public static TextMesh setCapitalTextMesh(GameObject gameObject, Vector3 position, string text, Color color, int fontSize)
		{
			Transform txtMeshTransform = GameObject.Instantiate(LinksManager.Get.r3DCountryTextPrefab).transform;
			txtMeshTransform.SetParent(gameObject.transform, false);

			Vector3 capitalTextPosition = position;
			capitalTextPosition.y += 2f;
			//capitalTextPosition.z -= 5f;
			txtMeshTransform.position = capitalTextPosition;

			var meshCapitalText = txtMeshTransform.GetComponent<TextMesh>();
			meshCapitalText.text = text;
			// meshCapitalText.fontSize *= 2;
			
			meshCapitalText.color = color;
			meshCapitalText.fontSize = fontSize;
			
			return meshCapitalText;
		}
	}
}
