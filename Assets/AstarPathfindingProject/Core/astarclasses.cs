using UnityEngine;
using System.Collections.Generic;

// Empty namespace declaration to avoid errors in the free version
// Which does not have any classes in the RVO namespace
namespace Pathfinding.RVO {}

namespace Pathfinding {
	using Pathfinding.Util;

#if UNITY_5_0
	/** Used in Unity 5.0 since the HelpURLAttribute was first added in Unity 5.1 */
	public class HelpURLAttribute : Attribute {
	}
#endif

	[System.Serializable]
	/** Stores editor colors */
	public class AstarColor {
		public Color _NodeConnection;
		public Color _UnwalkableNode;
		public Color _BoundsHandles;

		public Color _ConnectionLowLerp;
		public Color _ConnectionHighLerp;

		public Color _MeshEdgeColor;

		/** Holds user set area colors.
		 * Use GetAreaColor to get an area color */
		public Color[] _AreaColors;

		public static Color NodeConnection = new Color(1, 1, 1, 0.9F);
		public static Color UnwalkableNode = new Color(1, 0, 0, 0.5F);
		public static Color BoundsHandles = new Color(0.29F, 0.454F, 0.741F, 0.9F);

		public static Color ConnectionLowLerp = new Color(0, 1, 0, 0.5F);
		public static Color ConnectionHighLerp = new Color(1, 0, 0, 0.5F);

		public static Color MeshEdgeColor = new Color(0, 0, 0, 0.5F);

		/** Holds user set area colors.
		 * Use GetAreaColor to get an area color */
		private static Color[] AreaColors;

		/** Returns an color for an area, uses both user set ones and calculated.
		 * If the user has set a color for the area, it is used, but otherwise the color is calculated using Mathfx.IntToColor
		 * \see #AreaColors */
		public static Color GetAreaColor (uint area) {
			if (AreaColors == null || area >= AreaColors.Length) {
				return AstarMath.IntToColor((int)area, 1F);
			}
			return AreaColors[(int)area];
		}

		/** Pushes all local variables out to static ones.
		 * This is done because that makes it so much easier to access the colors during Gizmo rendering
		 * and it has a positive performance impact as well (gizmo rendering is hot code).
		 */
		public void OnEnable () {
			NodeConnection = _NodeConnection;
			UnwalkableNode = _UnwalkableNode;
			BoundsHandles = _BoundsHandles;
			ConnectionLowLerp = _ConnectionLowLerp;
			ConnectionHighLerp = _ConnectionHighLerp;
			MeshEdgeColor = _MeshEdgeColor;
			AreaColors = _AreaColors;
		}

		public AstarColor () {
			// Set default colors
			_NodeConnection = new Color(1, 1, 1, 0.9F);
			_UnwalkableNode = new Color(1, 0, 0, 0.5F);
			_BoundsHandles = new Color(0.29F, 0.454F, 0.741F, 0.9F);
			_ConnectionLowLerp = new Color(0, 1, 0, 0.5F);
			_ConnectionHighLerp = new Color(1, 0, 0, 0.5F);
			_MeshEdgeColor = new Color(0, 0, 0, 0.5F);
		}
	}


	/** Returned by graph ray- or linecasts containing info about the hit.
	 * This is the return value by the #Pathfinding.IRaycastableGraph.Linecast methods.
	 * Some members will also be initialized even if nothing was hit, see the individual member descriptions for more info.
	 *
	 * \shadowimage{linecast.png}
	 */
	public struct GraphHitInfo {
		/** Start of the line/ray.
		 * Note that the point passed to the Linecast method will be clamped to the closest point on the navmesh.
		 */
		public Vector3 origin;
		/** Hit point.
		 * In case no obstacle was hit then this will be set to the endpoint of the line.
		 */
		public Vector3 point;
		/** Node which contained the edge which was hit.
		 * If the linecast did not hit anything then this will be set to the last node along the line's path (the one which contains the endpoint).
		 *
		 * For layered grid graphs the linecast will return true (i.e: no free line of sight) if when walking the graph we ended up at X,Z coordinate for the end node
		 * but the end node was on a different level (e.g the floor below or above in a building). In this case no node edge was really hit so this field will still be null.
		 */
		public GraphNode node;
		/** Where the tangent starts. #tangentOrigin and #tangent together actually describes the edge which was hit.
		 * \shadowimage{linecast_tangent.png}
		 */
		public Vector3 tangentOrigin;
		/** Tangent of the edge which was hit.
		 * \shadowimage{linecast_tangent.png}
		 */
		public Vector3 tangent;

