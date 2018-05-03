using UnityEngine;

namespace Pathfinding {
	/** Basic path, finds the shortest path from A to B.
	 * \ingroup paths
	 * This is the most basic path object it will try to find the shortest path between two points.\n
	 * Many other path types inherit from this type.
	 * \see Seeker.StartPath
	 * \see \ref calling-pathfinding
	 * \see \ref getstarted
	 */
	public class ABPath : Path {
		/** Start node of the path */
		public GraphNode startNode;

		/** End node of the path */
		public GraphNode endNode;

		/** Start Point exactly as in the path request */
		public Vector3 originalStartPoint;

		/** End Point exactly as in the path request */
		public Vector3 originalEndPoint;

		/** Start point of the path.
		 * This is the closest point on the #startNode to #originalStartPoint
		 */
		public Vector3 startPoint;

		/** End point of the path.
		 * This is the closest point on the #endNode to #originalEndPoint
		 */
		public Vector3 endPoint;

		/** Determines if a search for an end node should be done.
		 * Set by different path types.
		 * \since Added in 3.0.8.3
		 */
		protected virtual bool hasEndPoint {
			get {
				return true;
			}
		}

		public Int3 startIntPoint; /**< Start point in integer coordinates */

		/** Calculate partial path if the target node cannot be reached.
		 * If the target node cannot be reached, the node which was closest (given by heuristic) will be chosen as target node
		 * and a partial path will be returned.
		 * This only works if a heuristic is used (which is the default).
		 * If a partial path is found, CompleteState is set to Partial.
		 * \note It is not required by other path types to respect this setting
		 *
		 * \warning This feature is currently a work in progress and may not work in the current version
		 */
		public bool calculatePartial;

		/** Current best target for the partial path.
		 * This is the node with the lowest H score.
		 * \warning This feature is currently a work in progress and may not work in the current version
		 */
		protected PathNode partialBestTarget;

		/** Saved original costs for the end node. \see ResetCosts */
		protected int[] endNodeCosts;

		/** Used in EndPointGridGraphSpecialCase */
		GridNode gridSpecialCaseNode;

		/** @{ @name Constructors */

		/** Default constructor.
		 * Do not use this. Instead use the static Construct method which can handle path pooling.
		 */
		public ABPath () {}

		/** Construct a path with a start and end point.
		 * The delegate will be called when the path has been calculated.
		 * Do not confuse it with the Seeker callback as they are sent at different times.
		 * If you are using a Seeker to start the path you can set \a callback to null.
		 *
		 * \returns The constructed path object
		 */
		public static ABPath Construct (Vector3 start, Vector3 end, OnPathDelegate callback = null) {
			var p = PathPool.GetPath<ABPath>();

			p.Setup(start, end, callback);
			return p;
		}

		protected void Setup (Vector3 start, Vector3 end, OnPathDelegate callbackDelegate) {
			callback = callbackDelegate;
			UpdateStartEnd(start, end);
		}

		/** @} */

		/** Sets the start and end points.
		 * Sets #originalStartPoint, #originalEndPoint, #startPoint, #endPoint, #startIntPoint and #hTarget (to \a end ) */
		protected void UpdateStartEnd (Vector3 start, Vector3 end) {
			originalStartPoint = start;
			originalEndPoint = end;

			startPoint = start;
			endPoint = end;

			startIntPoint = (Int3)start;
			hTarget = (Int3)end;
		}

