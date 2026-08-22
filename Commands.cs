using CommandSystem;
using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YamlDotNet.Core.Tokens;

namespace TieSystem
{
    public class Commands
    {
        [CommandHandler(typeof(ClientCommandHandler))]
        public class KnowTieCount : ICommand
        {
            public string Command { get; set; } = "tiecount";
            public string[] Aliases { get; set; } = new string[] { "tc" };
            public string Description { get; set; } = "Узнать кол-во стяжек, находящихся у вас.";

            public bool Execute(ArraySegment<string> ar, ICommandSender player, out string r)
            {
                Exiled.API.Features.Player ev = Exiled.API.Features.Player.Get(player);

                if (Plugin.Instance.PlayerTieCountList.TryGetValue(ev, out int count))
                {
                    r = $"У вас осталось {count} стяжек";
                    ev.ShowHint($"У вас осталось {count} стяжек");
                    return true;
                }

                r = $"Если вы видите это сообщение, сообщите об этом администрации проекта.";
                return false;
            }
        }

        [CommandHandler(typeof(RemoteAdminCommandHandler))]
        public class GiveTies : ICommand
        {
            public string Command { get; set; } = "giveties";
            public string[] Aliases { get; set; } = new string[] { "gties" };
            public string Description { get; set; } = "Выдаёт заданное кол-во стяжек выбранному игроку.";

            public bool Execute(ArraySegment<string> ar, ICommandSender player, out string r)
            {
                if (ar.Count < 2)
                {
                    r = "Ошибка! Используйте giveties [ID игрока] [Кол-во стяжек]";
                    return false;
                }

                Exiled.API.Features.Player tar = Exiled.API.Features.Player.Get(ar.At(0));

                if (tar == null)
                {
                    r = $"Игрок '{ar.At(0)}' не найден на сервере.";
                    return false;
                }

                if (!int.TryParse(ar.At(1), out int tiesAmount))
                {
                    r = "Количество стяжек должно быть целым числом!";
                    return false;
                }

                if (Plugin.Instance.PlayerTieCountList.ContainsKey(tar))
                {
                    Plugin.Instance.PlayerTieCountList[tar] += tiesAmount;
                }
                else
                {
                    Plugin.Instance.PlayerTieCountList.Add(tar, tiesAmount);
                }

                tar.ShowHint($"Администратор изменил вам количество стяжек. Теперь у вас: {tiesAmount}", 4f);

                r = $"Успешно выдано {tiesAmount} стяжек игроку {tar.Nickname} (ID: {tar.Id}).";
                return true;
            }
        }

        [CommandHandler(typeof(RemoteAdminCommandHandler))]
        public class KnowCountTieOfOtherPlayer : ICommand
        {
            public string Command { get; set; } = "tiecount";
            public string[] Aliases { get; set; } = new string[] { "tc" };
            public string Description { get; set; } = "Позволяет узнать кол-во стяжек любого игрока.";

            public bool Execute(ArraySegment<string> ar, ICommandSender player, out string r)
            {
                if (ar.Count < 1)
                {
                    r = "Ошибка! Используйте giveties [ID игрока]";
                    return false;
                }

                Exiled.API.Features.Player tar = Exiled.API.Features.Player.Get(ar.At(0));

                if (tar == null)
                {
                    r = $"Игрок '{ar.At(0)}' не найден на сервере.";
                    return false;
                }

                if (Plugin.Instance.PlayerTieCountList.TryGetValue(tar, out int tiesCount))
                {
                    r = $"У игрока {tar.Nickname} (ID: {tar.Id}) осталось {tiesCount} стяжек.";
                    return true;
                }
                else
                {
                    r = $"У игрока {tar.Nickname} (ID: {tar.Id}) сейчас 0 стяжек.";
                    return true;
                }
            }
        }

        [CommandHandler(typeof(ClientCommandHandler))]
        public class Uncuffing : ICommand
        {
            public string Command { get; set; } = "uncuff";
            public string[] Aliases { get; set; } = new string[] { "uc" };
            public string Description { get; set; } = "Начинет попытку развязывания.";

            public bool Execute(ArraySegment<string> ar, ICommandSender player, out string r)
            {
                Player ev = Player.Get(player);
                Plugin pl = Plugin.Instance;

                if (ev == null)
                {
                    r = "Эту команду может использовать только живой игрок!";
                    return false;
                }

                if (ev.IsCuffed)
                {
                    Timing.RunCoroutine(RunProgressTimer(ev));
                    r = "Вы пытаетесь снять стяжки.";
                    return true;
                }
                else
                {
                    r = "Вы не связаны.";
                    return false;
                }
            }

