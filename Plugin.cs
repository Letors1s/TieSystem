using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using YamlDotNet.Core.Tokens;
using static UnityEngine.GraphicsBuffer;

namespace TieSystem
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "TieSystem";
        public override string Author => "Letors1s";
        public override Version Version => new Version(3, 0, 0);
        public override Version RequiredExiledVersion => new Version(9, 14, 2);

        // Доп переменные
        public static Plugin Instance;

        public Dictionary<Player, int> PlayerTieCountList = new Dictionary<Player, int>();

        public Dictionary<Ragdoll, int> RagdollTieCountList = new Dictionary<Ragdoll, int>();


        public Dictionary<Player, List<ItemType>> PlayerItemsCountList = new Dictionary<Player, List<ItemType>>();

        public Dictionary<Ragdoll, List<ItemType>> RagdollItemsCountList = new Dictionary<Ragdoll, List<ItemType>>();

        public override void OnEnabled()
        {
            Instance = this;
            Exiled.Events.Handlers.Player.Handcuffing += OnHandCuff;
            Exiled.Events.Handlers.Player.RemovingHandcuffs += OnReHandCuff;
            Exiled.Events.Handlers.Player.RemovedHandcuffs += OnReingHandCuff;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;


            Exiled.Events.Handlers.Player.Left += OnLeft;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnd;
            Exiled.Events.Handlers.Player.Died += OnDied;
            Exiled.Events.Handlers.Player.Dying += OnDying;

            Exiled.Events.Handlers.Player.DroppingItem += OnDroppingItem;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;
            Exiled.Events.Handlers.Player.Handcuffing -= OnHandCuff;
            Exiled.Events.Handlers.Player.RemovingHandcuffs -= OnReHandCuff;
            Exiled.Events.Handlers.Player.RemovedHandcuffs -= OnReingHandCuff;
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;

            Exiled.Events.Handlers.Player.Left -= OnLeft;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnd;
            Exiled.Events.Handlers.Player.Died -= OnDied;
            Exiled.Events.Handlers.Player.Dying -= OnDying;

            Exiled.Events.Handlers.Player.DroppingItem -= OnDroppingItem;

            base.OnDisabled();
        }

        public void OnReingHandCuff(RemovedHandcuffsEventArgs ar)
        {
            if (PlayerItemsCountList.TryGetValue(ar.Target, out List<ItemType> items))
            {
                foreach (ItemType item in items)
                {
                    ar.Target.AddItem(item);
                }
                PlayerItemsCountList.Remove(ar.Target);
            }
        }

        public void OnDying(DyingEventArgs ar)
        {
            if(!PlayerItemsCountList.TryGetValue(ar.Player, out List<ItemType> itemlist))
            {
                List<ItemType> items = new List<ItemType>();

                foreach(var item in ar.Player.Items)
                {
                    items.Add(item.Type);
                }

                PlayerItemsCountList.Add(ar.Player, items);
                ar.Player.ClearInventory();
            }
        }

        public void OnDroppingItem(DroppingItemEventArgs ar)
        {
            if (ar.Player == null) return;

            if (!Config.PlayerUncuffOnDeath)
            {
                foreach (Player target in Player.List)
                {
                    if (target.Cuffer == ar.Player)
                    {
                        target.Handcuff();
                    }
                }
            }
        }

        public void OnDied(DiedEventArgs ar)
        {
            if(PlayerTieCountList.TryGetValue(ar.Player, out int tc))
                RagdollTieCountList.Add(ar.Ragdoll, tc);

            if (ar.Player != null && PlayerTieCountList.ContainsKey(ar.Player))
            {
                PlayerTieCountList.Remove(ar.Player);
            }

            if (!Config.PlayerUncuffOnDeath)
            {
                foreach (Player target in Player.List)
                {
                    if (target.Cuffer == ar.Player)
                    {
                        target.Handcuff();
                    }
                }
            }

            if (PlayerItemsCountList.TryGetValue(ar.Player, out List<ItemType> items))
            {
                RagdollItemsCountList.Add(ar.Ragdoll, items);
                PlayerItemsCountList.Remove(ar.Player);
            }
        }

        public void OnRoundEnd(RoundEndedEventArgs ar)
        {
            PlayerTieCountList.Clear();
        }

        public void OnLeft(LeftEventArgs ar)
        {
            if (PlayerTieCountList.ContainsKey(ar.Player)) PlayerTieCountList.Remove(ar.Player);
        }

        public void OnSpawned(SpawnedEventArgs ar)
        {
            if (Config.TieCountEnabled)
            {
                //если игрок отсуцтвует или мёртв то заканчиваем блок
                if (ar.Player == null || !ar.Player.IsAlive) return;

                //не даём игроку оказаться в списке 2 раза
                if (PlayerTieCountList.ContainsKey(ar.Player))
                {
                    PlayerTieCountList.Remove(ar.Player);
                }

                RoleTypeId playerRole = ar.Player.Role.Type;

                //ищем роль в списке из конфига и присваеваем её значение игроку
                if (Config.RoleTieCount.TryGetValue(playerRole, out int count))
                {
                    PlayerTieCountList.Add(ar.Player, count);
                }
            }
        }

        public void OnReHandCuff(RemovingHandcuffsEventArgs ar)
        {
            if (Config.TieCountEnabled)
            {
                if (PlayerTieCountList.TryGetValue(ar.Player, out int count))
                {
                    count += 1;
                    ar.Player.ShowHint($"У вас осталось {count} стяжек.");
                    PlayerTieCountList[ar.Player] = count;
                }

                ar.IsAllowed = true;
            }
            else ar.IsAllowed = true;
        }

        //При связывании действие передаётся серверу, дабы исключить произвольное развязывание, - 1 стяжка
        public void OnHandCuff(HandcuffingEventArgs ar)
        {
            if (ar.Player == null || ar.Target == null) return;

            if (ar.Target.IsCuffed || ar.Player == ar.Target)
            {
                ar.IsAllowed = false;
                return;
            }

            if (Config.TieCountEnabled)
            {
                if (PlayerTieCountList.TryGetValue(ar.Player, out int count) && count > 0)
                {
                    count -= 1;
                    ar.Player.ShowHint($"У вас осталось {count} стяжек.");
                    PlayerTieCountList[ar.Player] = count;

                    ar.IsAllowed = true;
                }
                else
                {
                    ar.Player.ShowHint($"У вас нет стяжек!");

                    ar.IsAllowed = false;
                }
            }
            else
            {
                ar.IsAllowed = true;
            }

            List<ItemType> items = new List<ItemType>();

            foreach (var item in ar.Target.Items)
            {
                items.Add(item.Type);
            }

            PlayerItemsCountList.Add(ar.Target, items);
            ar.Target.ClearInventory();

        }
        public static Player GetLookedAtPlayer(Player player, float maxDistance = 4f)
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
    }
}