		/** Distance from #origin to #point */
		public float distance {
			get {
				return (point-origin).magnitude;
			}
		}

		public GraphHitInfo (Vector3 point) {
			tangentOrigin  = Vector3.zero;
			origin = Vector3.zero;
			this.point = point;
			node = null;
			tangent = Vector3.zero;
		}
	}

	/** Nearest node constraint. Constrains which nodes will be returned by the \link AstarPath.GetNearest GetNearest\endlink function */
	public class NNConstraint {
		/** Graphs treated as valid to search on.
		 * This is a bitmask meaning that bit 0 specifies whether or not the first graph in the graphs list should be able to be included in the search,
		 * bit 1 specifies whether or not the second graph should be included and so on.
		 * \code
		 * // Enables the first and third graphs to be included, but not the rest
		 * myNNConstraint.graphMask = (1 << 0) | (1 << 2);
		 * \endcode
		 * \note This does only affect which nodes are returned from a \link AstarPath.GetNearest GetNearest\endlink call, if a valid graph is connected to an invalid graph using a node link then it might be searched anyway.
		 *
		 * \see #AstarPath.GetNearest
		 * \see #SuitableGraph
		 * \see \ref bitmasks
		 */
		public int graphMask = -1;

		/** Only treat nodes in the area #area as suitable. Does not affect anything if #area is less than 0 (zero) */
		public bool constrainArea;

		/** Area ID to constrain to. Will not affect anything if less than 0 (zero) or if #constrainArea is false */
		public int area = -1;

		/** Constrain the search to only walkable or unwalkable nodes depending on #walkable. */
		public bool constrainWalkability = true;

		/** Only search for walkable or unwalkable nodes if #constrainWalkability is enabled.
		 * If true, only walkable nodes will be searched for, otherwise only unwalkable nodes will be searched for.
		 * Does not affect anything if #constrainWalkability if false.
		 */
		public bool walkable = true;

		/** if available, do an XZ check instead of checking on all axes.
		 * The navmesh/recast graph supports this.
		 *
		 * This can be important on sloped surfaces. See the image below in which the closest point for each blue point is queried for:
		 * \shadowimage{distanceXZ2.png}
		 *
		 * The navmesh/recast graphs also contain a global option for this: \link Pathfinding.NavmeshBase.nearestSearchOnlyXZ nearestSearchOnlyXZ\endlink.
		 */
		public bool distanceXZ;

		/** Sets if tags should be constrained.
		 * \see #tags
		 */
		public bool constrainTags = true;

		/** Nodes which have any of these tags set are suitable.
		 * This is a bitmask, i.e bit 0 indicates that tag 0 is good, bit 3 indicates tag 3 is good etc.
		 * \see #constrainTags
		 * \see #graphMask
		 * \see \ref bitmasks
		 */
		public int tags = -1;

		/** Constrain distance to node.
		 * Uses distance from #AstarPath.maxNearestNodeDistance.
		 * If this is false, it will completely ignore the distance limit.
		 *
		 * If there are no suitable nodes within the distance limit then the search will terminate with a null node as a result.
		 * \note This value is not used in this class, it is used by the AstarPath.GetNearest function.
		 */
		public bool constrainDistance = true;

		/** Returns whether or not the graph conforms to this NNConstraint's rules.
		 * Note that only the first 31 graphs are considered using this function.
		 * If the #graphMask has bit 31 set (i.e the last graph possible to fit in the mask), all graphs
		 * above index 31 will also be considered suitable.
		 */
		public virtual bool SuitableGraph (int graphIndex, NavGraph graph) {
			return ((graphMask >> graphIndex) & 1) != 0;
		}

		/** Returns whether or not the node conforms to this NNConstraint's rules */
		public virtual bool Suitable (GraphNode node) {
			if (constrainWalkability && node.Walkable != walkable) return false;

			if (constrainArea && area >= 0 && node.Area != area) return false;

			if (constrainTags && ((tags >> (int)node.Tag) & 0x1) == 0) return false;

			return true;
		}

		/** The default NNConstraint.
		 * Equivalent to new NNConstraint ().
		 * This NNConstraint has settings which works for most, it only finds walkable nodes
		 * and it constrains distance set by A* Inspector -> Settings -> Max Nearest Node Distance */
		public static NNConstraint Default {
			get {
				return new NNConstraint();
			}
		}

