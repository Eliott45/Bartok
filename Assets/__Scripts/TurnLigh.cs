using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnLigh : MonoBehaviour
{
    void Update()
    {
        transform.position = Vector3.back * 3;
        if(Bartok.CURRENT_PLAYER == null)
        {
            return;
        }

        transform.position += Bartok.CURRENT_PLAYER.handSlotDef.pos;
    }
}
