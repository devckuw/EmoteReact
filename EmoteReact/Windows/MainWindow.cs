using System;
using System.Numerics;
using Dalamud.Interface.Components;
using Dalamud.Interface;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System.Collections.Generic;
using Lumina.Excel.Sheets;
using FFXIVClientStructs.FFXIV.Common.Lua;

namespace EmoteReact.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin plugin;
    private string inputName = "";
    private string inputReaction = "";
    private string inputWorld = "World";
    private string inputEmote = "Emote";
    private int inputEmoteInt = -1;
    private List<string> worlds = new List<string>();
    private List<(int, string)> emotes = new List<(int, string)>();
    private float[] sizes;

    private List<(string, int)> toRemove = new List<(string, int)>();
    private int num = 0;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("Emote React##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(740, 500),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
        foreach (var line in Plugin.DataManager.GameData.GetExcelSheet<World>())
        {
            if (line.IsPublic)
                worlds.Add(line.Name.ToString());
        }

        foreach (var emote in Plugin.DataManager.GameData.GetExcelSheet<Emote>())
        {
            if (!emote.Name.IsEmpty)
                emotes.Add(((int)emote.RowId,emote.Name.ToString()));
        }

    }

    public void Dispose() { }

    public override void Draw()
    {
        sizes = [
            100 + (70 * ImGuiHelpers.GlobalScale),
            100 + (10 * ImGuiHelpers.GlobalScale),
            100 + (26 * ImGuiHelpers.GlobalScale),
            ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X - (100 + 100 + 100 + 50 + 40) - ((70 + 10 + 26) * ImGuiHelpers.GlobalScale),
            50,
            40
        ];

        // name server emote raction
        ImGui.Columns(6);
        for (int i = 0; i < 6; i++)
        {
            ImGui.SetColumnWidth(i, sizes[i]);
        }

        ImGui.Separator();

        DrawHeader();


        foreach (var character in plugin.emoteHandler.data)
        {
            string name = character.Key.Split(" ")[0] + " " + character.Key.Split(" ")[1];
            string world = character.Key.Split(" ")[2];
            foreach (var em in character.Value)
            {
                DrawLine(name, world, em.Key, em.Value.command);
            }
        }
        num = 0;

        DrawAddEntry();

        if (toRemove.Count != 0)
        {
            foreach (var r in  toRemove)
            {
                Plugin.Log.Debug($"remove : {r.Item1} {r.Item2}");
                plugin.emoteHandler.RemoveEntry(r.Item1, r.Item2);
            }
            toRemove.Clear();
        }
    }

    private void DrawHeader()
    {
        ImGui.TextUnformatted("Name");
        ImGui.NextColumn();
        ImGui.TextUnformatted("Server");
        ImGui.NextColumn();
        ImGui.TextUnformatted("Emote");
        ImGui.NextColumn();
        ImGui.TextUnformatted("Reaction");
        ImGui.NextColumn();
        ImGui.TextUnformatted("Enable");
        ImGui.NextColumn();
        ImGui.TextUnformatted("");
        ImGui.NextColumn();
        ImGui.Separator();
    }

    private void DrawLine(string name, string server, int emote, string reaction)
    {
        ImGui.TextUnformatted(name);
        ImGui.NextColumn();

        ImGui.TextUnformatted(server);
        ImGui.NextColumn();

        ImGui.TextUnformatted(plugin.emoteHandler.emotes[emote].ToString());
        ImGui.NextColumn();

        ImGui.TextUnformatted(reaction);
        ImGui.NextColumn();

        if (ImGui.Checkbox($"##{name}{server}{emote}", ref plugin.emoteHandler.data[$"{name} {server}"][emote].isEnable))
        {

        }
        ImGui.NextColumn();

        if (ImGuiComponents.IconButton(num++, FontAwesomeIcon.Trash))
        {
            Plugin.Log.Debug($"add to remove : {name} {server} {emote}");
            toRemove.Add(($"{name} {server}", emote));
        }
        ImGui.NextColumn();
        ImGui.Separator();
    }

    public void DrawAddEntry()
    {
        ImGui.SetNextItemWidth(sizes[0] - 15);
        ImGui.InputTextWithHint("##nameinput", "Name LastName", ref inputName, 200);
        ImGui.NextColumn();

        ImGui.SetNextItemWidth(sizes[1] - 15);
        if (ImGui.BeginCombo("##worldinput", inputWorld))
        {
            foreach (var w in worlds)
            {
                if (ImGui.Selectable(w))
                {
                    inputWorld = w;
                }
            }
            ImGui.EndCombo();
        }
        ImGui.NextColumn();

        ImGui.SetNextItemWidth(sizes[2] - 15);
        if (ImGui.BeginCombo("##emoteinput", inputEmote))
        {
            foreach (var e in emotes)
            {
                if (ImGui.Selectable(e.Item2))
                {
                    inputEmote = e.Item2;
                    inputEmoteInt = e.Item1;
                }
            }
            ImGui.EndCombo();
        }
        ImGui.NextColumn();

        ImGui.SetNextItemWidth(sizes[3] - 15);
        ImGui.InputTextWithHint("##reactinput", "/Command to execute", ref inputReaction, 200);
        ImGui.NextColumn();

        ImGui.NextColumn();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Plus))
        {
            if (inputName.Split(" ").Length == 2 && inputWorld != "World" && inputEmote != "Emote" && inputReaction != string.Empty)
            {
                plugin.emoteHandler.AddEntry($"{inputName} {inputWorld}", inputEmoteInt, inputReaction, 0);
                inputReaction = "";
                inputEmote = "Emote";
                inputEmoteInt = -1;
            }
        }
        ImGui.NextColumn();
    }

}
