using Sandbox;

[Library( "weapon_grenadelauncher", Title = "Grenade Launcher", Spawnable = true )]
partial class GrenadeLauncher : Weapon
{
	public override string ViewModelPath => "weapons/rust_pumpshotgun/v_rust_pumpshotgun.vmdl";
	public override float PrimaryRate => 1f;
	//public override float SecondaryRate => 1f;
	public override float ReloadTime => 0.5f;

	public float detTime = 1f;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" );

		HasAmmo = true;

		AmmoMax = 4;
		AmmoClip = 4;
		AmmoAdd = 1;
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "rust_pumpshotgun.shoot" );

		AmmoClip -= AmmoSub1;

		//
		// Shoot the bullets
		//
		//ShootBullets( 10, 0.1f, 10.0f, 9.0f, 3.0f );
		if ( !IsServer )
			return;
		GrenadeProjectile proj = new GrenadeProjectile();
		proj.Position = Owner.EyePos + Owner.EyeRot.Forward * 70;
		proj.Velocity = Owner.EyeRot.Forward * 2000 + new Vector3(0f, 0f, 120f);
		proj.Rotation = Owner.EyeRot;
		proj.detTime = detTime;
		proj.owner = this;
		//proj.Spawn();
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		ViewModelEntity?.SetAnimBool( "fire", true );

		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin( 1.0f, 1.5f, 2.0f );
		}

		CrosshairPanel?.CreateEvent( "fire" );
	}

	public override void OnReloadFinish()
	{
		base.OnReloadFinish();

		if ( AmmoClip < AmmoMax )
		{
			Reload();
		} else
		{
			FinishReload();
		}
	}

	[ClientRpc]
	protected virtual void FinishReload()
	{
		ViewModelEntity?.SetAnimBool( "reload_finished", true );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 3 ); // TODO this is shit
		anim.SetParam( "aimat_weight", 1.0f );
	}
}
