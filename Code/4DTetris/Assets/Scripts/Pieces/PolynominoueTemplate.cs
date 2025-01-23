using UnityEngine;

[CreateAssetMenu(fileName = "Polynomino4DTemplate", menuName = "4D/Polynomino Template")]
public class Polynomino4DTemplate : ScriptableObject
{
    // Each entry is the local 4D offset of a hypercube relative to the polynomino's origin
    public Vector4[] blockOffsets;
}
