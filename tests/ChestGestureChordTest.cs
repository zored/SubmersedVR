using System;
using ZoredPdaScale;
class ChestGestureChordTest {
 static void Main() {
  if (!ChestGestureRules.MayToggle(true, true, false, false)) throw new Exception("normal open PDA gesture allowed");
  if (ChestGestureRules.MayToggle(true, true, false, true)) throw new Exception("right grip must suppress close gesture while PDA is open");
  if (!ChestGestureRules.MayToggle(false, false, false, true)) throw new Exception("closed PDA gesture remains available");
  Console.WriteLine("Chest gesture chord tests passed");
 }
}
