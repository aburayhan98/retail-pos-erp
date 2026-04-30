using FluentValidation;
using RetailErp.Pos.Application.DTOs.Products;

namespace RetailErp.Pos.Application.Validators;

public sealed class CreateProductRequestValidator
		: AbstractValidator<CreateProductRequest>
{
	public CreateProductRequestValidator()
	{
		RuleFor(x => x.Name)
				.NotEmpty()
				.WithMessage("Product name is required.")
				.MaximumLength(100)
				.WithMessage("Product name must not exceed 100 characters.");

		RuleFor(x => x.Barcode)
				.NotEmpty()
				.WithMessage("Barcode is required.")
				.MaximumLength(50)
				.WithMessage("Barcode must not exceed 50 characters.");

		RuleFor(x => x.Price)
				.GreaterThan(0)
				.WithMessage("Price must be greater than zero.");

		RuleFor(x => x.StockQuantity)
				.GreaterThanOrEqualTo(0)
				.WithMessage("Stock quantity cannot be negative.");
	}
}