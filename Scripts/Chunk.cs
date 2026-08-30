using Godot;
using System;

// An individual chunk
// Contains its own mesh built from a grid of verts
public partial class Chunk : MeshInstance3D
{
	public bool debugToggle = false; // Toggle for Debug printouts
	
	// Placeholder chunk settings for intialization
	public int chunkSize = 20; // The x and y size of an individual chunk
	public int chunkResolution = 20; // The amount of subdivisions within one chunk
	public float noiseHeight = 1; // A multiplier for the noise to control how extreme the differences are between verts
	public FastNoiseLite noise;
	
	public void GenerateChunk()
	{
		int verts = chunkResolution + 1; // The additional +1 is for the boundary triangles
		float step = (float)chunkSize / chunkResolution;
		
		SurfaceTool st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);
		
		// Build a [verts x verts] grid of height-sampled points.
		Vector3[,] points = new Vector3[verts, verts];
		for (int x = 0; x < verts; x++)
		{
			for (int z = 0; z < verts; z++)
			{
				float localX = x * step;
				float localZ = z * step;
				float worldX = Position.X + localX;
				float worldZ = Position.Z + localZ;
				
				float height = noise != null ? noise.GetNoise2D(worldX, worldZ) * noiseHeight : 0f;
				
				points[x, z] = new Vector3(localX, height, localZ);
				
				// Debug Prints
				if (debugToggle) 
				{
					GD.Print("Points: " + points[x, z]);
					GD.Print("Verts: " + verts);
					GD.Print("Step: " + step);
				}
			}
		}
		
		// Create 2 triangles for each grid square
		for (int x = 0; x < chunkResolution; x++)
		{
			for (int z = 0; z < chunkResolution; z++)
			{
				Vector3 a = points[x, z];
				Vector3 b = points[x + 1, z];
				Vector3 c = points[x, z + 1];
				Vector3 d = points[x + 1, z + 1];
 				
				AddTriangle(st, a, b, c); // there was a winding order issue which meant the triangles were only visible from below
				AddTriangle(st, b, d, c);
			}
		}
		
		st.GenerateNormals();
		st.Index();
		
		Mesh = st.Commit();
	}
	
	private void AddTriangle(SurfaceTool st, Vector3 a, Vector3 b, Vector3 c)
	{
		// Simple planar UVs based on local XZ position, scaled to 0-1 per chunk.
		st.SetUV(new Vector2(a.X / chunkSize, a.Z / chunkSize));
		st.AddVertex(a);
		st.SetUV(new Vector2(b.X / chunkSize, b.Z / chunkSize));
		st.AddVertex(b);
		st.SetUV(new Vector2(c.X / chunkSize, c.Z / chunkSize));
		st.AddVertex(c);
	}
}
