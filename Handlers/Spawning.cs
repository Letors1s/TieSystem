using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TieSystem2.Handlers
{
    public static class Spawning
    {
        public static void OnSpawn(SpawnedEventArgs ar)
        {
            Plugin pl = Plugin.pl;

            if (ar.Player == null || !ar.Player.IsAlive || ar.Player.IsScp) return;

            if(pl.PlayerTieList.ContainsKey(ar.Player))
            {
                pl.PlayerTieList.Remove(ar.Player);
            }

            RoleTypeId plRole = ar.Player.Role.Type;

            if(pl.Config.RoleTieCount.TryGetValue(plRole, out int tieCount))
            {
                pl.PlayerTieList.Add(ar.Player, tieCount);
            }
        }
    }
}
