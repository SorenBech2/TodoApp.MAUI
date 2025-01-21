using TodoApp.MAUI.Models;
using TodoApp.MAUI.ViewModels;

namespace TodoApp.MAUI;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage()
    {
        InitializeComponent();
        this._viewModel = App.GetRequiredService<MainViewModel>();
        BindingContext = this._viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        this._viewModel.RefreshItemsCommand.Execute(null);
    }

    private void OnListItemTapped(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is TodoItem item)
        {
            this._viewModel.UpdateItemCommand.Execute(item.Id);
        }

        if (sender is CollectionView itemList)
        {
            itemList.SelectedItem = null;
        }
    }
}

