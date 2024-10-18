using System;
using System.Diagnostics;
using static Game;

public interface IGame
{
    void StartGame();
    void PlaySudoku(int[,] board);
    void MainMenu();
}

public interface IBoardGenerator
{
    int[,] GenerateSudoku(string difficulty);
    int[,] GenerateFullSudoku();
    bool IsValidMove(int[,] board, int row, int col, int num); // Добавлен метод
    bool IsRowFilled(int[,] board, int row); // Добавлен метод
    bool IsColumnFilled(int[,] board, int col); // Добавлен метод
    bool IsBoxFilled(int[,] board, int startRow, int startCol); // Добавлен метод
}

public class Program
{
    static void Main(string[] args)
    {
        IBoardGenerator boardGenerator = new BoardGenerator();
        IGame game = new Game(boardGenerator);
        game.MainMenu();
    }
}


public class Game : IGame
{
    private string difficulty;
    private static Random rand = new Random();
    private int[,] originalBoard;
    private int[,] currentBoard;
    private int mistakes;
    private int maxAllowedMistakes = 3;

    private readonly IBoardGenerator _boardGenerator;

    public Game(IBoardGenerator boardGenerator)
    {
        _boardGenerator = boardGenerator;
    }

    public void MainMenu()
    {
        Console.Title = "Sudoku";
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Добро пожаловать в игру Судоку!");
            Console.WriteLine("1. Начать игру");
            Console.WriteLine("2. О разработчике");
            Console.WriteLine("3. Топ 3 результата");
            Console.WriteLine("4. Выйти из программы");

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
                    DisplayTop3Results();
                    break;
                case "4":
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

        string choice = Console.ReadLine();

        difficulty = choice switch
        {
            "1" => "Простая",
            "2" => "Средняя",
            "3" => "Сложная",
            _ => throw new ArgumentException("Неверный выбор сложности.")
        };

        originalBoard = _boardGenerator.GenerateSudoku(choice);
        currentBoard = (int[,])originalBoard.Clone();
        PlaySudoku(currentBoard);
    }

    public class BoardGenerator : IBoardGenerator
    {
        private Random rand = new Random();

        public int[,] GenerateSudoku(string difficulty)
        {
            int[,] board = GenerateFullSudoku();
            int removeCount = difficulty switch
            {
                "1" => 10,
                "2" => 20,
                "3" => 30,
                _ => throw new ArgumentException("Неверный выбор сложности.")
            };

            RemoveNumbersWithSolutionCheck(board, removeCount);
            return board;
        }

        public int[,] GenerateFullSudoku()
        {
            int[,] board = new int[9, 9];
            FillBoard(board);
            return board;
        }

        public bool IsValidMove(int[,] board, int row, int col, int num)
        {
            for (int x = 0; x < 9; x++)
            {
                if (board[row, x] == num || board[x, col] == num ||
                    board[row - row % 3 + x / 3, col - col % 3 + x % 3] == num)
                    return false;
            }
            return true;
        }

        public bool IsRowFilled(int[,] board, int row)
        {
            for (int col = 0; col < 9; col++)
                if (board[row, col] == 0)
                    return false;
            return true;
        }

        public bool IsColumnFilled(int[,] board, int col)
        {
            for (int row = 0; row < 9; row++)
                if (board[row, col] == 0)
                    return false;
            return true;
        }

        public bool IsBoxFilled(int[,] board, int startRow, int startCol)
        {
            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 3; col++)
                    if (board[startRow + row, startCol + col] == 0)
                        return false;
            return true;
        }

        public bool FillBoard(int[,] board)
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
                                if (FillBoard(board))  // Рекурсивный вызов
                                    return true;

