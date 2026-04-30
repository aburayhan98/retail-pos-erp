using FluentValidation;
using RetailErp.Pos.Application.DTOs.Sales;

namespace RetailErp.Pos.Application.Validators;

public sealed class CreateSalesRequestValidator
		: AbstractValidator<CreateSaleRequest>
{
	public CreateSalesRequestValidator()
	{
		RuleFor(x => x.OutletId)
				.NotEmpty()
				.WithMessage("OutletId is required.");

		RuleFor(x => x.Items)
				.NotNull()
				.WithMessage("Sale items are required.")
				.Must(items => items.Count > 0)
				.WithMessage("Sale must contain at least one item.");

		RuleForEach(x => x.Items)
				.SetValidator(new CreateSaleItemRequestValidator());
	}
}

public sealed class CreateSaleItemRequestValidator
		: AbstractValidator<CreateSaleItemRequest>
{
	public CreateSaleItemRequestValidator()
	{
		RuleFor(x => x.ProductId)
				.NotEmpty()
				.WithMessage("ProductId is required.");

		RuleFor(x => x.Barcode)
				.NotEmpty()
				.WithMessage("Barcode is required.")
				.MaximumLength(50)
				.WithMessage("Barcode must not exceed 50 characters.");

		RuleFor(x => x.Quantity)
				.GreaterThan(0)
				.WithMessage("Quantity must be greater than zero.");

		RuleFor(x => x.UnitPrice)
				.GreaterThan(0)
				.WithMessage("Unit price must be greater than zero.");
	}
}
