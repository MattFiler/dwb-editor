using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    bool doorOpen = false;
    float currentOpenTime = 0.0f;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Pawn")) return;
        OpenDoor();
    }

    void OpenDoor()
    {
        currentOpenTime = 0.0f;
        if (doorOpen) return;
        doorOpen = true;
        RotateDoor(GetComponentInParent<LevelPropEntity>());
        GetComponent<Collider2D>().enabled = false;
    }

    void CloseDoor()
    {
        if (!doorOpen) return;
        doorOpen = false;
        RotateDoor(GetComponentInParent<LevelPropEntity>());
        GetComponent<Collider2D>().enabled = true;
    }

    void Update()
    {
        if(doorOpen)
        {
            currentOpenTime += Time.deltaTime;
            if(currentOpenTime >= BespokePropManager.Instance.OpenTime) CloseDoor();
        }
    }

    private void RotateDoor(LevelPropEntity entity)
    {
        switch (entity.Rotation)
        {
            case PropRotation.FACING_FRONT:
                entity.SetRotation(PropRotation.FACING_LEFT);
                break;
            case PropRotation.FACING_LEFT:
                entity.SetRotation(PropRotation.FACING_FRONT);
                break;
            case PropRotation.FACING_BACK:
                entity.SetRotation(PropRotation.FACING_RIGHT);
                break;
            case PropRotation.FACING_RIGHT:
                entity.SetRotation(PropRotation.FACING_BACK);
                break;
        }
    }
}
