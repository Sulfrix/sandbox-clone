using Sandbox;

public partial class Weapon : BaseWeapon, IUse
{
	public virtual float ReloadTime => 3.0f;

	public PickupTrigger PickupTrigger { get; protected set; }

	[Net, Predicted]
	public TimeSince TimeSinceReload { get; set; }

	[Net, Predicted]
	public bool IsReloading { get; set; }

	[Net, Predicted]
	public TimeSince TimeSinceDeployed { get; set; }

	[Net, Predicted]
	public int AmmoMax { get; set; } = 8;

	[Net, Predicted]
	public int AmmoClip { get; set; } = 8;

	[Net]
	public int AmmoAdd { get; set; } = 8;

	[Net]
	public int AmmoSub1 { get; set; } = 1;

	[Net]
	public int AmmoSub2 { get; set; } = 2;

	[Net]
	public bool HasAmmo { get; set; } = false;

	public override void Spawn()
	{
		base.Spawn();

		PickupTrigger = new PickupTrigger
		{
			Parent = this,
			Position = Position,
			EnableTouch = true,
			EnableSelfCollisions = false
		};

		PickupTrigger.PhysicsBody.EnableAutoSleeping = false;
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		TimeSinceDeployed = 0;
	}

	public override bool CanPrimaryAttack()
	{
		var answer = base.CanPrimaryAttack();
		if ( AmmoClip < AmmoSub1 && HasAmmo )
		{
			if (answer)
			{
				Reload();
			}
			answer = false;
		}
		return answer;
	}

	public override bool CanSecondaryAttack()
	{
		var answer = base.CanSecondaryAttack();
		if ( AmmoClip < AmmoSub2 && HasAmmo )
		{
			if ( answer )
			{
				Reload();
			}
			answer = false;
		}
		return answer;
	}

	public override void Reload()
	{
		if ( IsReloading )
			return;

		if ( AmmoClip >= AmmoMax )
			return;

		TimeSinceReload = 0;
		IsReloading = true;


		StartReloadEffects();
	}

	public override void Simulate( Client player )
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

		if ( CanReload() )
		{
			Reload();
		}

		//
		// Reload could have deleted us
		//
		if ( !this.IsValid() )
			return;

		if ( CanPrimaryAttack() )
		{
			TimeSincePrimaryAttack = 0;
			IsReloading = false;
			AttackPrimary();
		}

		//
		// AttackPrimary could have deleted us
		//
		if ( !player.IsValid() )
			return;

		if ( CanSecondaryAttack() )
		{
			TimeSinceSecondaryAttack = 0;
			IsReloading = false;
			AttackSecondary();
		}

		if ( AmmoClip > AmmoMax )
		{
			AmmoClip = AmmoMax;
		}

		if ( IsReloading && TimeSinceReload > ReloadTime )
		{
			OnReloadFinish();
		}
	}

	public override bool CanReload()
	{
		if ( TimeSincePrimaryAttack < ( 1 / PrimaryRate ) )
		{
			return false;
		}
		if (TimeSinceSecondaryAttack < ( 1 / SecondaryRate ) )
		{
			return false;
		}
		return base.CanReload();
	}

	public virtual void OnReloadFinish()
	{
		IsReloading = false;
		AmmoClip += AmmoAdd;
	}

	[ClientRpc]
	public virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimBool( "reload", true );
		(Owner as AnimEntity)?.SetAnimBool( "b_reload", true );

		// TODO - player third person model reload
	}

	public override void CreateViewModel()
	{
		Host.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		ViewModelEntity = new ViewModel
		{
			Position = Position,
			Owner = Owner,
			EnableViewmodelRendering = true
		};

		ViewModelEntity.SetModel( ViewModelPath );
	}

	public bool OnUse( Entity user )
	{
		if ( Owner != null )
			return false;

		if ( !user.IsValid() )
			return false;

		user.StartTouch( this );

		return false;
	}

	public virtual bool IsUsable( Entity user )
	{
		if ( Owner != null ) return false;

		if ( user.Inventory is Inventory inventory )
		{
			return inventory.CanAdd( this );
		}

		return true;
	}

	public void Remove()
	{
		PhysicsGroup?.Wake();
		Delete();
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		if ( IsLocalPawn )
		{
			_ = new Sandbox.ScreenShake.Perlin();
		}

		ViewModelEntity?.SetAnimBool( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

	/// <summary>
	/// Shoot a single bullet
	/// </summary>
	public virtual void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize )
	{
		var forward = dir;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;

		//
		// ShootBullet is coded in a way where we can have bullets pass through shit
		// or bounce off shit, in which case it'll return multiple results
		//
		foreach ( var tr in TraceBullet( pos, pos + forward * 5000, bulletSize ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !IsServer ) continue;
			if ( !tr.Entity.IsValid() ) continue;

			//
			// We turn predictiuon off for this, so any exploding effects don't get culled etc
			//
			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}
	}

	/// <summary>
	/// Shoot a single bullet from owners view point
	/// </summary>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		ShootBullet( Owner.EyePos, Owner.EyeRot.Forward, spread, force, damage, bulletSize );
	}

	/// <summary>
	/// Shoot a multiple bullets from owners view point
	/// </summary>
	public virtual void ShootBullets( int numBullets, float spread, float force, float damage, float bulletSize )
	{
		var pos = Owner.EyePos;
		var dir = Owner.EyeRot.Forward;

		for ( int i = 0; i < numBullets; i++ )
		{
			ShootBullet( pos, dir, spread, force / numBullets, damage, bulletSize );
		}
	}
}
