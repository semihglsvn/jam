using Godot;

public partial class RhythmManager : Node
{
	// 1. We create a custom Godot Signal. 
	// Any platform in the game can "listen" to this signal to know when to glitch!
	[Signal]
	public delegate void BeatTickEventHandler(bool isBeatA);

	private Timer _beatTimer;
	private bool _isBeatA = true; // We start on Beat A

	public override void _Ready()
	{
		// Create the invisible metronome timer in code
		_beatTimer = new Timer();
		_beatTimer.WaitTime = 0.545f; // ~110 BPM (0.545 seconds per beat)
		_beatTimer.Autostart = true;
		_beatTimer.OneShot = false;   // Keep looping forever
		
		// Wire the timer up to our OnBeat function
		_beatTimer.Timeout += OnBeat; 
		
		AddChild(_beatTimer);
	}

	private void OnBeat()
	{
		_isBeatA = !_isBeatA; 
		
		// 2. Broadcast the signal to the whole game
		EmitSignal(SignalName.BeatTick, _isBeatA);
		
		// Let's print it to the console so we can see the backend working!
		if (_isBeatA)
		{
			GD.Print("[ Rhythm ] --- BEAT A (Yellow Blocks Active) ---");
		}
		else
		{
			GD.Print("[ Rhythm ] --- BEAT B (Blue Blocks Active) ---");
		}
	}
}
