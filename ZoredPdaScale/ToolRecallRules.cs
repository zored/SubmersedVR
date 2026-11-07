namespace ZoredPdaScale
{
    internal static class ToolRecallRules
    {
        internal static bool InZone(float right, float up, float forward)
        { return right >= -.15f && right <= .85f && up >= -1.35f && up <= -.12f && forward >= -.35f && forward <= .65f; }
        internal static bool ShoulderZone(float right, float up, float forward)
        { return right >= .05f && right <= .85f && up >= -.3f && up <= .4f && forward >= -.65f && forward <= -.08f; }
        internal static bool MayRecall(bool pda, bool menu, bool vehicle, bool emptyHand, bool hasItem)
        { return !pda && !menu && !vehicle && emptyHand && hasItem; }
    }
}


