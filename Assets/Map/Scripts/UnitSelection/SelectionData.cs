using System.Collections.Generic;
using UnityEngine;

namespace Nashet.UnitSelection
{
	public class SelectionData : ISelectionData
	{
		public SelectionData(Collider selected)
		{
			SingleSelection = selected;
		}

		public SelectionData(IEnumerable<Collider> selected)
		{
			MultipleSelection = selected;
		}

		public IEnumerable<Collider> MultipleSelection { get; private set; }
		public Collider SingleSelection { get; private set; }
	}
}