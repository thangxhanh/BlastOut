using System;
// using PathCreation;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Dacodelaac.Common
{
    public class PathCreatorToLineRenderer : MonoBehaviour
    {
        [SerializeField] float segmentDistance = 0.1f;

        void Awake()
        {
            // Destroy(GetComponent<PathCreator>());
        }

        [ContextMenu("Setup")]
        public void Setup()
        {
//             var pathCreator = GetComponent<PathCreator>();
//             var vertexPath = pathCreator.GetVertexPath(segmentDistance);
//
//             var lineRenderer = GetComponent<LineRenderer>();
//             lineRenderer.positionCount = vertexPath.NumPoints;
//             lineRenderer.SetPositions(vertexPath.localPoints);
// #if UNITY_EDITOR
//             EditorUtility.SetDirty(lineRenderer);
// #endif
        }
    }
}