		/** Returns a constraint which does not filter the results */
		public static NNConstraint None {
			get {
				return new NNConstraint {
						   constrainWalkability = false,
						   constrainArea = false,
						   constrainTags = false,
						   constrainDistance = false,
						   graphMask = -1,
				};
			}
		}

		/** Default constructor. Equals to the property #Default */
		public NNConstraint () {
		}
	}

	/** A special NNConstraint which can use different logic for the start node and end node in a path.
	 * A PathNNConstraint can be assigned to the Path.nnConstraint field, the path will first search for the start node, then it will call #SetStart and proceed with searching for the end node (nodes in the case of a MultiTargetPath).\n
	 * The default PathNNConstraint will constrain the end point to lie inside the same area as the start point.
	 */
	public class PathNNConstraint : NNConstraint {
		public static new PathNNConstraint Default {
			get {
				return new PathNNConstraint {
						   constrainArea = true
				};
			}
		}

		/** Called after the start node has been found. This is used to get different search logic for the start and end nodes in a path */
		public virtual void SetStart (GraphNode node) {
			if (node != null) {
				area = (int)node.Area;
			} else {
				constrainArea = false;
			}
		}
	}

	/** Internal result of a nearest node query.
	 * \see NNInfo
	 */
	public struct NNInfoInternal {
		/** Closest node found.
		 * This node is not necessarily accepted by any NNConstraint passed.
		 * \see constrainedNode
		 */
		public GraphNode node;

		/** Optional to be filled in.
		 * If the search will be able to find the constrained node without any extra effort it can fill it in. */
		public GraphNode constrainedNode;

		/** The position clamped to the closest point on the #node.
		 */
		public Vector3 clampedPosition;

		/** Clamped position for the optional constrainedNode */
		public Vector3 constClampedPosition;

		public NNInfoInternal (GraphNode node) {
			this.node = node;
			constrainedNode = null;
			clampedPosition = Vector3.zero;
			constClampedPosition = Vector3.zero;

			UpdateInfo();
		}

		/** Updates #clampedPosition and #constClampedPosition from node positions */
		public void UpdateInfo () {
			clampedPosition = node != null ? (Vector3)node.position : Vector3.zero;
			constClampedPosition = constrainedNode != null ? (Vector3)constrainedNode.position : Vector3.zero;
		}
	}

	/** Result of a nearest node query */
	public struct NNInfo {
		/** Closest node */
		public readonly GraphNode node;

		/** Closest point on the navmesh.
		 * This is the query position clamped to the closest point on the #node.
		 */
		public readonly Vector3 position;

		/** Closest point on the navmesh.
		 * \deprecated This field has been renamed to #position
		 */
		[System.Obsolete("This field has been renamed to 'position'")]
		public Vector3 clampedPosition {
			get {
				return position;
			}
		}

		public NNInfo (NNInfoInternal internalInfo) {
			node = internalInfo.node;
			position = internalInfo.clampedPosition;
		}

		public static explicit operator Vector3 (NNInfo ob) {
			return ob.position;
		}

		public static explicit operator GraphNode (NNInfo ob) {
			return ob.node;
		}
	}

	/** Progress info for e.g a progressbar.
	 * Used by the scan functions in the project
	 * \see #AstarPath.ScanAsync
	 */
	public struct Progress {
		/** Current progress as a value between 0 and 1 */
		public readonly float progress;
		/** Description of what is currently being done */
		public readonly string description;

		public Progress (float progress, string description) {
			this.progress = progress;
			this.description = description;
		}

		public Progress MapTo (float min, float max, string prefix = null) {
			return new Progress(Mathf.Lerp(min, max, progress), prefix + description);
		}

		public override string ToString () {
			return progress.ToString("0.0") + " " + description;
		}
	}

	/** Graphs which can be updated during runtime */
	public interface IUpdatableGraph {
		/** Updates an area using the specified #GraphUpdateObject.
		 *
		 * Notes to implementators.
		 * This function should (in order):
		 * -# Call o.WillUpdateNode on the GUO for every node it will update, it is important that this is called BEFORE any changes are made to the nodes.
		 * -# Update walkabilty using special settings such as the usePhysics flag used with the GridGraph.
		 * -# Call Apply on the GUO for every node which should be updated with the GUO.
		 * -# Update connectivity info if appropriate (GridGraphs updates connectivity, but most other graphs don't since then the connectivity cannot be recovered later).
		 */
		void UpdateArea (GraphUpdateObject o);

