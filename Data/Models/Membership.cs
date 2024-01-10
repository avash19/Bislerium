namespace BisleriumCafe.Data.Models;

public class Membership : IModel, ICloneable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; }
    public string Email { get; set; }
    public int DaysPurchased { get; set; }
    public int DrinksPurchased { get; set; }
    public bool IsRegularCustomer { get; set; }
    public string PhoneNumber { get; set; }
    public bool ThisDrinkFree { get; set; }
    public bool GetsDiscount { get; set; }
    public int DiscountPercent { get; set; } = 10;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public Guid CreatedBy { get; set; }


    public object Clone()
    {
        return new Membership
        {
            Id = Id,
            FullName = FullName,
            Email = Email,
            PhoneNumber = PhoneNumber,
            DrinksPurchased = DrinksPurchased,
            ThisDrinkFree = ThisDrinkFree,
            GetsDiscount = GetsDiscount,
            DiscountPercent = DiscountPercent,
            CreatedAt = CreatedAt,
            CreatedBy = CreatedBy,
            IsRegularCustomer = IsRegularCustomer,
        };
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}