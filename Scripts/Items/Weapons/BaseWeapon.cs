using System;
using Server.Network;
using Server.Mobiles;
using Server.Spells;
using Server.Factions;
using Server.Engines.Craft;
using System.Collections.Generic;

namespace Server.Items
{
    public interface ISlayer
	{
		SlayerName Slayer { get; set; }
		SlayerName Slayer2 { get; set; }
	}

	public abstract class BaseWeapon : Item, IWeapon, IFactionItem, ICraftable, ISlayer, IDurability
    {
        private string m_EngravedText;
		
		[CommandProperty( AccessLevel.GameMaster )]
		public string EngravedText
		{
			get{ return m_EngravedText; }
			set{ m_EngravedText = value; InvalidateProperties(); }
		}

		#region Factions
		private FactionItem m_FactionState;

		public FactionItem FactionItemState
		{
			get{ return m_FactionState; }
			set
			{
				m_FactionState = value;

				if ( m_FactionState == null )
					Hue = CraftResources.GetHue( Resource );

				LootType = ( m_FactionState == null ? LootType.Regular : LootType.Blessed );
			}
		}
		#endregion

		/* Weapon internals work differently now (Mar 13 2003)
		 * 
		 * The attributes defined below default to -1.
		 * If the value is -1, the corresponding virtual 'Aos/Old' property is used.
		 * If not, the attribute value itself is used. Here's the list:
		 *  - MinDamage
		 *  - MaxDamage
		 *  - Speed
		 *  - HitSound
		 *  - MissSound
		 *  - StrRequirement, DexRequirement, IntRequirement
		 *  - WeaponType
		 *  - WeaponAnimation
		 *  - MaxRange
		 */

		#region Var declarations

		// Instance values. These values are unique to each weapon.
		private WeaponDamageLevel m_DamageLevel;
		private WeaponAccuracyLevel m_AccuracyLevel;
		private WeaponDurabilityLevel m_DurabilityLevel;
		private WeaponQuality m_Quality;
		private Mobile m_Crafter;
		private Poison m_Poison;
		private int m_PoisonCharges;
		private bool m_Identified;
		private int m_Hits;
		private int m_MaxHits;
		private SlayerName m_Slayer;
		private SlayerName m_Slayer2;
		private SkillMod m_SkillMod, m_MageMod;
		private CraftResource m_Resource;
		private bool m_PlayerConstructed;

		private bool m_Cursed; // Is this weapon cursed via Curse Weapon necromancer spell? Temporary; not serialized.
		private bool m_Consecrated; // Is this weapon blessed via Consecrate Weapon paladin ability? Temporary; not serialized.

		// Overridable values. These values are provided to override the defaults which get defined in the individual weapon scripts.
		private int m_StrReq, m_DexReq, m_IntReq;
		private int m_MinDamage, m_MaxDamage;
		private int m_HitSound, m_MissSound;
		private float m_Speed;
		private int m_MaxRange;
		private SkillName m_Skill;
		private WeaponType m_Type;
		private WeaponAnimation m_Animation;
		#endregion

		#region Virtual Properties
		public virtual int DefMaxRange{ get{ return 1; } }
		public virtual int DefHitSound{ get{ return 0; } }
		public virtual int DefMissSound{ get{ return 0; } }
		public virtual SkillName DefSkill{ get{ return SkillName.Swords; } }
		public virtual WeaponType DefType{ get{ return WeaponType.Slashing; } }
		public virtual WeaponAnimation DefAnimation{ get{ return WeaponAnimation.Slash1H; } }

		public virtual int OldStrengthReq{ get{ return 0; } }
		public virtual int OldDexterityReq{ get{ return 0; } }
		public virtual int OldIntelligenceReq{ get{ return 0; } }
		public virtual int OldMinDamage{ get{ return 0; } }
		public virtual int OldMaxDamage{ get{ return 0; } }
		public virtual int OldSpeed{ get{ return 0; } }
		public virtual int OldMaxRange{ get{ return DefMaxRange; } }
		public virtual int OldHitSound{ get{ return DefHitSound; } }
		public virtual int OldMissSound{ get{ return DefMissSound; } }
		public virtual SkillName OldSkill{ get{ return DefSkill; } }
		public virtual WeaponType OldType{ get{ return DefType; } }
		public virtual WeaponAnimation OldAnimation{ get{ return DefAnimation; } }

		public virtual int InitMinHits{ get{ return 0; } }
		public virtual int InitMaxHits{ get{ return 0; } }

		public virtual bool CanFortify{ get{ return true; } }

		public virtual SkillName AccuracySkill { get { return SkillName.Tactics; } }
		#endregion

