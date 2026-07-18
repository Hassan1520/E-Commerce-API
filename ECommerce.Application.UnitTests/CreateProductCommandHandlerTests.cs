using AutoMapper;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Features.Products.Commands.CreateProduct;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace ECommerce.Application.Tests.Features.Products;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _handler = new CreateProductCommandHandler(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var command = new CreateProductCommand(new CreateProductDto
        {
            Name = "Test Product",
            CategoryId = 999
        });

        _unitOfWorkMock
            .Setup(u => u.Categories.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>() ) )
            .ReturnsAsync(false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999*");

        // تأكد إننا ماعملناش أي إضافة للمنتج لو الفئة مش موجودة
        _unitOfWorkMock.Verify(u => u.Products.AddAsync(It.IsAny<Product>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateProduct_WhenCategoryExists()
    {
        // Arrange
        var dto = new CreateProductDto { Name = "Wireless Mouse", CategoryId = 1, Price = 250 };
        var command = new CreateProductCommand(dto);

        var mappedProduct = new Product { Id = 0, Name = dto.Name, CategoryId = dto.CategoryId };
        var createdProductWithCategory = new Product { Id = 42, Name = dto.Name, CategoryId = dto.CategoryId };
        var expectedDto = new ProductDto { Id = 42, Name = dto.Name };

        _unitOfWorkMock
            .Setup(u => u.Categories.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>() ) )
            .ReturnsAsync(true);

        _mapperMock.Setup(m => m.Map<Product>(dto)).Returns(mappedProduct);
        _mapperMock.Setup(m => m.Map<ProductDto>(createdProductWithCategory)).Returns(expectedDto);

        _unitOfWorkMock
            .Setup(u => u.Products.GetByIdWithCategoryAsync(mappedProduct.Id))
            .ReturnsAsync(createdProductWithCategory);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(42);
        result.Name.Should().Be("Wireless Mouse");

        _unitOfWorkMock.Verify(u => u.Products.AddAsync(mappedProduct), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync(ECommerce.Application.Common.CacheKeys.AllProducts), Times.Once);
    }
}