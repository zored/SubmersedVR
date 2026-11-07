namespace ZoredPdaScale
{
    internal static class MenuControlRules
    {
        internal static void AdjustPdaSize(float ui, float tablet, bool increase, out float newUi, out float newTablet)
        {
            ui = System.Math.Max(.5f, System.Math.Min(3f, ui));
            tablet = System.Math.Max(.5f, System.Math.Min(3f, tablet));
            float factor = increase ? 1.05f : 1f / 1.05f;
            float minimum = System.Math.Max(.5f / ui, .5f / tablet);
            float maximum = System.Math.Min(3f / ui, 3f / tablet);
            factor = System.Math.Max(minimum, System.Math.Min(maximum, factor));
            newUi = ui * factor;
            newTablet = tablet * factor;
        }
        internal static float Adjust(float value, float change, float min, float max) { return System.Math.Max(min, System.Math.Min(max, value + change)); }
        internal static bool MayRecover(bool inUse, bool focused, bool explicitMenu, float timeScale, bool hasFreezers) { return inUse && (focused || explicitMenu) && timeScale == 0f && !hasFreezers; }
        internal static bool ShouldRetryCamera(float now, float nextAttempt) { return now >= nextAttempt; }
        internal static bool ShouldStartWatcher(bool enabled, bool exists) { return enabled && !exists; }
        internal static int Steps(float value, float baseline, float step) { return (int)System.Math.Round((value - baseline) / step, System.MidpointRounding.AwayFromZero); }
    }
}
