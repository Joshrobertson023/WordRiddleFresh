using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;

/// <summary>
/// This class manages the state of the game.
/// </summary>
public class Game
{
    public int maxHints = 4;              // Maximum number of hints allowed

    private string[] possibleWords;       // List of possible words to be chosen for a guess
    private string[] allowedWords;        // List of valid words
    public string chosenWord;             // The chosen word the user has to guess
    public int hints;                     // Number of hints used
    private char randomLetter;            // Random letter used to give hints
    private char lastHint;                // Letter last used as a hint

    private Random random = new Random(); // Randomly select a word from the list of possible words
    public Board board;                   // The logical game board

    private Dictionary<char, int> remainingChars; // Letters remaining in the chosen word to be guessed and their occurrences
    private List<char> remainingUsedChars;        // Letters remaining in the chosen word to be guessed - used to update used letters
    private Dictionary<char, int> charsInWord;    // Letters in the chosen word and their occurrences
    private List<int> shownPositions;             // Positions already shown as a hint
    public List<Cell> usedLetters;                // Letters used for a guess
    private List<char> availableLetters;          // Letters still available to use as a hint
    public List<char> usedLettersChars;           // Characters of used letters to compare for hints
    private List<(char letter, int correctPosition)> misplacedLetters;
                                                  // Letters user guessed that are not in the correct spot
    private List<(char letter, int correctPosition)> notUsedHints;
                                                  // Misplaced letters still not used as a hint

