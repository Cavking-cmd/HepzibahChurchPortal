namespace ChurchPortal.Services.Interfaces;
using ChurchPortal.DataContext;
using global::ChurchPortal.Core.Entities;
using Microsoft.EntityFrameworkCore;


public static class FellowshipCenterSeeder
{
    private sealed record SeedCenter(string GridLetter, string CenterName, string Zone, string LeaderName, string Location, string? Description);

    private static readonly SeedCenter[] Centers =
    {
        new("A", "131 Le Marie", "A", "House Host", "131 Le Marie", "5:00PM fellowship meeting"),
        new("B", "406 Munroe Avenue", "B", "House Host", "406 Munroe Avenue", "5:00PM-6:00PM fellowship meeting"),
        new("C", "668 Linden Avenue", "C", "House Host", "668 Linden Avenue", "5:00PM-6:00PM fellowship meeting; meeting id: 74192820092; meeting password: 708wTF"),
        new("D", "111-365 Thames Avenue", "D", "House Host", "111-365 Thames Avenue", "5:00PM-6:00PM fellowship meeting; meeting id: 8115724434; meeting password: 841806"),
        new("E", "399 Big bluestem Road", "E", "House Host", "399 Big bluestem Road", "5:00PM-6:00PM fellowship meeting; meeting id: 3560854104; meeting password: 653054"),
        new("F", "804 Wayoata Street", "F", "House Host", "804 Wayoata Street", "5:00PM-6:00PM fellowship meeting; meeting id: 4059760495; meeting password: 029326"),
        new("G", "Attridge Lane center", "G", "House Host", "Attridge Lane", "6:00PM fellowship meeting; meeting id: 85264166493; meeting password: grace"),
        new("H", "Attridge Lane Center", "H", "House Host", "Attridge Lane", "4:00PM-5:00PM fellowship meeting; meeting id: 74161832393; meeting password: Es5eFz"),
        new("I", "19 Brereton Road", "I", "House Host", "19 Brereton Road", "5:00PM-6:00PM fellowship meeting"),
        new("J", "28 Dr. Michael Grace lane", "J", "House Host", "28 Dr. Michael Grace lane", "5:00PM-6:00PM fellowship meeting"),
        new("K", "Solution Center", "K", "House Host", "22 Beddington Street", "5:00PM-6:00PM fellowship meeting; meeting id: 71593539972; meeting password: 871138"),
        new("L", "33 Hargrave Street", "L", "House Host", "33 Hargrave Street", "5:00PM fellowship meeting"),
        new("M", "House of Praise", "M", "House Host", "223 Larsen Avenue", "5:00PM-6:00PM fellowship meeting"),
        new("N", "Jedidah center", "N", "House Host", "174 Eau-Claire Drive", "6:00PM-7:00PM fellowship meeting"),
        new("O", "Tanager trail center", "O", "House Host", "294 Tanager trail", "6:00PM-7:00PM fellowship meeting"),
        new("P", "201 Ravenhurt Street", "P", "House Host", "201 Ravenhurt Street", "6:00PM-7:00PM fellowship meeting")
    };

    public static async Task SeedAsync(ChurchPortalDbContext context)
    {
        var existing = await context.FellowshipCenters
            .AsNoTracking()
            .ToListAsync();

        var waiting = new List<FellowshipCenter>(existing);
        var matchedIds = new HashSet<Guid>();

        foreach (var seed in Centers)
        {
            FellowshipCenter? match = null;

            foreach (var candidate in waiting)
            {
                if (!matchedIds.Contains(candidate.Id) &&
                    NormalizeForMatch(candidate.CenterName) == NormalizeForMatch(seed.CenterName) &&
                    NormalizeForMatch(candidate.Location) == NormalizeForMatch(seed.Location))
                {
                    match = candidate;
                    waiting.Remove(candidate);
                    break;
                }
            }

            if (match != null)
            {
                matchedIds.Add(match.Id);
                match.CenterName = seed.CenterName;
                match.Zone = seed.Zone;
                match.LeaderName = seed.LeaderName;
                match.Location = seed.Location;
                match.Description = seed.Description;
                context.FellowshipCenters.Update(match);
            }
            else
            {
                await context.FellowshipCenters.AddAsync(new FellowshipCenter
                {
                    CenterName = seed.CenterName,
                    Zone = seed.Zone,
                    LeaderName = seed.LeaderName,
                    Location = seed.Location,
                    Description = seed.Description
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static string NormalizeForMatch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Concat(value.Trim().ToUpperInvariant()
            .Where(c => char.IsLetterOrDigit(c)));
    }
}
