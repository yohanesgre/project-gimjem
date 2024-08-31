using UnityEngine;

public class RopeController : MonoBehaviour
{
    public void MoveInDirection(Vector3 movement)
    {
        // Apply movement to the game object's position
        transform.Translate(movement);
    }
}
