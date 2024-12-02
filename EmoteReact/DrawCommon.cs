using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoteReact
{
    internal class DrawCommon
    {
        public static void DropDownFilter(string label, string hint, ref string input, List<string> autocomplete, int maxDisplay = 10)
        {
            bool is_input_text_enter_pressed = ImGui.InputTextWithHint($"##input{label}", hint, ref input, 32, ImGuiInputTextFlags.EnterReturnsTrue);
            bool is_input_text_active = ImGui.IsItemActive();
            bool is_input_text_activated = ImGui.IsItemActivated();

            List<string> filtered = new List<string>();
            for (int i = 0; i < autocomplete.Count; i++)
            {
                if (!autocomplete[i].Contains(input, StringComparison.OrdinalIgnoreCase) && input != string.Empty)
                    continue;
                filtered.Add(autocomplete[i]);
            }

            if (is_input_text_activated)
                ImGui.OpenPopup($"##popup{label}");

            {
                ImGui.SetNextWindowPos(new Vector2(ImGui.GetItemRectMin().X, ImGui.GetItemRectMax().Y));
                ImGui.SetNextWindowSize(new Vector2(ImGui.GetItemRectSize().X, Math.Min(22 * filtered.Count + 5, 22 * maxDisplay + 5)));
                if (ImGui.BeginPopup($"##popup{label}", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.ChildWindow))// | ImGuiWindowFlags.AlwaysVerticalScrollbar))
                {
                    foreach (var item in filtered)
                    {
                        if (ImGui.Selectable(item))
                        {
                            input = item;
                        }
                    }

                    if (is_input_text_enter_pressed || (!is_input_text_active && !ImGui.IsWindowFocused()))
                        ImGui.CloseCurrentPopup();

                    ImGui.EndPopup();
                }
            }
        }
        public static void DropDownFilterEmote(string label, string hint, ref string input, ref int inputInt, List<(int,string)> autocomplete, int maxDisplay = 10)
        {
            bool is_input_text_enter_pressed = ImGui.InputTextWithHint($"##input{label}", hint, ref input, 32, ImGuiInputTextFlags.EnterReturnsTrue);
            bool is_input_text_active = ImGui.IsItemActive();
            bool is_input_text_activated = ImGui.IsItemActivated();

            List<(int, string)> filtered = new List<(int, string)>();
            for (int i = 0; i < autocomplete.Count; i++)
            {
                if (!autocomplete[i].Item2.Contains(input, StringComparison.OrdinalIgnoreCase) && input != string.Empty)
                    continue;
                filtered.Add(autocomplete[i]);
            }

            if (is_input_text_activated)
                ImGui.OpenPopup($"##popup{label}");

            {
                ImGui.SetNextWindowPos(new Vector2(ImGui.GetItemRectMin().X, ImGui.GetItemRectMax().Y));
                ImGui.SetNextWindowSize(new Vector2(ImGui.GetItemRectSize().X, Math.Min(22 * filtered.Count + 5, 22 * maxDisplay + 5)));
                if (ImGui.BeginPopup($"##popup{label}", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.ChildWindow))// | ImGuiWindowFlags.AlwaysVerticalScrollbar))
                {
                    foreach (var item in filtered)
                    {
                        if (ImGui.Selectable(item.Item2))
                        {
                            input = item.Item2;
                            inputInt = item.Item1;
                        }
                    }

                    if (is_input_text_enter_pressed || (!is_input_text_active && !ImGui.IsWindowFocused()))
                        ImGui.CloseCurrentPopup();

                    ImGui.EndPopup();
                }
            }
        }
    }
}
