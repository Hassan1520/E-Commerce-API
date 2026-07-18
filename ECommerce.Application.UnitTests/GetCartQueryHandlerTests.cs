using Moq;
using FluentAssertions;
using AutoMapper;
using ECommerce.Application.Features.Cart.Queries.GetCart;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Domain.Entities;
using ECommerce.Application.DTOs.Cart;

namespace ECommerce.Application.UnitTests.Features.Cart.Queries;

public class GetCartQueryHandlerTests
{
    // 1. بنعرف الـ Fields للـ Mocks والـ Handler اللي هنختبره
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetCartQueryHandler _handler;

    public GetCartQueryHandlerTests()
    {
        // 2. بنعمل Initialize للـ Mocks
        _cartRepositoryMock = new Mock<ICartRepository>();
        _mapperMock = new Mock<IMapper>();

        // 3. بنحقن الـ الأوبجكتس الوهمية (.Object) جوه الـ Handler الرئيسي
        _handler = new GetCartQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);

    }
    [Fact] // بتقول للـ xUnit إن دي ميثود اختبار
    public async Task Handle_ShouldThrowNotFoundException_WhenCartIsEmpty()
    {
        // --- 1. Arrange (تجهيز البيانات الوهمية والشرط المحاكي) ---
        var userId = 10; // افترضنا أي رقم ID
        var query = new GetCartQuery(userId);

        // بنبرمج الـ Mock: "لو اتناديت بـ userId ده، رجع لستة فاضية"
        _cartRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<CartItem>()); // لستة فاضية

        // --- 2. Act (تنفيذ الكود الفعلي اللي بنختبره) ---
        // بما إننا متوقعين إيرور، بنحفظ العملية جوه Func مش بنعمل لها await علطول
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // --- 3. Assert (التأكد من النتيجة باستخدام FluentAssertions) ---
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Cart is empty.");
    }
    [Fact]
    public async Task Handle_ShouldReturnCartDto_WhenCartHasItems()
    {
        // --- 1. Arrange ---
        var userId = 10;
        var query = new GetCartQuery(userId);

        // بنجهز لستة فيها عنصر وهمي عشان نعدي الـ if بتاعة الـ count == 0
        var fakeCartItems = new List<CartItem>
        {
            new CartItem { Id = 1, ProductId = 5, Quantity = 2, UserId = userId }
        };

        var fakeCartDto = new CartDto
        {
            // حط هنا الـ Properties اللي جوه الـ CartDto عندك، مثلاً:
            Items = new List<CartItemDto> { new CartItemDto { ProductId = 5, Quantity = 2 } },
        };

        // بنبرمج الـ Repositoy يرجع اللستة
        _cartRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(fakeCartItems);

        // بنبرمج الـ AutoMapper: "لما يجيلك لستة الـ Items دي، رجع الـ fakeCartDto الكبير"
        _mapperMock
            .Setup(mapper => mapper.Map<CartDto>(It.IsAny<List<CartItem>>()))
            .Returns(fakeCartDto);

        // --- 2. Act ---
        var result = await _handler.Handle(query, CancellationToken.None);

        // --- 3. Assert ---
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().ProductId.Should().Be(5);

        // بنتأكد إن الـ Repository والـ Mapper اتنادوا فعلاً مرة واحدة بس (Verification)
        _cartRepositoryMock.Verify(repo => repo.GetByUserIdAsync(userId), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<CartDto>(It.IsAny<List<CartItem>>()), Times.Once);
    }

}
