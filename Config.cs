using Exiled.API.Interfaces;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TieSystem
{
    public class Config : IConfig
    {
        [Description("Основа плагина.")]
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;


        [Description("Стяжек изначально.")]
        public Dictionary<RoleTypeId, int> RoleTieCount { get; set; } = new Dictionary<RoleTypeId, int>
        {
            {RoleTypeId.ClassD, 0},
            {RoleTypeId.Scientist, 0},
            {RoleTypeId.Tutorial, 0 },

            {RoleTypeId.FacilityGuard, 2},

            {RoleTypeId.NtfCaptain, 4 },
            {RoleTypeId.NtfPrivate, 4 },
            {RoleTypeId.NtfSergeant, 4 },
            {RoleTypeId.NtfSpecialist, 4 },

            {RoleTypeId.ChaosConscript, 2 },
            {RoleTypeId.ChaosMarauder, 2 },
            {RoleTypeId.ChaosRepressor, 2 },
            {RoleTypeId.ChaosRifleman, 2 },

            {RoleTypeId.Flamingo, 0 },
            {RoleTypeId.AlphaFlamingo, 0 },
            {RoleTypeId.NtfFlamingo, 0 },
            {RoleTypeId.ChaosFlamingo, 0 },
            {RoleTypeId.ZombieFlamingo, 0 },

            {RoleTypeId.Spectator, 0 },
            {RoleTypeId.Overwatch, 0 },

            {RoleTypeId.Scp049, 0 },
            {RoleTypeId.Scp0492, 0 },
            {RoleTypeId.Scp079, 0 },
            {RoleTypeId.Scp096, 0 },
            {RoleTypeId.Scp106, 0 },
            {RoleTypeId.Scp173, 0 },
            {RoleTypeId.Scp3114, 0 },
            {RoleTypeId.Scp939, 0 },

        };

        [Description("Включено ограничение стяжек?")]
        public bool TieCountEnabled = true;

        [Description("Время развязывания. (Желательно целое число.)")]
        public float UnTieTime = 60f;

        [Description("При смерти игрок развязывается?")]
        public bool PlayerUncuffOnDeath = false;

        [Description("Шанс на сохранение стяжек при развязывании")]
        public int ChanceToSaveTiesOnUncuff = 25;
    }
}
