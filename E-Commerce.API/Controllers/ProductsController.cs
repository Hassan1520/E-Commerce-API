using ECommerce.API.Helpers;
using ECommerce.Application.Common;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Features.Products.Commands.CreateProduct;
using ECommerce.Application.Features.Products.Commands.DeleteProduct;
using ECommerce.Application.Features.Products.Commands.DeleteProductImage;
using ECommerce.Application.Features.Products.Commands.UpdateProduct;
using ECommerce.Application.Features.Products.Commands.UpdateProductImage;
using ECommerce.Application.Features.Products.Queries.GetAllProducts;
using ECommerce.Application.Features.Products.Queries.GetProductById;
using ECommerce.Application.Features.Products.Queries.GetProductPage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("catalog-policy")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAll()
    {
        var products = await mediator.Send(new GetAllProductsQuery());
        return Ok(ApiResponse<IEnumerable<ProductDto>>.Ok(products));
    }

    [HttpGet("with-Pagination")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PaginationResult<ProductDto>>>> GetAll([FromQuery] ProductSpecParams @params)
    {
        var pagedProducts = await mediator.Send(new GetProductsPagedQuery(@params));
        return Ok(ApiResponse<PaginationResult<ProductDto>>.Ok(pagedProducts));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id)
    {
        var product = await mediator.Send(new GetProductByIdQuery(id));
        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto dto)
    {
        var product = await mediator.Send(new CreateProductCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = product.Id },
            ApiResponse<ProductDto>.Ok(product, "Product created successfully."));
    }

    [HttpPost("{id:int}/images")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductImageDto>>> UploadImage(int id, IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var result = await mediator.Send(new UploadProductImageCommand(id, stream, file.FileName, file.ContentType));
        return Ok(ApiResponse<ProductImageDto>.Ok(result, "Image uploaded successfully."));
    }

    [HttpDelete("{id:int}/images/{imageId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> DeleteImage(int id, int imageId)
    {
        await mediator.Send(new DeleteProductImageCommand(id, imageId));
        return Ok(ApiResponse.Ok("Image deleted successfully."));
    }

    [HttpPut("{id:int}/images/{imageId:int}/set-main")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> SetMainImage(int id, int imageId)
    {
        await mediator.Send(new SetMainProductImageCommand(id, imageId));
        return Ok(ApiResponse.Ok("Main image updated successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var product = await mediator.Send(new UpdateProductCommand(id, dto));
        return Ok(ApiResponse<ProductDto>.Ok(product, "Product updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        await mediator.Send(new DeleteProductCommand(id));
        return Ok(ApiResponse.Ok("Product deleted successfully."));
    }
}