		internal override uint GetConnectionSpecialCost (GraphNode a, GraphNode b, uint currentCost) {
			if (startNode != null && endNode != null) {
				if (a == startNode) {
					return (uint)((startIntPoint - (b == endNode ? hTarget : b.position)).costMagnitude * (currentCost*1.0/(a.position-b.position).costMagnitude));
				}
				if (b == startNode) {
					return (uint)((startIntPoint - (a == endNode ? hTarget : a.position)).costMagnitude * (currentCost*1.0/(a.position-b.position).costMagnitude));
				}
				if (a == endNode) {
					return (uint)((hTarget - b.position).costMagnitude * (currentCost*1.0/(a.position-b.position).costMagnitude));
				}
				if (b == endNode) {
					return (uint)((hTarget - a.position).costMagnitude * (currentCost*1.0/(a.position-b.position).costMagnitude));
				}
			} else {
				// endNode is null, startNode should never be null for an ABPath
				if (a == startNode) {
					return (uint)((startIntPoint - b.position).costMagnitude * (currentCost*1.0/(a.position-b.position).costMagnitude));
				}
				if (b == startNode) {
					return (uint)((startIntPoint - a.position).costMagnitude * (currentCost*1.0/(a.position-b.position).costMagnitude));
				}
			}

			return currentCost;
		}

		/** Reset all values to their default values.
		 * All inheriting path types must implement this function, resetting ALL their variables to enable recycling of paths.
		 * Call this base function in inheriting types with base.Reset ();
		 */
		protected override void Reset () {
			base.Reset();

			startNode = null;
			endNode = null;
			originalStartPoint = Vector3.zero;
			originalEndPoint = Vector3.zero;
			startPoint = Vector3.zero;
			endPoint = Vector3.zero;
			calculatePartial = false;
			partialBestTarget = null;
			startIntPoint = new Int3();
			hTarget = new Int3();
			endNodeCosts = null;

			gridSpecialCaseNode = null;
		}

		/** Applies a special case for grid nodes.
		 *
		 * Assume the closest walkable node is a grid node.
		 * We will now apply a special case only for grid graphs.
		 * In tile based games, an obstacle often occupies a whole
		 * node. When a path is requested to the position of an obstacle
		 * (single unwalkable node) the closest walkable node will be
		 * one of the 8 nodes surrounding that unwalkable node
		 * but that node is not neccessarily the one that is most
		 * optimal to walk to so in this special case
		 * we mark all nodes around the unwalkable node as targets
		 * and when we search and find any one of them we simply exit
		 * and set that first node we found to be the 'real' end node
		 * because that will be the optimal node (this does not apply
		 * in general unless the heuristic is set to None, but
		 * for a single unwalkable node it does).
		 * This also applies if the nearest node cannot be traversed for
		 * some other reason like restricted tags.
		 *
		 * \returns True if the workaround was applied. If this happens the
		 * endPoint, endNode, hTarget and hTargetNode fields will be modified.
		 *
		 * Image below shows paths when this special case is applied. The path goes from the white sphere to the orange box.
		 * \shadowimage{abpath_grid_special.gif}
		 *
		 * Image below shows paths when this special case has been disabled
		 * \shadowimage{abpath_grid_not_special.gif}
		 */
		protected virtual bool EndPointGridGraphSpecialCase (GraphNode closestWalkableEndNode) {
			var gridNode = closestWalkableEndNode as GridNode;

			if (gridNode != null) {
				var gridGraph = GridNode.GetGridGraph(gridNode.GraphIndex);

				// Find the closest node, not neccessarily walkable
				var endNNInfo2 = AstarPath.active.GetNearest(originalEndPoint, NNConstraint.None);
				var gridNode2 = endNNInfo2.node as GridNode;

				if (gridNode != gridNode2 && gridNode2 != null && gridNode.GraphIndex == gridNode2.GraphIndex) {
					// Calculate the coordinates of the nodes
					var x1 = gridNode.NodeInGridIndex % gridGraph.width;
					var z1 = gridNode.NodeInGridIndex / gridGraph.width;

					var x2 = gridNode2.NodeInGridIndex % gridGraph.width;
					var z2 = gridNode2.NodeInGridIndex / gridGraph.width;

					bool wasClose = false;
					switch (gridGraph.neighbours) {
					case NumNeighbours.Four:
						if ((x1 == x2 && System.Math.Abs(z1-z2) == 1) || (z1 == z2 && System.Math.Abs(x1-x2) == 1)) {
							// If 'O' is gridNode2, then gridNode is one of the nodes marked with an 'x'
							//    x
							//  x O x
							//    x
							wasClose = true;
						}
						break;
					case NumNeighbours.Eight:
						if (System.Math.Abs(x1-x2) <= 1 && System.Math.Abs(z1-z2) <= 1) {
							// If 'O' is gridNode2, then gridNode is one of the nodes marked with an 'x'
							//  x x x
							//  x O x
							//  x x x
							wasClose = true;
						}
						break;
					case NumNeighbours.Six:
						// Hexagon graph
						for (int i = 0; i < 6; i++) {
							var nx = x2 + gridGraph.neighbourXOffsets[GridGraph.hexagonNeighbourIndices[i]];
							var nz = z2 + gridGraph.neighbourZOffsets[GridGraph.hexagonNeighbourIndices[i]];
							if (x1 == nx && z1 == nz) {
								// If 'O' is gridNode2, then gridNode is one of the nodes marked with an 'x'
								//    x x
								//  x O x
								//  x x
								wasClose = true;
								break;
							}
						}
						break;
					default:
						// Should not happen unless NumNeighbours is modified in the future
						throw new System.Exception("Unhandled NumNeighbours");
					}

					if (wasClose) {
						// We now need to find all nodes marked with an x to be able to mark them as targets
						SetFlagOnSurroundingGridNodes(gridNode2, 1, true);

						// Note, other methods assume hTarget is (Int3)endPoint
						endPoint = (Vector3)gridNode2.position;
						hTarget = gridNode2.position;
						endNode = gridNode2;

						// hTargetNode is used for heuristic optimizations
						// (also known as euclidean embedding).
						// Even though the endNode is not walkable
						// we can use it for better heuristics since
						// there is a workaround added (EuclideanEmbedding.ApplyGridGraphEndpointSpecialCase)
						// which is there to support this case.
						hTargetNode = endNode;

						// We need to save this node
						// so that we can reset flag1 on all nodes later
						gridSpecialCaseNode = gridNode2;
						return true;
					}
				}
			}

			return false;
		}

