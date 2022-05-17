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
        public bool MeleeResistanceCleave { get; set; }
        public bool MeleeCriticalStrike { get; set; }
        public float MeleeBitingStrike { get; set; }
        public float MeleeCripplingBlow { get; set; }
        public float MeleeCrushingBlow { get; set; }
        public bool MissileArmorRend { get; set; }
        public bool MissileArmorCleave { get; set; }
        public bool MissileResistanceRend { get; set; }
        public bool MissileResistanceCleave { get; set; }
        public bool MissileCriticalStrike { get; set; }
        public float MissileBitingStrike { get; set; }
        public float MissileCripplingBlow { get; set; }
        public float MissileCrushingBlow { get; set; }
        public bool MagicResistanceRend { get; set; }
        public bool MagicResistanceCleave { get; set; }
        public bool MagicCriticalStrike { get; set; }
        public float MagicBitingStrike { get; set; }
        public float MagicCripplingBlow { get; set; }
        public float MagicCrushingBlow { get; set; }

        // Spells/Slayer
        public string MeleeBloodDrinker { get; set; }
        public string MeleeBloodThirst { get; set; }
        public string MeleeHeartSeeker { get; set; }
        public string MeleeHeartThirst { get; set; }
        public string MeleeHighestSpell { get; set; }
        public float MeleeSlayer { get; set; }
        public string MissileBloodDrinker { get; set; }
        public string MissileBloodThirst { get; set; }
        public string MissileHighestSpell { get; set; }
        public float MissileSlayer { get; set; }
        public string MagicSpiritDrinker { get; set; }
        public string MagicSpiritThirst { get; set; }
        public float MagicSlayer { get; set; }

        // Power Bar
        public int MeleePowerBar { get; set; }
        public int MissilePowerBar { get; set; }
    }
}
