using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollisionCounter : MonoBehaviour
{
    // Start is called before the first frame update

    public UnityEngine.UI.Text _CollisionCounter;
    private int collisionCount;

    void Start()
    {
        collisionCount = 0;
    }


    void OnCollisionEnter(Collision collision)
    {
        collisionCount++;

        //_CollisionCounter.text = "Collisions: " + collisionCount;
        _CollisionCounter.text = "Collisions: " + collisionCount;
    }
}