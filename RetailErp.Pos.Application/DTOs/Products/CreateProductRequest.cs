using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailErp.Pos.Application.DTOs.Products
{
	public sealed record CreateProductRequest
	{
		public string Name { get; set; } = string.Empty;
		public string Barcode { get; set; } = string.Empty;
		public int StockQuantity { get; set; }
		public decimal Price { get; set; }
	}
}
