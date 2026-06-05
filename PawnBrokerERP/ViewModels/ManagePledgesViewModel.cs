using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PawnBrokerERP.Data;
using PawnBrokerERP.Models;
using System.Collections.ObjectModel;

namespace PawnBrokerERP.ViewModels;

public partial class ManagePledgesViewModel : BaseViewModel
{
    private readonly AppDbContext _db;

    [ObservableProperty] private ObservableCollection<Pledge> _pledges = new();
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _filterStatus = "All";

    public string[] StatusFilters { get; } = { "All", "Active", "PartPaid", "Redeemed", "Overdue" };

    public ManagePledgesViewModel(AppDbContext db)
    {
        _db = db;
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var query = _db.Pledges.Include(p => p.Customer).AsQueryable();

            if (FilterStatus != "All" && Enum.TryParse<PledgeStatus>(FilterStatus, out var status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(p =>
                    p.TicketNumber.Contains(SearchText) ||
                    (p.Customer != null && p.Customer.Name.Contains(SearchText)) ||
                    (p.Customer != null && p.Customer.Phone.Contains(SearchText)));

            var result = await query.OrderByDescending(p => p.PledgeDate).ToListAsync();
            Pledges = new ObservableCollection<Pledge>(result);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value) => _ = LoadAsync();
    partial void OnFilterStatusChanged(string value) => _ = LoadAsync();
}
