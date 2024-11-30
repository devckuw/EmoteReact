using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Hooking;
using EmoteReact;

namespace SamplePlugin
{
    public class EmoteReaderHooks : IDisposable
    {
        public Action<IPlayerCharacter, ushort>? OnEmote;

        public delegate void OnEmoteFuncDelegate(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong unk2);
        private readonly Hook<OnEmoteFuncDelegate>? hookEmote;

        public bool IsValid = false;
        Plugin plugin;

        public EmoteReaderHooks(Plugin p)
        {
            plugin = p;
            try
            {
                hookEmote = Plugin.InteropSigScanner.HookFromSignature<OnEmoteFuncDelegate>("E8 ?? ?? ?? ?? 48 8D 8B ?? ?? ?? ?? 4C 89 74 24", OnEmoteDetour);
                hookEmote.Enable();

                IsValid = true;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error(ex, "failed to hook emotes!");
            }
        }

        public void Dispose()
        {
            hookEmote?.Dispose();
            IsValid = false;
        }

        void OnEmoteDetour(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong unk2)
        {
            // unk - some field of event framework singleton? doesn't matter here anyway
            Plugin.Log.Info($"Emote >> unk:{unk:X}, instigatorAddr:{instigatorAddr:X}, emoteId:{emoteId}, targetId:{targetId:X}, unk2:{unk2:X}");

            if (Plugin.ClientState.LocalPlayer != null)
            {
                if (targetId == Plugin.ClientState.LocalPlayer.GameObjectId)
                {
                    var instigatorOb = Plugin.ObjectTable.FirstOrDefault(x => (ulong)x.Address == instigatorAddr) as IPlayerCharacter;
                    if (instigatorOb != null)
                    {
                        bool canCount = (instigatorOb.EntityId != targetId);

                        if (canCount)
                        {
                            Plugin.Log.Info($"on me {instigatorOb.ObjectIndex} {instigatorOb.EntityId:X} {instigatorOb.Name} {instigatorOb.HomeWorld.Value.Name}");
                            Plugin.Log.Debug("emote handler ?");
                            plugin.emoteHandler.ProcessEmote(emoteId, $"{instigatorOb.Name} {instigatorOb.HomeWorld.Value.Name}");
                        }
                        else
                        {
                            Plugin.Log.Info($"by me {instigatorOb.ObjectIndex} {instigatorOb.EntityId:X} {instigatorOb.Name} {instigatorOb.HomeWorld.Value.Name}");
                        }
                    }
                }
            }

            hookEmote?.Original(unk, instigatorAddr, emoteId, targetId, unk2);
        }
    }
}
