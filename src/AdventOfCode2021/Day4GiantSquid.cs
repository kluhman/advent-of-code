using System.Text;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2021;

public class Day4GiantSquid : IChallenge
{
    public int ChallengeId => 4;

    public object SolvePart1(string input)
    {
        var lines = input.GetLines();
        var numbers = lines.First().Split(',').Select(int.Parse).ToArray();
        var boards = ParseBoards(lines).ToList();

        for (var index = 0; index < numbers.Length; index++)
        {
            var currentNumber = numbers[index];
            foreach (var board in boards)
            {
                board.MarkNumber(currentNumber);
                if (index >= 4 && board.CheckIfWinner())
                {
                    return board.CalculateScore(currentNumber);
                }
            }
        }

        throw new Exception("No board was a winner");
    }

    public object SolvePart2(string input)
    {
        var lines = input.GetLines();
        var numbers = lines.First().Split(',').Select(int.Parse).ToArray();
        var boards = ParseBoards(lines).ToList();

        for (var index = 0; index < numbers.Length; index++)
        {
            var currentNumber = numbers[index];
            foreach (var board in boards.ToList())
            {
                board.MarkNumber(currentNumber);
                if (index < 4 || !board.CheckIfWinner())
                {
                    continue;
                }

                if (boards.Count == 1)
                {
                    return board.CalculateScore(currentNumber);
                }

                boards.Remove(board);
            }
        }

        throw new Exception("No board was a winner");
    }

    private static IEnumerable<BingoBoard> ParseBoards(string[] lines)
    {
        for (var index = 1; index < lines.Length; index += BingoBoard.BoardDimensions)
        {
            var lastRowIndex = index + BingoBoard.BoardDimensions;
            var boardRows = lines[index..lastRowIndex];

            yield return BingoBoard.ParseBoard(boardRows);
        }
    }

    public class BingoBoard
    {
        public const int BoardDimensions = 5;

        public BingoBoard(BingoSpot[,] spots)
        {
            Spots = spots;
        }

        public BingoSpot[,] Spots { get; }

        public void MarkNumber(int number)
        {
            for (var row = 0; row < BoardDimensions; row++)
            {
                for (var column = 0; column < BoardDimensions; column++)
                {
                    var spot = Spots[row, column];
                    if (spot.Value != number)
                    {
                        continue;
                    }

                    spot.MarkSpot();
                    return;
                }
            }
        }

        public bool CheckIfWinner()
        {
            for (var row = 0; row < BoardDimensions; row++)
            {
                var isWinner = true;
                for (var column = 0; column < BoardDimensions; column++)
                {
                    if (Spots[row, column].HasBeenCalled)
                    {
                        continue;
                    }

                    isWinner = false;
                    break;
                }

                if (isWinner)
                {
                    return true;
                }
            }

            for (var column = 0; column < BoardDimensions; column++)
            {
                var isWinner = true;
                for (var row = 0; row < BoardDimensions; row++)
                {
                    if (Spots[row, column].HasBeenCalled)
                    {
                        continue;
                    }

                    isWinner = false;
                    break;
                }

                if (isWinner)
                {
                    return true;
                }
            }

            return false;
        }

        public int CalculateScore(int winningNumber)
        {
            var sum = 0;
            for (var row = 0; row < BoardDimensions; row++)
            {
                for (var column = 0; column < BoardDimensions; column++)
                {
                    var spot = Spots[row, column];
                    if (!spot.HasBeenCalled)
                    {
                        sum += spot.Value;
                    }
                }
            }

            return sum * winningNumber;
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            for (var row = 0; row < BoardDimensions; row++)
            {
                for (var column = 0; column < BoardDimensions; column++)
                {
                    result.Append(Spots[row, column].ToString().PadLeft(3));
                }

                result.AppendLine();
            }

            return result.ToString();
        }

        public static BingoBoard ParseBoard(string[] rows)
        {
            var spots = new BingoSpot[BoardDimensions, BoardDimensions];
            for (var row = 0; row < BoardDimensions; row++)
            {
                var values = rows[row].Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                for (var column = 0; column < BoardDimensions; column++)
                {
                    spots[row, column] = new BingoSpot(int.Parse(values[column]));
                }
            }

            return new BingoBoard(spots);
        }
    }

    public class BingoSpot
    {
        public BingoSpot(int value)
        {
            Value = value;
        }

        public int Value { get; }
        public bool HasBeenCalled { get; private set; }

        public void MarkSpot() => HasBeenCalled = true;

        public override string ToString() => Value.ToString();
    }
}
