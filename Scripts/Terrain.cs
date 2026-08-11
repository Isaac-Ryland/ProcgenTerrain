using Godot;
using System;

[Tool]
public partial class Terrain : MeshInstance3D
{
	
	[Export] public int xSize = 20;
	[Export] public int ySize = 20;
	
	public PlaneMesh planeMesh;
	
	public override void _Ready()
	{
		GD.Print("HELLOOOOO"); // Debug print
		
		// This is where we will generate the initial mesh
		
		planeMesh = new PlaneMesh();
		planeMesh.Size = new Vector2(xSize, ySize);
		GD.Print(planeMesh.Size); // Debug print
		GD.Print(planeMesh.CenterOffset); // Debug print
		
		Mesh = planeMesh;
		
		UpdateMesh();
		
	}
	
	 public override void _PhysicsProcess(double delta)
	 {
	 	UpdateMesh(); // this is probably not what needs to be done here, a chunking system needs to be implemented
	 }

	void UpdateMesh() // Updates the mesh to redraw with any changes
	{
		// GD.Print("Updating!");
		
	}
}
