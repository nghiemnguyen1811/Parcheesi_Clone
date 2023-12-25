using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public struct FieldData : IEquatable<FieldData>
{
    public string name;
    public Field field;
    public Material material;
    public Transform[] spawnPoints;
    public Transform[] HOFPositions;
    public List<Piece> pieceList;
    public List<Piece> HOFPieceList;

    public static bool operator ==(FieldData fd1, FieldData fd2)
    {
        return fd1.Equals(fd2);
    }

    public static bool operator !=(FieldData fd1, FieldData fd2)
    {
        return !fd1.Equals(fd2);
    }

    public override bool Equals(object obj) => Equals(obj is FieldData);

    public bool Equals(FieldData other)
    {
        return (name == other.name);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
public class Board : MonoBehaviour
{
    private const int MaxStepEachField = 14;

    public static int FieldIndex = 0;
    [SerializeField] private VoidEvent OnThingSelected;
    [SerializeField] private ResultEvent OnResultReceive;
    [SerializeField] private TransformRuntimeSet allStepRuntimeSet;
    [SerializeField] private FieldData[] dataArray;
    [SerializeField] private Piece piecePrefab;
    [SerializeField] private Transform[] stepHolder;

    [Header("Cinemachine Camera")]
    [SerializeField] private CameraSwitcher cameraSwitcher;

    public FieldData SelectedField { get => dataArray[FieldIndex]; }
    public FieldData[] DataArray { get => dataArray; }
    private Result diceManagerResult;
    private List<InteractableObject> interactableObjects = new List<InteractableObject>();
    private void OnEnable()
    {
        //Get all steps in board
        List<Transform> tempList = new List<Transform>();
        for (int i = 0; i < stepHolder.Length; i++)
        {
            foreach (Transform step in stepHolder[i])
            {
                allStepRuntimeSet.AddToList(step);
            }
        }
    }
    private void OnDisable()
    {
        allStepRuntimeSet.Initialize();
    }
    private void Start()
    {
        InitPieces();
        cameraSwitcher.SwitchCameraTo(FieldIndex);
    }

    //Event listener methods
    public void DiceManager_OnFinishChecking(Result result)
    {
        diceManagerResult = result;
        OnResultReceive?.Raise(result);
        if (result.isMatched)
        {
            if (HaveActivePiece())
            {
                if (!CheckStartStep(GetStartStep())) //if startstep have selected field piece
                {
                    InteractableObject field = SelectedField.field;
                    field.IsInteractable = true;
                    interactableObjects.Add(field);
                }
                CheckHOF(result.totalValue / 2);

                CheckNormalPiece();

                if (!interactableObjects.Any())
                {
                    NextField();
                }
            }
            else
            {
                SpawnPiece();
            }
        }
        else
        {
            Debug.Log("No Matched!");
            if (HaveActivePiece())
            {
                CheckNormalPiece();
                if (!interactableObjects.Any())
                {
                    NextField();
                }
            }
            else
            {
                NextField();
            }
        }
    }

    private void CheckNormalPiece()
    {
        for (int i = 0; i < SelectedField.pieceList.Count; i++)
        {
            Piece piece = SelectedField.pieceList[i];
            if (!piece.IsActive || piece.IsHOFPiece) continue;
            int toStepIndex = GetToStepIndex(piece);
            if (IsOutOfBound(piece) || BlockByAnother(piece, toStepIndex))
            {
                Debug.Log(" " + IsOutOfBound(piece) + " " + BlockByAnother(piece, toStepIndex));
                piece.IsInteractable = false;
            }
            else
            {
                interactableObjects.Add(piece);
                piece.IsInteractable = true;
            }
        }
    }

    public void Piece_OnPieceInteract(Piece selectedPiece)
    {
        OnThingSelected?.Raise();
        int toStepIndex = GetToStepIndex(selectedPiece);

        //Handle if HOF step have a piece
        if (selectedPiece.IsHOFPiece)
        {
            int value = diceManagerResult.totalValue / 2;
            selectedPiece.HOFindex = value;
            Transform HOFTransform = SelectedField.HOFPositions[value - 1];
            selectedPiece.MovePieceToHOF(HOFTransform);
            SelectedField.pieceList.Remove(selectedPiece);
            SelectedField.HOFPieceList.Add(selectedPiece);
        }
        else
        {
            selectedPiece.MovePieceToStep(diceManagerResult.totalValue, CheckStepIsHOFStep(toStepIndex), GetBlockedPieceAtOther(toStepIndex));
        }

        //End game condition
        if (!SelectedField.pieceList.Any())
        {
            //End game events
            return;
        }
        //Player make a matched, gains a extra turn
        // if (diceManagerResult.isMatched)
        // {
        //     Reset();
        //     return;
        // }
        NextField();
    }

    public void Field_OnFieldClicked()
    {
        OnThingSelected?.Raise();
        SpawnPiece();
    }

    private void InitPieces()
    {
        Debug.Log("Game started!");
        for (int i = 0; i < dataArray.Length; i++)
        {
            FieldData fieldData = dataArray[i];
            GameObject parent = new GameObject("Piece Holder " + i);
            for (int j = 0; j < fieldData.spawnPoints.Length; j++)
            {
                Transform spawnPoint = fieldData.spawnPoints[j];
                Piece pieceInstance = GameObject.Instantiate(piecePrefab, spawnPoint.position, Quaternion.identity, parent.transform);
                pieceInstance.SpawnPoint = spawnPoint;
                pieceInstance.BaseRotation = Quaternion.identity;
                pieceInstance.ChangeMaterial(fieldData.material);
                fieldData.pieceList.Add(pieceInstance);
            }
        }
    }

    public bool HaveActivePiece()
    {
        for (int i = 0; i < SelectedField.pieceList.Count; i++)
        {
            Piece piece = SelectedField.pieceList[i];
            if (piece.IsActive)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckStartStep(int startStep)
    {
        for (int i = 0; i < SelectedField.pieceList.Count; i++)
        {

            Piece piece = SelectedField.pieceList[i];
            if (!piece.IsActive) continue;
            if (piece.StepIndex == startStep)
            {
                return true;
            }
        }
        return false;
    }

    private void CheckHOF(int singleDiceResult)
    {
        for (int i = 0; i < SelectedField.pieceList.Count; i++)
        {
            Piece piece = SelectedField.pieceList[i];
            if (!piece.IsActive) continue;
            //Piece is HOF piece
            if (piece.IsHOFPiece)
            {
                if (CheckHOFList(singleDiceResult))
                {
                    piece.IsInteractable = false;
                }
                else
                {
                    interactableObjects.Add(piece);
                    piece.IsInteractable = true;
                }
            }
        }
    }

    private bool CheckHOFList(int singleDiceResult)
    {
        if (!SelectedField.HOFPieceList.Any()) return false;
        for (int i = 0; i < SelectedField.HOFPieceList.Count; i++)
        {
            Piece hofPiece = SelectedField.HOFPieceList[i];
            if (hofPiece.HOFindex == singleDiceResult)
            {
                return true;
            }
        }
        return false;
    }

    private bool BlockByAnother(Piece piece, int toStepIndex)
    {
        if (GetBlockedPieceAtOther(toStepIndex))
        {
            return false;
        }

        for (int i = 0; i < diceManagerResult.totalValue; i++)
        {
            int interatedIndex = piece.StepIndex + i + 1;

            if (CheckAhead(CheckIndex(interatedIndex)))
            {
                return true;
            }
        }
        return false;
    }

    private int CheckIndex(int temp)
    {
        List<Transform> allStep = allStepRuntimeSet.Items;
        if (temp >= allStep.Count)
        {
            return temp - allStep.Count;
        }
        return temp;
    }

    private bool CheckAhead(int temp)
    {
        Debug.Log(temp);
        for (int i = 0; i < dataArray.Length; i++)
        {
            for (int j = 0; j < dataArray[i].pieceList.Count; j++)
            {
                Piece piece = dataArray[i].pieceList[j];
                if (!piece.IsActive) continue;
                if (piece.StepIndex == temp)
                {
                    return true;
                }
            }
        }
        return false;
    }

    //TODO:Implement this
    private bool IsOutOfBound(Piece piece)
    {
        int temp = piece.TotalStepCounter + diceManagerResult.totalValue;
        if (temp > allStepRuntimeSet.Items.Count)
        {
            return true;
        }
        return false;
    }

    //Place piece on start position
    private void SpawnPiece()
    {
        Piece selectedPiece = GetInactivePiece(SelectedField);
        int startStep = GetStartStep();

        selectedPiece.IsActive = true;
        selectedPiece.MovePieceToStart(startStep, GetBlockedPieceAtOther(startStep));
        NextField();
    }

    private Piece GetBlockedPieceAtOther(int checkStep)
    {
        for (int i = 0; i < dataArray.Length; i++)
        {
            if (dataArray[i] == SelectedField)
            {
                continue;
            }
            for (int j = 0; j < dataArray[i].pieceList.Count; j++)
            {
                Piece piece = dataArray[i].pieceList[j];
                if (piece.StepIndex == checkStep)
                {
                    return piece;
                }
            }
        }
        return null;
    }

    private void NextField()
    {
        Reset();
        FieldIndex++;
        if (FieldIndex > dataArray.Length - 1)
        {
            FieldIndex = 0;
        }
        cameraSwitcher.SwitchCameraTo(FieldIndex);
    }

    private void Reset()
    {
        interactableObjects.Clear();
        DiceManager.Rolled = false;
    }

    private bool CheckStepIsHOFStep(int step)
    {
        return step == GetHOFStep();
    }

    private int GetStartStep()
    {
        return GetHOFStep() + 1;
    }

    private int GetHOFStep()
    {
        return FieldIndex * MaxStepEachField;
    }

    private Piece GetInactivePiece(FieldData selectedPiece)
    {
        foreach (Piece piece in SelectedField.pieceList)
        {
            if (!piece.IsActive)
            {
                return piece;
            }
        }
        return null;
    }

    private int GetToStepIndex(Piece selectedPiece)
    {
        int toStepIndex = selectedPiece.StepIndex + diceManagerResult.totalValue;
        List<Transform> allStep = allStepRuntimeSet.Items;
        if (toStepIndex >= allStep.Count)
        {
            var temp = toStepIndex - allStep.Count;
            toStepIndex = temp;
        }

        return toStepIndex;
    }

}
