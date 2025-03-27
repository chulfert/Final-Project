using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

[Serializable]
public class KeyIndicator
{
    public string key;
    public GameObject upIndicator;
    public GameObject downIndicator;
}

public class InputManager : MonoBehaviour
{
    [Header("References")]
    private Polynomino4D polynomino; 
    public TextMeshProUGUI planeIndicator;

    public Button leftArrowButton;
    public Button rightArrowButton;
    public Button upArrowButton;
    public Button downArrowButton;

    private TextMeshProUGUI leftArrow;
    private TextMeshProUGUI rightArrow;
    private TextMeshProUGUI upArrow;
    private TextMeshProUGUI downArrow;


    [Header("Key Indicators")]
    public List<KeyIndicator> keyIndicators = new List<KeyIndicator>();

    bool lineCleared = false;
    float lineClearCounter = 0.0f;

    [Header("Movement Settings")]
    public float moveStep = 0.1f; // how much to move per frame in x or y

    // If you want to store a position in 4D or 3D,
    // you might keep it here. For demo, we'll just move
    // your polynomino in X/Y of Unity's 3D space.

    bool[] keyWasDown = new bool[4] ;
    
    private float lastFall = 0;

    GameObject board;

    private void Start()
    {
        board = GameObject.Find("Board");

        leftArrow = leftArrowButton.GetComponentInChildren<TextMeshProUGUI>();
        rightArrow = rightArrowButton.GetComponentInChildren<TextMeshProUGUI>();
        upArrow = upArrowButton.GetComponentInChildren<TextMeshProUGUI>();
        downArrow = downArrowButton.GetComponentInChildren<TextMeshProUGUI>();

        planeIndicator.gameObject.SetActive(false);

    }
    void Update()
    {

        polynomino = GetComponent<PolyManager>().getCurrentPoly();
        if (polynomino == null) return;


        if(lineCleared)
        {
            lineClearCounter += Time.deltaTime;
            if (lineClearCounter >= 1.0f)
            {
                lineCleared = false;
                lineClearCounter = 0.0f;
            }
        }
        // -------------------------------
        // 1) Movement with W/A/S/D alone
        //    (no Shift/CTRL/Alt pressed)
        // -------------------------------
        bool mod_1 = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftShift);
        bool mod_2 = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.S);
        bool mod_3 = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.D);
#if DEBUG
        if (Input.GetKey(KeyCode.Escape)){
            // Quit the game
            Application.Quit();
        }

        if (Input.GetKey(KeyCode.Space))
        {
            // Restart the game
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        }

        if (Input.GetKey(KeyCode.C))
        {
            BoardState b = board.GetComponent<BoardState>();
            b.ClearLayer(9);
        }
