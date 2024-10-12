using Godot;
using System;
using System.Runtime.CompilerServices;

[Tool]
[GlobalClass]
public partial class HealthIndicator : Control
{
	private float[] samples = new float[60];
	private float currSample = 0;
	private float currTime = 0;
	private float currValue = 1;
	[Export] private float pulseLenght = 2;
	[Export] private float sampleSpeed = 2;
	[Export] private float decayLength = 15;
	[Export] private Curve curve;
	[Export] public float SpeedMult = 1f;
	[Export] public Color LineColor = Colors.Green;
    public override void _Process(double delta)
    {
		var d = (float)delta;
		// Update wave
		currTime -= d;
		if (currTime < 0) {
			currValue = 0f;
			currTime = pulseLenght * SpeedMult;
		}
		currValue = Mathf.MoveToward(currValue, 1f, d / SpeedMult);
		currSample += d * sampleSpeed;
		// Draw sample
		while (currSample >= samples.Length) currSample -= samples.Length;
		int currSampleIdx = (int)currSample;
		samples[currSampleIdx] = currValue;
		QueueRedraw();
    }
    public override void _Draw()
    {
		var startPoint = Vector2.Zero;
		var endPoint = Size + startPoint;
		int len = (int)(endPoint.X - startPoint.X);
		for (int x = (int)startPoint.X; x < endPoint.X; x++) {
			int idx = x * samples.Length / len;
			int pIdx = (idx <= 0) ? samples.Length-1 : idx-1;
			float currL = curve.Sample(samples[idx]) + .5f;
			float prevL = curve.Sample(samples[pIdx]) + .5f;
			int currY = (int)Mathf.Lerp(startPoint.Y, endPoint.Y, currL);
			int prevY = (int)Mathf.Lerp(startPoint.Y, endPoint.Y, prevL);
			int y = Math.Min(currY,prevY);
			int h = Math.Max(currY,prevY)-y; if(h<1) h=1;
			var r = new Rect2(x, y, 1, h);
			float decay = 1f;
			if (idx > currSample) {
				// Idx is to the right of currSample
				var currSampleWrap = currSample + samples.Length;
				decay = 1 - (currSampleWrap - idx) / decayLength;
			} else {
				// Idx is to the left of currSample
				decay = 1 - (currSample - idx) / decayLength;
			}
			// 
			var c = new Color(LineColor, decay);
			DrawRect(r, c);
		}
    }
}
