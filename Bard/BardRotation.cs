using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.JobGauge.Enums;
using ECommons.ExcelServices;
using PromeRotation;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.Resolvers;
using PromeRotation.Rotation;
using Wotou.Bard;
using Wotou.Bard.Action;
using Wotou.Bard.Data;
using Wotou.Bard.Opener;
using PromeRotation.UI;
using PromeRotation.UI.HotKey;
using PromeRotation.UI.Hotkeys;

namespace Wotou.Bard;

[RotationMetadata((uint)Job.BRD, "诗人", "Wotou", "1.0.0.0")]
public class BardRotation : IRotation
{
    public string RotationName => "诗人";
    public uint JobId => (uint)Job.BRD;

    //private readonly IRotationEventHandler _eventHandler = new DancerRotationRotationEventHandler();
   // public IRotationEventHandler GetEventHandler() => _eventHandler;

    private readonly List<IDecisionResolver> _gcdResolvers = new();
    private readonly List<IDecisionResolver> _offGcdResolvers = new();

    public BardRotation()
    {
        _offGcdResolvers.Add(new BardPotionOffGcd());
        _offGcdResolvers.Add(new BardRadiantFinaleOffGcd());
        _offGcdResolvers.Add(new BardRagingStrikesOffGcd());
        _offGcdResolvers.Add(new BardBattleVoiceOffGcd());
        _offGcdResolvers.Add(new BardEmpyrealArrowOffGcd());
        _offGcdResolvers.Add(new BardPitchPerfectMaxOffGcd());
        _offGcdResolvers.Add(new BardHeartBreakMaxChargeOffGcd());
        _offGcdResolvers.Add(new BardBarrageOffGcd());
        _offGcdResolvers.Add(new BardSidewinderOffGcd());
        _offGcdResolvers.Add(new BardPitchPerfectOffGcd());
        _offGcdResolvers.Add(new BardSongAbility());
        _offGcdResolvers.Add(new BardHeartBreakOffGcd());

        _gcdResolvers.Add(new BardBlastArrowMaxGcd());
        _gcdResolvers.Add(new BardIronJawsGcd());
        _gcdResolvers.Add(new BardApexMaxGcd());
        _gcdResolvers.Add(new BardRadiantEncoreMaxGcd());
        _gcdResolvers.Add(new BardResonantArrowMaxGcd());
        _gcdResolvers.Add(new BardBarrageBuffMaxGcd());
        _gcdResolvers.Add(new BardDotGcd());
        _gcdResolvers.Add(new BardApexWithoutBurstGcd());
        _gcdResolvers.Add(new BardRefulgentArrowMaxGcd());
        _gcdResolvers.Add(new BardApexGcd());
        _gcdResolvers.Add(new BardBlastArrowGcd());
        _gcdResolvers.Add(new BardRadiantEncoreGcd());
        _gcdResolvers.Add(new BardResonantArrowGcd());
        _gcdResolvers.Add(new BaseGcd());

        foreach (var (name, def) in QtList)
            PromeSettings.Instance.AddQt(name, def);

        RegisterHotkeys();
    }

    private static void RegisterHotkeys()
    {
        HotkeyUI.AddHotkey(new ActionHotkey(new PAction(BRDSkill.ArmsLength, ActionType.OffGcd, ActionTargetType.Self)));
        HotkeyUI.AddHotkey(new ActionHotkey(new PAction(BRDSkill.IronJaws, ActionType.Gcd, ActionTargetType.Target)));
        HotkeyUI.AddHotkey(new ActionHotkey(new PAction(BRDSkill.SecondWind, ActionType.OffGcd, ActionTargetType.Self)));
        HotkeyUI.AddHotkey(new ActionHotkey(new PAction(BRDSkill.Troubadour, ActionType.OffGcd, ActionTargetType.Self)));
        HotkeyUI.AddHotkey(new ActionHotkey(new PAction(BRDSkill.NaturesMinne, ActionType.OffGcd, ActionTargetType.Target)));
        HotkeyUI.AddHotkey(new ActionHotkey(new PAction(3, ActionType.OffGcd, ActionTargetType.Self)));
        HotkeyUI.AddHotkey(new ActionHotkey(new PAction(BRDSkill.RepellingShot, ActionType.OffGcd, ActionTargetType.Target)));
        HotkeyUI.AddHotkey(new DelegateHotkey(
            "爆发药",
            new ExecuteLogic(EnqueuePotionHotkey),
            iconActionId: BRDSkill.Potion));
        HotkeyUI.AddHotkey(new DelegateHotkey(
            "停止自动移动",
            new ExecuteLogic(StopGreenMoveHotkey),
            customIconPath: "Resources/stop-sign.png"));
        HotkeyUI.AddHotkey(new DelegateHotkey(
            "绝峰箭",
            new ExecuteLogic(EnqueueApexArrowHotkey),
            iconActionId: BRDSkill.ApexArrow));
        HotkeyUI.AddHotkey(new ActionHotkey(new PAction(BRDSkill.HeadGraze, ActionType.OffGcd, ActionTargetType.Target)));
    }