		/** May be called on the Unity thread before starting the update.
		 * \see CanUpdateAsync
		 */
		void UpdateAreaInit (GraphUpdateObject o);

		/** May be called on the Unity thread after executing the update.
		 * \see CanUpdateAsync
		 */
		void UpdateAreaPost (GraphUpdateObject o);

		GraphUpdateThreading CanUpdateAsync (GraphUpdateObject o);
	}

	/** Represents a collection of settings used to update nodes in a specific region of a graph.
	 * \see AstarPath.UpdateGraphs
	 * \see \ref graph-updates
	 */
	public class GraphUpdateObject {
		/** The bounds to update nodes within.
		 * Defined in world space.
		 */
		public Bounds bounds;

		/** Controlls if a flood fill will be carried out after this GUO has been applied.
		 * Disabling this can be used to gain a performance boost, but use with care.
		 * If you are sure that a GUO will not modify walkability or connections. You can set this to false.
		 * For example when only updating penalty values it can save processing power when setting this to false. Especially on large graphs.
		 * \note If you set this to false, even though it does change e.g walkability, it can lead to paths returning that they failed even though there is a path,
		 * or the try to search the whole graph for a path even though there is none, and will in the processes use wast amounts of processing power.
		 *
		 * If using the basic GraphUpdateObject (not a derived class), a quick way to check if it is going to need a flood fill is to check if #modifyWalkability is true or #updatePhysics is true.
		 *
		 */
		public bool requiresFloodFill = true;

		/** Use physics checks to update nodes.
		 * When updating a grid graph and this is true, the nodes' position and walkability will be updated using physics checks
		 * with settings from "Collision Testing" and "Height Testing".
		 *
		 * When updating a PointGraph, setting this to true will make it re-evaluate all connections in the graph which passes through the #bounds.
		 * This has no effect when updating GridGraphs if #modifyWalkability is turned on.
		 *
		 * On RecastGraphs, having this enabled will trigger a complete recalculation of all tiles intersecting the bounds.
		 * This is quite slow (but powerful). If you only want to update e.g penalty on existing nodes, leave it disabled.
		 */
		public bool updatePhysics = true;

		/** Reset penalties to their initial values when updating grid graphs and #updatePhysics is true.
		 * If you want to keep old penalties even when you update the graph you may want to disable this option.
		 *
		 * The images below shows two overlapping graph update objects, the right one happened to be applied before the left one. They both have updatePhysics = true and are
		 * set to increase the penalty of the nodes by some amount.
		 *
		 * The first image shows the result when resetPenaltyOnPhysics is false. Both penalties are added correctly.
		 * \shadowimage{resetPenaltyOnPhysics_False.png}
		 *
		 * This second image shows when resetPenaltyOnPhysics is set to true. The first GUO is applied correctly, but then the second one (the left one) is applied
		 * and during its updating, it resets the penalties first and then adds penalty to the nodes. The result is that the penalties from both GUOs are not added together.
		 * The green patch in at the border is there because physics recalculation (recalculation of the position of the node, checking for obstacles etc.) affects a slightly larger
		 * area than the original GUO bounds because of the Grid Graph -> Collision Testing -> Diameter setting (it is enlarged by that value). So some extra nodes have their penalties reset.
		 *
		 * \shadowimage{resetPenaltyOnPhysics_True.png}
		 */
		public bool resetPenaltyOnPhysics = true;

		/** Update Erosion for GridGraphs.
		 * When enabled, erosion will be recalculated for grid graphs
		 * after the GUO has been applied.
		 *
		 * In the below image you can see the different effects you can get with the different values.\n
		 * The first image shows the graph when no GUO has been applied. The blue box is not identified as an obstacle by the graph, the reason
		 * there are unwalkable nodes around it is because there is a height difference (nodes are placed on top of the box) so erosion will be applied (an erosion value of 2 is used in this graph).
		 * The orange box is identified as an obstacle, so the area of unwalkable nodes around it is a bit larger since both erosion and collision has made
		 * nodes unwalkable.\n
		 * The GUO used simply sets walkability to true, i.e making all nodes walkable.
		 *
		 * \shadowimage{updateErosion.png}
		 *
		 * When updateErosion=True, the reason the blue box still has unwalkable nodes around it is because there is still a height difference
		 * so erosion will still be applied. The orange box on the other hand has no height difference and all nodes are set to walkable.\n
		 * \n
		 * When updateErosion=False, all nodes walkability are simply set to be walkable in this example.
		 *
		 * \see Pathfinding.GridGraph
		 */
		public bool updateErosion = true;

