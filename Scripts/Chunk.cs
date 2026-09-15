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
	
	public StandardMaterial3D terrainMaterial;
	public float oceanTop = 0.30f;
	public float beachTop = 0.34f;
	public float grassTop = 0.60f;
	public float rockTop = 0.85f;
	
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
				Vector3 topLeftVert = points[x, z];
				Vector3 topRightVert = points[x + 1, z];
				Vector3 bottomLeftVert = points[x, z + 1];
				Vector3 bottomRightVert = points[x + 1, z + 1];
 				
				AddTriangle(st, topLeftVert, topRightVert, bottomLeftVert);
				AddTriangle(st, topRightVert, bottomRightVert, bottomLeftVert);
			}
		}
		
		st.GenerateNormals();
		st.Index();
		
		Mesh = st.Commit();
		Mesh.SurfaceSetMaterial(0, terrainMaterial);
	}
	
	private static readonly Color OceanColour = new Color(0.10f, 0.30f, 0.65f);
	private static readonly Color BeachColour = new Color(0.90f, 0.85f, 0.55f);
	private static readonly Color GrassColour = new Color(0.30f, 0.55f, 0.20f);
	private static readonly Color RockColour = new Color(0.30f, 0.30f, 0.30f);
	private static readonly Color SnowColour = new Color(1.0f, 1.0f, 1.0f);
	
	private Color GetTerrainColour(float height)
	{
		// heightFraction is in world units; normalize against the noiseHeight range
		float heightFraction = Mathf.InverseLerp(-noiseHeight, noiseHeight, height);
		
		if (heightFraction < oceanTop) return OceanColour;
		if (heightFraction < beachTop) return BeachColour;
		if (heightFraction < grassTop) return GrassColour;
		if (heightFraction < rockTop) return RockColour;
		return SnowColour;
	}
	
	
	private void AddTriangle(SurfaceTool st, Vector3 vertA, Vector3 vertB, Vector3 vertC)
	{
		st.SetColor(GetTerrainColour(vertA.Y));
		st.SetUV(new Vector2(vertA.X / chunkSize, vertA.Z / chunkSize));
		st.AddVertex(vertA);
		
		st.SetColor(GetTerrainColour(vertB.Y));
		st.SetUV(new Vector2(vertB.X / chunkSize, vertB.Z / chunkSize));
		st.AddVertex(vertB);
		
		st.SetColor(GetTerrainColour(vertC.Y));
		st.SetUV(new Vector2(vertC.X / chunkSize, vertC.Z / chunkSize));
		st.AddVertex(vertC);
	}
}
