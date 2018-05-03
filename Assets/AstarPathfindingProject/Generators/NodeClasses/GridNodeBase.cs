using UnityEngine;
using Pathfinding.Serialization;

namespace Pathfinding {
	/** Base class for GridNode and LevelGridNode */
	public abstract class GridNodeBase : GraphNode {
		protected GridNodeBase (AstarPath astar) : base(astar) {
		}

		const int GridFlagsWalkableErosionOffset = 8;
		const int GridFlagsWalkableErosionMask = 1 << GridFlagsWalkableErosionOffset;

		const int GridFlagsWalkableTmpOffset = 9;
		const int GridFlagsWalkableTmpMask = 1 << GridFlagsWalkableTmpOffset;

		protected const int NodeInGridIndexLayerOffset = 24;
		protected const int NodeInGridIndexMask = 0xFFFFFF;

		/** Bitfield containing the x and z coordinates of the node as well as the layer (for layered grid graphs).
		 * \see NodeInGridIndex
		 */
		protected int nodeInGridIndex;
		protected ushort gridFlags;

#if !ASTAR_GRID_NO_CUSTOM_CONNECTIONS
		public Connection[] connections;
#endif

		/** The index of the node in the grid.
		 * This is x + z*graph.width
		 * So you can get the X and Z indices using
		 * \code
		 * int index = node.NodeInGridIndex;
		 * int x = index % graph.width;
		 * int z = index / graph.width;
		 * // where graph is GridNode.GetGridGraph (node.graphIndex), i.e the graph the nodes are contained in.
		 * \endcode
		 */
		public int NodeInGridIndex { get { return nodeInGridIndex & NodeInGridIndexMask; } set { nodeInGridIndex = (nodeInGridIndex & ~NodeInGridIndexMask) | value; } }

		/** X coordinate of the node in the grid.
		 * The node in the bottom left corner has (x,z) = (0,0) and the one in the opposite
		 * corner has (x,z) = (width-1, depth-1)
		 * \see ZCoordInGrid
		 * \see NodeInGridIndex
		 */
		public int XCoordinateInGrid {
			get {
				return NodeInGridIndex % GridNode.GetGridGraph(GraphIndex).width;
			}
		}

		/** Z coordinate of the node in the grid.
		 * The node in the bottom left corner has (x,z) = (0,0) and the one in the opposite
		 * corner has (x,z) = (width-1, depth-1)
		 * \see XCoordInGrid
		 * \see NodeInGridIndex
		 */
		public int ZCoordinateInGrid {
			get {
				return NodeInGridIndex / GridNode.GetGridGraph(GraphIndex).width;
			}
		}

		/** Stores walkability before erosion is applied.
		 * Used internally when updating the graph.
		 */
		public bool WalkableErosion {
			get {
				return (gridFlags & GridFlagsWalkableErosionMask) != 0;
			}
			set {
				unchecked { gridFlags = (ushort)(gridFlags & ~GridFlagsWalkableErosionMask | (value ? (ushort)GridFlagsWalkableErosionMask : (ushort)0)); }
			}
		}

		/** Temporary variable used internally when updating the graph. */
		public bool TmpWalkable {
			get {
				return (gridFlags & GridFlagsWalkableTmpMask) != 0;
			}
			set {
				unchecked { gridFlags = (ushort)(gridFlags & ~GridFlagsWalkableTmpMask | (value ? (ushort)GridFlagsWalkableTmpMask : (ushort)0)); }
			}
		}

		/** True if the node has grid connections to all its 8 neighbours.
		 * \note This will always return false if GridGraph.neighbours is set to anything other than Eight.
		 * \see GetNeighbourAlongDirection
		 */
		public abstract bool HasConnectionsToAllEightNeighbours { get; }

		public override float SurfaceArea () {
			GridGraph gg = GridNode.GetGridGraph(GraphIndex);

			return gg.nodeSize*gg.nodeSize;
		}

