using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class BoardScript : MonoBehaviour
{
    // make an array for the game board
    public int[,] gameBoard = new int[,]
    {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
    };

    // make an array for the left L block
    public int[,] leftLBlock = new int[,]
    {
        {0, 0},
        {0, 1}, {1, 1}, {2, 1}
    };

    // function for testing if the next block position will go out of bounds
    // true means outside of bounds
    public bool TestOutside(int x, int y, int xUpdates = 0, int yUpdates = 0) {
        // if it is outside of bounds of 10 x 20 box return true
        if ((x + xUpdates > 10) || (y + yUpdates > 20))
        {
            return true;
        }
        //otherwise return false
        else {
            return false;
        }
    }


    public bool TestOutsideBlock(int x, int y, int xUpdates = 0, int yUpdates = 0) {
        // test if left L block is outside of the game board
        for (int i = 0; i < leftLBlock.GetLength(0); i++) {
            TestOutside(x + leftLBlock[i, 0], y + leftLBlock[i, 1], xUpdates, yUpdates);
        }
        return false;
    }

    // function for updating a block's position
    public void UpdateBlock(int x, int y, int value) {
        // left L block game board updates
        for (int i = 0; i < leftLBlock.GetLength(0); i++) {
            UpdateBoard(x + leftLBlock[i, 0], y + leftLBlock[i, 1], value);
        }
    }

    // function for updating the game board
    public void UpdateBoard(int x, int y, int value) {
        gameBoard[y, x] = value;
    }

    //function for printing the board
    public void PrintBoard() {
        // Get the number of rows and columns
        int rows = gameBoard.GetLength(0);
        int cols = gameBoard.GetLength(1);
        string combineString = "";

        // Iterate through each row
        for (int i = 0; i < rows; i++)
        {
            // Iterate through each column in the current row
            for (int j = 0; j < cols; j++)
            {
                // Append the element to the string using the concatenation operator +=
                combineString += gameBoard[i, j] + " ";
            }
            // Log the complete row string to the Unity console [1]
            combineString += "\n";
        }
        print(combineString);
    }
}
