using Bogus;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Infrastructure.Persistence.Seeders
{
    public static class ItemSeeder
    {

        public static IReadOnlyList<Item> Generate(int seed = 20260908)
        {
            var faker = new Faker("en");
            faker.Random = new Randomizer(seed);

            return SeedData
                .Select(data =>
                {
                    var minimumStock = data.Unit == UnitOfMeasure.Unit
                        ? faker.Random.Decimal(10m, 100m)
                        : faker.Random.Decimal(100m, 1000m);

                    return new Item(
                        faker.Random.Guid(),
                        data.Code,
                        data.Name,
                        data.Unit,
                        decimal.Round(minimumStock, 2),
                        faker.Random.Int(14, 90));
                })
                .ToList();
        }
        private static readonly IReadOnlyList<(string Name, string Code, UnitOfMeasure Unit)>
        SeedData =
        [
            ("CONSTELLATION® dPCR Master Mix 2X", "DPCR-MM-001", UnitOfMeasure.Milliliter),
        ("Nuclease-Free Water (PCR Grade)", "DPCR-NFW-002", UnitOfMeasure.Milliliter),
        ("dPCR Partitioning Oil", "DPCR-OIL-003", UnitOfMeasure.Milliliter),
        ("Positive Control DNA Standard", "DPCR-STD-004", UnitOfMeasure.Vial),
        ("FAM-labeled TaqMan Probe", "DPCR-PRB-005", UnitOfMeasure.Vial),

        ("Mantis® High Volume Silicone Chip", "MF-MANT-001", UnitOfMeasure.Unit),
        ("Mantis® Low Volume PFE Chip", "MF-MANT-002", UnitOfMeasure.Unit),
        ("µPulse® TFF Concentration Chip", "MF-TFF-003", UnitOfMeasure.Unit),
        ("CONSTELLATION® 96-Well Microplate", "MF-PLT-004", UnitOfMeasure.Unit),
        ("Tempest® 96-Channel Dispense Head", "MF-TMP-005", UnitOfMeasure.Unit),

        ("DMEM High Glucose Cell Media", "CELL-MED-001", UnitOfMeasure.Milliliter),
        ("Fetal Bovine Serum (FBS)", "CELL-FBS-002", UnitOfMeasure.Milliliter),
        ("Trypsin-EDTA 0.25%", "CELL-TRY-003", UnitOfMeasure.Milliliter),
        ("Puroxia® Hydrogen Peroxide (H2O2) 35%", "CELL-PRX-004", UnitOfMeasure.Milliliter),
        ("Penicillin-Streptomycin Solution", "CELL-PEN-005", UnitOfMeasure.Milliliter),

        ("LCP Monoolein Substrate (99%)", "XTAL-LCP-001", UnitOfMeasure.Gram),
        ("PEG 3350 Crystallization Screen Kit", "XTAL-SCR-002", UnitOfMeasure.Vial),
        ("NT8® 96-Well Hanging Drop Plate", "XTAL-PLT-003", UnitOfMeasure.Unit),
        ("HEPES Buffer Solution pH 7.5", "XTAL-BUF-004", UnitOfMeasure.Milliliter),
        ("Ammonium Sulfate Precipitant", "XTAL-PPT-005", UnitOfMeasure.Milliliter)
        ];

    }
}