using UnityEngine;

namespace Nashet.MapMeshes
{
	public class MapTextLabel
	{		
		public static GameObject CreateMapTextLabel(GameObject GameObject, Vector3 position, string text, Color color)
		{
			// Add 4 LOD levels
			LOD[] lods = new LOD[1];
			var txtMeshGl = GameObject.Instantiate(GameObject);
			LODGroup group = txtMeshGl.AddComponent<LODGroup>();
			Transform txtMeshTransform = txtMeshGl.transform;
			
			Renderer[] renderers = new Renderer[1];
			renderers[0] = txtMeshTransform.GetComponent<Renderer>();
			lods[0] = new LOD(0.25F, renderers);

			var _position = position;
			_position.z -= 0.12f; //Overwise it would be covered by province border quad
			txtMeshTransform.position = _position;

			TextMesh txtMesh = txtMeshTransform.GetComponent<TextMesh>();

			txtMesh.text = text;
			txtMesh.color = color;

			group.SetLODs(lods);
			group.size = 20;
			return txtMeshGl;
		}

		public static TextMesh CreateMapTextLabel(GameObject prefab, Vector3 position, string text, Color color, int fontSize)
		{
			Transform txtMeshTransform = GameObject.Instantiate(prefab).transform;			

			Vector3 capitalTextPosition = position;
			capitalTextPosition.y += 2f;
			//capitalTextPosition.z -= 5f;
			txtMeshTransform.position = capitalTextPosition;

			var meshCapitalText = txtMeshTransform.GetComponent<TextMesh>();
			meshCapitalText.text = text;
			
			meshCapitalText.color = color;
			meshCapitalText.fontSize = fontSize;
			
			return meshCapitalText;
		}
	}
}
