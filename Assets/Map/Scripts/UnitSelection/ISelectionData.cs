using System.Collections.Generic;
using UnityEngine;

namespace Nashet.UnitSelection
{
	public interface ISelectionData
	{
		IEnumerable<Collider> MultipleSelection { get; }
		Collider SingleSelection { get; }
	}
}