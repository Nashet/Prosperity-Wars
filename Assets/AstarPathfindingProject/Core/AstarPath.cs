using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

#if NETFX_CORE
using Thread = Pathfinding.WindowsStore.Thread;
#else
using Thread = System.Threading.Thread;
#endif

[ExecuteInEditMode]
[AddComponentMenu("Pathfinding/Pathfinder")]
/**
 * Core component for the A* %Pathfinding System.
 * This class handles all of the pathfinding system, calculates all paths and stores the info.\n
 * This class is a singleton class, meaning there should only exist at most one active instance of it in the scene.\n
 * It might be a bit hard to use directly, usually interfacing with the pathfinding system is done through the \link Pathfinding.Seeker Seeker\endlink class.
 *
 * \nosubgrouping
 * \ingroup relevant
 */
[HelpURL("http://arongranberg.com/astar/docs/class_astar_path.php")]
public class AstarPath : VersionedMonoBehaviour {
	/** The version number for the A* %Pathfinding Project */
	public static readonly System.Version Version = new System.Version(4, 1, 16);

	/** Information about where the package was downloaded */
	public enum AstarDistribution { WebsiteDownload, AssetStore };

	/** Used by the editor to guide the user to the correct place to download updates */
	public static readonly AstarDistribution Distribution = AstarDistribution.WebsiteDownload;

	/** Which branch of the A* %Pathfinding Project is this release.
	 * Used when checking for updates so that
	 * users of the development versions can get notifications of development
	 * updates.
	 */
	public static readonly string Branch = "master_Free";

	/** See Pathfinding.AstarData
	 * \deprecated
	 */
	[System.Obsolete]
	public System.Type[] graphTypes {
		get {
			return data.graphTypes;
		}
	}

	/** Holds all graph data */
	[UnityEngine.Serialization.FormerlySerializedAs("astarData")]
	public AstarData data;

	/** Holds all graph data.
	 * \deprecated The 'astarData' field has been renamed to 'data'
	 */
	[System.Obsolete("The 'astarData' field has been renamed to 'data'")]
	public AstarData astarData { get { return data; } }

	/** Returns the active AstarPath object in the scene.
	 * \note This is only set if the AstarPath object has been initialized (which happens in Awake).
	 */
#if UNITY_4_6 || UNITY_4_3
	public static new AstarPath active;
#else
	public static AstarPath active;
#endif

	/** Shortcut to Pathfinding.AstarData.graphs */
	public NavGraph[] graphs {
		get {
			if (data == null)
				data = new AstarData();
			return data.graphs;
		}
	}

	#region InspectorDebug
	/** @name Inspector - Debug
	 * @{ */

	/** Toggle for showing the gizmo debugging for the graphs in the scene view (editor only). */
	public bool showNavGraphs = true;

	/** Toggle to show unwalkable nodes.
	 *
	 * \note Only relevant in the editor
	 *
	 * \see #unwalkableNodeDebugSize
	 */
	public bool showUnwalkableNodes = true;

	/** The mode to use for drawing nodes in the sceneview.
	 *
	 * \note Only relevant in the editor
	 *
	 * \see Pathfinding.GraphDebugMode
	 */
	public GraphDebugMode debugMode;

	/** Low value to use for certain #debugMode modes.
	 * For example if #debugMode is set to G, this value will determine when the node will be completely red.
	 *
	 * \note Only relevant in the editor
	 *
	 * \see #debugRoof
	 * \see #debugMode
	 */
	public float debugFloor = 0;

	/** High value to use for certain #debugMode modes.
	 * For example if #debugMode is set to G, this value will determine when the node will be completely green.
	 *
	 * For the penalty debug mode, the nodes will be colored green when they have a penalty less than #debugFloor and red
	 * when their penalty is greater or equal to this value and something between red and green otherwise.
	 *
	 * \note Only relevant in the editor
	 *
	 * \see #debugFloor
	 * \see #debugMode
	 */
	public float debugRoof = 20000;

	/** If set, the #debugFloor and #debugRoof values will not be automatically recalculated.
	 *
	 * \note Only relevant in the editor
	 */
	public bool manualDebugFloorRoof = false;


	/** If enabled, nodes will draw a line to their 'parent'.
	 * This will show the search tree for the latest path.
	 *
	 * \note Only relevant in the editor
	 *
	 * \todo Add a showOnlyLastPath flag to indicate whether to draw every node or only the ones visited by the latest path.
	 */
	public bool showSearchTree = false;

	/** Size of the red cubes shown in place of unwalkable nodes.
	 *
	 * \note Only relevant in the editor. Does not apply to grid graphs.
	 * \see #showUnwalkableNodes
	 */
	public float unwalkableNodeDebugSize = 0.3F;

	/** The amount of debugging messages.
	 * Use less debugging to improve performance (a bit) or just to get rid of the Console spamming.
	 * Use more debugging (heavy) if you want more information about what the pathfinding scripts are doing.
	 * The InGame option will display the latest path log using in-game GUI.
	 *
	 * \shadowimage{path_logging.png}
	 */
	public PathLog logPathResults = PathLog.Normal;

	/** @} */
	#endregion

	#region InspectorSettings
	/** @name Inspector - Settings
	 * @{ */

	/** Maximum distance to search for nodes.
	 * When searching for the nearest node to a point, this is the limit (in world units) for how far away it is allowed to be.
	 *
	 * This is relevant if you try to request a path to a point that cannot be reached and it thus has to search for
	 * the closest node to that point which can be reached (which might be far away). If it cannot find a node within this distance
	 * then the path will fail.
	 *
	 * \shadowimage{max_nearest_node_distance.png}
	 *
	 * \see Pathfinding.NNConstraint.constrainDistance
	 */
	public float maxNearestNodeDistance = 100;

	/** Max Nearest Node Distance Squared.
	 * \see #maxNearestNodeDistance */
	public float maxNearestNodeDistanceSqr {
		get { return maxNearestNodeDistance*maxNearestNodeDistance; }
	}

	/** If true, all graphs will be scanned during Awake.
	 * This does not include loading from the cache.
	 * If you disable this, you will have to call \link Scan AstarPath.active.Scan() \endlink yourself to enable pathfinding.
	 * Alternatively you could load a saved graph from a file.
	 *
	 * \see #Scan
	 * \see #ScanAsync
	 */
	public bool scanOnStartup = true;

	/** Do a full GetNearest search for all graphs.
	 * Additional searches will normally only be done on the graph which in the first fast search seemed to have the closest node.
	 * With this setting on, additional searches will be done on all graphs since the first check is not always completely accurate.\n
	 * More technically: GetNearestForce on all graphs will be called if true, otherwise only on the one graph which's GetNearest search returned the best node.\n
	 * Usually faster when disabled, but higher quality searches when enabled.
	 * When using a a navmesh or recast graph, for best quality, this setting should be combined with the Pathfinding.NavMeshGraph.accurateNearestNode setting set to true.
	 * \note For the PointGraph this setting doesn't matter much as it has only one search mode.
	 */
	public bool fullGetNearestSearch = false;

	/** Prioritize graphs.
	 * Graphs will be prioritized based on their order in the inspector.
	 * The first graph which has a node closer than #prioritizeGraphsLimit will be chosen instead of searching all graphs.
	 */
	public bool prioritizeGraphs = false;

	/** Distance limit for #prioritizeGraphs.
	 * \see #prioritizeGraphs
	 */
	public float prioritizeGraphsLimit = 1F;

	/** Reference to the color settings for this AstarPath object.
	 * Color settings include for example which color the nodes should be in, in the sceneview. */
	public AstarColor colorSettings;

	/** Stored tag names.
	 * \see AstarPath.FindTagNames
	 * \see AstarPath.GetTagNames
	 */
	[SerializeField]
	protected string[] tagNames = null;

	/** The distance function to use as a heuristic.
	 * The heuristic, often referred to as just 'H' is the estimated cost from a node to the target.
	 * Different heuristics affect how the path picks which one to follow from multiple possible with the same length
	 * \see #Pathfinding.Heuristic for more details and descriptions of the different modes.
	 * \see <a href="https://en.wikipedia.org/wiki/Admissible_heuristic">Wikipedia: Admissible heuristic</a>
	 * \see <a href="https://en.wikipedia.org/wiki/A*_search_algorithm">Wikipedia: A* search algorithm</a>
	 * \see <a href="https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm">Wikipedia: Dijkstra's Algorithm</a>
	 */
	public Heuristic heuristic = Heuristic.Euclidean;

	/** The scale of the heuristic.
	 * If a value lower than 1 is used, the pathfinder will search more nodes (slower).
	 * If 0 is used, the pathfinding algorithm will be reduced to dijkstra's algorithm. This is equivalent to setting #heuristic to None.
	 * If a value larger than 1 is used the pathfinding will (usually) be faster because it expands fewer nodes, but the paths may no longer be the optimal (i.e the shortest possible paths).
	 *
	 * Usually you should leave this to the default value of 1.
	 *
	 * \see https://en.wikipedia.org/wiki/Admissible_heuristic
	 * \see https://en.wikipedia.org/wiki/A*_search_algorithm
	 * \see https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
	 */
	public float heuristicScale = 1F;

