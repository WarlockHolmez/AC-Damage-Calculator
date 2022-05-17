using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AC_Damage_Calculator
{
    public class Weapon
    {
        public string Name { get; set; }

        // Base Stats
        public string MeleeAttackAnimation { get; set; }
        public float MeleeMinDamage { get; set; }
        public float MeleeMaxDamage{ get; set; }
        public float MeleeAttackMod{ get; set; }
        public int MeleeSpeed { get; set; }
        public string MissileAttackAnimation{ get; set; }
        public string MissileAmmoType { get; set; }
        public int MissileDamageMod{ get; set; }
        public int MissileDamageBonus { get; set; }
        public int MissileSpeed { get; set; }
        public string MagicAttackAnimation{ get; set; }
        public string MagicSpell{ get; set; }
        public int MagicDamageBonus{ get; set; }

        // Imbues
        public bool MeleeArmorRend { get; set; }
        public bool MeleeArmorCleave { get; set; }
        public bool MeleeResistanceRend { get; set; }
        public float MeleeResistanceCleave { get; set; }
        public bool MeleeCriticalStrike { get; set; }
        public decimal MeleeBitingStrike { get; set; }
        public bool MeleeCripplingBlow { get; set; }
        public decimal MeleeCrushingBlow { get; set; }
        public bool MissileArmorRend { get; set; }
        public bool MissileArmorCleave { get; set; }
        public bool MissileResistanceRend { get; set; }
        public float MissileResistanceCleave { get; set; }
        public bool MissileCriticalStrike { get; set; }
        public decimal MissileBitingStrike { get; set; }
        public bool MissileCripplingBlow { get; set; }
        public decimal MissileCrushingBlow { get; set; }
        public bool MagicResistanceRend { get; set; }
        public float MagicResistanceCleave { get; set; }
        public bool MagicCriticalStrike { get; set; }
        public decimal MagicBitingStrike { get; set; }
        public bool MagicCripplingBlow { get; set; }
        public decimal MagicCrushingBlow { get; set; }

        // Spells/Slayer
        public string MeleeBloodDrinker { get; set; }
        public string MeleeBloodThirst { get; set; }
        public string MeleeHeartSeeker { get; set; }
        public string MeleeHeartThirst { get; set; }
        public string MeleeHighestSpell { get; set; }
        public decimal MeleeSlayer { get; set; }
        public string MissileBloodDrinker { get; set; }
        public string MissileBloodThirst { get; set; }
        public string MissileHighestSpell { get; set; }
        public decimal MissileSlayer { get; set; }
        public string MagicSpiritDrinker { get; set; }
        public string MagicSpiritThirst { get; set; }
        public decimal MagicSlayer { get; set; }

        // Power Bar
        public int MeleePowerBar { get; set; }
        public int MissileAccuracyBar { get; set; }
    }
}
