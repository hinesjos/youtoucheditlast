using Sandbox;
using System;

public sealed class GameManager : Component
{
	[Property]
	[Sync] public double MaxGameTime { get; set; } = 300f;
	[Property]
	[Sync] public TimeSince TimeCountdown { get; set; };
	[Property]
	[Sync] public double TimeLeft { get; set; }
	[Property]
	[Sync] public String TimeLeftString { get; set; }
	[Property]
	[Sync] public bool PlayerSet { get; set; }
	[Property]
	[Sync] public bool GameEnded { get; set; }
	[Property]
	[Sync] public string PlayerIt { get; set; }
	Random rnd = new Random();
	[Property]
	[Sync] public TimeSince SinceGameEnded { get; set; };

	protected override void OnFixedUpdate()
	{
		SetTimer();

		IEnumerable<PlayerInfo> players = Scene.GetAllComponents<PlayerInfo>();
		int playercount = players.Count();

		if( playercount >= 1 && PlayerSet == false )
		{
			GameStart( players, playercount );
			PlayerSet = true;
		}

		if( TimeLeft == 0 && GameEnded == false )
		{
			GameEnd();
			GameEnded = true;
			SinceGameEnded = 0;
		}

		if( GameEnded == true && SinceGameEnded >= 10f && playercount >= 2 )
		{
			GameStart( players, playercount );
			GameEnded = false;
		}

		else if( GameEnded == true && SinceGameEnded >= 10f && playercount < 2 )
		{
		}

		WhoIsIt();
	}

	[Rpc.Broadcast]
	public void SetTimer()
	{
		TimeLeft = Math.Floor(Math.Clamp( MaxGameTime - TimeCountdown, 0f, MaxGameTime ));
		TimeSpan timespan = TimeSpan.FromSeconds( TimeLeft );
		TimeLeftString = timespan.ToString(@"mm\:ss");
	}

	[Rpc.Broadcast]
	public void GameStart( IEnumerable<PlayerInfo> players, int playercount )
	{
		TimeLeft = MaxGameTime;
		TimeCountdown = 0;

		foreach( var p in players )
		{
			p.IsIt = false;
			p.IsHoldingBall = false;
		}

		List<PlayerInfo> playerslist = players.ToList();

		var player = playerslist[ rnd.Next( playercount )];

		player.IsIt = true;
		player.IsHoldingBall = true;
		PlayerIt = player.PlayerName;
	}

	[Rpc.Broadcast]
	public void GameEnd()
	{
		IEnumerable<PlayerInfo> players = Scene.GetAllComponents<PlayerInfo>();

		foreach( var p in players )
		{
			if( p.IsIt == true )
			{
				Log.Info( $"{p.PlayerName} loses!");
				Log.Info($"{p.PlayerName}: {p.IsIt}");
				var spec = p.GameObject.Parent.Components.Get<Spectator>();
				if( spec != null )
				{
					spec.IsSpectating = true;
				}
				p.GameObject.Destroy();
			}
		}


	}

	[Rpc.Broadcast]
	public void WhoIsIt()
	{
		IEnumerable<PlayerInfo> players = Scene.GetAllComponents<PlayerInfo>();

		foreach( var p in players )
		{
			if( p.IsIt == true )
			{
				PlayerIt = p.PlayerName;
			}
		}
	}
}
