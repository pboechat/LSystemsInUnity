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
		
		MeshRenderer renderer = gameObject.AddComponent<MeshRenderer> ();
		renderer.material = new Material (Shader.Find ("Diffuse"));
		gameObject.AddComponent<MeshFilter> ();
		
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
	
	void CreateSegment (ref List<Vector3> vertices, ref List<Vector3> normals, ref List<int> indices, ref List<Vector2> uvs, Turtle turtle)
	{
		/*float width = Mathf.Max (0.1f, _segmentWidth);
		float height = Mathf.Max (0.1f, _segmentHeight);
		
		Vector3 topRight = turtle.direction * new Vector3 (width, height, 0);
		Vector3 right = turtle.direction * new Vector3 (width, 0, 0);
		Vector3 up = turtle.direction * new Vector3 (0, height, 0);
		Vector3 bottomLeft = turtle.position - topRight;
		
		int c = vertices.Count;
		
		vertices.Add (bottomLeft);
		vertices.Add (bottomLeft + right);
		vertices.Add (bottomLeft + topRight);
		vertices.Add (bottomLeft + up);
		
		uvs.Add (Vector2.zero);
		uvs.Add (Vector2.right);
		uvs.Add (new Vector2 (1, 1));
		uvs.Add (Vector2.up);
		
		indices.Add (c + 1);
		indices.Add (c);
		indices.Add (c + 3);
		
		indices.Add (c + 1);
		indices.Add (c + 3);
		indices.Add (c + 2);*/
		
		Vector3[] newVertices;
		Vector3[] newNormals;
		Vector2[] newUVs;
		int[] newIndices;
		
		ProceduralMeshes.CreateCylinder (3, 3, _segmentWidth * 0.5f, _segmentHeight, out newVertices, out newNormals, out newUVs, out newIndices);
		
		int indexOffset = vertices.Count;
		Vector3 vertexOffset = turtle.position - (new Vector3 (_segmentWidth, _segmentHeight, 0) * 0.5f);
		
		for (int i = 0; i < newVertices.Length; i++) {
			Vector3 vertex = newVertices [i];
			vertices.Add (vertexOffset + (turtle.direction * vertex));
		}
		
		for (int i = 0; i < newIndices.Length; i++) {
			int index = newIndices [i];
			indices.Add (indexOffset + index);
		}
		
		normals.AddRange (newNormals);
		uvs.AddRange (newUVs);
	}
	
	void Interpret ()
	{
		List<Vector3> vertices = new List<Vector3> ();
		List<int> indices = new List<int> ();
		List<Vector2> uvs = new List<Vector2> ();
		List<Vector3> normals = new List<Vector3> ();
		Turtle current = new Turtle (Quaternion.identity, Vector3.zero, new Vector3 (0, _segmentHeight, 0));
		Stack<Turtle> stack = new Stack<Turtle> ();
		for (int i = 0; i < _moduleString.Length; i++) {
			string module = _moduleString [i] + "";
			
			if (module == "F") {
				current.Forward ();
				CreateSegment (ref vertices, ref normals, ref indices, ref uvs, current);
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
		
		if (vertices [vertices.Count - 1] != current.position) {
			CreateSegment (ref vertices, ref normals, ref indices, ref uvs, current);
		}
		
		Mesh mesh = new Mesh ();
		
		mesh.vertices = vertices.ToArray ();
		mesh.uv = uvs.ToArray ();
		mesh.triangles = indices.ToArray ();
		mesh.normals = normals.ToArray ();
		mesh.RecalculateBounds ();
		
		GetComponent<MeshFilter> ().mesh = mesh;
	}

	void AdjustCameraDistance ()
	{
		float width = renderer.bounds.extents.x * 2;
		float height = renderer.bounds.extents.y * 2;
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
