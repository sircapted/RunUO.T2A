using System;

namespace Server.Mobiles
{
    [CorpseName( "a dark wolf corpse" )]
	public class DarkWolfFamiliar : BaseFamiliar
	{
		public DarkWolfFamiliar()
		{
			Name = "a dark wolf";
			Body = 99;
			Hue = 0x901;
			BaseSoundID = 0xE5;

			SetStr( 100 );
			SetDex( 90 );
			SetInt( 90 );

			SetHits( 60 );
			SetStam( 90 );
			SetMana( 0 );

			SetDamage( 5, 10 );

			SetSkill( SkillName.Wrestling, 85.1, 90.0 );
			SetSkill( SkillName.Tactics, 50.0 );

			ControlSlots = 1;
		}

		private DateTime m_NextRestore;

		public override void OnThink()
		{
			base.OnThink();

			if ( DateTime.Now < m_NextRestore )
				return;

			m_NextRestore = DateTime.Now + TimeSpan.FromSeconds( 2.0 );

			Mobile caster = ControlMaster;

			if ( caster == null )
				caster = SummonMaster;

			if ( caster != null )
				++caster.Stam;
		}

		public DarkWolfFamiliar( Serial serial ) : base( serial )
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