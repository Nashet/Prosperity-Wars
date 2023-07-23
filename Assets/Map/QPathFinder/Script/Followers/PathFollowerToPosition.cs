using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QPathFinder
{
    public class PathFollowerToPosition : PathFollower
    {
        public override void MoveTo(int pointIndex)
        {
            var targetPos = CastToVec( _pathToFollow[pointIndex] ) ;

                var deltaPos = targetPos - _transform.position;
                //deltaPos.z = 0f;
                if ( alignToPath )
                {
                    _transform.up = Vector3.up;
                    _transform.forward = deltaPos.normalized;
                }

			_transform.position =	Vector3.MoveTowards(_transform.position, targetPos, moveSpeed * Time.smoothDeltaTime);
        }

        protected override bool IsOnPoint(int pointIndex) { return (_transform.position - CastToVec( _pathToFollow[pointIndex]) ).sqrMagnitude < 0.1f; }
    }
}