		/** Helper method to set PathNode.flag1 to a specific value for all nodes adjacent to a grid node */
		void SetFlagOnSurroundingGridNodes (GridNode gridNode, int flag, bool flagState) {
			// Loop through all adjacent grid nodes
			var gridGraph = GridNode.GetGridGraph(gridNode.GraphIndex);

			// Number of neighbours as an int
			int mxnum = gridGraph.neighbours == NumNeighbours.Four ? 4 : (gridGraph.neighbours == NumNeighbours.Eight ? 8 : 6);

			// Calculate the coordinates of the node
			var x = gridNode.NodeInGridIndex % gridGraph.width;
			var z = gridNode.NodeInGridIndex / gridGraph.width;

			if (flag != 1 && flag != 2)
				throw new System.ArgumentOutOfRangeException("flag");

			for (int i = 0; i < mxnum; i++) {
				int nx, nz;
				if (gridGraph.neighbours == NumNeighbours.Six) {
					// Hexagon graph
					nx = x + gridGraph.neighbourXOffsets[GridGraph.hexagonNeighbourIndices[i]];
					nz = z + gridGraph.neighbourZOffsets[GridGraph.hexagonNeighbourIndices[i]];
				} else {
					nx = x + gridGraph.neighbourXOffsets[i];
					nz = z + gridGraph.neighbourZOffsets[i];
				}

				// Check if the position is still inside the grid
				if (nx >= 0 && nz >= 0 && nx < gridGraph.width && nz < gridGraph.depth) {
					var adjacentNode = gridGraph.nodes[nz*gridGraph.width + nx];
					var pathNode = pathHandler.GetPathNode(adjacentNode);
					if (flag == 1) pathNode.flag1 = flagState;
					else pathNode.flag2 = flagState;
				}
			}
		}