		/** NNConstraint to use.
		 * The Pathfinding.NNConstraint.SuitableGraph function will be called on the NNConstraint to enable filtering of which graphs to update.\n
		 * \note As the Pathfinding.NNConstraint.SuitableGraph function is A* Pathfinding Project Pro only, this variable doesn't really affect anything in the free version.
		 *
		 *
		 * \astarpro */
		public NNConstraint nnConstraint = NNConstraint.None;

		/** Penalty to add to the nodes.
		 * A penalty of 1000 is equivalent to the cost of moving 1 world unit.
		 */
		public int addPenalty;

		/** If true, all nodes' \a walkable variable will be set to #setWalkability */
		public bool modifyWalkability;

		/** If #modifyWalkability is true, the nodes' \a walkable variable will be set to this value */
		public bool setWalkability;

		/** If true, all nodes' \a tag will be set to #setTag */
		public bool modifyTag;

		/** If #modifyTag is true, all nodes' \a tag will be set to this value */
		public int setTag;

		/** Track which nodes are changed and save backup data.
		 * Used internally to revert changes if needed.
		 */
		public bool trackChangedNodes;

		/** Nodes which were updated by this GraphUpdateObject.
		 * Will only be filled if #trackChangedNodes is true.
		 * \note It might take a few frames for graph update objects to be applied.
		 * If you need this info immediately, use #AstarPath.FlushGraphUpdates.
		 */
		public List<GraphNode> changedNodes;
		private List<uint> backupData;
		private List<Int3> backupPositionData;

		/** A shape can be specified if a bounds object does not give enough precision.
		 * Note that if you set this, you should set the bounds so that it encloses the shape
		 * because the bounds will be used as an initial fast check for which nodes that should
		 * be updated.
		 */
		public GraphUpdateShape shape;

		/** Should be called on every node which is updated with this GUO before it is updated.
		 * \param node The node to save fields for. If null, nothing will be done
		 * \see #trackChangedNodes
		 */
		public virtual void WillUpdateNode (GraphNode node) {
			if (trackChangedNodes && node != null) {
				if (changedNodes == null) { changedNodes = ListPool<GraphNode>.Claim(); backupData = ListPool<uint>.Claim(); backupPositionData = ListPool<Int3>.Claim(); }
				changedNodes.Add(node);
				backupPositionData.Add(node.position);
				backupData.Add(node.Penalty);
				backupData.Add(node.Flags);
				var gridNode = node as GridNode;
				if (gridNode != null) backupData.Add(gridNode.InternalGridFlags);
			}
		}

		/** Reverts penalties and flags (which includes walkability) on every node which was updated using this GUO.
		 * Data for reversion is only saved if #trackChangedNodes is true.
		 *
		 * \note Not all data is saved. The saved data includes: penalties, walkability, tags, area, position and for grid graphs (not layered) it also includes connection data.
		 */
		public virtual void RevertFromBackup () {
			if (trackChangedNodes) {
				if (changedNodes == null) return;

				int counter = 0;
				for (int i = 0; i < changedNodes.Count; i++) {
					changedNodes[i].Penalty = backupData[counter];
					counter++;
					changedNodes[i].Flags = backupData[counter];
					counter++;
					var gridNode = changedNodes[i] as GridNode;
					if (gridNode != null) {
						gridNode.InternalGridFlags = (ushort)backupData[counter];
						counter++;
					}
					changedNodes[i].position = backupPositionData[i];
				}

				ListPool<GraphNode>.Release(ref changedNodes);
				ListPool<uint>.Release(ref backupData);
				ListPool<Int3>.Release(ref backupPositionData);
			} else {
				throw new System.InvalidOperationException("Changed nodes have not been tracked, cannot revert from backup. Please set trackChangedNodes to true before applying the update.");
			}
		}

