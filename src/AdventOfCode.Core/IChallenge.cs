using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Core
{
    public interface IChallenge
    {
        int ChallengeId { get; }
        object SolvePart1(string input);
        object SolvePart2(string input);
    }
}
