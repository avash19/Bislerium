using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BisleriumCafe.Shared.Dialogs
{
    public partial class AddMembershipDialog
    {
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }
        [Parameter] public Action ChangeParentState { get; set; }

        private MudForm form;

        private string FullName;
        private string Email;
        private string PhoneNumber;
        private bool ThisDrinkFree = false;
        private bool IsRegularCustomer = false;
        private bool GetsDiscount = false;
        private int DiscountPercent;

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task AddMembership()
        {
            await form.Validate();
            if (form.IsValid)
            {
                var membership = new Membership
                {
                    FullName = FullName,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    ThisDrinkFree = ThisDrinkFree,
                    IsRegularCustomer = IsRegularCustomer,
                    GetsDiscount = GetsDiscount,
                    DiscountPercent = DiscountPercent
                };

                MembershipRepository.Add(membership);
                ChangeParentState.Invoke();

                Snackbar.Add("Membership Added!", Severity.Success);
                MudDialog.Close();
            }
        
    }
}
}
