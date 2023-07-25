using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QPathFinder
{
	public enum SearchMode 
	{
		Simple = 0,
		Intermediate, 
		Complex
	}
	
	public static class PathFinderExtensions
	{
		/// Finds shortest path between Nodes.
		/// Once the path is found, it will return the path as List of Positions (not Nodes, but vector3. If you need Nodes, use FindShortestPathOfNodes). 
		/// <returns> Returns list of **Positions**</returns>
		/// <param name="startNodeID">Find the path from this node</param>
		/// <param name="endNodeID">Find the path to this node</param>
		/// <param name="pathType">Path type. It can be a straight line or curved path</param>
		/// <param name="executionType">Synchronous is immediate and locks the control till path is found and returns the path. 
		/// Asynchronous type runs in coroutines without locking the control. If you have more than 50 Nodes, Asynchronous is recommended</param>
		/// <param name="OnPathFound">Callback once the path is found</param>

		public static void FindShortestPathOfPoints (  this PathFinder manager, int startNodeID, int endNodeID, PathLineType pathType, Execution executionType, System.Action<List<Vector3>> OnPathFound )
		{
			PathFollowerUtility.FindShortestPathOfPoints_Internal( manager, startNodeID, endNodeID, pathType, executionType, OnPathFound );
		}

		
		/// Finds shortest path between Nodes.
		/// Once the path is found, it will return the path as List of Positions ( not Nodes, but vector3. If you need Nodes, use FindShortestPathOfNodes). 
		/// <returns> Returns list of **Positions**</returns>
		/// <param name="startNodeID">Find the path from this node</param>
		/// <param name="endNodeID">Find the path to this node</param>
		/// <param name="pathType">Path type. It can be a straight line or curved path</param>
		/// <param name="executionType">Synchronous is immediate and locks the control till path is found and returns the path. 
		/// Asynchronous type runs in coroutines with out locking the control. If you have more than 50 Nodes, Asynchronous is recommended</param>
		/// <param name="searchMode"> This is still WIP. For now, Intermediate and Complex does a tad bit more calculations to make the path even shorter</param>
		/// <param name="OnPathFound">Callback once the path is found</param>


		public static void FindShortestPathOfPoints ( this PathFinder manager, Vector3 startPoint, Vector3 endPoint, PathLineType pathType, Execution executionType, SearchMode searchMode,  System.Action<List<Vector3>> OnPathFound )
		{
			PathFollowerUtility.FindShortestPathOfPoints_Internal ( manager, startPoint, endPoint, pathType, executionType, searchMode, OnPathFound);
		}
	}

    public static class PathFollowerUtility 
    {
		/// <Summary>
		///	This will move the game object through the points specified.
		/// </Summary>
		/// <param name="transform">The object you want to move along the path</param>
		/// <param name="points">List of positions along which the object is moved.</param>
		/// <param name="moveSpeed">Movement speed</param>
		/// <param name="autoRotateToDestination">'true' if you want transform to rotate in the direction of the movement</param>

		public static PathFollower FollowPath( Transform transform, List<Vector3> points, float moveSpeed, bool autoRotateToDestination )
		{
			if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo("Initiated Follow path for transform " + transform.name, true );
			var pathFollower = CreateOrGetPathFollowerToPosition(transform);
			if (points != null) 
				pathFollower.Follow(points, moveSpeed, autoRotateToDestination);
			else
			{ 
				if ( QPathFinder.Logger.CanLogError ) QPathFinder.Logger.LogError("Could not find the path for path follower to follow!", true );
			}
			return pathFollower;
		}

		/// <Summary>
		///	This will move the game object through the NODES specified.
		/// </Summary>
		/// <param name="transform">The object you want to move along the path</param>
		/// <param name="nodes">List of Nodes along which the object is moved.</param>
		/// <param name="moveSpeed">Movement speed</param>
		/// <param name="autoRotateToDestination">'true' if you want transform to rotate in the direction of the movement</param>

		public static PathFollower FollowPath( Transform transform, List<Node> nodes, float moveSpeed, bool autoRotateToDestination )
		{
			if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo("Initiated Follow path for transform " + transform.name, true );
			var pathFollower = CreateOrGetPathFollowerWithNodes(transform);
			if (nodes != null) 
				pathFollower.Follow(nodes, moveSpeed, autoRotateToDestination);
			else
			{ 
				if ( QPathFinder.Logger.CanLogError ) QPathFinder.Logger.LogError("Could not find the path for path follower to follow!", true );
			}
			return pathFollower;
		}

		/// <Summary>
		/// This will move the game object through the points specified, Also, it will keep the gameobject snapped to the ground.
		/// So if your Nodes are a little above the ground, your target will still move on the ground.
		/// We are doing this by raycasting from above the player to the ground. At the ray cast hit position, we are snapping the player.
		/// </Summary>
		/// <param name="transform">The object you want to move along the path</param>
		/// <param name="points">List of positions along which the object is moved.</param>
		/// <param name="moveSpeed">Movement speed</param>
		/// <param name="autoRotateToDestination">'true' if you want transform to rotate in the direction of the movement</param>
		/// <param name="directionOfRayCast"> We use raycasting to find the ground position. If your ground is down, the ray has to go down, so use Vector3.down. </param>
		/// <param name="offsetDistanceToFloatFromGround"> If you want your character to float a little above the ground, give the offset value here </param>
		/// <param name="groundGameObjectLayer">This is the ground Gameobject's layer. When we use raycast we target to hit this layer</param>
		/// <param name="offsetDistanceFromPoint">this is to calculate the raycast origin, from where we shoot rays. raycast origin is generally above the player, casting rays towards ground. For most cases, you can leave this as default.</param>
		/// <param name="maxDistanceForRayCast">this is the distance of ray from the raycast origin. For most cases you can let this be default value. </param>

		public static PathFollower FollowPathWithGroundSnap( Transform transform, List<Vector3> points
																, float moveSpeed
																, bool autoRotateToDestination
																, Vector3 directionOfRayCast			
																, float offsetDistanceToFloatFromGround 
																, int groundGameObjectLayer				
																, float offsetDistanceFromPoint = 10	
																, int maxDistanceForRayCast = 40		
																 )
		{
			if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo("Initiated Follow path[With ground snap] for transform " + transform.name , true );
			var pathFollower = CreateWithSnapToGround(transform, directionOfRayCast, offsetDistanceFromPoint, offsetDistanceToFloatFromGround, maxDistanceForRayCast, groundGameObjectLayer );
			if (points != null) 
				pathFollower.Follow(points, moveSpeed, autoRotateToDestination);
			else 
			{
				if ( QPathFinder.Logger.CanLogError ) QPathFinder.Logger.LogError("Could not find the path for path follower to follow!", true );
			}
			return pathFollower;
		}

		/// <summary>
		/// Stops the gameobject while moving along the path. 
		/// </summary>
		/// <param name="transform">The gameobject which needs to stop moving</param>
		public static void StopFollowing( Transform transform )
		{
			if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo("Stopping FollowPath");
			Stop(transform);
		}

		// 
		// *** PRIVATE AND INTERNAL ***

		#region PRIVATE AND INTERNAL

		internal static void FindShortestPathOfPoints_Internal (  PathFinder manager, int startNodeID, int endNodeID, PathLineType pathType, Execution execution, System.Action<List<Vector3>> OnPathFound )
		{
			int nearestPointFromStart = startNodeID;
			int nearestPointFromEnd = endNodeID;


			if ( nearestPointFromEnd == -1 || nearestPointFromStart == -1 )
			{
				if ( QPathFinder.Logger.CanLogError ) QPathFinder.Logger.LogError("Could not find path between " + nearestPointFromStart + " and " + nearestPointFromEnd, true );
				OnPathFound( null );
				return;
			}

			float startTime = Time.realtimeSinceStartup;

			System.Action<List<Node>> onPathOfNodesFound = delegate ( List<Node> nodes ) 
			{ 
				if ( nodes == null || nodes.Count == 0 )
					OnPathFound ( null );

				List<System.Object> allNodes = new List<System.Object>();
				List<Vector3> path = null;

				if ( nodes != null )
				{
					foreach ( var a in nodes ) 
					{
						allNodes.Add ( a.Position );
					}
				}
				path = (pathType == PathLineType.Straight ? GetStraightPathPoints(allNodes) : GetCatmullRomCurvePathPoints ( allNodes ) );

				if ( QPathFinder.Logger.CanLogInfo )
					for ( int i = 1; i < path.Count; i++ )
					{
						Debug.DrawLine(path[i - 1], path[i], Color.red, QPathFinder.Logger.DrawLineDuration );
					}

				OnPathFound ( path );
			};
            
			manager.FindShortestPathOfNodes( nearestPointFromStart, nearestPointFromEnd, execution, onPathOfNodesFound );
		}

		internal static void FindShortestPathOfPoints_Internal (  PathFinder manager, Vector3 startPoint, Vector3 endPoint, PathLineType pathType, Execution execution, SearchMode searchMode,  System.Action<List<Vector3>> OnPathFound )
		{
			bool makeItMoreAccurate = searchMode == SearchMode.Intermediate || searchMode == SearchMode.Complex;
			int nearestPointFromStart = manager.FindNearestNode ( startPoint );
			int nearestPointFromEnd = -1;
			if ( nearestPointFromStart != -1 )
				nearestPointFromEnd = manager.FindNearestNode ( endPoint );
			
			if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo("Nearest point from start" + startPoint + " is " + nearestPointFromStart, true );
			if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo("Nearest point from end:" + endPoint + " is " + nearestPointFromEnd, true );

			if ( nearestPointFromEnd == -1 || nearestPointFromStart == -1 )
			{
				if ( QPathFinder.Logger.CanLogError ) QPathFinder.Logger.LogError("Could not find path between " + nearestPointFromStart + " and " + nearestPointFromEnd, true );
				OnPathFound( null );
				return;
			}

			float startTime = Time.realtimeSinceStartup;


			System.Action<List<Node>> onPathOfNodesFound = delegate ( List<Node> nodes ) 
			{ 
				if ( nodes == null || nodes.Count == 0 )
				{
					OnPathFound ( null );
					return;
				}

				List<System.Object> allNodes	= new List<System.Object>();
				if ( nodes != null )
				{
					foreach ( var a in nodes ) 
					{
						allNodes.Add ( a );
					}
				}

				if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo("Search Mode " + searchMode.ToString() + " opted", true );

				if ( makeItMoreAccurate )
				{
					if ( allNodes.Count > 1 ) 
					{
						Vector3 shortestPointOnPath;
						int nearestNode = -1;
						Path currentPath = null;
						int shortestPathID = -1;


						{
							nearestNode = ((Node) allNodes[0]).autoGeneratedID;
							shortestPathID = -1;
							currentPath = manager.graphData.GetPathBetween ( GetNodeFromNodeOrVector ( allNodes, 0 ), GetNodeFromNodeOrVector ( allNodes, 1 ) );

							if ( currentPath != null ) 
							{
								shortestPointOnPath = GetClosestPointOnAnyPath( nearestNode, manager, startPoint, out shortestPathID );

								if ( shortestPathID == currentPath.autoGeneratedID ) 
								{
									allNodes[0] = shortestPointOnPath;
								}
								else 
								{
									allNodes.Insert(0, shortestPointOnPath );
								}
							}
							else 
							{
								if ( QPathFinder.Logger.CanLogError ) QPathFinder.Logger.LogInfo("Error occured while finding path");
							}
						}

						{
							shortestPathID = -1;
							nearestNode = ((Node) allNodes[allNodes.Count - 1]).autoGeneratedID;
							currentPath = manager.graphData.GetPathBetween ( GetNodeFromNodeOrVector ( allNodes, allNodes.Count - 2 ), GetNodeFromNodeOrVector ( allNodes, allNodes.Count - 1 ) );

							if ( currentPath != null ) 
							{
								shortestPointOnPath = GetClosestPointOnAnyPath( nearestNode, manager, endPoint, out shortestPathID );

								if ( shortestPathID == currentPath.autoGeneratedID ) 
								{
									allNodes[allNodes.Count - 1] = shortestPointOnPath;
								}
								else 
								{
									allNodes.Add( shortestPointOnPath );
								}
							}
							else 
							{
								if ( QPathFinder.Logger.CanLogError ) QPathFinder.Logger.LogInfo("Error occured while finding path");
							}
						}

					}
					else
					{
						if ( QPathFinder.Logger.CanLogWarning ) QPathFinder.Logger.LogWarning("Unable to get the best result due to less node count", true ); 
					}
				}

				List<Vector3> path = null;
				
				{
					allNodes.Insert ( 0, startPoint );
					allNodes.Add ( endPoint );

					path = (pathType == PathLineType.Straight ? GetStraightPathPoints(allNodes) : GetCatmullRomCurvePathPoints ( allNodes ) );
				}
				
				if ( QPathFinder.Logger.CanLogInfo )
				{
					for ( int i = 1; i < path.Count; i++ )
					{
						Debug.DrawLine(path[i - 1], path[i], Color.red, QPathFinder.Logger.DrawLineDuration );
					}
				}

				OnPathFound ( path );
			};
            
			manager.FindShortestPathOfNodes ( nearestPointFromStart, nearestPointFromEnd, execution, onPathOfNodesFound );
		}


		internal static List<Vector3> GetStraightPathPoints( List<System.Object> nodePoints )
        {
            if ( nodePoints == null )
                return null;

			if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo ("Straight line path choosen!");

            List<Vector3> path = new List<Vector3>();
            
            if (nodePoints.Count < 2)
            {
                return null;
            }

            for (int i = 0; i < nodePoints.Count; i++)
            {
					path.Add( GetPositionFromNodeOrVector( nodePoints, i) );	
            }
            return path;
        }

        internal static List<Vector3> GetCatmullRomCurvePathPoints( List<System.Object> nodePoints )
        {
            if ( nodePoints == null )
                return null;

			if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo ("CatmullRomCurve line path choosen!");

            List<Vector3> path = new List<Vector3>();
            
            if (nodePoints.Count < 3)
            {
                for( int i = 0; i < nodePoints.Count; i++ )
                {
                    path.Add ( GetPositionFromNodeOrVector( nodePoints, i ) );
                }
                return path;
            }

            Vector3[] catmullRomPoints = new Vector3[nodePoints.Count + 2];
            for( int i = 0; i < nodePoints.Count; i++ )
            {
                catmullRomPoints[i+1] = GetPositionFromNodeOrVector( nodePoints, i );
            }
            int endIndex = catmullRomPoints.Length - 1;

            catmullRomPoints[0] = catmullRomPoints[1] + (catmullRomPoints[1] - catmullRomPoints[2]) + (catmullRomPoints[3] - catmullRomPoints[2]);
            catmullRomPoints[endIndex] = catmullRomPoints[endIndex - 1] + (catmullRomPoints[endIndex - 1] - catmullRomPoints[endIndex - 2])
                                    + (catmullRomPoints[endIndex - 3] - catmullRomPoints[endIndex - 2]);

            path.Add( GetPositionFromNodeOrVector( nodePoints, 0 ) );

            for (int i = 0; i < catmullRomPoints.Length - 3; i++)
            {
                for (float t = 0.05f; t <= 1.0f; t += 0.05f)
                {
                    Vector3 pt = ComputeCatmullRom(catmullRomPoints[i], catmullRomPoints[i + 1], catmullRomPoints[i + 2], catmullRomPoints[i + 3], t);
                    path.Add(pt);
                }
            }

            path.Add( GetPositionFromNodeOrVector( nodePoints, nodePoints.Count - 1 ) );
            return path;
        }

		private static PathFollower CreateOrGetPathFollowerToPosition(Transform transform)
		{
			var pathFollower = transform.GetComponent<PathFollowerToPosition>();
			if (pathFollower == null) 
			{
				if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo ("PathFollower Script created and attached");
				pathFollower = transform.gameObject.AddComponent<PathFollowerToPosition>();
			}
			else 
			{
				if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo ("Using existing PathFollower Script to follow the path!");
			}
			pathFollower._transform = transform;
			return pathFollower;
		}
		private static PathFollower CreateOrGetPathFollowerWithNodes(Transform transform)
		{
			var pathFollower = transform.GetComponent<PathFollowerWithNodes>();
			if (pathFollower == null) 
			{
				if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo ("PathFollower Script created and attached");
				pathFollower = transform.gameObject.AddComponent<PathFollowerWithNodes>();
			}
			else 
			{
				if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo ("Using existing PathFollower Script to follow the path!");
			}
			pathFollower._transform = transform;
			return pathFollower;
		}
		private static PathFollower CreateWithSnapToGround(Transform transform, Vector3 directionOfRayCast, float offsetDistanceFromPoint, float offsetDistanceToFloatFromGround, int maxDistanceForRayCast, int groundLayer)
		{
			var pathFollower = transform.GetComponent<PathFollowerToPositionAndSnapToGround>();
			if (pathFollower == null)
			{
				if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo ("PathFollowerSnapToGround Script created and attached");
				pathFollower = transform.gameObject.AddComponent<PathFollowerToPositionAndSnapToGround>();
			}
			else 
			{
				if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo ("Using existing PathFollowerSnapToGround Script to follow the path!");
			}

			pathFollower.SetContext( directionOfRayCast, offsetDistanceFromPoint, offsetDistanceToFloatFromGround, maxDistanceForRayCast, groundLayer);

			pathFollower._transform = transform;
			return pathFollower;
		}
		private static void Stop(Transform transform)
		{
			var pathFollower = transform.GetComponent<PathFollower>();
			if (pathFollower != null) 
			{ 
				pathFollower.StopFollowing(); GameObject.DestroyImmediate(pathFollower); 
				if ( QPathFinder.Logger.CanLogInfo ) QPathFinder.Logger.LogInfo ("PathFollower stopped and its Script is destroyed!");
			}
		}
		private static Vector3 ComputeCatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            Vector3 pt = 0.5f * ((-p0 + 3f * p1 - 3f * p2 + p3) * t3
                        + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
                        + (-p0 + p2) * t
                        + 2f * p1);

            return pt;
        }
		private static Vector3 GetClosestPointOnAnyPath ( int nodeID , PathFinder manager, Vector3 pos, out int pathID )
		{
			Node node = manager.graphData.GetNode ( nodeID );
			Vector3 vClosestPoint = node.Position;
			float fClosestDist = (pos - node.Position).sqrMagnitude;

			bool isOnExtremities = false;
			pathID = -1;

			foreach ( Path p in manager.graphData.paths )
			{
				if ( p.IDOfA == node.autoGeneratedID || p.IDOfB == node.autoGeneratedID ) 
				{
					Vector3 vPos = ComputeClosestPointFromPointToLine(pos, manager.graphData.GetNode( p.IDOfA ).Position , manager.graphData.GetNode(p.IDOfB).Position, out isOnExtremities );
					float fDist = (vPos - pos).sqrMagnitude;

					if (fDist < fClosestDist)
					{
						fClosestDist = fDist;
						vClosestPoint = vPos;
						pathID = p.autoGeneratedID;
					}
				}
			}

			//Debug.DrawLine(pos, vClosestPoint, Color.green, 1f);
			return vClosestPoint;
		}

		private static Vector3 ComputeClosestPointFromPointToLine(Vector3 vPt, Vector3 vLinePt0, Vector3 vLinePt1, out bool isOnExtremities )
        {
            float t = -Vector3.Dot(vPt - vLinePt0, vLinePt1 - vLinePt0) / Vector3.Dot(vLinePt0 - vLinePt1, vLinePt1 - vLinePt0);

            Vector3 vClosestPt;

            if (t < 0f)
			{
                vClosestPt = vLinePt0;
				isOnExtremities = true;
			}
            else if (t > 1f)
			{
                vClosestPt = vLinePt1;
				isOnExtremities = true; 
			}
            else
			{
                vClosestPt = vLinePt0 + t * (vLinePt1 - vLinePt0);
				isOnExtremities = false; 
			}

            //Debug.DrawLine(vPt, vClosestPt, Color.red, 1f);
            return vClosestPt;
        }

		static System.Func<List<System.Object> ,int, Vector3> GetPositionFromNodeOrVector = delegate ( List<System.Object> list, int index ) { return (list[index] is Vector3 ? (Vector3)list[index] : ((Node)list[index]).Position); };
		static System.Func<List<System.Object> ,int, Node> GetNodeFromNodeOrVector = delegate ( List<System.Object> list, int index ) { return (list[index] is Node ? (Node)list[index] : null); };

		#endregion
    }
}