		/** Updates the specified node using this GUO's settings */
		public virtual void Apply (GraphNode node) {
			if (shape == null || shape.Contains(node)) {
				//Update penalty and walkability
				node.Penalty = (uint)(node.Penalty+addPenalty);
				if (modifyWalkability) {
					node.Walkable = setWalkability;
				}

				//Update tags
				if (modifyTag) node.Tag = (uint)setTag;
			}
		}

		public GraphUpdateObject () {
		}

		/** Creates a new GUO with the specified bounds */
		public GraphUpdateObject (Bounds b) {
			bounds = b;
		}
	}

	/** Graph which has a well defined transformation from graph space to world space */
	public interface ITransformedGraph {
		GraphTransform transform { get; }
	}

	/** Graph which supports the Linecast method */
	public interface IRaycastableGraph {
		bool Linecast (Vector3 start, Vector3 end);
		bool Linecast (Vector3 start, Vector3 end, GraphNode hint);
		bool Linecast (Vector3 start, Vector3 end, GraphNode hint, out GraphHitInfo hit);
		bool Linecast (Vector3 start, Vector3 end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace);
	}

	/** Integer Rectangle.
	 * Works almost like UnityEngine.Rect but with integer coordinates
	 */
	[System.Serializable]
	public struct IntRect {
		public int xmin, ymin, xmax, ymax;

		public IntRect (int xmin, int ymin, int xmax, int ymax) {
			this.xmin = xmin;
			this.xmax = xmax;
			this.ymin = ymin;
			this.ymax = ymax;
		}

		public bool Contains (int x, int y) {
			return !(x < xmin || y < ymin || x > xmax || y > ymax);
		}

		public int Width {
			get {
				return xmax-xmin+1;
			}
		}

		public int Height {
			get {
				return ymax-ymin+1;
			}
		}

		/** Returns if this rectangle is valid.
		 * An invalid rect could have e.g xmin > xmax.
		 * Rectamgles with a zero area area invalid.
		 */
		public bool IsValid () {
			return xmin <= xmax && ymin <= ymax;
		}

		public static bool operator == (IntRect a, IntRect b) {
			return a.xmin == b.xmin && a.xmax == b.xmax && a.ymin == b.ymin && a.ymax == b.ymax;
		}

		public static bool operator != (IntRect a, IntRect b) {
			return a.xmin != b.xmin || a.xmax != b.xmax || a.ymin != b.ymin || a.ymax != b.ymax;
		}

		public override bool Equals (System.Object obj) {
			var rect = (IntRect)obj;

			return xmin == rect.xmin && xmax == rect.xmax && ymin == rect.ymin && ymax == rect.ymax;
		}

		public override int GetHashCode () {
			return xmin*131071 ^ xmax*3571 ^ ymin*3109 ^ ymax*7;
		}

		/** Returns the intersection rect between the two rects.
		 * The intersection rect is the area which is inside both rects.
		 * If the rects do not have an intersection, an invalid rect is returned.
		 * \see IsValid
		 */
		public static IntRect Intersection (IntRect a, IntRect b) {
			return new IntRect(
				System.Math.Max(a.xmin, b.xmin),
				System.Math.Max(a.ymin, b.ymin),
				System.Math.Min(a.xmax, b.xmax),
				System.Math.Min(a.ymax, b.ymax)
				);
		}

		/** Returns if the two rectangles intersect each other
		 */
		public static bool Intersects (IntRect a, IntRect b) {
			return !(a.xmin > b.xmax || a.ymin > b.ymax || a.xmax < b.xmin || a.ymax < b.ymin);
		}

		/** Returns a new rect which contains both input rects.
		 * This rectangle may contain areas outside both input rects as well in some cases.
		 */
		public static IntRect Union (IntRect a, IntRect b) {
			return new IntRect(
				System.Math.Min(a.xmin, b.xmin),
				System.Math.Min(a.ymin, b.ymin),
				System.Math.Max(a.xmax, b.xmax),
				System.Math.Max(a.ymax, b.ymax)
				);
		}

		/** Returns a new IntRect which is expanded to contain the point */
		public IntRect ExpandToContain (int x, int y) {
			return new IntRect(
				System.Math.Min(xmin, x),
				System.Math.Min(ymin, y),
				System.Math.Max(xmax, x),
				System.Math.Max(ymax, y)
				);
		}

		/** Returns a new rect which is expanded by \a range in all directions.
		 * \param range How far to expand. Negative values are permitted.
		 */
		public IntRect Expand (int range) {
			return new IntRect(xmin-range,
				ymin-range,
				xmax+range,
				ymax+range
				);
		}