    /// <summary>
    /// Constructor
    /// </summary>
    public Game()
    {
        int letterIndex;   // Index of a letter in the chosen word
        char letterInWord; // A letter in the chosen word

        board = new Board();
        board.initializeCells();

        possibleWords = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "shuffled_real_wordles.txt"));
        allowedWords = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "official_allowed_guesses.txt"));

        chosenWord = possibleWords[random.Next(possibleWords.Length)];

        usedLetters = new List<Cell>();
        usedLettersChars = new List<char>();
        remainingChars = new Dictionary<char, int>();
        shownPositions = new List<int>();

        lastHint = ' ';

        // Count occurrences of each letter in the chosen word
        for (letterIndex = 0; letterIndex < chosenWord.Length; letterIndex++)
        {
            letterInWord = chosenWord[letterIndex];
            if (remainingChars.ContainsKey(letterInWord))
                remainingChars[letterInWord]++;
            else
                remainingChars[letterInWord] = 1;
        }
    }

    /// <summary>
    /// Set the letter in the board at a given row and column
    /// </summary>
    /// <param name="letter"></param>
    public bool addLetter(char letter)
    {
        bool found = false; // The letter was found in the used letters

        board.addLetter(letter);

        foreach (Cell cell in usedLetters)
            if (cell.character == letter)
                found = true;

        if (!found)
        {
            usedLetters.Add(new Cell(letter));
            if (!usedLettersChars.Contains(letter))
                usedLettersChars.Add(letter);
            updateUsedLetters();
        }

        return found;
    }

    /// <summary>
    /// Delete the last letter in the board
    /// </summary>
    /// <returns></returns>
    public bool deleteLetter()
    {
        int usedLetterIndex; // Index of the letter in the used letters
        bool found = false;  // The letter was found in the used letters
        char deleteLetter;   // The letter being deleted

        deleteLetter = board.deleteLetter();

        for (usedLetterIndex = usedLetters.Count - 1; usedLetterIndex >= 0; usedLetterIndex--)
        {
            if (usedLetters[usedLetterIndex].character == deleteLetter)
            {
                found = true;
                usedLetters.RemoveAt(usedLetterIndex);
            }
        }

        return found;
    }

    /// <summary>
    /// Check to see if the user's word is a valid word
    /// </summary>
    public bool checkWord()
    {
        string guess; // The user's guessed word

        guess = board.getWordFromRow();

        return allowedWords.Contains(guess);
    }

    /// <summary>
    /// Check to see if the user guessed the chosen word
    /// </summary>
    public bool checkGuess()
    {
        if (board.activeColumn < board.NUM_COLUMNS)
            throw new Exception("Cannot check guess because the word is not complete.");
        if (board.activeRow < 0 || board.activeRow >= board.NUM_ROWS)
            throw new Exception("Cannot check guess because the active row is out of bounds.");

        int chosenWordIndex; // Index of a letter in the chosen word
        int guessIndex;      // Index of a letter in the guessed word
        int positionIndex;   // Index of a letter's position in a word
        bool equals;         // The user guessed correctly
        string guess;        // The user's guessed word

        equals = board.getWordFromRow().Equals(chosenWord);
        guess = board.getWordFromRow();

        // Count occurrences of each letter in the chosen word
        charsInWord = new Dictionary<char, int>();
        for (chosenWordIndex = 0; chosenWordIndex < chosenWord.Length; chosenWordIndex++)
        {
            char character = chosenWord[chosenWordIndex];
            if (charsInWord.ContainsKey(character))
                charsInWord[character]++;
            else
                charsInWord[character] = 1;
        }

        // Compare each letter guessed to the chosen word and update isInWord and isInCorrectPosition
        for (guessIndex = 0; guessIndex < board.NUM_COLUMNS; guessIndex++)
        {
            if (guess[guessIndex] == chosenWord[guessIndex])
            {
                board.board[board.activeRow, guessIndex].isInCorrectPosition = true;
                board.board[board.activeRow, guessIndex].isInWord = true;
                charsInWord[guess[guessIndex]]--;
            }
        }
        for (positionIndex = 0; positionIndex < board.NUM_COLUMNS; positionIndex++)
        {
            if (!board.board[board.activeRow, positionIndex].isInCorrectPosition)
            {
                if (charsInWord.TryGetValue(guess[positionIndex], out int count) && count > 0)
                {
                    board.board[board.activeRow, positionIndex].isInWord = true;
                    charsInWord[guess[positionIndex]]--;
                }
            }
        }

        board.activeColumn = 0;
        if (board.activeRow < board.NUM_ROWS)
            board.activeRow++;

        return equals;
    }

    /// <summary>
    /// Compare each used letter to the chosen word and set isInWord and isInCorrectPosition
    /// </summary>
    public void updateUsedLetters()
    {
        int activeWordIndex;   // Index of a letter in the active word
        int usedLetterIndex;   // Index of a letter in the used letters list
        int wordPositionIndex; // Index of a letter's position in a word
        string activeWord;     // The user's currently guessed word

        if (board.activeRow > 0)
        {
            activeWord = board.getWordFromRow(board.activeRow - 1);

            remainingUsedChars = new List<char>();
            remainingUsedChars = chosenWord.ToList();

            for (usedLetterIndex = 0; usedLetterIndex < usedLetters.Count; usedLetterIndex++)
            {
                for (activeWordIndex = 0; activeWordIndex < activeWord.Length; activeWordIndex++)
                {
                    if (activeWord[activeWordIndex] == chosenWord[activeWordIndex] && usedLetters[usedLetterIndex].character == chosenWord[activeWordIndex])
                    {
                        usedLetters[usedLetterIndex].isInCorrectPosition = true;
                        usedLetters[usedLetterIndex].isInWord = true;
                    }
                }

                for (wordPositionIndex = 0; wordPositionIndex < activeWord.Length; wordPositionIndex++)
                {
                    if (!board.board[board.activeRow - 1, wordPositionIndex].isInCorrectPosition)
                    {
                        if (remainingUsedChars.Contains(activeWord[wordPositionIndex]))
                        {
                            if (usedLetters[usedLetterIndex].character == activeWord[wordPositionIndex])
                            {
                                usedLetters[usedLetterIndex].isInWord = true;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Get a hint
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string useHint()
    {
        string returnString = ""; // Hint to be returned
        int chosenWordIndex;      // Index of a letter in the chosen word

        if (hints >= maxHints)
            throw new Exception("You already used all your hints.");

        getMisplacedLetters();
        findAvailableLetters();
        checkNotUsedHints();

        if (lastHint != ' ')
        {
            // If the last hint was revealing what a new letter was,
            // show the position of that letter

            for (chosenWordIndex = 0; chosenWordIndex < chosenWord.Length; chosenWordIndex++)
            {
                if (lastHint == chosenWord[chosenWordIndex])
                {
                    if (hints > 0)
                        returnString += "\n";

                    shownPositions.Add(chosenWordIndex);
                    hints++;
                    returnString += "Hint #" + hints + ": '" + randomLetter + "' is in position " + (chosenWordIndex + 1) + ".";
                    lastHint = ' ';
                }
            }
        }
        else
        {
            // If this is the first hint, or if the last hint was revealing the
            // position of a letter

            if (availableLetters.Count > 0)
            {
                // Get a new random letter as a hint

                randomLetter = availableLetters[random.Next(availableLetters.Count)];

                remainingChars[randomLetter]--;
                if (remainingChars[randomLetter] == 0)
                    remainingChars.Remove(randomLetter);
                usedLettersChars.Add(randomLetter);

                if (remainingChars.Count == 0)
                    throw new Exception("No more letters left to use for a hint.");

                if (hints > 0)
                    returnString += "\n";

                returnString += "Hint #" + (hints + 1) + ": \'" + randomLetter + "\' is in the word.";
                hints++;
                lastHint = randomLetter;
            }
            else
            {
                // Once all available letters are used up, show position
                // of a user-guessed letter that is not in the correct spot

                if (notUsedHints.Count > 0)
                {
                    var hint = notUsedHints[random.Next(notUsedHints.Count)];

                    if (hints > 0)
                        returnString += "\n";

                    hints++;
                    returnString += "Hint #" + hints + ": 'The letter " + hint.letter + " should be at position " + (hint.correctPosition + 1) + ".'";
                    shownPositions.Add(hint.correctPosition);
                }
                else
                {
                    throw new Exception("No more letters available to use for a hint.");
                }

            }

        }

        return returnString;
    }

    /// <summary>
    /// Get all misplaced letters still not used as a hint
    /// </summary>
    private void checkNotUsedHints()
    {
        notUsedHints = new List<(char letter, int correctPosition)>();

        foreach (var hint in misplacedLetters)
        {
            if (!shownPositions.Contains(hint.correctPosition))
            {
                notUsedHints.Add(hint);
            }
        }
    }

    /// <summary>
    /// Check previous guesses for a misplaced letter
    /// </summary>
    private void getMisplacedLetters()
    {
        int lastGuessIndex;  // Index of a letter in the last guessed word
        int correctPosition; // Index of a letter's position in the word
        string lastGuess;    // The last guessed word
        Cell cell;           // A cell in the board

        misplacedLetters = new List<(char, int)>();

        if (board.activeRow > 0)
        {
            lastGuess = board.getWordFromRow(board.activeRow - 1);

            for (lastGuessIndex = 0; lastGuessIndex < lastGuess.Length; lastGuessIndex++)
            {
                cell = board.board[board.activeRow - 1, lastGuessIndex];

                if (cell.isInWord && !cell.isInCorrectPosition)
                {
                    for (correctPosition = 0; correctPosition < chosenWord.Length; correctPosition++)
                    {
                        if (chosenWord[correctPosition] == cell.character)
                        {
                            misplacedLetters.Add((cell.character, correctPosition));
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Get letters still available to use for a hint
    /// </summary>
    private void findAvailableLetters()
    {
        availableLetters = new List<char>();

        foreach (Char c in remainingChars.Keys)
        {
            if (remainingChars[c] > 0 && !usedLettersChars.Contains(c))
                availableLetters.Add(c);
        }
    }
}