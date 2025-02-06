using UnityEngine;
using System.Collections;
public class CubeRep : MonoBehaviour
{
    
    public Vector3 position;
    public float size;

    [Header("References")]
    public Polynomino4D polynomino;

    public Rigidbody rb;

    public CubeRep(Vector3 position)
    {
        this.position = position;
        this.size = polynomino.cubeSize;
    }

    public Mesh mesh;
    public Material material;    

    public bool render = false;
    public void Start()
    {
        // create a cube
        // attach a rigidbody to the cube
        rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.mass = 100f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        // Add a simple cube mesh
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = new Vector3[]
        {
            new Vector3(-size, -size, -size),
            new Vector3(size, -size, -size),
            new Vector3(size, size, -size),
            new Vector3(-size, size, -size),
            new Vector3(-size, -size, size),
            new Vector3(size, -size, size),
            new Vector3(size, size, size),
            new Vector3(-size, size, size)
        };
    }

    public void Update()
    {
        //Check if colliding with another cube
        if(!render)
        {
            //disable rendering of the mesh
            GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            //enable rendering of the mesh
            GetComponent<MeshRenderer>().enabled = true;
        }
        
        // Update the position of the rigidbody
        rb.MovePosition(position);

    }

    //inform the collision partner we are a cube
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<CubeRep>() != null)
        {
            Debug.Log("Colliding with another cube");
        }
    }

    public void ToggleRender()
    {
        render = !render;
    }
    
}
