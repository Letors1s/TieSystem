using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YamlDotNet.Core.Tokens;

namespace TieSystem2.Commands.Client
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Frisk : ICommand
    {
        public string Command => "frisk";

        public string[] Aliases => new string[] { "fr" };

        public string Description => "Обыскивает игрока / труп напротив.";

        public bool Execute(ArraySegment<string> ar, ICommandSender sender, out string r)
        {
            Plugin pl = Plugin.pl;
            Player ev = Player.Get(sender);

            Player tar = Plugin.GetLookedAtPlayer(ev, 2.5f);
            Ragdoll rag = Plugin.GetLookedAtRagdoll(ev, 2.5f);

            if (ev == null || !ev.IsAlive || ev.IsScp)
            {
                r = "Только люди могут использовать эту комманду!";
                return false;
            }

            if(tar != null && tar.IsAlive && !tar.IsScp)
            {
                Timing.RunCoroutine(FriskTar(ev, tar));
                r = "Вы успешно обыскали игрока.";
                return true;
            }
            if (rag != null)
            {
                Timing.RunCoroutine(FriskRag(ev, rag));
                r = "Вы успешно обыскали трупа.";
                return true;
            }

            r = "Вы не смотрите ни на игрка, ни на труп.";
            return false;
        }

        private IEnumerator<float> FriskTar(Player ev, Player tar)
        {
            Plugin pl = Plugin.pl;

            float left = 0;

            while(pl.Config.FriskTime > left)
            {
                if (Vector3.Distance(ev.Position, tar.Position) > 3) yield break;

                if (ev == null || !ev.IsAlive || ev.IsScp || tar == null || !tar.IsAlive || tar.IsScp) yield break;

                ev.ShowHint($"Вы обыскиваете {tar.DisplayNickname}, осталось <color=green>{Math.Round(pl.Config.FriskTime - left, 1)}</color> секунд.", 0.2f);

                left += 0.1f;

                yield return Timing.WaitForSeconds(0.1f);
            }

            Vector3 itemSpawnPos = tar.Position + Vector3.up * 0.7f;

            if (pl.PlayerItemList.ContainsKey(tar) || pl.PlayerAmmoList.ContainsKey(tar))
            {
                if(pl.PlayerItemList.ContainsKey(tar))
                {
                    if(pl.PlayerItemList.TryGetValue(tar, out List<ItemType> items))
                    {
                        foreach (ItemType item in items)
                        {
                            Exiled.API.Features.Items.Item.Create(item).CreatePickup(itemSpawnPos);
                        }
                        pl.PlayerItemList.Remove(tar);
                    }
                }

                if (pl.PlayerAmmoList.ContainsKey(tar))
                {
                    if (pl.PlayerAmmoList.TryGetValue(tar, out Dictionary<AmmoType, ushort> ammos))
                    {
                        foreach (var ammo in ammos)
                        {
                            Exiled.API.Features.Items.Ammo.Create((ItemType)ammo.Key).CreatePickup(itemSpawnPos);
                        }
                        pl.PlayerAmmoList.Remove(tar);
                    }
                }

                if(pl.PlayerTieList.ContainsKey(tar))
                {
                    if(pl.PlayerTieList.ContainsKey(ev))
                    {
                        pl.PlayerTieList[ev] += pl.PlayerTieList[tar];
                        pl.PlayerTieList.Remove(tar);
                    }
                    else
                    {
                        pl.PlayerTieList.Add(ev, pl.PlayerTieList[tar]);
                        pl.PlayerTieList.Remove(tar);
                    }
                }
            }
            ev.ShowHint($"<color=green>Вы успешно обыскали{tar.DisplayNickname}!</color>");
        }

        private IEnumerator<float> FriskRag(Player ev, Ragdoll rag)
        {
            Plugin pl = Plugin.pl;

            float left = 0;

            while (pl.Config.FriskTime > left)
            {
                if (Vector3.Distance(ev.Position, rag.Position) > 3) yield break;

                if (ev == null || !ev.IsAlive || ev.IsScp || rag == null) yield break;

                ev.ShowHint($"Вы обыскиваете {rag.Name}, осталось <color=green>{Math.Round(pl.Config.FriskTime - left, 1)}</color> секунд.", 0.2f);

                left += 0.1f;

                yield return Timing.WaitForSeconds(0.1f);
            }

            Vector3 itemSpawnPos = rag.Position + Vector3.up * 0.7f;

            if (pl.RagdollItemList.ContainsKey(rag) || pl.RagdollAmmoList.ContainsKey(rag))
            {
                if (pl.RagdollItemList.ContainsKey(rag))
                {
                    if (pl.RagdollItemList.TryGetValue(rag, out List<ItemType> items))
                    {
                        foreach (ItemType item in items)
                        {
                            Exiled.API.Features.Items.Item.Create(item).CreatePickup(itemSpawnPos);
                        }
                        pl.RagdollItemList.Remove(rag);
                    }
                }

                if (pl.RagdollAmmoList.ContainsKey(rag))
                {
                    if (pl.RagdollAmmoList.TryGetValue(rag, out Dictionary<AmmoType, ushort> ammos))
                    {
                        foreach (var ammo in ammos)
                        {
                            Exiled.API.Features.Items.Ammo.Create((ItemType)ammo.Key).CreatePickup(itemSpawnPos);
                        }
                        pl.RagdollAmmoList.Remove(rag);
                    }
                }

                if (pl.RagdollTieList.ContainsKey(rag))
                {
                    if (pl.PlayerTieList.ContainsKey(ev))
                    {
                        pl.PlayerTieList[ev] += pl.RagdollTieList[rag];
                        pl.RagdollTieList.Remove(rag);
                    }
                    else
                    {
                        pl.PlayerTieList.Add(ev, pl.RagdollTieList[rag]);
                        pl.RagdollTieList.Remove(rag);
                    }
                }
            }
            ev.ShowHint($"<color=green>Вы успешно обыскали{rag.Name}!</color>");
        }
    }
}
