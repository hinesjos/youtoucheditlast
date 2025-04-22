using Sandbox;

public sealed class ItBall : Component, Component.ICollisionListener
{
	[Property]
	public Rigidbody BallBody { get; set; }

	protected override void OnStart()
	{
		this.Network.DropOwnership();
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

			Log.Info( player.IsIt );
			GameObject.Destroy();
		}
	}

	[Rpc.Broadcast]
	public void TagPlayer( PlayerInfo plr )
	{
		plr.IsIt = true;
		plr.IsHoldingBall = true;
	}
}


