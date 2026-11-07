using System;
using ZoredPdaScale;
class RippleRulesTest
{
 static void Check(bool value,string message) { if (!value) throw new Exception(message); }
 static void Main() {
  var r=new RippleRules();
  Check(r.Update(true,true,0f,0f,1f),"press emits");
  Check(!r.Update(false,true,0f,0f,1.1f),"stationary finger emits nothing");
  Check(!r.Update(false,true,.03f,0f,1.11f),"throttle prevents dense trail");
  Check(!r.Update(false,true,.03f,0f,1.2f),"movement is throttled to half the previous rate");
  Check(r.Update(false,true,.03f,0f,1.25f),"moving finger emits after the longer interval");
  Check(!r.Update(false,false,.2f,0f,2f),"release emits nothing");
  Check(r.Update(true,true,.2f,0f,2.01f),"new press immediately emits");
  Console.WriteLine("RIPPLE_RULES_OK");
 }
}