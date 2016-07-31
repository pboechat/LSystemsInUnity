using UnityEngine;
using System.Collections.Generic;

public class LSystemExecutor : MonoBehaviour
{
    [SerializeField]
    private TextAsset file;
    [SerializeField]
    private int segmentAxialSamples = 3;
    [SerializeField]
    private int segmentRadialSamples = 3;
    [SerializeField]
    private float segmentWidth;
    [SerializeField]
    private float segmentHeight;
    [SerializeField]
    private float leafSize;
    [SerializeField]
    private int leafAxialDensity = 1;
    [SerializeField]
    private int leafRadialDensity = 1;
    [SerializeField]
    private bool useFoliage;
    [SerializeField]
    private bool narrowBranches = true;
    [SerializeField]
    private Material trunkMaterial;
    [SerializeField]
    private Material leafMaterial;

    void Start()
    {
        string axiom;
        float angle;
        int derivations;
        Dictionary<string, List<Production>> productions;
        LSystemParser.Parse(
            file.text,
            out axiom,
            out angle,
            out derivations,
            out productions);

        string moduleString;
        LSystemDeriver.Derive(
            axiom,
            angle,
            derivations,
            productions,
            out moduleString);

        GameObject leaves, trunk;
        LSystemInterpreter.Interpret(
            segmentAxialSamples,
            segmentRadialSamples,
            segmentWidth,
            segmentHeight,
            leafSize,
            leafAxialDensity,
            leafRadialDensity,
            useFoliage,
            narrowBranches,
            leafMaterial,
            trunkMaterial,
            angle,
            moduleString,
            out leaves,
            out trunk);

        leaves.transform.parent = transform;
        leaves.transform.localPosition = Vector3.zero;
        trunk.transform.parent = transform;
        trunk.transform.localPosition = Vector3.zero;

        UpdateColliderBounds(trunk);
    }

    void UpdateColliderBounds(GameObject trunk)
    {
        // Calculate AABB
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < trunk.transform.childCount; i++)
        {
            Transform chunk = trunk.transform.GetChild(i);
            min.x = Mathf.Min(min.x, chunk.GetComponent<Renderer>().bounds.min.x);
            min.y = Mathf.Min(min.y, chunk.GetComponent<Renderer>().bounds.min.y);
            min.z = Mathf.Min(min.z, chunk.GetComponent<Renderer>().bounds.min.z);
            max.x = Mathf.Max(max.x, chunk.GetComponent<Renderer>().bounds.max.x);
            max.y = Mathf.Max(max.y, chunk.GetComponent<Renderer>().bounds.max.y);
            max.z = Mathf.Max(max.z, chunk.GetComponent<Renderer>().bounds.max.z);
        }

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);

        BoxCollider collider = gameObject.GetComponent<BoxCollider>();
        if (collider == null)
            return;
        collider.center = bounds.center - transform.position;
        collider.size = 2 * bounds.extents;
    }

}
