using Server.Items;

namespace Server.Mobiles
{
    [CorpseName( "a revenant lion corpse" )]
	public class RevenantLion : BaseCreature
	{
		[Constructable]
		public RevenantLion() : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a Revenant Lion";
			Body = 251;

			SetStr( 276, 325 );
			SetDex( 156, 175 );
			SetInt( 76, 105 );

			SetHits( 251, 280 );

			SetDamage( 18, 24 );

			SetSkill( SkillName.EvalInt, 80.1, 90.0 );
			SetSkill( SkillName.Magery, 80.1, 90.0 );
			SetSkill( SkillName.Poisoning, 120.1, 130.0 );
			SetSkill( SkillName.MagicResist, 70.1, 90.0 );
			SetSkill( SkillName.Tactics, 60.1, 80.0 );
			SetSkill( SkillName.Wrestling, 80.1, 88.0 );

			Fame = 4000;
			Karma = -4000;
			PackNecroReg( 6, 8 );
			
			switch ( Utility.Random( 10 ))
			{
				case 0: PackItem( new LeftArm() ); break;
				case 1: PackItem( new RightArm() ); break;
				case 2: PackItem( new Torso() ); break;
				case 3: PackItem( new Bone() ); break;
				case 4: PackItem( new RibCage() ); break;
				case 5: PackItem( new RibCage() ); break;
				case 6: PackItem( new BonePile() ); break;
				case 7: PackItem( new BonePile() ); break;
				case 8: PackItem( new BonePile() ); break;
				case 9: PackItem( new BonePile() ); break;
			}
		}

		public override int GetAngerSound()
		{
			return 0x518;
		}

		public override int GetIdleSound()
		{
			return 0x517;
		}

		public override int GetAttackSound()
		{
			return 0x516;
		}

		public override int GetHurtSound()
		{
			return 0x519;
		}

		public override int GetDeathSound()
		{
			return 0x515;
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Rich, 2 );
			AddLoot( LootPack.MedScrolls, 2 );

			// TODO: Bone Pile
		}

		public override bool BleedImmune{ get{ return true; } }
		public override Poison PoisonImmune{ get{ return Poison.Greater; } }
		public override Poison HitPoison{ get{ return Poison.Greater; } }

		public RevenantLion( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
