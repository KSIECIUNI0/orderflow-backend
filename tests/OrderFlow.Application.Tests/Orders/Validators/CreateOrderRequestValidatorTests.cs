using FluentValidation.TestHelper;
using OrderFlow.Application.Orders.Validators;
using OrderFlow.Application.TestUtils.Builders.DTOs;

namespace OrderFlow.Application.Tests.Orders.Validators;

public class CreateOrderRequestValidatorTests
{
    private readonly CreateOrderRequestValidator _sut = new();
    
    [Fact]
    public void Validate_ShouldHaveValidationErrorForProductName_WhenProductNameIsEmpty()
    {
        // Arrange
        var model = new CreateOrderRequestBuilder()
            .WithEmptyProductName()
            .Build();

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductName)
            .WithErrorMessage("Product name is required");
    }
    
    [Fact]
    public void Validate_ShouldHaveValidationErrorForProductName_WhenProductNameIsTooLong()
    {
        // Arrange
        var model = new CreateOrderRequestBuilder()
            .WithTooLongProductName()
            .Build();

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductName)
            .WithErrorMessage("Product name must not exceed 100 characters");
    }
    
    [Fact]
    public void Validate_ShouldHaveValidationErrorForQuantity_WhenQuantityIsNegative()
    {
        // Arrange
        var model = new CreateOrderRequestBuilder()
            .WithNegativeQuantity()
            .Build();

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than 0");
    }
    
    [Fact]
    public void Validate_ShouldNotHaveValidationError_WhenModelIsValid()
    {
        // Arrange
        var model = new CreateOrderRequestBuilder().Build();

        // Act
        var result = _sut.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}