	/** Number of pathfinding threads to use.
	 * Multithreading puts pathfinding in another thread, this is great for performance on 2+ core computers since the framerate will barely be affected by the pathfinding at all.
	 * - None indicates that the pathfinding is run in the Unity thread as a coroutine
	 * - Automatic will try to adjust the number of threads to the number of cores and memory on the computer.
	 *  Less than 512mb of memory or a single core computer will make it revert to using no multithreading.
	 *
	 * It is recommended that you use one of the "Auto" settings that are available.
	 * The reason is that even if your computer might be beefy and have 8 cores.
	 * Other computers might only be quad core or dual core in which case they will not benefit from more than
	 * 1 or 3 threads respectively (you usually want to leave one core for the unity thread).
	 * If you use more threads than the number of cores on the computer it is mostly just wasting memory, it will not run any faster.
	 * The extra memory usage is not trivially small. Each thread needs to keep a small amount of data for each node in all the graphs.
	 * It is not the full graph data but it is proportional to the number of nodes.
	 * The automatic settings will inspect the machine it is running on and use that to determine the number of threads so that no memory is wasted.
	 *
	 * The exception is if you only have one (or maybe two characters) active at time. Then you should probably just go with one thread always since it is very unlikely
	 * that you will need the extra throughput given by more threads. Keep in mind that more threads primarily increases throughput by calculating different paths on different
	 * threads, it will not calculate individual paths any faster.
	 *
	 * Note that if you are modifying the pathfinding core scripts or if you are directly modifying graph data without using any of the
	 * safe wrappers (like #AddWorkItem) multithreading can cause strange errors and pathfinding stopping to work if you are not careful.
	 * For basic usage (not modding the pathfinding core) it should be safe.
	 *
	 * \note WebGL does not support threads at all (since javascript is single-threaded) so no threads will be used on that platform.
	 *
	 * \see CalculateThreadCount
	 *
	 * \astarpro
	 */
	public ThreadCount threadCount = ThreadCount.One;

	/** Max number of milliseconds to spend each frame for pathfinding.
	 * At least 500 nodes will be searched each frame (if there are that many to search).
	 * When using multithreading this value is irrelevant.
	 */
	public float maxFrameTime = 1F;

	/** Defines the minimum amount of nodes in an area.
	 * If an area has less than this amount of nodes, the area will be flood filled again with the area ID GraphNode.MaxAreaIndex-1,
	 * it shouldn't affect pathfinding in any significant way.\n
	 * If you want to be able to separate areas from one another for some reason (for example to do a fast check to see if a path is at all possible)
	 * you should set this variable to 0.
	 *
	 * \version Since version 3.6, this variable should in most cases be set to 0 since the max number of area indices available has been greatly increased.
	 * \deprecated This is handled automatically now
	 */
	[System.Obsolete("Minimum area size is mostly obsolete since the limit has been raised significantly, and the edge cases are handled automatically")]
	public int minAreaSize = 0;

	/** Throttle graph updates and batch them to improve performance.
	 * If toggled, graph updates will batched and executed less often (specified by #graphUpdateBatchingInterval).
	 *
	 * This can have a positive impact on pathfinding throughput since the pathfinding threads do not need
	 * to be stopped as often, and it reduces the overhead per graph update.
	 * All graph updates are still applied however, they are just batched together so that more of them are
	 * applied at the same time.
	 *
	 * However do not use this if you want minimal latency between a graph update being requested
	 * and it being applied.
	 *
	 * This only applies to graph updates requested using the #UpdateGraphs method. Not those requested
	 * using #RegisterSafeUpdate or #AddWorkItem.
	 *
	 * \see \ref graph-updates
	 */
	public bool batchGraphUpdates = false;

	/** Minimum number of seconds between each batch of graph updates.
	 * If #batchGraphUpdates is true, this defines the minimum number of seconds between each batch of graph updates.
	 *
	 * This can have a positive impact on pathfinding throughput since the pathfinding threads do not need
	 * to be stopped as often, and it reduces the overhead per graph update.
	 * All graph updates are still applied however, they are just batched together so that more of them are
	 * applied at the same time.
	 *
	 * Do not use this if you want minimal latency between a graph update being requested
	 * and it being applied.
	 *
	 * This only applies to graph updates requested using the #UpdateGraphs method. Not those requested
	 * using #RegisterSafeUpdate or #AddWorkItem.
	 *
	 * \see \ref graph-updates
	 */
	public float graphUpdateBatchingInterval = 0.2F;

	/** Batch graph updates.
	 * \deprecated This field has been renamed to #batchGraphUpdates.
	 */
	[System.Obsolete("This field has been renamed to 'batchGraphUpdates'")]
	public bool limitGraphUpdates { get { return batchGraphUpdates; } set { batchGraphUpdates = value; } }

	/** Limit for how often should graphs be updated.
	 * \deprecated This field has been renamed to #graphUpdateBatchingInterval.
	 */
	[System.Obsolete("This field has been renamed to 'graphUpdateBatchingInterval'")]
	public float maxGraphUpdateFreq { get { return graphUpdateBatchingInterval; } set { graphUpdateBatchingInterval = value; } }

	/** @} */
	#endregion

	#region DebugVariables
	/** @name Debug Members
	 * @{ */


	/** The time it took for the last call to Scan() to complete.
	 * Used to prevent automatically rescanning the graphs too often (editor only)
	 */
	public float lastScanTime { get; private set; }

	/** The path to debug using gizmos.
	 * This is the path handler used to calculate the last path.
	 * It is used in the editor to draw debug information using gizmos.
	 */
	[System.NonSerialized]
	public PathHandler debugPathData;

	/** The path ID to debug using gizmos */
	[System.NonSerialized]
	public ushort debugPathID;

	/** Debug string from the last completed path.
	 * Will be updated if #logPathResults == PathLog.InGame
	 */
	string inGameDebugPath;

	/* @} */
	#endregion

	#region StatusVariables

	/** Backing field for #isScanning.
	 * Cannot use an auto-property because they cannot be marked with System.NonSerialized.
	 */
	[System.NonSerialized]
	bool isScanningBacking;

	/** Set while any graphs are being scanned.
	 * It will be true up until the FloodFill is done.
	 *
	 * \note Not to be confused with graph updates.
	 *
	 * Used to better support Graph Update Objects called for example in OnPostScan
	 *
	 * \see IsAnyGraphUpdateQueued
	 * \see IsAnyGraphUpdateInProgress
	 */
	public bool isScanning { get { return isScanningBacking; } private set { isScanningBacking = value; } }

	/** Number of parallel pathfinders.
	 * Returns the number of concurrent processes which can calculate paths at once.
	 * When using multithreading, this will be the number of threads, if not using multithreading it is always 1 (since only 1 coroutine is used).
	 * \see IsUsingMultithreading
	 */
	public int NumParallelThreads {
		get {
			return pathProcessor.NumThreads;
		}
	}

	/** Returns whether or not multithreading is used.
	 * \exception System.Exception Is thrown when it could not be decided if multithreading was used or not.
	 * This should not happen if pathfinding is set up correctly.
	 * \note This uses info about if threads are running right now, it does not use info from the settings on the A* object.
	 */
	public bool IsUsingMultithreading {
		get {
			return pathProcessor.IsUsingMultithreading;
		}
	}

	/** Returns if any graph updates are waiting to be applied.
	 * \deprecated Use IsAnyGraphUpdateQueued instead
	 */
	[System.Obsolete("Fixed grammar, use IsAnyGraphUpdateQueued instead")]
	public bool IsAnyGraphUpdatesQueued { get { return IsAnyGraphUpdateQueued; } }

	/** Returns if any graph updates are waiting to be applied.
	 * \note This is false while the updates are being performed.
	 * \note This does *not* includes other types of work items such as navmesh cutting or anything added by #RegisterSafeUpdate or #AddWorkItem.
	 */
	public bool IsAnyGraphUpdateQueued { get { return graphUpdates.IsAnyGraphUpdateQueued; } }

	/** Returns if any graph updates are being calculated right now.
	 * \note This does *not* includes other types of work items such as navmesh cutting or anything added by #RegisterSafeUpdate or #AddWorkItem.
	 *
	 * \see IsAnyWorkItemInProgress
	 */
	public bool IsAnyGraphUpdateInProgress { get { return graphUpdates.IsAnyGraphUpdateInProgress; } }

	/** Returns if any work items are in progress right now.
	 * \note This includes pretty much all types of graph updates.
	 * Such as normal graph updates, navmesh cutting and anything added by #RegisterSafeUpdate or #AddWorkItem.
	 */
	public bool IsAnyWorkItemInProgress { get { return workItems.workItemsInProgress; } }

