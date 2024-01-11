using System;
using System.Collections.Generic;
using System.Linq;
using BisleriumCafe.Data.Models;
using BisleriumCafe.Data.Repositories;
using Microsoft.AspNetCore.Components;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
//using QuestPDF.Common;

namespace BisleriumCafe.Pages
{
	public partial class ExportPdf
	{
		public const string Route = "/export-pdfs";

		private readonly bool Dense = true;
		private readonly bool Fixed_header = true;
		private readonly bool Fixed_footer = true;
		private readonly bool Hover = true;
		private readonly bool ReadOnly = false;

		private int monthNumber = DateTime.Now.Month;

		private Action<string> SetAppBarTitle { get; set; }
		private IEnumerable<TopCoffeeData> Top5CoffeeData;
		private IEnumerable<TopAddInData> Top5AddInData;
	

		protected sealed override void OnInitialized()
		{
			LoadTop5CoffeeData();
			LoadTop5AddinData();

		}

		private ICollection<NewTransaction> GetAllTransactions()
		{
			//return AuthService.IsUserAdmin()
			//    ? AllTransactionRepository.GetAll()
			//    : AllTransactionRepository.GetAll().Where(x => x.CreatedBy == AuthService.CurrentUser.Id).ToList();
			return NewTransactionRepository.GetAll();
		}

		private void LoadTop5CoffeeData()
		{
			DateTime currentDate = DateTime.Now;

			var coffeeTransactions = GetAllTransactions()
				.Where(x => x.SpareType.Equals("Coffee", StringComparison.OrdinalIgnoreCase) &&
					x.TransactionDate.Year == currentDate.Year &&
					x.TransactionDate.Month == monthNumber);

			var topCoffeeData = coffeeTransactions
				.GroupBy(x => x.SpareName, StringComparer.OrdinalIgnoreCase)
				.Select(group => new TopCoffeeData
				{
					ProductName = group.Key,
					TotalQuantity = group.Sum(x => x.Quantity),
					TotalRevenue = group.Sum(x => x.TotalAmount)
				})
				.OrderByDescending(x => x.TotalQuantity)
				.Take(5)
				.ToList();

			Top5CoffeeData = topCoffeeData;
		}

		private void LoadTop5AddinData()
		{
			DateTime currentDate = DateTime.Now;

			var addinTransactions = GetAllTransactions()
				.Where(x => x.SpareType.Equals("AddIn", StringComparison.OrdinalIgnoreCase) &&
					x.TransactionDate.Year == currentDate.Year &&
					x.TransactionDate.Month == monthNumber);

			var topAddInData = addinTransactions
				.GroupBy(x => x.SpareName, StringComparer.OrdinalIgnoreCase)
				.Select(group => new TopAddInData
				{
					ProductName = group.Key,
					TotalQuantity = group.Sum(x => x.Quantity),
					TotalRevenue = group.Sum(x => x.TotalAmount)
				})
				.OrderByDescending(x => x.TotalQuantity)
				.Take(5)
				.ToList();

			Top5AddInData = topAddInData;
		}

		private void SearchMonth()
		{


			Snackbar.Add(@DateTimeFormatInfo.CurrentInfo.GetMonthName(monthNumber).ToString(), Severity.Info);
			LoadTop5AddinData();
			LoadTop5AddinData();


		}


		private class TopCoffeeData
		{
			public string ProductName { get; set; }
			public int TotalQuantity { get; set; }
			public decimal TotalRevenue { get; set; }
		}

		private class TopAddInData
		{
			public string ProductName { get; set; }
			public int TotalQuantity { get; set; }
			public decimal TotalRevenue { get; set; }
		}

		private void GeneratePdfWithCustomData()
		{
			var topCoffeeData = Top5CoffeeData.ToList(); // Assuming Top5CoffeeData is your data collection
			var topAddInData = Top5AddInData.ToList(); // Assuming Top5CoffeeData is your data collection

			Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Size(PageSizes.A4);
					page.Margin(2, Unit.Centimetre);
					page.PageColor(QuestPDF.Helpers.Colors.White);
					page.DefaultTextStyle(x => x.FontSize(20));

					page.Header()
						.Text("Top 5 Coffee Sales")
						.SemiBold().FontSize(36).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);

					page.Content()
						.PaddingVertical(1, Unit.Centimetre)
						.Column(x =>
						{
							x.Spacing(20);

							foreach (var coffeeData in topCoffeeData)
							{
								x.Item().Text($"Product Name: {coffeeData.ProductName} \n Total Quantity: {coffeeData.TotalQuantity} \n Total Revenue: ${coffeeData.TotalRevenue}");
								//x.Item(item =>
								//{
								//    item.Text($"Product Name: {coffeeData.ProductName}");
								//    item.Text($"Total Quantity: {coffeeData.TotalQuantity}");
								//    item.Text($"Total Revenue: {coffeeData.TotalRevenue}");
								//});
							}
						});

					page.Footer()
						.AlignCenter()
						.Text(x =>
						{
							x.Span("Page ");
							x.CurrentPageNumber();
						});
				});
			})
			.GeneratePdf(@"C:\pdf\topcoffee.pdf");

			Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Size(PageSizes.A4);
					page.Margin(2, Unit.Centimetre);
					page.PageColor(QuestPDF.Helpers.Colors.White);
					page.DefaultTextStyle(x => x.FontSize(20));

					page.Header()
						.Text("Top 5 AddIn Sales")
						.SemiBold().FontSize(36).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);

					page.Content()
						.PaddingVertical(1, Unit.Centimetre)
						.Column(x =>
						{
							x.Spacing(20);

							foreach (var addInData in topAddInData)
							{
								x.Item().Text($"Product Name: {addInData.ProductName} \n Total Quantity: {addInData.TotalQuantity} \n Total Revenue: ${addInData.TotalRevenue}");
								
							}
						});

					page.Footer()
						.AlignCenter()
						.Text(x =>
						{
							x.Span("Page ");
							x.CurrentPageNumber();
						});
				});
			})
			.GeneratePdf(@"C:\pdf\topaddins.pdf");

			Snackbar.Add($"PDF Generated at: C:\\pdf\\topcoffee.pdf", Severity.Success);
			Snackbar.Add($"PDF Generated at: C:\\pdf\\topaddins.pdf", Severity.Success);
		}
	}
}
