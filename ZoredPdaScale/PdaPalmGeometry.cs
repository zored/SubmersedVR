using UnityEngine;

namespace ZoredPdaScale
{
    internal static class PdaPalmGeometry
    {
        internal static Vector3 StraightTipInHand(Transform first, Transform middle, Transform tip)
        {
            var firstRest = Matrix4x4.TRS(first.localPosition, Quaternion.identity, first.localScale);
            var middleRest = Matrix4x4.TRS(middle.localPosition, Quaternion.identity, middle.localScale);
            return (firstRest * middleRest).MultiplyPoint3x4(tip.localPosition);
        }
    }
}
