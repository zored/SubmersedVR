using System;
class SelectionTest {
    static void Main() {
        Check(Selection.Choose(new long[] { 5, 9, 7 }, new bool[] { false, false, false }) == 1);
        Check(Selection.Choose(new long[] { 5, 9 }, new bool[] { false, true }) == -1);
        Check(Selection.Choose(new long[0], new bool[0]) == -1);
        Check(Selection.Choose(new long[] { 0 }, new bool[] { false }) == -1);
        Console.WriteLine("Selection tests passed");
    }
    static void Check(bool condition) { if (!condition) throw new Exception("Selection failed"); }
}
