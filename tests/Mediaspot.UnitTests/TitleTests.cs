using Mediaspot.Domain.Titles;
using Shouldly;

namespace Mediaspot.UnitTests
{
    public class TitleTests
    {

        [Fact]
        public void Create_Should_Throw_When_Name_Is_Empty()
        {
            var action = () => Title.Create(
                string.Empty,
                "Description",
                new DateOnly(2024, 3, 1),
                TitleType.Movie);

            Should.Throw<ArgumentException>(action);
        }

        [Fact]
        public void Update_Should_Update_Title_Properties()
        {
            var title = Title.Create(
                "Star Wars",
                "Original description",
                new DateOnly(1977, 5, 25),
                TitleType.Movie);

            title.Update(
                "Star Wars: A New Hope",
                "Updated description",
                new DateOnly(1977, 5, 25),
                TitleType.Movie);

            title.Name.ShouldBe("Star Wars: A New Hope");
            title.Description.ShouldBe("Updated description");
            title.ReleaseDate.ShouldBe(new DateOnly(1977, 5, 25));
            title.Type.ShouldBe(TitleType.Movie);
        }

    }
}