		public override Vector3 RandomPointOnSurface () {
			GridGraph gg = GridNode.GetGridGraph(GraphIndex);

			var graphSpacePosition = gg.transform.InverseTransform((Vector3)position);

			return gg.transform.Transform(graphSpacePosition + new Vector3(Random.value - 0.5f, 0, Random.value - 0.5f));
		}

		public override int GetGizmoHashCode () {
			var hash = base.GetGizmoHashCode();

#if !ASTAR_GRID_NO_CUSTOM_CONNECTIONS
			if (connections != null) {
				for (int i = 0; i < connections.Length; i++) {
					hash ^= 17 * connections[i].GetHashCode();
				}
			}
#endif
			hash ^= 109 * gridFlags;
			return hash;
		}

		/** Adjacent grid node in the specified direction.
		 * This will return null if the node does not have a connection to a node
		 * in that direction.
		 *
		 * The dir parameter corresponds to directions in the grid as:
		 * \code
		 *         Z
		 *         |
		 *         |
		 *
		 *      6  2  5
		 *       \ | /
		 * --  3 - X - 1  ----- X
		 *       / | \
		 *      7  0  4
		 *
		 *         |
		 *         |
		 * \endcode
		 *
		 * \see GetConnections
		 */
		public abstract GridNodeBase GetNeighbourAlongDirection (int direction);

		public override bool ContainsConnection (GraphNode node) {
#if !ASTAR_GRID_NO_CUSTOM_CONNECTIONS
			if (connections != null) {
				for (int i = 0; i < connections.Length; i++) {
					if (connections[i].node == node) {
						return true;
					}
				}
			}
#endif

			for (int i = 0; i < 8; i++) {
				if (node == GetNeighbourAlongDirection(i)) {
					return true;
				}
			}

			return false;
		}

#if ASTAR_GRID_NO_CUSTOM_CONNECTIONS
		public override void AddConnection (GraphNode node, uint cost) {
			throw new System.NotImplementedException("GridNodes do not have support for adding manual connections with your current settings."+
				"\nPlease disable ASTAR_GRID_NO_CUSTOM_CONNECTIONS in the Optimizations tab in the A* Inspector");
		}

		public override void RemoveConnection (GraphNode node) {
			throw new System.NotImplementedException("GridNodes do not have support for adding manual connections with your current settings."+
				"\nPlease disable ASTAR_GRID_NO_CUSTOM_CONNECTIONS in the Optimizations tab in the A* Inspector");
		}

		public void ClearCustomConnections (bool alsoReverse) {
		}
#else
		public override void FloodFill (System.Collections.Generic.Stack<GraphNode> stack, uint region) {
			if (connections != null) for (int i = 0; i < connections.Length; i++) {
					GraphNode other = connections[i].node;
					if (other.Area != region) {
						other.Area = region;
						stack.Push(other);
					}
				}
		}

		/** Same as #ClearConnections, but does not clear grid connections, only custom ones (e.g added by #AddConnection or a NodeLink component) */
		public void ClearCustomConnections (bool alsoReverse) {
			if (connections != null) for (int i = 0; i < connections.Length; i++) connections[i].node.RemoveConnection(this);
			connections = null;
		}

		public override void ClearConnections (bool alsoReverse) {
			ClearCustomConnections(alsoReverse);
		}

		public override void GetConnections (System.Action<GraphNode> action) {
			if (connections != null) for (int i = 0; i < connections.Length; i++) action(connections[i].node);
		}

		public override void UpdateRecursiveG (Path path, PathNode pathNode, PathHandler handler) {
			ushort pid = handler.PathID;

			if (connections != null) for (int i = 0; i < connections.Length; i++) {
					GraphNode other = connections[i].node;
					PathNode otherPN = handler.GetPathNode(other);
					if (otherPN.parent == pathNode && otherPN.pathID == pid) other.UpdateRecursiveG(path, otherPN, handler);
				}
		}

