namespace BisleriumCafe.Pages;

public partial class Memberships
{
    public const string Route = "/membership";

    private readonly bool Dense = true;
    private readonly bool Fixed_header = true;
    private readonly bool Fixed_footer = true;
    private readonly bool Hover = true;
    private readonly bool ReadOnly = false;
    private readonly bool VanCancelEdit = true;
    private readonly bool BlockSwitch = true;
    private string SearchString;
    private Membership ElementBeforeEdit;
    private readonly TableApplyButtonPosition ApplyButtonPosition = TableApplyButtonPosition.End;
    private readonly TableEditButtonPosition EditButtonPosition = TableEditButtonPosition.End;
    private readonly TableEditTrigger EditTrigger = TableEditTrigger.RowClick;
    private IEnumerable<Membership> Elements;

    [CascadingParameter]
    private Action<string> SetAppBarTitle { get; set; }

    protected override void OnInitialized()
    {
        SetAppBarTitle.Invoke("Manage Members");
        Elements = MembershipRepository.GetAll();
    }

    private void BackupItem(object element)
    {
        ElementBeforeEdit = ((Membership)element).Clone() as Membership;
    }

    private string GetUserName(Guid id)
    {
        var phonenumber = MembershipRepository.Get(x => x.Id, id)?.PhoneNumber;
        return phonenumber is null ? "N/A" : phonenumber;
    }

    private void ResetItemToOriginalValues(object element)
    {
        ((Membership)element).PhoneNumber = ElementBeforeEdit.PhoneNumber;
        ((Membership)element).Email = ElementBeforeEdit.Email;
        ((Membership)element).FullName = ElementBeforeEdit.FullName;
        //((User)element).Role = ElementBeforeEdit.Role;
    }

    private bool FilterFunc(Membership element)
    {
        return string.IsNullOrWhiteSpace(SearchString)
               || element.Id.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.PhoneNumber.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.Email.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.FullName.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               //|| element.HasInitialPassword.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.CreatedAt.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase);
    }

    private async Task AddDialog()
    {
        DialogParameters parameters = new()
        {
            { "ChangeParentState", new Action(StateHasChanged) }
        };
        await DialogService.ShowAsync<Shared.Dialogs.AddMembershipDialog>("Add Member", parameters);
    }

    private void Reload()
    {
        StateHasChanged();
    }
}