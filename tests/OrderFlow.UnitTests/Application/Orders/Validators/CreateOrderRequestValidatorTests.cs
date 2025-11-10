using FluentValidation.TestHelper;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Orders.Validators;

namespace OrderFlow.UnitTests.Application.Orders.Validators;

public class CreateOrderRequestValidatorTests
{
    private const int MaxProductNameLength = 100;
    private readonly CreateOrderRequestValidator _sut = new();

    [Fact]
    public void Validate_ShouldHaveValidationErrorForProductName_WhenProductNameIsEmpty()
    {
        const int quantity = 1;
        var productName = string.Empty;
        var model = new CreateOrderRequest(productName, quantity);

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ProductName)
            .WithErrorMessage("Product name is required");
    }
    
    [Fact]
    public void Validate_ShouldHaveValidationErrorForProductName_WhenProductNameIsTooLong()
    {
        const int quantity = 1;
        var productName = new string('A', MaxProductNameLength + 50);
        var model = new CreateOrderRequest(productName, quantity);

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ProductName)
            .WithErrorMessage("Product name must not exceed 100 characters");
    }
    
    [Fact]
    public void Validate_ShouldHaveValidationErrorForQuantity_WhenQuantityIsNegative()
    {
        const int quantity = -1;
        var productName = new  string('A', MaxProductNameLength);
        var model = new CreateOrderRequest(productName, quantity);

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than 0");
    }
    
    [Fact]
    public void Validate_ShouldNotHaveValidationError_WhenModelIsValid()
    {
        const int quantity = 10;
        var productName = new  string('A', MaxProductNameLength);
        var model = new CreateOrderRequest(productName, quantity);

        var result = _sut.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }
}