using UnityEngine;
using Pathfinding.Serialization;

namespace Pathfinding {
	/** Node used for the PointGraph.
	 * This is just a simple point with a list of connections (and associated costs) to other nodes.
	 * It does not have any concept of a surface like many other node types.
	 *
	 * \see PointGraph
	 */
	public class PointNode : GraphNode {
		public Connection[] connections;

		/** GameObject this node was created from (if any).
		 * \warning When loading a graph from a saved file or from cache, this field will be null.
		 *
		 * \snippet MiscSnippets.cs PointNode.gameObject
		 */
		public GameObject gameObject;

		public void SetPosition (Int3 value) {
			position = value;
		}

		public PointNode (AstarPath astar) : base(astar) {
		}

		public override void GetConnections (System.Action<GraphNode> action) {
			if (connections == null) return;
			for (int i = 0; i < connections.Length; i++) action(connections[i].node);
		}

		public override void ClearConnections (bool alsoReverse) {
			if (alsoReverse && connections != null) {
				for (int i = 0; i < connections.Length; i++) {
					connections[i].node.RemoveConnection(this);
				}
			}

			connections = null;
		}

		public override void UpdateRecursiveG (Path path, PathNode pathNode, PathHandler handler) {
			pathNode.UpdateG(path);

			handler.heap.Add(pathNode);

			for (int i = 0; i < connections.Length; i++) {
				GraphNode other = connections[i].node;
				PathNode otherPN = handler.GetPathNode(other);
				if (otherPN.parent == pathNode && otherPN.pathID == handler.PathID) {
					other.UpdateRecursiveG(path, otherPN, handler);
				}
			}
		}

		public override bool ContainsConnection (GraphNode node) {
			if (connections == null) return false;
			for (int i = 0; i < connections.Length; i++) if (connections[i].node == node) return true;
			return false;
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

		public override void Open (Path path, PathNode pathNode, PathHandler handler) {
			if (connections == null) return;

			for (int i = 0; i < connections.Length; i++) {
				GraphNode other = connections[i].node;

				if (path.CanTraverse(other)) {
					PathNode pathOther = handler.GetPathNode(other);

					if (pathOther.pathID != handler.PathID) {
						pathOther.parent = pathNode;
						pathOther.pathID = handler.PathID;

						pathOther.cost = connections[i].cost;

						pathOther.H = path.CalculateHScore(other);
						pathOther.UpdateG(path);

						handler.heap.Add(pathOther);
					} else {
						//If not we can test if the path from this node to the other one is a better one then the one already used
						uint tmpCost = connections[i].cost;

						if (pathNode.G + tmpCost + path.GetTraversalCost(other) < pathOther.G) {
							pathOther.cost = tmpCost;
							pathOther.parent = pathNode;

							other.UpdateRecursiveG(path, pathOther, handler);
						}
					}
				}
			}
		}

		public override int GetGizmoHashCode () {
			var hash = base.GetGizmoHashCode();

			if (connections != null) {
				for (int i = 0; i < connections.Length; i++) {
					hash ^= 17 * connections[i].GetHashCode();
				}
			}
			return hash;
		}

		public override void SerializeNode (GraphSerializationContext ctx) {
			base.SerializeNode(ctx);
			ctx.SerializeInt3(position);
		}

		public override void DeserializeNode (GraphSerializationContext ctx) {
			base.DeserializeNode(ctx);
			position = ctx.DeserializeInt3();
		}

		public override void SerializeReferences (GraphSerializationContext ctx) {
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
	}
}
