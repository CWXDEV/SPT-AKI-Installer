﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Serilog;
using SPTInstaller.Models;

namespace SPTInstaller.Helpers;

public static class FileHelper
{
    private static Result IterateDirectories(DirectoryInfo sourceDir, DirectoryInfo targetDir, string[] exclusions)
    {
        try
        {
            foreach (var dir in sourceDir.GetDirectories("*", SearchOption.AllDirectories))
            {
                var exclude = false;

                foreach (var exclusion in exclusions)
                {
                    var currentDirRelativePath = dir.FullName.Replace(sourceDir.FullName, "");

                    if (currentDirRelativePath.StartsWith(exclusion) || currentDirRelativePath == exclusion)
                    {
                        exclude = true;
                        break;
                    }
                }

                if (exclude)
                    continue;

                Directory.CreateDirectory(dir.FullName.Replace(sourceDir.FullName, targetDir.FullName));
            }
            return Result.FromSuccess();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while creating directories");
            return Result.FromError(ex.Message);
        }
    }

    private static Result IterateFiles(DirectoryInfo sourceDir, DirectoryInfo targetDir, string[] exclusions, Action<string, int> updateCallback = null)
    {
        try
        {
            int totalFiles = sourceDir.GetFiles("*.*", SearchOption.AllDirectories).Length;
            int processedFiles = 0;

            foreach (var file in sourceDir.GetFiles("*.*", SearchOption.AllDirectories))
            {
                var exclude = false;

                updateCallback?.Invoke(file.Name, (int)Math.Floor(((double)processedFiles / totalFiles) * 100));

                foreach (var exclusion in exclusions)
                {
                    var currentFileRelativePath = file.FullName.Replace(sourceDir.FullName, "");

                    if (currentFileRelativePath.StartsWith(exclusion) || currentFileRelativePath == exclusion)
                    {
                        exclude = true;
                        break;
                    }
                }

                if (exclude)
                    continue;

                File.Copy(file.FullName, file.FullName.Replace(sourceDir.FullName, targetDir.FullName), true);
                processedFiles++;
            }

            return Result.FromSuccess();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while copying files");
            return Result.FromError(ex.Message);
        }
    }

    public static string GetRedactedPath(string path)
    {
        var nameMatched = Regex.Match(path, @".:\\[uU]sers\\(?<NAME>[^\\]+)");

        if (nameMatched.Success)
        {
            var name = nameMatched.Groups["NAME"].Value;
            return path.Replace(name, "-REDACTED-");
        }

        return path;
    }

    public static Result CopyDirectoryWithProgress(DirectoryInfo sourceDir, DirectoryInfo targetDir, IProgress<double> progress = null, string[] exclusions = null) =>
        CopyDirectoryWithProgress(sourceDir, targetDir, (msg, prog) => progress?.Report(prog), exclusions);

    public static Result CopyDirectoryWithProgress(DirectoryInfo sourceDir, DirectoryInfo targetDir, Action<string, int> updateCallback = null, string[] exclusions = null)
    {
        try
        {
            var iterateDirectoriesResult = IterateDirectories(sourceDir, targetDir, exclusions ??= new string[0]);

            if(!iterateDirectoriesResult.Succeeded) return iterateDirectoriesResult;

            var iterateFilesResult = IterateFiles(sourceDir, targetDir, exclusions ??= new string[0], updateCallback);

            if (!iterateFilesResult.Succeeded) return iterateDirectoriesResult;

            return Result.FromSuccess();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during directory copy");
            return Result.FromError(ex.Message);
        }
    }

    public static void StreamAssemblyResourceOut(string resourceName, string outputFilePath)
    {
        var assembly = Assembly.GetExecutingAssembly();

        FileInfo outputFile = new FileInfo(outputFilePath);

        if (outputFile.Exists)
        {
            outputFile.Delete();
        }

        if (!outputFile.Directory.Exists)
        {
            Directory.CreateDirectory(outputFile.Directory.FullName);
        }

        var resName = assembly.GetManifestResourceNames().First(x => x.EndsWith(resourceName));

        using (FileStream fs = File.Create(outputFilePath))
        using (Stream s = assembly.GetManifestResourceStream(resName))
        {
            s.CopyTo(fs);
        }
    }

    private enum PathCheckType
    {
        EndsWith = 0,
        Contains = 1,
    }

    public static bool CheckPathForProblemLocations(string path, out string detectedName)
    {
        detectedName = "";

        var problemNames = new Dictionary<string, PathCheckType>()
        {
            { "Desktop", PathCheckType.EndsWith },
            { "Downloads", PathCheckType.EndsWith },
            { "OneDrive", PathCheckType.Contains },
            { "NextCloud", PathCheckType.Contains },
            { "DropBox", PathCheckType.Contains },
            { "Google", PathCheckType.Contains },
        };

        foreach (var name in  problemNames)
        {
            switch (name.Value)
            {
                case PathCheckType.EndsWith:
                    if (path.ToLower().EndsWith(name.Key.ToLower()))
                    {
                        detectedName = name.Key;
                        return true;
                    }
                    break;
                case PathCheckType.Contains:
                    if (path.ToLower().Contains(name.Key.ToLower()))
                    {
                        detectedName = name.Key;
                        return true;
                    }
                    break;
                default:
                    break;
            }
        }

        return false;
    }

}