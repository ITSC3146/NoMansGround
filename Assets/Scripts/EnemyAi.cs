using System;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyAi : MonoBehaviour
{
    public float MaxSpeed = 5;
    public float protectivradius = .2f;
    public float rotationSpeed = 5f;

    private UnityEngine.Vector3 CurrentPosition;

    public NewGridSystem gridSystem; 
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 go = ConverPosition();
        float targetspeed = MaxSpeed;
        Vector3 Velocity = go.normalized * targetspeed;

        Quaternion targetRotation = Quaternion.LookRotation(Velocity.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.position += Velocity * Time.deltaTime;
    }

    private Vector3 ConverPosition()
    {
        CurrentPosition = transform.position;
        
        //Take Each Position
        float xpos = CurrentPosition.x;
        float zpos = CurrentPosition.z;
        
        //Cast it to an int so we get the initial number (Ie: 5.77 -> 5)
        int ixpos = (int)xpos;
        int izpos = (int)zpos; 

        //Get the Vector2Int direction from the flowfield by using a new Vector2Int based on our pos
        Vector2Int flowdir = gridSystem.flowField.GetDirection(new Vector2Int(ixpos, izpos));
        Vector3 direction = new Vector3(flowdir.x , 0, flowdir.y);

        return direction;
    }
}
