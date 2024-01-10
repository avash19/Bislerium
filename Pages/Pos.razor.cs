using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

//sing System.Transactions;

namespace BisleriumCafe.Pages;


public partial class Pos
{
    public const string Route = "/pos";

    public class ProductCartItem
    {
        public Spare Spare_ { get; set; }
        public int Quantity { get; set; }
    }

    private List<ProductCartItem> CartItems = new List<ProductCartItem>();
    private List<Spare> Products = new List<Spare>();

    //For membership

    private string searchPhoneNumber { get; set; }
    private Membership foundMember;
    private bool isRegularMember;
    private bool isThisDrinkFree;
    private List<Membership> Members = new List<Membership>();





    [CascadingParameter]
    private Action<string> SetAppBarTitle { get; set; }

    protected override void OnInitialized()
    {
        SetAppBarTitle.Invoke("POS");

        Products = SpareRepository.GetAll().ToList();

        Members = MembershipRepository.GetAll().ToList();

    }

    private void AddToCart(Spare spare)
    {
        var cartItem = CartItems.FirstOrDefault(item => item.Spare_.Id == spare.Id);

        if (cartItem == null)
        {
            CartItems.Add(new ProductCartItem { Spare_ = spare, Quantity = 1 });
        }
        else
        {
            cartItem.Quantity++;
        }
    }

    private void RemoveFromCart(ProductCartItem cartItem)
    {
        if (cartItem.Quantity > 1)
        {
            cartItem.Quantity--;
        }
        else
        {
            CartItems.Remove(cartItem);
        }
    }

    private decimal CalculateTotal()
    {
        return CartItems.Sum(item => item.Quantity * item.Spare_.Price);
    }

    private bool AtleastOneCoffee()
    {
        bool atleast = false;
        if (CartItems.Count > 0)
        {
            foreach (var cartItem in CartItems)
            {

                if (cartItem.Spare_.ItemType == "Coffee")
                {
                    atleast = true;
                }
            }
        }
        return atleast;

    }


    private decimal CalculateRegularCustomerDiscount()
    {
        // Define your regular customer discount logic here
        // For example, a flat 10% discount for regular customers
        return CalculateTotal() * 0.1m;
    }

    private decimal CalculateTotalWithDiscount()
    {
        // Calculate the total with regular customer discount
        return CalculateTotal() - CalculateRegularCustomerDiscount();
    }

    private decimal CalculateDiscount(decimal originalPrice, decimal discountPercentage)
    {
        return originalPrice * discountPercentage;
    }

    // Atleast one purchase per month
    private void UpdateRegularCustomerStatus(Membership member)
    {
        // Check if the member is already marked as a regular customer
        if (member.IsRegularCustomer)
        {
            // Skip the calculation if the member is already a regular customer
            return;
        }

        // Get the current date
        DateTime currentDate = DateTime.Now;

        // Get the start date for the month
        DateTime monthStartDate = new DateTime(currentDate.Year, currentDate.Month, 1);

        // Check if any transactions were made in the current month
        bool madePurchaseThisMonth = NewTransactionRepository.GetAll()
            .Any(transaction => transaction.MemberName.Equals(member.FullName, StringComparison.OrdinalIgnoreCase)
                                && transaction.TransactionDate.Year == currentDate.Year
                                && transaction.TransactionDate.Month == currentDate.Month);

        // Update the IsRegularCustomer property based on the condition
        member.IsRegularCustomer = madePurchaseThisMonth;

        System.Diagnostics.Debug.WriteLine($"Member: {member}");


        // Update the member in the repository
        System.Diagnostics.Debug.WriteLine($"Going to update inside UpdateRegularCustomerStatus");

        MembershipRepository.Update(member);
    }


