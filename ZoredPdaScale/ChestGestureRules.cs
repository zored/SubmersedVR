namespace ZoredPdaScale
{
    internal static class ChestGestureRules
    {
        internal static bool MayToggle(bool inUse, bool open, bool paused) { return !paused && (!inUse || open); }
        internal static bool InZone(float right, float up, float forward)
        {
            return right >= -.85f && right <= -.08f && up >= -1.35f && up <= -.12f && forward >= -.35f && forward <= .65f;
        }
    }
}

