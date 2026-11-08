using System;
using ZoredPdaScale;
class ReloadChordTest {
 static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
 static void Main() {
  var chord = new ReloadChordRules();
  Check(!chord.Step(true, true, true, 0f), "startup held is disarmed");
  Check(!chord.Step(true, false, false, .1f), "release arms");
  Check(!chord.Step(true, true, true, 1f), "start hold");
  Check(!chord.Step(true, true, true, 3.99f), "not before 3 seconds");
  Check(chord.Step(true, true, true, 4f), "fires at 3 seconds");
  Check(!chord.Step(true, true, true, 8f), "fires only once");
  chord.Step(true, false, false, 9f);
  chord.Step(false, true, true, 10f);
  Check(!chord.Step(false, true, true, 14f), "closed PDA cannot fire");
  chord.Step(true, false, false, 15f);
  chord.Step(true, true, true, 16f);
  chord.Step(true, false, true, 18f);
  Check(!chord.Step(true, true, true, 22f), "interrupted hold requires full release");
  Console.WriteLine("Reload chord tests passed");
 }
}
