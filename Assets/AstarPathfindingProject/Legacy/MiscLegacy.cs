using UnityEngine;
using Pathfinding.Util;

namespace Pathfinding {
	// Obsolete methods in AIPath
	public partial class AIPath {
		/** True if the end of the path has been reached.
		 * \deprecated When unifying the interfaces for different movement scripts, this property has been renamed to #reachedEndOfPath
		  */
		[System.Obsolete("When unifying the interfaces for different movement scripts, this property has been renamed to reachedEndOfPath.  [AstarUpgradable: 'TargetReached' -> 'reachedEndOfPath']")]
		public bool TargetReached { get { return reachedEndOfPath; } }

		/** Rotation speed.
		 * \deprecated This field has been renamed to #rotationSpeed and is now in degrees per second instead of a damping factor.
		 */
		[System.Obsolete("This field has been renamed to #rotationSpeed and is now in degrees per second instead of a damping factor")]
		public float turningSpeed { get { return rotationSpeed/90; } set { rotationSpeed = value*90; } }

		/** Maximum speed in world units per second.
		 * \deprecated Use #maxSpeed instead
		 */
		[System.Obsolete("This member has been deprecated. Use 'maxSpeed' instead. [AstarUpgradable: 'speed' -> 'maxSpeed']")]
		public float speed { get { return maxSpeed; } set { maxSpeed = value; } }

		/** Direction that the agent wants to move in (excluding physics and local avoidance).
		 * \deprecated Only exists for compatibility reasons. Use #desiredVelocity or #steeringTarget instead instead.
		 */
		[System.Obsolete("Only exists for compatibility reasons. Use desiredVelocity or steeringTarget instead.")]
		public Vector3 targetDirection {
			get {
				return (steeringTarget - tr.position).normalized;
			}
		}

		/** Current desired velocity of the agent (excluding physics and local avoidance but it includes gravity).
		 * \deprecated This method no longer calculates the velocity. Use the #desiredVelocity property instead.
		 */
		[System.Obsolete("This method no longer calculates the velocity. Use the desiredVelocity property instead")]
		public Vector3 CalculateVelocity (Vector3 position) {
			return desiredVelocity;
		}
	}
}
