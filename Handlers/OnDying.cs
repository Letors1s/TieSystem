using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TieSystem2.Handlers
{
    public class OnDying
    {
        public static void OnDyingEv(DyingEventArgs ar)
        {
            Plugin pl = Plugin.pl;

            if (!pl.PlayerItemList.ContainsKey(ar.Player) || !pl.PlayerAmmoList.ContainsKey(ar.Player))
            {
                Plugin.SaveItems(ar.Player);
                ar.Player.ClearInventory();
            }
        }

        public static void OnDied(DiedEventArgs ar)
        {
            Plugin pl = Plugin.pl;

            if (pl.PlayerItemList.TryGetValue(ar.Player, out List<ItemType> items))
            {
                pl.RagdollItemList.Add(ar.Ragdoll, items);
                pl.PlayerItemList.Remove(ar.Player);
            }

            if (pl.PlayerAmmoList.TryGetValue(ar.Player, out Dictionary<AmmoType, ushort> ammo))
            {
                pl.RagdollAmmoList.Add(ar.Ragdoll, ammo);
                pl.PlayerAmmoList.Remove(ar.Player);
            }

            if(pl.PlayerTieList.TryGetValue(ar.Player, out int tie))
            {
                pl.RagdollTieList.Add(ar.Ragdoll, tie);
                pl.PlayerTieList.Remove(ar.Player);
            }
        }
    }
}
