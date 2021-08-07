using Sandbox;
using Sandbox.UI;

class CenterHud : Panel
{
	private Meter healthMeter;

	private Panel rowPanel;
	private Label ammoCounter;

	public CenterHud()
	{
		healthMeter = new Meter(50f, 100f);
		healthMeter.textPrefix = "+ ";
		healthMeter.Parent = this;
		healthMeter.AddClass( "healthMeter" );

		AddChild<InventoryBar>();

		rowPanel = new Panel();
		rowPanel.SetClass( "rowPanel", true );
		rowPanel.Parent = this;

		ammoCounter = new Label();
		ammoCounter.Text = "0 / 0";
		ammoCounter.SetClass( "ammoCounter", true );
		ammoCounter.Parent = rowPanel;
	}

	public override void Tick()
	{
		base.Tick();
		if (Local.Pawn != null)
		{
			healthMeter.SetValue( Local.Pawn.Health );

			if ( Local.Pawn.ActiveChild.IsValid() && Local.Pawn.ActiveChild is Weapon )
			{
				var w = (Weapon)Local.Pawn.ActiveChild;
				if (w.HasAmmo)
				{
					ammoCounter.SetClass( "hidden", false );
					ammoCounter.SetText( $"{w.AmmoClip} / {w.AmmoMax}" );
				} else
				{
					ammoCounter.SetClass( "hidden", true );
				}
			}
			else
			{
				ammoCounter.SetClass( "hidden", true );
			}
		}
	}

	[Event( "buildinput" )]
	public void ProcessClientInput( InputBuilder input )
	{
		var player = Local.Pawn as Player;
		if ( player == null )
			return;

		var inventory = player.Inventory;
		if ( inventory == null )
			return;

		if ( player.ActiveChild is PhysGun physgun && physgun.BeamActive )
		{
			return;
		}

		if ( input.Pressed( InputButton.Slot1 ) ) SetActiveSlot( input, inventory, 0 );
		if ( input.Pressed( InputButton.Slot2 ) ) SetActiveSlot( input, inventory, 1 );
		if ( input.Pressed( InputButton.Slot3 ) ) SetActiveSlot( input, inventory, 2 );
		if ( input.Pressed( InputButton.Slot4 ) ) SetActiveSlot( input, inventory, 3 );
		if ( input.Pressed( InputButton.Slot5 ) ) SetActiveSlot( input, inventory, 4 );
		if ( input.Pressed( InputButton.Slot6 ) ) SetActiveSlot( input, inventory, 5 );
		if ( input.Pressed( InputButton.Slot7 ) ) SetActiveSlot( input, inventory, 6 );
		if ( input.Pressed( InputButton.Slot8 ) ) SetActiveSlot( input, inventory, 7 );
		if ( input.Pressed( InputButton.Slot9 ) ) SetActiveSlot( input, inventory, 8 );

		if ( input.MouseWheel != 0 ) SwitchActiveSlot( input, inventory, -input.MouseWheel );
	}

	private static void SetActiveSlot( InputBuilder input, IBaseInventory inventory, int i )
	{
		var player = Local.Pawn;
		if ( player == null )
			return;

		var ent = inventory.GetSlot( i );
		if ( player.ActiveChild == ent )
			return;

		if ( ent == null )
			return;

		input.ActiveChild = ent;
	}

	private static void SwitchActiveSlot( InputBuilder input, IBaseInventory inventory, int idelta )
	{
		var count = inventory.Count();
		if ( count == 0 ) return;

		var slot = inventory.GetActiveSlot();
		var nextSlot = slot + idelta;

		while ( nextSlot < 0 ) nextSlot += count;
		while ( nextSlot >= count ) nextSlot -= count;

		SetActiveSlot( input, inventory, nextSlot );
	}
}