            private IEnumerator<float> RunProgressTimer(Player ev)
            {
                Plugin pl = Plugin.Instance;
                float UnCuffTime = pl.Config.UnTieTime;

                float leftTime = 0f;
                float HP = ev.Health;

                while (leftTime < UnCuffTime)
                {
                    if (ev == null || !ev.IsAlive || !ev.IsCuffed) yield break;

                    float NowHealt = ev.Health;

                    if (NowHealt < HP) yield break;

                    leftTime += 0.1f;

                    ev.ShowHint($"Вы пытаетесь развязаться, осталось <color=green>{Math.Round(UnCuffTime - leftTime, 1)}</color> секунд.", 0.2f);

                    yield return Timing.WaitForSeconds(0.1f);
                }

                if (ev != null && ev.IsAlive && ev.IsCuffed)
                {

                    int range = UnityEngine.Random.Range(0, 101);
                    if (range <= pl.Config.ChanceToSaveTiesOnUncuff)
                    {
                        if (Plugin.Instance.PlayerTieCountList.ContainsKey(ev))
                        {
                            if (Plugin.Instance.PlayerTieCountList.ContainsKey(ev))
                            {
                                Plugin.Instance.PlayerTieCountList[ev] += 1;
                            }
                            else
                            {
                                Plugin.Instance.PlayerTieCountList.Add(ev, 1);
                            }
                        }
                        ev.ShowHint($"<color=green>Вы сохранили стяжки при развязывании!</color>", 4f);
                    }
                    else
                    {

                        ev.ShowHint($"<color=green>Вы освободились, но стяжки сохранить не удалось.</color>", 4f);
                    }

                    ev.RemoveHandcuffs();
                }
            }
        }

        [CommandHandler(typeof(ClientCommandHandler))]
        public class GetTiesFromRagdoll : ICommand
        {
            public string Command { get; set; } = "friskties";
            public string[] Aliases { get; set; } = new string[] { "ft" };
            public string Description { get; set; } = "Начинет обыск трупа на наличие стяжек.";

            public bool Execute(ArraySegment<string> ar, ICommandSender player, out string r)
            {
                Player ev = Player.Get(player);
                Ragdoll tarRG = GetLookedAtRagdoll(ev, Plugin.Instance.Config.FriskDist);

                if(tarRG == null)
                {
                    r = "Вы не смотрите на труп.";
                    ev.ShowHint("<color=red>Вы не смотрите на труп.</color>");
                    return false;
                }

                if(Plugin.Instance.RagdollTieCountList.TryGetValue(tarRG, out int tieCount))
                {
                    r = "Поиск стяжек начат.";
                    Timing.RunCoroutine(FriskTies(ev, tarRG));
                    return true;
                }

                r = "У этого трупа нет стяжек.";
                return false;
            }

            private IEnumerator<float> FriskTies(Player ev, Ragdoll rag)
            {
                Plugin pl = Plugin.Instance;
                float FriskTieTime = pl.Config.FriskTieTime;

                float TimeLeft = 0f;

                while(FriskTieTime > TimeLeft)
                {
                    if (ev == null || !ev.IsAlive || ev.IsCuffed) yield break;

                    if (Vector3.Distance(rag.Position, ev.Position) > Plugin.Instance.Config.FriskDist)
                    {
                        ev.ShowHint("<color=red>Вы перестали обыскивать труп.</color>");
                        yield break;
                    }

                    TimeLeft += 0.1f;

                    ev.ShowHint($"Вы обыскиваете труп на наличие стяжек, осталось <color=green>{Math.Round(FriskTieTime - TimeLeft, 1)}</color> секунд.", 0.2f);

                    yield return Timing.WaitForSeconds(0.1f);
                }

                if (ev != null && ev.IsAlive && !ev.IsCuffed)
                {
                    if (!pl.RagdollTieCountList.TryGetValue(rag, out int tc))
                    {
                        pl.PlayerTieCountList.Add(ev, tc);
                    }
                    else
                    {
                        pl.PlayerTieCountList[ev] = tc + 1;
                    }

                    pl.RagdollTieCountList.Remove(rag);
                    ev.ShowHint($"<color=green>Вы успешно обыскали труп и забрали {tc} стяж(ек/ки)!</color>");
                }
            }
        }

        public static Ragdoll GetLookedAtRagdoll(Player player,float maxDistance)
        {
            var cam = player.CameraTransform;
            var hits = UnityEngine.Physics.RaycastAll(cam.position, cam.forward, maxDistance);
            foreach (var hit in hits)
            {
                var ragdollComponent = hit.collider.GetComponentInParent<PlayerRoles.Ragdolls.BasicRagdoll>();
                if (ragdollComponent != null)
                {
                    var target = Ragdoll.Get(ragdollComponent);
                    if (target != null)
                        return target;
                }
            }
            return null;
        }

        [CommandHandler(typeof(ClientCommandHandler))]
        public class FriskItemsFromPlayer : ICommand
        {
            public string Command { get; set; } = "frisk";
            public string[] Aliases { get; set; } = new string[] { "fr" };
            public string Description { get; set; } = "Начинет обыск игрока.";

