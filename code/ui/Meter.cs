using Sandbox;
using Sandbox.UI;

class Meter : Panel
{
	public string textPrefix = "";
	public float value { get; private set; }
	public float max;
	public bool round = true;
	public bool showMax = false;

	private Panel background;
	private Panel foreground;
	private Label text;

	public Meter(float value, float max)
	{
		background = Add.Panel();
		background.AddClass( "meterBackground" );
		
		foreground = background.Add.Panel();
		foreground.AddClass( "meterForeground" );

		text = new Label();
		text.AddClass( "meterText" );
		text.Parent = background;

		this.max = max;

		SetValue( value );
	}

	public override void Tick()
	{
		var textValue = value;

		if (round)
		{
			textValue = MathX.CeilToInt(value);
		}

		if ( showMax )
		{
			text.SetText( textPrefix + textValue + " / " + max );
		} 
		else
		{
			text.SetText( textPrefix + textValue );
		}
		
		base.Tick();
	}

	public void SetValue(float x)
	{
		value = x;
		foreground.Style.Width = Length.Percent( (value / max) * 100 );
		foreground.Style.Dirty();
	}
}
