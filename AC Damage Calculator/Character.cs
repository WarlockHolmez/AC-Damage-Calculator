using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AC_Damage_Calculator
{
    public class Character
    {
        public string Name { get; set; }

        // Skills
        public bool FinesseWeapon { get; set; }
        public bool ThrownWeapon { get; set; }
        public int MeleeBase { get; set; }
        public int MeleeBuffed { get; set; }
        public int MissileBase { get; set; }
        public int MissileBuffed { get; set; }
        public int MagicBase { get; set; }
        public int MagicBuffed { get; set; }

        // Attributes
        public int Strength { get; set; }
        public int Coordination { get; set; }
        public int Quickness { get; set; }

        // Other Skills
        public int Recklessness { get; set; }
        public int SneakAttack { get; set; }
        public int Deception { get; set; }
        public string RecklessnessComboBox { get; set; }
        public string SneakAttackComboBox { get; set; }
        public string DeceptionComboBox { get; set; }

        // Ratings
        public int DamageRating { get; set; }
        public int CritDamageRating { get; set; }
        public int CritChance { get; set; }

        // Rares/Surges
        public bool StrengthRare { get; set; }
        public bool CoordinationRare { get; set; }
        public bool QuicknessRare { get; set; }
        public bool FocusRare { get; set; }
        public bool SelfRare { get; set; }
        public bool MeleeRare { get; set; }
        public bool MissileRare { get; set; }
        public bool MagicRare { get; set; }
        public bool CisSurge { get; set; }
        public bool DestSurge { get; set; }
    }
}
