using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;

public class OtherMoving : Other
{
    public float speed = 5f;
    public bool[] movementsKey = new bool[4];

    private void Update()
    {
        Moving();
    }
    /// <summary>
    /// EnemyMove
    /// </summary>
    void Moving()
    {
        if (movementsKey[(int)Key.W])
        {
            this.transform.Translate(0, 0, 1f * Time.deltaTime * speed);
        }
        if (movementsKey[(int)Key.S])
        {
            this.transform.Translate(0, 0, -1f * Time.deltaTime * speed);
        }
        if (movementsKey[(int)Key.A])
        {
            this.transform.Translate(-1f * Time.deltaTime * speed, 0, 0);
        }
        if (movementsKey[(int)Key.D])
        {
            this.transform.Translate(1f * Time.deltaTime * speed, 0, 0);
        }
    }

    void Turning() { }
}
