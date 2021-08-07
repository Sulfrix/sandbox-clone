using Sandbox;
using System;

[Library( "ent_grenade_projectile", Title = "hehehe grenade go boom", Spawnable = false )]
public partial class GrenadeProjectile : Prop
{
	public Entity owner;
	private bool hasTouched = false;
	private float worldImpactTime;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/pipe.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if (!eventData.Entity.IsWorld && eventData.Entity.IsValid())
		{
			if ( !hasTouched && eventData.Entity.LifeState == LifeState.Alive && eventData.Entity.Health > -1)
			{

				Explode(eventData.Entity);
			}
		} else
		{
			if (!hasTouched)
			{
				hasTouched = true;
				worldImpactTime = Time.Now;
				//Velocity = Velocity * 0.5f;
				//AngularVelocity *= 0f;
			}
		}
		//Delete();
	}

	public void Explode( Entity hitEnt )
	{
		if ( !PhysicsBody.IsValid() )
			return;
		var expldamage = 120;
		var explforce = 20;
		var radius = 200;
		var sourcePos = PhysicsBody.MassCenter;
		var overlaps = Physics.GetEntitiesInSphere( sourcePos, radius );

		Sound.FromWorld( "rust_pumpshotgun.shootdouble", PhysicsBody.MassCenter );
		Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", PhysicsBody.MassCenter );

		if ( debug_prop_explosion )
			DebugOverlay.Sphere( sourcePos, radius, Color.Orange, true, 5 );

		// Stolen from prop code, sorry :P
		foreach ( var overlap in overlaps )
		{
			if ( overlap is not ModelEntity ent || !ent.IsValid() )
				continue;

			if ( ent.LifeState != LifeState.Alive )
				continue;

			if ( !ent.PhysicsBody.IsValid() )
				continue;

			if ( ent.IsWorld )
				continue;

			var targetPos = ent.PhysicsBody.MassCenter;

			var dist = Vector3.DistanceBetween( sourcePos, targetPos );
			if ( dist > radius )
				continue;

			var tr = Trace.Ray( sourcePos, targetPos )
				.Ignore( this )
				.WorldOnly()
				.Run();

			if ( tr.Fraction < 1.0f )
			{
				if ( debug_prop_explosion )
					DebugOverlay.Line( sourcePos, tr.EndPos, Color.Red, 5, true );

				continue;
			}

			if ( debug_prop_explosion )
				DebugOverlay.Line( sourcePos, targetPos, 5, true );

			var distanceMul = 1.0f - Math.Clamp( dist / radius, 0.0f, 1.0f );
			var damage = expldamage * distanceMul;
			if ( ent == hitEnt )
			{
				damage = 100;
			}
			var force = (explforce * distanceMul) * ent.PhysicsBody.Mass;
			var forceDir = (targetPos - sourcePos).Normal;

			ent.TakeDamage( DamageInfo.Explosion( sourcePos, forceDir * force, damage )
				.WithAttacker( owner.Owner ).WithWeapon( owner ) );
		}
		Delete();
	}

	[Event.Tick]
	public void Tick()
	{
		if ( !IsServer )
			return;
		if (!hasTouched)
		{
			Rotation = Rotation.LookAt( Velocity.Normal );
		}
		else
		{
			if (Time.Now - worldImpactTime > 0.75	)
			{
				Explode(null);
			}
		}
	}

}
