using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PawnBrokerERP.Data;
using PawnBrokerERP.Models;

namespace PawnBrokerERP.ViewModels;

public partial class RedeemPledgeViewModel : BaseViewModel
{
    private readonly AppDbContext _db;
    private Pledge? _foundPledge;

    [ObservableProperty] private string _ticketSearch = string.Empty;
    [ObservableProperty] private bool _pledgeFound;

    [ObservableProperty] private string _customerName = string.Empty;
    [ObservableProperty] private string _itemDescription = string.Empty;
    [ObservableProperty] private decimal _loanAmount;
    [ObservableProperty] private decimal _interestAccrued;
    [ObservableProperty] private decimal _partPaid;
    [ObservableProperty] private decimal _totalDue;
    [ObservableProperty] private int _daysElapsed;
    [ObservableProperty] private string _pledgeDate = string.Empty;

    public RedeemPledgeViewModel(AppDbContext db)
    {
        _db = db;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        ClearMessages();
        PledgeFound = false;
        _foundPledge = null;

        if (string.IsNullOrWhiteSpace(TicketSearch))
        { ErrorMessage = "Enter a ticket number."; return; }

        IsBusy = true;
        try
        {
            _foundPledge = await _db.Pledges
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(p => p.TicketNumber == TicketSearch.Trim().ToUpper());

            if (_foundPledge == null)
            { ErrorMessage = "Ticket not found."; return; }

            if (_foundPledge.Status == PledgeStatus.Redeemed)
            { ErrorMessage = "This pledge has already been redeemed."; return; }

            CustomerName = _foundPledge.Customer?.Name ?? "N/A";
            ItemDescription = _foundPledge.ItemDescription;
            LoanAmount = _foundPledge.LoanAmount;
            PartPaid = _foundPledge.PartPaidAmount;
            DaysElapsed = (int)(DateTime.Now - _foundPledge.PledgeDate).TotalDays;
            PledgeDate = _foundPledge.PledgeDate.ToString("dd MMM yyyy");
            InterestAccrued = _foundPledge.InterestAccrued;
            TotalDue = _foundPledge.TotalDue;
            PledgeFound = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RedeemAsync()
    {
        if (_foundPledge == null) return;
        IsBusy = true;
        try
        {
            _foundPledge.Status = PledgeStatus.Redeemed;
            _foundPledge.RedeemDate = DateTime.Now;
            _foundPledge.LastUpdated = DateTime.UtcNow;
            _foundPledge.IsSynced = false;
            await _db.SaveChangesAsync();

            SuccessMessage = $"Pledge {_foundPledge.TicketNumber} redeemed. Total collected: ₹{TotalDue:N2}";
            PledgeFound = false;
            TicketSearch = string.Empty;
            _foundPledge = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Redemption failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