            public bool Execute(ArraySegment<string> ar, ICommandSender player, out string r)
            {
                Player ev = Player.Get(player);
                Player tar = GetLookedAtPlayer(ev, Plugin.Instance.Config.FriskDist);

                Plugin pl = Plugin.Instance;

                if(tar == null)
                {
                    ev.ShowHint("Вы не смотрите на игрока.");
                    r = "Вы не смотрите на игрока.";
                    return false;
                }

                if(pl.PlayerItemsCountList.TryGetValue(tar, out List<ItemType> itemlist))
                {
                    Timing.RunCoroutine(FriskItemsPl(itemlist, ev, tar));
                    r = "Обыск начат.";
                    return true;
                }

                ev.ShowHint("Этого игрока нельзя обыскать.");
                r = "Этого игрока нельзя обыскать.";
                return false;
            }

            private IEnumerator<float> FriskItemsPl(List<ItemType> itemlist, Player ev, Player tar)
            {
                Plugin pl = Plugin.Instance;
                float FriskItemsTime = pl.Config.FriskItemsTime;

                Vector3 itemSpawnPos = tar.Position + Vector3.up * 0.7f;

                float TimeLeft = 0f;

                while(FriskItemsTime > TimeLeft)
                {
                    if (ev == null || !ev.IsAlive || ev.IsCuffed) yield break;

                    if (tar == null || !tar.IsAlive) yield break;

                    if (Vector3.Distance(tar.Position, ev.Position) > Plugin.Instance.Config.FriskDist)
                    {
                        ev.ShowHint("<color=red>Вы прекратили обыск.</color>");
                        yield break;
                    }

                    TimeLeft += 0.1f;

                    ev.ShowHint($"Вы обыскиваете {tar.DisplayNickname}, осталось <color=green>{Math.Round(FriskItemsTime - TimeLeft, 1)}</color> секунд.", 0.2f);

                    yield return Timing.WaitForSeconds(0.1f);
                }

                if (ev != null && ev.IsAlive && !ev.IsCuffed)
                {
                    foreach (ItemType item in itemlist)
                    {
                        Exiled.API.Features.Items.Item.Create(item).CreatePickup(itemSpawnPos);
                    }
                    ev.ShowHint($"<color=green>Вы успешно обыскали{tar.DisplayNickname}!</color>");
                    pl.PlayerItemsCountList.Remove(tar);
                }
            }
        }

        public static Player GetLookedAtPlayer(Player player, float maxDistance)
        {
            var cam = player.CameraTransform;
            var hits = Physics.RaycastAll(cam.position, cam.forward, maxDistance);
            foreach (var hit in hits)
            {
                var hub = hit.collider.GetComponentInParent<ReferenceHub>();
                if (hub != null)
                {
                    var target = Player.Get(hub);
                    if (target != null && target != player)
                        return target;
                }
            }
            return null;
        }

        [CommandHandler(typeof(ClientCommandHandler))]
        public class FriskItemsFromRagdoll : ICommand
        {
            public string Command { get; set; } = "friskbody";
            public string[] Aliases { get; set; } = new string[] { "fb" };
            public string Description { get; set; } = "Начинет обыск трупа.";

            public bool Execute(ArraySegment<string> ar, ICommandSender player, out string r)
            {
                Player ev = Player.Get(player);
                Ragdoll tar = GetLookedAtRagdoll(ev, Plugin.Instance.Config.FriskDist);

                Plugin pl = Plugin.Instance;

                if (tar == null)
                {
                    ev.ShowHint("Вы не смотрите на труп.");
                    r = "Вы не смотрите на труп.";
                    return false;
                }

                if (pl.RagdollItemsCountList.TryGetValue(tar, out List<ItemType> itemlist))
                {
                    Timing.RunCoroutine(FriskItemsRg(itemlist, ev, tar));
                    r = "Обыск начат.";
                    return true;
                }

                ev.ShowHint("Этот труп нельзя обыскать.");
                r = "Этот труп нельзя обыскать.";
                return false;
            }

            private IEnumerator<float> FriskItemsRg(List<ItemType> itemlist, Player ev, Ragdoll tar)
            {
                Plugin pl = Plugin.Instance;
                float FriskItemsTime = pl.Config.FriskItemsTime;

                Vector3 itemSpawnPos = tar.Position + Vector3.up * 0.7f;

                float TimeLeft = 0f;

                while (FriskItemsTime > TimeLeft)
                {
                    if (ev == null || !ev.IsAlive || ev.IsCuffed) yield break;

                    if (tar == null) yield break;

                    if (Vector3.Distance(tar.Position, ev.Position) > Plugin.Instance.Config.FriskDist)
                    {
                        ev.ShowHint("<color=red>Вы прекратили обыск.</color>");
                        yield break;
                    }

                    TimeLeft += 0.1f;

                    ev.ShowHint($"Вы обыскиваете труп, осталось <color=green>{Math.Round(FriskItemsTime - TimeLeft, 1)}</color> секунд.", 0.2f);

                    yield return Timing.WaitForSeconds(0.1f);
                }

                if (ev != null && ev.IsAlive && !ev.IsCuffed)
                {
                    foreach (ItemType item in itemlist)
                    {
                        Exiled.API.Features.Items.Item.Create(item).CreatePickup(itemSpawnPos);
                    }
                    ev.ShowHint($"<color=green>Вы успешно обыскали труп!</color>");
                    pl.RagdollItemsCountList.Remove(tar);
                }
            }
        }
    }
}
