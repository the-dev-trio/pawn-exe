using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PawnBrokerERP.Data;
using PawnBrokerERP.Models;

namespace PawnBrokerERP.ViewModels;

public partial class NewPledgeViewModel : BaseViewModel
{
    private readonly AppDbContext _db;

    // Customer fields
    [ObservableProperty] private string _customerName = string.Empty;
    [ObservableProperty] private string _customerPhone = string.Empty;
    [ObservableProperty] private string _customerAddress = string.Empty;
    [ObservableProperty] private string _aadhaarNumber = string.Empty;

    // Item fields
    [ObservableProperty] private string _itemDescription = string.Empty;
    [ObservableProperty] private string _itemCategory = "Gold";
    [ObservableProperty] private decimal _weightGrams;
    [ObservableProperty] private decimal _purityPercent = 91.6m;

    // Loan fields
    [ObservableProperty] private decimal _loanAmount;
    [ObservableProperty] private decimal _interestRate = 2.0m;

    [ObservableProperty] private string _generatedTicket = string.Empty;

    public string[] ItemCategories { get; } = { "Gold", "Silver", "Diamond", "Watch", "Electronics", "Other" };

    public NewPledgeViewModel(AppDbContext db)
    {
        _db = db;
    }

    [RelayCommand]
    private async Task CreatePledgeAsync()
    {
        ClearMessages();

        if (string.IsNullOrWhiteSpace(CustomerName))
        { ErrorMessage = "Customer name is required."; return; }

        if (CustomerPhone.Length != 10 || !CustomerPhone.All(char.IsDigit))
        { ErrorMessage = "Valid 10-digit phone number required."; return; }

        if (string.IsNullOrWhiteSpace(ItemDescription))
        { ErrorMessage = "Item description is required."; return; }

        if (LoanAmount <= 0)
        { ErrorMessage = "Loan amount must be greater than zero."; return; }

        IsBusy = true;
        try
        {
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Phone == CustomerPhone);
            if (customer == null)
            {
                customer = new Customer
                {
                    Name = CustomerName,
                    Phone = CustomerPhone,
                    Address = CustomerAddress,
                    AadhaarNumber = AadhaarNumber
                };
                _db.Customers.Add(customer);
                await _db.SaveChangesAsync();
            }
            else
            {
                customer.Name = CustomerName;
                customer.Address = CustomerAddress;
                customer.LastUpdated = DateTime.UtcNow;
            }

            var ticket = _db.GenerateTicketNumber();
            var pledge = new Pledge
            {
                TicketNumber = ticket,
                CustomerId = customer.Id,
                ItemDescription = ItemDescription,
                ItemCategory = ItemCategory,
                WeightGrams = WeightGrams,
                PurityPercent = PurityPercent,
                LoanAmount = LoanAmount,
                InterestRatePercent = InterestRate,
                PledgeDate = DateTime.Now,
                Status = PledgeStatus.Active
            };

            _db.Pledges.Add(pledge);
            await _db.SaveChangesAsync();

            GeneratedTicket = ticket;
            SuccessMessage = $"Pledge created! Ticket: {ticket}";
            ResetForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create pledge: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ResetForm()
    {
        CustomerPhone = CustomerAddress = AadhaarNumber = string.Empty;
        ItemDescription = string.Empty;
        WeightGrams = 0; PurityPercent = 91.6m; LoanAmount = 0; InterestRate = 2.0m;
        ItemCategory = "Gold";
    }
}