		#region Getters & Setters
		[CommandProperty( AccessLevel.GameMaster )]
		public bool Cursed
		{
			get{ return m_Cursed; }
			set{ m_Cursed = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Consecrated
		{
			get{ return m_Consecrated; }
			set{ m_Consecrated = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Identified
		{
			get{ return m_Identified; }
			set{ m_Identified = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int HitPoints
		{
			get{ return m_Hits; }
			set
			{
				if ( m_Hits == value )
					return;

				if ( value > m_MaxHits )
					value = m_MaxHits;

				m_Hits = value;

				InvalidateProperties();
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int MaxHitPoints
		{
			get{ return m_MaxHits; }
			set{ m_MaxHits = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int PoisonCharges
		{
			get{ return m_PoisonCharges; }
			set{ m_PoisonCharges = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Poison Poison
		{
			get{ return m_Poison; }
			set{ m_Poison = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public WeaponQuality Quality
		{
			get{ return m_Quality; }
			set{ UnscaleDurability(); m_Quality = value; ScaleDurability(); InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Crafter
		{
			get{ return m_Crafter; }
			set{ m_Crafter = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public SlayerName Slayer
		{
			get{ return m_Slayer; }
			set{ m_Slayer = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public SlayerName Slayer2
		{
			get { return m_Slayer2; }
			set { m_Slayer2 = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public CraftResource Resource
		{
			get{ return m_Resource; }
			set{ UnscaleDurability(); m_Resource = value; Hue = CraftResources.GetHue( m_Resource ); InvalidateProperties(); ScaleDurability(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public WeaponDamageLevel DamageLevel
		{
			get{ return m_DamageLevel; }
			set{ m_DamageLevel = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public WeaponDurabilityLevel DurabilityLevel
		{
			get{ return m_DurabilityLevel; }
			set{ UnscaleDurability(); m_DurabilityLevel = value; InvalidateProperties(); ScaleDurability(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool PlayerConstructed
		{
			get{ return m_PlayerConstructed; }
			set{ m_PlayerConstructed = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int MaxRange
		{
			get{ return ( m_MaxRange == -1 ? OldMaxRange : m_MaxRange ); }
			set{ m_MaxRange = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public WeaponAnimation Animation
		{
			get{ return ( m_Animation == (WeaponAnimation)(-1) ? OldAnimation : m_Animation ); } 
			set{ m_Animation = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public WeaponType Type
		{
			get{ return ( m_Type == (WeaponType)(-1) ? OldType : m_Type ); }
			set{ m_Type = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public SkillName Skill
		{
			get{ return ( m_Skill == (SkillName)(-1) ? OldSkill : m_Skill ); }
			set{ m_Skill = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int HitSound
		{
			get{ return ( m_HitSound == -1 ? OldHitSound : m_HitSound ); }
			set{ m_HitSound = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int MissSound
		{
			get{ return ( m_MissSound == -1 ? OldMissSound : m_MissSound ); }
			set{ m_MissSound = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int MinDamage
		{
			get{ return ( m_MinDamage == -1 ? OldMinDamage : m_MinDamage ); }
			set{ m_MinDamage = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int MaxDamage
		{
			get{ return ( m_MaxDamage == -1 ? OldMaxDamage : m_MaxDamage ); }
			set{ m_MaxDamage = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public float Speed
		{
			get
			{
				if ( m_Speed != -1 )
					return m_Speed;

				return OldSpeed;
			}
			set{ m_Speed = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int StrRequirement
		{
			get{ return ( m_StrReq == -1 ? OldStrengthReq : m_StrReq ); }
			set{ m_StrReq = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int DexRequirement
		{
			get{ return ( m_DexReq == -1 ? OldDexterityReq : m_DexReq ); }
			set{ m_DexReq = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int IntRequirement
		{
			get{ return ( m_IntReq == -1 ? OldIntelligenceReq : m_IntReq ); }
			set{ m_IntReq = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public WeaponAccuracyLevel AccuracyLevel
		{
			get
			{
				return m_AccuracyLevel;
			}
			set
			{
				if ( m_AccuracyLevel != value )
				{
					m_AccuracyLevel = value;

					if ( UseSkillMod )
					{
						if ( m_AccuracyLevel == WeaponAccuracyLevel.Regular )
						{
							if ( m_SkillMod != null )
								m_SkillMod.Remove();

							m_SkillMod = null;
						}
						else if ( m_SkillMod == null && Parent is Mobile )
						{
							m_SkillMod = new DefaultSkillMod( AccuracySkill, true, (int)m_AccuracyLevel * 5 );
							((Mobile)Parent).AddSkillMod( m_SkillMod );
						}
						else if ( m_SkillMod != null )
						{
							m_SkillMod.Value = (int)m_AccuracyLevel * 5;
						}
					}

					InvalidateProperties();
				}
			}
		}

		#endregion

		public override void OnAfterDuped( Item newItem )
		{
			BaseWeapon weap = newItem as BaseWeapon;

			if ( weap == null )
				return;
		}

		public virtual void UnscaleDurability()
		{
			int scale = 100 + GetDurabilityBonus();

			m_Hits = ((m_Hits * 100) + (scale - 1)) / scale;
			m_MaxHits = ((m_MaxHits * 100) + (scale - 1)) / scale;
			InvalidateProperties();
		}

		public virtual void ScaleDurability()
		{
			int scale = 100 + GetDurabilityBonus();

			m_Hits = ((m_Hits * scale) + 99) / 100;
			m_MaxHits = ((m_MaxHits * scale) + 99) / 100;
			InvalidateProperties();
		}

		public int GetDurabilityBonus()
		{
			int bonus = 0;

			if ( m_Quality == WeaponQuality.Exceptional )
				bonus += 20;

			switch ( m_DurabilityLevel )
			{
				case WeaponDurabilityLevel.Durable: bonus += 20; break;
				case WeaponDurabilityLevel.Substantial: bonus += 50; break;
				case WeaponDurabilityLevel.Massive: bonus += 70; break;
				case WeaponDurabilityLevel.Fortified: bonus += 100; break;
				case WeaponDurabilityLevel.Indestructible: bonus += 120; break;
			}

			return bonus;
		}

		public static void BlockEquip( Mobile m, TimeSpan duration )
		{
			if ( m.BeginAction( typeof( BaseWeapon ) ) )
				new ResetEquipTimer( m, duration ).Start();
		}

		private class ResetEquipTimer : Timer
		{
			private Mobile m_Mobile;

			public ResetEquipTimer( Mobile m, TimeSpan duration ) : base( duration )
			{
				m_Mobile = m;
			}

			protected override void OnTick()
			{
				m_Mobile.EndAction( typeof( BaseWeapon ) );
			}
		}

		public override bool CheckConflictingLayer( Mobile m, Item item, Layer layer )
		{
			if ( base.CheckConflictingLayer( m, item, layer ) )
				return true;

			if ( this.Layer == Layer.TwoHanded && layer == Layer.OneHanded )
			{
				m.SendLocalizedMessage( 500214 ); // You already have something in both hands.
				return true;
			}
			else if ( this.Layer == Layer.OneHanded && layer == Layer.TwoHanded && !(item is BaseShield) && !(item is BaseEquipableLight) )
			{
				m.SendLocalizedMessage( 500215 ); // You can only wield one weapon at a time.
				return true;
			}

			return false;
		}

		public override bool AllowSecureTrade( Mobile from, Mobile to, Mobile newOwner, bool accepted )
		{
			if ( !Ethics.Ethic.CheckTrade( from, to, newOwner, this ) )
				return false;

			return base.AllowSecureTrade( from, to, newOwner, accepted );
		}

		public virtual Race RequiredRace { get { return null; } }	//On OSI, there are no weapons with race requirements, this is for custom stuff

		public override bool CanEquip( Mobile from )
		{
			if ( !Ethics.Ethic.CheckEquip( from, this ) )
				return false;

			if( RequiredRace != null && from.Race != RequiredRace )
			{
				if( RequiredRace == Race.Elf )
					from.SendLocalizedMessage( 1072203 ); // Only Elves may use this.
				else
					from.SendMessage( "Only {0} may use this.", RequiredRace.PluralName );

				return false;
			}
			else if ( from.Dex < DexRequirement )
			{
				from.SendMessage( "You are not nimble enough to equip that." );
				return false;
			} 
			else if ( from.Str < StrRequirement )
            {
				from.SendLocalizedMessage( 500213 ); // You are not strong enough to equip that.
				return false;
			}
			else if ( from.Int < IntRequirement )
			{
				from.SendMessage( "You are not smart enough to equip that." );
				return false;
			}
			else if ( !from.CanBeginAction( typeof( BaseWeapon ) ) )
			{
				return false;
			}
			else
			{
				return base.CanEquip( from );
			}
		}

		public virtual bool UseSkillMod{ get{ return true; } }

		public override bool OnEquip( Mobile from )
		{
			from.NextCombatTime = DateTime.Now + GetDelay( from );

			if ( UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular )
			{
				if ( m_SkillMod != null )
					m_SkillMod.Remove();

				m_SkillMod = new DefaultSkillMod( AccuracySkill, true, (int)m_AccuracyLevel * 5 );
				from.AddSkillMod( m_SkillMod );
			}

			return true;
		}

		public override void OnAdded( object parent )
		{
			base.OnAdded( parent );

			if ( parent is Mobile )
			{
				Mobile from = (Mobile)parent;

				from.CheckStatTimers();
				from.Delta( MobileDelta.WeaponDamage );
			}
		}

		public override void OnRemoved( object parent )
		{
			if ( parent is Mobile )
			{
				Mobile m = (Mobile)parent;
				BaseWeapon weapon = m.Weapon as BaseWeapon;

				string modName = this.Serial.ToString();

				m.RemoveStatMod( modName + "Str" );
				m.RemoveStatMod( modName + "Dex" );
				m.RemoveStatMod( modName + "Int" );

				if ( weapon != null )
					m.NextCombatTime = DateTime.Now + weapon.GetDelay( m );

				if ( UseSkillMod && m_SkillMod != null )
				{
					m_SkillMod.Remove();
					m_SkillMod = null;
				}

				if ( m_MageMod != null )
				{
					m_MageMod.Remove();
					m_MageMod = null;
				}

				m.CheckStatTimers();

				m.Delta( MobileDelta.WeaponDamage );
			}
		}

		public virtual SkillName GetUsedSkill( Mobile m, bool checkSkillAttrs )
		{
			SkillName sk = Skill;

				if ( sk != SkillName.Wrestling && !m.Player && !m.Body.IsHuman && m.Skills[SkillName.Wrestling].Value > m.Skills[sk].Value )
					sk = SkillName.Wrestling;

			return sk;
		}

		public virtual double GetAttackSkillValue( Mobile attacker, Mobile defender )
		{
			return attacker.Skills[GetUsedSkill( attacker, true )].Value;
		}

		public virtual double GetDefendSkillValue( Mobile attacker, Mobile defender )
		{
			return defender.Skills[GetUsedSkill( defender, true )].Value;
		}

		public virtual bool CheckHit( Mobile attacker, Mobile defender )
		{
			BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
			BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

			Skill atkSkill = attacker.Skills[atkWeapon.Skill];
			Skill defSkill = defender.Skills[defWeapon.Skill];

			double atkValue = atkWeapon.GetAttackSkillValue( attacker, defender );
			double defValue = defWeapon.GetDefendSkillValue( attacker, defender );

			double ourValue, theirValue;

			if ( atkValue <= -50.0 )
				atkValue = -49.9;

			if ( defValue <= -50.0 )
				defValue = -49.9;

			ourValue = (atkValue + 50.0);
			theirValue = (defValue + 50.0);

			double chance = ourValue / (theirValue * 2.0);

			return attacker.CheckSkill( atkSkill.SkillName, chance );
		}

		public virtual TimeSpan GetDelay( Mobile m )
		{
			double speed = this.Speed;

			if ( speed == 0 )
				return TimeSpan.FromHours( 1.0 );

			int v = (m.Stam + 100) * (int) speed;

			if ( v <= 0 )
				v = 1;

            double delayInSeconds = 15000.0 / v;
			
			return TimeSpan.FromSeconds( delayInSeconds );
		}

		public virtual TimeSpan OnSwing( Mobile attacker, Mobile defender )
		{
			return OnSwing( attacker, defender, 1.0 );
		}

		public virtual TimeSpan OnSwing( Mobile attacker, Mobile defender, double damageBonus )
		{
			bool canSwing = true;

			#region Dueling
			if ( attacker is PlayerMobile )
			{
				PlayerMobile pm = (PlayerMobile)attacker;

				if ( pm.DuelContext != null && !pm.DuelContext.CheckItemEquip( attacker, this ) )
					canSwing = false;
			}
			#endregion

			if ( canSwing && attacker.HarmfulCheck( defender ) )
			{
				attacker.DisruptiveAction();

				if ( attacker.NetState != null )
					attacker.Send( new Swing( 0, attacker, defender ) );

				if ( CheckHit( attacker, defender ) )
					OnHit( attacker, defender, damageBonus );
				else
					OnMiss( attacker, defender );
			}

			return GetDelay( attacker );
		}

		#region Sounds
		public virtual int GetHitAttackSound( Mobile attacker, Mobile defender )
		{
			int sound = attacker.GetAttackSound();

			if ( sound == -1 )
				sound = HitSound;

			return sound;
		}

		public virtual int GetHitDefendSound( Mobile attacker, Mobile defender )
		{
			return defender.GetHurtSound();
		}

		public virtual int GetMissAttackSound( Mobile attacker, Mobile defender )
		{
			if ( attacker.GetAttackSound() == -1 )
				return MissSound;
			else
				return -1;
		}

		public virtual int GetMissDefendSound( Mobile attacker, Mobile defender )
		{
			return -1;
		}
		#endregion

		public virtual int AbsorbDamage( Mobile attacker, Mobile defender, int damage )
		{
			BaseShield shield = defender.FindItemOnLayer( Layer.TwoHanded ) as BaseShield;
			if ( shield != null )
				damage = shield.OnHit( this, damage );

			double chance = Utility.RandomDouble();

			Item armorItem;

			if( chance < 0.07 )
				armorItem = defender.NeckArmor;
			else if( chance < 0.14 )
				armorItem = defender.HandArmor;
			else if( chance < 0.28 )
				armorItem = defender.ArmsArmor;
			else if( chance < 0.43 )
				armorItem = defender.HeadArmor;
			else if( chance < 0.65 )
				armorItem = defender.LegsArmor;
			else
				armorItem = defender.ChestArmor;

			IWearableDurability armor = armorItem as IWearableDurability;

			if ( armor != null )
				damage = armor.OnHit( this, damage );

			int virtualArmor = defender.VirtualArmor + defender.VirtualArmorMod;

			if ( virtualArmor > 0 )
			{
				double scalar;

				if ( chance < 0.14 )
					scalar = 0.07;
				else if ( chance < 0.28 )
					scalar = 0.14;
				else if ( chance < 0.43 )
					scalar = 0.15;
				else if ( chance < 0.65 )
					scalar = 0.22;
				else
					scalar = 0.35;

				int from = (int)(virtualArmor * scalar) / 2;
				int to = (int)(virtualArmor * scalar);

				damage -= Utility.Random( from, (to - from) + 1 );
			}

			return damage;
		}

		public void OnHit( Mobile attacker, Mobile defender )
		{
			OnHit( attacker, defender, 1.0 );
		}

		public virtual void OnHit( Mobile attacker, Mobile defender, double damageBonus )
        {
            PlaySwingAnimation(attacker);
            PlayHurtAnimation(defender);

            attacker.PlaySound(GetHitAttackSound(attacker, defender));
            defender.PlaySound(GetHitDefendSound(attacker, defender));

            /// Passively check Anatomy and tactics for gain
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            attacker.CheckSkill(SkillName.Tactics, 0.0, attacker.Skills[SkillName.Tactics].Cap);
            attacker.CheckSkill(SkillName.Anatomy, 0.0, attacker.Skills[SkillName.Anatomy].Cap);

            /// GET DAMAGE
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            int min, max;

            /// GetBaseDamageRange(attacker, out min, out max); - ToDo DamageMin/DamageMax to MinDamage/MaxDamage ones only and not in every hit, if possible.
            //////////////////////////////////////////////////////////////////////////////////////////////////////

            if (attacker is BaseCreature)
            {
                BaseCreature c = (BaseCreature)attacker;

                if (c.DamageMin >= 0)
                {
                    min = c.DamageMin;
                    max = c.DamageMax;
                }

                else if (this is Fists && !attacker.Body.IsHuman)   // <-- Why This?
                {
                    min = attacker.Str / 28;
                    max = attacker.Str / 28;
                }
            }

            min = MinDamage;
            max = MaxDamage;


            /// ROLL BASE DAMAGE
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            int baseDamage = Utility.RandomMinMax(min, max);

            /// ADD DAMAGE LEVEL BONUS (Ruin: 1, Might: 3, Force: 5, Power: 7, Vanq: 9)
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            if (m_DamageLevel != WeaponDamageLevel.Regular)
                baseDamage += (2 * (int)m_DamageLevel) - 1;

            /// Tactics Modifiers to Damage Dealt ( FORMULA: % of Base Damage that is Dealt= Tactics + 50 )
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            baseDamage = (int)(baseDamage * ((attacker.Skills[SkillName.Tactics].Value + 50.0) / 100.0));

            /// STR Bonuses to Damage Dealt ( FORMULA: 1% bonus for every 5 points of strength )
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            int bonusDamage = attacker.Str / 5;

            /// Anatomy Bonuses to Damage Dealt ( FORMULA: 1% bonus for every 5 points of anatomy )
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            bonusDamage += (int)attacker.Skills[SkillName.Anatomy].Value / 5;

            /// Calculate Damage with damage level, skill and stat bonus
            //////////////////////////////////////////////////////////////////////////////////////////////////////            
            int damage = ((bonusDamage / 100) + 1) * baseDamage;

            // Add Slayer Damage (FORMULA: Damage x2)
            //////////////////////////////////////////////////////////////////////////////////////////////////////            
            CheckSlayerResult cs = CheckSlayers(attacker, defender);

            if (cs != CheckSlayerResult.None)
            {
                if (cs == CheckSlayerResult.Slayer)
                    defender.FixedEffect(0x37B9, 10, 5);

                damage = damage * 2;
            }


            /// Add Exeptional weapon bonus, T2A??
            //////////////////////////////////////////////////////////////////////////////////////////////////////            
            //          if (m_Quality != WeaponQuality.Regular)
            //              modifiers += (((int)m_Quality - 1) * 0.2);

            // Virtual damage bonus, T2A??
            //////////////////////////////////////////////////////////////////////////////////////////////////////            
            //          if (VirtualDamageBonus != 0)
            //              modifiers += (VirtualDamageBonus / 100.0);


            /// Scale after items Durability (did this happen in T2A?)
            //////////////////////////////////////////////////////////////////////////////////////////////////////            
            if (m_MaxHits > 0 && m_Hits < m_MaxHits)
            {
                int scale = 50 + ((50 * m_Hits) / m_MaxHits);
                damage = damage * scale / 100;
            }

            /// IF THE DEFENDER IS A PLAYER OR THE ATTACKER IS NOT THEN GIVE HALF DAMAGE
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            if (defender is PlayerMobile || !(attacker is PlayerMobile))
                damage = (int)(damage / 2.0);


            /// Alter Damage In Special Situations
            //////////////////////////////////////////////////////////////////////////////////////////////////////

            if ( attacker is BaseCreature )
				((BaseCreature)attacker).AlterMeleeDamageTo( defender, ref damage );

			if ( defender is BaseCreature )
				((BaseCreature)defender).AlterMeleeDamageFrom( attacker, ref damage );

            /// ABSORB DAMAGE
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            damage = AbsorbDamage( attacker, defender, damage );

            if (damage < 1) // <-- in absorb?
                damage = 1; // <-- in absorb?

            /// Add Blood On ground and do Damage
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            AddBlood( attacker, defender, damage );
            attacker.Damage( damage, defender );

            /// Slime Damage to Weapons
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            if ( m_MaxHits > 0 && ((MaxRange <= 1 && (defender is Slime || defender is AcidElemental)) || Utility.Random( 25 ) == 0) ) // Stratics says 50% chance, seems more like 4%..
			{
				if ( MaxRange <= 1 && (defender is Slime || defender is AcidElemental) )
					attacker.LocalOverheadMessage( MessageType.Regular, 0x3B2, 500263 ); // *Acid blood scars your weapon!*

				if ( m_Hits > 0 )
				{
					--HitPoints;
				}
				else if ( m_MaxHits > 1 )
				{
					--MaxHitPoints;

                    if ( Parent is Mobile )
						((Mobile)Parent).LocalOverheadMessage( MessageType.Regular, 0x3B2, 1061121 ); // Your equipment is severely damaged.
				}
				else
				{
					Delete();
				}
			}

            /// Do on damage overides
            //////////////////////////////////////////////////////////////////////////////////////////////////////

            if ( attacker is BaseCreature )
				((BaseCreature)attacker).OnGaveMeleeAttack( defender );

			if ( defender is BaseCreature )
				((BaseCreature)defender).OnGotMeleeAttack( attacker );

            /// Honor in T2A ?
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            // if ( defender is IHonorTarget && ((IHonorTarget)defender).ReceivedHonorContext != null )
			//	((IHonorTarget)defender).ReceivedHonorContext.OnTargetHit( attacker );
		}

		public virtual CheckSlayerResult CheckSlayers( Mobile attacker, Mobile defender )
		{
			BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
			SlayerEntry atkSlayer = SlayerGroup.GetEntryByName( atkWeapon.Slayer );
			SlayerEntry atkSlayer2 = SlayerGroup.GetEntryByName( atkWeapon.Slayer2 );

			if( atkWeapon is ButchersWarCleaver && TalismanSlayer.Slays( TalismanSlayerName.Bovine, defender ) )
				return CheckSlayerResult.Slayer;

			if ( atkSlayer != null && atkSlayer.Slays( defender )  || atkSlayer2 != null && atkSlayer2.Slays( defender ) )
				return CheckSlayerResult.Slayer;

			BaseTalisman talisman = attacker.Talisman as BaseTalisman;

			if ( talisman != null && TalismanSlayer.Slays( talisman.Slayer, defender ) )
				return CheckSlayerResult.Slayer;

			ISlayer defISlayer = Spellbook.FindEquippedSpellbook( defender );

			if( defISlayer == null )
				defISlayer = defender.Weapon as ISlayer;

			if( defISlayer != null )
			{
				SlayerEntry defSlayer = SlayerGroup.GetEntryByName( defISlayer.Slayer );
				SlayerEntry defSlayer2 = SlayerGroup.GetEntryByName( defISlayer.Slayer2 );

				if( defSlayer != null && defSlayer.Group.OppositionSuperSlays( attacker ) || defSlayer2 != null && defSlayer2.Group.OppositionSuperSlays( attacker ) )
					return CheckSlayerResult.Opposition;
			}

			return CheckSlayerResult.None;
		}

		public virtual void AddBlood( Mobile attacker, Mobile defender, int damage )
		{
			if ( damage > 0 )
			{
				new Blood().MoveToWorld( defender.Location, defender.Map );

				int extraBlood = Utility.RandomMinMax( 0, 1 );

				for( int i = 0; i < extraBlood; i++ )
				{
					new Blood().MoveToWorld( new Point3D(
						defender.X + Utility.RandomMinMax( -1, 1 ),
						defender.Y + Utility.RandomMinMax( -1, 1 ),
						defender.Z ), defender.Map );
				}
			}
		}

		public virtual void OnMiss( Mobile attacker, Mobile defender )
		{
			PlaySwingAnimation( attacker );
			attacker.PlaySound( GetMissAttackSound( attacker, defender ) );
			defender.PlaySound( GetMissDefendSound( attacker, defender ) );

			if ( defender is IHonorTarget && ((IHonorTarget)defender).ReceivedHonorContext != null )
				((IHonorTarget)defender).ReceivedHonorContext.OnTargetMissed( attacker );
		}

		public virtual int VirtualDamageBonus{ get{ return 0; } }

		public virtual void PlayHurtAnimation( Mobile from )
		{
			int action;
			int frames;

			switch ( from.Body.Type )
			{
				case BodyType.Sea:
				case BodyType.Animal:
				{
					action = 7;
					frames = 5;
					break;
				}
				case BodyType.Monster:
				{
					action = 10;
					frames = 4;
					break;
				}
				case BodyType.Human:
				{
					action = 20;
					frames = 5;
					break;
				}
				default: return;
			}

			if ( from.Mounted )
				return;

			from.Animate( action, frames, 1, true, false, 0 );
		}

		public virtual void PlaySwingAnimation( Mobile from )
		{
			int action;

			switch ( from.Body.Type )
			{
				case BodyType.Sea:
				case BodyType.Animal:
				{
					action = Utility.Random( 5, 2 );
					break;
				}
				case BodyType.Monster:
				{
					switch ( Animation )
					{
						default:
						case WeaponAnimation.Wrestle:
						case WeaponAnimation.Bash1H:
						case WeaponAnimation.Pierce1H:
						case WeaponAnimation.Slash1H:
						case WeaponAnimation.Bash2H:
						case WeaponAnimation.Pierce2H:
						case WeaponAnimation.Slash2H: action = Utility.Random( 4, 3 ); break;
						case WeaponAnimation.ShootBow:  return; // 7
						case WeaponAnimation.ShootXBow: return; // 8
					}

					break;
				}
				case BodyType.Human:
				{
					if ( !from.Mounted )
					{
						action = (int)Animation;
					}
					else
					{
						switch ( Animation )
						{
							default:
							case WeaponAnimation.Wrestle:
							case WeaponAnimation.Bash1H:
							case WeaponAnimation.Pierce1H:
							case WeaponAnimation.Slash1H: action = 26; break;
							case WeaponAnimation.Bash2H:
							case WeaponAnimation.Pierce2H:
							case WeaponAnimation.Slash2H: action = 29; break;
							case WeaponAnimation.ShootBow: action = 27; break;
							case WeaponAnimation.ShootXBow: action = 28; break;
						}
					}

					break;
				}
				default: return;
			}

			from.Animate( action, 7, 1, true, false, 0 );
		}

		#region Serialization/Deserialization
		private static void SetSaveFlag( ref SaveFlag flags, SaveFlag toSet, bool setIf )
		{
			if ( setIf )
				flags |= toSet;
		}

		private static bool GetSaveFlag( SaveFlag flags, SaveFlag toGet )
		{
			return ( (flags & toGet) != 0 );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 9 ); // version

			SaveFlag flags = SaveFlag.None;

			SetSaveFlag( ref flags, SaveFlag.DamageLevel,		m_DamageLevel != WeaponDamageLevel.Regular );
			SetSaveFlag( ref flags, SaveFlag.AccuracyLevel,		m_AccuracyLevel != WeaponAccuracyLevel.Regular );
			SetSaveFlag( ref flags, SaveFlag.DurabilityLevel,	m_DurabilityLevel != WeaponDurabilityLevel.Regular );
			SetSaveFlag( ref flags, SaveFlag.Quality,			m_Quality != WeaponQuality.Regular );
			SetSaveFlag( ref flags, SaveFlag.Hits,				m_Hits != 0 );
			SetSaveFlag( ref flags, SaveFlag.MaxHits,			m_MaxHits != 0 );
			SetSaveFlag( ref flags, SaveFlag.Slayer,			m_Slayer != SlayerName.None );
			SetSaveFlag( ref flags, SaveFlag.Poison,			m_Poison != null );
			SetSaveFlag( ref flags, SaveFlag.PoisonCharges,		m_PoisonCharges != 0 );
			SetSaveFlag( ref flags, SaveFlag.Crafter,			m_Crafter != null );
			SetSaveFlag( ref flags, SaveFlag.Identified,		m_Identified != false );
			SetSaveFlag( ref flags, SaveFlag.StrReq,			m_StrReq != -1 );
			SetSaveFlag( ref flags, SaveFlag.DexReq,			m_DexReq != -1 );
			SetSaveFlag( ref flags, SaveFlag.IntReq,			m_IntReq != -1 );
			SetSaveFlag( ref flags, SaveFlag.MinDamage,			m_MinDamage != -1 );
			SetSaveFlag( ref flags, SaveFlag.MaxDamage,			m_MaxDamage != -1 );
			SetSaveFlag( ref flags, SaveFlag.HitSound,			m_HitSound != -1 );
			SetSaveFlag( ref flags, SaveFlag.MissSound,			m_MissSound != -1 );
			SetSaveFlag( ref flags, SaveFlag.Speed,				m_Speed != -1 );
			SetSaveFlag( ref flags, SaveFlag.MaxRange,			m_MaxRange != -1 );
			SetSaveFlag( ref flags, SaveFlag.Skill,				m_Skill != (SkillName)(-1) );
			SetSaveFlag( ref flags, SaveFlag.Type,				m_Type != (WeaponType)(-1) );
			SetSaveFlag( ref flags, SaveFlag.Animation,			m_Animation != (WeaponAnimation)(-1) );
			SetSaveFlag( ref flags, SaveFlag.Resource,			m_Resource != CraftResource.Iron );
			SetSaveFlag( ref flags, SaveFlag.PlayerConstructed,	m_PlayerConstructed );
			SetSaveFlag( ref flags, SaveFlag.Slayer2,			m_Slayer2 != SlayerName.None );
			SetSaveFlag( ref flags, SaveFlag.EngravedText,		!String.IsNullOrEmpty( m_EngravedText ) );

			writer.Write( (int) flags );

			if ( GetSaveFlag( flags, SaveFlag.DamageLevel ) )
				writer.Write( (int) m_DamageLevel );

			if ( GetSaveFlag( flags, SaveFlag.AccuracyLevel ) )
				writer.Write( (int) m_AccuracyLevel );

			if ( GetSaveFlag( flags, SaveFlag.DurabilityLevel ) )
				writer.Write( (int) m_DurabilityLevel );

			if ( GetSaveFlag( flags, SaveFlag.Quality ) )
				writer.Write( (int) m_Quality );

			if ( GetSaveFlag( flags, SaveFlag.Hits ) )
				writer.Write( (int) m_Hits );

			if ( GetSaveFlag( flags, SaveFlag.MaxHits ) )
				writer.Write( (int) m_MaxHits );

			if ( GetSaveFlag( flags, SaveFlag.Slayer ) )
				writer.Write( (int) m_Slayer );

			if ( GetSaveFlag( flags, SaveFlag.Poison ) )
				Poison.Serialize( m_Poison, writer );

			if ( GetSaveFlag( flags, SaveFlag.PoisonCharges ) )
				writer.Write( (int) m_PoisonCharges );

			if ( GetSaveFlag( flags, SaveFlag.Crafter ) )
				writer.Write( (Mobile) m_Crafter );

			if ( GetSaveFlag( flags, SaveFlag.StrReq ) )
				writer.Write( (int) m_StrReq );

			if ( GetSaveFlag( flags, SaveFlag.DexReq ) )
				writer.Write( (int) m_DexReq );

			if ( GetSaveFlag( flags, SaveFlag.IntReq ) )
				writer.Write( (int) m_IntReq );

			if ( GetSaveFlag( flags, SaveFlag.MinDamage ) )
				writer.Write( (int) m_MinDamage );

			if ( GetSaveFlag( flags, SaveFlag.MaxDamage ) )
				writer.Write( (int) m_MaxDamage );

			if ( GetSaveFlag( flags, SaveFlag.HitSound ) )
				writer.Write( (int) m_HitSound );

			if ( GetSaveFlag( flags, SaveFlag.MissSound ) )
				writer.Write( (int) m_MissSound );

			if ( GetSaveFlag( flags, SaveFlag.Speed ) )
				writer.Write( (float) m_Speed );

			if ( GetSaveFlag( flags, SaveFlag.MaxRange ) )
				writer.Write( (int) m_MaxRange );

			if ( GetSaveFlag( flags, SaveFlag.Skill ) )
				writer.Write( (int) m_Skill );

			if ( GetSaveFlag( flags, SaveFlag.Type ) )
				writer.Write( (int) m_Type );

			if ( GetSaveFlag( flags, SaveFlag.Animation ) )
				writer.Write( (int) m_Animation );

			if ( GetSaveFlag( flags, SaveFlag.Resource ) )
				writer.Write( (int) m_Resource );

			if ( GetSaveFlag( flags, SaveFlag.Slayer2 ) )
				writer.Write( (int)m_Slayer2 );

			if( GetSaveFlag( flags, SaveFlag.EngravedText ) )
				writer.Write( (string) m_EngravedText );
		}

		[Flags]
		private enum SaveFlag
		{
			None					= 0x00000000,
			DamageLevel				= 0x00000001,
			AccuracyLevel			= 0x00000002,
			DurabilityLevel			= 0x00000004,
			Quality					= 0x00000008,
			Hits					= 0x00000010,
			MaxHits					= 0x00000020,
			Slayer					= 0x00000040,
			Poison					= 0x00000080,
			PoisonCharges			= 0x00000100,
			Crafter					= 0x00000200,
			Identified				= 0x00000400,
			StrReq					= 0x00000800,
			DexReq					= 0x00001000,
			IntReq					= 0x00002000,
			MinDamage				= 0x00004000,
			MaxDamage				= 0x00008000,
			HitSound				= 0x00010000,
			MissSound				= 0x00020000,
			Speed					= 0x00040000,
			MaxRange				= 0x00080000,
			Skill					= 0x00100000,
			Type					= 0x00200000,
			Animation				= 0x00400000,
			Resource				= 0x00800000,
			xAttributes				= 0x01000000,
			xWeaponAttributes		= 0x02000000,
			PlayerConstructed		= 0x04000000,
			SkillBonuses			= 0x08000000,
			Slayer2					= 0x10000000,
			ElementalDamages		= 0x20000000,
			EngravedText			= 0x40000000
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 9:
				case 8:
				case 7:
				case 6:
				case 5:
				{
					SaveFlag flags = (SaveFlag)reader.ReadInt();

					if ( GetSaveFlag( flags, SaveFlag.DamageLevel ) )
					{
						m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();

						if ( m_DamageLevel > WeaponDamageLevel.Vanq )
							m_DamageLevel = WeaponDamageLevel.Ruin;
					}

					if ( GetSaveFlag( flags, SaveFlag.AccuracyLevel ) )
					{
						m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();

						if ( m_AccuracyLevel > WeaponAccuracyLevel.Supremely )
							m_AccuracyLevel = WeaponAccuracyLevel.Accurate;
					}

					if ( GetSaveFlag( flags, SaveFlag.DurabilityLevel ) )
					{
						m_DurabilityLevel = (WeaponDurabilityLevel)reader.ReadInt();

						if ( m_DurabilityLevel > WeaponDurabilityLevel.Indestructible )
							m_DurabilityLevel = WeaponDurabilityLevel.Durable;
					}

					if ( GetSaveFlag( flags, SaveFlag.Quality ) )
						m_Quality = (WeaponQuality)reader.ReadInt();
					else
						m_Quality = WeaponQuality.Regular;

					if ( GetSaveFlag( flags, SaveFlag.Hits ) )
						m_Hits = reader.ReadInt();

					if ( GetSaveFlag( flags, SaveFlag.MaxHits ) )
						m_MaxHits = reader.ReadInt();

					if ( GetSaveFlag( flags, SaveFlag.Slayer ) )
						m_Slayer = (SlayerName)reader.ReadInt();

					if ( GetSaveFlag( flags, SaveFlag.Poison ) )
						m_Poison = Poison.Deserialize( reader );

					if ( GetSaveFlag( flags, SaveFlag.PoisonCharges ) )
						m_PoisonCharges = reader.ReadInt();

					if ( GetSaveFlag( flags, SaveFlag.Crafter ) )
						m_Crafter = reader.ReadMobile();

					if ( GetSaveFlag( flags, SaveFlag.Identified ) )
						m_Identified = ( version >= 6 || reader.ReadBool() );

					if ( GetSaveFlag( flags, SaveFlag.StrReq ) )
						m_StrReq = reader.ReadInt();
					else
						m_StrReq = -1;

					if ( GetSaveFlag( flags, SaveFlag.DexReq ) )
						m_DexReq = reader.ReadInt();
					else
						m_DexReq = -1;

					if ( GetSaveFlag( flags, SaveFlag.IntReq ) )
						m_IntReq = reader.ReadInt();
					else
						m_IntReq = -1;

					if ( GetSaveFlag( flags, SaveFlag.MinDamage ) )
						m_MinDamage = reader.ReadInt();
					else
						m_MinDamage = -1;

					if ( GetSaveFlag( flags, SaveFlag.MaxDamage ) )
						m_MaxDamage = reader.ReadInt();
					else
						m_MaxDamage = -1;

					if ( GetSaveFlag( flags, SaveFlag.HitSound ) )
						m_HitSound = reader.ReadInt();
					else
						m_HitSound = -1;

					if ( GetSaveFlag( flags, SaveFlag.MissSound ) )
						m_MissSound = reader.ReadInt();
					else
						m_MissSound = -1;

					if ( GetSaveFlag( flags, SaveFlag.Speed ) )
					{
						if ( version < 9 )
							m_Speed = reader.ReadInt();
						else
							m_Speed = reader.ReadFloat();
					}
					else
						m_Speed = -1;

					if ( GetSaveFlag( flags, SaveFlag.MaxRange ) )
						m_MaxRange = reader.ReadInt();
					else
						m_MaxRange = -1;

					if ( GetSaveFlag( flags, SaveFlag.Skill ) )
						m_Skill = (SkillName)reader.ReadInt();
					else
						m_Skill = (SkillName)(-1);

					if ( GetSaveFlag( flags, SaveFlag.Type ) )
						m_Type = (WeaponType)reader.ReadInt();
					else
						m_Type = (WeaponType)(-1);

					if ( GetSaveFlag( flags, SaveFlag.Animation ) )
						m_Animation = (WeaponAnimation)reader.ReadInt();
					else
						m_Animation = (WeaponAnimation)(-1);

					if ( GetSaveFlag( flags, SaveFlag.Resource ) )
						m_Resource = (CraftResource)reader.ReadInt();
					else
						m_Resource = CraftResource.Iron;

					if ( UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular && Parent is Mobile )
					{
						m_SkillMod = new DefaultSkillMod( AccuracySkill, true, (int)m_AccuracyLevel * 5 );
						((Mobile)Parent).AddSkillMod( m_SkillMod );
					}

					if ( GetSaveFlag( flags, SaveFlag.PlayerConstructed ) )
						m_PlayerConstructed = true;

					if( GetSaveFlag( flags, SaveFlag.Slayer2 ) )
						m_Slayer2 = (SlayerName)reader.ReadInt();

					if( GetSaveFlag( flags, SaveFlag.EngravedText ) )
						m_EngravedText = reader.ReadString();

					break;
				}
				case 4:
				{
					m_Slayer = (SlayerName)reader.ReadInt();

					goto case 3;
				}
				case 3:
				{
					m_StrReq = reader.ReadInt();
					m_DexReq = reader.ReadInt();
					m_IntReq = reader.ReadInt();

					goto case 2;
				}
				case 2:
				{
					m_Identified = reader.ReadBool();

					goto case 1;
				}
				case 1:
				{
					m_MaxRange = reader.ReadInt();

					goto case 0;
				}
				case 0:
				{
					if ( version == 0 )
						m_MaxRange = 1; // default

					if ( version < 5 )
					{
						m_Resource = CraftResource.Iron;
					}

					m_MinDamage = reader.ReadInt();
					m_MaxDamage = reader.ReadInt();

					m_Speed = reader.ReadInt();

					m_HitSound = reader.ReadInt();
					m_MissSound = reader.ReadInt();

					m_Skill = (SkillName)reader.ReadInt();
					m_Type = (WeaponType)reader.ReadInt();
					m_Animation = (WeaponAnimation)reader.ReadInt();
					m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();
					m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();
					m_DurabilityLevel = (WeaponDurabilityLevel)reader.ReadInt();
					m_Quality = (WeaponQuality)reader.ReadInt();

					m_Crafter = reader.ReadMobile();

					m_Poison = Poison.Deserialize( reader );
					m_PoisonCharges = reader.ReadInt();

					if ( m_StrReq == OldStrengthReq )
						m_StrReq = -1;

					if ( m_DexReq == OldDexterityReq )
						m_DexReq = -1;

					if ( m_IntReq == OldIntelligenceReq )
						m_IntReq = -1;

					if ( m_MinDamage == OldMinDamage )
						m_MinDamage = -1;

					if ( m_MaxDamage == OldMaxDamage )
						m_MaxDamage = -1;

					if ( m_HitSound == OldHitSound )
						m_HitSound = -1;

					if ( m_MissSound == OldMissSound )
						m_MissSound = -1;

					if ( m_Speed == OldSpeed )
						m_Speed = -1;

					if ( m_MaxRange == OldMaxRange )
						m_MaxRange = -1;

					if ( m_Skill == OldSkill )
						m_Skill = (SkillName)(-1);

					if ( m_Type == OldType )
						m_Type = (WeaponType)(-1);

					if ( m_Animation == OldAnimation )
						m_Animation = (WeaponAnimation)(-1);

					if ( UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular && Parent is Mobile )
					{
						m_SkillMod = new DefaultSkillMod( AccuracySkill, true, (int)m_AccuracyLevel * 5);
						((Mobile)Parent).AddSkillMod( m_SkillMod );
					}

					break;
				}
			}

			if ( Parent is Mobile )
				((Mobile)Parent).CheckStatTimers();

			if ( m_Hits <= 0 && m_MaxHits <= 0 )
			{
				m_Hits = m_MaxHits = Utility.RandomMinMax( InitMinHits, InitMaxHits );
			}

			if ( version < 6 )
				m_PlayerConstructed = true; // we don't know, so, assume it's crafted
		}
		#endregion

		public BaseWeapon( int itemID ) : base( itemID )
		{
			Layer = (Layer)ItemData.Quality;

			m_Quality = WeaponQuality.Regular;
			m_StrReq = -1;
			m_DexReq = -1;
			m_IntReq = -1;
			m_MinDamage = -1;
			m_MaxDamage = -1;
			m_HitSound = -1;
			m_MissSound = -1;
			m_Speed = -1;
			m_MaxRange = -1;
			m_Skill = (SkillName)(-1);
			m_Type = (WeaponType)(-1);
			m_Animation = (WeaponAnimation)(-1);

			m_Hits = m_MaxHits = Utility.RandomMinMax( InitMinHits, InitMaxHits );

			m_Resource = CraftResource.Iron;
		}

		public BaseWeapon( Serial serial ) : base( serial )
		{
		}

		private string GetNameString()
		{
			string name = this.Name;

			if ( name == null )
				name = String.Format( "#{0}", LabelNumber );

			return name;
		}

		[Hue, CommandProperty( AccessLevel.GameMaster )]
		public override int Hue
		{
			get{ return base.Hue; }
			set{ base.Hue = value; InvalidateProperties(); }
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			int oreType;

			switch ( m_Resource )
			{
				case CraftResource.DullCopper:		oreType = 1053108; break; // dull copper
				case CraftResource.ShadowIron:		oreType = 1053107; break; // shadow iron
				case CraftResource.Copper:			oreType = 1053106; break; // copper
				case CraftResource.Bronze:			oreType = 1053105; break; // bronze
				case CraftResource.Gold:			oreType = 1053104; break; // golden
				case CraftResource.Agapite:			oreType = 1053103; break; // agapite
				case CraftResource.Verite:			oreType = 1053102; break; // verite
				case CraftResource.Valorite:		oreType = 1053101; break; // valorite
				case CraftResource.SpinedLeather:	oreType = 1061118; break; // spined
				case CraftResource.HornedLeather:	oreType = 1061117; break; // horned
				case CraftResource.BarbedLeather:	oreType = 1061116; break; // barbed
				case CraftResource.RedScales:		oreType = 1060814; break; // red
				case CraftResource.YellowScales:	oreType = 1060818; break; // yellow
				case CraftResource.BlackScales:		oreType = 1060820; break; // black
				case CraftResource.GreenScales:		oreType = 1060819; break; // green
				case CraftResource.WhiteScales:		oreType = 1060821; break; // white
				case CraftResource.BlueScales:		oreType = 1060815; break; // blue
				default: oreType = 0; break;
			}

			if ( oreType != 0 )
				list.Add( 1053099, "#{0}\t{1}", oreType, GetNameString() ); // ~1_oretype~ ~2_armortype~
			else if ( Name == null )
				list.Add( LabelNumber );
			else
				list.Add( Name );
				
			/*
			 * Want to move this to the engraving tool, let the non-harmful 
			 * formatting show, and remove CLILOCs embedded: more like OSI
			 * did with the books that had markup, etc.
			 * 
			 * This will have a negative effect on a few event things imgame 
			 * as is.
			 * 
			 * If we cant find a more OSI-ish way to clean it up, we can 
			 * easily put this back, and use it in the deserialize
			 * method and engraving tool, to make it perm cleaned up.
			 */

			if ( !String.IsNullOrEmpty( m_EngravedText ) )
				list.Add( 1062613, m_EngravedText );

				/* list.Add( 1062613, Utility.FixHtml( m_EngravedText ) ); */
		}

		public override bool AllowEquipedCast( Mobile from )
		{
			if ( base.AllowEquipedCast( from ) )
				return true;

			return false;
		}

		public virtual int ArtifactRarity
		{
			get{ return 0; }
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( m_Crafter != null )
				list.Add( 1050043, m_Crafter.Name ); // crafted by ~1_NAME~

			#region Factions
			if ( m_FactionState != null )
				list.Add( 1041350 ); // faction item
			#endregion

			if ( m_Quality == WeaponQuality.Exceptional )
				list.Add( 1060636 ); // exceptional

			if( RequiredRace == Race.Elf )
				list.Add( 1075086 ); // Elves Only

			if ( ArtifactRarity > 0 )
				list.Add( 1061078, ArtifactRarity.ToString() ); // artifact rarity ~1_val~

			if ( this is IUsesRemaining && ((IUsesRemaining)this).ShowUsesRemaining )
				list.Add( 1060584, ((IUsesRemaining)this).UsesRemaining.ToString() ); // uses remaining: ~1_val~

			if ( m_Poison != null && m_PoisonCharges > 0 )
				list.Add( 1062412 + m_Poison.Level, m_PoisonCharges.ToString() );

			if( m_Slayer != SlayerName.None )
			{
				SlayerEntry entry = SlayerGroup.GetEntryByName( m_Slayer );
				if( entry != null )
					list.Add( entry.Title );
			}

			if( m_Slayer2 != SlayerName.None )
			{
				SlayerEntry entry = SlayerGroup.GetEntryByName( m_Slayer2 );
				if( entry != null )
					list.Add( entry.Title );
			}

			list.Add( 1061168, "{0}\t{1}", MinDamage.ToString(), MaxDamage.ToString() ); // weapon damage ~1_val~ - ~2_val~

			list.Add( 1061167, Speed.ToString() );

			if ( MaxRange > 1 )
				list.Add( 1061169, MaxRange.ToString() ); // range ~1_val~

			if ( StrRequirement > 0 )
				list.Add( 1061170, StrRequirement.ToString() ); // strength requirement ~1_val~

			if ( Layer == Layer.TwoHanded )
				list.Add( 1061171 ); // two-handed weapon
			else
				list.Add( 1061824 ); // one-handed weapon

			if ( m_Hits >= 0 && m_MaxHits > 0 )
				list.Add( 1060639, "{0}\t{1}", m_Hits, m_MaxHits ); // durability ~1_val~ / ~2_val~
		}

		public override void OnSingleClick( Mobile from )
		{
			List<EquipInfoAttribute> attrs = new List<EquipInfoAttribute>();

			if ( DisplayLootType )
			{
				if ( LootType == LootType.Blessed )
					attrs.Add( new EquipInfoAttribute( 1038021 ) ); // blessed
				else if ( LootType == LootType.Cursed )
					attrs.Add( new EquipInfoAttribute( 1049643 ) ); // cursed
			}

			#region Factions
			if ( m_FactionState != null )
				attrs.Add( new EquipInfoAttribute( 1041350 ) ); // faction item
			#endregion

			if ( m_Quality == WeaponQuality.Exceptional )
				attrs.Add( new EquipInfoAttribute( 1018305 - (int)m_Quality ) );

			if ( m_Identified || from.AccessLevel >= AccessLevel.GameMaster )
			{
				if( m_Slayer != SlayerName.None )
				{
					SlayerEntry entry = SlayerGroup.GetEntryByName( m_Slayer );
					if( entry != null )
						attrs.Add( new EquipInfoAttribute( entry.Title ) );
				}

				if( m_Slayer2 != SlayerName.None )
				{
					SlayerEntry entry = SlayerGroup.GetEntryByName( m_Slayer2 );
					if( entry != null )
						attrs.Add( new EquipInfoAttribute( entry.Title ) );
				}

				if ( m_DurabilityLevel != WeaponDurabilityLevel.Regular )
					attrs.Add( new EquipInfoAttribute( 1038000 + (int)m_DurabilityLevel ) );

				if ( m_DamageLevel != WeaponDamageLevel.Regular )
					attrs.Add( new EquipInfoAttribute( 1038015 + (int)m_DamageLevel ) );

				if ( m_AccuracyLevel != WeaponAccuracyLevel.Regular )
					attrs.Add( new EquipInfoAttribute( 1038010 + (int)m_AccuracyLevel ) );
			}
			else if( m_Slayer != SlayerName.None || m_Slayer2 != SlayerName.None || m_DurabilityLevel != WeaponDurabilityLevel.Regular || m_DamageLevel != WeaponDamageLevel.Regular || m_AccuracyLevel != WeaponAccuracyLevel.Regular )
				attrs.Add( new EquipInfoAttribute( 1038000 ) ); // Unidentified

			if ( m_Poison != null && m_PoisonCharges > 0 )
				attrs.Add( new EquipInfoAttribute( 1017383, m_PoisonCharges ) );

			int number;

			if ( Name == null )
			{
				number = LabelNumber;
			}
			else
			{
				this.LabelTo( from, Name );
				number = 1041000;
			}

			if ( attrs.Count == 0 && Crafter == null && Name != null )
				return;

			EquipmentInfo eqInfo = new EquipmentInfo( number, m_Crafter, false, attrs.ToArray() );

			from.Send( new DisplayEquipmentInfo( this, eqInfo ) );
		}

		#region ICraftable Members

		public int OnCraft( int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue )
		{
			Quality = (WeaponQuality)quality;

			if ( makersMark )
				Crafter = from;

			PlayerConstructed = true;

			Type resourceType = typeRes;

			if ( resourceType == null )
				resourceType = craftItem.Resources.GetAt( 0 ).ItemType;

			if ( tool is BaseRunicTool )
			{
				CraftResource thisResource = CraftResources.GetFromType( resourceType );

				if ( thisResource == ((BaseRunicTool)tool).Resource )
				{
					Resource = thisResource;

					CraftContext context = craftSystem.GetContext( from );

					if ( context != null && context.DoNotColor )
						Hue = 0;

					switch ( thisResource )
					{
						case CraftResource.DullCopper:
						{
							Identified = true;
							DurabilityLevel = WeaponDurabilityLevel.Durable;
							AccuracyLevel = WeaponAccuracyLevel.Accurate;
							break;
						}
						case CraftResource.ShadowIron:
						{
							Identified = true;
							DurabilityLevel = WeaponDurabilityLevel.Durable;
							DamageLevel = WeaponDamageLevel.Ruin;
							break;
						}
						case CraftResource.Copper:
						{
							Identified = true;
							DurabilityLevel = WeaponDurabilityLevel.Fortified;
							DamageLevel = WeaponDamageLevel.Ruin;
							AccuracyLevel = WeaponAccuracyLevel.Surpassingly;
							break;
						}
						case CraftResource.Bronze:
						{
							Identified = true;
							DurabilityLevel = WeaponDurabilityLevel.Fortified;
							DamageLevel = WeaponDamageLevel.Might;
							AccuracyLevel = WeaponAccuracyLevel.Surpassingly;
							break;
						}
						case CraftResource.Gold:
						{
							Identified = true;
							DurabilityLevel = WeaponDurabilityLevel.Indestructible;
							DamageLevel = WeaponDamageLevel.Force;
							AccuracyLevel = WeaponAccuracyLevel.Eminently;
							break;
						}
						case CraftResource.Agapite:
						{
							Identified = true;
							DurabilityLevel = WeaponDurabilityLevel.Indestructible;
							DamageLevel = WeaponDamageLevel.Power;
							AccuracyLevel = WeaponAccuracyLevel.Eminently;
							break;
						}
						case CraftResource.Verite:
						{
							Identified = true;
							DurabilityLevel = WeaponDurabilityLevel.Indestructible;
							DamageLevel = WeaponDamageLevel.Power;
							AccuracyLevel = WeaponAccuracyLevel.Exceedingly;
							break;
						}
						case CraftResource.Valorite:
						{
							Identified = true;
							DurabilityLevel = WeaponDurabilityLevel.Indestructible;
							DamageLevel = WeaponDamageLevel.Vanq;
							AccuracyLevel = WeaponAccuracyLevel.Supremely;
							break;
						}
					}
				}
			}

			return quality;
		}

		#endregion
	}

	public enum CheckSlayerResult
	{
		None,
		Slayer,
		Opposition
	}
}
