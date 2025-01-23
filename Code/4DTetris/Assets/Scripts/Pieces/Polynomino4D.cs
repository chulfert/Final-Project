using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Polynomino4D manages a collection of Hypercubes that form a 4D shape.
/// - Builds itself from a template (array of 4D offsets).
/// - Keeps track of rotation angles in 4D (XY, XZ, XW, YZ, YW, ZW).
/// - Forwards those angles + each block's offset to the Hypercube scripts.
/// - Supports removing hypercubes (e.g. for Tetris line clears).
/// </summary>
public class Polynomino4D : MonoBehaviour
{
    [Header("Template & Prefab")]
    [Tooltip("Which 4D shape do we use? (set of blockOffsets in 4D)")]
    public Polynomino4DTemplate template;

    [Tooltip("Prefab that contains the 'Hypercube' script.")]
    public GameObject hypercubePrefab;

    // We'll store references to each Hypercube we spawn
    private List<Hypercube> hypercubes = new List<Hypercube>();

    [Header("4D Rotation Angles (in degrees)")]
    public float rotationXY, rotationXZ, rotationXW, rotationYZ, rotationYW, rotationZW;

    void Start()
    {
        BuildFromTemplate();
    }

    void Update()
    {
        // If you allow continuous rotation, or your Tetris game
        // updates angles in real-time, you'd do it here.
        // Then we must forward the angles to each Hypercube:

        foreach (var cube in hypercubes)
        {
            if (cube != null)
            {
                cube.SetRotation4D(
                    rotationXY, rotationXZ, rotationXW,
                    rotationYZ, rotationYW, rotationZW
                );
            }
        }
    }

    /// <summary>
    /// Instantiates one Hypercube for each 4D offset in the template.
    /// Then store them in our 'hypercubes' list.
    /// </summary>
    private void BuildFromTemplate()
    {
        if (template == null || hypercubePrefab == null)
        {
            Debug.LogWarning("Polynomino4D: Missing template or prefab!");
            return;
        }

        ClearAllHypercubes(); // if we had any old ones

        for (int i = 0; i < template.blockOffsets.Length; i++)
        {
            Vector4 offset = template.blockOffsets[i];

            GameObject go = Instantiate(hypercubePrefab, this.transform);
            Hypercube hc = go.GetComponent<Hypercube>();
            if (hc == null)
            {
                Debug.LogError("Prefab missing Hypercube component!", go);
                Destroy(go);
                continue;
            }

            // Set the local offset so the hypercube knows where it is in 4D
            hc.localOffset4D = offset;

            // Also set the current rotation angles, so it starts correctly
            hc.SetRotation4D(
                rotationXY, rotationXZ, rotationXW,
                rotationYZ, rotationYW, rotationZW
            );

            hypercubes.Add(hc);
        }
    }

    /// <summary>
    /// Removes a single hypercube from this polynomino (e.g., Tetris line clear).
    /// Destroys the GameObject, and removes it from the 'hypercubes' list.
    /// </summary>
    public void RemoveHypercube(Hypercube cube)
    {
        if (cube != null)
        {
            hypercubes.Remove(cube);
            Destroy(cube.gameObject);
        }
    }

    /// <summary>
    /// Remove ALL hypercubes (destroy them). Called if the entire piece is removed.
    /// </summary>
    public void ClearAllHypercubes()
    {
        foreach (var hc in hypercubes)
        {
            if (hc != null) Destroy(hc.gameObject);
        }
        hypercubes.Clear();
    }

    /// <summary>
    /// If you want to add an extra hypercube at runtime (rare in Tetris, but possible).
    /// localOffset is in 4D, referencing the polynomino's origin.
    /// </summary>
    public Hypercube AddHypercube(Vector4 localOffset)
    {
        if (hypercubePrefab == null) return null;

        GameObject go = Instantiate(hypercubePrefab, this.transform);
        Hypercube hc = go.GetComponent<Hypercube>();
        hc.localOffset4D = localOffset;
        hc.SetRotation4D(
            rotationXY, rotationXZ, rotationXW,
            rotationYZ, rotationYW, rotationZW
        );

        hypercubes.Add(hc);
        return hc;
    }
}