    private static void EnqueuePotionHotkey()
    {
        var potionId = Core.GameData.GetBestPotionId();
        if (potionId == 0)
            return;

        ActionQueueManager.Enqueue(new PAction(potionId, ActionType.Item, ActionTargetType.Self), isHighPriority: true);
    }

    private static void EnqueueApexArrowHotkey()
    {
        if (JobGaugeHelper.BRD.GetSoulVoice < 20)
            return;

        ActionQueueManager.Enqueue(new PAction(BardHelper.Adjust(BRDSkill.ApexArrow), ActionType.Gcd, ActionTargetType.Target), isHighPriority: true);
    }

    private static void StopGreenMoveHotkey()
    {
        Plugin.Instance.GreenMoveSystem.Stop();
    }

    public static IReadOnlyDictionary<string, bool> QtList { get; } = new Dictionary<string, bool>
    {
        { BRDQt.Burst, true },
        { BRDQt.Potion, false },
        { BRDQt.Apex, true },
        { BRDQt.DOT, true },
        { BRDQt.Song, true },
        { BRDQt.BurstWithWanderer, true },
        { BRDQt.EmpyrealArrow, true },
        { BRDQt.Sidewinder, true },
        { BRDQt.HeartBreakSave, true },
        { BRDQt.ClearHawkEyesBuffBeforeDots, true },
        { BRDQt.AOE, false }
    };
    
    public static IReadOnlyDictionary<string, Type> Openers { get; } = new Dictionary<string, Type>
    {
        {"90-100级 3G团辅起手", typeof(Bard3GOpener100)},
        {"90-100级 2G团辅起手", typeof(Bard2GOpener100)}
    };

    public PAction? NextGcd()
    {
        // 遍历所有GCD解析器
        foreach (var resolver in _gcdResolvers)
        {
            if (resolver.Check().Success)
            {
                // 找到第一个满足条件的，返回它的决策结果
                return resolver.GetAction();
            }
        }
        // 如果所有求解器都不满足条件，返回null
        return null;
    }

    public PAction? NextOffGcd()
    {
        // 遍历所有oGCD解析器 同上
        foreach (var resolver in _offGcdResolvers)
        {
            if (resolver.Check().Success)
            {
                return resolver.GetAction();
            }
        }
        return null;
    }

    public void UpdateDebugStatus()
    {
        RotationManager.GcdSolverStatus.Clear();
        RotationManager.OffGcdSolverStatus.Clear();

        foreach (var resolver in _gcdResolvers)
        {
            var result = resolver.Check();
            RotationManager.GcdSolverStatus.Add(new SolverStatus
            {
                Name = resolver.GetType().Name,
                Success = result.Success,
                Message = result.Message
            });
        }

        foreach (var resolver in _offGcdResolvers)
        {
            var result = resolver.Check();
            RotationManager.OffGcdSolverStatus.Add(new SolverStatus
            {
                Name = resolver.GetType().Name,
                Success = result.Success,
                Message = result.Message
            });
        }

        RotationManager.OffGcdSolverStatus.Add(BardNoTargetSongSwitcher.GetStatus());
    }

    public IOpener? GetOpener()
    {
        return BardSettings.Instance.Opener switch
        {
            1 => new Bard2GOpener100(),
            _ => new Bard3GOpener100()
        };
    }

