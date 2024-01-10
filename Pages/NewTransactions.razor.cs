using System;
using System.Collections.Generic;
using System.Linq;
using BisleriumCafe.Data.Models;
using BisleriumCafe.Data.Repositories;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BisleriumCafe.Pages;

public partial class NewTransactions
{
	public const string Route = "/new-transactions";

	private readonly bool Dense = true;
	private readonly bool Fixed_header = true;
	private readonly bool Fixed_footer = true;
	private readonly bool Hover = true;
	private readonly bool ReadOnly = false;
	private string SearchString;
	private IEnumerable<NewTransaction> Elements;

	[CascadingParameter]
	private Action<string> SetAppBarTitle { get; set; }

	protected sealed override void OnInitialized()
	{
		SetAppBarTitle.Invoke("All Transactions");
		Elements = GetNewTransactions();
	}

	private string GetUserName(Guid id)
	{
		return UserRepository.Get(x => x.Id, id)?.UserName;
	}

	private ICollection<NewTransaction> GetNewTransactions()
	{
		return AuthService.IsUserAdmin()
			? NewTransactionRepository.GetAll()
			: NewTransactionRepository.GetAll().Where(x => x.CreatedBy == AuthService.CurrentUser.Id).ToList();
	}

	private Tuple<bool, string> GetUser(Guid id)
	{
		User user = UserRepository.Get(x => x.Id, id);
		return new Tuple<bool, string>(user?.Role == UserRole.Admin, user?.UserName);
	}

	private bool FilterFunc(NewTransaction element)
	{
		if (string.IsNullOrWhiteSpace(SearchString))
		{
			return true;
		}

		if (element.Id.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		if (element.MemberName.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		if (element.TransactionDate.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		if (element.TotalAmount.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		if (element.CreatedAt.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		if (element.CreatedBy.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		string createdByUser = GetUserName(element.CreatedBy);
		return (createdByUser is not null && createdByUser.Contains(SearchString, StringComparison.OrdinalIgnoreCase));
	}

	private void FilterByMonth(string a)
	{
		ICollection<NewTransaction> repo = GetNewTransactions();
		if (string.IsNullOrEmpty(a))
		{
			Elements = repo;
			return;
		}
		string[] yearMonth = a.Split('-');
		Elements = repo.Where(x => x.CreatedAt.Year == int.Parse(yearMonth[0]) && x.CreatedAt.Month == int.Parse(yearMonth[1])).ToList();
	}
}