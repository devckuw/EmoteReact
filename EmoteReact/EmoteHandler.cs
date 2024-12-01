using combatHelper.Utils;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoteReact
{
    public class Reaction
    {
        public string command;
        public int cd;
        public int lastProc;
        public bool isEnable;

        public Reaction(string command, int cd)
        {
            this.command = command;
            this.cd = Math.Max(0, cd);
            this.lastProc = 0;
            this.isEnable = true;
        }

        public void Toggle()
        {
            Plugin.Log.Debug($"{isEnable} => {!isEnable}");
            isEnable = !isEnable;
        }
    }

    public class EmoteHandler : IDisposable
    {
        // <char name, <emote number, reaction>>
        public Dictionary<string, Dictionary<int, Reaction>> data;
        public FrozenDictionary<int, Lumina.Text.ReadOnly.ReadOnlySeString> emotes;
        private List<string> cmds = new List<string>();
        Plugin plugin;

        public EmoteHandler(Plugin p)
        {
            this.plugin = p;
            data = p.Configuration.data;
            emotes = Plugin.DataManager.GameData.GetExcelSheet<Emote>().ToFrozenDictionary(e => (int)e.RowId, e => e.Name);
            //Plugin.Log.Debug(GetEmoteId("Pet").ToString());
            //AddEntry("Ke Win Ragnarok", 105, "/e test", 10);
            Plugin.Framework.Update += OnUpdate;
        }

        public void Dispose()
        {
            Plugin.Framework.Update -= OnUpdate;
        }

        private void OnUpdate(IFramework framework)
        {
            foreach (var s in cmds)
            {
                ChatHelper.SendChatMessage(s);
            }
            cmds.Clear();
        }

        public void AddEntry(string name, int emote, string command, int cd)
        {
            if (!data.ContainsKey(name))
            {
                data.Add(name, new Dictionary<int, Reaction>());
            }
            data[name].TryAdd(emote, new Reaction(command, cd));
            plugin.Configuration.data = data;
            plugin.Configuration.Save();
        }

        public void RemoveEntry(string name, int emote)
        {
            if (data.ContainsKey(name))
            {
                data[name].Remove(emote);
                plugin.Configuration.data = data;
                plugin.Configuration.Save();
            }
        }

        public int GetEmoteId(string name)
        {
            foreach (var e in emotes)
            {
                if (e.Value == name)
                    return e.Key;
            }

            return -1;
        }

        public void ProcessEmote(int emoteId, string charName)
        {
            if (!data.ContainsKey(charName))
                return;
            if (!data[charName].ContainsKey(emoteId))
                return;
            cmds.Clear();
            if (data[charName][emoteId].isEnable)
                cmds.Add(data[charName][emoteId].command);
        }

    }
}
