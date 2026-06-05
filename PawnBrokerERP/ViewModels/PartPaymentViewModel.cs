using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PawnBrokerERP.Data;
using PawnBrokerERP.Models;

namespace PawnBrokerERP.ViewModels;

public partial class PartPaymentViewModel : BaseViewModel
{
    private readonly AppDbContext _db;
    private Pledge? _foundPledge;

    [ObservableProperty] private string _ticketSearch = string.Empty;
    [ObservableProperty] private bool _pledgeFound;
    [ObservableProperty] private string _customerName = string.Empty;
    [ObservableProperty] private decimal _loanAmount;
    [ObservableProperty] private decimal _totalDue;
    [ObservableProperty] private decimal _alreadyPaid;
    [ObservableProperty] private decimal _paymentAmount;

    public PartPaymentViewModel(AppDbContext db) => _db = db;

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
                .FirstOrDefaultAsync(p => p.TicketNumber == TicketSearch.Trim().ToUpper() &&
                                          p.Status != PledgeStatus.Redeemed);

            if (_foundPledge == null)
            { ErrorMessage = "Active pledge not found for that ticket."; return; }

            CustomerName = _foundPledge.Customer?.Name ?? "N/A";
            LoanAmount = _foundPledge.LoanAmount;
            AlreadyPaid = _foundPledge.PartPaidAmount;
            TotalDue = _foundPledge.TotalDue;
            PledgeFound = true;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task RecordPaymentAsync()
    {
        if (_foundPledge == null) return;

        if (PaymentAmount <= 0)
        { ErrorMessage = "Payment amount must be greater than zero."; return; }

        if (PaymentAmount > TotalDue)
        { ErrorMessage = "Payment cannot exceed total due."; return; }

        IsBusy = true;
        try
        {
            _foundPledge.PartPaidAmount += PaymentAmount;
            _foundPledge.Status = PledgeStatus.PartPaid;
            _foundPledge.LastUpdated = DateTime.UtcNow;
            _foundPledge.IsSynced = false;
            await _db.SaveChangesAsync();

            SuccessMessage = $"₹{PaymentAmount:N2} recorded for {_foundPledge.TicketNumber}.";
            PledgeFound = false;
            TicketSearch = string.Empty;
            PaymentAmount = 0;
            _foundPledge = null;
        }
        catch (Exception ex) { ErrorMessage = $"Payment failed: {ex.Message}"; }
        finally { IsBusy = false; }
    }
}
