using Godot;
using System;

public partial class Terrain : MeshInstance3D
{
	
	public int xSize = 20;
	public int ySize = 20;
	
	void _Ready()
	{
		Console.WriteLine("Test?");
		// This is where we will generate the initial mesh
		Mesh.set_size(Vector2(10, 10));
		
		UpdateMesh();
		
	}
	
	void _process()
	{
		UpdateMesh(); // this is probably not what needs to be done here, a chunking system needs to be implemented
	}

	void UpdateMesh() // Updates the mesh to redraw with any changes
	{
		
	}
}