    private void CompleteTransaction()
    {
        System.Diagnostics.Debug.WriteLine(CartItems, "capital cartItemcartItemcartItem");

        //User user = AuthService.CurrentUser;

        var atleastonecoffee = AtleastOneCoffee();



        if (CartItems.Count > 0)
        {
            if (atleastonecoffee)
            {
                decimal totalDiscount = 0;

                var member = SearchMember();

                Guid memberId;

                String memberName;

                if (member == null)
                {
                    memberId = Guid.Empty;
                    memberName = "";

                }
                else
                {
                    memberId = member.Id;
                    memberName = member.FullName;

                }

                User user = AuthService.CurrentUser;

                //Discount = isRegular ? "10%" : "-",

                // Perform the logic to complete the transaction (e.g., save to database)
                // You can access the items in the cart via the 'CartItems' list
                // and perform any additional business logic here.

                foreach (var cartItem in CartItems)
                {
                    System.Diagnostics.Debug.WriteLine(cartItem, "cartItemcartItemcartItem");
                    // Perform logic to save each item in the cart to the database
                    // Here, you might want to create a new transaction record in your database
                    // with details like Product Id, Quantity, Price, etc.
                    // For demonstration purposes, let's assume you have a TransactionRepository
                    // that allows you to save transactions to the database.

                    var productType = cartItem.Spare_.ItemType;

                    decimal discountAmount = 0;

                    if (productType.ToString().Equals("coffee", StringComparison.OrdinalIgnoreCase))
                    {
                        // do all calculations like complementary and regular stuff

                        if (member != null)
                        {
                            member.DrinksPurchased += cartItem.Quantity;
                            if (member.IsRegularCustomer)
                            {
                                // Calculate the discount amount without modifying the original price
                                discountAmount = CalculateDiscount(cartItem.Spare_.Price, 0.1m); // 10% discount

                                // Display the discount information
                                Snackbar.Add($"You've received a 10% discount on {cartItem.Spare_.Name}. Discount Amount: {discountAmount}");

                                // Accumulate the discount for the entire cart
                                totalDiscount += discountAmount;
                            }
                            // Calculate the number of complimentary drinks earned
                            int complimentaryDrinks = member.DrinksPurchased / 10;

                            // Check if the member earned any complimentary drinks
                            if (complimentaryDrinks > 0)
                            {
                                // Redeem complimentary drinks
                                Snackbar.Add($"Congratulations! You've earned {complimentaryDrinks} free complimentary drink(s).", Severity.Success);

                                // Subtract the corresponding purchase count for the earned drinks
                                member.DrinksPurchased -= complimentaryDrinks * 10;
                            }

                            var transaction = new NewTransaction
                            {
                                Id = Guid.NewGuid(),
                                MemberId = memberId,
                                MemberName = memberName,
                                TransactionDate = DateTime.Now,
                                CreatedAt = DateTime.Now,
                                CreatedBy = user.Id,
                                SpareId = cartItem.Spare_.Id,
                                SpareName = cartItem.Spare_.Name,
                                SpareType = cartItem.Spare_.ItemType.ToString(),
                                Quantity = cartItem.Quantity,
                                TotalAmount = (cartItem.Quantity * cartItem.Spare_.Price) - discountAmount,
                                Discount = discountAmount.ToString(),

                            };

                            NewTransactionRepository.Add(transaction);

                        }
                        else
                        {
                            var transaction = new NewTransaction
                            {
                                Id = Guid.NewGuid(),
                                MemberId = memberId,
                                MemberName = memberName,
                                TransactionDate = DateTime.Now,
                                CreatedAt = DateTime.Now,
                                CreatedBy = user.Id,
                                SpareId = cartItem.Spare_.Id,
                                SpareName = cartItem.Spare_.Name,
                                SpareType = cartItem.Spare_.ItemType.ToString(),
                                Quantity = cartItem.Quantity,
                                TotalAmount = cartItem.Quantity * cartItem.Spare_.Price,
                                Discount = "0",

                            };

                            NewTransactionRepository.Add(transaction);
                        }


                    }
                    else
                    {
                        var transaction = new NewTransaction
                        {
                            Id = Guid.NewGuid(),
                            MemberId = memberId,
                            MemberName = memberName,
                            TransactionDate = DateTime.Now,
                            CreatedAt = DateTime.Now,
                            CreatedBy = user.Id,
                            SpareId = cartItem.Spare_.Id,
                            SpareName = cartItem.Spare_.Name,
                            SpareType = cartItem.Spare_.ItemType.ToString(),
                            Quantity = cartItem.Quantity,
                            TotalAmount = cartItem.Quantity * cartItem.Spare_.Price,
                            Discount = "0",

                        };

                        NewTransactionRepository.Add(transaction);
                    }



                }

                // Clear the cart after completing the transaction
                CartItems.Clear();

                // Show a success message or perform any other post-transaction actions
                Snackbar.Add("Transaction completed successfully!", Severity.Success);
            }
            else
            {
                Snackbar.Add("Select atleast one coffee!", Severity.Error);
            }
        }

    }

