using CommandSystem;
using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TieSystem2.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class UnCuff : ICommand
    {
        public string Command => "uncuff";

        public string[] Aliases => new string[] {"uc"};

        public string Description => "Развязывает.";

        public bool Execute(ArraySegment<string> ar, ICommandSender sender, out string r)
        {
            Plugin pl = Plugin.pl;

            Player ev = Player.Get(sender);

            //Player tar = Plugin.GetLookedAtPlayer(ev, 2);

            if(ev == null || !ev.IsAlive || ev.IsScp)
            {
                r = "Эту комманду могут использовать только люди.";
                return false;
            }

            //if(tar == null || !tar.IsAlive || tar.IsScp)
            //{
            if(ev.IsCuffed)
            {
                Timing.RunCoroutine(UnCuffev(ev));
                r = "Вы успешно развязались";
                return true;
            }
            else
            {
                r = "Вы не связаны!";
                return false;
            }
            //}
            //else
            //{
            //    if(tar.IsCuffed)
            //    {
            //        Timing.RunCoroutine.UnCufftar(tar);
            //    }
            //}
        }

        private IEnumerator<float> UnCuffev(Player ev)
        {
            Plugin pl = Plugin.pl;
            float left = 0;

            float Hp = ev.Health;

            while (pl.Config.UnTieTime > left)
            {
                ev.ShowHint($"Вы пытаетесь развязаться, осталось <color=green>{Math.Round(pl.Config.UnTieTime - left, 1)}</color> секунд.", 0.2f);

                float HpNow = ev.Health;
                if (Hp != HpNow) yield break;

                left++;
                yield return Timing.WaitForSeconds(0.1f);
            }

            if (ev != null && ev.IsAlive && ev.IsCuffed)
            {

                int range = UnityEngine.Random.Range(0, 101);
                if (range <= pl.Config.ChanceToSaveTiesOnUncuff)
                {
                    if (Plugin.pl.PlayerTieList.ContainsKey(ev))
                    {
                        if (Plugin.pl.PlayerTieList.ContainsKey(ev))
                        {
                            Plugin.pl.PlayerTieList[ev] += 1;
                        }
                        else
                        {
                            Plugin.pl.PlayerTieList.Add(ev, 1);
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
}
