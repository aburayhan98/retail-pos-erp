using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RetailErp.Pos.API.Filters;

public sealed class ValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
	private readonly IServiceProvider _serviceProvider = serviceProvider;

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		foreach (var argument in context.ActionArguments.Values)
		{
			if (argument is null)
			{
				continue;
			}

			var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
			var validator = _serviceProvider.GetService(validatorType) as IValidator;

			if (validator is null)
			{
				continue;
			}

			var validationContextType = typeof(ValidationContext<>).MakeGenericType(argument.GetType());
			var validationContext = Activator.CreateInstance(validationContextType, argument);

			if (validationContext is null)
			{
				continue;
			}

			var validateAsyncMethod = validator.GetType().GetMethod(
				nameof(IValidator.ValidateAsync),
				[
					validationContextType,
					typeof(CancellationToken)
				]);

			if (validateAsyncMethod is null)
			{
				continue;
			}

			var validationTask = (Task<FluentValidation.Results.ValidationResult>?)validateAsyncMethod.Invoke(
				validator,
				[
					validationContext,
					context.HttpContext.RequestAborted
				]);

			if (validationTask is null)
			{
				continue;
			}

			var result = await validationTask;

			if (!result.IsValid)
			{
				var errors = result.Errors
					.GroupBy(error => error.PropertyName)
					.ToDictionary(
						group => group.Key,
						group => group.Select(error => error.ErrorMessage).ToArray());

				context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors)
				{
					Title = "One or more validation errors occurred.",
					Status = StatusCodes.Status400BadRequest,
					Detail = "See the errors property for details.",
					Instance = context.HttpContext.Request.Path
				});

				return;
			}
		}

		await next();
	}
}
