using UnityEngine;
using UnityEngine.Splines;

namespace TreeGeneration
{
    public struct SplineDataStruct
    {
        public GameObject Object;
        public Spline Spline;
        public SplineContainer Container;
        public CustomExtrudeSpline Extrude;
    }
}