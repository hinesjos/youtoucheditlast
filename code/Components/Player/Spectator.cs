using Sandbox;

public sealed class Spectator : Component
{
	[Property]
	[Sync] public bool IsSpectating { get; set; }

public static Spectator Local
	{
		get
		{
			if( !_local.IsValid() )
			{
				_local = Game.ActiveScene.GetAllComponents<Spectator>().FirstOrDefault( x => x.Network.IsOwner );
			}
			return _local;
		}
	}

	private static Spectator _local = null;
}
