// Dependency doubles isolate lifecycle handling in the actual SpellRewardService source.
namespace Akka.Actor { internal sealed class NamespaceMarker { } }
namespace Imcodec.IO {
    internal sealed class ByteString(byte[] bytes) {
        private readonly byte[] data = bytes;
        public static implicit operator ByteString(byte[] value) => new(value);
        public static implicit operator byte[](ByteString value) => value.data;
    }
}
namespace Imcodec.MessageLayer {
    internal static class MessageEncoder {
        public static byte[] Encode(Imcodec.MessageLayer.Generated.MG2_MESSAGES_43_PROTOCOL.MSG_MG2_REWARDS message)
            => new byte[8].Concat(System.Text.Encoding.UTF8.GetBytes(message.gameName)).ToArray();
    }
}
namespace Imcodec.MessageLayer.Generated {
    internal static class GAME_5_PROTOCOL {
        internal sealed class MSG_KILL_CLIENT_PROCESS { public uint JobID; }
        internal sealed class MSG_START_CLIENT_PROCESS { public ulong OwnerGID; public uint JobID; public string ScriptName; }
        internal sealed class MSG_MESSAGE_PROCESS { public uint JobID; public Imcodec.IO.ByteString Message; }
    }
    internal static class MG2_MESSAGES_43_PROTOCOL {
        internal sealed class MSG_MG2_REWARDS { public int score; public string gameName; }
    }
}
namespace Imcodec.ObjectProperty.TypeCache {
    internal sealed class Rank {
        public int m_spellRank=1, m_shadowPips, m_balancePips, m_deathPips, m_firePips, m_icePips, m_lifePips, m_mythPips, m_stormPips;
        public bool m_xPipSpell;
    }
    internal sealed class SpellTemplate {
        public Rank m_spellRank=new(); public string m_displayName="Firecat", m_sMagicSchoolName="Fire"; public int m_accuracy=75;
    }
}
namespace Imlight.CoreLib.WizardData.Models.Player {
    internal sealed class Spellbook { public bool HasSpell(uint id) => true; }
    internal sealed class Wizard { public ulong GameObjectID=123; public bool IsInDuel; public Spellbook SpellbookBehavior=new(); }
}
namespace Imlight.CoreLib.Shared.Packets {
    internal static class SERVICE_101_PROTOCOL { internal sealed class MSG_ATTACHCOMPLETE { } }
    internal static class CHARACTER_103_PROTOCOL { internal sealed class MSG_SETACTIVEWIZARD { public Imlight.CoreLib.WizardData.Models.Player.Wizard Wizard; } }
}
namespace Imlight.CoreLib.Shared.Networking {
    [AttributeUsage(AttributeTargets.Method)] internal sealed class MessageHandlerAttribute(Type type) : Attribute { }
    internal sealed class SessionActor { public List<object> Sent=new(); public int WizardLookups; public Imlight.CoreLib.WizardData.Models.Player.Wizard QueryResult; }
    internal abstract class MessageService(SessionActor session) {
        protected Imlight.CoreLib.WizardData.Models.Player.Wizard GetActiveWizard() { session.WizardLookups++; return session.QueryResult; }
        protected void SendToSocket(object message) => session.Sent.Add(message);
    }
}
namespace Imlight.CoreLib.Shared.Resources {
    internal static class CoreObjectFactory { public static object GetCoreTemplate(uint id) => new Imcodec.ObjectProperty.TypeCache.SpellTemplate(); }
}
namespace Imlight.CoreLib.Game {
    internal sealed class Variant { public uint[] Variants=[101,102]; }
    internal static class RandomizerRuntime {
        public static bool Enabled=true;
        public static Variant[] DropSpellPool=[new()];
        public static bool TryReadMenuMessage(Imcodec.IO.ByteString message,string name,out int token) {
            byte[] raw=message; token=raw.Length==4?BitConverter.ToInt32(raw):0; return Enabled && raw.Length==4;
        }
    }
}
