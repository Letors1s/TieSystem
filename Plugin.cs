using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TieSystem2.Handlers;
using UnityEngine;

namespace TieSystem2
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "TieSystem";
        public override string Author => "Letors1s";
        public override Version Version => new Version(2, 0, 0);
        public override Version RequiredExiledVersion => new Version(9, 14, 2);

        // Доп переменные
        public static Plugin pl;

        public Dictionary<Player, int> PlayerTieList = new Dictionary<Player, int>();
        public Dictionary<Ragdoll, int> RagdollTieList = new Dictionary<Ragdoll, int>();

        public Dictionary<Player, List<ItemType>> PlayerItemList = new Dictionary<Player, List<ItemType>>();
        public Dictionary<Ragdoll, List<ItemType>> RagdollItemList = new Dictionary<Ragdoll, List<ItemType>>();
        public Dictionary<Player, Dictionary<AmmoType, ushort>> PlayerAmmoList = new Dictionary<Player, Dictionary<AmmoType, ushort>>();
        public Dictionary<Ragdoll, Dictionary<AmmoType, ushort>> RagdollAmmoList = new Dictionary<Ragdoll, Dictionary<AmmoType, ushort>>();


        public override void OnEnabled()
        {
            pl = this;

            Exiled.Events.Handlers.Player.Spawned += Spawning.OnSpawn;
            Exiled.Events.Handlers.Player.Handcuffing += Handcuff.OnHandcuffing;
            Exiled.Events.Handlers.Player.RemovedHandcuffs += ReHandCuff.OnRemovedHandcuff;
            Exiled.Events.Handlers.Player.Dying += OnDying.OnDyingEv;
            Exiled.Events.Handlers.Player.Died += OnDying.OnDied;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnd;
            Exiled.Events.Handlers.Player.Left += OnLeft;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Spawned -= Spawning.OnSpawn;
            Exiled.Events.Handlers.Player.Handcuffing -= Handcuff.OnHandcuffing;
            Exiled.Events.Handlers.Player.RemovedHandcuffs -= ReHandCuff.OnRemovedHandcuff;
            Exiled.Events.Handlers.Player.Dying -= OnDying.OnDyingEv;
            Exiled.Events.Handlers.Player.Died -= OnDying.OnDied;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnd;
            Exiled.Events.Handlers.Player.Left -= OnLeft;

            pl = null;
            base.OnDisabled();
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

        public static Ragdoll GetLookedAtRagdoll(Player player, float maxDistance)
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

        public void OnRoundEnd(RoundEndedEventArgs ar)
        {
            PlayerTieList.Clear();
        }

        public void OnLeft(LeftEventArgs ar)
        {
            if (PlayerTieList.ContainsKey(ar.Player)) PlayerTieList.Remove(ar.Player);
        }

        public void OnDroppingItem(DroppingItemEventArgs ar)
        {
            if (ar.Player == null) return;

            foreach (Player target in Player.List)
            {
                if (target.Cuffer == ar.Player)
                {
                    target.Handcuff();
                }
            }
        }

        public static void SaveItems(Player tar)
        {
            Plugin pl = Plugin.pl;

            if (tar == null || !tar.IsAlive || tar.IsScp) return;

            if (!pl.PlayerItemList.ContainsKey(tar))
            {
                List<ItemType> items = new List<ItemType>();

                foreach (var item in tar.Items)
                {
                    items.Add(item.Type);
                }

                pl.PlayerItemList.Add(tar, items);
            }

            Dictionary<AmmoType, ushort> ammos = new Dictionary<AmmoType, ushort>();
            foreach (var ammo in tar.Ammo)
            {
                if (ammo.Value > 0)
                {
                    ammos.Add((AmmoType)ammo.Key, ammo.Value);
                }
            }
            pl.PlayerAmmoList[tar] = ammos;
        }
    }
}
