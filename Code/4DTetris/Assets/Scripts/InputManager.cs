using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    public Polynomino4D polynomino; // Assign via Inspector

    [Header("Movement Settings")]
    public float moveStep = 0.1f; // how much to move per frame in x or y

    // If you want to store a position in 4D or 3D,
    // you might keep it here. For demo, we'll just move
    // your polynomino in X/Y of Unity's 3D space.

    bool[] keyWasDown = new bool[4] ;
    

    void Update()
    {
        if (polynomino == null) return;

        // -------------------------------
        // 1) Movement with W/A/S/D alone
        //    (no Shift/CTRL/Alt pressed)
        // -------------------------------
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        

        // Only do WASD movement if NO modifiers are held only have 1 impulse per key, not held
        if (!shift && !control && !alt)
        {
            Vector3 currentPos = polynomino.transform.position;
            // W => +y
            if (Input.GetKey(KeyCode.W) && !keyWasDown[0])
            {
                polynomino.addMovement(Polynomino4D.MovementAxis.Y, true);
                keyWasDown[0] = true;
            }
            // A => -x
            if (Input.GetKey(KeyCode.A) && !keyWasDown[1])
            {
                polynomino.addMovement(Polynomino4D.MovementAxis.X, false);
                keyWasDown[1] = true;
            }
            // S => -y
            if (Input.GetKey(KeyCode.S) && !keyWasDown[2])
            {
                polynomino.addMovement(Polynomino4D.MovementAxis.Y, false);
                keyWasDown[2] = true;
            }
            // D => +x
            if (Input.GetKey(KeyCode.D) && !keyWasDown[3])
            {
                polynomino.addMovement(Polynomino4D.MovementAxis.X, true);
                keyWasDown[3] = true;
            }
        }
        else if (shift && !control && !alt)
        {
            // Shift + A/d rotates around XY plane
            if (Input.GetKeyDown(KeyCode.A))
                polynomino.addRotation(Polynomino4D.RotationAxis.XY, false);
            if (Input.GetKeyDown(KeyCode.D))
                polynomino.addRotation(Polynomino4D.RotationAxis.XY, true);

            // Shift + W/S rotates around XZ plane
            if (Input.GetKeyDown(KeyCode.W))
                polynomino.addRotation(Polynomino4D.RotationAxis.XZ, true);
            if (Input.GetKeyDown(KeyCode.S))
                polynomino.addRotation(Polynomino4D.RotationAxis.XZ, false);
        }
        else if (!shift && control && !alt)
        {
            // ctrl + A/d rotates around Xw plane
            if (Input.GetKeyDown(KeyCode.A))
                polynomino.addRotation(Polynomino4D.RotationAxis.XW, false);
            if (Input.GetKeyDown(KeyCode.D))
                polynomino.addRotation(Polynomino4D.RotationAxis.XW, true);
            // ctrl + W/S rotates around YZ plane
            if (Input.GetKeyDown(KeyCode.W))
                polynomino.addRotation(Polynomino4D.RotationAxis.YZ, true);
            if (Input.GetKeyDown(KeyCode.S))
                polynomino.addRotation(Polynomino4D.RotationAxis.YZ, false);
        }
        else if (!shift && !control && alt)
        {
            // alt + A/d rotates around Yw plane
            if (Input.GetKeyDown(KeyCode.A))
                polynomino.addRotation(Polynomino4D.RotationAxis.YW, false);
            if (Input.GetKeyDown(KeyCode.D))
                polynomino.addRotation(Polynomino4D.RotationAxis.YW, true);
            // alt + W/S rotates around ZW plane
            if (Input.GetKeyDown(KeyCode.W))
                polynomino.addRotation(Polynomino4D.RotationAxis.ZW, true);
            if (Input.GetKeyDown(KeyCode.S))
                polynomino.addRotation(Polynomino4D.RotationAxis.ZW, false);
        }
        else
        {
            // If multiple or none are pressed, we do not define an axis
            // (You can remap these however you want.)
        }
        
        // check if the key is released
        if (!Input.GetKey(KeyCode.W) && keyWasDown[0])
        {
            keyWasDown[0] = false;
        }
        if (!Input.GetKey(KeyCode.A) && keyWasDown[1])
        {
            keyWasDown[1] = false;
        }
        if (!Input.GetKey(KeyCode.S) && keyWasDown[2])
        {
            keyWasDown[2] = false;
        }
        if (!Input.GetKey(KeyCode.D) && keyWasDown[3])
        {
            keyWasDown[3] = false;
        }

        
    }

}
