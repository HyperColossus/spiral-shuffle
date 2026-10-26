using System;
using System.Collections.Generic;
using Akka.Actor;
using Imcodec.Cryptography;
using Imcodec.MessageLayer.Generated;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.CoreLib.Game.WizBang;
using Imlight.CoreLib.Game.Zone.Core;
using Imlight.CoreLib.Shared.Packets;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Zone.Components;

internal sealed class InteractSeamstressComponent(ZoneEntity entity) : ZoneEntityComponent(entity), IServiceComponent, IComponentFactory {

    public string ServiceName     => "StitchService";
    public string NpcIcon         => null;
    public string NpcNameKey      => null;
    public string NpcTextKey      => null;
    public WizBangs WizBang       => WizBangs.Shopping;
    public string StateName       => "Shop";
    public string InteractWizBang => "Registrar";
    public string DisplayKey      => "GUI_Seamstress";

    public static bool ShouldAttachToEntity(CoreTemplate template)
        => template is GameObjectTemplate gameObjectTemplate
        && (gameObjectTemplate.m_objectName.ToString().Contains("Seamstress", StringComparison.OrdinalIgnoreCase)
            || gameObjectTemplate.m_objectName.ToString().Contains("Eloise", StringComparison.OrdinalIgnoreCase)
            || gameObjectTemplate.m_objectName.ToString().Contains("Stitch", StringComparison.OrdinalIgnoreCase));

    public IEnumerable<ServiceOptionBase> GetServiceOptions(Wizard _)
        => [
            new SeamstressOption {
                m_displayKey = "GUI_Seamstress",
                m_serviceName = ServiceName,
            },
            new UnstitchOption {
                m_displayKey = "GUI_Unstitch",
                m_serviceName = ServiceName,
            }
        ];

    public void OnServiceInteraction(IActorRef playerActor, Wizard playerCharacter, CoreObject playerObject, uint serviceOptionIndex) {
        if (serviceOptionIndex == 0) {
            playerActor.Tell(new WIZARD_12_PROTOCOL.MSG_SEAMSTRESSOPEN {
                GlobalID = Entity.ActiveGameObject.m_globalID,
                ShopTitle = "Stitch",
                Credits = playerCharacter.Account.Crowns,
                MergeCost = 100,
                WebFailure = 0
            });
        }
        else {
            playerActor.Tell(new WIZARD_12_PROTOCOL.MSG_UNSTITCHOPEN {
                GlobalID = Entity.ActiveGameObject.m_globalID,
                ShopTitle = "Unstitch",
                Credits = playerCharacter.Account.Crowns,
                MergeCost = 100,
                WebFailure = 0
            });
        }

        SendPlayerIntoWizbang(playerObject.m_globalID);
        SendPlayerIntoState(playerObject.m_globalID);
    }

    private void SendPlayerIntoWizbang(ulong playerObjID) {
        var wizBangMsg = new GAME_5_PROTOCOL.MSG_WIZBANG {
            WizBangID = (uint) WizBang,
            GameObjectID = playerObjID
        };
        var broadcastMsg = new ZONE_102_PROTOCOL.MSG_ZONEBROADCAST {
            Message = wizBangMsg,
            Selfless = false,
        };
        Entity.ZoneRef.Tell(broadcastMsg);
    }

    private void SendPlayerIntoState(ulong playerObjID) {
        var changeStateMsg = new GAME_5_PROTOCOL.MSG_ENTERSTATE {
            State = StringHash.Compute(StateName),
            GameObjectID = playerObjID
        };
        var broadcastMsg = new ZONE_102_PROTOCOL.MSG_ZONEBROADCAST {
            Message = changeStateMsg,
            Selfless = false,
        };
        Entity.ZoneRef.Tell(broadcastMsg);
    }
}
