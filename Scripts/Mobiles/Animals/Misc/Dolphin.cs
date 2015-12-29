namespace Server.Mobiles
{
    [CorpseName( "a dolphin corpse" )]
	public class Dolphin : BaseCreature
	{
		[Constructable]
		public Dolphin()
			: base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			Name = "a dolphin";
			Body = 0x97;
			BaseSoundID = 0x8A;

			SetStr( 21, 49 );
			SetDex( 66, 85 );
			SetInt( 96, 110 );

			SetHits( 15, 27 );

			SetDamage( 3, 6 );

			SetSkill( SkillName.MagicResist, 15.1, 20.0 );
			SetSkill( SkillName.Tactics, 19.2, 29.0 );
			SetSkill( SkillName.Wrestling, 19.2, 29.0 );

			Fame = 500;
			Karma = 2000;

			VirtualArmor = 16;
			CanSwim = true;
			CantWalk = true;
		}

		public override int Meat { get { return 1; } }

		public Dolphin( Serial serial )
			: base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if( from.AccessLevel >= AccessLevel.GameMaster )
				Jump();
		}

		public virtual void Jump()
		{
			if( Utility.RandomBool() )
				Animate( 3, 16, 1, true, false, 0 );
			else
				Animate( 4, 20, 1, true, false, 0 );
		}

		public override void OnThink()
		{
			if( Utility.RandomDouble() < .005 ) // slim chance to jump
				Jump();

			base.OnThink();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}