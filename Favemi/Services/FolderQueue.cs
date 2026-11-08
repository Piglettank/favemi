using System;
using System.Collections.Generic;
using System.IO;

namespace Favemi.Services;

internal sealed class FolderQueue
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff", ".webp"
    };

    private readonly List<string> _files = new();

    public string? Folder { get; private set; }

    public int Index { get; private set; } = -1;

    public int Count => _files.Count;

    public bool IsEmpty => _files.Count == 0;

    public string? CurrentPath => Index >= 0 && Index < _files.Count ? _files[Index] : null;

    public string? CurrentName => CurrentPath is null ? null : Path.GetFileName(CurrentPath);

    public string? FavoritesFolder => Folder is null ? null : Path.Combine(Folder, "Favorites");

    public bool CanGoPrevious => Index > 0;

    public bool CanGoNext => Index >= 0 && Index < _files.Count - 1;

    public void Open(string folder)
    {
        Folder = Path.GetFullPath(folder);
        _files.Clear();

        foreach (var path in Directory.EnumerateFiles(Folder))
        {
            if (Extensions.Contains(Path.GetExtension(path)))
            {
                _files.Add(path);
            }
        }

        _files.Sort(CompareNatural);
        Index = _files.Count == 0 ? -1 : 0;
    }

    public bool GoPrevious()
    {
        if (!CanGoPrevious)
        {
            return false;
        }

        Index--;
        return true;
    }

    public bool GoNext()
    {
        if (!CanGoNext)
        {
            return false;
        }

        Index++;
        return true;
    }

    public bool IsCurrentFavorited()
    {
        if (CurrentName is null || FavoritesFolder is null)
        {
            return false;
        }

        return File.Exists(Path.Combine(FavoritesFolder, CurrentName));
    }

    public bool FavoriteCurrent()
    {
        if (CurrentPath is null || FavoritesFolder is null || CurrentName is null)
        {
            return false;
        }

        if (IsCurrentFavorited())
        {
            return false;
        }

        Directory.CreateDirectory(FavoritesFolder);
        File.Copy(CurrentPath, Path.Combine(FavoritesFolder, CurrentName));
        return true;
    }

    private static int CompareNatural(string leftPath, string rightPath)
    {
        return NaturalCompare(Path.GetFileName(leftPath), Path.GetFileName(rightPath));
    }

    private static int NaturalCompare(string left, string right)
    {
        var i = 0;
        var j = 0;
        while (i < left.Length && j < right.Length)
        {
            if (char.IsDigit(left[i]) && char.IsDigit(right[j]))
            {
                var i0 = i;
                var j0 = j;
                while (i < left.Length && char.IsDigit(left[i]))
                {
                    i++;
                }

                while (j < right.Length && char.IsDigit(right[j]))
                {
                    j++;
                }

                var compared = CompareNumber(left.AsSpan(i0, i - i0), right.AsSpan(j0, j - j0));
                if (compared != 0)
                {
                    return compared;
                }
            }
            else
            {
                var compared = char.ToUpperInvariant(left[i]).CompareTo(char.ToUpperInvariant(right[j]));
                if (compared != 0)
                {
                    return compared;
                }

                i++;
                j++;
            }
        }

        return (left.Length - i).CompareTo(right.Length - j);
    }

    private static int CompareNumber(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        left = left.TrimStart('0');
        right = right.TrimStart('0');
        if (left.Length != right.Length)
        {
            return left.Length.CompareTo(right.Length);
        }

        return left.CompareTo(right, StringComparison.Ordinal);
    }
}
