using System;

/// <summary>
/// This class manages the logical board
/// </summary>
public class Board
{
    public int NUM_ROWS { get { return 6; } }    // Number of rows in the board
    public int NUM_COLUMNS { get { return 5; } } // Number of columns in the board

    public string activeWord; // The word the user is currently guessing
    public int activeRow;     // The row the user is currently on
    public int activeColumn;  // The column the user is currently on

    public Cell[,] board;     // The logical board

    /// <summary>
    /// Constructor
    /// </summary>
    public Board()
    {
        board = new Cell[NUM_ROWS, NUM_COLUMNS];
        initializeCells();
    }

    /// <summary>
    /// Initialize the board with empty cells
    /// </summary>
    public void initializeCells()
    {
        int rowIndex;
        int columnIndex;

        activeRow = 0;
        activeColumn = 0;

        for (rowIndex = 0; rowIndex < NUM_ROWS; rowIndex++)
        {
            for (columnIndex = 0; columnIndex < NUM_COLUMNS; columnIndex++)
            {
                if (board[rowIndex, columnIndex] == null)
                    board[rowIndex, columnIndex] = new Cell();
                else
                    board[rowIndex, columnIndex].character = ' ';
            }
        }
    }

    /// <summary>
    /// Add a letter to the next available space in the board
    /// </summary>
    public void addLetter(char letter)
    {
        if (!(activeColumn == NUM_COLUMNS && activeRow == NUM_ROWS))
        {
            if (activeColumn <= NUM_COLUMNS - 1)
            {
                activeColumn++;
                board[activeRow, activeColumn - 1].character = char.ToLower(letter);
            }
        }

    }

    /// <summary>
    /// Delete the last letter in the board
    /// </summary>
    public char deleteLetter()
    {
        char deletingLetter = ' ';

        if (!(activeRow == 0 && activeColumn == 0))
        {
            if (activeColumn > 0)
            {
                activeColumn--;
                deletingLetter = board[activeRow, activeColumn].character;
                board[activeRow, activeColumn].character = ' ';
            }
        }

        return deletingLetter;
    }

    /// <summary>
    /// Get the word from the active row
    /// </summary>
    /// <returns></returns>
    public string getWordFromRow()
    {
        string word = "";
        int    columnIndex;

        for (columnIndex = 0; columnIndex < NUM_COLUMNS; columnIndex++)
            word += board[activeRow, columnIndex].character;

        return word;
    }

    /// <summary>
    /// Get the word from a specific row
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string getWordFromRow(int row)
    {
        int rowIndex;

        if (row < 0 || row >= NUM_ROWS)
            throw new Exception("Row out of bounds");

        string word = "";

        for (rowIndex = 0; rowIndex < NUM_COLUMNS; rowIndex++)
            word += board[row, rowIndex].character;

        return word;
    }

    /// <summary>
    /// Check if the active row is full
    /// </summary>
    /// <returns></returns>
    public bool rowIsFull()
    {
        return activeColumn == NUM_COLUMNS;
    }
}