namespace ZoredPdaScale
{
    internal static class PdaSurfaceRules
    {
        internal static float ProbeStartDistance(float distance, bool held) { return held && distance < 0f ? .1f : distance + .1f; }
        internal static bool AcceptDepth(float distance, bool held) { return held || distance >= -.08f; }
        internal static float VisualCorrection(float distance, bool held) { return held && distance < 0f ? -distance : 0f; }
    }
}
