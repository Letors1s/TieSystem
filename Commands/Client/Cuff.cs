using CommandSystem;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TieSystem2.Commands.Client
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Cuff : ICommand
    {
        public string Command => "cuff";

        public string[] Aliases => new string[] { "cf" };

        public string Description => "Связывает игрока напротив.";

        public bool Execute(ArraySegment<string> ar, ICommandSender sender, out string r)
        {
            Plugin pl = Plugin.pl;

            Player ev = Player.Get(sender);

            Player tar = Plugin.GetLookedAtPlayer(ev, 2);

            if(ev == null || tar == null || !ev.IsAlive || !tar.IsAlive || ev.IsScp || tar.IsScp)
            {
                r = "Только люди могут связывать и только людей!";
                return false;
            }

            if(ev.IsCuffed)
            {
                r = "Вы не можете связывать людей, будучи связанными!";
                return false;
            }

            tar.Handcuff();
            r = "Вы связали человека напротив.";
            return true;
        }
    }
}