		/** Prepares the path. Searches for start and end nodes and does some simple checking if a path is at all possible */
		protected override void Prepare () {
			AstarProfiler.StartProfile("Get Nearest");

			//Initialize the NNConstraint
			nnConstraint.tags = enabledTags;
			var startNNInfo  = AstarPath.active.GetNearest(startPoint, nnConstraint);

			//Tell the NNConstraint which node was found as the start node if it is a PathNNConstraint and not a normal NNConstraint
			var pathNNConstraint = nnConstraint as PathNNConstraint;
			if (pathNNConstraint != null) {
				pathNNConstraint.SetStart(startNNInfo.node);
			}

			startPoint = startNNInfo.position;

			startIntPoint = (Int3)startPoint;
			startNode = startNNInfo.node;

			if (startNode == null) {
				FailWithError("Couldn't find a node close to the start point");
				return;
			}

			if (!CanTraverse(startNode)) {
				FailWithError("The node closest to the start point could not be traversed");
				return;
			}

			// If it is declared that this path type has an end point
			// Some path types might want to use most of the ABPath code, but will not have an explicit end point at this stage
			if (hasEndPoint) {
				var endNNInfo = AstarPath.active.GetNearest(endPoint, nnConstraint);
				endPoint = endNNInfo.position;
				endNode = endNNInfo.node;

				if (endNode == null) {
					FailWithError("Couldn't find a node close to the end point");
					return;
				}

				// This should not trigger unless the user has modified the NNConstraint
				if (!CanTraverse(endNode)) {
					FailWithError("The node closest to the end point could not be traversed");
					return;
				}

				// This should not trigger unless the user has modified the NNConstraint
				if (startNode.Area != endNode.Area) {
					FailWithError("There is no valid path to the target");
					return;
				}

				// Potentially we want to special case grid graphs a bit
				// to better support some kinds of games
				// If this returns true it will overwrite the
				// endNode, endPoint, hTarget and hTargetNode fields
				if (!EndPointGridGraphSpecialCase(endNNInfo.node)) {
					// Note, other methods assume hTarget is (Int3)endPoint
					hTarget = (Int3)endPoint;
					hTargetNode = endNode;

					// Mark end node with flag1 to mark it as a target point
					pathHandler.GetPathNode(endNode).flag1 = true;
				}
			}

			AstarProfiler.EndProfile();
		}

		/** Checks if the start node is the target and complete the path if that is the case.
		 * This is necessary so that subclasses (e.g XPath) can override this behaviour.
		 *
		 * If the start node is a valid target point, this method should set CompleteState to Complete
		 * and trace the path.
		 */
		protected virtual void CompletePathIfStartIsValidTarget () {
			// flag1 specifies if a node is a target node for the path
			if (hasEndPoint && pathHandler.GetPathNode(startNode).flag1) {
				CompleteWith(startNode);
				Trace(pathHandler.GetPathNode(startNode));
			}
		}

		protected override void Initialize () {
			// Mark nodes to enable special connection costs for start and end nodes
			// See GetConnectionSpecialCost
			if (startNode != null) pathHandler.GetPathNode(startNode).flag2 = true;
			if (endNode != null) pathHandler.GetPathNode(endNode).flag2 = true;

			// Zero out the properties on the start node
			PathNode startRNode = pathHandler.GetPathNode(startNode);
			startRNode.node = startNode;
			startRNode.pathID = pathHandler.PathID;
			startRNode.parent = null;
			startRNode.cost = 0;
			startRNode.G = GetTraversalCost(startNode);
			startRNode.H = CalculateHScore(startNode);

			// Check if the start node is the target and complete the path if that is the case
			CompletePathIfStartIsValidTarget();
			if (CompleteState == PathCompleteState.Complete) return;

			// Open the start node and puts its neighbours in the open list
			startNode.Open(this, startRNode, pathHandler);

			searchedNodes++;

			partialBestTarget = startRNode;

			// Any nodes left to search?
			if (pathHandler.heap.isEmpty) {
				if (calculatePartial) {
					CompleteState = PathCompleteState.Partial;
					Trace(partialBestTarget);
				} else {
					FailWithError("No open points, the start node didn't open any nodes");
				}
				return;
			}

			// Pop the first node off the open list
			currentR = pathHandler.heap.Remove();
		}

