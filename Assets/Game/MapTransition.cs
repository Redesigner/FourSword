using System;
using Unity.Cinemachine;
using UnityEngine;

public class MapTransition : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundary;
    CinemachineConfiner2D confiner;
    [SerializeField] Direction direction;
    [SerializeField] float additivePos = 2f;
    
    enum Direction { Up, Down, Left, Right }

    private void Awake()
    {
        confiner = FindAnyObjectByType<CinemachineConfiner2D>();
    }

    //Trigger for when a player enters a transition waypoint
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            confiner.BoundingShape2D = mapBoundary;
            updatePlayerPosition(collision.gameObject);
        }
    }

    //Puts player a few units forward when going into a room
    private void updatePlayerPosition(GameObject player)
    {
        Vector3 newPos = player.transform.position;

        switch (direction)
        {
            case Direction.Up:
                newPos.y += additivePos;
                break;
            case Direction.Down:
                newPos.y -= additivePos;
                break;
            case Direction.Left:
                newPos.x += additivePos;
                break;
            case Direction.Right:
                newPos.x -= additivePos;
                break;
        }
        
        player.transform.position = newPos;
    }
}
