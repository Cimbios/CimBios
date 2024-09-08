﻿using System.Threading.Tasks;
using Avalonia.Controls;
using CimBios.Tools.ModelDebug.Views;
using CommunityToolkit.Mvvm.Input;

namespace CimBios.Tools.ModelDebug.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public AsyncRelayCommand ShowDataProviderSelectorCommand { get; }

    public Avalonia.Visual OwnerView { get; }

    public MainWindowViewModel(Avalonia.Visual ownerView)
    {
        OwnerView = ownerView;

        ShowDataProviderSelectorCommand = 
            new AsyncRelayCommand(ShowDataProviderSelectorWindow);
    }

    private Task ShowDataProviderSelectorWindow()
    {
        if (OwnerView is Window ownerWindow == false)
        {
            return Task.CompletedTask;
        }

        var dpsWindow = new DataSelectorWindow();
        return dpsWindow.ShowDialog(ownerWindow);
    }
}
