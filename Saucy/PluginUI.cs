using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;
using FFTriadBuddy;
using FFXIVClientStructs.FFXIV.Client.Game.GoldSaucer;
using PunishLib.ImGuiMethods;
using Saucy.CuffACur;
using Saucy.OtherGames;
using Saucy.TripleTriad;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using TriadBuddyPlugin;

namespace Saucy;

// It is good to have this be disposable in general, in case you ever need it
// to do any cleanup
public unsafe class PluginUI : Window
{
    public PluginUI() : base("Saucy##Saucy")
    {
        Size = new Vector2(520, 420);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public GameNpcInfo CurrentNPC
    {
        get;
        set
        {
            if (field != value)
            {
                TriadAutomater.TempCardsWonList.Clear();
                field = value;
            }
        }
    }

    public bool Enabled { get; set; } = false;

    public override void Draw()
    {
        ImGuiEx.EzTabBar("###Games",
            ("重擊伽美什", DrawCufTab, null, false),
            ("九宮幻卡", DrawTriadTab, null, false),
            ("孤樹無援", () =>
            {
                ImGuiEx.EzTabBar("Out on a Limb",
                    ("主要", P.LimbManager.DrawSettings, null, false),
                    ("偵錯", P.LimbManager.DrawDebug, null, false));
            }, null, false),
            ("其他遊戲", DrawOtherGamesTab, null, false),
            ("統計", DrawStatsTab, null, false),
            ("關於", () => AboutTab.Draw("Saucy"), null, false)
#if DEBUG
            , ("Debug", DrawDebugTab, null, false)
#endif
            );
    }

    private void DrawOtherGamesTab()
    {
        //ImGui.Checkbox("Enable Air Force One Module", ref AirForceOneModule.ModuleEnabled);

        if (ImGui.Checkbox("啟用「必中一閃快刀斬魔」模組", ref C.SliceIsRightModuleEnabled))
        {
            if (C.SliceIsRightModuleEnabled)
                C.EnabledModules.Add(ModuleManager.GetModule<SliceIsRight>().InternalName);
            else
                C.EnabledModules.Remove(ModuleManager.GetModule<SliceIsRight>().InternalName);
        }

        if (ImGui.Checkbox("啟用自動仙人微彩", ref C.EnableAutoMiniCactpot))
        {
            if (C.EnableAutoMiniCactpot)
                C.EnabledModules.Add(ModuleManager.GetModule<MiniCactpot.MiniCactpot>().InternalName);
            else
                C.EnabledModules.Remove(ModuleManager.GetModule<MiniCactpot.MiniCactpot>().InternalName);
        }

        if (ImGui.Checkbox("啟用「暴風倖存者」模組", ref C.AnyWayTheWindowBlowsModuleEnabled))
        {
            if (C.AnyWayTheWindowBlowsModuleEnabled)
                C.EnabledModules.Add(ModuleManager.GetModule<AnyWayTheWindBlows>().InternalName);
            else
                C.EnabledModules.Remove(ModuleManager.GetModule<AnyWayTheWindBlows>().InternalName);
        }
    }

    private void DrawStatsTab()
    {
        if (ImGui.BeginTabBar("Stats"))
        {
            if (ImGui.BeginTabItem("累計"))
            {
                DrawStatsTab(C.Stats, out var reset);

                if (reset)
                {
                    C.Stats = new();
                    C.Save();
                }

                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("本次啟動"))
            {
                DrawStatsTab(C.SessionStats, out var reset);
                if (reset)
                    C.SessionStats = new();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DrawStatsTab(Stats stat, out bool reset)
    {
        if (ImGui.BeginTabBar("Games"))
        {
            if (ImGui.BeginTabItem("重擊伽美什"))
            {
                DrawCuffStats(stat);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("九宮幻卡"))
            {
                DrawTTStats(stat);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("孤樹無援"))
            {
                DrawLimbStats(stat);
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        reset = ImGui.Button("重設統計（按住 Ctrl）", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y)) && ImGui.GetIO().KeyCtrl;
    }

    private void DrawLimbStats(Stats stat)
    {
        ImGui.BeginChild("Limb Stats", new Vector2(0, ImGui.GetContentRegionAvail().Y - 30f), true);
        ImGui.Columns(3, default, false);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText(ImGuiColors.DalamudRed, "孤樹無援", true);
        ImGuiHelpers.ScaledDummy(10f);
        ImGui.Columns(2, default, false);
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("遊玩次數", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("獲得金碟幣", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.LimbGamesPlayed:N0}");
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.LimbMGP:N0}");

        ImGui.EndChild();
    }

    private void DrawCuffStats(Stats stat)
    {
        ImGui.BeginChild("Cuff Stats", new Vector2(0, ImGui.GetContentRegionAvail().Y - 30f), true);
        ImGui.Columns(3, default, false);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText(ImGuiColors.DalamudRed, "重擊伽美什", true);
        ImGuiHelpers.ScaledDummy(10f);
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("遊玩次數", true);
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.CuffGamesPlayed:N0}");
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.Spacing();
        ImGuiEx.CenterColumnText("重擊！", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("痛擊！！", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("猛擊！！！！", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.CuffBruisings:N0}");
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.CuffPunishings:N0}");
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.CuffBrutals:N0}");
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("獲得金碟幣", true);
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.CuffMGP:N0}");

        ImGui.EndChild();
    }

    private void DrawTTStats(Stats stat)
    {
        ImGui.BeginChild("TT Stats", new Vector2(0, ImGui.GetContentRegionAvail().Y - 30f), true);
        ImGui.Columns(3, default, false);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText(ImGuiColors.DalamudRed, "九宮幻卡", true);
        ImGuiHelpers.ScaledDummy(10f);
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("對局次數", true);
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.GamesPlayedWithSaucy:N0}");
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.Spacing();
        ImGuiEx.CenterColumnText("勝利", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("落敗", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("平手", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.GamesWonWithSaucy:N0}");
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.GamesLostWithSaucy:N0}");
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.GamesDrawnWithSaucy:N0}");
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("勝率", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("獲得幻卡", true);
        ImGui.NextColumn();
        if (stat.NPCsPlayed.Count > 0)
        {
            ImGuiEx.CenterColumnText("對局最多的 NPC", true);
            ImGui.NextColumn();
        }
        else
        {
            ImGui.NextColumn();
        }

        if (stat.GamesPlayedWithSaucy > 0)
        {
            ImGuiEx.CenterColumnText($"{Math.Round((stat.GamesWonWithSaucy / (double)stat.GamesPlayedWithSaucy) * 100, 2)}%");
        }
        else
        {
            ImGuiEx.CenterColumnText("");
        }
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.CardsDroppedWithSaucy:N0}");
        ImGui.NextColumn();

        if (stat.NPCsPlayed.Count > 0)
        {
            ImGuiEx.CenterColumnText($"{stat.NPCsPlayed.OrderByDescending(x => x.Value).First().Key}");
            ImGuiEx.CenterColumnText($"{stat.NPCsPlayed.OrderByDescending(x => x.Value).First().Value:N0} 次");
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
        }

        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("獲得金碟幣", true);
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText("幻卡出售總值", true);
        ImGui.NextColumn();
        if (stat.CardsWon.Count > 0)
        {
            ImGuiEx.CenterColumnText("獲得最多的幻卡", true);
        }
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{stat.MGPWon:N0} MGP");
        ImGui.NextColumn();
        ImGuiEx.CenterColumnText($"{GetDroppedCardValues(stat):N0} MGP");
        ImGui.NextColumn();
        if (stat.CardsWon.Count > 0)
        {
            ImGuiEx.CenterColumnText($"{TriadCardDB.Get().FindById((int)stat.CardsWon.OrderByDescending(x => x.Value).First().Key).Name.GetLocalized()}");
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.CardsWon.OrderByDescending(x => x.Value).First().Value:N0} 次");
        }

        ImGui.Columns(1);
        ImGui.EndChild();
    }

    private int GetDroppedCardValues(Stats stat)
    {
        var output = 0;
        foreach (var card in stat.CardsWon)
            output += GameCardDB.Get().FindById((int)card.Key).SaleValue * stat.CardsWon[card.Key];

        return output;
    }

    public void DrawTriadTab()
    {
        var enabled = TriadAutomater.ModuleEnabled;

        ImGui.TextWrapped("使用方式：向想要對局的 NPC 發起九宮幻卡挑戰。進入挑戰畫面後，勾選「啟用九宮幻卡模組」。");
        ImGui.Separator();

        if (ImGui.Checkbox("啟用九宮幻卡模組", ref enabled))
        {
            TriadAutomater.ModuleEnabled = enabled;

            if (enabled)
                CufModule.ModuleEnabled = false;
        }

        var autoOpen = C.OpenAutomatically;

        if (ImGui.Checkbox("挑戰 NPC 時自動開啟 Saucy", ref autoOpen))
        {
            C.OpenAutomatically = autoOpen;
            C.Save();
        }

        var selectedDeck = C.SelectedDeckIndex;

        if (Saucy.TTSolver.profileGS.GetPlayerDecks().Count() > 0)
        {
            var useAutoDeck = C.UseRecommendedDeck;
            if (ImGui.Checkbox("自動選擇勝率最高的牌組", ref useAutoDeck))
            {
                C.UseRecommendedDeck = useAutoDeck;
                C.Save();
            }

            if (!C.UseRecommendedDeck)
            {
                ImGui.PushItemWidth(200);
                string preview;
                if (selectedDeck == -1 || Saucy.TTSolver.profileGS.GetPlayerDecks()[selectedDeck] is null)
                {
                    preview = "";
                }
                else
                {
                    preview = selectedDeck >= 0 ? Saucy.TTSolver.profileGS.GetPlayerDecks()[selectedDeck].name : string.Empty;
                }

                if (ImGui.BeginCombo("選擇牌組", preview))
                {
                    if (ImGui.Selectable(""))
                    {
                        C.SelectedDeckIndex = -1;
                    }

                    foreach (var deck in Saucy.TTSolver.profileGS.GetPlayerDecks())
                    {
                        if (deck is null) continue;
                        var index = deck.id;
                        //var index = Saucy.TTSolver.preGameDecks.Where(x => x.Value == deck).First().Key;
                        if (ImGui.Selectable(deck.name, index == selectedDeck))
                        {
                            C.SelectedDeckIndex = index;
                            C.Save();
                        }
                    }

                    ImGui.EndCombo();
                }
            }
        }
        else
        {
            ImGui.TextWrapped("請先向 NPC 發起挑戰，以讀取你的牌組清單。");
        }

        if (ImGui.Checkbox("遊玩指定次數", ref TriadAutomater.PlayXTimes) && (TriadAutomater.NumberOfTimes <= 0 || TriadAutomater.PlayUntilCardDrops || TriadAutomater.PlayUntilAllCardsDropOnce))
        {
            TriadAutomater.NumberOfTimes = 1;
            TriadAutomater.PlayUntilCardDrops = false;
            TriadAutomater.PlayUntilAllCardsDropOnce = false;
        }

        if (ImGui.Checkbox("遊玩至獲得任一張幻卡", ref TriadAutomater.PlayUntilCardDrops) && (TriadAutomater.NumberOfTimes <= 0 || TriadAutomater.PlayXTimes || TriadAutomater.PlayUntilAllCardsDropOnce))
        {
            TriadAutomater.NumberOfTimes = 1;
            TriadAutomater.PlayXTimes = false;
            TriadAutomater.PlayUntilAllCardsDropOnce = false;
        }

        if (GameNpcDB.Get().mapNpcs.TryGetValue(Saucy.TTSolver.preGameNpc?.Id ?? -1, out var npcInfo))
        {
            CurrentNPC = npcInfo;
        }
        else
        {
            CurrentNPC = null;
        }

        if (ImGui.Checkbox($"遊玩至 NPC 的每張幻卡至少各獲得指定次數 {(CurrentNPC is null ? "" : $"({TriadNpcDB.Get().FindByID(CurrentNPC.npcId).Name.GetLocalized()})")}", ref TriadAutomater.PlayUntilAllCardsDropOnce))
        {
            TriadAutomater.TempCardsWonList.Clear();
            TriadAutomater.PlayUntilCardDrops = false;
            TriadAutomater.PlayXTimes = false;
            TriadAutomater.NumberOfTimes = 1;
        }

        var onlyUnobtained = C.OnlyUnobtainedCards;

        if (TriadAutomater.PlayUntilAllCardsDropOnce)
        {
            ImGui.SameLine();
            if (ImGui.Checkbox("僅計算尚未擁有的幻卡", ref onlyUnobtained))
            {
                TriadAutomater.TempCardsWonList.Clear();
                C.OnlyUnobtainedCards = onlyUnobtained;
                C.Save();
            }
        }

        if (TriadAutomater.PlayUntilAllCardsDropOnce && CurrentNPC != null)
        {
            ImGui.Indent();
            GameCardDB.Get().Refresh();
            foreach (var card in CurrentNPC.rewardCards)
            {
                if ((C.OnlyUnobtainedCards && !GameCardDB.Get().FindById(card).IsOwned) || !C.OnlyUnobtainedCards)
                {
                    TriadAutomater.TempCardsWonList.TryAdd((uint)card, 0);
                    ImGui.Text($"- {TriadCardDB.Get().FindById(GameCardDB.Get().FindById(card).CardId).Name.GetLocalized()} {TriadAutomater.TempCardsWonList[(uint)card]}/{TriadAutomater.NumberOfTimes}");
                }
            }

            if (C.OnlyUnobtainedCards && TriadAutomater.TempCardsWonList.Count == 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
                ImGui.TextWrapped("你已擁有此 NPC 的所有幻卡。請取消勾選「僅計算尚未擁有的幻卡」，或選擇其他 NPC。");
                ImGui.PopStyleColor();
            }
            ImGui.Unindent();
        }

        if (TriadAutomater.PlayXTimes || TriadAutomater.PlayUntilCardDrops || TriadAutomater.PlayUntilAllCardsDropOnce)
        {
            ImGui.PushItemWidth(150f);
            ImGui.Text("次數：");
            ImGui.SameLine();

            if (ImGui.InputInt("###NumberOfTimes", ref TriadAutomater.NumberOfTimes))
            {
                if (TriadAutomater.NumberOfTimes <= 0)
                    TriadAutomater.NumberOfTimes = 1;
            }

            ImGui.Checkbox("完成後登出", ref TriadAutomater.LogOutAfterCompletion);

            var playSound = C.PlaySound;

            ImGui.Columns(2, default, false);
            if (ImGui.Checkbox("完成時播放音效", ref playSound))
            {
                C.PlaySound = playSound;
                C.Save();
            }

            if (playSound)
            {
                ImGui.NextColumn();
                ImGui.Text("選擇音效");
                if (ImGui.BeginCombo("###SelectSound", C.SelectedSound))
                {
                    var path = Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds");
                    foreach (var file in new DirectoryInfo(path).GetFiles())
                    {
                        if (ImGui.Selectable($"{Path.GetFileNameWithoutExtension(file.FullName)}", C.SelectedSound == Path.GetFileNameWithoutExtension(file.FullName)))
                        {
                            C.SelectedSound = Path.GetFileNameWithoutExtension(file.FullName);
                            C.Save();
                        }
                    }

                    ImGui.EndCombo();
                }

                if (ImGui.Button("開啟音效資料夾"))
                {
                    Process.Start("explorer.exe", @$"{Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds")}");
                }
                ImGuiComponents.HelpMarker("將 MP3 檔案放入音效資料夾，即可加入自訂音效。");
            }
            ImGui.Columns(1);
        }
    }