		public override void Open (Path path, PathNode pathNode, PathHandler handler) {
			ushort pid = handler.PathID;

			if (connections != null) for (int i = 0; i < connections.Length; i++) {
					GraphNode other = connections[i].node;
					if (!path.CanTraverse(other)) continue;

					PathNode otherPN = handler.GetPathNode(other);

					uint tmpCost = connections[i].cost;

					if (otherPN.pathID != pid) {
						otherPN.parent = pathNode;
						otherPN.pathID = pid;

						otherPN.cost = tmpCost;

						otherPN.H = path.CalculateHScore(other);
						otherPN.UpdateG(path);

						//Debug.Log ("G " + otherPN.G + " F " + otherPN.F);
						handler.heap.Add(otherPN);
						//Debug.DrawRay ((Vector3)otherPN.node.Position, Vector3.up,Color.blue);
					} else {
						// Sorry for the huge number of #ifs

						//If not we can test if the path from the current node to this one is a better one then the one already used

#if ASTAR_NO_TRAVERSAL_COST
						if (pathNode.G+tmpCost < otherPN.G)
#else
						if (pathNode.G+tmpCost+path.GetTraversalCost(other) < otherPN.G)
#endif
						{
							//Debug.Log ("Path better from " + NodeIndex + " to " + otherPN.node.NodeIndex + " " + (pathNode.G+tmpCost+path.GetTraversalCost(other)) + " < " + otherPN.G);
							otherPN.cost = tmpCost;

							otherPN.parent = pathNode;

							other.UpdateRecursiveG(path, otherPN, handler);
						}
					}
				}
		}

		/** Add a connection from this node to the specified node.
		 * If the connection already exists, the cost will simply be updated and
		 * no extra connection added.
		 *
		 * \note Only adds a one-way connection. Consider calling the same function on the other node
		 * to get a two-way connection.
		 */
		public override void AddConnection (GraphNode node, uint cost) {
			if (node == null) throw new System.ArgumentNullException();

			if (connections != null) {
				for (int i = 0; i < connections.Length; i++) {
					if (connections[i].node == node) {
						connections[i].cost = cost;
						return;
					}
				}
			}

			int connLength = connections != null ? connections.Length : 0;

			var newconns = new Connection[connLength+1];
			for (int i = 0; i < connLength; i++) {
				newconns[i] = connections[i];
			}

			newconns[connLength] = new Connection(node, cost);

			connections = newconns;
		}

		/** Removes any connection from this node to the specified node.
		 * If no such connection exists, nothing will be done.
		 *
		 * \note This only removes the connection from this node to the other node.
		 * You may want to call the same function on the other node to remove its eventual connection
		 * to this node.
		 */
		public override void RemoveConnection (GraphNode node) {
			if (connections == null) return;

			for (int i = 0; i < connections.Length; i++) {
				if (connections[i].node == node) {
					int connLength = connections.Length;

					var newconns = new Connection[connLength-1];
					for (int j = 0; j < i; j++) {
						newconns[j] = connections[j];
					}
					for (int j = i+1; j < connLength; j++) {
						newconns[j-1] = connections[j];
					}

					connections = newconns;
					return;
				}
			}
		}

		public override void SerializeReferences (GraphSerializationContext ctx) {
			// TODO: Deduplicate code
			if (connections == null) {
				ctx.writer.Write(-1);
			} else {
				ctx.writer.Write(connections.Length);
				for (int i = 0; i < connections.Length; i++) {
					ctx.SerializeNodeReference(connections[i].node);
					ctx.writer.Write(connections[i].cost);
				}
			}
		}

		public override void DeserializeReferences (GraphSerializationContext ctx) {
			// Grid nodes didn't serialize references before 3.8.3
			if (ctx.meta.version < AstarSerializer.V3_8_3)
				return;

			int count = ctx.reader.ReadInt32();

			if (count == -1) {
				connections = null;
			} else {
				connections = new Connection[count];

				for (int i = 0; i < count; i++) {
					connections[i] = new Connection(ctx.DeserializeNodeReference(), ctx.reader.ReadUInt32());
				}
			}
		}
#endif
	}
}
