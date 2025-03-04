// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Datasync.Client;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TodoApp.MAUI.Models;
using TodoApp.MAUI.Services;

namespace TodoApp.MAUI.ViewModels;

public partial class MainViewModel(AppDbContext context, IAlertService alertService) : ObservableRecipient
{
    [ObservableProperty]
    private bool isRefreshing = false;

    [ObservableProperty]
    private ConcurrentObservableCollection<TodoItem> items = [];

    [RelayCommand]
    public async Task RefreshItemsAsync(CancellationToken cancellationToken = default)
    {
        if (IsRefreshing)
        {
            return;
        }

        try
        {
            await context.SynchronizeAsync(cancellationToken);
            List<TodoItem> items = await context.TodoItems.ToListAsync(cancellationToken);
            Items.ReplaceAll(items);
        }
        catch (Exception ex)
        {
            await alertService.ShowErrorAlertAsync("RefreshItems", ex.Message);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task UpdateItemAsync(string itemId, CancellationToken cancellationToken = default)
    {
        try
        {
            TodoItem? item = await context.TodoItems.FindAsync([itemId]);
            if (item is not null)
            {
                Debug.WriteLine($"item UpdatedAt is (before writing changes to SQLite) : {item.UpdatedAt}");
                Debug.WriteLine($"item IsComplete is (before writing changes to SQLite) : {item.IsComplete}");

                item.IsComplete = !item.IsComplete;
                _ = context.TodoItems.Update(item);
                _ = Items.ReplaceIf(x => x.Id == itemId, item);
                _ = await context.SaveChangesAsync(cancellationToken);
            }
            TodoItem? itemAfter = await context.TodoItems.FindAsync([itemId]);
            if (itemAfter is not null)
            {
                Debug.WriteLine($"item UpdatedAt is (after writing changes to SQLite) : {itemAfter.UpdatedAt.ToString()}");
                Debug.WriteLine($"item IsComplete is (after writing changes to SQLite) : {itemAfter.IsComplete}");
            }
        }
        catch (Exception ex)
        {
            await alertService.ShowErrorAlertAsync("UpdateItem", ex.Message);
        }
    }

    [RelayCommand]
    public async Task AddItemAsync(object returnValue, CancellationToken cancellationToken = default)
    {
        try
        {
            string text = ((Entry)returnValue).Text;
            TodoItem item = new() { Title = text };
            _ = context.TodoItems.Add(item);
            _ = await context.SaveChangesAsync(cancellationToken);
            Items.Add(item);
        }
        catch (Exception ex)
        {
            await alertService.ShowErrorAlertAsync("AddItem", ex.Message);
        }
    }
}
