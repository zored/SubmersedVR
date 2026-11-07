using System;
using ZoredPdaScale;
class OpeningVisibilityTest {
 static void Check(bool x,string m) { if (!x) throw new Exception(m); }
 static void Main() {
  Check(!PdaOpeningVisibilityRules.ShouldReveal(false,true,.02f),"opening pose remains hidden");
  Check(PdaOpeningVisibilityRules.ShouldReveal(true,true,.03f),"completed pose can be revealed");
  Check(PdaOpeningVisibilityRules.ShouldReveal(false,true,.25f),"bounded fallback prevents hidden PDA");
  Check(PdaOpeningVisibilityRules.ShouldReveal(false,false,.01f),"aborted opening restores visibility");
 }
}
