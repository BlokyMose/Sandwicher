using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandwicher
{
    public class LoopingMovement : MonoBehaviour
    {
        [SerializeField]
        Transform origin;

        [SerializeField]
        Transform destination;

        [SerializeField]
        float speed = 1f;

        [SerializeField]
        float speedRange = 1f;

        [SerializeField]
        bool hasInitialDelay = false;

        [SerializeField]
        float delay = 1f;

        [SerializeField]
        float delayRange = 1f;

        [SerializeField]
        bool isLooping = true;

        [SerializeField]
        bool isInitiallyRandomlySpawned = true;

        float currentSpeed;
        float currentDelay;
        float delayTime;

        void Awake()
        {
            currentSpeed = speed + Random.Range(-speedRange, speedRange);
            currentDelay = hasInitialDelay ? delay + Random.Range(-delayRange, delayRange) : 0f;
            if(isInitiallyRandomlySpawned)
                transform.position = new Vector2(
                    Random.Range(origin.position.x, destination.position.x),
                    Random.Range(origin.position.y, destination.position.y));
        }

        void Update()
        {
            delayTime += Time.deltaTime;
            if (delayTime > currentDelay)
            {
                transform.position = Vector2.MoveTowards(transform.position, destination.position, currentSpeed * Time.deltaTime);
                
                if (Vector2.Distance(transform.position, destination.position) < .1f && isLooping)
                {
                    transform.position = origin.position;
                    currentSpeed = speed + Random.Range(-speedRange, speedRange);
                    currentDelay = delay + Random.Range(-delayRange, delayRange);
                    delayTime = 0f;
                }
            }
        }
    }
}
