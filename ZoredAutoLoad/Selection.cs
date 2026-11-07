internal static class Selection {
    internal static int Choose(long[] dates, bool[] corrupt) {
        int best = -1;
        long latest = 0;
        for (int i = 0; i < dates.Length; i++)
            if (dates[i] > latest) { latest = dates[i]; best = i; }
        return best >= 0 && !corrupt[best] ? best : -1;
    }
}
