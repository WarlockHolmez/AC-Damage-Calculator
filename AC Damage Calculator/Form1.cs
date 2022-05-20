using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace AC_Damage_Calculator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Final Calculations
        // ---------- FINAL CALCULATIONS ----------

        // This is called whenever any value is changed within the DPS Calculator tab 
        private void CalculateFinalDps(object sender, EventArgs e)
        {
            var finalDpsFront = FinalAvgHitDamageFront() * FinalAttackSpeed() * HitChance();
            labelFinalDpsFront.Text = Math.Round(finalDpsFront, 0).ToString();

            var finalDpsRear = FinalAvgHitDamageRear() * FinalAttackSpeed() * HitChance();
            labelFinalDpsRear.Text = Math.Round(finalDpsRear, 0).ToString();

            QuickCompare();

            Application.DoEvents();
        }

        // Returns the final attack speed (attacks per second)
        // Also updates the Final Attack Speed label
        private float FinalAttackSpeed()
        {
            var finalAttackSpeed = 1.0f;

            // Anim lengths
            var slash = 1.676f;
            var stab = 1.45f;
            var punch = 1.2f;
            var multiSlash = 1.35f;
            var multiStab = 1.58f;
            var twohandCleave = 1f;
            var bow = 2.065f;
            var xbow = 3.063f;
            var atlatl = 3.089f;

            // Cast Anim casts per second
            var castNormal = 0.3f;
            var castWand1 = 0.48f;
            var castWand2 = 1.0f;

            // Calculate attacks per second, based on quickness and weapon speed
            decimal weaponSpeed = 0;
            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                weaponSpeed = numericUpDownMeleeWeaponSpeed.Value;
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                weaponSpeed = numericUpDownMissileWeaponSpeed.Value;
            }

            var quickness = numericUpDownBuffedQuickness.Value > 300 ? 300 : numericUpDownBuffedQuickness.Value;
            var divisor = 1 - (quickness / 300) + (weaponSpeed / 150);

            float animSpeedMod;

            if(divisor <= 0)
            {
                animSpeedMod = 2;
            }
            else
            {
               animSpeedMod = Math.Max(Math.Min(1.0f / (float)divisor, 2.0f), 0.5f);
            }

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                if (comboBoxMeleeAnimation.SelectedItem.ToString() == "Slash")
                {
                    finalAttackSpeed = 1 / (slash / animSpeedMod + ((float)trackBarMeleePowerBar.Value / 100));
                }
                else if (comboBoxMeleeAnimation.SelectedItem.ToString() == "Stab")
                {
                    finalAttackSpeed = 1 / (stab / animSpeedMod + ((float)trackBarMeleePowerBar.Value / 100));
                }
                else if (comboBoxMeleeAnimation.SelectedItem.ToString() == "Punch")
                {
                    finalAttackSpeed = 1 / (punch / animSpeedMod + ((float)trackBarMeleePowerBar.Value / 100));
                }
                else if (comboBoxMeleeAnimation.SelectedItem.ToString() == "Multi-Slash")
                {
                    finalAttackSpeed = 1 / ((multiSlash / animSpeedMod * 2) + ((float)trackBarMeleePowerBar.Value / 100)) * 2;
                }
                else if (comboBoxMeleeAnimation.SelectedItem.ToString() == "Multi-Stab")
                {
                    finalAttackSpeed = 1 / ((multiStab / animSpeedMod * 2) + ((float)trackBarMeleePowerBar.Value / 100)) * 2;
                }
                else if (comboBoxMeleeAnimation.SelectedItem.ToString() == "Two-hand")
                {
                    finalAttackSpeed = (1 / ((twohandCleave / animSpeedMod * 2) + (float)trackBarMeleePowerBar.Value / 100)) * 2;
                }
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                if (comboBoxMissileAnimation.SelectedItem.ToString() == "Bow")
                {
                    finalAttackSpeed = 1 / (bow / animSpeedMod + ((float)trackBarMissileAccuracyBar.Value / 100));
                }
                else if (comboBoxMissileAnimation.SelectedItem.ToString() == "Xbow")
                {
                    finalAttackSpeed = 1 / (xbow / animSpeedMod + ((float)trackBarMissileAccuracyBar.Value / 100));
                }
                else if (comboBoxMissileAnimation.SelectedItem.ToString() == "Atlatl")
                {
                    finalAttackSpeed = 1 / (atlatl / animSpeedMod + ((float)trackBarMissileAccuracyBar.Value / 100));
                }
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Magic"])
            {
                if (comboBoxMagicAnimation.SelectedItem.ToString() == "Normal")
                {
                    finalAttackSpeed = castNormal;
                }
                else if (comboBoxMagicAnimation.SelectedItem.ToString() == "Normal")
                {
                    finalAttackSpeed = castWand1;
                }
                else
                {
                    finalAttackSpeed = castWand2;
                }
            }
                //Console.WriteLine("ComboBoxText = " + comboBoxMeleeAnimation.SelectedItem.ToString() + ". Slash anim = " + slash + ". Divisor = " + divisor + ". animSpeedMod = " + animSpeedMod + ". powerBarValue = " + (float)trackBarMeleePowerBar.Value / 100 + ". finalAttackSpeed = " + finalAttackSpeed);
                //Console.WriteLine(1 / (slash / animSpeedMod + ((float)trackBarMeleePowerBar.Value / 100)));

                labelEffectiveAttackSpeed.Text = Math.Round(finalAttackSpeed, 2).ToString();

            return finalAttackSpeed;
        }

        // Returns the hit chance
        // Also updates the Hit Chance label
        private double HitChance()
        {
            double hitChance = 1;
            int meleeDef = FinalEnemyMeleeDefense();
            int missileDef = FinalEnemyMissileDefense();
            int magicDef = FinalEnemyMagicDefense();

            if (checkBoxUseHitChance.Checked)
            {
                if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
                {
                    hitChance = Math.Max(1 - (1 / Math.Exp(1 + 0.03 * (FinalMeleeAttackSkill() - meleeDef))), 0);
                }
                else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
                {
                    hitChance = Math.Max(1 - (1 / Math.Exp(1 + 0.03 * (FinalMissileAttackSkill() - missileDef))), 0);
                }
                else
                {
                    hitChance = Math.Max(1 - (1 / Math.Exp(1 + 0.03 * (FinalMagicAttackSkill() - magicDef))), 0);
                }
            }

            labelHitChance.Text = Math.Round(hitChance * 100, 2).ToString() + "%";

            return hitChance;
        }

        // Returns final average damage (front) per attack
        // Also updates the Final Avg Damage (front) label
        private int FinalAvgHitDamageFront()
        {
            var finalAvgHit = (FinalCritDamageFront() * FinalCritChance()) + (FinalNonCritDamageFront() * (1 - FinalCritChance()));

            labelFinalAvgHitFront.Text = Math.Round(finalAvgHit, 0).ToString();

            return (int)finalAvgHit;
        }

        // Returns the final average damage (rear) per attack, with any Sneak Attack bonuses
        // Also updates the Final Avg Damage (rear) label
        private int FinalAvgHitDamageRear()
        {
            var finalAvgHit = (FinalCritDamageRear() * FinalCritChance()) + (FinalNonCritDamageRear() * (1 - FinalCritChance()));

            labelFinalAvgHitRear.Text = Math.Round(finalAvgHit, 0).ToString();

            return (int)finalAvgHit;
        }

        // Returns final non-crit damage (front) per attack
        // Also updates the Final non-Crit Damage (front) label
        private int FinalNonCritDamageFront()
        {
            var finalNonCritDamageFront = 0.0f;
            float finalNonCritMinDamageFront = NonCritMinDamageFront();
            float finalNonCritMaxDamageFront = NonCritMaxDamageFront();
            var avgDamage = (finalNonCritMaxDamageFront + finalNonCritMinDamageFront) / 2;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Magic"])
            {
                finalNonCritDamageFront = Math.Abs(avgDamage * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());
                finalNonCritMinDamageFront = Math.Abs(finalNonCritMinDamageFront * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());
                finalNonCritMaxDamageFront = Math.Abs(finalNonCritMaxDamageFront * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());

                labelFinalNonCritFront.Text = Math.Round(finalNonCritMinDamageFront, 0).ToString() + " - " + Math.Round((float)finalNonCritMaxDamageFront, 0).ToString();
                labelNonCritFront.Text = Math.Round((float)NonCritMinDamageFront(), 0).ToString() + " - " + Math.Round((float)NonCritMaxDamageFront(), 0).ToString();

            }
            else
            {
                //finalNonCritDamageFront = Math.Abs(NonCritDamageFront() * FinalEnemyArmorMod());
                finalNonCritDamageFront = Math.Abs(avgDamage * FinalEnemyArmorMod() * FinalEnemyShieldMod() * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());
                finalNonCritMinDamageFront = Math.Abs(finalNonCritMinDamageFront * FinalEnemyArmorMod() * FinalEnemyShieldMod() * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());
                finalNonCritMaxDamageFront = Math.Abs(finalNonCritMaxDamageFront * FinalEnemyArmorMod() * FinalEnemyShieldMod() * FinalEnemyResitanceVulnMod()) * FinalDamageRatingVoidMod();

                labelFinalNonCritFront.Text = Math.Round(finalNonCritMinDamageFront, 0).ToString() + " - " + Math.Round(finalNonCritMaxDamageFront, 0).ToString();// + "  (" + Math.Round(finalNonCritDamageFront, 0).ToString() + ")";
                labelNonCritFront.Text = Math.Round((float)NonCritMinDamageFront(), 0).ToString() + " - " + Math.Round((float)NonCritMaxDamageFront(), 0).ToString();
            }

            return (int)finalNonCritDamageFront;
        }

        // Returns final non-crit damage (rear) per attack
        // Also updates the Final non-Crit Damage (rear) label
        private int FinalNonCritDamageRear()
        {
            var finalNonCritDamageRear = 0.0f;
            float finalNonCritMinDamageRear = NonCritMinDamageRear();
            float finalNonCritMaxDamageRear = NonCritMaxDamageRear();
            var avgDamage = (finalNonCritMinDamageRear + finalNonCritMaxDamageRear) / 2;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Magic"])
            {
                finalNonCritDamageRear = avgDamage * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod();
                finalNonCritMinDamageRear = Math.Abs(finalNonCritMinDamageRear * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());
                finalNonCritMaxDamageRear = Math.Abs(finalNonCritMaxDamageRear * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());

                labelFinalNonCritRear.Text = Math.Round(finalNonCritMinDamageRear, 0).ToString() + " - " + Math.Round((float)finalNonCritMaxDamageRear, 0).ToString();
                labelNonCritRear.Text = Math.Round((float)NonCritMinDamageRear(), 0).ToString() + " - " + Math.Round((float)NonCritMaxDamageRear(), 0).ToString();
            }
            else
            {
                finalNonCritDamageRear = Math.Abs(avgDamage * FinalEnemyArmorMod() * FinalEnemyShieldMod() * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());
                finalNonCritMinDamageRear = Math.Abs(finalNonCritMinDamageRear * FinalEnemyArmorMod() * FinalEnemyShieldMod() * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());
                finalNonCritMaxDamageRear = Math.Abs(finalNonCritMaxDamageRear * FinalEnemyArmorMod() * FinalEnemyShieldMod() * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());

                labelFinalNonCritRear.Text = Math.Round(finalNonCritMinDamageRear, 0).ToString() + " - " + Math.Round(finalNonCritMaxDamageRear, 0).ToString();// + "  (" + Math.Round(finalNonCritDamageFront, 0).ToString() + ")";
                labelNonCritRear.Text = Math.Round((float)NonCritMinDamageRear(), 0).ToString() + " - " + Math.Round((float)NonCritMaxDamageRear(), 0).ToString();
            }
            
            return (int)finalNonCritDamageRear;
        }

        // Returns final crit damage (front) per attack
        // Also updates the Final Crit Damage (front) label
        private int FinalCritDamageFront()
        {
            var finalCritDamageFront = 0.0f;
            float finalCritMaxDamageFront = CritMaxDamageFront();

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Magic"])
            {
                float critMinDamageFront = (SpellMinDamage() + (SpellMaxDamage() * 0.5f * FinalCritMultiplier()) + AttributeMod()) * (1 + (float)BuffedElementalDamageBonus() / 100) *
                                       (float)numericUpDownMagicSlayer.Value * (FinalCritDamageRatingMod() + DeceptionMod() - 1);
                var critMaxDamageFront = (SpellMaxDamage() + (SpellMaxDamage() * 0.5f * FinalCritMultiplier()) + AttributeMod())  * (1 + (float)BuffedElementalDamageBonus() / 100) *
                                       (float)numericUpDownMagicSlayer.Value * (FinalCritDamageRatingMod() + DeceptionMod() - 1);

                finalCritDamageFront = (AvgDamage() + (SpellMaxDamage() * 0.5f * FinalCritMultiplier()) + AttributeMod())  * (1 + (float)BuffedElementalDamageBonus() / 100) *
                                       (float)numericUpDownMagicSlayer.Value * (FinalCritDamageRatingMod() + DeceptionMod() - 1) * FinalEnemyResitanceVulnMod();

                var finalCritMinDamageFront = Math.Abs(critMinDamageFront * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());
                finalCritMaxDamageFront = Math.Abs(critMaxDamageFront * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());

                labelFinalCritFront.Text = Math.Round(finalCritMinDamageFront, 0).ToString() + " - " + Math.Round(finalCritMaxDamageFront, 0).ToString();
                labelCritFront.Text = Math.Round(critMinDamageFront, 0).ToString() + " - " + Math.Round(critMaxDamageFront, 0).ToString();
            }
            else
            {
                finalCritDamageFront = Math.Abs(finalCritMaxDamageFront * FinalEnemyArmorMod() * FinalEnemyShieldMod() * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());

                labelFinalCritFront.Text = Math.Round(finalCritDamageFront, 0).ToString();
                labelCritFront.Text = Math.Round((float)CritMaxDamageFront(), 0).ToString();
            }

            

            return (int)finalCritDamageFront;
        }

        // Returns final crit damage (rear) per attack
        // Also updates the Final Crit Damage (rear) label
        private int FinalCritDamageRear()
        {
            var finalCritDamageRear = 0.0f;

            float finalCritMaxDamageRear = CritMaxDamageRear();

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Magic"])
            {
                float critMinDamageRear = (SpellMinDamage() + (SpellMaxDamage() * 0.5f * FinalCritMultiplier()) + AttributeMod())  * (1 + (float)BuffedElementalDamageBonus() / 100) *
                                       (float)numericUpDownMagicSlayer.Value * (FinalCritDamageRatingMod() + SneakAttackMod() - 1);
                var critMaxDamageRear = (SpellMaxDamage() + (SpellMaxDamage() * 0.5f * FinalCritMultiplier()) + AttributeMod())  * (1 + (float)BuffedElementalDamageBonus() / 100) *
                                       (float)numericUpDownMagicSlayer.Value * (FinalCritDamageRatingMod() + SneakAttackMod() - 1);

                finalCritDamageRear = (AvgDamage() + (SpellMaxDamage() * 0.5f * FinalCritMultiplier()) + AttributeMod())  * (1 + (float)BuffedElementalDamageBonus() / 100) *
                                       (float)numericUpDownMagicSlayer.Value * (FinalCritDamageRatingMod() + SneakAttackMod() - 1) * FinalEnemyResitanceVulnMod();

                var finalCritMinDamageRear = Math.Abs(critMinDamageRear * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());
                finalCritMaxDamageRear = Math.Abs(critMaxDamageRear * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());

                labelFinalCritRear.Text = Math.Round(finalCritMinDamageRear, 0).ToString() + " - " + Math.Round(finalCritMaxDamageRear, 0).ToString();
                labelCritRear.Text = Math.Round(critMinDamageRear, 0).ToString() + " - " + Math.Round(critMaxDamageRear, 0).ToString();
            }
            else
            {
                finalCritDamageRear = Math.Abs(finalCritMaxDamageRear * FinalEnemyArmorMod() * FinalEnemyShieldMod() * FinalEnemyResitanceVulnMod() * FinalDamageRatingVoidMod());

                labelFinalCritRear.Text = Math.Round(finalCritDamageRear, 0).ToString();
                labelCritRear.Text = Math.Round((float)CritMaxDamageRear(), 0).ToString();
            }

            return (int)finalCritDamageRear;
        }

        // -------------------- BEFORE ARMOR --------------------

        // NON-CRIT HIT FRONT
        private int NonCritMaxDamageFront()
        {
            var nonCritDamageFront = 0.0f;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                var recklessnessMod = (int)trackBarMeleePowerBar.Value >= 10 && (int)trackBarMeleePowerBar.Value <= 90 ? RecklessnessMod() : 1;

                nonCritDamageFront = MaxDamage() * PowerBarMod() * (float)numericUpDownMeleeSlayer.Value * (FinalDamageRatingMod() + DeceptionMod() + recklessnessMod - 2) * AttributeMod();
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                var recklessnessMod = (int)trackBarMissileAccuracyBar.Value >= 10 && (int)trackBarMissileAccuracyBar.Value <= 90 ? RecklessnessMod() : 1;

                nonCritDamageFront = AmmoMaxDamageBuffed() * (float)numericUpDownMissileSlayer.Value * (FinalDamageRatingMod() + DeceptionMod() + recklessnessMod - 2) * AttributeMod();
            }
            else
            {
                nonCritDamageFront = (SpellMaxDamage() + AttributeMod()) * (float)numericUpDownMagicSlayer.Value * (1 + (float)BuffedElementalDamageBonus() / 100) * (FinalDamageRatingMod() + DeceptionMod() - 1);
            }

            return (int)nonCritDamageFront;
        }

        private int NonCritMinDamageFront()
        {
            var nonCritDamageFront = 0.0f;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                var recklessnessMod = (int)trackBarMeleePowerBar.Value >= 10 && (int)trackBarMeleePowerBar.Value <= 90 ? RecklessnessMod() : 1;

                nonCritDamageFront = MinDamage() * PowerBarMod() * (float)numericUpDownMeleeSlayer.Value * (FinalDamageRatingMod() + DeceptionMod() + recklessnessMod - 2) * AttributeMod();
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                var recklessnessMod = (int)trackBarMissileAccuracyBar.Value >= 10 && (int)trackBarMissileAccuracyBar.Value <= 90 ? RecklessnessMod() : 1;

                nonCritDamageFront = AmmoMinDamage() * (float)numericUpDownMissileSlayer.Value * (FinalDamageRatingMod() + DeceptionMod() + recklessnessMod - 2) * AttributeMod() * WeaponResistanceMod();
            }
            else
            {
                nonCritDamageFront = (SpellMinDamage() + AttributeMod()) * (float)numericUpDownMagicSlayer.Value * (1 + (float)BuffedElementalDamageBonus() / 100) * (FinalDamageRatingMod() + DeceptionMod() - 1);
            }

            return (int)nonCritDamageFront;
        }

        // NON-CRIT HIT REAR
        private int NonCritMaxDamageRear()
        {
            var nonCritDamageRear = 0.0f;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                var recklessnessMod = (int)trackBarMeleePowerBar.Value >= 10 && (int)trackBarMeleePowerBar.Value <= 90 ? RecklessnessMod() : 1;

                nonCritDamageRear = MaxDamage() * PowerBarMod() * (float)numericUpDownMeleeSlayer.Value * (FinalDamageRatingMod() + SneakAttackMod() + recklessnessMod - 2) * AttributeMod();
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                var recklessnessMod = (int)trackBarMissileAccuracyBar.Value >= 10 && (int)trackBarMissileAccuracyBar.Value <= 90 ? RecklessnessMod() : 1;

                nonCritDamageRear = AmmoMaxDamageBuffed() * (float)numericUpDownMissileSlayer.Value * (FinalDamageRatingMod() + SneakAttackMod() + recklessnessMod - 2) * AttributeMod();
            }
            else
            {
                nonCritDamageRear = (SpellMaxDamage() + AttributeMod()) * (float)numericUpDownMagicSlayer.Value * (1 + (float)BuffedElementalDamageBonus() / 100) * (FinalDamageRatingMod() + SneakAttackMod() - 1);
                //Console.WriteLine(nonCritDamageRear);
            }

            return (int)nonCritDamageRear;
        }

        private int NonCritMinDamageRear()
        {
            var nonCritDamageRear = 0.0f;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                var recklessnessMod = (int)trackBarMeleePowerBar.Value >= 10 && (int)trackBarMeleePowerBar.Value <= 90 ? RecklessnessMod() : 1;

                nonCritDamageRear = MinDamage() * PowerBarMod() * (float)numericUpDownMeleeSlayer.Value * (FinalDamageRatingMod() + SneakAttackMod() + recklessnessMod - 2) * AttributeMod();
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                var recklessnessMod = (int)trackBarMissileAccuracyBar.Value >= 10 && (int)trackBarMissileAccuracyBar.Value <= 90 ? RecklessnessMod() : 1;

                nonCritDamageRear = AmmoMinDamage() * (float)numericUpDownMissileSlayer.Value * (FinalDamageRatingMod() + SneakAttackMod() + recklessnessMod - 2) * AttributeMod();
            }
            else
            {
                nonCritDamageRear = (SpellMinDamage() + AttributeMod()) * (float)numericUpDownMagicSlayer.Value * (1 + (float)BuffedElementalDamageBonus() / 100) * (FinalDamageRatingMod() + SneakAttackMod() - 1);
            }

            return (int)nonCritDamageRear;
        }

        // CRIT HIT FRONT
        private int CritMaxDamageFront()
        {
            var critDamageFront = 0.0f;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                critDamageFront = MaxDamage() * PowerBarMod() * (float)numericUpDownMeleeSlayer.Value * (FinalCritDamageRatingMod() + DeceptionMod() - 1) * AttributeMod() * (FinalCritMultiplier() + 1);
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                critDamageFront = AmmoMaxDamageBuffed() * (float)numericUpDownMissileSlayer.Value * (FinalDamageRatingMod() + DeceptionMod() - 1) * AttributeMod() * (FinalCritMultiplier() + 1);
            }
            else
            {
                critDamageFront = (AvgDamage() + (SpellMaxDamage() * 0.5f * FinalCritMultiplier()) + AttributeMod()) * (1 + (float)BuffedElementalDamageBonus() / 100) *
                                       (float)numericUpDownMagicSlayer.Value * (FinalCritDamageRatingMod() + DeceptionMod() - 1);
            }

            return (int)critDamageFront;
        }

        // CRIT HIT REAR
        private int CritMaxDamageRear()
        {
            var critDamageRear = 0.0f;
            
            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                critDamageRear = MaxDamage() * PowerBarMod() * (float)numericUpDownMeleeSlayer.Value * (FinalCritDamageRatingMod() + SneakAttackMod() - 1) * MeleeMod() * (FinalCritMultiplier() + 1);
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                critDamageRear = AmmoMaxDamageBuffed() * (float)numericUpDownMissileSlayer.Value * (FinalDamageRatingMod() + SneakAttackMod() - 1) * MissileMod() * (FinalCritMultiplier() + 1);
            }
            else
            {
                critDamageRear = (AvgDamage() + (SpellMaxDamage() * 0.5f * FinalCritMultiplier()) + AttributeMod()) * (1 + (float)BuffedElementalDamageBonus() / 100) *
                                       (float)numericUpDownMagicSlayer.Value * (FinalCritDamageRatingMod() + SneakAttackMod() - 1);
            }

            return (int)critDamageRear;
        }

        #endregion

        #region Skill Calculations
        // ---------- SKILL CALCULATIONS ----------

        private int FinalMeleeAttackSkill()
        {
            var finalMeleeAttackSkill = EffectiveMeleeSkill() * EffectiveMeleeMod();

            labelEffectiveAttackSkill.Text = Math.Round(finalMeleeAttackSkill, 0).ToString();

            return (int)finalMeleeAttackSkill;
        }

        private float EffectiveMeleeMod()
        {
            var effectiveMeleeMod = (float)numericUpDownWeaponAttackMod.Value / 100 + HeartSeekerAmount() + HeartThirstAmount() + 1;

            return effectiveMeleeMod;
        }

        private int EffectiveMeleeSkill ()
        {
            var buffedMeleeSkill = Convert.ToInt32(numericUpDownBuffedMeleeSkill.Value);
            var rareBuffAmount = 0.0f;

            rareBuffAmount += checkBoxRareMelee.Checked ? 250 : 0;
            rareBuffAmount += checkBoxRareStrength.Checked ? 250 / 3 : 0;
            rareBuffAmount += checkBoxRareCoord.Checked ? 250 / 3 : 0;
            rareBuffAmount += checkBoxSurgeOfCloakedInSkill.Checked ? 20 : 0;

            int effectiveMeleeSkill = buffedMeleeSkill + (int)rareBuffAmount;

            labelCharacterEffectiveMeleeSkill.Text = effectiveMeleeSkill.ToString();

            return effectiveMeleeSkill;
        }

        private int FinalMissileAttackSkill()
        {
            var finalMissileAttackSkill = EffectiveMissileSkill() * EffectiveMissileMod();

            labelEffectiveAttackSkill.Text = Math.Round(finalMissileAttackSkill, 0).ToString();

            return (int)finalMissileAttackSkill;
        }

        private float EffectiveMissileMod()
        {
            var mod = 1 + ((float)trackBarMissileAccuracyBar.Value - 50) / 100;

            labelEffectiveAttackMod.Text = mod.ToString();

            return mod;
        }

        private int EffectiveMissileSkill()
        {
            var buffedMissileSkill = Convert.ToInt32(numericUpDownBuffedMissileSkill.Value);
            var rareBuffAmount = 0.0f;

            rareBuffAmount += checkBoxRareMissile.Checked ? 250 : 0;
            rareBuffAmount += checkBoxRareCoord.Checked ? 125 : 0;
            rareBuffAmount += checkBoxSurgeOfCloakedInSkill.Checked ? 20 : 0;

            int effectiveMissileSkill = buffedMissileSkill + (int)rareBuffAmount;

            labelCharacterEffectiveMissileSkill.Text = effectiveMissileSkill.ToString();

            return effectiveMissileSkill;
        }

        private int FinalMagicAttackSkill()
        {
            var finalMagicAttackSkill = EffectiveMagicSkill();

            labelEffectiveAttackSkill.Text = finalMagicAttackSkill.ToString();

            return finalMagicAttackSkill;
        }

        private int EffectiveMagicSkill()
        {
            var buffedMagicSkill = Convert.ToInt32(numericUpDownBuffedMagicSkill.Value);
            var rareBuffAmount = 0.0f;

            rareBuffAmount += checkBoxRareMagic.Checked ? 250 : 0;
            rareBuffAmount += checkBoxRareFocus.Checked ? 250 / 4 : 0;
            rareBuffAmount += checkBoxRareSelf.Checked ? 250 / 4 : 0;
            rareBuffAmount += checkBoxSurgeOfCloakedInSkill.Checked ? 20 : 0;

            int effectiveMagicSkill = buffedMagicSkill + (int)rareBuffAmount;

            labelCharacterEffectiveMagicSkill.Text = effectiveMagicSkill.ToString();

            return effectiveMagicSkill;
        }
        #endregion

        #region Enemy Defense Calculations

        private float FinalEnemyArmorMod()
        {
            var finalArmorMod = 1.0f;
            var enemyArmor = (float)numericUpDownEnemyArmor.Value;
            var finalArmor = 0.0f;
            var armorMod = WeaponArmorMod();

            if (enemyArmor - ArmorDebuffValue(comboBoxEnemyImperil) <= 0)
            {
                finalArmor = enemyArmor - ArmorDebuffValue(comboBoxEnemyImperil);
            }
            else
            {
                finalArmor = (enemyArmor - ArmorDebuffValue(comboBoxEnemyImperil)) * armorMod;
            }


            if (finalArmor > 0)
            {
                finalArmorMod = (200 / 3 / (finalArmor + 200 / 3));
            }
            else if (finalArmor < 0)
            {
                finalArmorMod = 1 - finalArmor / (200 / 3);
            }
            else
                finalArmorMod = 1;

            labelEnemyEffectiveArmor.Text = finalArmor.ToString();
            labelEnemyArmorMod.Text = Math.Round(finalArmorMod, 2).ToString();

            return finalArmorMod;
        }

        private float FinalEnemyShieldMod()
        {
            var shieldArmor = 0.0f;
            var shieldResist = 1.0f;

            // Get shield armor level
            if ((float)numericUpDownEnemyShieldAL.Value != 0)
            {
                shieldArmor = ((float)numericUpDownEnemyShieldAL.Value - ShieldArmorDebuffValue(comboBoxEnemyBrittlemail));
            }

            //Get shield resistance level
            if ((float)numericUpDownEnemyShieldResist.Value != 0)
            {
                shieldResist = ((float)numericUpDownEnemyShieldResist.Value - ShieldResistDebuffValue(comboBoxEnemyResistanceLure));
            }

            var finalShieldLevel = shieldArmor * shieldResist;
            var finalShieldMod = 1.0f;

            if (finalShieldLevel > 0)
            {
                finalShieldMod = (200 / 3 / (finalShieldLevel + 200 / 3));
            }
            else if (finalShieldLevel < 0)
            {
                finalShieldMod = 1 - finalShieldLevel / (200 / 3);
            }

            labelEnemyEffectiveShield.Text = finalShieldLevel.ToString();
            labelEnemyShieldMod.Text = Math.Round(finalShieldMod, 2).ToString();

            return finalShieldMod;
        }

        private float FinalEnemyResitanceVulnMod()
        {
            var finalResistanceVulnMod = Math.Max((float)numericUpDownEnemyResistance.Value * ResistanceVulnDebuffValue(comboBoxEnemyVuln),
                                              (float)numericUpDownEnemyResistance.Value * WeaponResistanceMod());

            labelEnemyEffectiveResistance.Text = Math.Round(finalResistanceVulnMod, 2).ToString();

            return finalResistanceVulnMod;
        }

        private int FinalEnemyMeleeDefense()
        {
            var debuffAmount = CreatureDebuffValue(comboBoxEnemyVulnerability) + (CreatureDebuffValue(comboBoxEnemyClumsiness) + CreatureDebuffValue(comboBoxEnemySlowness) / 3);

            if (comboBoxEnemyUnbalancingAssault.Text == "-10")
                debuffAmount += 10;
            else if (comboBoxEnemyUnbalancingAssault.Text == "-20")
                debuffAmount += 20;

            var finalDefense = (int)numericUpDownEnemyMeleeDefense.Value - debuffAmount;

            labelEnemyEffectiveMeleeDefense.Text = finalDefense.ToString();

            return finalDefense;
        }

        private int FinalEnemyMissileDefense()
        {
            var debuffAmount = CreatureDebuffValue(comboBoxEnemyDefenselessness) + (CreatureDebuffValue(comboBoxEnemyClumsiness) + CreatureDebuffValue(comboBoxEnemySlowness) / 5);

            if (comboBoxEnemyUnbalancingAssault.Text == "-10")
                debuffAmount += 10;
            else if (comboBoxEnemyUnbalancingAssault.Text == "-20")
                debuffAmount += 20;

            var finalDefense = (int)numericUpDownEnemyMissileDefense.Value - debuffAmount;

            labelEnemyEffectiveMissileDefense.Text = finalDefense.ToString();

            return finalDefense;
        }

        private int FinalEnemyMagicDefense()
        {
            var debuffAmount = CreatureDebuffValue(comboBoxEnemyMagicYield) + (CreatureDebuffValue(comboBoxEnemyBafflement) + CreatureDebuffValue(comboBoxEnemyFeeblemind) / 7);

            if (comboBoxEnemyUnbalancingAssault.Text == "-10")
                debuffAmount += 10;
            else if (comboBoxEnemyUnbalancingAssault.Text == "-20")
                debuffAmount += 20;

            var finalDefense = (int)numericUpDownEnemyMagicDefense.Value - debuffAmount;

            labelEnemyEffectiveMagicDefense.Text = finalDefense.ToString();

            return finalDefense;
        }

        private float SneakAttackMod()
        {
            var amount = 0.0f;
            var scale = 1.0f;
            var sneakAttack = (int)numericUpDownBuffedSneakAttack.Value;
            var rareAmount = 0.0f;

            rareAmount += checkBoxRareCoord.Checked ? 250 / 3 : 0;
            rareAmount += checkBoxRareQuick.Checked ? 250 / 3 : 0;
            rareAmount += checkBoxSurgeOfCloakedInSkill.Checked ? 20 : 0;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                scale = Math.Min((sneakAttack + rareAmount) / EffectiveMeleeSkill(), 1);
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                scale = Math.Min((sneakAttack + rareAmount) / EffectiveMissileSkill(), 1);
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Magic"])
            {
                scale = Math.Min((sneakAttack + rareAmount) / EffectiveMagicSkill(), 1);
            }

            if (comboBoxSneakAttack.Text == "Train")
            {
                amount = 10 * scale;
            }
            else if (comboBoxSneakAttack.Text == "Spec")
            {
                amount = 20 * scale;
            }

            labelCharacterSneakMod.Text = Math.Round(amount, 2).ToString();

            return amount / 100 + 1;
        }

        private float RecklessnessMod()
        {
            var amount = 0.0f;
            var scale = 1.0f;
            var recklessness = (int)numericUpDownBuffedRecklessness.Value;
            var rareAmount = 0.0f;

            rareAmount += checkBoxRareStrength.Checked ? 250 / 3 : 0;
            rareAmount += checkBoxRareQuick.Checked ? 250 / 3 : 0;
            rareAmount += checkBoxSurgeOfCloakedInSkill.Checked ? 20 : 0;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                scale = Math.Min((recklessness + rareAmount) / EffectiveMeleeSkill(), 1);
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                scale = Math.Min((recklessness + rareAmount) / EffectiveMissileSkill(), 1);
            }

            if (comboBoxRecklessness.Text == "Train")
            {
                amount = 10 * scale;
            }
            else if (comboBoxRecklessness.Text == "Spec")
            {
                amount = 20 * scale;
            }

            labelCharacterRecklessnessMod.Text = Math.Round(amount, 2).ToString();

            return amount / 100 + 1;
        }

        private float DeceptionChance()
        {
            var amount = 0.0f;
            var scale = Math.Min(1, (float)numericUpDownBuffedDeception.Value / 306);

            if (comboBoxDeception.Text == "Train")
            {
                amount = 0.1f * scale;
            }
            else if (comboBoxDeception.Text == "Spec")
            {
                amount = 0.15f * scale;
            }

            labelCharacterDeceptionMod.Text = Math.Round(amount*100, 0).ToString() + "%";

            return amount;
        }

        private float DeceptionMod()
        {
            var amount = SneakAttackMod() * DeceptionChance() + 1;

            return amount;
        }

        // Debuff Values

        private int ArmorDebuffValue(ComboBox comboBox)
        {
            var value = 0;

            if (comboBox.Text == "7")
                value = 225;
            else if (comboBox.Text == "6")
                value = 200;
            else if (comboBox.Text == "5")
                value = 150;
            else if (comboBox.Text == "4")
                value = 100;
            else if (comboBox.Text == "3")
                value = 75;
            else if (comboBox.Text == "2")
                value = 50;
            else if (comboBox.Text == "1")
                value = 20;

            return value;
        }

        private int ShieldArmorDebuffValue(ComboBox comboBox)
        {
            var value = 0;

            if (comboBox.Text == "7")
                value = 225;
            else if (comboBox.Text == "6")
                value = 200;
            else if (comboBox.Text == "5")
                value = 150;
            else if (comboBox.Text == "4")
                value = 100;
            else if (comboBox.Text == "3")
                value = 75;
            else if (comboBox.Text == "2")
                value = 50;
            else if (comboBox.Text == "1")
                value = 20;

            return value;
        }

        private float ShieldResistDebuffValue(ComboBox comboBox)
        {
            var value = 0.0f;

            if (comboBox.Text == "8")
                value = 2f;
            else if (comboBox.Text == "7")
                value = 1.7f;
            else if (comboBox.Text == "6")
                value = 1.5f;
            else if (comboBox.Text == "5")
                value = 1f;
            else if (comboBox.Text == "4")
                value = 0.75f;
            else if (comboBox.Text == "3")
                value = 0.5f;
            else if (comboBox.Text == "2")
                value = 0.25f;
            else if (comboBox.Text == "1")
                value = 0.1f;

            return value;
        }

        private float ResistanceVulnDebuffValue(ComboBox comboBox)
        {
            var value = 0.0f;

            if (comboBox.Text == "8")
                value = 3.1f;
            else if (comboBox.Text == "7")
                value = 2.85f;
            else if (comboBox.Text == "6")
                value = 2.5f;
            else if (comboBox.Text == "5")
                value = 2f;
            else if (comboBox.Text == "4")
                value = 1.75f;
            else if (comboBox.Text == "3")
                value = 1.5f;
            else if (comboBox.Text == "2")
                value = 1.25f;
            else if (comboBox.Text == "1")
                value = 1.1f;

            return value;
        }

        private int CreatureDebuffValue(ComboBox comboBox)
        {
            var value = 0;

            if (comboBox.Text == "8")
                value = 40;
            else if (comboBox.Text == "7")
                value = 35;
            else if (comboBox.Text == "6")
                value = 30;
            else if (comboBox.Text == "5")
                value = 25;
            else if (comboBox.Text == "4")
                value = 20;
            else if (comboBox.Text == "3")
                value = 15;
            else if (comboBox.Text == "2")
                value = 10;
            else if (comboBox.Text == "1")
                value = 5;

            return value;
        }

        #endregion

        #region Weapon Calculations
        
        private float PowerBarMod()
        {
            var mod = 1 + ((float)trackBarMeleePowerBar.Value - 50) / 100;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                labelPowerBarMod.Text = mod.ToString();
            }
            else
            {
                labelPowerBarMod.Text = "--";
            }

            return mod;
        }

        private float AvgDamage()
        {
            // Set initial values
            float minDamage;
            float maxDamage;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                minDamage = (int)numericUpDownWeaponMinDamage.Value;
                maxDamage = (int)numericUpDownWeaponMaxDamage.Value;
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                minDamage = AmmoMinDamage();
                maxDamage = AmmoMaxDamageBuffed();
            }
            else
            {
                minDamage = SpellMinDamage();
                maxDamage = SpellMaxDamage();
            }

            // Calculate average damage, including spell buffs
            float buffedMaxDamage; 
            float buffedMinDamage;
            float avgDamage; 

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                buffedMaxDamage = maxDamage + BloodDrinkerAmount() + BloodThirstAmount();
                buffedMinDamage = buffedMaxDamage * (minDamage / maxDamage);

                avgDamage = (buffedMaxDamage + buffedMinDamage) / 2;

                labelEffectiveAvgDamage.Text = Math.Round(avgDamage, 1).ToString();
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                buffedMaxDamage = maxDamage + BloodDrinkerAmount() + BloodThirstAmount();
                buffedMinDamage = buffedMaxDamage * (minDamage / maxDamage);

                avgDamage = (buffedMaxDamage + buffedMinDamage) / 2;

                labelEffectiveAvgDamage.Text = Math.Round(avgDamage, 1).ToString();
            }
            else
            {
                buffedMaxDamage = maxDamage;
                buffedMinDamage = buffedMaxDamage * (minDamage / maxDamage);

                avgDamage = (buffedMaxDamage + buffedMinDamage) / 2;

                labelEffectiveAvgDamage.Text = Math.Round(avgDamage, 1).ToString();
            }
            return avgDamage;
        }

        private float MaxDamage()
        {
            var buffedMaxDamage = (int)numericUpDownWeaponMaxDamage.Value + BloodDrinkerAmount() + BloodThirstAmount();

            labelEffectiveMaxDamage.Text = buffedMaxDamage.ToString();

            return buffedMaxDamage;
        }

        private float MinDamage()
        {
            var minDamage = 1.0f;
            var maxDamage = 1.0f;

            minDamage = (int)numericUpDownWeaponMinDamage.Value;
            maxDamage = (int)numericUpDownWeaponMaxDamage.Value;
           
            var buffedMaxDamage = maxDamage + BloodDrinkerAmount() + BloodThirstAmount();
            var buffedMinDamage = buffedMaxDamage * (minDamage / maxDamage);

            return buffedMinDamage;
        }

        private float AmmoMaxDamageBuffed()
        {
            var damage = 0.0f;
            var ammoType = comboBoxAmmoType.Text;

            if (comboBoxMissileAnimation.Text == "Bow")
            {
                if (ammoType == "32-40")
                {
                    damage = 40;
                }
                else if (ammoType == "28-40")
                {
                    damage = 40;
                }
            }
            else if (comboBoxMissileAnimation.Text == "Xbow")
            {
                if (ammoType == "37.1-53")
                {
                    damage = 53;
                }
                else if (ammoType == "31.8-53")
                {
                    damage = 53;
                }
            }
            else if (comboBoxMissileAnimation.Text == "Atlatl")
            {
                if (ammoType == "41.6-52")
                {
                    damage = 52;
                }
                else if (ammoType == "36.4-52")
                {
                    damage = 52;
                }
            }

            damage += (BloodDrinkerAmount() + BloodThirstAmount() + (int)numericUpDownMissileDamageBonus.Value) * ((float)numericUpDownMissileDamageMod.Value / 100 + 1);

            return damage;
        }
        private float AmmoMaxDamageBase()
        {
            var damage = 0.0f;
            var ammoType = comboBoxAmmoType.Text;

            if (comboBoxMissileAnimation.Text == "Bow")
            {
                if (ammoType == "32-40")
                {
                    damage = 40;
                }
                else if (ammoType == "28-40")
                {
                    damage = 40;
                }
            }
            else if (comboBoxMissileAnimation.Text == "Xbow")
            {
                if (ammoType == "37.1-53")
                {
                    damage = 53;
                }
                else if (ammoType == "31.8-53")
                {
                    damage = 53;
                }
            }
            else if (comboBoxMissileAnimation.Text == "Atlatl")
            {
                if (ammoType == "41.6-52")
                {
                    damage = 52;
                }
                else if (ammoType == "36.4-52")
                {
                    damage = 52;
                }
            }

            return damage;
        }

        private float AmmoMinDamage()
        {
            var damage = 0.0f;
            var ammoType = comboBoxAmmoType.Text;

            if (comboBoxMissileAnimation.Text == "Bow")
            {
                if (ammoType == "32-40")
                {
                    damage = 32.0f;
                }
                else if (ammoType == "28-40")
                {
                    damage = 28.0f;
                }
            }
            else if (comboBoxMissileAnimation.Text == "Crossbow")
            {
                if (ammoType == "37.1-53")
                {
                    damage = 37.1f;
                }
                else if (ammoType == "31.8-53")
                {
                    damage = 31.8f;
                }
            }
            else if (comboBoxMissileAnimation.Text == "Atlatl")
            {
                if (ammoType == "41.6-52")
                {
                    damage = 41.6f;
                }
                else if (ammoType == "36.4-52")
                {
                    damage = 36.4f;
                }
            }

            var variance = damage / AmmoMaxDamageBase();   
            var buffedMaxDamage = AmmoMaxDamageBuffed();                

            var buffedMinDamage = buffedMaxDamage * variance;

            return buffedMinDamage;
        }

        private int SpellMaxDamage()
        {
            var damage = 0;

            if (comboBoxMagicSpell.Text == "War 8")
            {
                damage = 204;
            }
            else if (comboBoxMagicSpell.Text == "Void 8")
            {
                damage = 325;
            }

            return damage;
        }

        private int SpellMinDamage()
        {
            var damage = 0;

            if (comboBoxMagicSpell.Text == "War 8")
            {
                damage = 142;
            }
            else if (comboBoxMagicSpell.Text == "Void 8")
            {
                damage = 252;
            }

            return damage;
        }

        private int BuffedElementalDamageBonus()
        {
            var amount = (int)numericUpDownMagicElementalDamageBonus.Value + SpiritDrinkerAmount() + SpiritThirstAmount();

            return amount;
        }

        private float HeartSeekerAmount()
        {
            var amount = 0.0f;

            if (comboBoxMeleeHeartSeeker.Text == "Level 8")
                amount = 20.0f;
            else if (comboBoxMeleeHeartSeeker.Text == "Level 7")
                amount = 17.0f;
            else if (comboBoxMeleeHeartSeeker.Text == "Level 6")
                amount = 15f;
            else if (comboBoxMeleeHeartSeeker.Text == "Level 5")
                amount = 12.5f;
            else if (comboBoxMeleeHeartSeeker.Text == "Level 4")
                amount = 10.0f;
            else if (comboBoxMeleeHeartSeeker.Text == "Level 3")
                amount = 7.5f;
            else if (comboBoxMeleeHeartSeeker.Text == "Level 2")
                amount = 5.0f;
            else if (comboBoxMeleeHeartSeeker.Text == "Level 1")
                amount = 2.5f;

            return amount / 100;
        }

        private float HeartThirstAmount()
        {
            var amount = 0.0f;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                if (comboBoxMeleeHeartThirst.Text == "Minor")
                    amount = 3;
                else if (comboBoxMeleeHeartThirst.Text == "Major")
                    amount = 5;
                else if (comboBoxMeleeHeartThirst.Text == "Epic")
                    amount = 7;
                else if (comboBoxMeleeHeartThirst.Text == "Legendary")
                    amount = 9;
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                if (comboBoxMeleeHeartThirst.Text == "Minor")
                    amount = 3;
                else if (comboBoxMeleeHeartThirst.Text == "Major")
                    amount = 5;
                else if (comboBoxMeleeHeartThirst.Text == "Epic")
                    amount = 7;
                else if (comboBoxMeleeHeartThirst.Text == "Legendary")
                    amount = 9;
            }
            return amount / 100;
        }

        private int BloodDrinkerAmount()
        {
            int amount = 0;
            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                if (comboBoxMeleeBloodDrinker.Text == "8")
                    amount = 24;
                else if (comboBoxMeleeBloodDrinker.Text == "7")
                    amount = 22;
                else if (comboBoxMeleeBloodDrinker.Text == "6")
                    amount = 20;
                else if (comboBoxMeleeBloodDrinker.Text == "5")
                    amount = 16;
                else if (comboBoxMeleeBloodDrinker.Text == "4")
                    amount = 12;
                else if (comboBoxMeleeBloodDrinker.Text == "3")
                    amount = 8;
                else if (comboBoxMeleeBloodDrinker.Text == "2")
                    amount = 4;
                else if (comboBoxMeleeBloodDrinker.Text == "1")
                    amount = 2;
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                if (comboBoxMissileBloodDrinker.Text == "8")
                    amount = 24;
                else if (comboBoxMissileBloodDrinker.Text == "7")
                    amount = 22;
                else if (comboBoxMissileBloodDrinker.Text == "6")
                    amount = 20;
                else if (comboBoxMissileBloodDrinker.Text == "5")
                    amount = 16;
                else if (comboBoxMissileBloodDrinker.Text == "4")
                    amount = 12;
                else if (comboBoxMissileBloodDrinker.Text == "3")
                    amount = 8;
                else if (comboBoxMissileBloodDrinker.Text == "2")
                    amount = 4;
                else if (comboBoxMissileBloodDrinker.Text == "1")
                    amount = 2;
            }

            return amount;
        }

        private int BloodThirstAmount()
        {
            var bloodThirst = 0;
            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                if (comboBoxMeleeBloodThirst.Text == "Minor")
                    bloodThirst = 3;
                else if (comboBoxMeleeBloodThirst.Text == "Major")
                    bloodThirst = 5;
                else if (comboBoxMeleeBloodThirst.Text == "Epic")
                    bloodThirst = 7;
                else if (comboBoxMeleeBloodThirst.Text == "Legendary")
                    bloodThirst = 10;
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                if (comboBoxMissileBloodThirst.Text == "Minor")
                    bloodThirst = 3;
                else if (comboBoxMissileBloodThirst.Text == "Major")
                    bloodThirst = 5;
                else if (comboBoxMissileBloodThirst.Text == "Epic")
                    bloodThirst = 7;
                else if (comboBoxMissileBloodThirst.Text == "Legendary")
                    bloodThirst = 10;
            }
                return bloodThirst;
        }

        private int SpiritDrinkerAmount()
        {
            var amount = 0;

            if (comboBoxMagicSpiritDrinker.Text == "8")
                amount = 8;
            else if (comboBoxMagicSpiritDrinker.Text == "7")
                amount = 7;
            else if (comboBoxMagicSpiritDrinker.Text == "6")
                amount = 6;
            else if (comboBoxMagicSpiritDrinker.Text == "5")
                amount = 5;
            else if (comboBoxMagicSpiritDrinker.Text == "4")
                amount = 4;
            else if (comboBoxMagicSpiritDrinker.Text == "3")
                amount = 3;
            else if (comboBoxMagicSpiritDrinker.Text == "2")
                amount = 2;
            else if (comboBoxMagicSpiritDrinker.Text == "1")
                amount = 1;

            return amount;
        }

        private int SpiritThirstAmount()
        {
            var amount = 0;

            if (comboBoxMagicSpiritThirst.Text == "Minor")
                amount = 1;
            else if (comboBoxMagicSpiritThirst.Text == "Major")
                amount = 3;
            else if (comboBoxMagicSpiritThirst.Text == "Epic")
                amount = 4;
            else if (comboBoxMagicSpiritThirst.Text == "Legendary")
                amount = 7;

            return amount;
        }
        private float WeaponArmorMod()
        {
            var armorRendMod = 1.0d;
            var armorCleavingMod = 1.0d;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                armorRendMod = checkBoxMeleeArmorRend.Checked ? Math.Max(0.4, 1 - (Math.Max(0, (float)numericUpDownBaseMeleeSkill.Value - 160)) / 400) : 1;
                armorCleavingMod = checkBoxMeleeArmorCleaving.Checked ? ArmorCleavingMod(comboBoxMeleeHighestSpell) : 1;
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                armorRendMod = checkBoxMissileArmorRend.Checked ? Math.Max(0.4, 1 - (Math.Max(0, (float)numericUpDownBaseMissileSkill.Value - 144)) / 360) : 1;
                armorCleavingMod = checkBoxMissileArmorCleaving.Checked ? ArmorCleavingMod(comboBoxMissileHighestSpell) : 1;
            }

            var armorMod = armorRendMod < armorCleavingMod ? armorRendMod : armorCleavingMod;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                labelEffectiveArmorMod.Text = Math.Round(armorMod, 2).ToString();
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                labelEffectiveArmorMod.Text = Math.Round(armorMod, 2).ToString();
            }
            else
            {
                labelEffectiveArmorMod.Text = "--";
            }
            
            return (float)armorMod;
        }

        private float ArmorCleavingMod(ComboBox highestSpell)
        {
            float value;

            if (highestSpell.Text == "Level 8")
                value = 0.5f;
            else if (highestSpell.Text == "Level 7")
                value = 0.55f;
            else if (highestSpell.Text == "Level 6")
                value = 0.6f;
            else if (highestSpell.Text == "Level 5")
                value = 0.65f;
            else if (highestSpell.Text == "Level 4")
                value = 0.7f;
            else if (highestSpell.Text == "Level 3")
                value = 0.75f;
            else if (highestSpell.Text == "Level 2")
                value = 0.8f;
            else if (highestSpell.Text == "Level 1")
                value = 0.85f;
            else
                value = 0.9f;

            return value;
        }

        private float WeaponResistanceMod()
        {
            var resistanceRendMod = 0.0d;
            var resistanceCleavingMod = 0.0d;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                resistanceRendMod = checkBoxMeleeResistanceRend.Checked ? Math.Min(2.5, (float)numericUpDownBaseMeleeSkill.Value / 160) : 1;
                resistanceCleavingMod = (float)numericUpDownMeleeResistanceCleaving.Value;
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                resistanceRendMod = checkBoxMissileResistanceRend.Checked ? Math.Min(2.5, (float)numericUpDownBaseMissileSkill.Value / 144) : 1;
                resistanceCleavingMod = (float)numericUpDownMissileResistanceCleaving.Value;
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Magic"])
            {
                resistanceRendMod = checkBoxMagicResistanceRend.Checked ? Math.Min(2.5, (float)numericUpDownBaseMagicSkill.Value / 144) : 1;
                resistanceCleavingMod = (float)numericUpDownMagicResistanceCleaving.Value;
            }

            var resistMod = resistanceRendMod > resistanceCleavingMod ? resistanceRendMod : resistanceCleavingMod;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                labelEffectiveResistMod.Text = Math.Round(resistMod, 2).ToString();
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                labelEffectiveResistMod.Text = Math.Round(resistMod, 2).ToString();
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Magic"])
            {
                labelEffectiveResistMod.Text = Math.Round(resistMod, 2).ToString();
            }

            return (float)resistMod;
        }

        private float FinalCritChance()
        {
            var criticalStrikeCritChance = 0.0d;
            var bitingStrikeCritChance = 0.0d;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                criticalStrikeCritChance = checkBoxMeleeCriticalStrike.Checked ? Math.Min(0.5, Math.Max(((float)numericUpDownBaseMeleeSkill.Value - 100) / 600, 0.1)) : 0.1;
                bitingStrikeCritChance = (float)numericUpDownMeleeBitingStrike.Value;
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                criticalStrikeCritChance = checkBoxMissileCriticalStrike.Checked ? Math.Min(0.5, Math.Max(((float)numericUpDownBaseMissileSkill.Value - 60) / 600, 0.1)) : 0.1;
                bitingStrikeCritChance = (float)numericUpDownMissileBitingStrike.Value;
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Magic"])
            {
                criticalStrikeCritChance = checkBoxMagicCriticalStrike.Checked ? Math.Min(0.5, Math.Max(((float)numericUpDownBaseMagicSkill.Value - 60) / 600, 0.05)) : 0.05;
                bitingStrikeCritChance = (float)numericUpDownMagicBitingStrike.Value;
            }

            criticalStrikeCritChance += +(float)numericUpDownCritChanceRating.Value / 100;
            bitingStrikeCritChance += +(float)numericUpDownCritChanceRating.Value / 100;

            var critChance = criticalStrikeCritChance > bitingStrikeCritChance ? criticalStrikeCritChance : bitingStrikeCritChance;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                labelEffectiveCritChance.Text = Math.Round(critChance * 100, 0).ToString() + "%";
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                labelEffectiveCritChance.Text = Math.Round(critChance * 100, 0).ToString() + "%";
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Magic"])
            {
                labelEffectiveCritChance.Text = Math.Round(critChance * 100, 0).ToString() + "%";
            }

            return (float)Math.Round(critChance, 2) + (float)numericUpDownCritChanceRating.Value / 100;
        }

        private float FinalCritMultiplier()
        {
            var cripplingBlowMultiplier = 1.0f;
            var crushingBlowMultiplier = 1.0f;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                cripplingBlowMultiplier = checkBoxMeleeCripplingBlow.Checked ? Math.Min(6, ((float)numericUpDownBaseMeleeSkill.Value - 40) / 60) : 1;
                crushingBlowMultiplier = (float)numericUpDownMeleeCrushingBlow.Value;
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                cripplingBlowMultiplier = checkBoxMissileCripplingBlow.Checked ? Math.Min(6, ((float)numericUpDownBaseMissileSkill.Value) / 60) : 1;
                crushingBlowMultiplier = (float)numericUpDownMissileCrushingBlow.Value;
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Magic"])
            {
                cripplingBlowMultiplier = checkBoxMagicCripplingBlow.Checked ? Math.Min(6, ((float)numericUpDownBaseMagicSkill.Value) / 60) : 1;
                crushingBlowMultiplier = (float)numericUpDownMagicCrushingBlow.Value;
            }

            var critMultiplier = cripplingBlowMultiplier > crushingBlowMultiplier ? cripplingBlowMultiplier : crushingBlowMultiplier;

            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                labelEffectiveCritMultiplier.Text = Math.Round(critMultiplier, 2).ToString();
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                labelEffectiveCritMultiplier.Text = Math.Round(critMultiplier, 2).ToString();
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Magic"])
            {
                labelEffectiveCritMultiplier.Text = Math.Round(critMultiplier, 2).ToString();
            }

            return (float)critMultiplier;
        }

        private float FinalDamageRatingVoidMod()
        {
            return (1 / FinalDamageRatingMod()) * (FinalDamageRatingMod() + (VoidDotDamageRating() / 100));
        }

        private int FinalDamageRating()
        {
            var rating = (int)numericUpDownDamageRating.Value;

            rating += checkBoxSurgeOfDestruction.Checked ? 20 : 0;

            //rating += (int)VoidDotDamageRating();

            return rating;
        }

        private float FinalDamageRatingMod()
        {
            var mod = 1 + (float)FinalDamageRating() / 100;
            return mod;
        }

        private int FinalCritDamageRating()
        {
            var rating = (int)numericUpDownCritDamageRating.Value + FinalDamageRating();

            return rating;
        }

        private float FinalCritDamageRatingMod()
        {
            var mod = 1 + (float)FinalCritDamageRating() / 100;

            return mod;
        }

        private float VoidDotDamageRating()
        {
            var dotAmount = (float)numericUpDownCorrosion.Value + (float)numericUpDownCorruption.Value + (float)numericUpDownDestructive.Value;

            var rating = dotAmount / 8;

            labelEnemyEffectiveVoidRating.Text = Math.Round(rating, 0).ToString();

            return rating;
        }

        private float AttributeMod()
        {
            var meleeMod = MeleeMod();
            var missileMod = MissileMod();
            var magicMod = MagicMod();

            float activeMod;
            if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Melee"])
            {
                activeMod = meleeMod;
            }
            else if (tabControlWeaponType.SelectedTab == tabControlWeaponType.TabPages["Missile"])
            {
                activeMod = missileMod;
            }
            else
            {
                activeMod = magicMod;
            }

            return activeMod;
        }

        private float MeleeMod()
        {
            var mod = 1.0f;

            if (checkBoxFinesse.Checked)
            {
                    mod = 1 + ((int)numericUpDownBuffedCoordination.Value + (checkBoxRareCoord.Checked ? 250 : 0) - 55) * 0.011f;
            }
            else
            {
                    mod = 1 + ((int)numericUpDownBuffedStrength.Value + (checkBoxRareStrength.Checked ? 250 : 0) - 55) * 0.011f;
            }

            labelCharacterMeleeMod.Text = Math.Round(mod, 1).ToString();

            return mod;
        }

        private float MissileMod()
        {
            var mod = 1.0f;

            if (checkBoxThrown.Checked)
            {
                mod = 1 + ((int)numericUpDownBuffedStrength.Value + (checkBoxRareStrength.Checked ? 250 : 0) - 55) * 0.008f;
            }
            else
            {
                mod = 1 + ((int)numericUpDownBuffedCoordination.Value + (checkBoxRareCoord.Checked ? 250 : 0) - 55) * 0.008f;
            }

            labelCharacterMissileMod.Text = Math.Round(mod, 1).ToString();

            return mod;
        }

        private float MagicMod()
        {
            float mod = 1 + ((float)(EffectiveMagicSkill() - 350) / 1000) * SpellMinDamage();
            labelCharacterSpellMod.Text = Math.Round(mod, 1).ToString();

            return mod;
        }


        #endregion

        private void BtnSetCompare(object sender, EventArgs e)
        {
            labelQuickCompareFront.Text = labelFinalDpsFront.Text;
            labelQuickCompareRear.Text = labelFinalDpsRear.Text;
        }

        private void QuickCompare()
        {
            var quickFront = Convert.ToInt32(labelQuickCompareFront.Text);
            var quickRear = Convert.ToInt32(labelQuickCompareRear.Text);
            var currentDpsFront = Convert.ToInt32(labelFinalDpsFront.Text);
            var currentDpsRear = Convert.ToInt32(labelFinalDpsRear.Text);

            var differenceFront = 0.0f;
            var differenceRear = 0.0f;

            if (quickFront != 0 && quickRear != 0)
            {
                differenceFront = (float)currentDpsFront / quickFront * 100 - 100;
                differenceRear = (float)currentDpsRear / quickRear * 100 - 100;
            }

            if (differenceFront > 0)
            {
                labelQuickCompareDifferenceFront.Text = "+" + Math.Round(differenceFront, 2).ToString() + "%";
                labelQuickCompareDifferenceRear.Text = "+" + Math.Round(differenceRear, 2).ToString() + "%";
            }
            else
            {
                labelQuickCompareDifferenceFront.Text = Math.Round(differenceFront, 2).ToString() + "%";
                labelQuickCompareDifferenceRear.Text = Math.Round(differenceRear, 2).ToString() + "%";
            }
        }

        private void SetPowerBarValue(object sender, EventArgs e)
        {
            labelPowerBarValue.Text = trackBarMeleePowerBar.Value.ToString();

            CalculateFinalDps(sender, e);
        }

        private void SetAccuracyBarValue(object sender, EventArgs e)
        {
            labelAccuracyBarValue.Text = trackBarMissileAccuracyBar.Value.ToString();

            CalculateFinalDps(sender, e);
        }

        private void SetAmmoType(object sender, EventArgs e)
        {
            if(comboBoxMissileAnimation.Text == "Bow")
            {
                comboBoxAmmoType.Items.Clear();
                comboBoxAmmoType.Items.Add("32-40");
                comboBoxAmmoType.Items.Add("28-40");
                comboBoxAmmoType.Text = "32-40";
            }
            else if (comboBoxMissileAnimation.Text == "Xbow")
            {
                comboBoxAmmoType.Items.Clear();
                comboBoxAmmoType.Items.Add("37.1-53");
                comboBoxAmmoType.Items.Add("31.9-53");
                comboBoxAmmoType.Text = "37.1-53";
            }
            else
            {
                comboBoxAmmoType.Items.Clear();
                comboBoxAmmoType.Items.Add("41.6-52");
                comboBoxAmmoType.Items.Add("36.4-52");
                comboBoxAmmoType.Text = "41.6-52";
            }

            CalculateFinalDps(sender, e);
        }

        #region Tool Interactions
        private void OnAdjustBaseWeaponSkills(object sender, EventArgs e)
        {
            if(numericUpDownBaseMeleeSkill.Value > numericUpDownBuffedMeleeSkill.Value)
            {
                numericUpDownBuffedMeleeSkill.Value = numericUpDownBaseMeleeSkill.Value;
            }

            if (numericUpDownBaseMissileSkill.Value > numericUpDownBuffedMissileSkill.Value)
            {
                numericUpDownBuffedMissileSkill.Value = numericUpDownBaseMissileSkill.Value;
            }

            if (numericUpDownBaseMagicSkill.Value > numericUpDownBuffedMagicSkill.Value)
            {
                numericUpDownBuffedMagicSkill.Value = numericUpDownBaseMagicSkill.Value;
            }

            CalculateFinalDps(sender, e);
        }

        private void OnAdjustBuffedWeaponSkills(object sender, EventArgs e)
        {
            if (numericUpDownBaseMeleeSkill.Value > numericUpDownBuffedMeleeSkill.Value)
            {
                numericUpDownBaseMeleeSkill.Value = numericUpDownBuffedMeleeSkill.Value;
            }

            if (numericUpDownBaseMissileSkill.Value > numericUpDownBuffedMissileSkill.Value)
            {
                numericUpDownBaseMissileSkill.Value = numericUpDownBuffedMissileSkill.Value;
            }

            if (numericUpDownBaseMagicSkill.Value > numericUpDownBuffedMagicSkill.Value)
            {
                numericUpDownBaseMagicSkill.Value = numericUpDownBuffedMagicSkill.Value;
            }

            CalculateFinalDps(sender, e);
        }

        private void OnAdjustMinWeaponDamage(object sender, EventArgs e)
        {
            if(numericUpDownWeaponMinDamage.Value > numericUpDownWeaponMaxDamage.Value)
            {
                numericUpDownWeaponMaxDamage.Value = numericUpDownWeaponMinDamage.Value;

                CalculateFinalDps(sender, e);
            }
        }

        private void OnAdjustMaxWeaponDamage(object sender, EventArgs e)
        {
            if (numericUpDownWeaponMinDamage.Value > numericUpDownWeaponMaxDamage.Value)
            {
                numericUpDownWeaponMinDamage.Value = numericUpDownWeaponMaxDamage.Value;

                CalculateFinalDps(sender, e);
            }
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupBox11_Enter(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void labelFinalNonCritRear_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel14_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label5_Click_1(object sender, EventArgs e)
        {

        }

        private void label143_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel24_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_2(object sender, EventArgs e)
        {

        }

        private void label111_Click(object sender, EventArgs e)
        {

        }

        private void label79_Click(object sender, EventArgs e)
        {

        }

        private void label64_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label26_Click(object sender, EventArgs e)
        {

        }

        private void label27_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel12_Paint(object sender, PaintEventArgs e)
        {

        }
        #endregion



        private void menuItemSaveCharacter_Click(object sender, EventArgs e)
        {
            if(textBoxCharacterName.Text == "")
            {
                MessageBox.Show("Please enter a name for your character before saving.", "Name Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxCharacterName.Select();
                return;
            }


            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + @"\Character";
            saveFileDialog1.RestoreDirectory = false;
            saveFileDialog1.Filter = "JSON|*.json";
            saveFileDialog1.Title = "Save Character Template";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                Character character = new Character
                {
                    Name = textBoxCharacterName.Text,
                    FinesseWeapon = checkBoxFinesse.Checked,
                    ThrownWeapon = checkBoxThrown.Checked,
                    MeleeBase = (int)numericUpDownBaseMeleeSkill.Value,
                    MeleeBuffed = (int)numericUpDownBuffedMeleeSkill.Value,
                    MissileBase = (int)numericUpDownBaseMissileSkill.Value,
                    MissileBuffed = (int)numericUpDownBuffedMissileSkill.Value,
                    MagicBase = (int)numericUpDownBaseMagicSkill.Value,
                    MagicBuffed = (int)numericUpDownBuffedMagicSkill.Value,
                    Strength = (int)numericUpDownBuffedStrength.Value,
                    Coordination = (int)numericUpDownBuffedCoordination.Value,
                    Quickness = (int)numericUpDownBuffedQuickness.Value,
                    Recklessness = (int)numericUpDownBuffedRecklessness.Value,
                    SneakAttack = (int)numericUpDownBuffedSneakAttack.Value,
                    Deception = (int)numericUpDownBuffedDeception.Value,
                    RecklessnessComboBox = comboBoxRecklessness.Text,
                    SneakAttackComboBox = comboBoxSneakAttack.Text,
                    DeceptionComboBox = comboBoxDeception.Text,
                    StrengthRare = checkBoxRareStrength.Checked,
                    CoordinationRare = checkBoxRareCoord.Checked,
                    QuicknessRare = checkBoxRareQuick.Checked,
                    FocusRare = checkBoxRareFocus.Checked,
                    SelfRare = checkBoxRareSelf.Checked,
                    MeleeRare = checkBoxRareMelee.Checked,
                    MissileRare = checkBoxRareMissile.Checked,
                    MagicRare = checkBoxRareMagic.Checked,
                    CisSurge = checkBoxSurgeOfCloakedInSkill.Checked,
                    DestSurge = checkBoxSurgeOfDestruction.Checked
                };

                // serialize JSON directly to a file
                using (StreamWriter file = File.CreateText(saveFileDialog1.FileName))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, character);
                }
            }
        }

        
    private void menuItemLoadCharacter_Click(object sender, EventArgs e)
    {

        // Displays a OpenFileDialog so the user can save the Image
        // assigned to Button2.
        OpenFileDialog openFileDialog1 = new OpenFileDialog();
        openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + @"\Character";
        openFileDialog1.Filter = "JSON|*.json";
        openFileDialog1.Title = "Open Character Template";
        openFileDialog1.ShowDialog();

        // If the file name is not an empty string open it for saving.
        if (openFileDialog1.FileName != "")
        {
            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText(openFileDialog1.FileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                Character character = (Character)serializer.Deserialize(file, typeof(Character));

                textBoxCharacterName.Text = character.Name;
                checkBoxFinesse.Checked = character.FinesseWeapon;
                checkBoxThrown.Checked = character.ThrownWeapon;
                numericUpDownBaseMeleeSkill.Value = character.MeleeBase;
                numericUpDownBuffedMeleeSkill.Value = character.MeleeBuffed;
                numericUpDownBaseMissileSkill.Value = character.MissileBase;
                numericUpDownBuffedMissileSkill.Value = character.MissileBuffed;
                numericUpDownBaseMagicSkill.Value = character.MagicBase;
                numericUpDownBuffedMagicSkill.Value = character.MagicBuffed;
                numericUpDownBuffedStrength.Value = character.Strength;
                numericUpDownBuffedCoordination.Value = character.Coordination;
                numericUpDownBuffedQuickness.Value = character.Quickness;
                numericUpDownBuffedRecklessness.Value = character.Recklessness;
                numericUpDownBuffedSneakAttack.Value = character.SneakAttack;
                numericUpDownBuffedDeception.Value = character.Deception;
                comboBoxRecklessness.Text = character.RecklessnessComboBox;
                comboBoxSneakAttack.Text = character.SneakAttackComboBox;
                comboBoxDeception.Text = character.DeceptionComboBox;
                checkBoxRareStrength.Checked = character.StrengthRare;
                checkBoxRareCoord.Checked = character.CoordinationRare;
                checkBoxRareQuick.Checked = character.QuicknessRare;
                checkBoxRareFocus.Checked = character.FocusRare;
                checkBoxRareSelf.Checked = character.SelfRare;
                checkBoxRareMelee.Checked = character.MeleeRare;
                checkBoxRareMissile.Checked = character.MissileRare;
                checkBoxRareMagic.Checked = character.MagicRare;
                checkBoxSurgeOfCloakedInSkill.Checked = character.CisSurge;
                checkBoxSurgeOfDestruction.Checked = character.DestSurge;
            }
        }
    }

        private void menuItemResetCharacter_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("Are you sure want to reset Character settings?",
                                        "Confirm Character Reset",
                                        MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.No)
            {
                return;
            }

            textBoxCharacterName.Text = "";
            checkBoxFinesse.Checked = false;
            checkBoxThrown.Checked = false;
            numericUpDownBaseMeleeSkill.Value = 10;
            numericUpDownBuffedMeleeSkill.Value = 10;
            numericUpDownBaseMissileSkill.Value = 10;
            numericUpDownBuffedMissileSkill.Value = 10;
            numericUpDownBaseMagicSkill.Value = 10;
            numericUpDownBuffedMagicSkill.Value = 10;
            numericUpDownBuffedStrength.Value = 10;
            numericUpDownBuffedCoordination.Value = 10;
            numericUpDownBuffedQuickness.Value = 10;
            numericUpDownBuffedRecklessness.Value = 10;
            numericUpDownBuffedSneakAttack.Value = 10;
            numericUpDownBuffedDeception.Value = 10;
            comboBoxRecklessness.Text = "None";
            comboBoxSneakAttack.Text = "None";
            comboBoxDeception.Text = "None";
            checkBoxRareStrength.Checked = false;
            checkBoxRareCoord.Checked = false;
            checkBoxRareQuick.Checked = false;
            checkBoxRareFocus.Checked = false;
            checkBoxRareSelf.Checked = false;
            checkBoxRareMelee.Checked = false;
            checkBoxRareMissile.Checked = false;
            checkBoxRareMagic.Checked = false;
            checkBoxSurgeOfCloakedInSkill.Checked = false;
            checkBoxSurgeOfDestruction.Checked = false;
        }
        private void menuItemSaveWeapon_Click(object sender, EventArgs e)
        {
            if (textBoxWeaponName.Text == "")
            {
                MessageBox.Show("Please enter a name for your weapon before saving.", "Name Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxWeaponName.Select();
                return;
            }


            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            SaveFileDialog saveFileDialog2 = new SaveFileDialog();
            saveFileDialog2.InitialDirectory = Directory.GetCurrentDirectory() + @"\Weapon";
            saveFileDialog2.RestoreDirectory = false;
            saveFileDialog2.Filter = "JSON|*.json";
            saveFileDialog2.Title = "Save Weapon Template";
            saveFileDialog2.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog2.FileName != "")
            {
                Weapon weapon = new Weapon
                {
                    Name = textBoxWeaponName.Text,
                    MeleeAttackAnimation = comboBoxMeleeAnimation.Text,
                    MeleeMinDamage = (float)numericUpDownWeaponMinDamage.Value,
                    MeleeMaxDamage = (float)numericUpDownWeaponMaxDamage.Value,
                    MeleeAttackMod = (float)numericUpDownWeaponAttackMod.Value,
                    MeleeSpeed = (int)numericUpDownMeleeWeaponSpeed.Value,
                    MissileAmmoType = comboBoxAmmoType.Text,
                    MissileDamageMod = (int)numericUpDownMissileDamageMod.Value,
                    MissileDamageBonus = (int)numericUpDownMissileDamageBonus.Value,
                    MissileSpeed = (int)numericUpDownMissileWeaponSpeed.Value,
                    MissileAttackAnimation = comboBoxMissileAnimation.Text,
                    MagicAttackAnimation = comboBoxMagicAnimation.Text,
                    MagicSpell = comboBoxMagicSpell.Text,
                    MagicDamageBonus = (int)numericUpDownMagicElementalDamageBonus.Value,
                    MeleeArmorRend = checkBoxMeleeArmorRend.Checked,
                    MeleeArmorCleave = checkBoxMeleeArmorCleaving.Checked,
                    MeleeResistanceRend = checkBoxMeleeResistanceRend.Checked,
                    MeleeResistanceCleave = (float)numericUpDownMeleeResistanceCleaving.Value,
                    MeleeCriticalStrike = checkBoxMeleeCriticalStrike.Checked,
                    MeleeBitingStrike = numericUpDownMeleeBitingStrike.Value,
                    MeleeCripplingBlow = checkBoxMeleeCripplingBlow.Checked,
                    MeleeCrushingBlow = numericUpDownMeleeCrushingBlow.Value,
                    MissileArmorRend = checkBoxMissileArmorRend.Checked,
                    MissileArmorCleave = checkBoxMissileArmorCleaving.Checked,
                    MissileResistanceRend = checkBoxMissileResistanceRend.Checked,
                    MissileResistanceCleave = (float)numericUpDownMissileResistanceCleaving.Value,
                    MissileCriticalStrike = checkBoxMissileCriticalStrike.Checked,
                    MissileBitingStrike = numericUpDownMissileBitingStrike.Value,
                    MissileCripplingBlow = checkBoxMissileCripplingBlow.Checked,
                    MissileCrushingBlow = numericUpDownMissileCrushingBlow.Value,
                    MagicResistanceRend = checkBoxMagicResistanceRend.Checked,
                    MagicResistanceCleave = (float)numericUpDownMagicResistanceCleaving.Value,
                    MagicCriticalStrike = checkBoxMagicCriticalStrike.Checked,
                    MagicBitingStrike = numericUpDownMagicBitingStrike.Value,
                    MagicCripplingBlow = checkBoxMagicCripplingBlow.Checked,
                    MagicCrushingBlow = numericUpDownMagicCrushingBlow.Value,
                    MeleeBloodDrinker = comboBoxMeleeBloodDrinker.Text,
                    MeleeBloodThirst = comboBoxMeleeBloodThirst.Text,
                    MeleeHeartSeeker = comboBoxMeleeHeartSeeker.Text,
                    MeleeHeartThirst = comboBoxMeleeHeartThirst.Text,
                    MeleeHighestSpell = comboBoxMeleeHighestSpell.Text,
                    MeleeSlayer = numericUpDownMeleeSlayer.Value,
                    MissileBloodDrinker = comboBoxMissileBloodDrinker.Text,
                    MissileBloodThirst = comboBoxMissileBloodThirst.Text,
                    MissileHighestSpell = comboBoxMissileHighestSpell.Text,
                    MissileSlayer = numericUpDownMissileSlayer.Value,
                    MagicSpiritDrinker = comboBoxMagicSpiritDrinker.Text,
                    MagicSpiritThirst = comboBoxMagicSpiritThirst.Text,
                    MagicSlayer = numericUpDownMagicSlayer.Value,
                    MeleePowerBar = trackBarMeleePowerBar.Value,
                    MissileAccuracyBar = trackBarMissileAccuracyBar.Value
                };

                // serialize JSON directly to a file
                using (StreamWriter file = File.CreateText(saveFileDialog2.FileName))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, weapon);
                }
            }
        }
        private void menuItemLoadWeapon_Click(object sender, EventArgs e)
        {
            // Displays a OpenFileDialog so the user can save the Image
            // assigned to Button2.
            OpenFileDialog openFileDialog2 = new OpenFileDialog();
            openFileDialog2.InitialDirectory = Directory.GetCurrentDirectory() + @"\Weapon";
            openFileDialog2.Filter = "JSON|*.json";
            openFileDialog2.Title = "Open Character Template";
            openFileDialog2.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (openFileDialog2.FileName != "")
            {
                // deserialize JSON directly from a file
                using (StreamReader file = File.OpenText(openFileDialog2.FileName))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    Weapon weapon = (Weapon)serializer.Deserialize(file, typeof(Weapon));

                    textBoxWeaponName.Text = weapon.Name;
                    comboBoxMeleeAnimation.Text = weapon.MeleeAttackAnimation;
                    numericUpDownWeaponMinDamage.Value = (decimal)weapon.MeleeMinDamage;
                    numericUpDownWeaponMaxDamage.Value = (decimal)weapon.MeleeMaxDamage;
                    numericUpDownWeaponAttackMod.Value = (decimal)weapon.MeleeAttackMod;
                    numericUpDownMeleeWeaponSpeed.Value = weapon.MeleeSpeed;
                    comboBoxMissileAnimation.Text = weapon.MissileAttackAnimation;
                    comboBoxAmmoType.Text = weapon.MissileAmmoType;
                    numericUpDownMissileDamageMod.Value = weapon.MissileDamageMod;
                    numericUpDownMissileDamageBonus.Value = weapon.MissileDamageBonus;
                    numericUpDownMissileWeaponSpeed.Value = weapon.MissileSpeed;
                    comboBoxMagicAnimation.Text = weapon.MagicAttackAnimation;
                    comboBoxMagicSpell.Text = weapon.MagicSpell;
                    numericUpDownMagicElementalDamageBonus.Value = weapon.MagicDamageBonus;
                    checkBoxMeleeArmorRend.Checked = weapon.MeleeArmorRend;
                    checkBoxMeleeArmorCleaving.Checked = weapon.MeleeArmorCleave;
                    checkBoxMeleeResistanceRend.Checked = weapon.MeleeResistanceRend;
                    numericUpDownMeleeResistanceCleaving.Value = (decimal)weapon.MeleeResistanceCleave;
                    checkBoxMeleeCriticalStrike.Checked = weapon.MeleeCriticalStrike;
                    numericUpDownMeleeBitingStrike.Value = weapon.MeleeBitingStrike;
                    checkBoxMeleeCripplingBlow.Checked = weapon.MeleeCripplingBlow;
                    numericUpDownMeleeCrushingBlow.Value = weapon.MeleeCrushingBlow;
                    checkBoxMissileArmorRend.Checked = weapon.MissileArmorRend;
                    checkBoxMissileArmorCleaving.Checked = weapon.MissileArmorCleave;
                    checkBoxMissileResistanceRend.Checked = weapon.MissileResistanceRend;
                    numericUpDownMissileResistanceCleaving.Value = (decimal)weapon.MeleeResistanceCleave;
                    checkBoxMissileCriticalStrike.Checked = weapon.MissileCriticalStrike;
                    numericUpDownMissileBitingStrike.Value = weapon.MissileBitingStrike;
                    checkBoxMissileCripplingBlow.Checked = weapon.MissileCripplingBlow;
                    numericUpDownMissileCrushingBlow.Value = weapon.MissileCrushingBlow;
                    checkBoxMagicResistanceRend.Checked = weapon.MagicResistanceRend;
                    numericUpDownMagicResistanceCleaving.Value = (decimal)weapon.MagicResistanceCleave;
                    checkBoxMagicCriticalStrike.Checked = weapon.MagicCriticalStrike;
                    numericUpDownMagicBitingStrike.Value = weapon.MagicBitingStrike;
                    checkBoxMagicCripplingBlow.Checked = weapon.MagicCripplingBlow;
                    numericUpDownMagicCrushingBlow.Value = weapon.MagicCrushingBlow;
                    comboBoxMeleeBloodDrinker.Text = weapon.MeleeBloodDrinker;
                    comboBoxMeleeBloodThirst.Text = weapon.MeleeBloodThirst;
                    comboBoxMeleeHeartSeeker.Text = weapon.MeleeHeartSeeker;
                    comboBoxMeleeHeartThirst.Text = weapon.MeleeHeartThirst;
                    comboBoxMeleeHighestSpell.Text = weapon.MeleeHighestSpell;
                    numericUpDownMeleeSlayer.Value = weapon.MeleeSlayer;
                    comboBoxMissileBloodDrinker.Text = weapon.MissileBloodDrinker;
                    comboBoxMissileBloodThirst.Text = weapon.MissileBloodThirst;
                    numericUpDownMissileSlayer.Value = weapon.MissileSlayer;
                    comboBoxMagicSpiritDrinker.Text = weapon.MagicSpiritDrinker;
                    comboBoxMagicSpiritThirst.Text = weapon.MagicSpiritThirst;
                    numericUpDownMagicSlayer.Value = weapon.MagicSlayer;
                    trackBarMeleePowerBar.Value = weapon.MeleePowerBar;
                    trackBarMissileAccuracyBar.Value = weapon.MissileAccuracyBar;
                }
            }
        }

        private void menuItemResetWeapon_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("Are you sure want to reset Weapon settings?",
                                        "Confirm Weapon Reset",
                                        MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.No)
            {
                return;
            }

            textBoxWeaponName.Text = "";
            comboBoxMeleeAnimation.Text = "Slash";
            numericUpDownWeaponMinDamage.Value = 1;
            numericUpDownWeaponMaxDamage.Value = 1;
            numericUpDownWeaponAttackMod.Value = 0;
            numericUpDownMeleeWeaponSpeed.Value = 0;
            comboBoxMissileAnimation.Text = "Bow";
            comboBoxAmmoType.Text = "32-40";
            numericUpDownMissileDamageMod.Value = 0;
            numericUpDownMissileDamageBonus.Value = 0;
            numericUpDownMissileWeaponSpeed.Value = 0;
            comboBoxMagicAnimation.Text = "Normal";
            comboBoxMagicSpell.Text = "War 8";
            numericUpDownMagicElementalDamageBonus.Value = 0;
            checkBoxMeleeArmorRend.Checked = false;
            checkBoxMeleeArmorCleaving.Checked = false;
            checkBoxMeleeResistanceRend.Checked = false;
            numericUpDownMeleeResistanceCleaving.Value = 1;
            checkBoxMeleeCriticalStrike.Checked = false;
            numericUpDownMeleeBitingStrike.Value = 0.1m;
            checkBoxMeleeCripplingBlow.Checked = false;
            numericUpDownMeleeCrushingBlow.Value = 1;
            checkBoxMissileArmorRend.Checked = false;
            checkBoxMissileArmorCleaving.Checked = false;
            checkBoxMissileResistanceRend.Checked = false;
            numericUpDownMissileResistanceCleaving.Value = 1;
            checkBoxMissileCriticalStrike.Checked = false;
            numericUpDownMissileBitingStrike.Value = 0.1m;
            checkBoxMissileCripplingBlow.Checked = false;
            numericUpDownMissileCrushingBlow.Value = 1;
            checkBoxMagicResistanceRend.Checked = false;
            numericUpDownMagicResistanceCleaving.Value = 1;
            checkBoxMagicCriticalStrike.Checked = false;
            numericUpDownMagicBitingStrike.Value = 0.05m;
            checkBoxMagicCripplingBlow.Checked = false;
            numericUpDownMagicCrushingBlow.Value = 1;
            comboBoxMeleeBloodDrinker.Text = "None";
            comboBoxMeleeBloodThirst.Text = "None";
            comboBoxMeleeHeartSeeker.Text = "None";
            comboBoxMeleeHeartThirst.Text = "None";
            comboBoxMeleeHighestSpell.Text = "None";
            numericUpDownMeleeSlayer.Value = 1;
            comboBoxMissileBloodDrinker.Text = "None";
            comboBoxMissileBloodThirst.Text = "None";
            numericUpDownMissileSlayer.Value = 1;
            comboBoxMagicSpiritDrinker.Text = "None";
            comboBoxMagicSpiritThirst.Text = "None";
            numericUpDownMagicSlayer.Value = 1;
            trackBarMeleePowerBar.Value = 50;
            trackBarMissileAccuracyBar.Value = 50;
        }

        private void menuItemSaveEnemy_Click(object sender, EventArgs e)
        {
            if (textBoxEnemyName.Text == "")
            {
                MessageBox.Show("Please enter a name for your enemy before saving.", "Name Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxEnemyName.Select();
                return;
            }


            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            SaveFileDialog saveFileDialog3 = new SaveFileDialog();
            saveFileDialog3.InitialDirectory = Directory.GetCurrentDirectory() + @"\Enemy";
            saveFileDialog3.RestoreDirectory = false;
            saveFileDialog3.Filter = "JSON|*.json";
            saveFileDialog3.Title = "Save Enemy Template";
            saveFileDialog3.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog3.FileName != "")
            {
                Enemy enemy = new Enemy
                {
                    Name = textBoxEnemyName.Text,
                    Armor = (int)numericUpDownEnemyArmor.Value,
                    Resist = numericUpDownEnemyResistance.Value,
                    ShieldArmor = (int)numericUpDownEnemyShieldAL.Value,
                    ShieldResist = numericUpDownEnemyShieldResist.Value,
                    MeleeDefense = (int)numericUpDownEnemyMeleeDefense.Value,
                    MissileDefense = (int)numericUpDownEnemyMissileDefense.Value,
                    MagicDefense = (int)numericUpDownEnemyMagicDefense.Value,
                    Imperil = comboBoxEnemyImperil.Text,
                    ResistanceVuln = comboBoxEnemyVuln.Text,
                    Corruption = (int)numericUpDownCorruption.Value,
                    Corrosion = (int)numericUpDownCorrosion.Value,
                    Destructive = (int)numericUpDownDestructive.Value,
                    Vulnerability = comboBoxEnemyVulnerability.Text,
                    Defenselessness = comboBoxEnemyDefenselessness.Text,
                    MagicYield = comboBoxEnemyMagicYield.Text,
                    UnbalancedAssault = comboBoxEnemyUnbalancingAssault.Text,
                    Clumsiness = comboBoxEnemyClumsiness.Text,
                    Slowness = comboBoxEnemySlowness.Text,
                    Bafflement = comboBoxEnemyBafflement.Text,
                    Feeblemind = comboBoxEnemyFeeblemind.Text,
                    Brittlemail = comboBoxEnemyBrittlemail.Text,
                    ResistanceLure = comboBoxEnemyResistanceLure.Text
                };

                // serialize JSON directly to a file
                using (StreamWriter file = File.CreateText(saveFileDialog3.FileName))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, enemy);
                }
            }
        }

        private void menuItemLoadEnemy_Click(object sender, EventArgs e)
        {
            // Displays a OpenFileDialog so the user can save the Image
            // assigned to Button2.
            OpenFileDialog openFileDialog3 = new OpenFileDialog();
            openFileDialog3.InitialDirectory = Directory.GetCurrentDirectory() + @"\Enemy";
            openFileDialog3.Filter = "JSON|*.json";
            openFileDialog3.Title = "Open Enemy Template";
            openFileDialog3.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (openFileDialog3.FileName != "")
            {
                // deserialize JSON directly from a file
                using (StreamReader file = File.OpenText(openFileDialog3.FileName))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    Enemy enemy = (Enemy)serializer.Deserialize(file, typeof(Enemy));

                    textBoxEnemyName.Text = enemy.Name;
                    numericUpDownEnemyArmor.Value = enemy.Armor;
                    numericUpDownEnemyResistance.Value = enemy.Resist;
                    numericUpDownEnemyShieldAL.Value = enemy.ShieldArmor;
                    numericUpDownEnemyShieldResist.Value = enemy.ShieldResist;
                    numericUpDownEnemyMeleeDefense.Value = enemy.MeleeDefense;
                    numericUpDownEnemyMissileDefense.Value = enemy.MissileDefense;
                    numericUpDownEnemyMagicDefense.Value = enemy.MagicDefense;
                    comboBoxEnemyImperil.Text = enemy.Imperil;
                    comboBoxEnemyVuln.Text = enemy.ResistanceVuln;
                    numericUpDownCorruption.Value = enemy.Corruption;
                    numericUpDownCorrosion.Value = enemy.Corrosion;
                    numericUpDownDestructive.Value = enemy.Destructive;
                    comboBoxEnemyVulnerability.Text = enemy.Vulnerability;
                    comboBoxEnemyDefenselessness.Text = enemy.Defenselessness;
                    comboBoxEnemyMagicYield.Text = enemy.MagicYield;
                    comboBoxEnemyUnbalancingAssault.Text = enemy.UnbalancedAssault;
                    comboBoxEnemyClumsiness.Text = enemy.Clumsiness;
                    comboBoxEnemySlowness.Text = enemy.Slowness;
                    comboBoxEnemyBafflement.Text = enemy.Bafflement;
                    comboBoxEnemyFeeblemind.Text = enemy.Feeblemind;
                    comboBoxEnemyBrittlemail.Text = enemy.Brittlemail;
                    comboBoxEnemyResistanceLure.Text = enemy.ResistanceLure;
                }
            }
        }

     private void menuItemResetEnemy_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("Are you sure want to reset Enemy settings?",
                                        "Confirm Enemy Reset",
                                        MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.No)
            {
                return;
            }

            textBoxEnemyName.Text = "";
            numericUpDownEnemyArmor.Value = 0;
            numericUpDownEnemyResistance.Value = 0m;
            numericUpDownEnemyShieldAL.Value = 0;
            numericUpDownEnemyShieldResist.Value = 0m;
            numericUpDownEnemyMeleeDefense.Value = 0;
            numericUpDownEnemyMissileDefense.Value = 0;
            numericUpDownEnemyMagicDefense.Value = 0;
            comboBoxEnemyImperil.Text = "None";
            comboBoxEnemyVuln.Text = "None";
            numericUpDownCorruption.Value = 0;
            numericUpDownCorrosion.Value = 0;
            numericUpDownDestructive.Value = 0;
            comboBoxEnemyVulnerability.Text = "None";
            comboBoxEnemyDefenselessness.Text = "None";
            comboBoxEnemyMagicYield.Text = "None";
            comboBoxEnemyUnbalancingAssault.Text = "None";
            comboBoxEnemyClumsiness.Text = "None";
            comboBoxEnemySlowness.Text = "None";
            comboBoxEnemyBafflement.Text = "None";
            comboBoxEnemyFeeblemind.Text = "None";
            comboBoxEnemyBrittlemail.Text = "None";
            comboBoxEnemyResistanceLure.Text = "None";
        }

        private void menuItemQuit_Click(object sender, EventArgs e)
        {
            if (Application.MessageLoop)
            {
                // WinForms app
                Application.Exit();
            }
            else
            {
                // Console app
                Environment.Exit(1);
            }
        }
    }
}
