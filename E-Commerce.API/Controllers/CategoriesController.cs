using ECommerce.API.Helpers;
using ECommerce.Application.DTOs.Categories;
using ECommerce.Application.Features.Categories.Commands.CreateCategorie;
using ECommerce.Application.Features.Categories.Commands.DeleteCategorie;
using ECommerce.Application.Features.Categories.Commands.UpdateCategorie;
using ECommerce.Application.Features.Categories.Queries.GetAllCategories;
using ECommerce.Application.Features.Categories.Queries.GetCategorieById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("catalog-policy")]
public class CategoriesController(IMediator mediator) : ControllerBase
{
    [OutputCache(Duration = 300)]
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll()
    {
        var categories = await mediator.Send(new GetAllCategoriesQuery());
        return Ok(ApiResponse<IEnumerable<CategoryDto>>.Ok(categories));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(int id)
    {
        var category = await mediator.Send(new GetCategoryByIdQuery(id));
        return Ok(ApiResponse<CategoryDto>.Ok(category));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CreateCategoryDto dto)
    {
        var category = await mediator.Send(new CreateCategoryCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = category.Id },
            ApiResponse<CategoryDto>.Ok(category, "Category created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        var category = await mediator.Send(new UpdateCategoryCommand(id, dto));
        return Ok(ApiResponse<CategoryDto>.Ok(category, "Category updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        await mediator.Send(new DeleteCategoryCommand(id));
        return Ok(ApiResponse.Ok("Category deleted successfully."));
    }
}
