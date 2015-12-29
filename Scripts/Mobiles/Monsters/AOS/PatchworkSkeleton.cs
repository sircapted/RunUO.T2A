using Server.Items;

namespace Server.Mobiles
{
    [CorpseName( "a patchwork skeletal corpse" )]
	public class PatchworkSkeleton : BaseCreature
	{
		[Constructable]
		public PatchworkSkeleton() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a patchwork skeleton";
			Body = 309;
			BaseSoundID = 0x48D;

			SetStr( 96, 120 );
			SetDex( 71, 95 );
			SetInt( 16, 40 );

			SetHits( 58, 72 );

			SetDamage( 18, 22 );

			SetSkill( SkillName.MagicResist, 70.1, 95.0 );
			SetSkill( SkillName.Tactics, 55.1, 80.0 );
			SetSkill( SkillName.Wrestling, 50.1, 70.0 );

			Fame = 500;
			Karma = -500;

			VirtualArmor = 54;
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Meager );
		}

		public override bool BleedImmune{ get{ return true; } }
		public override Poison PoisonImmune{ get{ return Poison.Lethal; } }

		public override int TreasureMapLevel{ get{ return 1; } }

		public PatchworkSkeleton( Serial serial ) : base( serial )
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