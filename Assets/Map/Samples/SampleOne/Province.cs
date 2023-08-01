using System;
using System.Collections.Generic;
using System.Linq;
using Nashet.Map.Utils;
using Nashet.MapMeshes;
using QPathFinder;
using UnityEngine;

namespace Nashet.Map.Examples
{
	public class Province : IProvince
	{
		public enum TerrainTypes { Plains, Mountains }

		static public readonly Dictionary<int, Province> AllProvinces = new Dictionary<int, Province>();

		public object Country { set; get; }
		public TerrainTypes Terrain { get; internal set; }

		public readonly HashSet<Border> neughbors = new HashSet<Border>();
		public Node Node;
		public Vector3 Position;
		public int Id;
		public ProvinceMesh provinceMesh;
		private string name;

		public Province(int Id, string name)
		{
			AllProvinces.Add(Id, this);
			Terrain = TerrainTypes.Plains;
			Rand.Call(() => Terrain = TerrainTypes.Mountains, 3);
			this.Id = Id;
			this.name = name;
		}

		internal bool isNeighbor(Province province2)
		{
			return neughbors.Any(x => x.Province == province2 && x.IsPassable);
		}

		internal bool isRiverNeighbor(Province neighbor)
		{
			return neughbors.Any(x => x.Province == neighbor && x.IsRiverBorder);
		}

		internal void AddRiverBorder(Province province2, Material riverMaterial)
		{
			provinceMesh.SetBorderMaterial(province2.Id, riverMaterial);
			var border = neughbors.FirstOrDefault(x => x.Province == province2);
			border.IsRiverBorder = true;
		}

		public void SetBorderMaterial(int id, Material material)
		{
			provinceMesh.SetBorderMaterial(id, material);
		}

		public override string ToString()
		{
			return name;
		}
	}
}