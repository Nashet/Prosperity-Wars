using Nashet.MeshData;
using System.Collections.Generic;

namespace Nashet.MarchingSquares
{
	public interface IVoxelGrid
	{
		MeshStructure getMesh(int analysingProvince, out Dictionary<int, MeshStructure> borders);
	}
}