#endif

        if (Input.GetKey(KeyCode.A))
        {
            bool go = GameObject.Find("GameManager").GetComponent<GameStateManager>().gameOver;
            if (go && !lineCleared)
            {
                GameObject.Find("GameManager").GetComponent<GameStateManager>().RestartGame();
                return;
            }
        }

        // Only do WASD movement if NO modifiers are held only have 1 impulse per key, not held
        if (!mod_1 && !mod_2 && !mod_3)
        {
            planeIndicator.text = "Movement";

            for(int i = 0; i < 3; i++) // Not so smart, dont want to reset the arrow key, think of something else
            {
                KeyIndicator ki = keyIndicators[i];
                ki.upIndicator.SetActive(true);
                ki.downIndicator.SetActive(false);
            }

            leftArrow.text = "Left";
            rightArrow.text = "Right";
            upArrow.text = "Up";
            downArrow.text = "Down";

            ChangeKeyText("left", "Left");
            ChangeKeyText("right", "Right");
            ChangeKeyText("up", "Up");
            ChangeKeyText("down", "Down");

            Vector3 currentPos = polynomino.transform.position;
            // W => +y
            if (Input.GetKey(KeyCode.UpArrow) && !keyWasDown[0])
            {
                polynomino.addMovement(Polynomino4D.MovementAxis.Y, true);
                keyWasDown[0] = true;
            }
            // A => -x
            if (Input.GetKey(KeyCode.LeftArrow) && !keyWasDown[1])
            {
                polynomino.addMovement(Polynomino4D.MovementAxis.X, false);
                keyWasDown[1] = true;
            }
            // S => -y
            if (Input.GetKey(KeyCode.DownArrow) && !keyWasDown[2])
            {
                polynomino.addMovement(Polynomino4D.MovementAxis.Y, false);
                keyWasDown[2] = true;
            }
            // D => +x
            if (Input.GetKey(KeyCode.RightArrow) && !keyWasDown[3])
            {
                polynomino.addMovement(Polynomino4D.MovementAxis.X, true);
                keyWasDown[3] = true;
            }
        }
        else if (mod_1 && !mod_2 && !mod_3)
        {
            planeIndicator.text = "Rotation XY (L/R) || Rotation XZ (U/D)";
            leftArrow.text = "XY-";
            rightArrow.text = "XY+";
            upArrow.text = "XZ+";
            downArrow.text = "XZ-";

            ChangeKeyText("left", "XY-");
            ChangeKeyText("right", "XY+");
            ChangeKeyText("up", "XZ+");
            ChangeKeyText("down", "XZ-");

            PressKeyViz("a");

            // Shift + A/d rotates around XY plane
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                polynomino.addRotation(Polynomino4D.RotationAxis.XY, false);
            if (Input.GetKeyDown(KeyCode.RightArrow))
                polynomino.addRotation(Polynomino4D.RotationAxis.XY, true);

            // Shift + W/S rotates around XZ plane
            if (Input.GetKeyDown(KeyCode.UpArrow))
                polynomino.addRotation(Polynomino4D.RotationAxis.XZ, true);
            if (Input.GetKeyDown(KeyCode.DownArrow))
                polynomino.addRotation(Polynomino4D.RotationAxis.XZ, false);
        }
        else if (!mod_1 && mod_2 && !mod_3)
        {
            planeIndicator.text = "Rotation XW (L/R) || Rotation YZ (U/D)";
            leftArrow.text = "XW-";
            rightArrow.text = "XW+";
            upArrow.text = "YZ+";
            downArrow.text = "YZ-";

            ChangeKeyText("left", "XW-");
            ChangeKeyText("right", "XW+");
            ChangeKeyText("up", "YZ+");
            ChangeKeyText("down", "YZ-");


            PressKeyViz("s");

            // ctrl + A/d rotates around Xw plane
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                polynomino.addRotation(Polynomino4D.RotationAxis.XW, false);
            if (Input.GetKeyDown(KeyCode.RightArrow))
                polynomino.addRotation(Polynomino4D.RotationAxis.XW, true);
            // ctrl + W/S rotates around YZ plane
            if (Input.GetKeyDown(KeyCode.UpArrow))
                polynomino.addRotation(Polynomino4D.RotationAxis.YZ, true);
            if (Input.GetKeyDown(KeyCode.DownArrow))
                polynomino.addRotation(Polynomino4D.RotationAxis.YZ, false);
        }
        else if (!mod_1 && !mod_2 && mod_3)
        {
            planeIndicator.text = "Rotation YW (L/R) || Rotation ZW (U/D)";
            leftArrow.text = "YW-";
            rightArrow.text = "YW+";
            upArrow.text = "ZW+";
            downArrow.text = "ZW-";

            ChangeKeyText("left", "YW-");
            ChangeKeyText("right", "YW+");
            ChangeKeyText("up", "ZW+");
            ChangeKeyText("down", "ZW-");


            PressKeyViz("d");

            // alt + A/d rotates around Yw plane
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                polynomino.addRotation(Polynomino4D.RotationAxis.YW, false);
            if (Input.GetKeyDown(KeyCode.RightArrow))
                polynomino.addRotation(Polynomino4D.RotationAxis.YW, true);
            // alt + W/S rotates around ZW plane
            if (Input.GetKeyDown(KeyCode.UpArrow))
                polynomino.addRotation(Polynomino4D.RotationAxis.ZW, true);
            if (Input.GetKeyDown(KeyCode.DownArrow))
                polynomino.addRotation(Polynomino4D.RotationAxis.ZW, false);
        }
        else
        {
            // If multiple or none are pressed, we do not define an axis
            // (You can remap these however you want.)
            planeIndicator.text = "To many modifiers pressed";
        }
        
        // check if the key is released
        if (!Input.GetKey(KeyCode.UpArrow) && keyWasDown[0])
        {
            keyWasDown[0] = false;
        }
        if (!Input.GetKey(KeyCode.LeftArrow) && keyWasDown[1])
        {
            keyWasDown[1] = false;
        }
        if (!Input.GetKey(KeyCode.DownArrow) && keyWasDown[2])
        {
            keyWasDown[2] = false;
        }
        if (!Input.GetKey(KeyCode.RightArrow) && keyWasDown[3])
        {
            keyWasDown[3] = false;
        }

        if(Input.GetKeyDown(KeyCode.LeftArrow))
            PressKeyViz("left");
        if (Input.GetKeyDown(KeyCode.RightArrow))
            PressKeyViz("right");
        if (Input.GetKeyDown(KeyCode.UpArrow))
            PressKeyViz("up");
        if (Input.GetKeyDown(KeyCode.DownArrow))
            PressKeyViz("down");
        if (!Input.GetKey(KeyCode.LeftArrow))
            ReleaseKeyViz("left");
        if (!Input.GetKey(KeyCode.RightArrow))
            ReleaseKeyViz("right");
        if (!Input.GetKey(KeyCode.UpArrow))
            ReleaseKeyViz("up");
        if (!Input.GetKey(KeyCode.DownArrow))
            ReleaseKeyViz("down");

        //every 10 seconds, move the polynomino down probly should not be done here, but it works for now........TODO!!
        if (Time.time - lastFall >= 1)
        {
            polynomino.addMovement(Polynomino4D.MovementAxis.Z, true);
            lastFall = Time.time;
        }
        
    }

    private void PressKeyViz(string key)
    {
        for (int i = 0; i < keyIndicators.Count; i++)
        {
            KeyIndicator ki = keyIndicators[i];
            if (ki.key == key)
            {
                ki.upIndicator.SetActive(false);
                ki.downIndicator.SetActive(true);
            }
        }
    }

    private void ReleaseKeyViz(string key)
    {
        for (int i = 0; i < keyIndicators.Count; i++)
        {
            KeyIndicator ki = keyIndicators[i];
            if (ki.key == key)
            {
                ki.upIndicator.SetActive(true);
                ki.downIndicator.SetActive(false);
            }
        }
    }

    private void PressKeyVizText(string key, string text)
    {
        for (int i = 0; i < keyIndicators.Count; i++)
        {
            KeyIndicator ki = keyIndicators[i];
            if (ki.key == key)
            {
                ki.upIndicator.SetActive(false);
                ki.downIndicator.SetActive(true);
                ki.downIndicator.GetComponentInChildren<TextMeshProUGUI>().text = text;
            }
        }
    }

    private void ReleaseKeyVizeText(string key, string text)
    {
        for (int i = 0; i < keyIndicators.Count; i++)
        {
            KeyIndicator ki = keyIndicators[i];
            if (ki.key == key)
            {
                ki.upIndicator.SetActive(true);
                ki.downIndicator.SetActive(false);
                ki.upIndicator.GetComponentInChildren<TextMeshProUGUI>().text = text;
            }
        }
    } 
    
    private void ChangeKeyText(string key, string text)
    {
        for (int i = 0; i < keyIndicators.Count; i++)
        {
            KeyIndicator ki = keyIndicators[i];
            if (ki.key == key)
            {
                ki.upIndicator.GetComponentInChildren<TextMeshProUGUI>().text = text;
            }
        }
    }

}
