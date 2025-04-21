using Sandbox;
using System;
using System.Linq;

public sealed class PlayerInfo : Component
{
//All of the player components
	[Property]
	private ModelRenderer PlayerMdl { get; set; }
	[Property]
	private PlayerController player { get; set; }
	[Property]
	private GameObject ItBallMdl { get; set; }
	[Property]
	[Sync] private GameObject PlayerCamera { get; set; }

//All of the running components
	[Sync] public float CurrentStamina { get; set; }
	[Sync] public bool IsRunning { get; set; }
	[Property]
	public float MaxStamina { get; set; } = 100f;
	[Property]
	public float StaminaDrainRate { get; set; } = 20f;
	[Property]
	public float StaminaRegenRate { get; set; } = 10f;
	[Property]
	public float StaminaRegenDelay { get; set; } = 1f;
	[Sync] public float RunningSpeed { get; set; }

//All of the ball logic
	[Property]
	[Sync] public bool IsIt { get; set; } = false;
	[Property]
	public GameObject ItBallPrefab { get; set; }
	[Property]
	[Sync] public bool IsHoldingBall { get; set; } = false;

//All of the ball throwing logic
	[Property]
	public float BallThrowMax { get; set; } = 750f;
	[Property]
	public float BallThrowMin { get; set; } = 100f;
	[Property]
	public float BallOscillationSpeed { get; set; } = 2f;
	
	[Sync] public bool IsCharging {get; set; } = false;
	[Sync] public bool OscillatingUp { get; set; } = true;
	[Sync] public float OscillationTime { get; set; }
	[Sync] public float CurrentBallThrow { get; set; }
	[Sync] public Vector3 CameraPosition { get; set; }
	[Sync] public Angles CameraAngles { get; set; }

	[Sync] TimeSince _lastRun { get; set; };

	public static PlayerInfo Local
	{
		get
		{
			if( !_local.IsValid() )
			{
				_local = Game.ActiveScene.GetAllComponents<PlayerInfo>().FirstOrDefault( x => x.Network.IsOwner );
			}
			return _local;
		}
	}

	private static PlayerInfo _local = null;

	protected override void OnStart()
	{
		if( Network.IsProxy ) return;

		IEnumerable<CameraComponent> cam = Scene.GetAll<CameraComponent>();
		var pcam = cam.FirstOrDefault();

		PlayerCamera = pcam.GameObject;

		player = PlayerInfo.Local.player;
		PlayerMdl = PlayerInfo.Local.PlayerMdl;
		ItBallMdl = PlayerInfo.Local.ItBallMdl;
		PlayerCamera = PlayerInfo.Local.PlayerCamera;

		ItBallMdl.SetParent( PlayerCamera );

		PlayerInfo.Local.ItBallMdl.LocalPosition = new Vector3( ItBallMdl.LocalPosition.x, ItBallMdl.LocalPosition.y, -15f );

		IsIt = false;
		IsRunning = false;
		CurrentStamina = MaxStamina;
		RunningSpeed = player.RunSpeed;

		ItBallMdl.WorldScale = new Vector3( 0, 0, 0 );
	}

	protected override void OnFixedUpdate()
	{
		if( Network.IsProxy ) return;

		PlayerActions();
		UpdateStamina();
		PlayerIt();
	}

	public void PlayerActions()
	{
		if( Input.Down("Run") && CurrentStamina > 0f )
		{
			IsRunning = true;
			player.RunSpeed = RunningSpeed; 
		}
		else if( !Input.Down("Run") || CurrentStamina == 0f )
		{
			IsRunning = false;
			player.RunSpeed = player.WalkSpeed;
		}

		if( Input.Released( "Attack1" ) && IsIt == true )
		{
			CameraPosition = PlayerCamera.WorldPosition;
			CameraAngles = PlayerCamera.WorldRotation;
			ThrowBall();

			IsIt = false;
			IsHoldingBall = false;

			CurrentBallThrow = 0f;
		}

		if( Input.Down( "Attack1" )  && IsIt == true )
		{
			IsCharging = true;
		}

		else
		{
			IsCharging = false;
			OscillationTime = 0f;
			OscillatingUp = true;
		}

		if( IsCharging )
		{
			float delta = Time.Delta * BallOscillationSpeed;
			if( OscillatingUp )
			{
				CurrentBallThrow += delta * ( BallThrowMax - BallThrowMin );
				if( CurrentBallThrow >= BallThrowMax )
				{
					CurrentBallThrow = BallThrowMax;
					OscillatingUp = false;
				}
			}
			else
			{
				CurrentBallThrow -= delta * (BallThrowMax - BallThrowMin);
				if( CurrentBallThrow <= BallThrowMin )
				{
					CurrentBallThrow = BallThrowMin;
					OscillatingUp = true;
				}
			}
		}
	}

	public void PlayerIt()
	{
		if( IsIt == true )
		{
			 PlayerMdl.Tint = new Color( 1.0f, 0.25f, 0.25f );

			if( IsHoldingBall == true )
			{
				//ItBallMdl.WorldScale = new Vector3( 0.2f, 0.2f, 0.2f );
			}
		}

		else if ( IsIt == false )
		{
			PlayerMdl.Tint = new Color( 1.0f, 1.0f, 1.0f );
			ItBallMdl.WorldScale = new Vector3( 0, 0, 0 );
		} 
	}

	public void UpdateStamina()
	{
		if( IsRunning == true )
		{
			CurrentStamina = Math.Clamp( CurrentStamina - (StaminaDrainRate * Time.Delta), 0f, MaxStamina );

			_lastRun = 0f;
		}

		else if( _lastRun > StaminaRegenDelay )
		{
			CurrentStamina = Math.Clamp( CurrentStamina + (StaminaRegenRate * Time.Delta), 0f, MaxStamina );
		}
	}

	public void ThrowBall()
	{
		GameObject ItBallSpawn = ItBallPrefab.Clone( CameraPosition + (CameraAngles.Forward * 75f) );

		bool success = ItBallSpawn.NetworkSpawn();

		if( success )
		{
			var ballComponent = ItBallSpawn.Components.Get<ItBall>();
			if (ballComponent != null)
			{
				var forward = CameraAngles.Forward;
				ballComponent.BallBody.Velocity = forward * CurrentBallThrow;
			}
		}
	}
}
