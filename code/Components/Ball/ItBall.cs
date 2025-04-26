using Sandbox;

public sealed class ItBall : Component, Component.ICollisionListener
{
	[Property]
	public Rigidbody BallBody { get; set; }
	[Property, ResourceType("sound")]
	[Sync] public string ThrowingSound { get; set; }
	[Property, ResourceType("sound")]
	[Sync] public string TaggingSound { get; set; }

	protected override void OnStart()
	{
		this.Network.DropOwnership();
		PlaySound();
	}

	[Rpc.Broadcast]
	public void ChangePlayerValues()
	{
		IEnumerable<PlayerInfo> components = Scene.GetAllComponents<PlayerInfo>();

		int n = 0;

		foreach (var component in components)
		{
			Log.Info( $"{n}: {component}" );
			component.IsIt = false;
			component.IsHoldingBall = false;
			n += 1;
		}
	}

	public void OnCollisionStart( Collision other )
	{
		var player = other.Other.GameObject.Parent.Components.Get<PlayerInfo>();

		if( player.IsValid() )
		{			
			ChangePlayerValues();
			TagPlayer( player );

			GameObject.Destroy();
		}
	}

	[Rpc.Broadcast]
	public void TagPlayer( PlayerInfo plr )
	{
		if( plr.IsIt == false )
		{
			Sound.Play( TaggingSound );
			plr.IsIt = true;
		}
		plr.IsHoldingBall = true;
	}

	[Rpc.Broadcast]
	public void PlaySound()
	{
		Sound.Play( ThrowingSound );
	}
}


