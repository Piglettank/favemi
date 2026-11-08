using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Favemi.Services;
using Microsoft.Win32;

namespace Favemi;

public partial class MainWindow : Window
{
    private readonly FolderQueue _queue = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnOpenFolder(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Choose a folder of images"
        };

        if (dialog.ShowDialog(this) == true)
        {
            OpenFolder(dialog.FolderName);
        }
    }

    private void OnPrevious(object sender, RoutedEventArgs e)
    {
        if (_queue.GoPrevious())
        {
            ShowCurrent();
        }
    }

    private void OnNext(object sender, RoutedEventArgs e)
    {
        if (_queue.GoNext())
        {
            ShowCurrent();
        }
    }

    private void OnFavorite(object sender, RoutedEventArgs e)
    {
        try
        {
            _queue.FavoriteCurrent();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "favemi", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        UpdateChrome();
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
        {
            OnOpenFolder(sender, e);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Left)
        {
            OnPrevious(sender, e);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Right)
        {
            OnNext(sender, e);
            e.Handled = true;
        }
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = DroppedFolder(e) is null ? DragDropEffects.None : DragDropEffects.Copy;
        e.Handled = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        var folder = DroppedFolder(e);
        if (folder is not null)
        {
            OpenFolder(folder);
        }

        e.Handled = true;
    }

    private void OpenFolder(string folder)
    {
        try
        {
            _queue.Open(folder);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "favemi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ShowCurrent();
    }

    private void ShowCurrent()
    {
        Viewer.Source = null;

        if (_queue.CurrentPath is null)
        {
            Placeholder.Text = _queue.Folder is null
                ? "Open a folder to look through its images"
                : "There are no images in this folder";
            Placeholder.Visibility = Visibility.Visible;
            UpdateChrome();
            return;
        }

        try
        {
            using var stream = File.OpenRead(_queue.CurrentPath);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            Viewer.Source = bitmap;
            Placeholder.Visibility = Visibility.Collapsed;
        }
        catch (Exception)
        {
            Placeholder.Text = $"Could not show {_queue.CurrentName}";
            Placeholder.Visibility = Visibility.Visible;
        }

        UpdateChrome();
    }

    private void UpdateChrome()
    {
        PreviousButton.IsEnabled = _queue.CanGoPrevious;
        NextButton.IsEnabled = _queue.CanGoNext;

        var hasCurrent = _queue.CurrentPath is not null;
        var favorited = _queue.IsCurrentFavorited();
        FavoriteButton.IsEnabled = hasCurrent && !favorited;
        FavoriteButton.Content = favorited ? "♥ Favorited" : "Favorite";

        if (_queue.Folder is null)
        {
            Status.Text = string.Empty;
            return;
        }

        if (_queue.IsEmpty)
        {
            Status.Text = _queue.Folder;
            return;
        }

        Status.Text = $"{_queue.Index + 1} / {_queue.Count}    {_queue.CurrentName}";
    }

    private static string? DroppedFolder(DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return null;
        }

        if (e.Data.GetData(DataFormats.FileDrop) is not string[] paths)
        {
            return null;
        }

        foreach (var path in paths)
        {
            if (Directory.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
}
