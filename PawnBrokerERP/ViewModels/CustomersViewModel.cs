using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PawnBrokerERP.Data;
using PawnBrokerERP.Models;
using System.Collections.ObjectModel;

namespace PawnBrokerERP.ViewModels;

public partial class CustomersViewModel : BaseViewModel
{
    private readonly AppDbContext _db;

    [ObservableProperty] private ObservableCollection<Customer> _customers = new();
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private int _totalCustomers;

    public CustomersViewModel(AppDbContext db)
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
            var query = _db.Customers
                .Include(c => c.Pledges)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(c =>
                    c.Name.Contains(SearchText) ||
                    c.Phone.Contains(SearchText) ||
                    c.AadhaarNumber.Contains(SearchText));

            var result = await query.OrderBy(c => c.Name).ToListAsync();
            Customers = new ObservableCollection<Customer>(result);
            TotalCustomers = await _db.Customers.CountAsync();
        }
        finally { IsBusy = false; }
    }

    partial void OnSearchTextChanged(string value) => _ = LoadAsync();
}
