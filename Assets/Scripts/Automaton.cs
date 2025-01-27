using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class Automaton : MonoBehaviour
{
    Rigidbody2D rigidBody;
    public bool isDead;
    public float timeAlive;
    public float detectionRadius = 10f; // Radius to detect other automata
    public Vector2 desiredDirection;

    private Vector3 prevPosition;
    private int minimalMovementCounter = 0;
    private const float movementThreshold = 0.025f; // Threshold for minimal movement
    private const int maxMinimalMovementUpdates = 100;

    public Network network;
    [SerializeField] int[] layerSizes;

    public void BuildAutomaton()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        network = new Network(layerSizes);
    }

    private void Update()
    {
        if (!isDead)
        {
            // Create a list of automata in a fixed radius around automaton
            List<Automaton> closestAutomata = FindAutomataInRadius(detectionRadius);
            // Calculate vectors based on Boids algorithm and unobstructed vector
            Vector2[] directionVectors = CalculateDirectionVectors(closestAutomata);
            // Convert the vectors to an array of doubles
            Vector2[] distanceToBounds = FindDistanceToBounds();
            double[] inputs = new double[]
            {
                rigidBody.velocity.x,
                rigidBody.velocity.y,
                distanceToBounds[0].x,
                distanceToBounds[0].y,
                distanceToBounds[1].x,
                distanceToBounds[1].y,
                closestAutomata.Count
            };

            // Calculate the outputs of the neural network
            double[] outputs = network.CalculateOutputs(inputs);

            // Move the automaton based on the outputs of the neural network
            Move(outputs, directionVectors);
            // Check if the automaton is a dud and execute
            if (isDud())
            {
                Kill();
            }

            timeAlive += Time.deltaTime;
        }
    }

    private bool isDud()
    {
        float distanceMoved = Vector3.Distance(prevPosition, transform.position);
        prevPosition = transform.position;

        if (distanceMoved < movementThreshold)
        {
            minimalMovementCounter++;
        }
        else
        {
            minimalMovementCounter = 0;
        }

        return minimalMovementCounter >= maxMinimalMovementUpdates;
    }

    public void Move(double[] scalars, Vector2[] directions)
    {
        Vector2 netForce = new Vector2();
        for (int i = 0; i < scalars.Length; i++)
        {
            netForce += (float)scalars[i] * directions[i];
        }
        // Generate a random direction
        Vector2 randomDirection = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
        // Generate a random scalar for the force magnitude
        float randomForceMagnitude = UnityEngine.Random.Range(0.1f, 4f);
        // Add the random force to the net force
        netForce += randomDirection * randomForceMagnitude;

        rigidBody.AddForce(netForce);
        desiredDirection = netForce;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "bounds")
        {
            Kill();
        }
    }

    private List<Automaton> FindAutomataInRadius(float radius)
    {
        List<Automaton> allAutomata = FindObjectsOfType<Automaton>().ToList();
        allAutomata.Remove(this); // Remove the current automaton from the list

        return allAutomata
            .Where(automaton => !automaton.isDead && Vector3.Distance(transform.position, automaton.transform.position) <= radius)
            .ToList();
    }

    public Vector2[] CalculateDirectionVectors(List<Automaton> neighbors)
    {
        Vector2 alignmentVector = new Vector2();
        Vector2 cohesionVector = new Vector2();
        Vector2 separationVector = new Vector2();
        Vector2 cooperationVector = new Vector2();

        foreach (Automaton automaton in neighbors)
        {
            alignmentVector += automaton.GetComponent<Rigidbody2D>().velocity;
            cohesionVector += new Vector2(automaton.transform.position.x - transform.position.x, automaton.transform.position.y - transform.position.y);
            float automatonDistance = Vector2.Distance(automaton.transform.position, transform.position);
            separationVector += new Vector2(transform.position.x - automaton.transform.position.x, transform.position.y - automaton.transform.position.y).normalized / automatonDistance;
            cooperationVector += automaton.desiredDirection;
        }

        return new Vector2[] { 2 * alignmentVector.normalized, cohesionVector.normalized, separationVector.normalized, 2 * desiredDirection.normalized, 2 * UnobstructedVector().normalized };
    }

    Vector2 UnobstructedVector()
    {
        Vector2 bestDirection = transform.up;
        float furthestUnobstructedDistance = 0;
        RaycastHit2D hit;

        Vector2[] directions = Generate2DDirections(300);

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2 dir = transform.TransformDirection(directions[i]);
            hit = Physics2D.CircleCast(transform.position, 0.2f, dir, detectionRadius, LayerMask.GetMask("bounds"));
            if (hit.collider != null)
            {
                if (hit.distance > furthestUnobstructedDistance)
                {
                    bestDirection = dir;
                    furthestUnobstructedDistance = hit.distance;
                }
            }
            else
            {
                return dir;
            }
        }
        return bestDirection;
    }

    public static Vector2[] Generate2DDirections(int numDirections)
    {
        Vector2[] directions = new Vector2[numDirections];
        float angleIncrement = 360f / numDirections;

        for (int i = 0; i < numDirections; i++)
        {
            float angle = i * angleIncrement;
            float x = Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = Mathf.Sin(angle * Mathf.Deg2Rad);
            directions[i] = new Vector2(x, y);
        }

        return directions;
    }

    private Vector2[] FindDistanceToBounds()
    {
        Vector2 direction = rigidBody.velocity.normalized;
        float angle = 15f; // Half of 30 degrees

        // Calculate the two direction vectors with a 30-degree angle between them
        Vector2 direction1 = Quaternion.Euler(0, 0, angle) * direction;
        Vector2 direction2 = Quaternion.Euler(0, 0, -angle) * direction;

        // Perform raycasts for both directions
        RaycastHit2D hit1 = Physics2D.Raycast(transform.position, direction1, detectionRadius, LayerMask.GetMask("bounds"));
        RaycastHit2D hit2 = Physics2D.Raycast(transform.position, direction2, detectionRadius, LayerMask.GetMask("bounds"));

        // Calculate the distances
        Vector2 distance1 = direction1 * (hit1.collider != null ? hit1.distance : detectionRadius);
        Vector2 distance2 = direction2 * (hit2.collider != null ? hit2.distance : detectionRadius);

        return new Vector2[] { distance1, distance2 };
    }

    private void Kill()
    {
        isDead = true;
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        gameObject.layer = LayerMask.NameToLayer("deadAutomata");
    }
}