    public IRotationEventHandler GetEventHandler()
    {
        return new BardRoationEventHandler();
    }

    public void DrawSettings()
    {
        if (!ImGui.BeginTabBar("Settings##Bard")) return;

        if (ImGui.BeginTabItem("歌轴"))
        {
            DrawSongSettings();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("起手"))
        {
            DrawOpenerSettings();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    public void DrawQTs()
    {
    }

    private static void DrawSongSettings()
    {
        var settings = BardSettings.Instance;

        var resetSongOrder = settings.ResetSongOrder;
        if (ImGui.Checkbox("战斗开始重置歌序", ref resetSongOrder))
            settings.ResetSongOrder = resetSongOrder;

        var firstSong = settings.FirstSong;
        if (DrawSongCombo("第一首", ref firstSong))
        {
            settings.FirstSong = firstSong;
            settings.SyncSongOrderOnReset();
        }

        var secondSong = settings.SecondSong;
        if (DrawSongCombo("第二首", ref secondSong))
        {
            settings.SecondSong = secondSong;
            settings.SyncSongOrderOnReset();
        }

        var thirdSong = settings.ThirdSong;
        if (DrawSongCombo("第三首", ref thirdSong))
        {
            settings.ThirdSong = thirdSong;
            settings.SyncSongOrderOnReset();
        }

        var wandererDuration = settings.WandererSongDuration;
        if (ImGui.SliderFloat("旅神歌时长", ref wandererDuration, 3f, 45f, "%.1f"))
            settings.WandererSongDuration = wandererDuration;

        var mageDuration = settings.MageSongDuration;
        if (ImGui.SliderFloat("贤者歌时长", ref mageDuration, 3f, 45f, "%.1f"))
            settings.MageSongDuration = mageDuration;

        var armyDuration = settings.ArmySongDuration;
        if (ImGui.SliderFloat("军神歌时长", ref armyDuration, 3f, 45f, "%.1f"))
            settings.ArmySongDuration = armyDuration;

        var wandererBeforeGcdTime = settings.WandererBeforeGcdTime;
        if (ImGui.SliderInt("旅神GCD前置毫秒", ref wandererBeforeGcdTime, 0, 1500))
            settings.WandererBeforeGcdTime = wandererBeforeGcdTime;

        if (ImGui.Button("重置默认歌序"))
            settings.ResetSongOrderNormal();
    }

    private static bool DrawSongCombo(string label, ref Song song)
    {
        var changed = false;
        if (!ImGui.BeginCombo(label, SongDisplayName(song))) return false;

        foreach (var candidate in new[] { Song.WanderersMinuet, Song.MagesBallad, Song.ArmysPaeon })
        {
            var selected = song == candidate;
            if (ImGui.Selectable(SongDisplayName(candidate), selected))
            {
                song = candidate;
                changed = true;
            }

            if (selected)
                ImGui.SetItemDefaultFocus();
        }

        ImGui.EndCombo();
        return changed;
    }

    private static void DrawOpenerSettings()
    {
        var settings = BardSettings.Instance;

        var usePotionInOpener = settings.UsePotionInOpener;
        if (ImGui.Checkbox("起手吃爆发药", ref usePotionInOpener))
            settings.UsePotionInOpener = usePotionInOpener;

        var opener = settings.Opener;
        if (ImGui.BeginCombo("起手选择", OpenerDisplayName(opener)))
        {
            if (ImGui.Selectable("90-100级 3G团辅起手", opener == 0))
                settings.Opener = 0;
            if (ImGui.Selectable("90-100级 2G团辅起手", opener == 1))
                settings.Opener = 1;

            ImGui.EndCombo();
        }
    }

    private static string OpenerDisplayName(int opener)
    {
        return opener switch
        {
            1 => "90-100级 2G团辅起手",
            _ => "90-100级 3G团辅起手"
        };
    }

    private static string SongDisplayName(Song song)
    {
        return song switch
        {
            Song.WanderersMinuet => "旅神",
            Song.MagesBallad => "贤者",
            Song.ArmysPaeon => "军神",
            _ => "无"
        };
    }
}
