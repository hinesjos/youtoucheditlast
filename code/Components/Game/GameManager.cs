using Sandbox;

public sealed class GameManager : Component
{
	[Property]
	public TimeSince GameTime { get; set; } = 300f;
	protected override void OnFixedUpdate()
	{

	}
}
