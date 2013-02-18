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
	[SerializeField]
	private string _moduleString;
	[SerializeField]
	private float _segmentWidth;
	[SerializeField]
	private float _segmentHeight;
	private Mesh _mesh;
	
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
		
		public void RotateLeft (float angle)
		{
			direction *= Quaternion.Euler (0, 0, angle);
		}
		
		public void RotateRight (float angle)
		{
			direction *= Quaternion.Euler (0, 0, -angle);
		}
	}
	
	void Start ()
	{
		LoadFromFile ();
		
		MeshRenderer renderer = gameObject.AddComponent<MeshRenderer> ();
		renderer.material = new Material (Shader.Find ("Diffuse"));
		MeshFilter filter = gameObject.AddComponent<MeshFilter> ();
		_mesh = new Mesh ();
		filter.mesh = _mesh;
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
	
	void CreateSegment (ref List<Vector3> vertices, ref List<int> indices, ref List<Vector2> uvs, Turtle turtle)
	{
		float halfWidth = Mathf.Max (0.1f, _segmentWidth) * 0.5f;
		float halfHeight = Mathf.Max (0.1f, _segmentHeight) * 0.5f;
		
		Vector3 position = turtle.position;
		
		int c = vertices.Count;
		
		vertices.Add (position);
		vertices.Add (position + (turtle.direction * new Vector3 (halfWidth, halfHeight, 0)));
		vertices.Add (turtle.position + (turtle.direction * new Vector3 (halfWidth, 0, 0)));
		vertices.Add (turtle.position + (turtle.direction * new Vector3 (0, halfHeight, 0)));
		
		uvs.Add (Vector2.zero);
		uvs.Add (new Vector2 (1, 1));
		uvs.Add (Vector2.right);
		uvs.Add (Vector2.up);
		
		indices.Add (c);
		indices.Add (c + 1);
		indices.Add (c + 2);
		indices.Add (c);
		indices.Add (c + 3);
		indices.Add (c + 1);
	}
	
	void Interpret ()
	{
		List<Vector3> vertices = new List<Vector3> ();
		List<int> indices = new List<int> ();
		List<Vector2> uvs = new List<Vector2> ();
		Turtle current = new Turtle (Quaternion.identity, Vector3.zero, new Vector3 (0, _segmentHeight, 0));
		Queue<Turtle> stack = new Queue<Turtle> ();
		for (int i = 0; i < _moduleString.Length; i++) {
			string module = _moduleString [i] + "";
			
			if (module == "F") {
				current.Forward ();
				CreateSegment (ref vertices, ref indices, ref uvs, current);
			} else if (module == "+") {
				current.RotateLeft (_angle);
			} else if (module == "-") {
				current.RotateRight (_angle);
			} else if (module == "[") {
				stack.Enqueue (current);
				current = new Turtle (current);
			} else if (module == "]") {
				current = stack.Dequeue ();
			}
		}
		
		if (vertices [vertices.Count - 1] != current.position) {
			CreateSegment (ref vertices, ref indices, ref uvs, current);
		}
		
		_mesh.vertices = vertices.ToArray ();
		_mesh.uv = uvs.ToArray ();
		_mesh.triangles = indices.ToArray ();
		_mesh.normals = Arrays.New<Vector3> (Vector3.forward * -1, vertices.Count);
		_mesh.RecalculateBounds ();
	}
	
	void Update ()
	{
		Derive ();
		Interpret ();
	}
}
