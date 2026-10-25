using System.Reflection;
using System.Text;
using Imcodec.IO;
using Imcodec.MessageLayer.Generated;
using Imlight.CoreLib.Game;
using Imlight.CoreLib.Game.Services;
using Imlight.CoreLib.Shared.Networking;
using Imlight.CoreLib.Shared.Packets;
using Imlight.CoreLib.WizardData.Models.Player;

var session=new SessionActor();
var service=new SpellRewardService(session);
int checks=0;
void Check(bool ok,string name) { if(!ok)throw new Exception(name); checks++; }
void Call(string name,params object[] args) {
    try { typeof(SpellRewardService).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(service,args); }
    catch(TargetInvocationException e) { throw e.InnerException!; }
}
void Poll(int token=0) {
    typeof(SpellRewardService).GetField("_lastPoll",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(service,DateTime.MinValue);
    Call("ReceivePoll",new GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS {JobID=770103,Message=(ByteString)BitConverter.GetBytes(token)});
}
Poll();
Check(session.Sent.Count==0,"A poll before attachment must not access a missing wizard or send a response.");
Check(session.WizardLookups==0,"An early UI poll must not synchronously query another actor.");
Call("StartRewards");
Check(session.Sent.Count==0,"Attach completion without a wizard must be harmless.");
var wizard=new Wizard();
Call("ReceiveActiveWizard",new CHARACTER_103_PROTOCOL.MSG_SETACTIVEWIZARD {Wizard=wizard});
Poll();
Check(session.Sent.Count==0,"Character assignment alone must not enable polling before attach completes.");
Call("StartRewards");
Check(session.Sent.Count==2,"A completed attachment must start one reward UI process.");
Poll();
Check(Encoding.UTF8.GetString((byte[])((GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS)session.Sent.Last()).Message)=="SpellRewardEmpty","Attached polls must respond normally.");
wizard.IsInDuel=true;
Poll();
Check(Encoding.UTF8.GetString((byte[])((GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS)session.Sent.Last()).Message)=="SpellRewardCombat","Combat state must use the assigned live wizard.");
int before=session.Sent.Count;
Call("ReceiveActiveWizard",new CHARACTER_103_PROTOCOL.MSG_SETACTIVEWIZARD {Wizard=null!});
Poll();
Call("ReceiveRewards",new SpellDropRewards([101],1));
Check(session.Sent.Count==before,"Late polls and rewards after clearing attachment must be harmless.");
Check(session.WizardLookups==0,"The reward lifecycle must not make any synchronous wizard queries.");
wizard.IsInDuel=false;
Call("ReceiveActiveWizard",new CHARACTER_103_PROTOCOL.MSG_SETACTIVEWIZARD {Wizard=wizard});
Call("StartRewards");
Call("ReceiveRewards",new SpellDropRewards([101],1));
Poll();
Check(Encoding.UTF8.GetString((byte[])((GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS)session.Sent.Last()).Message).StartsWith("SpellReward|"),"Real reward notices must still be delivered after reattachment.");
Console.WriteLine($"PASS: {checks} reward-service lifecycle checks using the actual service source.");
