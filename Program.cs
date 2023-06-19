// Made by Kobins with ChatGPT-4

using GitBlamedCodeExporter;
using LibGit2Sharp;

static void Log(string message)
{
    Console.WriteLine(message);
}



static void QueueBlame(string repoPath, string file, string fileName, string folderPath)
{
    Atomics.Increase();
    Log($"Queued {folderPath} - {fileName}.cs - now {Atomics.Count}");
    Task.Run(() =>
    {
        using (var repo = new Repository(repoPath))
        {
            string relativePath = Path.GetRelativePath(repo.Info.WorkingDirectory, file).Replace('\\', '/');
            var blameResult = repo.Blame(relativePath);
            string[] lines = File.ReadAllLines(file);
            int lineNumber = 1;

            // create a new text file with '_Blamed' appended to the name
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(folderPath, $"{fileName}_Blamed.cs")))
            {
                foreach (var hunk in blameResult)
                {
                    for (int i = 0; i < hunk.LineCount; i++)
                    {
                        string lineContent = lines[lineNumber - 1];
                        var signature = hunk.FinalSignature ?? hunk.InitialSignature;
                        if (signature == null)
                        {
                            outputFile.WriteLine($"/* {string.Empty,24}                          {lineNumber,4} */ {lineContent}");
                        }
                        else
                        {
                            outputFile.WriteLine($"/* {signature.Name,24}, {signature.When:yyyy-MM-dd hh:mm:ss z}, {lineNumber,4} */ {lineContent}");
                        }
                        lineNumber++;
                    }
                }
            }
            Atomics.Decrease();
            Log($"Completed {folderPath} - {fileName}.cs - {Atomics.Count} left");
        }
    });
}

static void PrintAllTextFilesAndGitBlame(string folderPath)
{
    foreach (string file in Directory.GetFiles(folderPath))
    {
        if (Path.GetExtension(file) != ".cs")
        {
            continue;
        }

        var fileName = Path.GetFileNameWithoutExtension(file);
        if (fileName.Contains("_Blamed"))
        {
            continue;
        }

        string repoPath = Repository.Discover(folderPath);
        if (repoPath == null)
        {
            continue;
        }

        QueueBlame(repoPath, file, fileName, folderPath);

    }

    foreach (string dir in Directory.GetDirectories(folderPath))
    {
        PrintAllTextFilesAndGitBlame(dir);
    }
}

while (true)
{
    Log("추출하려는 root 폴더 경로 입력: ");
    var rawPath = Console.ReadLine();
    if (rawPath == null)
    {
        Log("rawPath == null");
        continue;
    }
    if (Directory.Exists(rawPath))
    {
        PrintAllTextFilesAndGitBlame(rawPath);
    }
    else
    {
        Console.WriteLine("올바른 폴더 경로를 입력하세요.");
    }
}