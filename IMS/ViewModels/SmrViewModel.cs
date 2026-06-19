using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IMS.Models;
using IMS.Models.Enums;
using IMS.Services;

namespace IMS.ViewModels;

public partial class SmrViewModel : ObservableObject
{
    private readonly IDataStoreService _dataStore;
    private readonly IAuthService _authService;

    public SmrViewModel(IDataStoreService dataStore, IAuthService authService)
    {
        _dataStore = dataStore;
        _authService = authService;
        Refresh();
    }

    [ObservableProperty]
    private List<SmrServiceTicket> _tickets = new();

    [ObservableProperty]
    private List<SmrServiceTicket> _openTickets = new();

    [ObservableProperty]
    private SmrServiceTicket? _selectedTicket;

    [ObservableProperty]
    private List<SmrLaborLineItem> _laborLines = new();

    [ObservableProperty]
    private List<SmrPartsUsageLedger> _partsUsage = new();

    [ObservableProperty]
    private bool _isTicketSelected;

    [ObservableProperty]
    private decimal _totalTicketCost;

    [RelayCommand]
    private void Refresh()
    {
        Tickets = _dataStore.GetAll<SmrServiceTicket>()
            .OrderByDescending(t => t.CreatedAt)
            .ToList();
        OpenTickets = _dataStore.GetOpenTickets();
    }

    [RelayCommand]
    private void SelectTicket(SmrServiceTicket ticket)
    {
        SelectedTicket = ticket;
        IsTicketSelected = true;
        LaborLines = _dataStore.GetAll<SmrLaborLineItem>()
            .Where(l => l.TicketId == ticket.TicketId)
            .ToList();
        PartsUsage = _dataStore.GetAll<SmrPartsUsageLedger>()
            .Where(p => p.TicketId == ticket.TicketId)
            .ToList();
        TotalTicketCost = LaborLines.Sum(l => l.CalculatedBurdenCost)
            + PartsUsage.Sum(p => p.UnitCostAtConsumption * p.QuantityConsumed);
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedTicket = null;
        IsTicketSelected = false;
        LaborLines = new();
        PartsUsage = new();
        TotalTicketCost = 0;
    }
}
