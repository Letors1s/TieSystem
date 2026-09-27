using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;
using LabApi.Loader.Features.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TieSystem2.Handlers
{
    public static class ReHandCuff
    {
        public static void OnRemovedHandcuff(RemovedHandcuffsEventArgs ar)
        {
            Plugin pl = Plugin.pl;
            if(!pl.PlayerTieList.TryGetValue(ar.Player, out int count))
            {
                count += 1;
                pl.PlayerTieList[ar.Player] = count;
            }

            if(pl.PlayerItemList.TryGetValue(ar.Target, out List<ItemType> items))
            {
                foreach(ItemType item in items)
                {
                    ar.Target.AddItem(item);
                }
                pl.PlayerItemList.Remove(ar.Target);
            }

            if(pl.PlayerAmmoList.TryGetValue(ar.Target, out Dictionary<AmmoType, ushort> ammok))
            {
                foreach(var ammo in ammok)
                {
                    ar.Target.AddAmmo(ammo.Key, ammo.Value);
                }
                pl.PlayerAmmoList.Remove(ar.Target);
            }
        }
    }
}
