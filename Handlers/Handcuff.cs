using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TieSystem2.Handlers
{
    public static class Handcuff
    {
        public static void OnHandcuffing(HandcuffingEventArgs ar)
        {
            Plugin pl = Plugin.pl;

            if(pl.PlayerTieList.TryGetValue(ar.Player, out int count))
            {
                if(count > 0)
                {
                    count -= 1;
                    pl.PlayerTieList[ar.Player] = count;
                    ar.Player.ShowHint($"\n\nУ вас осталось {count} стяжек.");


                    Plugin.SaveItems(ar.Target);
                    ar.Target.ClearInventory();
                }
                else
                {
                    ar.Player.ShowHint($"\n\nУ вас нет стяжек.");
                    ar.IsAllowed = false;
                }
            }
            else
            {
                ar.IsAllowed = false;
            }
        }
    }
}