    public unsafe void DrawCufTab()
    {
        var enabled = CufModule.ModuleEnabled;

        ImGui.TextWrapped("使用方式：勾選「啟用重擊伽美什模組」，再走到重擊伽美什機台前。");
        ImGui.Separator();

        if (ImGui.Checkbox("啟用重擊伽美什模組", ref enabled))
        {
            CufModule.ModuleEnabled = enabled;
            if (enabled && TriadAutomater.ModuleEnabled)
                TriadAutomater.ModuleEnabled = false;
        }

        if (ImGui.Checkbox("遊玩指定次數", ref TriadAutomater.PlayXTimes) && TriadAutomater.NumberOfTimes <= 0)
        {
            TriadAutomater.NumberOfTimes = 1;
        }

        if (TriadAutomater.PlayXTimes)
        {
            ImGui.PushItemWidth(150f);
            ImGui.Text("次數：");
            ImGui.SameLine();

            if (ImGui.InputInt("###NumberOfTimes", ref TriadAutomater.NumberOfTimes))
            {
                if (TriadAutomater.NumberOfTimes <= 0)
                    TriadAutomater.NumberOfTimes = 1;
            }

            ImGui.Checkbox("完成後登出", ref TriadAutomater.LogOutAfterCompletion);

            var playSound = C.PlaySound;

            ImGui.Columns(2, default, false);
            if (ImGui.Checkbox("完成時播放音效", ref playSound))
            {
                C.PlaySound = playSound;
                C.Save();
            }

            if (playSound)
            {
                ImGui.NextColumn();
                ImGui.Text("選擇音效");
                if (ImGui.BeginCombo("###SelectSound", C.SelectedSound))
                {
                    var path = Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds");
                    foreach (var file in new DirectoryInfo(path).GetFiles())
                    {
                        if (ImGui.Selectable($"{Path.GetFileNameWithoutExtension(file.FullName)}", C.SelectedSound == Path.GetFileNameWithoutExtension(file.FullName)))
                        {
                            C.SelectedSound = Path.GetFileNameWithoutExtension(file.FullName);
                            C.Save();
                        }
                    }

                    ImGui.EndCombo();
                }

                if (ImGui.Button("開啟音效資料夾"))
                {
                    Process.Start("explorer.exe", @$"{Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds")}");
                }
                ImGuiComponents.HelpMarker("將 MP3 檔案放入音效資料夾，即可加入自訂音效。");
            }
            ImGui.Columns(1);
        }
    }

    private void DrawDebugTab()
    {
        if (GoldSaucerManager.Instance() != null && GoldSaucerManager.Instance()->CurrentGFateDirector != null)
        {
            var dir = GoldSaucerManager.Instance()->CurrentGFateDirector;
            ImGui.Text($"GateType: {dir->GateType}");
            ImGui.Text($"GatePositionType: {dir->GatePositionType}");
            ImGui.Text($"Flags: {dir->Flags}");
            ImGui.Text($"IsRunningGate: {dir->IsRunningGate()}");
            ImGui.Text($"IsAcceptingGate: {dir->IsAcceptingGate()}");
        }
    }
}
