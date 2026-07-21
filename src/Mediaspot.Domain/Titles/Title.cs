using Mediaspot.Domain.Common;

namespace Mediaspot.Domain.Titles
{
    public class Title : AggregateRoot
    {
        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public DateOnly? ReleaseDate { get; private set; }

        public TitleType Type { get; private set; }

        // Private constructor to prevent uncontrolled instanciation
        private Title()
        {
        }

        // Factory method used to create a valid title instance,
        // centralise entity initialisation and ensure domain invariance are enforced a creation time 
        public static Title Create(
          string name,
          string? description,
          DateOnly? releaseDate,
          TitleType type)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Title name cannot be empty.",
                    nameof(name));
            }

            return new Title
            {
                Name = name.Trim(),
                Description = description,
                ReleaseDate = releaseDate,
                Type = type
            };
        }

        // Update the state of title while preserving domain invariance 
        public void Update(
        string name,
        string? description,
        DateOnly? releaseDate,
        TitleType type)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Title name cannot be empty.",
                    nameof(name));
            }

            Name = name.Trim();
            Description = description;
            ReleaseDate = releaseDate;
            Type = type;
        }
    }
}
