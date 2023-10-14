using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using VidTools.Vis;

public class FluidBehaviour : MonoBehaviour
{
    public int particleCount;
    public float gravity;
    public float particleSize;
    public float bounceCoeficient;
    private Vector2 position;
    private Vector2 velocity;
    public Vector2 boundarySize;

    void Start()
    {
        
    }


    void Update()
    {
        velocity += Vector2.down * gravity * Time.deltaTime;
        position += velocity * Time.deltaTime;

        resolveColisions();

        Draw.Point(new Vector3(position.x, position.y, 0), particleSize, Color.white);
        Draw.BoxOutline(Vector2.zero, boundarySize, 1/10, Color.white, 1);
    }

    //Resolves colisions of particles with the boundary
    void resolveColisions () {
        Vector2 halfBoundSize = boundarySize / 2 - Vector2.one * particleSize;
        //resolves colisions on the x axis
        if(Abs(position.x) > halfBoundSize.x) {
            position.x = halfBoundSize.x * Sign(position.x);
            velocity.x *= -1 * bounceCoeficient;
        }
        //resolves colisions on the y axis
        if(Abs(position.y) > halfBoundSize.y) {
            position.y = halfBoundSize.y * Sign(position.y);
            velocity.y *= -1 * bounceCoeficient;
        }
    }

    //returns the absolute value of a given number
    float Abs (float x) {
        return Math.Abs(x);
    }

    //returns the sign of a given number
    float Sign (float x) {
        return Math.Sign(x);
    }
}
