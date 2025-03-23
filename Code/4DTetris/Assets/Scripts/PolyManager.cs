using UnityEngine;

public class PolyManager : MonoBehaviour
{
    private GameObject currentPolynomino;
    [Header("References")]
    public GameObject board;
    public GameObject Polynomninoe_prefab;


    public float cubeSize = 1.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        board = GameObject.Find("Board");
        SpawnNewPolynomino();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnNewPolynomino()
    {
        currentPolynomino = Instantiate(Polynomninoe_prefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        board.GetComponent<BoardState>().current_polynomino = currentPolynomino.GetComponent<Polynomino4D>();
    }
    public Polynomino4D getCurrentPoly()
    {
        return currentPolynomino.GetComponent<Polynomino4D>();
    }
}
