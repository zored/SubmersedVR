namespace ZoredPdaScale
{
    internal static class MenuControlRules
    {
        internal static float Adjust(float value, float change, float min, float max) { return System.Math.Max(min, System.Math.Min(max, value + change)); }
        internal static bool MayRecover(bool inUse, bool focused, bool explicitMenu, float timeScale, bool hasFreezers) { return inUse && (focused || explicitMenu) && timeScale == 0f && !hasFreezers; }
        internal static bool ShouldRetryCamera(float now, float nextAttempt) { return now >= nextAttempt; }
        internal static bool ShouldStartWatcher(bool enabled, bool exists) { return enabled && !exists; }
        internal static int Steps(float value, float baseline, float step) { return (int)System.Math.Round((value - baseline) / step, System.MidpointRounding.AwayFromZero); }
    }
}
