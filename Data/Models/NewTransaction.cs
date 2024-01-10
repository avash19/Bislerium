using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BisleriumCafe.Data.Models;

public class NewTransaction : IModel, ICloneable
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public Guid MemberId { get; set; }

	public string MemberName { get; set; }

    public Guid SpareId { get; set; }

    public string SpareName { get; set; }
    public string SpareType { get; set; }
    public string Discount { get; set; }

    public int Quantity { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.Now;
	public decimal TotalAmount { get; set; }

	public DateTime CreatedAt { get; set; } = DateTime.Now;
	public Guid CreatedBy { get; set; }


	public object Clone()
	{
		return new NewTransaction
		{
			Id = Id,
			
			MemberId = MemberId,
			MemberName = MemberName,
            SpareId =SpareId,
            SpareName = SpareName,
            TransactionDate = TransactionDate,
			TotalAmount = TotalAmount,
			CreatedAt = CreatedAt,
			CreatedBy = CreatedBy,
		};
	}

	public override string ToString()
	{
		return JsonSerializer.Serialize(this);
	}
}
