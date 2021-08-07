namespace Sandbox.Tools
{
	[Library( "tool_pawncontrol", Title = "Pawn Control", Description = "Possession", Group = "construction" )]
	public partial class PawnControl : BaseTool
	{
		public override void Simulate()
		{
			//if ( !Host.IsServer )
				//return;

			if ( !Input.Pressed( InputButton.Attack1 ) )
				return;

			//Owner.Health = 0;
			//Owner.OnKilled();
			//Log.Error( "Disabled!" );

			var startPos = Owner.EyePos;
			var dir = Owner.EyeRot.Forward;
			var client = Owner.GetClientOwner();

			var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
				.Ignore( Owner )
				.HitLayer( CollisionLayer.Debris )
				.Run();

			if ( !tr.Hit || !tr.Entity.IsValid() )
				return;

			CreateHitEffects( tr.EndPos );

			if ( tr.Entity.IsWorld )
				return;



			if ( tr.Entity is Player )
			{
				if ( tr.Entity.GetClientOwner() == null )
				{
					Owner.ActiveChild = null;
					Input.ActiveChild = null;
					Owner.Owner = null;
					//client.Camera = tr.Entity.Camera;
					client.Pawn = tr.Entity;
					//Owner.Respawn();
				}
				else
				{
					Log.Error( "That pawn belongs to " + tr.Entity.GetClientOwner().Name + "!" );
				}
			}

			//using ( Prediction.Off() )
			//{
				
			//}
		}
	}
}