	/** Returns if this code is currently being exectuted inside a work item.
	 * \note This includes pretty much all types of graph updates.
	 * Such as normal graph updates, navmesh cutting and anything added by #RegisterSafeUpdate or #AddWorkItem.
	 *
	 * In contrast to #IsAnyWorkItemInProgress this is only true when work item code is being executed, it is not
	 * true in-between the updates to a work item that takes several frames to complete.
	 */
	internal bool IsInsideWorkItem { get { return workItems.workItemsInProgressRightNow; } }

	#endregion

	#region Callbacks
	/** @name Callbacks */
	/* Callbacks to pathfinding events.
	 * These allow you to hook in to the pathfinding process.\n
	 * Callbacks can be used like this:
	 * \snippet MiscSnippets.cs AstarPath.Callbacks
	 */
	/** @{ */

	/** Called on Awake before anything else is done.
	 * This is called at the start of the Awake call, right after #active has been set, but this is the only thing that has been done.\n
	 * Use this when you want to set up default settings for an AstarPath component created during runtime since some settings can only be changed in Awake
	 * (such as multithreading related stuff)
	 * \snippet MiscSnippets.cs AstarPath.OnAwakeSettings
	 */
	public static System.Action OnAwakeSettings;

	/** Called for each graph before they are scanned */
	public static OnGraphDelegate OnGraphPreScan;

	/** Called for each graph after they have been scanned. All other graphs might not have been scanned yet. */
	public static OnGraphDelegate OnGraphPostScan;

	/** Called for each path before searching. Be careful when using multithreading since this will be called from a different thread. */
	public static OnPathDelegate OnPathPreSearch;

	/** Called for each path after searching. Be careful when using multithreading since this will be called from a different thread. */
	public static OnPathDelegate OnPathPostSearch;

	/** Called before starting the scanning */
	public static OnScanDelegate OnPreScan;

	/** Called after scanning. This is called before applying links, flood-filling the graphs and other post processing. */
	public static OnScanDelegate OnPostScan;

	/** Called after scanning has completed fully. This is called as the last thing in the Scan function. */
	public static OnScanDelegate OnLatePostScan;

	/** Called when any graphs are updated. Register to for example recalculate the path whenever a graph changes. */
	public static OnScanDelegate OnGraphsUpdated;

	/**
	 * Called when \a pathID overflows 65536 and resets back to zero.
	 * \note This callback will be cleared every time it is called, so if you want to register to it repeatedly, register to it directly on receiving the callback as well.
	 */
	public static System.Action On65KOverflow;

	/** \deprecated */
	[System.ObsoleteAttribute]
	public System.Action OnGraphsWillBeUpdated;

	/** \deprecated */
	[System.ObsoleteAttribute]
	public System.Action OnGraphsWillBeUpdated2;

	/* @} */
	#endregion

	#region MemoryStructures

	/** Processes graph updates */
	readonly GraphUpdateProcessor graphUpdates;

	/** Processes work items */
	readonly WorkItemProcessor workItems;

	/** Holds all paths waiting to be calculated and calculates them */
	PathProcessor pathProcessor;

	bool graphUpdateRoutineRunning = false;

	/** Makes sure QueueGraphUpdates will not queue multiple graph update orders */
	bool graphUpdatesWorkItemAdded = false;

	/** Time the last graph update was done.
	 * Used to group together frequent graph updates to batches
	 */
	float lastGraphUpdate = -9999F;

	/** Held if any work items are currently queued */
	PathProcessor.GraphUpdateLock workItemLock;

	/** Holds all completed paths waiting to be returned to where they were requested */
	internal readonly PathReturnQueue pathReturnQueue;

	/** Holds settings for heuristic optimization.
	 * \see heuristic-opt
	 *
	 * \astarpro
	 */
	public EuclideanEmbedding euclideanEmbedding = new EuclideanEmbedding();

	#endregion

	/** Shows or hides graph inspectors.
	 * Used internally by the editor
	 */
	public bool showGraphs = false;

	/** The next unused Path ID.
	 * Incremented for every call to GetNextPathID
	 */
	private ushort nextFreePathID = 1;

	private AstarPath () {
		pathReturnQueue = new PathReturnQueue(this);

		// Make sure that the pathProcessor is never null
		pathProcessor = new PathProcessor(this, pathReturnQueue, 1, false);

		workItems = new WorkItemProcessor(this);
		graphUpdates = new GraphUpdateProcessor(this);

		// Forward graphUpdates.OnGraphsUpdated to AstarPath.OnGraphsUpdated
		graphUpdates.OnGraphsUpdated += () => {
			if (OnGraphsUpdated != null) {
				OnGraphsUpdated(this);
			}
		};
	}

	/** Returns tag names.
	 * Makes sure that the tag names array is not null and of length 32.
	 * If it is null or not of length 32, it creates a new array and fills it with 0,1,2,3,4 etc...
	 * \see AstarPath.FindTagNames
	 */
	public string[] GetTagNames () {
		if (tagNames == null || tagNames.Length != 32) {
			tagNames = new string[32];
			for (int i = 0; i < tagNames.Length; i++) {
				tagNames[i] = ""+i;
			}
			tagNames[0] = "Basic Ground";
		}
		return tagNames;
	}

	/** Used outside of play mode to initialize the AstarPath object even if it has not been selected in the inspector yet.
	 * This will set the #active property and deserialize all graphs.
	 *
	 * This is useful if you want to do changes to the graphs in the editor outside of play mode, but cannot be sure that the graphs have been deserialized yet.
	 * In play mode this method does nothing.
	 */
	public static void FindAstarPath () {
		if (Application.isPlaying) return;
		if (active == null) active = GameObject.FindObjectOfType<AstarPath>();
		if (active != null && (active.data.graphs == null || active.data.graphs.Length == 0)) active.data.DeserializeGraphs();
	}

	/** Tries to find an AstarPath object and return tag names.
	 * If an AstarPath object cannot be found, it returns an array of length 1 with an error message.
	 * \see AstarPath.GetTagNames
	 */
	public static string[] FindTagNames () {
		FindAstarPath();
		return active != null ? active.GetTagNames() : new string[1] { "There is no AstarPath component in the scene" };
	}

	/** Returns the next free path ID */
	internal ushort GetNextPathID () {
		if (nextFreePathID == 0) {
			nextFreePathID++;

			if (On65KOverflow != null) {
				System.Action tmp = On65KOverflow;
				On65KOverflow = null;
				tmp();
			}
		}
		return nextFreePathID++;
	}

	void RecalculateDebugLimits () {
		debugFloor = float.PositiveInfinity;
		debugRoof = float.NegativeInfinity;

		bool ignoreSearchTree = !showSearchTree || debugPathData == null;
		for (int i = 0; i < graphs.Length; i++) {
			if (graphs[i] != null && graphs[i].drawGizmos) {
				graphs[i].GetNodes(node => {
					if (ignoreSearchTree || Pathfinding.Util.GraphGizmoHelper.InSearchTree(node, debugPathData, debugPathID)) {
						if (debugMode == GraphDebugMode.Penalty) {
							debugFloor = Mathf.Min(debugFloor, node.Penalty);
							debugRoof = Mathf.Max(debugRoof, node.Penalty);
						} else if (debugPathData != null) {
							var rnode = debugPathData.GetPathNode(node);
							switch (debugMode) {
							case GraphDebugMode.F:
								debugFloor = Mathf.Min(debugFloor, rnode.F);
								debugRoof = Mathf.Max(debugRoof, rnode.F);
								break;
							case GraphDebugMode.G:
								debugFloor = Mathf.Min(debugFloor, rnode.G);
								debugRoof = Mathf.Max(debugRoof, rnode.G);
								break;
							case GraphDebugMode.H:
								debugFloor = Mathf.Min(debugFloor, rnode.H);
								debugRoof = Mathf.Max(debugRoof, rnode.H);
								break;
							}
						}
					}
				});
			}
		}

		if (float.IsInfinity(debugFloor)) {
			debugFloor = 0;
			debugRoof = 1;
		}

		// Make sure they are not identical, that will cause the color interpolation to fail
		if (debugRoof-debugFloor < 1) debugRoof += 1;
	}

	Pathfinding.Util.RetainedGizmos gizmos = new Pathfinding.Util.RetainedGizmos();

