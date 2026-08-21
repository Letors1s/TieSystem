using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using YamlDotNet.Core.Tokens;
using static UnityEngine.GraphicsBuffer;
using MEC;

namespace TieSystem
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "TieSystem";
        public override string Author => "Letors1s";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(9, 14, 2);

        // Доп переменные
        public static Plugin Instance;

        public Dictionary<Player, int> PlayerTieCountList = new Dictionary<Player, int>();

        public override void OnEnabled()
        {
            Instance = this;
            Exiled.Events.Handlers.Player.Handcuffing += OnHandCuff;
            Exiled.Events.Handlers.Player.RemovingHandcuffs += OnReHandCuff;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;


            Exiled.Events.Handlers.Player.Left += OnLeft;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnd;
            Exiled.Events.Handlers.Player.Died += OnDied;

            Exiled.Events.Handlers.Player.DroppingItem += OnDroppingItem;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;
            Exiled.Events.Handlers.Player.Handcuffing -= OnHandCuff;
            Exiled.Events.Handlers.Player.RemovingHandcuffs -= OnReHandCuff;
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;

            Exiled.Events.Handlers.Player.Left -= OnLeft;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnd;
            Exiled.Events.Handlers.Player.Died -= OnDied;

            Exiled.Events.Handlers.Player.DroppingItem -= OnDroppingItem;
            base.OnDisabled();
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
            if(Config.TieCountEnabled)
            {
                //если игрок отсуцтвует или мёртв то заканчиваем блок
                if (ar.Player == null || !ar.Player.IsAlive) return;

                //не даём игроку оказаться в списке 2 раза
                if(PlayerTieCountList.ContainsKey(ar.Player))
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
            if(Config.TieCountEnabled)
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
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    public class KnowTieCount : ICommand
    {
        public string Command { get; set; } = "tiecount";
        public string[] Aliases { get; set; } = new string[] {"tc"};
        public string Description { get; set; } = "Узнать кол-во стяжек, находящихся у вас.";

        public bool Execute(ArraySegment<string> ar, ICommandSender player, out string r)
        {
            Exiled.API.Features.Player ev = Exiled.API.Features.Player.Get(player);

            if(Plugin.Instance.PlayerTieCountList.TryGetValue(ev, out int count))
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
            if(ar.Count < 2)
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

            if (!int.TryParse(ar.At(1), out int tiesAmount) || tiesAmount < 0)
            {
                r = "Количество стяжек должно быть целым положительным числом!";
                return false;
            }

            if (Plugin.Instance.PlayerTieCountList.ContainsKey(tar))
            {
                Plugin.Instance.PlayerTieCountList[tar] = tiesAmount;
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

            while(leftTime < UnCuffTime)
            {
                if (ev == null || !ev.IsAlive || !ev.IsCuffed) yield break;

                leftTime += 0.1f;

                ev.ShowHint($"Вы пытаетесь развязаться, осталось <color=green>{Math.Round(UnCuffTime - leftTime, 1)}</color> секунд.", 0.2f);

                yield return Timing.WaitForSeconds(0.1f);
            }

            if (ev != null && ev.IsAlive && ev.IsCuffed)
            {

                int range = UnityEngine.Random.Range(0, 101);
                if(range <= pl.Config.ChanceToSaveTiesOnUncuff)
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
}