                                board[row, col] = 0;  // Откат изменений
                            }
                        }
                        return false;  // Если не удалось заполнить текущую ячейку
                    }
                }
            }
            return true;  // Если вся доска заполнена
        }

        public void RemoveNumbersWithSolutionCheck(int[,] board, int amountToRemove)
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

                    if (HasUniqueSolution(board))
                        removed++;
                    else
                        board[row, col] = backup;
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
            if (solutions > 1) return false;
            if (row == 9)
            {
                solutions++;
                return solutions == 1;
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
                        return true;
                    board[row, col] = 0;
                }
            }

            return false;
        }

        private int[] ShuffleNumbers()
        {
            int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            for (int i = 0; i < numbers.Length; i++)
            {
                int j = rand.Next(i, numbers.Length);
                (numbers[i], numbers[j]) = (numbers[j], numbers[i]);
            }
            return numbers;
        }
    }


    public void PlaySudoku(int[,] board)
    {
        mistakes = 0;
        Stopwatch timer = new Stopwatch();
        timer.Start();

        while (mistakes < maxAllowedMistakes)
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

            if (_boardGenerator.IsValidMove(board, x, y, num))
            {
                board[x, y] = num;
                NotifyExtraMistake(board, x, y);
            }
            else
            {
                mistakes++;
                Console.WriteLine($"Ошибка! Осталось попыток: {maxAllowedMistakes - mistakes}");
                Console.ReadKey();
            }

            if (IsBoardCompleted(board))
            {
                timer.Stop();
                Console.Clear();
                PrintBoard(board);

                TimeSpan timeSpan = timer.Elapsed;
                string formattedTime = $"{(int)timeSpan.TotalMinutes} мин {timeSpan.Seconds} сек";

                Console.WriteLine($"Поздравляем! Вы прошли игру за {formattedTime} с {mistakes} ошибками.");
                SaveResult(difficulty, formattedTime); // Передаем сложность
                Console.ReadKey();
                MainMenu();
            }
        }

        Console.WriteLine($"Конец игры! Вы допустили {maxAllowedMistakes} ошибки.");
        timer.Stop();
        TimeSpan totalTime = timer.Elapsed;
        string totalFormattedTime = string.Format("{0} мин {1} сек", (int)totalTime.TotalMinutes, totalTime.Seconds);
        Console.WriteLine($"Время: {totalFormattedTime}.");
        Console.ReadKey();
        MainMenu();
    }

    private void SaveResult(string difficulty, string time)
    {
        string result = $"{difficulty} | {time} | {DateTime.Now}";
        File.AppendAllText("results.txt", result + Environment.NewLine);
        Console.WriteLine("Результат сохранен.");
    }

    private void DisplayTop3Results()
    {
        if (!File.Exists("results.txt"))
        {
            Console.WriteLine("Нет данных о прохождениях.");
            Console.WriteLine("\nНажмите любую клавишу для возврата в меню.");
            Console.ReadKey();
            return;
        }

        var results = File.ReadAllLines("results.txt")
            .Select(line => line.Split('|').Select(part => part.Trim()).ToArray())
            .Where(parts => parts.Length == 3) // Проверка на корректность данных
            .Select(parts => new
            {
                Difficulty = parts[0],
                Time = ParseTime(parts[1]),
                Date = parts[2]
            })
            .GroupBy(r => r.Difficulty)
            .Select(g => new { Difficulty = g.Key, TopResults = g.OrderBy(r => r.Time).Take(3) });

        foreach (var difficultyGroup in results)
        {
            Console.WriteLine($"\nТоп-3 прохождений для сложности: {difficultyGroup.Difficulty}");
            foreach (var result in difficultyGroup.TopResults)
            {
                Console.WriteLine($"Время: {result.Time.Minutes} мин {result.Time.Seconds} сек | Дата: {result.Date}");
            }
        }

        Console.WriteLine("\nНажмите любую клавишу для возврата в меню.");
        Console.ReadKey();
    }

    private TimeSpan ParseTime(string timeString)
    {
        // Ожидается формат "X мин Y сек"
        var parts = timeString.Split(' ');
        if (parts.Length >= 4 &&
            int.TryParse(parts[0], out int minutes) &&
            int.TryParse(parts[2], out int seconds))
        {
            return new TimeSpan(0, minutes, seconds);
        }
        return TimeSpan.Zero; // Возвращаем 0, если формат неправильный
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

        if (_boardGenerator.IsRowFilled(board, row))
        {
            maxAllowedMistakes++;
            Console.WriteLine("Получена дополнительная попытка за заполнение строки!");
            extraChanceGiven = true;
        }

        if (_boardGenerator.IsColumnFilled(board, col))
        {
            maxAllowedMistakes++;
            Console.WriteLine("Получена дополнительная попытка за заполнение столбца!");
            extraChanceGiven = true;
        }

        if (_boardGenerator.IsBoxFilled(board, row - row % 3, col - col % 3))
        {
            maxAllowedMistakes++;
            Console.WriteLine("Получена дополнительная попытка за заполнение квадрата 3x3!");
            extraChanceGiven = true;
        }

        if (extraChanceGiven)
        {
            Console.WriteLine($"Теперь у вас {maxAllowedMistakes - mistakes} оставшихся попыток.");
            Console.ReadKey();
        }
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