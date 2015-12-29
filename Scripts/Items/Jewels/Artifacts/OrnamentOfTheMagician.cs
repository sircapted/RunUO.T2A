namespace Server.Items
{
    public class OrnamentOfTheMagician : GoldBracelet
	{
		public override int LabelNumber{ get{ return 1061105; } } // Ornament of the Magician
		public override int ArtifactRarity{ get{ return 11; } }

		[Constructable]
		public OrnamentOfTheMagician()
		{
			Hue = 0x554;
		}

		public OrnamentOfTheMagician( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			if ( Hue == 0x12B )
				Hue = 0x554;
		}
	}
}