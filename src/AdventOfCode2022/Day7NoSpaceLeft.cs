using System.Text.RegularExpressions;
using AdventOfCode.Core;
using AdventOfCode.Core.Extensions;

namespace AdventOfCode2022;

internal class Day7NoSpaceLeft : IChallenge
{
    public int ChallengeId => 7;

    public object SolvePart1(string input)
    {
        var fileSystem = ParseFileSystem(input);

        var archivableSubdirectories = FindArchivableDirectories(fileSystem.RootDirectory);
        return archivableSubdirectories.Aggregate(0, (sum, directory) => sum + directory.GetSize());
    }

    public object SolvePart2(string input)
    {
        const int requiredDiscSpace = 30000000;

        var fileSystem = ParseFileSystem(input);
        var spaceToClear = requiredDiscSpace - fileSystem.FreeDiscSpace;

        var directoryToDelete = FindSmallestArchivableDirectory(fileSystem.RootDirectory, spaceToClear)!;
        return directoryToDelete.GetSize();
    }

    private static FileSystem ParseFileSystem(string input)
    {
        var fileSystem = new FileSystem();
        var lines = input.GetLines();

        foreach (var line in lines)
        {
            CheckForCommand(fileSystem, line);
            CheckForDirectory(fileSystem, line);
            CheckForFile(fileSystem, line);
        }

        return fileSystem;
    }

    private static void CheckForCommand(FileSystem fileSystem, string line)
    {
        var match = Regex.Match(line, @"\$ (?<command>(cd|ls))( (?<args>.*))?");
        if (!match.Success)
        {
            return;
        }

        var command = match.Groups["command"].Value;
        switch (command)
        {
            case "ls":
                break;
            case "cd" when match.Groups["args"].Value == "/":
                fileSystem.GoToRoot();
                break;
            case "cd" when match.Groups["args"].Value == "..":
                fileSystem.GoUp();
                break;
            case "cd":
                fileSystem.OpenDirectory(match.Groups["args"].Value);
                break;
        }
    }

    private static void CheckForDirectory(FileSystem fileSystem, string line)
    {
        var match = Regex.Match(line, @"dir (?<directory>.+)");
        if (!match.Success)
        {
            return;
        }

        var subdirectory = new Directory(match.Groups["directory"].Value);
        fileSystem.CurrentDirectory.AddSubdirectory(subdirectory);
    }

    private static void CheckForFile(FileSystem fileSystem, string line)
    {
        var match = Regex.Match(line, @"(?<size>\d+) (?<file>.+)");
        if (!match.Success)
        {
            return;
        }

        var file = new File(match.Groups["file"].Value, int.Parse(match.Groups["size"].Value));
        fileSystem.CurrentDirectory.AddFile(file);
    }

    private static IEnumerable<Directory> FindArchivableDirectories(Directory directory)
    {
        const int maxSize = 100000;
        if (directory.GetSize() <= maxSize)
        {
            yield return directory;
        }

        var archivableSubdirectories = directory.Subdirectories.SelectMany(FindArchivableDirectories);
        foreach (var subdirectory in archivableSubdirectories)
        {
            yield return subdirectory;
        }
    }

    private static Directory? FindSmallestArchivableDirectory(Directory directory, int requiredSpace)
    {
        if (directory.GetSize() < requiredSpace)
        {
            return null;
        }

        var candidate = default(Directory);
        foreach (var subdirectory in directory.Subdirectories)
        {
            var newCandidate = FindSmallestArchivableDirectory(subdirectory, requiredSpace);
            if (newCandidate is null)
            {
                continue;
            }

            if (candidate is null || newCandidate.GetSize() < candidate.GetSize())
            {
                candidate = newCandidate;
            }
        }

        return candidate ?? directory;
    }

    public class FileSystem
    {
        private readonly Stack<Directory> _nav;

        public FileSystem()
        {
            RootDirectory = new Directory("/");
            _nav = new Stack<Directory>(new[] { RootDirectory });
        }

        public Directory RootDirectory { get; }
        public Directory CurrentDirectory => _nav.Peek();
        public static int DiscCapacity => 70000000;
        public int FreeDiscSpace => DiscCapacity - RootDirectory.GetSize();

        public void GoToRoot()
        {
            while (_nav.Count > 1)
            {
                _nav.Pop();
            }
        }

        public void OpenDirectory(string name)
        {
            var subdirectory = CurrentDirectory.Subdirectories.Single(x => x.Name == name);
            _nav.Push(subdirectory);
        }

        public void GoUp() => _nav.Pop();
    }

    public class Directory
    {
        private readonly List<File> _files = new();
        private readonly List<Directory> _subdirectories = new();

        public Directory(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public IReadOnlyList<File> Files => _files;
        public IReadOnlyList<Directory> Subdirectories => _subdirectories;

        public void AddFile(File file) => _files.Add(file);

        public void AddSubdirectory(Directory directory) => _subdirectories.Add(directory);

        public int GetSize()
        {
            var fileSize = Files.Aggregate(0, (sum, file) => sum + file.Size);
            var subdirectorySize = Subdirectories.Aggregate(0, (sum, directory) => sum + directory.GetSize());

            return fileSize + subdirectorySize;
        }

        public override string ToString() => Name;
    }

    public class File
    {
        public File(string name, int size)
        {
            Name = name;
            Size = size;
        }

        public string Name { get; }
        public int Size { get; }

        public override string ToString() => $"{Name} ({Size})";
    }
}