		protected override void Cleanup () {
			// TODO: Set flag1 = false as well?
			if (startNode != null) {
				var pathStartNode = pathHandler.GetPathNode(startNode);
				pathStartNode.flag1 = false;
				pathStartNode.flag2 = false;
			}

			if (endNode != null) {
				var pathEndNode = pathHandler.GetPathNode(endNode);
				pathEndNode.flag1 = false;
				pathEndNode.flag2 = false;
			}

			// Set flag1 and flag2 to false on all nodes we set it to true on
			// at the start of the path call. Otherwise this state
			// will leak to other path calculations and cause all
			// kinds of havoc.
			// flag2 is also set because the end node could have changed
			// and thus the flag2 which is set to false above might not set
			// it on the correct node
			if (gridSpecialCaseNode != null) {
				var pathNode = pathHandler.GetPathNode(gridSpecialCaseNode);
				pathNode.flag1 = false;
				pathNode.flag2 = false;
				SetFlagOnSurroundingGridNodes(gridSpecialCaseNode, 1, false);
				SetFlagOnSurroundingGridNodes(gridSpecialCaseNode, 2, false);
			}
		}

		/** Completes the path using the specified target node.
		 * This method assumes that the node is a target node of the path
		 * not just any random node.
		 */
		void CompleteWith (GraphNode node) {
			if (endNode == node) {
				// Common case, no grid graph special case has been applied
				// Nothing to do
			} else {
				// See EndPointGridGraphSpecialCase()
				var gridNode = node as GridNode;
				if (gridNode == null) {
					throw new System.Exception("Some path is not cleaning up the flag1 field. This is a bug.");
				}

				// The grid graph special case has been applied
				// The closest point on the node is not yet known
				// so we need to calculate it
				endPoint = gridNode.ClosestPointOnNode(originalEndPoint);
				// This is now our end node
				// We didn't know it before, but apparently it was optimal
				// to move to this node
				endNode = node;
			}
			// Mark the path as completed
			CompleteState = PathCompleteState.Complete;
		}

		/** Calculates the path until completed or until the time has passed \a targetTick.
		 * Usually a check is only done every 500 nodes if the time has passed \a targetTick.
		 * Time/Ticks are got from System.DateTime.UtcNow.Ticks.
		 *
		 * Basic outline of what the function does for the standard path (Pathfinding.ABPath).
		 * \code
		 * while the end has not been found and no error has occurred
		 * check if we have reached the end
		 * if so, exit and return the path
		 *
		 * open the current node, i.e loop through its neighbours, mark them as visited and put them on a heap
		 *
		 * check if there are still nodes left to process (or have we searched the whole graph)
		 * if there are none, flag error and exit
		 *
		 * pop the next node of the heap and set it as current
		 *
		 * check if the function has exceeded the time limit
		 * if so, return and wait for the function to get called again
		 * \endcode
		 */
		protected override void CalculateStep (long targetTick) {
			int counter = 0;

			// Continue to search as long as we haven't encountered an error and we haven't found the target
			while (CompleteState == PathCompleteState.NotCalculated) {
				searchedNodes++;

				// Close the current node, if the current node is the target node then the path is finished
				if (currentR.flag1) {
					// We found a target point
					// Mark that node as the end point
					CompleteWith(currentR.node);
					break;
				}

				if (currentR.H < partialBestTarget.H) {
					partialBestTarget = currentR;
				}

				AstarProfiler.StartFastProfile(4);

				// Loop through all walkable neighbours of the node and add them to the open list.
				currentR.node.Open(this, currentR, pathHandler);

				AstarProfiler.EndFastProfile(4);

				// Any nodes left to search?
				if (pathHandler.heap.isEmpty) {
					if (calculatePartial && partialBestTarget != null) {
						CompleteState = PathCompleteState.Partial;
						Trace(partialBestTarget);
					} else {
						FailWithError("Searched whole area but could not find target");
					}
					return;
				}

				// Select the node with the lowest F score and remove it from the open list
				AstarProfiler.StartFastProfile(7);
				currentR = pathHandler.heap.Remove();
				AstarProfiler.EndFastProfile(7);

				// Check for time every 500 nodes, roughly every 0.5 ms usually
				if (counter > 500) {
					// Have we exceded the maxFrameTime, if so we should wait one frame before continuing the search since we don't want the game to lag
					if (System.DateTime.UtcNow.Ticks >= targetTick) {
						// Return instead of yield'ing, a separate function handles the yield (CalculatePaths)
						return;
					}
					counter = 0;

					// Mostly for development
					if (searchedNodes > 1000000) {
						throw new System.Exception("Probable infinite loop. Over 1,000,000 nodes searched");
					}
				}

				counter++;
			}

			AstarProfiler.StartProfile("Trace");

			if (CompleteState == PathCompleteState.Complete) {
				Trace(currentR);
			} else if (calculatePartial && partialBestTarget != null) {
				CompleteState = PathCompleteState.Partial;
				Trace(partialBestTarget);
			}

			AstarProfiler.EndProfile();
		}

