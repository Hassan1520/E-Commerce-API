using MediatR;

namespace ECommerce.Application.Features.Categories.Commands.DeleteCategorie;

public record DeleteCategoryCommand(int Id) : IRequest<Unit>;