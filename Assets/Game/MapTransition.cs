using System;
using Unity.Cinemachine;
using UnityEngine;

public class MapTransition : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundary;
    CinemachineConfiner2D confiner;

    private void Awake()
    {
        confiner = FindAnyObjectByType<CinemachineConfiner2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Contact");
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Contact Player");
            confiner.BoundingShape2D = mapBoundary;
        }
    }
}
