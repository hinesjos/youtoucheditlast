using Sandbox;

public sealed class ItBall : Component, Component.ICollisionListener
{
	[Property]
	public Rigidbody BallBody { get; set; }
	public void OnCollisionStart( Collision other )
	{
		var player = other.Other.GameObject.Parent.Components.Get<PlayerInfo>();

		if( player.IsValid() )
		{
			player.IsIt = true;
			player.IsHoldingBall = true;

			GameObject.Destroy();
		}
	}
}


