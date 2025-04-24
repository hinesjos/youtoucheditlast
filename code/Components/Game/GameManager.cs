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
		}
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
