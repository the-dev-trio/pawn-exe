using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PawnBrokerERP.Data;
using PawnBrokerERP.Models;

namespace PawnBrokerERP.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly AppDbContext _db;

    [ObservableProperty] private int _todayNewLoans;
    [ObservableProperty] private int _activeLoans;
    [ObservableProperty] private int _overdueLoans;
    [ObservableProperty] private decimal _cashInTill;
    [ObservableProperty] private int _pendingSyncCount;
    [ObservableProperty] private string _todayDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");

    public DashboardViewModel(AppDbContext db)
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
            var today = DateTime.Today;
            TodayNewLoans = await _db.Pledges.CountAsync(p => p.PledgeDate.Date == today);
            ActiveLoans = await _db.Pledges.CountAsync(p => p.Status == PledgeStatus.Active || p.Status == PledgeStatus.PartPaid);
            OverdueLoans = await _db.Pledges.CountAsync(p => p.Status == PledgeStatus.Active && p.PledgeDate < today.AddDays(-30));
            CashInTill = await _db.Pledges
                .Where(p => p.Status == PledgeStatus.Redeemed && p.RedeemDate.HasValue && p.RedeemDate.Value.Date == today)
                .SumAsync(p => (decimal?)p.TotalDue) ?? 0m;
            PendingSyncCount = await _db.Pledges.CountAsync(p => !p.IsSynced);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
