using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database.Configurations;

public static class AnimalBreedSeed
{
    private static readonly Guid DogId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid CatId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static AnimalBreed[] GetBreeds()
    {
        return
        [
            Create(1, DogId, "beagle", 1),
            Create(2, DogId, "bichon_frise", 2),
            Create(3, DogId, "maltese", 3),
            Create(4, DogId, "border_collie", 4),
            Create(5, DogId, "french_bulldog", 5),
            Create(6, DogId, "german_shepherd", 6),
            Create(7, DogId, "golden_retriever", 7),
            Create(8, DogId, "labrador_retriever", 8),
            Create(9, DogId, "poodle", 9),
            Create(10, DogId, "shih_tzu", 10),
            Create(11, DogId, "mixed_breed", 11),

            Create(101, CatId, "british_shorthair", 1),
            Create(102, CatId, "maine_coon", 2),
            Create(103, CatId, "persian", 3),
            Create(104, CatId, "ragdoll", 4),
            Create(105, CatId, "siamese", 5),
            Create(106, CatId, "sphynx", 6),
            Create(107, CatId, "mixed_breed", 7)
        ];
    }

    private static AnimalBreed Create(
        int number,
        Guid speciesId,
        string code,
        int sortOrder)
    {
        return new AnimalBreed
        {
            Id = Guid.Parse(
                $"aaaaaaaa-aaaa-aaaa-aaaa-{number:D12}"),
            AnimalSpeciesId = speciesId,
            Code = code,
            SortOrder = sortOrder,
            IsActive = true
        };
    }
}