using Godot;
using System;
using System.Collections.Generic;

// The Chunk manager
[Tool]
public partial class World : Node3D
{
	public bool debugToggle = false;
	
	// Chunk Settings (These are passed down to each newly generated chunk)
	[Export] public int chunkSize = 7; // The x and y size of an individual chunk
	[Export] public int chunkResolution = 20; // The amount of subdivisions within one chunk
	[Export] public int chunkRenderDistance = 11; // The distance from the camera (in chunks) where chunks will render
	
	// Noise Settings
	[Export] public int frequency = 1; // Controls how smooth/granular the noise is
	[Export] public int noiseHeight = 1; // A multiplier for the noise to control how extreme the differences are between verts
	[Export] public int seed = 1; // Temporary seed, will be available to choose by user eventually
	
	private FastNoiseLite noise; // A shared noise generator so all chunks sample from the same noise
	
	// Tracks currently loaded chunks by their integer chunk coordinate.
	private Dictionary<Vector2I, Chunk> loadedChunks = new Dictionary<Vector2I, Chunk>();
	
	private Camera3D viewer = null;
	
	public override void _Ready()
	{
		Camera3D viewer = GetViewport()?.GetCamera3D();
		
		UpdateChunks();
	}
	
	
	public override void _PhysicsProcess(double delta)
	{
		UpdateChunks();
	}
	
	
	// Finds which chunk the viewer is in, and uses that to load and unload chunks based on chunkRenderDistance
	private void UpdateChunks()
	{
		Node3D viewer = GetViewer();
		if (viewer == null) // If the camera cannot be found, this prevents chunks attempting to render around a point that doesn't exist
		{
			GD.Print("Viewer == null");
			return;
		}
		
		// Takes the current position and converts it into the nearest chunk coordinate
		Vector2I centerChunk = new Vector2I(
			Mathf.FloorToInt(viewer.GlobalPosition.X / chunkSize), 
			Mathf.FloorToInt(viewer.GlobalPosition.Z / chunkSize)
			);
		
		// Creates a list of all the chunks that SHOULD be generated this frame, 
		// and if one should be generated and is not, then it is generated
		List<Vector2I> desired = new List<Vector2I>();
		for (int x = -chunkRenderDistance; x <= chunkRenderDistance; x++)
		{
			for (int z = -chunkRenderDistance; z <= chunkRenderDistance; z++)
			{
				Vector2I coord = new Vector2I(centerChunk.X + x, centerChunk.Y + z);
				
				desired.Add(coord);
				
				// checks if the chunk at coord is generated, and if not then generates it
				if (!loadedChunks.ContainsKey(coord))
				{
					SpawnChunk(coord);
				}
			}
		}
		
		// creates a list of all chunks that are no longer desired 
		List<Vector2I> toRemove = new List<Vector2I>();
		foreach (var (coord, chunk) in loadedChunks)
		{
			if (!desired.Contains(coord))
			{
				toRemove.Add(coord);
			}
		}
		// removes all chunks that are no longer desired
		foreach (Vector2I coord in toRemove)
			DespawnChunk(coord);
	}
	
	
	private void SpawnChunk(Vector2I coord)
	{
		// Configuring the settings of the about-to-be spawned chunk
		Chunk chunk = new Chunk();
		chunk.Name = $"Chunk_{coord.X}_{coord.Y}";
		chunk.chunkSize = chunkSize;
		chunk.chunkResolution = chunkResolution;
		chunk.noiseHeight = noiseHeight;
		chunk.noise = noise;
		
		AddChild(chunk);
		
		chunk.GlobalPosition = new Vector3(coord.X * chunkSize, 0, coord.Y * chunkSize);
		
		chunk.GenerateChunk();
		
		loadedChunks[coord] = chunk;
		
		// Debug Prints
		if (debugToggle)
		{
			GD.Print("Spawned chunk: " + chunk.Name);
			GD.Print("Chunk global location: " + chunk.GlobalPosition);
			GD.Print(loadedChunks[coord]);
		}
	}
	
	
	private void DespawnChunk(Vector2I coord)
	{
		if (loadedChunks[coord] != null)
		{
			// Debug Prints
			if (debugToggle)
			{
				GD.Print("Removed chunk: " + loadedChunks[coord].Name);
			}
			
			loadedChunks[coord].QueueFree();
			loadedChunks.Remove(coord);
		}
	}
	
	
	private Node3D GetViewer()
	{
		if (viewer != null)
		{
			return viewer;
		}
		
		Camera3D cam = GetViewport()?.GetCamera3D();
		return cam;
	}
}
