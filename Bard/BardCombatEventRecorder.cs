using Dalamud.Game.ClientState.JobGauge.Enums;
using PromeRotation.Managers.CombatEventManager.Events;
using Wotou.Bard.Data;

namespace Wotou.Bard;

public static class BardCombatEventRecorder
{
    public static void ResetBattleState()
    {
        if (BardSettings.Instance.ResetSongOrder)
            BardSettings.Instance.ResetSongOrderCustom();

        BardBattleData.Instance.ResetForBattle();
    }

    public static void OnActionEffect(ActionEffectEvent ev)
    {
        var me = Core.Core.Me;
        if (me == null || ev.SourceId != me.EntityId)
            return;

        BardBattleData.Instance.RecordAction(ev.ActionId);
        BardBattleData.Instance.RecordGcdAction(ev.ActionId);
        BardBattleData.Instance.Record120SBuffAction(ev.ActionId);
        ResetIronJawsBurstUsage(ev.ActionId);
        RecordSong(ev.ActionId);
        RecordIronJawsBurstUsage(ev.ActionId);
        RecordApexUsage(ev.ActionId);
    }

    private static void RecordSong(uint actionId)
    {
        var song = BardSongHelper.GetSongBySpell(actionId);
        if (song == Song.None)
            return;

        BardBattleData.Instance.LastSong = song;
        BardBattleData.Instance.LastSongTime = Environment.TickCount64;
    }

    private static void RecordIronJawsBurstUsage(uint actionId)
    {
        if (actionId == BRDSkill.IronJaws && BardHelper.HasAllPartyBuff())
            BardBattleData.Instance.HasUseIronJawsInCurrentBursting = true;
    }

    private static void ResetIronJawsBurstUsage(uint actionId)
    {
        if (actionId == BRDSkill.RagingStrikes)
            BardBattleData.Instance.HasUseIronJawsInCurrentBursting = false;
    }

    private static void RecordApexUsage(uint actionId)
    {
        if (actionId == BRDSkill.ApexArrow && BardHelper.HasNoPartyBuff())
            BardBattleData.Instance.HasUseApexArrowInCurrentNonBurstingPeriod = true;
    }
}