		/** Matrices for rotation.
		 * Each group of 4 elements is a 2x2 matrix.
		 * The XZ position is multiplied by this.
		 * So
		 * \code
		 * //A rotation by 90 degrees clockwise, second matrix in the array
		 * (5,2) * ((0, 1), (-1, 0)) = (2,-5)
		 * \endcode
		 */
		private static readonly int[] Rotations = {
			1, 0,  //Identity matrix
			0, 1,

			0, 1,
			-1, 0,

			-1, 0,
			0, -1,

			0, -1,
			1, 0
		};

		/** Returns a new rect rotated around the origin 90*r degrees.
		 * Ensures that a valid rect is returned.
		 */
		public IntRect Rotate (int r) {
			int mx1 = Rotations[r*4+0];
			int mx2 = Rotations[r*4+1];
			int my1 = Rotations[r*4+2];
			int my2 = Rotations[r*4+3];

			int p1x = mx1*xmin + mx2*ymin;
			int p1y = my1*xmin + my2*ymin;

			int p2x = mx1*xmax + mx2*ymax;
			int p2y = my1*xmax + my2*ymax;

			return new IntRect(
				System.Math.Min(p1x, p2x),
				System.Math.Min(p1y, p2y),
				System.Math.Max(p1x, p2x),
				System.Math.Max(p1y, p2y)
				);
		}

		/** Returns a new rect which is offset by the specified amount.
		 */
		public IntRect Offset (Int2 offset) {
			return new IntRect(xmin+offset.x, ymin + offset.y, xmax + offset.x, ymax + offset.y);
		}

		/** Returns a new rect which is offset by the specified amount.
		 */
		public IntRect Offset (int x, int y) {
			return new IntRect(xmin+x, ymin + y, xmax + x, ymax + y);
		}

		public override string ToString () {
			return "[x: "+xmin+"..."+xmax+", y: " + ymin +"..."+ymax+"]";
		}

		/** Draws some debug lines representing the rect */
		public void DebugDraw (GraphTransform transform, Color color) {
			Vector3 p1 = transform.Transform(new Vector3(xmin, 0, ymin));
			Vector3 p2 = transform.Transform(new Vector3(xmin, 0, ymax));
			Vector3 p3 = transform.Transform(new Vector3(xmax, 0, ymax));
			Vector3 p4 = transform.Transform(new Vector3(xmax, 0, ymin));

			Debug.DrawLine(p1, p2, color);
			Debug.DrawLine(p2, p3, color);
			Debug.DrawLine(p3, p4, color);
			Debug.DrawLine(p4, p1, color);
		}
	}

	#region Delegates

	/* Delegate with on Path object as parameter.
	 * This is used for callbacks when a path has finished calculation.\n
	 * Example function:
	 * \snippet MiscSnippets.cs OnPathDelegate
	 */
	public delegate void OnPathDelegate (Path p);

	public delegate void OnGraphDelegate (NavGraph graph);

	public delegate void OnScanDelegate (AstarPath script);

	/** \deprecated */
	public delegate void OnScanStatus (Progress progress);

	#endregion

	#region Enums

	public enum GraphUpdateThreading {
		/** Call UpdateArea in the unity thread.
		 * This is the default value.
		 * Not compatible with SeparateThread.
		 */
		UnityThread = 0,
		/** Call UpdateArea in a separate thread. Not compatible with UnityThread. */
		SeparateThread = 1 << 0,
		/** Calls UpdateAreaInit in the Unity thread before everything else */
		UnityInit = 1 << 1,
		/** Calls UpdateAreaPost in the Unity thread after everything else.
		 * This is used together with SeparateThread to apply the result of the multithreaded
		 * calculations to the graph without modifying it at the same time as some other script
		 * might be using it (e.g calling GetNearest).
		 */
		UnityPost = 1 << 2,
		/** Combination of SeparateThread and UnityInit */
		SeparateAndUnityInit = SeparateThread | UnityInit
	}

	/** How path results are logged by the system */
	public enum PathLog {
		/** Does not log anything. This is recommended for release since logging path results has a performance overhead. */
		None,
		/** Logs basic info about the paths */
		Normal,
		/** Includes additional info */
		Heavy,
		/** Same as heavy, but displays the info in-game using GUI */
		InGame,
		/** Same as normal, but logs only paths which returned an error */
		OnlyErrors
	}

