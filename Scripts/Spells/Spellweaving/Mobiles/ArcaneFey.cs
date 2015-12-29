namespace Server.Mobiles
{
    [CorpseName( "a pixie corpse" )]
	public class ArcaneFey : BaseCreature
	{
		public override double DispelDifficulty { get { return 70.0; } }
		public override double DispelFocus { get { return 20.0; } }

		public override OppositionGroup OppositionGroup { get { return OppositionGroup.FeyAndUndead; } }
		public override bool InitialInnocent { get { return true; } }

		[Constructable]
		public ArcaneFey() : base( AIType.AI_Mage, FightMode.Evil, 10, 1, 0.2, 0.4 )
		{
			Name = NameList.RandomName( "pixie" );
			Body = 128;
			BaseSoundID = 0x467;

			SetStr( 20 );
			SetDex( 150 );
			SetInt( 125 );

			SetDamage( 9, 15 );

			SetSkill( SkillName.EvalInt, 70.1, 80.0 ); 
			SetSkill( SkillName.Magery, 70.1, 80.0 );
			SetSkill( SkillName.Meditation, 70.1, 80.0 );
			SetSkill( SkillName.MagicResist, 50.5, 100.0 );
			SetSkill( SkillName.Tactics, 10.1, 20.0 );
			SetSkill( SkillName.Wrestling, 10.1, 12.5 );

			Fame = 0;
			Karma = 0;

			ControlSlots = 1;
		}

		public ArcaneFey( Serial serial ) : base( serial )
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