namespace PawnBrokerERP.Models;

public enum PledgeStatus
{
    Active,
    Redeemed,
    PartPaid,
    Overdue
}

public class Pledge
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TicketNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public string ItemDescription { get; set; } = string.Empty;
    public decimal WeightGrams { get; set; }
    public decimal PurityPercent { get; set; }
    public string ItemCategory { get; set; } = "Gold";

    public decimal LoanAmount { get; set; }
    public decimal InterestRatePercent { get; set; } = 2.0m;
    public decimal PartPaidAmount { get; set; } = 0m;

    public DateTime PledgeDate { get; set; } = DateTime.Now;
    public DateTime? RedeemDate { get; set; }
    public PledgeStatus Status { get; set; } = PledgeStatus.Active;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; } = false;

    public decimal InterestAccrued =>
        LoanAmount * InterestRatePercent / 100m *
        (decimal)(RedeemDate ?? DateTime.Now).Subtract(PledgeDate).TotalDays / 30m;

    public decimal TotalDue => LoanAmount + InterestAccrued - PartPaidAmount;
}