    //private void SearchMember()
    //{
    //    // Implement the logic to search for a member using the provided phone number
    //    // You can use your MembershipRepository or any other service to perform the search
    //    // For demonstration purposes, let's assume you have a MembershipRepository.

    //    var foundMember = MembershipRepository.GetByPhoneNumber(searchPhoneNumber);

    //    if (foundMember != null)
    //    {
    //        foundMemberId = foundMember.Id;
    //        isRegularMember = !foundMember.ThisDrinkFree && !foundMember.GetsDiscount;
    //        isThisDrinkFree = foundMember.ThisDrinkFree;
    //    }
    //    else
    //    {
    //        // Handle the case when no member is found with the provided phone number
    //        Snackbar.Add("No member found with the provided phone number.", Severity.Warning);
    //    }
    //}

    private Membership SearchMember()
    {


        if (!string.IsNullOrWhiteSpace(searchPhoneNumber))
        {
            UpdateRegularCustomerStatus(Members.FirstOrDefault(m => m.PhoneNumber == searchPhoneNumber));
            return Members.FirstOrDefault(m => m.PhoneNumber == searchPhoneNumber);
        }

        return null;
    }

    private async Task SearchMemberHandler()
    {
        var member = SearchMember();

        if (member != null)
        {
            foundMember = member;
            // Handle the found member, for example, display some information
            Snackbar.Add($"Found member: {member.FullName} ({member.PhoneNumber})", Severity.Success);
        }
        else
        {
            foundMember = null;
            Snackbar.Add("Member not found.", Severity.Error);
        }
    }




    //private ICollection<ActivityLog> GetByUserType()
    //{
    //    return AuthService.IsUserAdmin()
    //    ? ActivityLogRepository.GetAll()
    //    : ActivityLogRepository.GetAll().Where(x => x.ActedBy == AuthService.CurrentUser.Id).ToList();
    //}

    //private Tuple<bool, string> GetUser(Guid id)
    //{
    //    User user = UserRepository.Get(x => x.Id, id);
    //    return new Tuple<bool, string>(user?.Role == UserRole.Admin, user?.UserName);
    //}

    //private string GetSpareName(Guid id)
    //{
    //    return ProductRepository.Get(x => x.Id, id)?.Name;
    //}

    //private string GetUserName(Guid id)
    //{
    //    return UserRepository.Get(x => x.Id, id)?.UserName;
    //}

    //private bool FilterFunc(ActivityLog element)
    //{
    //    if (string.IsNullOrWhiteSpace(SearchString))
    //    {
    //        return true;
    //    }

    //    if (element.Id.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase))
    //    {
    //        return true;
    //    }

    //    if (element.SpareID.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase))
    //    {
    //        return true;
    //    }

    //    string spare = GetSpareName(element.SpareID);
    //    if (spare is not null && spare.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
    //    {
    //        return true;
    //    }

    //    if (element.Quantity.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase))
    //    {
    //        return true;
    //    }

    //    if (element.Action.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase))
    //    {
    //        return true;
    //    }

    //    if (element.ActedBy.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase))
    //    {
    //        return true;
    //    }

    //    string takenByUser = GetUserName(element.ActedBy);
    //    if (takenByUser is not null && takenByUser.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
    //    {
    //        return true;
    //    }

    //    if (element.ApprovalStatus.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase))
    //    {
    //        return true;
    //    }

    //    if (element.Approver.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase))
    //    {
    //        return true;
    //    }

    //    string approvedByUser = GetUserName(element.Approver);
    //    return (approvedByUser is not null && approvedByUser.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
    //           || element.ActionOn.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase);
    //}

    //private void FilterByMonth(string a)
    //{
    //    ICollection<ActivityLog> repo = GetByUserType();
    //    if (string.IsNullOrEmpty(a))
    //    {
    //        Elements = repo;
    //        return;
    //    }
    //    string[] yearMonth = a.Split('-');
    //    Elements = repo.Where(x => x.ApprovalStatusOn.Year == int.Parse(yearMonth[0]) && x.ActionOn.Month == int.Parse(yearMonth[1])).ToList();
}

//.////////////////////////////////////////////////////////////////////////////////






/////////////////////////////////////////////////////////////////////////////////


