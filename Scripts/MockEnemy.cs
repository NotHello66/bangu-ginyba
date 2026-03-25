using System;
using Vector3 = System.Numerics.Vector3;

public partial class MockEnemy
{
	public HealthModel HealthModel { get; set; }
	public Vector3 GlobalPosition { get; set; }

	public MockEnemy(HealthModel healthModel, Vector3 globalPosition)
	{
		this.HealthModel = healthModel;
		this.GlobalPosition = globalPosition;
	}
}
