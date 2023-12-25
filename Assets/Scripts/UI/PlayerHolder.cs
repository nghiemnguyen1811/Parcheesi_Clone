using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHolder : MonoBehaviour
{
    [SerializeField] private PlayerUI[] playerUIArray;
    [SerializeField] private Board board;

    private void Start()
    {
        for (int i = 0; i < playerUIArray.Length; i++)
        {
            playerUIArray[i].imageColor.material = board.DataArray[i].material;
        }
    }

    public void UpdateUI(Result result)
    {
        Debug.Log(Board.FieldIndex);
        playerUIArray[Board.FieldIndex].text1.text = result.dice1Result.ToString();
        playerUIArray[Board.FieldIndex].text2.text = result.dice2Result.ToString();
    }
}
