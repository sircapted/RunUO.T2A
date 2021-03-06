using Server.Network;
using System;

namespace Server
{
    public class CurrentExpansion
	{
		public static void Configure()
		{
			Mobile.InsuranceEnabled = false;
			ObjectPropertyList.Enabled = false;
			Mobile.VisibleDamageType = VisibleDamageType.Related; // None, Related or Everyone
            Mobile.GuildClickMessage = true;
			Mobile.AsciiClickMessage = true;
			Mobile.ActionDelay = TimeSpan.FromSeconds( 1.0 );
		}
	}
}
