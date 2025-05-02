using System;

/// <summary>
/// This is a cell, representing a letter on the board
/// </summary>
public class Cell
{
    public char character;           // The letter in the cell
    public bool isInWord;            // Is the letter in the word
    public bool isInCorrectPosition; // Is the letter in the correct spot in the word

    /// <summary>
    /// Constructor
    /// </summary>
    public Cell()
    {
        character = ' ';
        isInWord = false;
        isInCorrectPosition = false;
    }

    /// <summary>
    /// Constructor with predetermined letter to set
    /// </summary>
    /// <param name="character"></param>
    public Cell(char character)
    {
        this.character = character;
        isInWord = false;
        isInCorrectPosition = false;
    }
}
