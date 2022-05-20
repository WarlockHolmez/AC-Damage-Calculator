using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AC_Damage_Calculator
{
    public class Enemy
    {
        public string Name { get; set; }

        // Base Defenses
        public int Armor { get; set; }
        public decimal Resist { get; set; }
        public int ShieldArmor { get; set; }
        public decimal ShieldResist { get; set; }
        public int MeleeDefense { get; set; }
        public int MissileDefense { get; set; }
        public int MagicDefense { get; set; }

        // Life Debuffs
        public string Imperil { get; set; }
        public string ResistanceVuln { get; set; }

        // Void Debuffs
        public int Corruption { get; set; }
        public int Corrosion { get; set; }
        public int Destructive { get; set; }

        // Creature Debuffs
        public string Vulnerability { get; set; }
        public string Defenselessness { get; set; }
        public string MagicYield { get; set; }
        public string UnbalancedAssault { get; set; }
        public string Weakness { get; set; }
        public string Clumsiness { get; set; }
        public string Slowness { get; set; }
        public string Bafflement { get; set; }
        public string Feeblemind { get; set; }

        // Item Debuffs
        public string Brittlemail { get; set; }
        public string ResistanceLure { get; set; }
    }
}
