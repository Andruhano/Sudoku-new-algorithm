using System;
using System.Diagnostics;

public interface IGame
{
    void StartGame();
    void PlaySudoku(int[,] board);
}

public interface IBoardGenerator
{
    int[,] GenerateSudoku(string difficulty);
    int[,] GenerateFullSudoku();
}

public class Program
{
    static Random rand = new Random();
    static void Main(string[] args)
    {
        Game game = new Game();
        game.MainMenu();
    }
}

public class Game 
{
    private static Random rand = new Random();
    private int[,] originalBoard;
    private int[,] currentBoard;
    private int mistakes;
    private int maxAllowedMistakes = 3;

    public void MainMenu()
    {
        Console.Title = "Sudoku";
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Добро пожаловать в игру Судоку!");
            Console.WriteLine("1. Начать игру");
            Console.WriteLine("2. О разработчике");
            Console.WriteLine("3. Выйти из программы");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    StartGame();
                    break;
                case "2":
                    DeveloperInfo();
                    break;
                case "3":
                    ExitGame();
                    break;
                default:
                    Console.WriteLine("Неверный выбор. Попробуйте снова.");
                    break;
            }
        }
    }

    private void DeveloperInfo()
    {
        Console.Clear();
        Console.WriteLine("Разработчик: Ваше Имя");
        Console.WriteLine("Игра разработана для проекта по программированию.");
        Console.WriteLine("\nНажмите любую клавишу, чтобы вернуться в меню.");
        Console.ReadKey();
    }

    private void ExitGame()
    {
        Console.WriteLine("Спасибо за игру! До свидания.");
        Environment.Exit(0);
    }

    public void StartGame()
    {
        Console.Clear();
        Console.WriteLine("Выберите уровень сложности:");
        Console.WriteLine("1. Простая");
        Console.WriteLine("2. Средняя");
        Console.WriteLine("3. Сложная");

        string difficulty = Console.ReadLine();
        originalBoard = GenerateSudoku(difficulty); // Generate the Sudoku board based on difficulty
        currentBoard = (int[,])originalBoard.Clone(); // Clone original board for gameplay
        PlaySudoku(currentBoard); // Pass the current board to PlaySudoku
    }

    private int[,] GenerateSudoku(string difficulty)
    {
        int[,] board = GenerateFullSudoku();

        switch (difficulty)
        {
            case "1":
                Console.WriteLine("Выбрана простая сложность.");
                RemoveNumbersWithSolutionCheck(board, 10);
                break;
            case "2":
                Console.WriteLine("Выбрана средняя сложность.");
                RemoveNumbersWithSolutionCheck(board, 20);
                break;
            case "3":
                Console.WriteLine("Выбрана сложная сложность.");
                RemoveNumbersWithSolutionCheck(board, 30);
                break;
            default:
                Console.WriteLine("Неверный выбор сложности.");
                StartGame();
                break;
        }

        return board;
    }

    private int[,] GenerateFullSudoku()
    {
        int[,] board = new int[9, 9];
        FillBoard(board);
        return board;
    }

    private bool FillBoard(int[,] board)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (board[row, col] == 0)
                {
                    int[] numbers = ShuffleNumbers();

                    foreach (int num in numbers)
                    {
                        if (IsValidMove(board, row, col, num))
                        {
                            board[row, col] = num;

                            if (FillBoard(board))
                                return true;
                            else
                                board[row, col] = 0;
                        }
                    }

                    return false;
                }
            }
        }
        return true;
    }

    private int[] ShuffleNumbers()
    {
        int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        for (int i = 0; i < numbers.Length; i++)
        {
            int j = rand.Next(i, numbers.Length);
            int temp = numbers[i];
            numbers[i] = numbers[j];
            numbers[j] = temp;
        }
        return numbers;
    }

    private void RemoveNumbersWithSolutionCheck(int[,] board, int amountToRemove)
    {
        int removed = 0;
        while (removed < amountToRemove)
        {
            int row = rand.Next(0, 9);
            int col = rand.Next(0, 9);

            if (board[row, col] != 0)
            {
                int backup = board[row, col];
                board[row, col] = 0;

                // Проверяем, осталась ли у доски только одно решение
                if (HasUniqueSolution(board))
                {
                    removed++;
                }
                else
                {
                    board[row, col] = backup; // Восстанавливаем если нет уникального решения
                }
            }
        }
    }

    private bool HasUniqueSolution(int[,] board)
    {
        int[,] copy = (int[,])board.Clone();
        int solutions = 0;
        SolveSudoku(copy, ref solutions);
        return solutions == 1;
    }

    private bool SolveSudoku(int[,] board, ref int solutions, int row = 0, int col = 0)
    {
        if (solutions > 1) return false; // Если нашли более одного решения, выходим

        if (row == 9)
        {
            solutions++;
            return solutions == 1; // Возвращаем true только при первом решении
        }

        if (col == 9)
            return SolveSudoku(board, ref solutions, row + 1, 0);

        if (board[row, col] != 0)
            return SolveSudoku(board, ref solutions, row, col + 1);

        for (int num = 1; num <= 9; num++)
        {
            if (IsValidMove(board, row, col, num))
            {
                board[row, col] = num;
                if (SolveSudoku(board, ref solutions, row, col + 1))
                    return true; // Если найдено одно решение, продолжаем
                board[row, col] = 0;
            }
        }

        return false;
    }

    public void PlaySudoku(int[,] board)
    {
        mistakes = 0;
        Stopwatch timer = new Stopwatch();
        timer.Start();

        while (mistakes < 3)
        {
            Console.Clear();
            PrintBoard(board);

            Console.WriteLine("\nВведите номер строки (0-8):");
            int x = GetValidCoordinate();

            Console.WriteLine("Введите номер столбца (0-8):");
            int y = GetValidCoordinate();

            if (board[x, y] != 0)
            {
                Console.WriteLine("Эта клетка уже заполнена. Выберите другую клетку.");
                Console.ReadKey();
                continue;
            }

            Console.WriteLine("Введите цифру (1-9):");
            int num = GetValidNumber();

            if (IsValidMove(board, x, y, num))
            {
                board[x, y] = num;
                NotifyExtraMistake(board, x, y);
            }
            else
            {
                mistakes++;
                Console.WriteLine($"Ошибка! Осталось попыток: {3 - mistakes}");
                Console.ReadKey();
            }

            if (IsBoardCompleted(board))
            {
                timer.Stop();
                Console.Clear();
                PrintBoard(board);
                Console.WriteLine($"Поздравляем! Вы прошли игру за {timer.Elapsed.TotalSeconds} секунд с {mistakes} ошибками.");
                Console.ReadKey();
                MainMenu();
            }
        }

        Console.WriteLine("Конец игры! Вы допустили 3 ошибки.");
        timer.Stop();
        Console.WriteLine($"Время: {timer.Elapsed.TotalSeconds} секунд.");
        Console.ReadKey();
        MainMenu();
    }

    private int GetValidCoordinate()
    {
        int coordinate;
        while (true)
        {
            string input = Console.ReadLine();
            if (int.TryParse(input, out coordinate) && coordinate >= 0 && coordinate <= 8)
            {
                return coordinate;
            }
            else
            {
                Console.WriteLine("Неверный ввод. Введите число от 0 до 8:");
            }
        }
    }

    private void NotifyExtraMistake(int[,] board, int row, int col)
    {
        bool extraChanceGiven = false;

        if (IsRowFilled(board, row))
        {
            maxAllowedMistakes++;
            Console.WriteLine("Получена дополнительная попытка за заполнение строки!");
            extraChanceGiven = true;
        }

        if (IsColumnFilled(board, col))
        {
            maxAllowedMistakes++;
            Console.WriteLine("Получена дополнительная попытка за заполнение столбца!");
            extraChanceGiven = true;
        }

        if (IsBoxFilled(board, row - row % 3, col - col % 3))
        {
            maxAllowedMistakes++;
            Console.WriteLine("Получена дополнительная попытка за заполнение 3x3 квадрата!");
            extraChanceGiven = true;
        }

        if (extraChanceGiven)
        {
            Console.WriteLine($"Теперь у вас {maxAllowedMistakes - mistakes} оставшихся попыток.");
            Console.ReadKey();
        }
    }

    private bool IsRowFilled(int[,] board, int row)
    {
        for (int col = 0; col < 9; col++)
        {
            if (board[row, col] == 0) return false;
        }
        return true;
    }

    private bool IsColumnFilled(int[,] board, int col)
    {
        for (int row = 0; row < 9; row++)
        {
            if (board[row, col] == 0) return false;
        }
        return true;
    }

    private bool IsBoxFilled(int[,] board, int startRow, int startCol)
    {
        for (int i = startRow; i < startRow + 3; i++)
        {
            for (int j = startCol; j < startCol + 3; j++)
            {
                if (board[i, j] == 0) return false;
            }
        }
        return true;
    }

    private int GetValidNumber()
    {
        int number;
        while (true)
        {
            string input = Console.ReadLine();
            if (int.TryParse(input, out number) && number >= 1 && number <= 9)
            {
                return number;
            }
            else
            {
                Console.WriteLine("Неверный ввод. Введите число от 1 до 9:");
            }
        }
    }

    private void PrintBoard(int[,] board)
    {
        Console.WriteLine("  0 1 2  3 4 5  6 7 8");
        Console.WriteLine("  ---------------------");

        for (int i = 0; i < 9; i++)
        {
            if (i % 3 == 0 && i != 0)
                Console.WriteLine("  ---------------------");

            Console.Write(i + "|");

            for (int j = 0; j < 9; j++)
            {
                if (j % 3 == 0 && j != 0)
                    Console.Write("|");

                if (originalBoard[i, j] != 0)
                {
                    Console.Write(originalBoard[i, j] + " ");
                }
                else
                {
                    // Display player's correct answers in green
                    if (board[i, j] != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(board[i, j] + " ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write(". "); // Empty cell
                    }
                }
            }

            Console.WriteLine();
        }
        Console.ResetColor();
    }

    private bool IsValidMove(int[,] board, int row, int col, int num)
    {
        for (int x = 0; x < 9; x++)
        {
            if (board[row, x] == num || board[x, col] == num || board[row - row % 3 + x / 3, col - col % 3 + x % 3] == num)
                return false;
        }
        return true;
    }

    private bool IsBoardCompleted(int[,] board)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (board[row, col] == 0)
                    return false;
            }
        }
        return true;
    }
}
