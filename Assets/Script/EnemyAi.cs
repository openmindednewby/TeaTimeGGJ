using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyAi : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    private GameManager _gameManager;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //States
    public float sightRange;
    public bool playerInSightRange;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        Debug.Log("YO WTF");
    }

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        //Check for sight range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);

        //Not
        if (!playerInSightRange) Patroling();

        if (playerInSightRange) ChasePlayer();
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet) agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        //Calc random Point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
        //transform.LookAt(player);
        Debug.Log("Chasing You");
        Debug.Log(player.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<InteractScript>(out var playerInteractScript)) return;
        if (!other.TryGetComponent<CharacterController>(out var characterController)) return;
        switch (playerInteractScript.collectedItems)
        {
            case 0:
                characterController.enabled = false;
                characterController.transform.position = _gameManager.GetLeve1RP().position;
                characterController.enabled = true;
                break;
            case 1:
                characterController.enabled = false;
                characterController.transform.position = _gameManager.GetLeve2RP().position;
                characterController.enabled = true;
                break;
            case 2:
                characterController.enabled = false;
                characterController.transform.position = _gameManager.GetLeve3RP().position;
                characterController.enabled = true;
                break;
            case 3:
                characterController.enabled = false;
                characterController.transform.position = _gameManager.GetLeve4RP().position;
                characterController.enabled = true;
                break;
        }
        Destroy(gameObject);
    }
}
