using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
public struct Result
{
    public int dice1Result;
    public int dice2Result;

    public int totalValue { get => dice1Result + dice2Result; }

    public bool isMatched { get => dice1Result == dice2Result; }
}
public class DiceManager : MonoBehaviour
{

    public static bool Rolled { get; set; }
    private const int amountOfDice = 2;
    [SerializeField] private ResultEvent OnResultChecked;
    [SerializeField] private Dice dicePrefab;
    [SerializeField] private float throwForce;
    [SerializeField] private float rollForce;
    [SerializeField] private bool debug;
    [Range(1, 6)][SerializeField] private int debugDice1Result, debugDice2Result;
    private Result Result;
    private List<Dice> allDices = new List<Dice>();
    private bool check;


    private void Update()
    {
        if (!Rolled)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Rolled = true;
                RollDice();
            }
        }

        if (!allDices.Any()) return;
        if (allDices[0].hasStoppedRolling && allDices[1].hasStoppedRolling && !check)
        {
            check = true;
            Check();
        }
    }

    private void Check()
    {
        if (debug)
        {
            allDices[0].result = debugDice1Result;
            allDices[1].result = debugDice2Result;
        }
        Result.dice1Result = allDices[0].result;
        Result.dice2Result = allDices[1].result;
        //Debug.Log($" Dice 1: {Result.dice1Result} Dice 2: {Result.dice2Result} Total result: {Result.totalValue} Matched?: {Result.isMatched}");
        OnResultChecked?.Raise(Result);
    }

    private async void RollDice()
    {
        check = false;
        foreach (Dice dice in allDices)
        {
            Destroy(dice.gameObject);
        }
        allDices.Clear();
        for (int i = 0; i < amountOfDice; i++)
        {
            Dice diceInstance = Instantiate(dicePrefab, this.transform.position, transform.rotation);
            allDices.Add(diceInstance);
            diceInstance.Roll(throwForce, rollForce, i);

            await Task.Yield();
        }
    }
}
