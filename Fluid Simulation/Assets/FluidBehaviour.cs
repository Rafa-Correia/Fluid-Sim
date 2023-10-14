using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using VidTools.Vis;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class FluidBehaviour : MonoBehaviour
{
    public int numParticles;
    public float gravity;
    public float mass;
    public float particleSize;
    public float particleSpacing;
    public float bounceCoeficient;
    public float smoothingRadius;
    private Vector2[] positions;
    private Vector2[] velocities;
    public Vector2 boundarySize;
    public float targetDensity;
    public float pressureMultiplier;
    private float[] densities;
    private Vector2[] predictedPositions;

    void Start()
    {
        positions = new Vector2[numParticles];
        velocities = new Vector2[numParticles];
        densities = new float[numParticles];
        predictedPositions = new Vector2[numParticles];


        int particlesPerRow = (int)Math.Sqrt(numParticles);
        int particlesPerCol = (numParticles-1)/(particlesPerRow+1);
        float spacing = particleSize*2 + particleSpacing;

        for(int i = 0; i < numParticles; i++) {
            positions[i].x=(i%particlesPerRow-particlesPerRow/2f-0.5f) * spacing;
            positions[i].y=(i/particlesPerRow-particlesPerCol/2f-0.5f) * spacing;
        }
    }


    void Update()
    {
        SimulationStep(Time.deltaTime);

        for(int i = 0; i < numParticles; i++) {
            Draw.Point(new Vector3(positions[i].x, positions[i].y, 0), particleSize, Color.white);
        }


        
        Draw.BoxOutline(Vector2.zero, boundarySize, 0.1f, Color.white, 1);
    }

    //Resolves colisions of particles with the boundary
    void resolveColisions (int i) {
        Vector2 halfBoundSize = boundarySize / 2 - Vector2.one * particleSize;
        //resolves colisions on the x axis
        if(Abs(positions[i].x) > halfBoundSize.x) {
            positions[i].x = halfBoundSize.x * Sign(positions[i].x);
            velocities[i].x *= -1 * bounceCoeficient;
        }
        //resolves colisions on the y axis
        if(Abs(positions[i].y) > halfBoundSize.y) {
            positions[i].y = halfBoundSize.y * Sign(positions[i].y);
            velocities[i].y *= -1 * bounceCoeficient;
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

    float smoothingKernel(float dst){
        if (dst >= smoothingRadius) return 0;

        float volume = (float)Math.PI * (float)Math.Pow(smoothingRadius, 4) / 6;
        return (smoothingRadius - dst) * (smoothingRadius - dst) / volume;
    }

    float smoothingKernelDerivative(float dst) {
        if (dst >= smoothingRadius) return 0;

        float scale = 12 / ((float)Math.Pow(smoothingRadius, 4) * (float)Math.PI);
        return (dst - smoothingRadius) * scale;
    }

    float calculateDensity (Vector2 samplePoint) {
        float density=0;

        foreach(Vector2 point in positions){
            float dst = (point - samplePoint).magnitude;
            float influence = smoothingKernelDerivative(dst);
            density += influence*mass;
        }

        return density;
    }

    float ConvertDensityToPressure(float density) {
        float densityError = density - targetDensity;
        float pressure = densityError * pressureMultiplier;
        return pressure;
    }

    Vector2 CalculatePressureForce (int particleIndex) {
        Vector2 pressureForce = Vector2.zero;

        for(int i = 0; i < numParticles; i++) {
            if(particleIndex == i) continue;

            Vector2 offset = positions[i] - positions[particleIndex];
            float dst = offset.magnitude;
            Vector2 dir = dst == 0 ? Vector2.one : offset / dst;

            float slope = smoothingKernelDerivative(dst);
            float density = densities[i];
            pressureForce += -ConvertDensityToPressure(density) * dir * slope * mass / density; 
        }

        return pressureForce;
    }





    void SimulationStep (float deltaTime) {
        //Apply gravity and calculate densities
        Parallel.For(0, numParticles, i =>{
            velocities[i] += Vector2.down * gravity * deltaTime;
            predictedPositions[i] = positions[i] + velocities[i] * deltaTime; 
            densities[i] = calculateDensity(predictedPositions[i]);
        });

        //Apply pressure force
        Parallel.For(0, numParticles, i =>{
            Vector2 pressureForce = CalculatePressureForce(i);
            Vector2 pressureAcceleration = pressureForce / densities[i];
            velocities[i] += pressureAcceleration * deltaTime;
        });

        //Update positions and resolve collisions
        Parallel.For(0, numParticles, i =>{
            positions[i] += velocities[i] * deltaTime;
            resolveColisions(i);
        });
    }


}
