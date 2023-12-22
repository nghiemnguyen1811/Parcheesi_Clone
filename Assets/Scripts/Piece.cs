using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Piece : InteractableObject
{
    [SerializeField] private TransformRuntimeSet allStepRuntimeSet;
    [SerializeField] private PieceEvent OnPieceInteract;
    [SerializeField] Renderer pieceRenderer;
    private bool isHOFPiece = false;
    private bool isActive = false;
    private int stepIndex;
    public Transform SpawnPoint { get; set; }
    public Quaternion BaseRotation { get; set; }
    public int TotalStepCounter { get; set; }
    public int HOFindex { get; set; }
    public int StepIndex { get => stepIndex; set => stepIndex = value; }
    public bool IsHOFPiece { get => isHOFPiece; set => isHOFPiece = value; }
    public bool IsActive
    {
        get => isActive;
        set
        {
            if (value == false)
            {
                TotalStepCounter = 0;
            }
            isActive = value;
        }
    }

    private void Awake()
    {
        selectVisual.SetActive(false);
    }

    public override void Interact()
    {
        OnPieceInteract.Raise(this);
    }

    public void ChangeMaterial(Material material)
    {
        pieceRenderer.material = material;
    }

    //Move piece to stepindex
    public void MovePieceToStep(int totalCount, bool isHOFStep, Piece blockedPiece)
    {
        if (isHOFStep)
        {
            isHOFPiece = true;
        }
        else
        {
            TotalStepCounter += totalCount;
        }

        StartCoroutine(MovePieceToIndex_Coroutine(totalCount, blockedPiece));
    }

    private IEnumerator MovePieceToIndex_Coroutine(int totalCount, Piece blockedPiece)
    {
        List<Transform> allSteps = allStepRuntimeSet.Items;

        int count = 0;
        while (count < totalCount)
        {
            var nextStep = stepIndex + 1;
            if (nextStep >= allSteps.Count)
            {
                nextStep = 0;
            }
            Vector3 fromPos = allSteps[stepIndex].position;

            Vector3 toPos = allSteps[nextStep].position;

            float distance = (toPos - fromPos).magnitude;

            Vector3 direction = (toPos - fromPos).normalized;

            float remainingDistance = distance;
            while (remainingDistance > 0.01f)
            {
                remainingDistance = LerpParabola(fromPos, toPos, distance, direction, remainingDistance);
                yield return null;
            }

            stepIndex = nextStep;
            count++;
        }
        Kick(blockedPiece);
    }

    //Move piece to start step
    public void MovePieceToStart(int toStepIndex, Piece blockedPiece)
    {

        StartCoroutine(MovePieceToStart_Coroutine(toStepIndex, blockedPiece));
    }

    private IEnumerator MovePieceToStart_Coroutine(int toStepIndex, Piece blockedPiece)
    {
        Vector3 fromPos = this.transform.position;

        Vector3 toPos = allStepRuntimeSet.GetItemIndex(toStepIndex).position;

        Vector3 nextPos = allStepRuntimeSet.GetItemIndex(toStepIndex + 1).position;

        float distance = (toPos - fromPos).magnitude;

        Vector3 direction = (nextPos - toPos).normalized;

        float remainingDistance = distance;

        while (remainingDistance > 0.01f)
        {
            remainingDistance = LerpParabola(fromPos, toPos, distance, direction, remainingDistance);
            yield return null;
        }

        stepIndex = toStepIndex;
        Kick(blockedPiece);
    }

    //Move piece to HOF transforms
    public void MovePieceToHOF(Transform HOFTransform)
    {
        StartCoroutine(MovePieceToHOF_Coroutine(HOFTransform));
    }

    private IEnumerator MovePieceToHOF_Coroutine(Transform HOFTransform)
    {
        Vector3 fromPos = this.transform.position;
        Vector3 toPos = HOFTransform.position;
        float distance = (toPos - fromPos).magnitude;
        Vector3 direction = (toPos - fromPos).normalized;
        float remainingDistance = distance;

        while (remainingDistance > 0.01f)
        {
            remainingDistance = LerpParabola(fromPos, toPos, distance, direction, remainingDistance);
            yield return null;
        }
    }

    private float LerpParabola(Vector3 fromPos, Vector3 toPos, float distance, Vector3 direction, float remainingDistance)
    {
        float moveSpeed = 2f;
        Vector3 jumpPos = Vector3.Lerp(fromPos, toPos, 0.5f);
        jumpPos.y += 1f;

        Quaternion fromRotation = transform.rotation;
        Quaternion toRotation = Quaternion.LookRotation(direction);

        //t value of lerp and slerp (it's stand for the percent of the process in t/1)
        float t = 1 - (remainingDistance / distance);
        //Lerp move object along parabola
        this.transform.position = Vector3.Lerp(Vector3.Lerp(fromPos, jumpPos, t), Vector3.Lerp(jumpPos, toPos, t), t);
        //Slerp rotation of object
        this.transform.rotation = Quaternion.Slerp(fromRotation, toRotation, t);
        remainingDistance -= moveSpeed * Time.deltaTime;
        return remainingDistance;
    }

    private static void Kick(Piece blockedPiece)
    {
        if (blockedPiece != null)
        {
            blockedPiece.IsActive = false;
            blockedPiece.transform.position = blockedPiece.SpawnPoint.position;
            blockedPiece.transform.rotation = blockedPiece.BaseRotation;
        }
    }
}
