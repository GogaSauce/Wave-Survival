using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviour
{
    Player player;
    public GameObject trail;
    public bool activated;
    [SerializeField] float rotateSpeed;
    float current;
    private void Awake()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        current = rotateSpeed;
    }
    private void Update()
    {
        if (!GetComponent<Rigidbody>().isKinematic)
        {
            transform.Rotate(Vector3.right * Time.deltaTime, rotateSpeed);

        }
        if (activated)
        {
            rotateSpeed = current;
        }
        if (!player.inHand)
        {
            trail.SetActive(true);
        }
        else
        {
            trail.SetActive(false);
            rotateSpeed = 0f;
        }
       
    }
    private void OnCollisionEnter(Collision collision)
    {
        GetComponent<Rigidbody>().isKinematic = true;
        activated = false;
        
    }
}
