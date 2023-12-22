using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
public class Dice : MonoBehaviour
{
    public Transform[] diceFaces;
    public Rigidbody rb;

    public bool hasStoppedRolling { get; set; }
    private bool delayFinished;
    private int index;
    public int result;
    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (!delayFinished) return;

        if (!hasStoppedRolling && rb.velocity.sqrMagnitude == 0f)
        {
            result = GetNumberOnTopFace();
            hasStoppedRolling = true;
        }
    }

    [ContextMenu("Get top face")]
    private int GetNumberOnTopFace()
    {
        if (diceFaces == null) return -1;

        int topFace = 0;
        float lastYPosition = diceFaces[0].position.y;

        for (int i = 0; i < diceFaces.Length; i++)
        {
            if (diceFaces[i].position.y > lastYPosition)
            {
                lastYPosition = diceFaces[i].position.y;
                topFace = i;
            }
        }
        //Debug.Log($"Dice result {topFace + 1}");
        return topFace + 1;
    }

    internal void Roll(float throwForce, float rollForce, int i)
    {
        index = i;
        var randomVariance = UnityEngine.Random.Range(-1f, 1f);
        rb.AddForce(transform.forward * (throwForce + randomVariance), ForceMode.Impulse);

        var randX = UnityEngine.Random.Range(0f, 1f);
        var randY = UnityEngine.Random.Range(0f, 1f);
        var randZ = UnityEngine.Random.Range(0f, 1f);

        rb.AddTorque(new Vector3(randX, randY, randZ) * (rollForce + randomVariance), ForceMode.Impulse);

        DelayResult();
    }

    private async void DelayResult()
    {
        await Task.Delay(1000);
        delayFinished = true;
    }
}
