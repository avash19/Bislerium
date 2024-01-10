namespace BisleriumCafe.Shared.Dialogs;

public partial class AddSpareDialog
{
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; }
    [Parameter] public Action ChangeParentState { get; set; }

    private MudForm form;

    private string Name;
    private string Description;
    private string Company;

    private string ItemType;
    private decimal Price;
    private int AvailableQuantity;

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private async Task AddSpare()
    {
        await form.Validate();
        if (form.IsValid)
        {
            Spare spare = new()
            {
                Name = Name,
                Description = Description,
                Company = Company,
                ItemType = ItemType,
                Price = Price,
                AvailableQuantity = AvailableQuantity
            };
            SpareRepository.Add(spare);
            ChangeParentState.Invoke();

            Snackbar.Add($"Item {Name} of type {ItemType} is Added!", Severity.Success);
            MudDialog.Close();
        }
    }
}
