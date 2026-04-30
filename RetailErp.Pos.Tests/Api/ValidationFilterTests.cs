using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using RetailErp.Pos.API.Filters;
using RetailErp.Pos.Application.DTOs.Products;

namespace RetailErp.Pos.Tests.Api;

public sealed class ValidationFilterTests
{
	[Fact]
	public async Task OnActionExecutionAsync_ShouldSetBadRequestResult_WhenValidationFails()
	{
		var filter = new ValidationFilter(new TestServiceProvider(new Dictionary<Type, object>
		{
			[typeof(IValidator<CreateProductRequest>)] = new FailingCreateProductRequestValidator()
		}));
		var httpContext = new DefaultHttpContext();
		httpContext.Request.Path = "/api/products";
		var context = new ActionExecutingContext(
			new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
			[],
			new Dictionary<string, object?> { ["request"] = new CreateProductRequest() },
			controller: new object());
		var nextCalled = false;

		await filter.OnActionExecutionAsync(context, () =>
		{
			nextCalled = true;
			return Task.FromResult(new ActionExecutedContext(context, [], new object()));
		});

		nextCalled.Should().BeFalse();
		var result = context.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
		var problem = result.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
		problem.Status.Should().Be(StatusCodes.Status400BadRequest);
		problem.Instance.Should().Be("/api/products");
		problem.Errors.Should().ContainKey(nameof(CreateProductRequest.Name));
		problem.Errors[nameof(CreateProductRequest.Name)].Should().Contain("Name is required.");
	}

	[Fact]
	public async Task OnActionExecutionAsync_ShouldCallNext_WhenNoValidatorExists()
	{
		var filter = new ValidationFilter(new TestServiceProvider());
		var context = new ActionExecutingContext(
			new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
			[],
			new Dictionary<string, object?> { ["request"] = new CreateProductRequest() },
			controller: new object());
		var nextCalled = false;

		await filter.OnActionExecutionAsync(context, () =>
		{
			nextCalled = true;
			return Task.FromResult(new ActionExecutedContext(context, [], new object()));
		});

		nextCalled.Should().BeTrue();
		context.Result.Should().BeNull();
	}

	private sealed class FailingCreateProductRequestValidator : AbstractValidator<CreateProductRequest>
	{
		public FailingCreateProductRequestValidator()
		{
			RuleFor(request => request.Name).NotEmpty().WithMessage("Name is required.");
		}
	}

	private sealed class TestServiceProvider(Dictionary<Type, object>? services = null) : IServiceProvider
	{
		private readonly Dictionary<Type, object> _services = services ?? [];

		public object? GetService(Type serviceType) =>
			_services.TryGetValue(serviceType, out var service) ? service : null;
	}
}
