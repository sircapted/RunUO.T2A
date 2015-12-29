namespace Server.Mobiles
{
    [CorpseName( "an undead horse corpse" )]
	public class SkeletalMount : BaseMount
	{
		[Constructable] 
		public SkeletalMount() : this( "a skeletal steed" )
		{
		}

		[Constructable]
		public SkeletalMount( string name ) : base( name, 793, 0x3EBB, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			SetStr( 91, 100 );
			SetDex( 46, 55 );
			SetInt( 46, 60 );

			SetHits( 41, 50 );

			SetDamage( 5, 12 );

			SetSkill( SkillName.MagicResist, 95.1, 100.0 );
			SetSkill( SkillName.Tactics, 50.0 );
			SetSkill( SkillName.Wrestling, 70.1, 80.0 );

			Fame = 0;
			Karma = 0;
		}

		public override Poison PoisonImmune{ get{ return Poison.Lethal; } }
		public override bool BleedImmune{ get{ return true; } }

		public SkeletalMount( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch( version )
			{
				case 0:
				{
					Name = "a skeletal steed";
					Tamable = false;
					MinTameSkill = 0.0;
					ControlSlots = 0;
					break;
				}
			}
		}
	}
}