	/** How to estimate the cost from to the destination.
	 *
	 * The heuristic is the estimated cost from the current node to the target.
	 * The different heuristics have roughly the same performance except not using any heuristic at all (#None)
	 * which is usually significantly slower.
	 *
	 * In the image below you can see a comparison of the different heuristic options for an 8-connected grid and
	 * for a 4-connected grid.
	 * Note that all paths within the green area will all have the same length. The only difference between the heuristics
	 * is which of those paths of the same length that will be chosen.
	 * Note that while the Diagonal Manhattan and Manhattan options seem to behave very differently on an 8-connected grid
	 * they only do it in this case because of very small rounding errors. Usually they behave almost identically on 8-connected grids.
	 *
	 * \shadowimage{heuristic.png}
	 *
	 * Generally for a 4-connected grid graph the Manhattan option should be used as it is the true distance on a 4-connected grid.
	 * For an 8-connected grid graph the Diagonal Manhattan option is the mathematically most correct option, however the Euclidean option
	 * is often preferred, especially if you are simplifying the path afterwards using modifiers.
	 *
	 * For any graph that is not grid based the Euclidean option is the best one to use.
	 *
	 * \see <a href="https://en.wikipedia.org/wiki/A*_search_algorithm">Wikipedia: A* search_algorithm</a>
	 */
	public enum Heuristic {
		/** Manhattan distance. \see https://en.wikipedia.org/wiki/Taxicab_geometry */
		Manhattan,
		/** Manhattan distance, but allowing diagonal movement as well.
		 * \note This option is currently hard coded for the XZ plane. It will be equivalent to Manhattan distance if you try to use it in the XY plane (i.e for a 2D game).
		 */
		DiagonalManhattan,
		/** Ordinary distance. \see https://en.wikipedia.org/wiki/Euclidean_distance */
		Euclidean,
		/** Use no heuristic at all.
		 * This reduces the pathfinding algorithm to Dijkstra's algorithm.
		 * This is usually significantly slower compared to using a heuristic, which is why the A* algorithm is usually preferred over Dijkstra's algorithm.
		 * You may have to use this if you have a very non-standard graph. For example a world with a <a href="https://en.wikipedia.org/wiki/Wraparound_(video_games)">wraparound playfield</a> (think Civilization or Asteroids) and you have custom links
		 * with a zero cost from one end of the map to the other end. Usually the A* algorithm wouldn't find the wraparound links because it wouldn't think to look in that direction.
		 * \see https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
		 */
		None
	}

	/** What data to draw the graph debugging with */
	public enum GraphDebugMode {
		Areas,
		G,
		H,
		F,
		Penalty,
		Connections,
		Tags
	}

	/** Number of threads to use */
	public enum ThreadCount {
		AutomaticLowLoad = -1,
		AutomaticHighLoad = -2,
		None = 0,
		One = 1,
		Two,
		Three,
		Four,
		Five,
		Six,
		Seven,
		Eight
	}

	/** Internal state of a path in the pipeline */
	public enum PathState {
		Created = 0,
		PathQueue = 1,
		Processing = 2,
		ReturnQueue = 3,
		Returned = 4
	}

	/** State of a path request */
	public enum PathCompleteState {
		/** The path has not been calculated yet.
		 * \see #Pathfinding.Path.IsDone()
		 */
		NotCalculated = 0,
		/** The path calculation is done, but it failed.
		 * \see #Pathfinding.Path.error
		 */
		Error = 1,
		/** The path has been successfully calculated */
		Complete = 2,
		/** The path has been calculated, but only a partial path could be found.
		 * \see #Pathfinding.ABPath.calculatePartial
		 */
		Partial = 3,
	}

	/** What to do when the character is close to the destination */
	public enum CloseToDestinationMode {
		/** The character will stop as quickly as possible when within \a endReachedDistance (field that exist on most movement scripts) units from the destination */
		Stop,
		/** The character will continue to the exact position of the destination */
		ContinueToExactDestination,
	}

	/** Indicates the side of a line that a point lies on */
	public enum Side : byte {
		/** The point lies exactly on the line */
		Colinear = 0,
		/** The point lies on the left side of the line */
		Left = 1,
		/** The point lies on the right side of the line */
		Right = 2
	}

	public enum InspectorGridMode {
		Grid,
		IsometricGrid,
		Hexagonal,
		Advanced
	}

	#endregion
}
