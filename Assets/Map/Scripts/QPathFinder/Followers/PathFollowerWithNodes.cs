using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QPathFinder
{
    public class PathFollowerWithNodes : PathFollower
    {
        public override void MoveTo(int pointIndex)
        {
            var targetNode = CastToNode( _pathToFollow[pointIndex] ) ;

                var deltaPos = targetNode.Position - _transform.position;
                //deltaPos.z = 0f;
                if ( alignToPath )
                {
                    _transform.up = Vector3.up;
                    _transform.forward = deltaPos.normalized;
                }

			_transform.position =	Vector3.MoveTowards(_transform.position, targetNode.Position, moveSpeed * Time.smoothDeltaTime);
        }

        protected override bool IsOnPoint(int pointIndex) { return (_transform.position - CastToNode( _pathToFollow[pointIndex] ).Position ).sqrMagnitude < 0.1f; }
    }
}
