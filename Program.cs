using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;


namespace PerfectPantheon
{
    class Program
    {
        public static Spell.Targeted Q;
        public static Spell.Targeted W;
        public static Spell.Skillshot E;
        static Spell.Targeted Smite = null;
        public static Menu Menu, FarmingMenu, MiscMenu, DrawMenu, HarassMenu, ComboMenu, SmiteMenu,Skin;
        static Item Healthpot;
        static Item CrystalFlask;
        static Item CorruptingPotion;
        static Item RefillablePotion;
        static Item HuntersPotion;
        public static SpellSlot SmiteSlot = SpellSlot.Unknown;
        private static Spell.Targeted _ignite;
        private static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        private static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        private static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        private static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };   

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }

        }
        private static string Smitetype
        {
            get
            {
                if (SmiteBlue.Any(i => Item.HasItem(i)))
                    return "s5_summonersmiteplayerganker";

                if (SmiteRed.Any(i => Item.HasItem(i)))
                    return "s5_summonersmiteduel";

                if (SmiteGrey.Any(i => Item.HasItem(i)))
                    return "s5_summonersmitequick";

                if (SmitePurple.Any(i => Item.HasItem(i)))
                    return "itemsmiteaoe";

                return "summonersmite";
            }
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Pantheon")
                return;

            SpellDataInst smite = _Player.Spellbook.Spells.Where(spell => spell.Name.Contains("smite")).Any() ? _Player.Spellbook.Spells.Where(spell => spell.Name.Contains("smite")).First() : null;
            if (smite != null)
            {
                Smite = new Spell.Targeted(smite.Slot, 500);
            }
            _ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Healthpot = new Item(2003, 0);
            CrystalFlask = new Item(2041, 0);
            CorruptingPotion = new Item(2033, 0);
            RefillablePotion = new Item(2031, 0);
            HuntersPotion = new Item(2032, 0);

            uint level = (uint)Player.Instance.Level;
            Q = new Spell.Targeted(SpellSlot.Q, 600);
            W = new Spell.Targeted(SpellSlot.W, 600);
            E = new Spell.Skillshot(SpellSlot.E, 600, SkillShotType.Cone, 250, 2000, 70);

            Menu = MainMenu.AddMenu("Perfect Pantheon", "perfectpant");
            Menu.AddLabel("Perrrrrrrrrfect Pantheon Addon");

            ComboMenu = Menu.AddSubMenu("Combo Settings","ComboSettings");            
            ComboMenu.AddLabel("Combo Settings");
            ComboMenu.Add("QCombo", new CheckBox("Use Q"));
            ComboMenu.Add("WCombo", new CheckBox("Use W"));
            ComboMenu.Add("ECombo", new CheckBox("Use E"));
            ComboMenu.Add("useTiamat", new CheckBox("Use Items"));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "HarassSettings");
            HarassMenu.AddLabel("Harass Settings");
            HarassMenu.Add("QHarass", new CheckBox("Use Q"));
            HarassMenu.Add("QHarassMana", new Slider("Mana < %", 30, 0, 100));

            FarmingMenu = Menu.AddSubMenu("Lane Clear", "FarmSettings");

            FarmingMenu.AddLabel("Lane Clear");
            FarmingMenu.Add("QLaneClear", new CheckBox("Use Q LaneClear"));
            FarmingMenu.Add("QlaneclearMana", new Slider("Mana < %", 50, 0, 100));
            FarmingMenu.Add("ELaneClear", new CheckBox("Use E LaneClear"));
            FarmingMenu.Add("ElaneclearMana", new Slider("Mana < %", 50, 0, 100));
            FarmingMenu.AddLabel("Jungle Clear");
            FarmingMenu.Add("Qjungle", new CheckBox("Use Q in Jungle"));
            FarmingMenu.Add("QjungleMana", new Slider("Mana < %", 35, 0, 100));
            FarmingMenu.Add("Wjungle", new CheckBox("Use W in Jungle"));
            FarmingMenu.Add("WjungleMana", new Slider("Mana < %", 35, 0, 100));
            FarmingMenu.Add("Ejungle", new CheckBox("Use E in Jungle"));
            FarmingMenu.Add("EjungleMana", new Slider("Mana < %", 35, 0, 100));
            FarmingMenu.AddLabel("Last Hit Settings");
            FarmingMenu.Add("Qlasthit", new CheckBox("Use Q LastHit"));
            FarmingMenu.Add("QlasthitMana", new Slider("Mana < %", 55, 0, 100));

            SetSmiteSlot();
            if (SmiteSlot != SpellSlot.Unknown)
            {
                SmiteMenu = Menu.AddSubMenu("Smite Usage", "SmiteUsage");
                SmiteMenu.AddLabel("Smite Usage");
                SmiteMenu.AddSeparator();
                SmiteMenu.Add("Use Smite?", new CheckBox("Use Smite"));
                SmiteMenu.Add("SmiteEnemy", new CheckBox("Use Smite Combo for Enemy!"));  
                SmiteMenu.AddSeparator();
                SmiteMenu.Add("Red?", new CheckBox("Red"));
                SmiteMenu.Add("Blue?", new CheckBox("Blue"));
                SmiteMenu.AddSeparator();
                SmiteMenu.Add("Dragon?", new CheckBox("Dragon"));
                SmiteMenu.Add("Baron?", new CheckBox("Baron"));
            }

            MiscMenu = Menu.AddSubMenu("More Settings", "Misc");

            MiscMenu.AddLabel("Auto");
            MiscMenu.Add("AutoIgnite", new CheckBox("Auto Ignite"));
            MiscMenu.Add("interrupter", new CheckBox("Use Interruptable Spells"));
            MiscMenu.Add("gapcloser", new CheckBox("Use Gapclose Spells"));
            MiscMenu.AddLabel("Items");
            MiscMenu.AddLabel("BOTRK,Bilgewater Cutlass Settings");
            MiscMenu.Add("botrkHP", new Slider("My HP < %", 60, 0, 100));
            MiscMenu.Add("botrkenemyHP", new Slider("Enemy HP < %", 60, 0, 100));
            MiscMenu.AddLabel("KillSteal");
            MiscMenu.Add("Qkill", new CheckBox("Use Q KillSteal"));
            MiscMenu.Add("Wkill", new CheckBox("Use W KillSteal"));
            MiscMenu.AddLabel("Activator");
            MiscMenu.Add("useHP", new CheckBox("Use Health Potion"));           
            MiscMenu.Add("useHPV", new Slider("HP < %", 45, 0, 100));
            MiscMenu.Add("useMana", new CheckBox("Use Mana Potion"));
            MiscMenu.Add("useManaV", new Slider("Mana < %", 45, 0, 100));
            MiscMenu.Add("useCrystal", new CheckBox("Use Refillable Potions",false));
            MiscMenu.Add("useCrystalHPV", new Slider("HP < %", 60, 0, 100));
            MiscMenu.Add("useCrystalManaV", new Slider("Mana < %", 60, 0, 100));

            Skin = Menu.AddSubMenu("Skin Changer", "SkinChanger");
            Skin.Add("checkSkin", new CheckBox("Use Skin Changer"));
            Skin.Add("skin.Id", new Slider("Skin", 4, 0, 6));

            DrawMenu = Menu.AddSubMenu("Draw Settings", "Drawings");
            DrawMenu.Add("drawAA", new CheckBox("Draw AA Range", false));
            DrawMenu.Add("drawQ", new CheckBox("Draw Q Range"));
            DrawMenu.Add("drawW", new CheckBox("Draw W Range"));
            DrawMenu.Add("drawE", new CheckBox("Draw E Range"));
            DrawMenu.Add("drawR", new CheckBox("Draw R Range"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
        }

        private static void SetSmiteSlot()
        {
            foreach (
                var spell in
                    _Player.Spellbook.Spells.Where(
                        spell => string.Equals(spell.Name, Smitetype, StringComparison.CurrentCultureIgnoreCase)))
            {
                SmiteSlot = spell.Slot;
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
             Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender.IsMe) return;
            if (MiscMenu["interrupter"].Cast<CheckBox>().CurrentValue && sender.IsEnemy &&
                e.DangerLevel >= DangerLevel.Medium && sender.IsValidTarget(W.Range))
            {
                Player.CastSpell(SpellSlot.W, sender);
            }

        }


        public static void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender.IsMe) return;
            if (MiscMenu["gapcloser"].Cast<CheckBox>().CurrentValue && sender.IsEnemy &&
                sender.IsValidTarget(W.Range))
            {
                Player.CastSpell(SpellSlot.W, sender);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            var HPpot = MiscMenu["useHP"].Cast<CheckBox>().CurrentValue;
            var Mpot = MiscMenu["useMana"].Cast<CheckBox>().CurrentValue;
            var Crystal = MiscMenu["useCrystal"].Cast<CheckBox>().CurrentValue;
            var HPv = MiscMenu["useHPv"].Cast<Slider>().CurrentValue;
            var Manav = MiscMenu["useManav"].Cast<Slider>().CurrentValue;
            var CrystalHPv = MiscMenu["useCrystalHPv"].Cast<Slider>().CurrentValue;
            var CrystalManav = MiscMenu["useCrystalManav"].Cast<Slider>().CurrentValue;
            var useItem = ComboMenu["useTiamat"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

            if (MiscMenu["AutoIgnite"].Cast<CheckBox>().CurrentValue)
            {
                if (!_ignite.IsReady() || Player.Instance.IsDead) return;
                foreach (
                    var source in
                        EntityManager.Heroes.Enemies
                            .Where(
                                a => a.IsValidTarget(_ignite.Range) &&
                                    a.Health < 50 + 20 * Player.Instance.Level - (a.HPRegenRate / 5 * 3)))
                {
                    _ignite.Cast(source);
                    return;
                }
            }

            if (Smite != null)
            {
                if (Smite.IsReady() && SmiteMenu["Use Smite?"].Cast<CheckBox>().CurrentValue)
                {
                    Obj_AI_Minion Mob = EntityManager.MinionsAndMonsters.GetJungleMonsters(_Player.Position, Smite.Range).FirstOrDefault();

                    if (Mob != default(Obj_AI_Minion))
                    {
                        bool kill = GetSmiteDamage() >= Mob.Health;

                        if (kill)
                        {
                            if ((Mob.Name.Contains("SRU_Dragon") || Mob.Name.Contains("SRU_Baron"))) Smite.Cast(Mob);
                            else if (Mob.Name.StartsWith("SRU_Red") && SmiteMenu["Red?"].Cast<CheckBox>().CurrentValue) Smite.Cast(Mob);
                            else if (Mob.Name.StartsWith("SRU_Blue") && SmiteMenu["Blue?"].Cast<CheckBox>().CurrentValue) Smite.Cast(Mob);
                        }
                    }
                }
            }

            if (HPpot && Player.Instance.HealthPercent < HPv)
            {
                if (Item.HasItem(Healthpot.Id) && Item.CanUseItem(Healthpot.Id) && !Player.HasBuff("RegenerationPotion"))
                {
                    Healthpot.Cast();
                }
            }

            if (Crystal && Player.Instance.HealthPercent < CrystalHPv || Crystal && Player.Instance.ManaPercent < CrystalManav)
            {
                if (Item.HasItem(RefillablePotion.Id) && Item.CanUseItem(RefillablePotion.Id) && !Player.HasBuff("RegenerationPotion") && !Player.HasBuff("FlaskOfCrystalWater") && !Player.HasBuff("ItemCrystalFlask") && !Player.HasBuff("ItemDarkCrystalFlaskJungle"))
                {
                    RefillablePotion.Cast();
                }
                else if (Item.HasItem(CorruptingPotion.Id) && Item.CanUseItem(CorruptingPotion.Id) && !Player.HasBuff("RegenerationPotion") && !Player.HasBuff("FlaskOfCrystalWater") && !Player.HasBuff("ItemCrystalFlask") && !Player.HasBuff("ItemDarkCrystalFlaskJungle"))
                {
                    CorruptingPotion.Cast();
                }
                else if (Item.HasItem(HuntersPotion.Id) && Item.CanUseItem(HuntersPotion.Id) && !Player.HasBuff("RegenerationPotion") && !Player.HasBuff("FlaskOfCrystalWater") && !Player.HasBuff("ItemCrystalFlask") && !Player.HasBuff("ItemCrystalFlaskJungle"))
                {
                    HuntersPotion.Cast();
                }

            }

            if (_Player.SkinId != Skin["skin.Id"].Cast<Slider>().CurrentValue)
            {
                if (checkSkin())
                {
                    Player.SetSkinId(SkinId());
                }
            }

            // if (Crystal && Player.Instance.HealthPercent < CrystalHPv || Crystal && Player.Instance.ManaPercent < CrystalManav)
            //{
            //  if (Item.HasItem(CrystalFlask.Id) && Item.CanUseItem(CrystalFlask.Id) && !Player.HasBuff("RegenerationPotion") && !Player.HasBuff("FlaskOfCrystalWater") && !Player.HasBuff("ItemCrystalFlask"))
            //{
            //  CrystalFlask.Cast();
            //}

            //}

            if (useItem && target.IsValidTarget(400) && !target.IsDead && !target.IsZombie && target.HealthPercent < 100)
            {
                HandleItems();
            }

            var t = TargetSelector.GetTarget(600, DamageType.True);
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
                SmiteOnTarget(t);
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }
            KillSteal();
            

        }

        public static int SkinId()
        {
            return Skin["skin.Id"].Cast<Slider>().CurrentValue;
        }
        public static bool checkSkin()
        {
            return Skin["checkSkin"].Cast<CheckBox>().CurrentValue;
        }

        private static void SmiteOnTarget(AIHeroClient t)
        {
            var range = 700f;
            var use = SmiteMenu["SmiteEnemy"].Cast<CheckBox>().CurrentValue;
            var itemCheck = SmiteBlue.Any(i => Item.HasItem(i)) || SmiteRed.Any(i => Item.HasItem(i));
            if (itemCheck && use &&
                _Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready &&
                t.Distance(_Player.Position) < range)
            {
                _Player.Spellbook.CastSpell(SmiteSlot, t);
            }
        }
        private static void JungleClear()
        {
            var useQ = FarmingMenu["Qjungle"].Cast<CheckBox>().CurrentValue;
            var useQMana = FarmingMenu["QjungleMana"].Cast<Slider>().CurrentValue;
            var useW = FarmingMenu["Wjungle"].Cast<CheckBox>().CurrentValue;
            var useWMana = FarmingMenu["WjungleMana"].Cast<Slider>().CurrentValue;
            var useE = FarmingMenu["Ejungle"].Cast<CheckBox>().CurrentValue;
            var useEMana = FarmingMenu["EjungleMana"].Cast<Slider>().CurrentValue;
            foreach (var monster in EntityManager.MinionsAndMonsters.Monsters)
            {
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent > useQMana)
                {
                    Q.Cast(monster);
                }
                if (useW && W.IsReady() && Player.Instance.ManaPercent > useWMana && Player.Instance.HealthPercent < 70)
                {
                    W.Cast(monster);
                }
                if (useE && E.IsReady() && Player.Instance.HealthPercent > useEMana)
                {
                    E.Cast(monster);
                }

                HandleItems();
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var useQ = ComboMenu["QCombo"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["WCombo"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ECombo"].Cast<CheckBox>().CurrentValue;
            var useItem = ComboMenu["useTiamat"].Cast<CheckBox>().CurrentValue;          
           
            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
            {
                Q.Cast(target);
            }
            if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsDead && !target.IsZombie)
            {
                W.Cast(target);
            }
            if (E.IsReady() && useE && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
            {
                E.Cast(target);
            }
        }
        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var useQ = MiscMenu["Qkill"].Cast<CheckBox>().CurrentValue;
            var useW = MiscMenu["Wkill"].Cast<CheckBox>().CurrentValue;

            if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range) && !target.IsZombie && target.Health <= _Player.GetSpellDamage(target, SpellSlot.Q))
            {
                Q.Cast(target);
            }
            if (W.IsReady() && useW && target.IsValidTarget(W.Range) && !target.IsZombie && target.Health <= _Player.GetSpellDamage(target, SpellSlot.W))
            {
                W.Cast(target);
            }
        }

        internal static void HandleItems()
        {
            var botrktarget = TargetSelector.GetTarget(550, DamageType.Physical);
            var useItem = ComboMenu["useTiamat"].Cast<CheckBox>().CurrentValue;
            var useBotrkHP = MiscMenu["botrkHP"].Cast<Slider>().CurrentValue;
            var useBotrkEnemyHP = MiscMenu["botrkenemyHP"].Cast<Slider>().CurrentValue;
            //HYDRA
            if (useItem && Item.HasItem(3077) && Item.CanUseItem(3077))
                Item.UseItem(3077);

            //TİAMAT
            if (useItem && Item.HasItem(3074) && Item.CanUseItem(3074))
                Item.UseItem(3074);

            //NEW ITEM
            if (useItem && Item.HasItem(3748) && Item.CanUseItem(3748))
                Item.UseItem(3748);

            //BİLGEWATER CUTLASS
            if (useItem && Item.HasItem(3144) && Item.CanUseItem(3144) && botrktarget.HealthPercent <= useBotrkEnemyHP && _Player.HealthPercent <= useBotrkHP)
                Item.UseItem(3144, botrktarget);

            //BOTRK
            if (useItem && Item.HasItem(3153) && Item.CanUseItem(3153) && botrktarget.HealthPercent <= useBotrkEnemyHP && _Player.HealthPercent <= useBotrkHP)
                Item.UseItem(3153, botrktarget);

            //YOUMU
            if (useItem && Item.HasItem(3142) && Item.CanUseItem(3142))
                Item.UseItem(3142);
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var useQ = HarassMenu["QHarass"].Cast<CheckBox>().CurrentValue;
            var useQMana = HarassMenu["QHarassMana"].Cast<Slider>().CurrentValue;
            if (Q.IsReady() && Player.Instance.ManaPercent > useQMana && useQ && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
            {
                Q.Cast(target);
            }

        }
        private static void LaneClear()
        {
            var useQ = FarmingMenu["QLaneClear"].Cast<CheckBox>().CurrentValue;
            var useE = FarmingMenu["ELaneClear"].Cast<CheckBox>().CurrentValue;
            var Qmana = FarmingMenu["QlaneclearMana"].Cast<Slider>().CurrentValue;
            var Emana = FarmingMenu["ElaneclearMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && !minion.IsValidTarget(_Player.AttackRange) && Player.Instance.ManaPercent > Qmana && minion.Health > _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
                if (useE && E.IsReady() && Player.Instance.ManaPercent > Emana && minion.IsValidTarget(E.Range) && minions.Count() >= 3)
                {
                    E.Cast(minion);
                }
            }
        }
        private static void LastHit()
        {
            var useQ = FarmingMenu["Qlasthit"].Cast<CheckBox>().CurrentValue;
            var mana = FarmingMenu["QlasthitMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && !minion.IsValidTarget(_Player.AttackRange) && Player.Instance.ManaPercent > mana && minion.Health < _Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
            }
        }
        static float GetSmiteDamage()
        {
            float damage = new float();

            if (_Player.Level < 10) damage = 360 + (_Player.Level - 1) * 30;

            else if (_Player.Level < 15) damage = 280 + (_Player.Level - 1) * 40;

            else if (_Player.Level < 19) damage = 150 + (_Player.Level - 1) * 50;

            return damage;
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (DrawMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (DrawMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
        }
    }
}