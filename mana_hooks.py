"""Keep server mana and the client display full while the account toggle is on."""
from collections.abc import Callable


def patch_infinite_mana(replace: Callable[[str, str, str], None]) -> None:
    replace("Login/Services/CharacterService.cs", """                 Imlight.CoreLib.Game.EquipmentItemCards.HandleMenuSetting(account.AccountId, message.Message, out var itemCardResponse)) {
            SendToSocket(new GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS {
                JobID = Imlight.CoreLib.Game.RandomizerRuntime.MenuJobId, Message = itemCardResponse
            });
        }""", """                 Imlight.CoreLib.Game.EquipmentItemCards.HandleMenuSetting(account.AccountId, message.Message, out var itemCardResponse)) {
            SendToSocket(new GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS {
                JobID = Imlight.CoreLib.Game.RandomizerRuntime.MenuJobId, Message = itemCardResponse
            });
        }
        else if (account is not null && message.JobID == Imlight.CoreLib.Game.RandomizerRuntime.MenuJobId &&
                 Imlight.CoreLib.Game.InfiniteMana.HandleMenuSetting(account.AccountId, message.Message, out var manaResponse)) {
            SendToSocket(new GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS {
                JobID = Imlight.CoreLib.Game.RandomizerRuntime.MenuJobId, Message = manaResponse
            });
        }""")
    replace("Shared/Character/CharacterHelper.cs",
            '        Logger.Debug("Game stats recalculated for {0}.",',
            '''        Imlight.CoreLib.Game.InfiniteMana.Refill(wizard);
        Logger.Debug("Game stats recalculated for {0}.",''')
    replace("WizardData/Models/Player/Wizard.cs", "        GameStats.m_currentMana = newMana;",
            "        GameStats.m_currentMana = Imlight.CoreLib.Game.InfiniteMana.Resolve(this, newMana);")
    replace("Game/Zone/Components/CombatDuelComponent.cs",
            "        // All spells have been called. Inform the client whether this duel continues or ends.",
            """        EnactActionOnSubCircles(circle => {
            if (circle._wizard is not null) {
                Imlight.CoreLib.Game.InfiniteMana.Synchronize(circle._wizard, circle.ParticipantActor);
            }
        });

        // All spells have been called. Inform the client whether this duel continues or ends.""")
    replace("Game/Services/MoveService.cs", "        wizard.GameStats.m_currentMana = (int) newMana;",
            "        wizard.UpdateMana((int) newMana);")
    replace("Game/Zone/Components/CombatDuelComponent.cs", """            caster._wizard.UpdateMana(0);
            actor.Tell(new WIZARD_12_PROTOCOL.MSG_UPDATEMANA {
                Mana = 0,""", """            caster._wizard.UpdateMana(0);
            actor.Tell(new WIZARD_12_PROTOCOL.MSG_UPDATEMANA {
                Mana = caster._wizard.GameStats.m_currentMana,""")
