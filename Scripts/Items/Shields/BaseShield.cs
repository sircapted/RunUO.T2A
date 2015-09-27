using Server.Network;

namespace Server.Items
{
    public class BaseShield : BaseArmor
	{
		public override ArmorMaterialType MaterialType{ get{ return ArmorMaterialType.Plate; } }

		public BaseShield( int itemID ) : base( itemID )
		{
		}

		public BaseShield( Serial serial ) : base(serial)
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 );//version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			if ( version < 1 )
			{
				if ( this is Aegis )
					return;

				// The 15 bonus points to resistances are not applied to shields on OSI.
				PhysicalBonus = 0;
				FireBonus = 0;
				ColdBonus = 0;
				PoisonBonus = 0;
				EnergyBonus = 0;
			}
		}

		public override double ArmorRating
		{
			get
			{
				Mobile m = this.Parent as Mobile;
				double ar = base.ArmorRating;

				if ( m != null )
					return ( ( m.Skills[SkillName.Parry].Value * ar ) / 200.0 ) + 1.0;
				else
					return ar;
			}
		}

        public override int OnHit(BaseWeapon weapon, int damage)
        {
            Mobile owner = this.Parent as Mobile;
            if (owner == null)
                return damage;

            double ar = this.ArmorRating;
            double chance = (owner.Skills[SkillName.Parry].Value - (ar * 2.0)) / 100.0;

            if (chance < 0.01)
                chance = 0.01;
            /*
            FORMULA: Displayed AR = ((Parrying Skill * Base AR of Shield) � 200) + 1 

            FORMULA: % Chance of Blocking = parry skill - (shieldAR * 2)

            FORMULA: Melee Damage Absorbed = (AR of Shield) / 2 | Archery Damage Absorbed = AR of Shield 
            */
            if (owner.CheckSkill(SkillName.Parry, chance))
            {
                if (weapon.Skill == SkillName.Archery)
                    damage -= (int)ar;
                else
                    damage -= (int)(ar / 2.0);

                if (damage < 0)
                    damage = 0;

                owner.FixedEffect(0x37B9, 10, 16);

                if (25 > Utility.Random(100)) // 25% chance to lower durability
                {
                    int wear = Utility.Random(2);

                    if (wear > 0 && MaxHitPoints > 0)
                    {
                        if (HitPoints >= wear)
                        {
                            HitPoints -= wear;
                            wear = 0;
                        }
                        else
                        {
                            wear -= HitPoints;
                            HitPoints = 0;
                        }

                        if (wear > 0)
                        {
                            if (MaxHitPoints > wear)
                            {
                                MaxHitPoints -= wear;

                                if (Parent is Mobile)
                                    ((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
                            }
                            else
                            {
                                Delete();
                            }
                        }
                    }
                }
            }

            return damage;
        }
    }
}
