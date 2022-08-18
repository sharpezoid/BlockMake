using UnityEngine;

public class FollowScript : MonoBehaviour
{
    public GameObject target;
    public float damping = 1;
    public float distance = 10;

    Vector3 playerMoveDir;

    void LateUpdate()
    {
        playerMoveDir = target.transform.forward;
        playerMoveDir.Normalize();
        Vector3 newPos = target.transform.position - playerMoveDir * distance;
        transform.position = newPos;

        transform.LookAt(target.transform.position);
    }
}