using System.Collections.Generic;
using System.Linq;
using Godot;

namespace HorrorGame.Scripts;

public partial class LightDetect : Node3D
{
	public double LightLevel { get; set; }
	public SubViewport SubViewport { get; private set; }
	public Camera3D Camera3D { get; private set; }
	public ICollection<float> LightnessList { get; } = new List<float>();

	public override void _Ready()
	{
		SubViewport = GetNode<SubViewport>("Container/SubViewport");
		Camera3D = GetNode<Camera3D>("Container/SubViewport/Camera3D");
	}

	public override void _Process(double delta)
	{
		var image = SubViewport.GetTexture().GetImage();
		LightnessList.Clear();
		for (var y = 0; y < image.GetHeight(); y++)
		{
			for (var x = 0; x < image.GetWidth(); x++)
			{
				var pixel = image.GetPixel(x, y);
				var lightness = (pixel.R + pixel.G + pixel.B) / 3f;
				LightnessList.Add(lightness);
			}
		}

		LightLevel = LightnessList.Average();
		SetCameraPosition();
	}

	private void SetCameraPosition()
	{
		Camera3D.GlobalPosition = new Vector3(GlobalPosition.X, GlobalPosition.Y + 0.3f, GlobalPosition.Z);
	}
}