namespace ZoredPdaScale
{
    internal static class PdaOpeningVisibilityRules
    {
        internal static bool ShouldReveal(bool skipped, bool opening, float elapsed)
        {
            return skipped || !opening || elapsed >= .25f;
        }
    }
}
