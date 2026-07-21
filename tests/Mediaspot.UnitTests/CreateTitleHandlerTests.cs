
using Mediaspot.Application.Assets.Commands.Create;
using Mediaspot.Application.Common;
using Mediaspot.Application.Titles.Commands.CreateTitle;
using Mediaspot.Domain.Assets;
using Mediaspot.Domain.Titles;
using Moq;
using Shouldly;


namespace Mediaspot.UnitTests
{
    public class CreateTitleHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Throw_When_Title_Name_Already_Exists()
        {
            var repo = new Mock<ITitleRepository>();
            var uow = new Mock<IUnitOfWork>();

            var existingTitle = Title.Create(
                "Star Wars",
                "Existing movie",
                new DateOnly(1977, 5, 25),
                TitleType.Movie);

            repo
                .Setup(r => r.GetByNameAsync(
                    "Star Wars",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTitle);

            var handler = new CreateTitleCommandHandler(
                repo.Object,
                uow.Object);

            var command = new CreateTitleCommand(
                "Star Wars",
                "Another movie",
                new DateOnly(2026, 1, 1),
                TitleType.Movie);

            await Should.ThrowAsync<InvalidOperationException>(
                () => handler.Handle(command, CancellationToken.None));

            repo.Verify(
                r => r.AddTitleAsync(
                    It.IsAny<Title>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            uow.Verify(
                u => u.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_Should_Add_Title_Save_Changes_And_Return_Id()
        {
            var repo = new Mock<ITitleRepository>();
            var uow = new Mock<IUnitOfWork>();

            repo.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Title?)null);
            repo.Setup(r => r.AddTitleAsync(It.IsAny<Title>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new CreateTitleCommandHandler(repo.Object, uow.Object);
            var cmd = new CreateTitleCommand("Star Wars : The Phantom Menace", "Science-fiction movie", new DateOnly(1999, 10, 13), TitleType.Movie);

            var result = await handler.Handle(cmd, CancellationToken.None);

            result.ShouldNotBe(Guid.Empty);

            repo.Verify(
                r => r.GetByNameAsync(
                    cmd.Name,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repo.Verify(
                r => r.AddTitleAsync(
                    It.Is<Title>(title =>
                    title.Id == result &&
                    title.Name == cmd.Name &&
                    title.Description == cmd.Description &&
                    title.ReleaseDate == cmd.ReleaseDate &&
                    title.Type == cmd.Type),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            uow.Verify(
                u => u.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
