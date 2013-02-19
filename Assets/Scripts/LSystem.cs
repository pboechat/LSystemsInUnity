using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class LSystem : MonoBehaviour
{

	[SerializeField]
	private string _fileName;
	[SerializeField]
	private string _axiom = "";
	[SerializeField]
	private float _angle = 0;
	[SerializeField]
	private int _numberOfDerivations = 0;
	private Dictionary<string, ProductionRule> _productionRules = new Dictionary<string, ProductionRule> ();
	private string _moduleString;
	[SerializeField]
	private float _segmentWidth;
	[SerializeField]
	private float _segmentHeight;
	[SerializeField]
	private bool _update;
	
	private struct Turtle
	{
		public Quaternion direction;
		public Vector3 position;
		public Vector3 step;
		
		public Turtle (Turtle other)
		{
			this.direction = other.direction;
			this.position = other.position;
			this.step = other.step;
		}
		
		public Turtle (Quaternion direction, Vector3 position, Vector3 step)
		{
			this.direction = direction;
			this.position = position;
			this.step = step;
		}
		
		public void Forward ()
		{
			position += direction * step;
		}
		
		public void RotateX (float angle)
		{
			direction *= Quaternion.Euler (angle, 0, 0);
		}
		
		public void RotateY (float angle)
		{
			direction *= Quaternion.Euler (0, angle, 0);
		}
		
		public void RotateZ (float angle)
		{
			direction *= Quaternion.Euler (0, 0, angle);
		}
	}
	
	void Start ()
	{
		LoadFromFile ();
		
		gameObject.AddComponent<BoxCollider> ();
		
		_update = true;
	}
	
	void LoadFromFile ()
	{
		StreamReader streamReader = new StreamReader ("Assets/" + _fileName);
		List<string> lines = new List<string> ();
		{
			string line;
			while ((line = streamReader.ReadLine ()) != null) {
				lines.Add (line);
			}
		}
		streamReader.Close ();

		foreach (string line in lines) {
			if (line.Length == 0) {
				continue;
			} else if (line [0] == '/' && line [1] == '/') {
				continue;
			}
	
			string value;
			
			if (line.IndexOf ("axiom") != -1) {
				value = line.Substring (line.IndexOf ("=") + 1);
				value = value.Trim ();
				_axiom = value;
			} else if (line.IndexOf ("angle") != -1) {
				value = line.Substring (line.IndexOf ("=") + 1);
				value = value.Trim ();
				_angle = float.Parse (value);
			} else if (line.IndexOf ("number of derivations") != -1) {
				value = line.Substring (line.IndexOf ("=") + 1);
				value = value.Trim ();
				_numberOfDerivations = int.Parse (value);
			} else {
				ProductionRule productionRule = ProductionRule.Build (line);
				_productionRules [productionRule.predecessor] = productionRule;
			}
		}
		
		if (!CheckProductionRulesProbabilities ()) {
			throw new Exception ("There's one of more production rules with probability < 1");
		}
	}
	
	bool CheckProductionRulesProbabilities ()
	{
		Dictionary<string, float> predecessorsProbability = new Dictionary<string, float> ();

		foreach (ProductionRule productionRule in _productionRules.Values) {
			if (predecessorsProbability.ContainsKey (productionRule.predecessor)) {
				predecessorsProbability [productionRule.predecessor] += productionRule.probability;
			} else {
				predecessorsProbability [productionRule.predecessor] = productionRule.probability;
			}
		}
	
		foreach (float probability in predecessorsProbability.Values) {
			if (probability != 1) {
				return false;
			}
		}
	
		return true;
	}
	
	void Derive ()
	{
		_moduleString = _axiom;
		for (int i = 0; i < Math.Max(1, _numberOfDerivations); i++) {
			string newModuleString = "";
			for (int j = 0; j < _moduleString.Length; j++) {
				string module = _moduleString [j] + "";
				
				if (!_productionRules.ContainsKey (module)) {
					newModuleString += module;
					continue;
				}
				
				ProductionRule productionRule = _productionRules [module];
				newModuleString += productionRule.successor;
			}
			_moduleString = newModuleString;
		}
			
	}
	
	void CreateNewChunk (Mesh mesh)
	{
		GameObject chunk = new GameObject ("chunk_" + (transform.childCount + 1));
		chunk.transform.parent = transform;
		chunk.transform.localPosition = Vector3.zero;
		chunk.AddComponent<MeshRenderer> ().material = new Material (Shader.Find ("Diffuse"));
		chunk.AddComponent<MeshFilter> ().mesh = mesh;
	}
	
	void CreateSegment (Turtle turtle, ref Mesh currentMesh)
	{
		Vector3[] newVertices;
		Vector3[] newNormals;
		Vector2[] newUVs;
		int[] newIndices;
		
		ProceduralMeshes.CreateCylinder (3, 3, _segmentWidth * 0.5f, _segmentHeight, out newVertices, out newNormals, out newUVs, out newIndices);
		
		if (currentMesh.vertices.Length + newVertices.Length > 65000) {
			CreateNewChunk (currentMesh);
			currentMesh = new Mesh ();
		}
		
		int numVertices = currentMesh.vertices.Length + newVertices.Length;
		int numTriangles = currentMesh.triangles.Length + newIndices.Length;
		
		Vector3[] vertices = new Vector3[numVertices];
		Vector3[] normals = new Vector3[numVertices];
		int[] indices = new int[numTriangles];
		Vector2[] uvs = new Vector2[numVertices];
		
		Array.Copy (currentMesh.vertices, 0, vertices, 0, currentMesh.vertices.Length);
		Array.Copy (currentMesh.normals, 0, normals, 0, currentMesh.normals.Length);
		Array.Copy (currentMesh.triangles, 0, indices, 0, currentMesh.triangles.Length);
		Array.Copy (currentMesh.uv, 0, uvs, 0, currentMesh.uv.Length);
		
		Vector3 vertexOffset = turtle.position - (turtle.direction * (new Vector3 (_segmentWidth, _segmentHeight, 0) * 0.5f));
		
		int offset = currentMesh.vertices.Length;
		for (int i = 0; i < newVertices.Length; i++) {
			Vector3 vertex = newVertices [i];
			vertices [offset + i] = vertexOffset + (turtle.direction * vertex);
		}
		
		int trianglesOffset = currentMesh.vertices.Length;
		offset = currentMesh.triangles.Length;
		for (int i = 0; i < newIndices.Length; i++) {
			int index = newIndices [i];
			indices [offset + i] = (trianglesOffset + index);
		}
		
		Array.Copy (newNormals, 0, normals, currentMesh.normals.Length, newNormals.Length);
		Array.Copy (newUVs, 0, uvs, currentMesh.uv.Length, newUVs.Length);
		
		currentMesh.vertices = vertices;
		currentMesh.normals = normals;
		currentMesh.triangles = indices;
		currentMesh.uv = uvs;
		
		currentMesh.Optimize ();
	}

	void DestroyChunks ()
	{
		for (int i = 0; i < transform.childCount; i++) {
			Destroy (transform.GetChild (i).gameObject);
		}
		
		transform.DetachChildren ();
	}
	
	void Interpret ()
	{
		DestroyChunks ();
		
		Mesh currentMesh = new Mesh ();
		
		Turtle current = new Turtle (Quaternion.identity, Vector3.zero, new Vector3 (0, _segmentHeight, 0));
		Stack<Turtle> stack = new Stack<Turtle> ();
		for (int i = 0; i < _moduleString.Length; i++) {
			string module = _moduleString [i] + "";
			
			if (module == "F") {
				current.Forward ();
				CreateSegment (current, ref currentMesh);
			} else if (module == "+") {
				current.RotateZ (_angle);
			} else if (module == "-") {
				current.RotateZ (-_angle);
			} else if (module == "&") {
				current.RotateX (_angle);
			} else if (module == "^") {
				current.RotateX (-_angle);
			} else if (module == "\\") {
				current.RotateY (_angle);
			} else if (module == "/") {
				current.RotateY (-_angle);
			} else if (module == "|") {
				current.RotateZ (180);
			} else if (module == "[") {
				stack.Push (current);
				current = new Turtle (current);
			} else if (module == "]") {
				current = stack.Pop ();
			}
		}
		
		CreateNewChunk (currentMesh);
		
		UpdateColliderBounds ();
	}
	
	void UpdateColliderBounds ()
	{
		// Calculate AABB
		Vector3 min = new Vector3 (float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 max = new Vector3 (float.MinValue, float.MinValue, float.MinValue);
		for (int i = 0; i < transform.childCount; i++) {
			Transform chunk = transform.GetChild (i);
			min.x = Mathf.Min (min.x, chunk.renderer.bounds.min.x);
			min.y = Mathf.Min (min.y, chunk.renderer.bounds.min.y);
			min.z = Mathf.Min (min.z, chunk.renderer.bounds.min.z);
			max.x = Mathf.Max (max.x, chunk.renderer.bounds.max.x);
			max.y = Mathf.Max (max.y, chunk.renderer.bounds.max.y);
			max.z = Mathf.Max (max.z, chunk.renderer.bounds.max.z);
		}
		
		Bounds bounds = new Bounds ();
		bounds.SetMinMax (min, max);
		
		BoxCollider collider = gameObject.GetComponent<BoxCollider> ();
		collider.center = bounds.center - transform.position;
		collider.extents = bounds.extents;
	}

	void AdjustCameraDistance ()
	{
		float width = collider.bounds.extents.x * 2;
		float height = collider.bounds.extents.y * 2;
		float size = Mathf.Sqrt (Mathf.Pow (width, 2) + Mathf.Pow (height, 2) + 1);
		float minimumCameraDistance = (size / 2.0f) / Mathf.Tan ((Mathf.Deg2Rad * Camera.mainCamera.fov) / 2.0f);
		minimumCameraDistance = Mathf.Min (minimumCameraDistance, Camera.mainCamera.far);
		Camera.mainCamera.transform.position = new Vector3 (0, 0, -minimumCameraDistance);
	}
	
	void Update ()
	{
		if (_update) {
			Derive ();
			Interpret ();
			AdjustCameraDistance ();
			_update = false;
		}
	}
}