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
	[Export] public int chunkRenderDistance = 25; // The distance from the camera (in chunks) where chunks will render
	[Export] public int chunksPerFrame = 15; // The maximum amount of chunks being spawned per frame
	
	// Noise Settings
	[Export] public float noiseFrequency = 0.004f; // Controls how smooth/granular the noise is
	[Export] public int noiseHeight = 80; // A multiplier for the noise to control how extreme the differences are between verts
	[Export] public int noiseSeed = 1; // Temporary seed, will be available to choose by user eventually
	
	private FastNoiseLite noise; // A shared noise generator so all chunks sample from the same noise
	
	// Colour band thresholds, as fractions of noiseHeight (0 = lowest possible point, 1 = highest)
	[Export] public float oceanTop = 0.30f;
	[Export] public float beachTop = 0.35f;
	[Export] public float grassTop = 0.60f;
	[Export] public float rockTop = 0.80f;
	// Anything above rockTop is snow
	
	// Tracks currently loaded chunks by their integer chunk coordinate.
	private Dictionary<Vector2I, Chunk> loadedChunks = new Dictionary<Vector2I, Chunk>();
	
	private Queue<Vector2I> spawnQueue = new Queue<Vector2I>(); // It refusing to colour 'Queue' is irritating me :/
	private HashSet<Vector2I> queuedCoords = new HashSet<Vector2I>(); 
	// Using a HashSet to allow for fast lookups as it is an 0(1) search instead of an O(n) search
	
	private Camera3D viewer;
	
	private StandardMaterial3D terrainMaterial;
	
	public override void _Ready()
	{
		noise = new FastNoiseLite();
		noise.Seed = noiseSeed;
		noise.Frequency = noiseFrequency;
		
		terrainMaterial = new StandardMaterial3D();
		terrainMaterial.VertexColorUseAsAlbedo = true;
		
		viewer = GetViewport()?.GetCamera3D();
		
		UpdateDesiredChunks();
		ProcessSpawnQueue();
	}
	
	
	public override void _PhysicsProcess(double delta)
	{
		// Updates Seed and Frequency so noise can be tweaked without a rebuild
		noise.Seed = noiseSeed;
		noise.Frequency = noiseFrequency;
		
		UpdateDesiredChunks();
		ProcessSpawnQueue();
	}
	
	
	// Finds which chunk the viewer is in, and uses that to load and unload chunks based on chunkRenderDistance
	private void UpdateDesiredChunks()
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
		HashSet<Vector2I> chunksInRange = new HashSet<Vector2I>();
		int renderDistanceSquared = chunkRenderDistance * chunkRenderDistance;
		
		for (int x = -chunkRenderDistance; x <= chunkRenderDistance; x++)
		{
			for (int z = -chunkRenderDistance; z <= chunkRenderDistance; z++)
			{
				// If the point is outside the circular render distance, skip it
				int distanceSquared = x * x + z * z;
				if (distanceSquared > renderDistanceSquared)
				{
					continue; 
				}
				
				Vector2I coord = new Vector2I(centerChunk.X + x, centerChunk.Y + z);
				
				chunksInRange.Add(coord);
				
				// checks if the chunk at coord is generated, and if not then generates it
				if (!loadedChunks.ContainsKey(coord) && !queuedCoords.Contains(coord))
				{
					queuedCoords.Add(coord); // A HashSet of all the chunks that need to be generated. 
					// This HashSet is being used instead of checking spawnQueue as lookups in a Queue are expensive
					spawnQueue.Enqueue(coord); // An ordered queue of the chunks that need to be spawned
				}
			}
		}
		
		// creates a list of all chunks that are no longer in range 
		List<Vector2I> toRemove = new List<Vector2I>();
		foreach (var (coord, chunk) in loadedChunks)
		{
			if (!chunksInRange.Contains(coord))
			{
				toRemove.Add(coord);
			}
		}
		// removes all chunks that are no longer in range
		foreach (Vector2I coord in toRemove)
		{
			DespawnChunk(coord);
		}
	}
	
	
	private void ProcessSpawnQueue()
	{
		int spawnedThisFrame = 0;
		while (spawnedThisFrame < chunksPerFrame && spawnQueue.Count > 0)
		{
			Vector2I coord = spawnQueue.Dequeue();
			queuedCoords.Remove(coord);
			
			if (!loadedChunks.ContainsKey(coord))
			{
				SpawnChunk(coord);
				spawnedThisFrame++;
			}
		}
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
		chunk.terrainMaterial = terrainMaterial;
		chunk.oceanTop = oceanTop;
		chunk.beachTop = beachTop;
		chunk.grassTop = grassTop;
		chunk.rockTop = rockTop;
		
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