		/** Returns a debug string for this path.
		 */
		internal override string DebugString (PathLog logMode) {
			if (logMode == PathLog.None || (!error && logMode == PathLog.OnlyErrors)) {
				return "";
			}

			var text = new System.Text.StringBuilder();

			DebugStringPrefix(logMode, text);

			if (!error && logMode == PathLog.Heavy) {
				if (hasEndPoint && endNode != null) {
					PathNode nodeR = pathHandler.GetPathNode(endNode);
					text.Append("\nEnd Node\n	G: ");
					text.Append(nodeR.G);
					text.Append("\n	H: ");
					text.Append(nodeR.H);
					text.Append("\n	F: ");
					text.Append(nodeR.F);
					text.Append("\n	Point: ");
					text.Append(((Vector3)endPoint).ToString());
					text.Append("\n	Graph: ");
					text.Append(endNode.GraphIndex);
				}

				text.Append("\nStart Node");
				text.Append("\n	Point: ");
				text.Append(((Vector3)startPoint).ToString());
				text.Append("\n	Graph: ");
				if (startNode != null) text.Append(startNode.GraphIndex);
				else text.Append("< null startNode >");
			}

			DebugStringSuffix(logMode, text);

			return text.ToString();
		}

		/** \cond INTERNAL */
		/** Returns in which direction to move from a point on the path.
		 * A simple and quite slow (well, compared to more optimized algorithms) algorithm first finds the closest path segment (from #vectorPath) and then returns
		 * the direction to the next point from there. The direction is not normalized.
		 * \returns Direction to move from a \a point, returns Vector3.zero if #vectorPath is null or has a length of 0
		 * \deprecated
		 */
		[System.Obsolete()]
		public Vector3 GetMovementVector (Vector3 point) {
			if (vectorPath == null || vectorPath.Count == 0) {
				return Vector3.zero;
			}

			if (vectorPath.Count == 1) {
				return vectorPath[0]-point;
			}

			float minDist = float.PositiveInfinity;//Mathf.Infinity;
			int minSegment = 0;

			for (int i = 0; i < vectorPath.Count-1; i++) {
				Vector3 closest = VectorMath.ClosestPointOnSegment(vectorPath[i], vectorPath[i+1], point);
				float dist = (closest-point).sqrMagnitude;
				if (dist < minDist) {
					minDist = dist;
					minSegment = i;
				}
			}

			return vectorPath[minSegment+1]-point;
		}

		/** \endcond */
	}
}
