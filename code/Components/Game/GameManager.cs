using Sandbox;
using System;

public sealed class GameManager : Component
{
	[Property]
	public double MaxGameTime { get; set; } = 300f;
	[Property]
	public TimeSince TimeCountdown { get; set; }
	[Property]
	public double TimeLeft { get; set; }
	[Property]
	[Sync(SyncFlags.FromHost)] public String TimeLeftString { get; set; }
	[Property]
	[Sync(SyncFlags.FromHost)] public bool PlayerSet { get; set; }
	[Property]
	[Sync(SyncFlags.FromHost)] public bool GameEnded { get; set; }
	[Property]
	[Sync(SyncFlags.FromHost)] public string PlayerIt { get; set; }
	Random rnd = new Random();
	[Property]
	public TimeSince SinceGameEnded { get; set; }
	[Property]
	public TimeSpan timespan { get; set; }
	[Sync] public TimeUntil Timer { get; set; } = new();

    [ConCmd( "timer" )] //client request
    static void TimerCommand()
    {
        Log.Error( Game.ActiveScene.GetAllComponents<GameManager>().First().Timer.Passed );
    }

	protected override void OnFixedUpdate()
	{		
		IEnumerable<PlayerInfo> players = Scene.GetAllComponents<PlayerInfo>();
		if( players == null ) return;
		
		int playercount = players.Count();

		if( playercount >= 1 && PlayerSet == false )
		{
			GameStart( players, playercount );
			PlayerSet = true;
		}

		if( GameEnded == false )
		{
			SetTimer();
		}

		if( TimeCountdown == 0 && GameEnded == false )
		{
			GameEnd( players );
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
	}

	[Rpc.Broadcast]
	public void SetTimer()
	{
		TimeLeft = Math.Floor(Math.Clamp( MaxGameTime - TimeCountdown, 0f, MaxGameTime ));
		timespan = TimeSpan.FromSeconds( TimeLeft );
		TimeLeftString = timespan.ToString(@"mm\:ss");
	}

	[Rpc.Broadcast]
	public void GameStart( IEnumerable<PlayerInfo> players, int playercount )
	{
		TimeLeft = 0f;

		foreach( var p in players )
		{
			if( p != null )
			{
				p.IsIt = false;
				p.IsHoldingBall = false;	
			}
		}

		List<PlayerInfo> playerslist = players.ToList();

		var player = playerslist[ rnd.Next( playercount )];

		if( player != null )
		{
			player.IsIt = true;
			player.IsHoldingBall = true;
			PlayerIt = player.PlayerName;
		}
	}

	[Rpc.Broadcast]
	public void GameEnd( IEnumerable<PlayerInfo> players )
	{
		foreach( var p in players )
		{
			if( p.IsIt == true )
			{
				Log.Info( $"{p.PlayerName} loses!");
				Log.Info($"{p.PlayerName}: {p.IsIt}");
				var spec = p.GameObject.Parent.Components.Get<Spectator>();
				//if( spec != null )
				//{
				//	spec.IsSpectating = true;
				//}
				//p.GameObject.Destroy();
			}
		}


	}
}
