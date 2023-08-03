using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QPathFinder
{
    
    public class PathFollowerToPositionAndSnapToGround : PathFollower
    {
        Vector3 directionOfRayCast;
        float offsetDistanceFromPoint;
        int maxDistanceForRayCast;
        LayerMask backgroundLayerMask;
        float offsetDistanceToFloatFromGround;

        public void SetContext ( Vector3 directionOfRayCast, float offsetDistanceFromPoint, float offsetDistanceToFloatFromGround, int maxDistanceForRayCast, int groundLayer )
        {
            this.directionOfRayCast = directionOfRayCast.normalized;
            this.offsetDistanceFromPoint = offsetDistanceFromPoint;
            this.maxDistanceForRayCast = maxDistanceForRayCast;
            this.offsetDistanceToFloatFromGround = offsetDistanceToFloatFromGround;

            backgroundLayerMask = 1 << groundLayer;
        }

        public Vector3 AdjustPositionIfNeeded ( Vector3 point )
        {
            RaycastHit hitInfo;
            if (Physics.Raycast ( point + offsetDistanceFromPoint * (-directionOfRayCast), directionOfRayCast,out hitInfo, maxDistanceForRayCast, backgroundLayerMask.value ) )
            {
                Vector3 hitPos = hitInfo.point;
				hitPos = hitPos + offsetDistanceToFloatFromGround * (-directionOfRayCast);
				return hitPos;
            }

            if ( QPathFinder.Logger.CanLogError )
            {

                QPathFinder.Logger.LogError("Ground not found at " + point + ". Could not snap to ground properly! Raycast origin: " + (point + offsetDistanceFromPoint * (-directionOfRayCast)) +
                        " raycast direction:" + directionOfRayCast + " Distance of raycase:" + maxDistanceForRayCast);


                Debug.DrawLine ( point + offsetDistanceFromPoint * (-directionOfRayCast), point + offsetDistanceFromPoint * (-directionOfRayCast) + directionOfRayCast * maxDistanceForRayCast, Color.red, QPathFinder.Logger.DrawLineDuration );
            }
            return point;
            
        }
		public override void MoveTo(int pointIndex)
		{
			var targetPos = CastToVec( _pathToFollow[pointIndex] );

			var deltaPos = targetPos - _transform.position;
			//deltaPos.z = 0f;
			_transform.up = Vector3.up;
			_transform.forward = deltaPos.normalized;

			if ( directionOfRayCast.x != 0 )
				targetPos.x = transform.position.x;
			else if ( directionOfRayCast.y != 0 )
				targetPos.y = transform.position.y;
			else if ( directionOfRayCast.z != 0 )
				targetPos.z = transform.position.z;

			var newTransformPos =	Vector3.MoveTowards(_transform.position, targetPos, moveSpeed * Time.smoothDeltaTime);
			newTransformPos = AdjustPositionIfNeeded ( newTransformPos );;

			if ( QPathFinder.Logger.CanLogInfo ) Debug.DrawLine( transform.position, newTransformPos, Color.blue, QPathFinder.Logger.DrawLineDuration );

			_transform.position = newTransformPos;
		}


		protected override bool IsOnPoint(int pointIndex) 
		{ 
			Vector3 finalPoint = AdjustPositionIfNeeded( CastToVec( _pathToFollow[pointIndex] ) );
			float mag = (_transform.position - finalPoint).sqrMagnitude; 
			return mag < 0.1f;
		}

    }
}
