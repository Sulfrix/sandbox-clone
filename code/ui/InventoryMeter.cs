using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

enum InventoryMeterStyle
{
	Start,
	Middle,
	End
}

class InventoryMeter : Panel
{
	readonly List<Panel> slots = new();

	public InventoryMeter()
	{
		for (int i = 0; i < 9; i++ )
		{
			var newSlot = new Panel();
			slots.Add( newSlot );
		}
	}

	public void updateSlots()
	{
		for (int i = 0; i < slots.Count; i++ )
		{
			var slot = slots[i];

		}
	}

	public void updateSlot(Entity ent, Panel icon, int x, InventoryMeterStyle style)
	{

	} 
}