	/** Calls OnDrawGizmos on graph generators */
	private void OnDrawGizmos () {
		// Make sure the singleton pattern holds
		// Might not hold if the Awake method
		// has not been called yet
		if (active == null) active = this;

		if (active != this || graphs == null) {
			return;
		}

		// In Unity one can select objects in the scene view by simply clicking on them with the mouse.
		// Graph gizmos interfere with this however. If we would draw a mesh here the user would
		// not be able to select whatever was behind it because the gizmos would block them.
		// (presumably Unity cannot associate the gizmos with the AstarPath component because we are using
		// Graphics.DrawMeshNow to draw most gizmos). It turns out that when scene picking happens
		// then Event.current.type will be 'mouseUp'. We will therefore ignore all events which are
		// not repaint events to make sure that the gizmos do not interfere with any kind of scene picking.
		// This will not have any visual impact as only repaint events will result in any changes on the screen.
		// From testing it seems the only events that can happen during OnDrawGizmos are the mouseUp and repaint events.
		if (Event.current.type != EventType.Repaint) return;

		AstarProfiler.StartProfile("OnDrawGizmos");

		if (workItems.workItemsInProgress || isScanning) {
			// If updating graphs, graph info might not be valid right now
			// so just draw the same thing as last frame
			gizmos.DrawExisting();
		} else {
			if (showNavGraphs && !manualDebugFloorRoof) {
				RecalculateDebugLimits();
			}

			Profiler.BeginSample("Graph.OnDrawGizmos");
			// Loop through all graphs and draw their gizmos
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] != null && graphs[i].drawGizmos)
					graphs[i].OnDrawGizmos(gizmos, showNavGraphs);
			}
			Profiler.EndSample();

			if (showNavGraphs) {
				euclideanEmbedding.OnDrawGizmos();
			}
		}

		gizmos.FinalizeDraw();

		AstarProfiler.EndProfile("OnDrawGizmos");
	}

	/** Draws the InGame debugging (if enabled), also shows the fps if 'L' is pressed down.
	 * \see #logPathResults PathLog
	 */
	private void OnGUI () {
		if (logPathResults == PathLog.InGame && inGameDebugPath != "") {
			GUI.Label(new Rect(5, 5, 400, 600), inGameDebugPath);
		}
	}

	/** Prints path results to the log. What it prints can be controled using #logPathResults.
	 * \see #logPathResults
	 * \see PathLog
	 * \see Pathfinding.Path.DebugString
	 */
	private void LogPathResults (Path path) {
		if (logPathResults != PathLog.None && (path.error || logPathResults != PathLog.OnlyErrors)) {
			string debug = path.DebugString(logPathResults);

			if (logPathResults == PathLog.InGame) {
				inGameDebugPath = debug;
			} else if (path.error) {
				Debug.LogWarning(debug);
			} else {
				Debug.Log(debug);
			}
		}
	}

	/**
	 * Checks if any work items need to be executed
	 * then runs pathfinding for a while (if not using multithreading because
	 * then the calculation happens in other threads)
	 * and then returns any calculated paths to the
	 * scripts that requested them.
	 *
	 * \see PerformBlockingActions
	 * \see PathProcessor.TickNonMultithreaded
	 * \see PathReturnQueue.ReturnPaths
	 */
	private void Update () {
		// This class uses the [ExecuteInEditMode] attribute
		// So Update is called even when not playing
		// Don't do anything when not in play mode
		if (!Application.isPlaying) return;

		// Execute blocking actions such as graph updates
		// when not scanning
		if (!isScanning) {
			PerformBlockingActions();
		}

		// Calculates paths when not using multithreading
		pathProcessor.TickNonMultithreaded();

		// Return calculated paths
		pathReturnQueue.ReturnPaths(true);
	}

	private void PerformBlockingActions (bool force = false) {
		if (workItemLock.Held && pathProcessor.queue.AllReceiversBlocked) {
			// Return all paths before starting blocking actions
			// since these might change the graph and make returned paths invalid (at least the nodes)
			pathReturnQueue.ReturnPaths(false);

			if (workItems.ProcessWorkItems(force)) {
				// At this stage there are no more work items, resume pathfinding threads
				workItemLock.Release();
			}
		}
	}

	/** Call during work items to queue a flood fill.
	 * \deprecated This method has been moved. Use the method on the context object that can be sent with work item delegates instead
	 * \snippet MiscSnippets.cs AstarPath.AddWorkItem1
	 *
	 * \see #Pathfinding.IWorkItemContext
	 */
	[System.Obsolete("This method has been moved. Use the method on the context object that can be sent with work item delegates instead")]
	public void QueueWorkItemFloodFill () {
		throw new System.Exception("This method has been moved. Use the method on the context object that can be sent with work item delegates instead");
	}

	/** If a WorkItem needs to have a valid flood fill during execution, call this method to ensure there are no pending flood fills.
	 * \deprecated This method has been moved. Use the method on the context object that can be sent with work item delegates instead
	 * \snippet MiscSnippets.cs AstarPath.AddWorkItem1
	 *
	 * \see #Pathfinding.IWorkItemContext
	 */
	[System.Obsolete("This method has been moved. Use the method on the context object that can be sent with work item delegates instead")]
	public void EnsureValidFloodFill () {
		throw new System.Exception("This method has been moved. Use the method on the context object that can be sent with work item delegates instead");
	}

	/** Add a work item to be processed when pathfinding is paused.
	 * Convenience method that is equivalent to
	 * \code
	 * AddWorkItem(new AstarWorkItem(callback));
	 * \endcode
	 *
	 * \see #AddWorkItem(AstarWorkItem)
	 */
	public void AddWorkItem (System.Action callback) {
		AddWorkItem(new AstarWorkItem(callback));
	}

	/** Add a work item to be processed when pathfinding is paused.
	 * Convenience method that is equivalent to
	 * \code
	 * AddWorkItem(new AstarWorkItem(callback));
	 * \endcode
	 *
	 * \see #AddWorkItem(AstarWorkItem)
	 */
	public void AddWorkItem (System.Action<IWorkItemContext> callback) {
		AddWorkItem(new AstarWorkItem(callback));
	}

	/** Add a work item to be processed when pathfinding is paused.
	 *
	 * The work item will be executed when it is safe to update nodes. This is defined as between the path searches.
	 * When using more threads than one, calling this often might decrease pathfinding performance due to a lot of idling in the threads.
	 * Not performance as in it will use much CPU power, but performance as in the number of paths per second will probably go down
	 * (though your framerate might actually increase a tiny bit).
	 *
	 * You should only call this function from the main unity thread (i.e normal game code).
	 *
	 * \snippet MiscSnippets.cs AstarPath.AddWorkItem1
	 *
	 * \snippet MiscSnippets.cs AstarPath.AddWorkItem2
	 *
	 * \see #FlushWorkItems
	 */
	public void AddWorkItem (AstarWorkItem item) {
		workItems.AddWorkItem(item);

		// Make sure pathfinding is stopped and work items are processed
		if (!workItemLock.Held) {
			workItemLock = PausePathfindingSoon();
		}

#if UNITY_EDITOR
		// If not playing, execute instantly
		if (!Application.isPlaying) {
			FlushWorkItems();
		}
#endif
	}

	#region GraphUpdateMethods

	/** Will apply queued graph updates as soon as possible, regardless of #batchGraphUpdates.
	 * Calling this multiple times will not create multiple callbacks.
	 * This function is useful if you are limiting graph updates, but you want a specific graph update to be applied as soon as possible regardless of the time limit.
	 * Note that this does not block until the updates are done, it merely bypasses the #batchGraphUpdates time limit.
	 *
	 * \see #FlushGraphUpdates
	 */
	public void QueueGraphUpdates () {
		if (!graphUpdatesWorkItemAdded) {
			graphUpdatesWorkItemAdded = true;
			var workItem = graphUpdates.GetWorkItem();

			// Add a new work item which first
			// sets the graphUpdatesWorkItemAdded flag to false
			// and then processes the graph updates
			AddWorkItem(new AstarWorkItem(() => {
				graphUpdatesWorkItemAdded = false;
				lastGraphUpdate = Time.realtimeSinceStartup;

				workItem.init();
			}, workItem.update));
		}
	}

	/** Waits a moment with updating graphs.
	 * If batchGraphUpdates is set, we want to keep some space between them to let pathfinding threads running and then calculate all queued calls at once
	 */
	IEnumerator DelayedGraphUpdate () {
		graphUpdateRoutineRunning = true;

		yield return new WaitForSeconds(graphUpdateBatchingInterval-(Time.realtimeSinceStartup-lastGraphUpdate));
		QueueGraphUpdates();
		graphUpdateRoutineRunning = false;
	}

	/** Update all graphs within \a bounds after \a delay seconds.
	 * The graphs will be updated as soon as possible.
	 *
	 * \see FlushGraphUpdates
	 * \see batchGraphUpdates
	 * \see \ref graph-updates
	 */
	public void UpdateGraphs (Bounds bounds, float delay) {
		UpdateGraphs(new GraphUpdateObject(bounds), delay);
	}

	/** Update all graphs using the GraphUpdateObject after \a delay seconds.
	 * This can be used to, e.g make all nodes in a region unwalkable, or set them to a higher penalty.
	 *
	 * \see FlushGraphUpdates
	 * \see batchGraphUpdates
	 * \see \ref graph-updates
	 */
	public void UpdateGraphs (GraphUpdateObject ob, float delay) {
		StartCoroutine(UpdateGraphsInternal(ob, delay));
	}

	/** Update all graphs using the GraphUpdateObject after \a delay seconds */
	IEnumerator UpdateGraphsInternal (GraphUpdateObject ob, float delay) {
		yield return new WaitForSeconds(delay);
		UpdateGraphs(ob);
	}

	/** Update all graphs within \a bounds.
	 * The graphs will be updated as soon as possible.
	 *
	 * This is equivalent to
	 * \code
	 * UpdateGraphs(new GraphUpdateObject(bounds));
	 * \endcode
	 *
	 * \see FlushGraphUpdates
	 * \see batchGraphUpdates
	 * \see \ref graph-updates
	 */
	public void UpdateGraphs (Bounds bounds) {
		UpdateGraphs(new GraphUpdateObject(bounds));
	}

	/** Update all graphs using the GraphUpdateObject.
	 * This can be used to, e.g make all nodes in a region unwalkable, or set them to a higher penalty.
	 * The graphs will be updated as soon as possible (with respect to #batchGraphUpdates)
	 *
	 * \see FlushGraphUpdates
	 * \see batchGraphUpdates
	 * \see \ref graph-updates
	 */
	public void UpdateGraphs (GraphUpdateObject ob) {
		graphUpdates.AddToQueue(ob);

		// If we should limit graph updates, start a coroutine which waits until we should update graphs
		if (batchGraphUpdates && Time.realtimeSinceStartup-lastGraphUpdate < graphUpdateBatchingInterval) {
			if (!graphUpdateRoutineRunning) {
				StartCoroutine(DelayedGraphUpdate());
			}
		} else {
			// Otherwise, graph updates should be carried out as soon as possible
			QueueGraphUpdates();
		}
	}

	/** Forces graph updates to complete in a single frame.
	 * This will force the pathfinding threads to finish calculating the path they are currently calculating (if any) and then pause.
	 * When all threads have paused, graph updates will be performed.
	 * \warning Using this very often (many times per second) can reduce your fps due to a lot of threads waiting for one another.
	 * But you probably wont have to worry about that.
	 *
	 * \note This is almost identical to #FlushWorkItems, but added for more descriptive name.
	 * This function will also override any time limit delays for graph updates.
	 * This is because graph updates are implemented using work items.
	 * So calling this function will also execute any other work items (if any are queued).
	 *
	 * Will not do anything if there are no graph updates queued (not even execute other work items).
	 */
	public void FlushGraphUpdates () {
		if (IsAnyGraphUpdateQueued) {
			QueueGraphUpdates();
			FlushWorkItems();
		}
	}

	#endregion

	/** Forces work items to complete in a single frame.
	 * This will force all work items to run immidiately.
	 * This will force the pathfinding threads to finish calculating the path they are currently calculating (if any) and then pause.
	 * When all threads have paused, work items will be executed (which can be e.g graph updates).
	 *
	 * \warning Using this very often (many times per second) can reduce your fps due to a lot of threads waiting for one another.
	 * But you probably wont have to worry about that
	 *
	 * \note This is almost (note almost) identical to #FlushGraphUpdates, but added for more descriptive name.
	 *
	 * Will not do anything if there are no queued work items waiting to run.
	 *
	  */
	public void FlushWorkItems () {
		var graphLock = PausePathfinding();

		PerformBlockingActions(true);
		graphLock.Release();
	}

	/** Make sure work items are executed.
	 *
	 * \param unblockOnComplete If true, pathfinding will be allowed to start running immediately after completing all work items.
	 * \param block If true, work items that usually take more than one frame to complete will be forced to complete during this call.
	 *              If false, then after this call there might still be work left to do.
	 *
	 * \see AddWorkItem
	 *
	 * \deprecated Use #FlushWorkItems() instead.
	 */
	[System.Obsolete("Use FlushWorkItems() instead")]
	public void FlushWorkItems (bool unblockOnComplete, bool block) {
		var graphLock = PausePathfinding();

		// Run tasks
		PerformBlockingActions(block);
		graphLock.Release();
	}

	/** Forces thread safe callbacks to run.
	 * \deprecated Use #FlushWorkItems instead
	 */
	[System.Obsolete("Use FlushWorkItems instead")]
	public void FlushThreadSafeCallbacks () {
		FlushWorkItems();
	}

	/** Calculates number of threads to use.
	 * If \a count is not Automatic, simply returns \a count casted to an int.
	 * \returns An int specifying how many threads to use, 0 means a coroutine should be used for pathfinding instead of a separate thread.
	 *
	 * If \a count is set to Automatic it will return a value based on the number of processors and memory for the current system.
	 * If memory is <= 512MB or logical cores are <= 1, it will return 0. If memory is <= 1024 it will clamp threads to max 2.
	 * Otherwise it will return the number of logical cores clamped to 6.
	 *
	 * When running on WebGL this method always returns 0
	 */
	public static int CalculateThreadCount (ThreadCount count) {
#if UNITY_WEBGL
		return 0;
#else
		if (count == ThreadCount.AutomaticLowLoad || count == ThreadCount.AutomaticHighLoad) {
			int logicalCores = Mathf.Max(1, SystemInfo.processorCount);
			int memory = SystemInfo.systemMemorySize;

			if (memory <= 0) {
				Debug.LogError("Machine reporting that is has <= 0 bytes of RAM. This is definitely not true, assuming 1 GiB");
				memory = 1024;
			}

			if (logicalCores <= 1) return 0;
			if (memory <= 512) return 0;

			return 1;
		} else {
			return (int)count > 0 ? 1 : 0;
		}
#endif
	}

	/** Sets up all needed variables and scans the graphs.
	 * Calls Initialize, starts the ReturnPaths coroutine and scans all graphs.
	 * Also starts threads if using multithreading
	 * \see #OnAwakeSettings
	 */
	protected override void Awake () {
		base.Awake();
		// Very important to set this. Ensures the singleton pattern holds
		active = this;

		if (FindObjectsOfType(typeof(AstarPath)).Length > 1) {
			Debug.LogError("You should NOT have more than one AstarPath component in the scene at any time.\n" +
				"This can cause serious errors since the AstarPath component builds around a singleton pattern.");
		}

		// Disable GUILayout to gain some performance, it is not used in the OnGUI call
		useGUILayout = false;

		// This class uses the [ExecuteInEditMode] attribute
		// So Awake is called even when not playing
		// Don't do anything when not in play mode
		if (!Application.isPlaying) return;

		if (OnAwakeSettings != null) {
			OnAwakeSettings();
		}

		// To make sure all graph modifiers have been enabled before scan (to avoid script execution order issues)
		GraphModifier.FindAllModifiers();
		RelevantGraphSurface.FindAllGraphSurfaces();

		InitializePathProcessor();
		InitializeProfiler();
		ConfigureReferencesInternal();
		InitializeAstarData();

		// Flush work items, possibly added in InitializeAstarData to load graph data
		FlushWorkItems();

		euclideanEmbedding.dirty = true;

		if (scanOnStartup && (!data.cacheStartup || data.file_cachedStartup == null)) {
			Scan();
		}
	}

	/** Initializes the #pathProcessor field */
	void InitializePathProcessor () {
		int numThreads = CalculateThreadCount(threadCount);

		// Outside of play mode everything is synchronous, so no threads are used.
		if (!Application.isPlaying) numThreads = 0;

		// Trying to prevent simple modding to add support for more than one thread
		if (numThreads > 1) {
			threadCount = ThreadCount.One;
			numThreads = 1;
		}

		int numProcessors = Mathf.Max(numThreads, 1);
		bool multithreaded = numThreads > 0;
		pathProcessor = new PathProcessor(this, pathReturnQueue, numProcessors, multithreaded);

		pathProcessor.OnPathPreSearch += path => {
			var tmp = OnPathPreSearch;
			if (tmp != null) tmp(path);
		};

		pathProcessor.OnPathPostSearch += path => {
			LogPathResults(path);
			var tmp = OnPathPostSearch;
			if (tmp != null) tmp(path);
		};

		// Sent every time the path queue is unblocked
		pathProcessor.OnQueueUnblocked += () => {
			if (euclideanEmbedding.dirty) {
				euclideanEmbedding.RecalculateCosts();
			}
		};

		if (multithreaded) {
			graphUpdates.EnableMultithreading();
		}
	}

	/** Does simple error checking */
	internal void VerifyIntegrity () {
		if (active != this) {
			throw new System.Exception("Singleton pattern broken. Make sure you only have one AstarPath object in the scene");
		}

		if (data == null) {
			throw new System.NullReferenceException("data is null... A* not set up correctly?");
		}

		if (data.graphs == null) {
			data.graphs = new NavGraph[0];
			data.UpdateShortcuts();
		}
	}

	/** \cond internal */
	/** Internal method to make sure #active is set to this object and that #data is not null.
	 * Also calls OnEnable for the #colorSettings and initializes data.userConnections if it wasn't initialized before
	 *
	 * \warning This is mostly for use internally by the system.
	 */
	public void ConfigureReferencesInternal () {
		active = this;
		data = data ?? new AstarData();
		colorSettings = colorSettings ?? new AstarColor();
		colorSettings.OnEnable();
	}
	/** \endcond */

	/** Calls AstarProfiler.InitializeFastProfile */
	void InitializeProfiler () {
		AstarProfiler.InitializeFastProfile(new string[14] {
			"Prepare",          //0
			"Initialize",       //1
			"CalculateStep",    //2
			"Trace",            //3
			"Open",             //4
			"UpdateAllG",       //5
			"Add",              //6
			"Remove",           //7
			"PreProcessing",    //8
			"Callback",         //9
			"Overhead",         //10
			"Log",              //11
			"ReturnPaths",      //12
			"PostPathCallback"  //13
		});
	}

	/** Initializes the AstarData class.
	 * Searches for graph types, calls Awake on #data and on all graphs
	 *
	 * \see AstarData.FindGraphTypes
	 */
	void InitializeAstarData () {
		data.FindGraphTypes();
		data.Awake();
		data.UpdateShortcuts();
	}

	/** Cleans up meshes to avoid memory leaks */
	void OnDisable () {
		gizmos.ClearCache();
	}

	/** Clears up variables and other stuff, destroys graphs.
	 * Note that when destroying an AstarPath object, all static variables such as callbacks will be cleared.
	 */
	void OnDestroy () {
		// This class uses the [ExecuteInEditMode] attribute
		// So OnDestroy is called even when not playing
		// Don't do anything when not in play mode
		if (!Application.isPlaying) return;

		if (logPathResults == PathLog.Heavy)
			Debug.Log("+++ AstarPath Component Destroyed - Cleaning Up Pathfinding Data +++");

		if (active != this) return;

		// Block until the pathfinding threads have
		// completed their current path calculation
		PausePathfinding();

		euclideanEmbedding.dirty = false;
		FlushWorkItems();

		// Don't accept any more path calls to this AstarPath instance.
		// This will cause all eventual multithreading threads to exit
		pathProcessor.queue.TerminateReceivers();

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Processing Possible Work Items");

		// Stop the graph update thread (if it is running)
		graphUpdates.DisableMultithreading();

		// Try to join pathfinding threads
		pathProcessor.JoinThreads();

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Returning Paths");


		// Return all paths
		pathReturnQueue.ReturnPaths(false);

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Destroying Graphs");


		// Clean up graph data
		data.OnDestroy();

		if (logPathResults == PathLog.Heavy)
			Debug.Log("Cleaning up variables");

		// Clear variables up, static variables are good to clean up, otherwise the next scene might get weird data

		// Clear all callbacks
		OnAwakeSettings         = null;
		OnGraphPreScan          = null;
		OnGraphPostScan         = null;
		OnPathPreSearch         = null;
		OnPathPostSearch        = null;
		OnPreScan               = null;
		OnPostScan              = null;
		OnLatePostScan          = null;
		On65KOverflow           = null;
		OnGraphsUpdated         = null;

		active = null;
	}

	#region ScanMethods

	/** Floodfills starting from the specified node */
	public void FloodFill (GraphNode seed) {
		graphUpdates.FloodFill(seed);
	}

	/** Floodfills starting from 'seed' using the specified area */
	public void FloodFill (GraphNode seed, uint area) {
		graphUpdates.FloodFill(seed, area);
	}

	/** Floodfills all graphs and updates areas for every node.
	 * The different colored areas that you see in the scene view when looking at graphs
	 * are called just 'areas', this method calculates which nodes are in what areas.
	 * \see Pathfinding.Node.area
	 */
	[ContextMenu("Flood Fill Graphs")]
	public void FloodFill () {
		Profiler.BeginSample("Recalculate Connected Components");
		graphUpdates.FloodFill();
		workItems.OnFloodFill();
		Profiler.EndSample();
	}

	/** Returns a new global node index.
	 * \warning This method should not be called directly. It is used by the GraphNode constructor.
	 */
	internal int GetNewNodeIndex () {
		return pathProcessor.GetNewNodeIndex();
	}

	/** Initializes temporary path data for a node.
	 * \warning This method should not be called directly. It is used by the GraphNode constructor.
	 */
	internal void InitializeNode (GraphNode node) {
		pathProcessor.InitializeNode(node);
	}

	/** Internal method to destroy a given node.
	 * This is to be called after the node has been disconnected from the graph so that it cannot be reached from any other nodes.
	 * It should only be called during graph updates, that is when the pathfinding threads are either not running or paused.
	 *
	 * \warning This method should not be called by user code. It is used internally by the system.
	 */
	internal void DestroyNode (GraphNode node) {
		pathProcessor.DestroyNode(node);
	}

	/** Blocks until all pathfinding threads are paused and blocked.
	 * \deprecated Use #PausePathfinding instead. Make sure to call Release on the returned lock.
	 */
	[System.Obsolete("Use PausePathfinding instead. Make sure to call Release on the returned lock.", true)]
	public void BlockUntilPathQueueBlocked () {
	}

	/** Blocks until all pathfinding threads are paused and blocked.
	 *
	 * \snippet MiscSnippets.cs AstarPath.PausePathfinding
	 *
	 * \returns A lock object. You need to call \link Pathfinding.PathProcessor.GraphUpdateLock.Release Release\endlink on that object to allow pathfinding to resume.
	 * \note In most cases this should not be called from user code. Use the #AddWorkItem method instead.
	 *
	 * \see #AddWorkItem
	 */
	public PathProcessor.GraphUpdateLock PausePathfinding () {
		return pathProcessor.PausePathfinding(true);
	}

	/** Blocks the path queue so that e.g work items can be performed */
	PathProcessor.GraphUpdateLock PausePathfindingSoon () {
		return pathProcessor.PausePathfinding(false);
	}

	/** Scans a particular graph.
	 * Calling this method will recalculate the specified graph.
	 * This method is pretty slow (depending on graph type and graph complexity of course), so it is advisable to use
	 * smaller graph updates whenever possible.
	 *
	 * \snippet MiscSnippets.cs AstarPath.Scan1
	 *
	 * \see \ref graph-updates
	 * \see ScanAsync
	 */
	public void Scan (NavGraph graphToScan) {
		if (graphToScan == null) throw new System.ArgumentNullException();
		Scan(new NavGraph[] { graphToScan });
	}

	/** Scans all specified graphs.
	 * \param graphsToScan The graphs to scan. If this parameter is null then all graphs will be scanned
	 *
	 * Calling this method will recalculate all specified graphs or all graphs if the \a graphsToScan parameter is null.
	 * This method is pretty slow (depending on graph type and graph complexity of course), so it is advisable to use
	 * smaller graph updates whenever possible.
	 *
	 * \snippet MiscSnippets.cs AstarPath.Scan1
	 *
	 * \see \ref graph-updates
	 * \see ScanAsync
	 */
	public void Scan (NavGraph[] graphsToScan = null) {
		var prevProgress = new Progress();

		Profiler.BeginSample("Scan");
		Profiler.BeginSample("Init");
		foreach (var p in ScanAsync(graphsToScan)) {
			if (prevProgress.description != p.description) {
#if !NETFX_CORE && UNITY_EDITOR
				Profiler.EndSample();
				Profiler.BeginSample(p.description);
				// Log progress to the console
				System.Console.WriteLine(p.description);
				prevProgress = p;
#endif
			}
		}
		Profiler.EndSample();
		Profiler.EndSample();
	}

	/** Scans all graphs.
	 * \deprecated Use #Scan or #ScanAsync instead
	 *
	 * \see Scan
	 */
	[System.Obsolete("ScanLoop is now named ScanAsync and is an IEnumerable<Progress>. Use foreach to iterate over the progress insead")]
	public void ScanLoop (OnScanStatus statusCallback) {
		foreach (var p in ScanAsync()) {
			statusCallback(p);
		}
	}

	/** Scans a particular graph asynchronously. This is a IEnumerable, you can loop through it to get the progress
	 * \snippet MiscSnippets.cs AstarPath.ScanAsync1
	 * You can scan graphs asyncronously by yielding when you loop through the progress.
	 * Note that this does not guarantee a good framerate, but it will allow you
	 * to at least show a progress bar during scanning.
	 * \snippet MiscSnippets.cs AstarPath.ScanAsync2
	 *
	 * \see Scan
	 *
	 * \astarpro
	 */
	public IEnumerable<Progress> ScanAsync (NavGraph graphToScan) {
		if (graphToScan == null) throw new System.ArgumentNullException();
		return ScanAsync(new NavGraph[] { graphToScan });
	}

	/** Scans all specified graphs asynchronously. This is a IEnumerable, you can loop through it to get the progress
	 * \param graphsToScan The graphs to scan. If this parameter is null then all graphs will be scanned
	 *
	 * \snippet MiscSnippets.cs AstarPath.ScanAsync1
	 * You can scan graphs asyncronously by yielding when you loop through the progress.
	 * Note that this does not guarantee a good framerate, but it will allow you
	 * to at least show a progress bar during scanning.
	 * \snippet MiscSnippets.cs AstarPath.ScanAsync2
	 *
	 * \see Scan
	 *
	 * \astarpro
	 */
	public IEnumerable<Progress> ScanAsync (NavGraph[] graphsToScan = null) {
		if (graphsToScan == null) graphsToScan = graphs;

		if (graphsToScan == null) {
			yield break;
		}

		if (isScanning) throw new System.InvalidOperationException("Another async scan is already running");

		isScanning = true;

		VerifyIntegrity();

		var graphUpdateLock = PausePathfinding();

		// Make sure all paths that are in the queue to be returned
		// are returned immediately
		// Some modifiers (e.g the funnel modifier) rely on
		// the nodes being valid when the path is returned
		pathReturnQueue.ReturnPaths(false);

		if (!Application.isPlaying) {
			data.FindGraphTypes();
			GraphModifier.FindAllModifiers();
		}

		int startFrame = Time.frameCount;

		yield return new Progress(0.05F, "Pre processing graphs");

		// Yes, this constraint is trivial to circumvent
		// the code is the same because it is annoying
		// to have to have separate code for the free
		// and the pro version that does essentially the same thing.
		// I would appreciate if you purchased the pro version of the A* Pathfinding Project
		// if you need async scanning.
		if (Time.frameCount != startFrame) {
			throw new System.Exception("Async scanning can only be done in the pro version of the A* Pathfinding Project");
		}

		if (OnPreScan != null) {
			OnPreScan(this);
		}

		GraphModifier.TriggerEvent(GraphModifier.EventType.PreScan);

		data.LockGraphStructure();

		var watch = System.Diagnostics.Stopwatch.StartNew();

		// Destroy previous nodes
		for (int i = 0; i < graphsToScan.Length; i++) {
			if (graphsToScan[i] != null) {
				((IGraphInternals)graphsToScan[i]).DestroyAllNodes();
			}
		}

		// Loop through all graphs and scan them one by one
		for (int i = 0; i < graphsToScan.Length; i++) {
			// Skip null graphs
			if (graphsToScan[i] == null) continue;

			// Just used for progress information
			// This graph will advance the progress bar from minp to maxp
			float minp = Mathf.Lerp(0.1F, 0.8F, (float)(i)/(graphsToScan.Length));
			float maxp = Mathf.Lerp(0.1F, 0.8F, (float)(i+0.95F)/(graphsToScan.Length));

			var progressDescriptionPrefix = "Scanning graph " + (i+1) + " of " + graphsToScan.Length + " - ";

			// Like a foreach loop but it gets a little complicated because of the exception
			// handling (it is not possible to yield inside try-except clause).
			var coroutine = ScanGraph(graphsToScan[i]).GetEnumerator();
			while (true) {
				try {
					if (!coroutine.MoveNext()) break;
				} catch {
					isScanning = false;
					data.UnlockGraphStructure();
					graphUpdateLock.Release();
					throw;
				}
				yield return coroutine.Current.MapTo(minp, maxp, progressDescriptionPrefix);
			}
		}

		data.UnlockGraphStructure();
		yield return new Progress(0.8F, "Post processing graphs");

		if (OnPostScan != null) {
			OnPostScan(this);
		}
		GraphModifier.TriggerEvent(GraphModifier.EventType.PostScan);

		FlushWorkItems();

		yield return new Progress(0.9F, "Computing areas");

		FloodFill();

		yield return new Progress(0.95F, "Late post processing");

		// Signal that we have stopped scanning here
		// Note that no yields can happen after this point
		// since then other parts of the system can start to interfere
		isScanning = false;

		if (OnLatePostScan != null) {
			OnLatePostScan(this);
		}
		GraphModifier.TriggerEvent(GraphModifier.EventType.LatePostScan);

		euclideanEmbedding.dirty = true;
		euclideanEmbedding.RecalculatePivots();

		// Perform any blocking actions
		FlushWorkItems();
		// Resume pathfinding threads
		graphUpdateLock.Release();

		watch.Stop();
		lastScanTime = (float)watch.Elapsed.TotalSeconds;

		System.GC.Collect();

		if (logPathResults != PathLog.None && logPathResults != PathLog.OnlyErrors) {
			Debug.Log("Scanning - Process took "+(lastScanTime*1000).ToString("0")+" ms to complete");
		}
	}

	IEnumerable<Progress> ScanGraph (NavGraph graph) {
		if (OnGraphPreScan != null) {
			yield return new Progress(0, "Pre processing");
			OnGraphPreScan(graph);
		}

		yield return new Progress(0, "");

		foreach (var p in ((IGraphInternals)graph).ScanInternal()) {
			yield return p.MapTo(0, 0.95f);
		}

		yield return new Progress(0.95f, "Assigning graph indices");

		// Assign the graph index to every node in the graph
		graph.GetNodes(node => node.GraphIndex = (uint)graph.graphIndex);

		if (OnGraphPostScan != null) {
			yield return new Progress(0.99f, "Post processing");
			OnGraphPostScan(graph);
		}
	}

	#endregion

	private static int waitForPathDepth = 0;

	/** Wait for the specified path to be calculated.
	 * Normally it takes a few frames for a path to get calculated and returned.
	 *
	 * \deprecated This method has been renamed to #BlockUntilCalculated.
	 */
	[System.Obsolete("This method has been renamed to BlockUntilCalculated")]
	public static void WaitForPath (Path path) {
		BlockUntilCalculated(path);
	}

	/** Blocks until the path has been calculated.
	 * \param path The path to wait for. The path must be started, otherwise an exception will be thrown.
	 *
	 * Normally it takes a few frames for a path to be calculated and returned.
	 * This function will ensure that the path will be calculated when this function returns
	 * and that the callback for that path has been called.
	 *
	 * If requesting a lot of paths in one go and waiting for the last one to complete,
	 * it will calculate most of the paths in the queue (only most if using multithreading, all if not using multithreading).
	 *
	 * Use this function only if you really need to.
	 * There is a point to spreading path calculations out over several frames.
	 * It smoothes out the framerate and makes sure requesting a large
	 * number of paths at the same time does not cause lag.
	 *
	 * \note Graph updates and other callbacks might get called during the execution of this function.
	 *
	 * When the pathfinder is shutting down. I.e in OnDestroy, this function will not do anything.
	 *
	 * \throws Exception if pathfinding is not initialized properly for this scene (most likely no AstarPath object exists)
	 * or if the path has not been started yet.
	 * Also throws an exception if critical errors occur such as when the pathfinding threads have crashed (which should not happen in normal cases).
	 * This prevents an infinite loop while waiting for the path.
	 *
	 * \see Pathfinding.Path.WaitForPath
	 * \see Pathfinding.Path.BlockUntilCalculated
	 */
	public static void BlockUntilCalculated (Path path) {
		if (active == null)
			throw new System.Exception("Pathfinding is not correctly initialized in this scene (yet?). " +
				"AstarPath.active is null.\nDo not call this function in Awake");

		if (path == null) throw new System.ArgumentNullException("Path must not be null");

		if (active.pathProcessor.queue.IsTerminating) return;

		if (path.PipelineState == PathState.Created) {
			throw new System.Exception("The specified path has not been started yet.");
		}

		waitForPathDepth++;

		if (waitForPathDepth == 5) {
			Debug.LogError("You are calling the BlockUntilCalculated function recursively (maybe from a path callback). Please don't do this.");
		}

		if (path.PipelineState < PathState.ReturnQueue) {
			if (active.IsUsingMultithreading) {
				while (path.PipelineState < PathState.ReturnQueue) {
					if (active.pathProcessor.queue.IsTerminating) {
						waitForPathDepth--;
						throw new System.Exception("Pathfinding Threads seem to have crashed.");
					}

					// Wait for threads to calculate paths
					Thread.Sleep(1);
					active.PerformBlockingActions(true);
				}
			} else {
				while (path.PipelineState < PathState.ReturnQueue) {
					if (active.pathProcessor.queue.IsEmpty && path.PipelineState != PathState.Processing) {
						waitForPathDepth--;
						throw new System.Exception("Critical error. Path Queue is empty but the path state is '" + path.PipelineState + "'");
					}

					// Calculate some paths
					active.pathProcessor.TickNonMultithreaded();
					active.PerformBlockingActions(true);
				}
			}
		}

		active.pathReturnQueue.ReturnPaths(false);
		waitForPathDepth--;
	}

	/** Will send a callback when it is safe to update nodes. This is defined as between the path searches.
	 * This callback will only be sent once and is nulled directly after the callback has been sent.
	 * When using more threads than one, calling this often might decrease pathfinding performance due to a lot of idling in the threads.
	 * Not performance as in it will use much CPU power,
	 * but performance as in the number of paths per second will probably go down (though your framerate might actually increase a tiny bit)
	 *
	 * You should only call this function from the main unity thread (i.e normal game code).
	 *
	 * \note The threadSafe parameter has been deprecated
	 * \deprecated
	 */
	[System.Obsolete("The threadSafe parameter has been deprecated")]
	public static void RegisterSafeUpdate (System.Action callback, bool threadSafe) {
		RegisterSafeUpdate(callback);
	}

	/** Will send a callback when it is safe to update nodes. This is defined as between the path searches.
	 * This callback will only be sent once and is nulled directly after the callback has been sent.
	 * When using more threads than one, calling this often might decrease pathfinding performance due to a lot of idling in the threads.
	 * Not performance as in it will use much CPU power,
	 * but performance as in the number of paths per second will probably go down (though your framerate might actually increase a tiny bit)
	 *
	 * You should only call this function from the main unity thread (i.e normal game code).
	 *
	 * \version Since version 4.0 this is equivalent to AddWorkItem(new AstarWorkItem(callback)). Previously the
	 * callbacks added using this method would not be ordered with respect to other work items, so they could be
	 * executed before other work items or after them.
	 *
	 * \deprecated Use #AddWorkItem(System.Action) instead. Note the slight change in behavior (mentioned above).
	 */
	[System.Obsolete("Use AddWorkItem(System.Action) instead. Note the slight change in behavior (mentioned in the documentation).")]
	public static void RegisterSafeUpdate (System.Action callback) {
		active.AddWorkItem(new AstarWorkItem(callback));
	}

	/** Adds the path to a queue so that it will be calculated as soon as possible.
	 * The callback specified when constructing the path will be called when the path has been calculated.
	 * Usually you should use the Seeker component instead of calling this function directly.
	 *
	 * \param path The path that should be enqueued.
	 * \param pushToFront If true, the path will be pushed to the front of the queue, bypassing all waiting paths and making it the next path to be calculated.
	 * This can be useful if you have a path which you want to prioritize over all others. Be careful to not overuse it though.
	 * If too many paths are put in the front of the queue often, this can lead to normal paths having to wait a very long time before being calculated.
	 */
	public static void StartPath (Path path, bool pushToFront = false) {
		// Copy to local variable to avoid multithreading issues
		var astar = active;

		if (System.Object.ReferenceEquals(astar, null)) {
			Debug.LogError("There is no AstarPath object in the scene or it has not been initialized yet");
			return;
		}

		if (path.PipelineState != PathState.Created) {
			throw new System.Exception("The path has an invalid state. Expected " + PathState.Created + " found " + path.PipelineState + "\n" +
				"Make sure you are not requesting the same path twice");
		}

		if (astar.pathProcessor.queue.IsTerminating) {
			path.FailWithError("No new paths are accepted");
			return;
		}

		if (astar.graphs == null || astar.graphs.Length == 0) {
			Debug.LogError("There are no graphs in the scene");
			path.FailWithError("There are no graphs in the scene");
			Debug.LogError(path.errorLog);
			return;
		}

		path.Claim(astar);

		// Will increment p.state to PathState.PathQueue
		((IPathInternals)path).AdvanceState(PathState.PathQueue);
		if (pushToFront) {
			astar.pathProcessor.queue.PushFront(path);
		} else {
			astar.pathProcessor.queue.Push(path);
		}

		// Outside of play mode, all path requests are synchronous
		if (!Application.isPlaying) {
			BlockUntilCalculated(path);
		}
	}

	/** Terminates pathfinding threads when the application quits */
	void OnApplicationQuit () {
		OnDestroy();

		// Abort threads if they are still running (likely because of some bug in that case)
		// to make sure that the application can shut down properly
		pathProcessor.AbortThreads();
	}

	/**
	 * Returns the nearest node to a position using the specified NNConstraint.
	 * Searches through all graphs for their nearest nodes to the specified position and picks the closest one.\n
	 * Using the NNConstraint.None constraint.
	 *
	 * \snippet MiscSnippets.cs AstarPath.GetNearest1
	 *
	 * \see Pathfinding.NNConstraint
	 */
	public NNInfo GetNearest (Vector3 position) {
		return GetNearest(position, NNConstraint.None);
	}

	/**
	 * Returns the nearest node to a position using the specified NNConstraint.
	 * Searches through all graphs for their nearest nodes to the specified position and picks the closest one.
	 * The NNConstraint can be used to specify constraints on which nodes can be chosen such as only picking walkable nodes.
	 *
	 * \snippet MiscSnippets.cs AstarPath.GetNearest2
	 *
	 * \snippet MiscSnippets.cs AstarPath.GetNearest3
	 *
	 * \see Pathfinding.NNConstraint
	 */
	public NNInfo GetNearest (Vector3 position, NNConstraint constraint) {
		return GetNearest(position, constraint, null);
	}

	/**
	 * Returns the nearest node to a position using the specified NNConstraint.
	 * Searches through all graphs for their nearest nodes to the specified position and picks the closest one.
	 * The NNConstraint can be used to specify constraints on which nodes can be chosen such as only picking walkable nodes.
	 * \see Pathfinding.NNConstraint
	 */
	public NNInfo GetNearest (Vector3 position, NNConstraint constraint, GraphNode hint) {
		// Cache property lookup
		var graphs = this.graphs;

		float minDist = float.PositiveInfinity;
		NNInfoInternal nearestNode = new NNInfoInternal();
		int nearestGraph = -1;

		if (graphs != null) {
			for (int i = 0; i < graphs.Length; i++) {
				NavGraph graph = graphs[i];

				// Check if this graph should be searched
				if (graph == null || !constraint.SuitableGraph(i, graph)) {
					continue;
				}

				NNInfoInternal nnInfo;
				if (fullGetNearestSearch) {
					// Slower nearest node search
					// this will try to find a node which is suitable according to the constraint
					nnInfo = graph.GetNearestForce(position, constraint);
				} else {
					// Fast nearest node search
					// just find a node close to the position without using the constraint that much
					// (unless that comes essentially 'for free')
					nnInfo = graph.GetNearest(position, constraint);
				}

				GraphNode node = nnInfo.node;

				// No node found in this graph
				if (node == null) {
					continue;
				}

				// Distance to the closest point on the node from the requested position
				float dist = ((Vector3)nnInfo.clampedPosition-position).magnitude;

				if (prioritizeGraphs && dist < prioritizeGraphsLimit) {
					// The node is close enough, choose this graph and discard all others
					minDist = dist;
					nearestNode = nnInfo;
					nearestGraph = i;
					break;
				} else {
					// Choose the best node found so far
					if (dist < minDist) {
						minDist = dist;
						nearestNode = nnInfo;
						nearestGraph = i;
					}
				}
			}
		}

		// No matches found
		if (nearestGraph == -1) {
			return new NNInfo();
		}

		// Check if a constrained node has already been set
		if (nearestNode.constrainedNode != null) {
			nearestNode.node = nearestNode.constrainedNode;
			nearestNode.clampedPosition = nearestNode.constClampedPosition;
		}

		if (!fullGetNearestSearch && nearestNode.node != null && !constraint.Suitable(nearestNode.node)) {
			// Otherwise, perform a check to force the graphs to check for a suitable node
			NNInfoInternal nnInfo = graphs[nearestGraph].GetNearestForce(position, constraint);

			if (nnInfo.node != null) {
				nearestNode = nnInfo;
			}
		}

		if (!constraint.Suitable(nearestNode.node) || (constraint.constrainDistance && (nearestNode.clampedPosition - position).sqrMagnitude > maxNearestNodeDistanceSqr)) {
			return new NNInfo();
		}

		// Convert to NNInfo which doesn't have all the internal fields
		return new NNInfo(nearestNode);
	}

	/** Returns the node closest to the ray (slow).
	 * \warning This function is brute-force and very slow, use with caution
	 */
	public GraphNode GetNearest (Ray ray) {
		if (graphs == null) return null;

		float minDist = Mathf.Infinity;
		GraphNode nearestNode = null;

		Vector3 lineDirection = ray.direction;
		Vector3 lineOrigin = ray.origin;

		for (int i = 0; i < graphs.Length; i++) {
			NavGraph graph = graphs[i];

			graph.GetNodes(node => {
				Vector3 pos = (Vector3)node.position;
				Vector3 p = lineOrigin+(Vector3.Dot(pos-lineOrigin, lineDirection)*lineDirection);

				float tmp = Mathf.Abs(p.x-pos.x);
				tmp *= tmp;
				if (tmp > minDist) return;

				tmp = Mathf.Abs(p.z-pos.z);
				tmp *= tmp;
				if (tmp > minDist) return;

				float dist = (p-pos).sqrMagnitude;

				if (dist < minDist) {
					minDist = dist;
					nearestNode = node;
				}
				return;
			});
		}

		return nearestNode;
	}
}
