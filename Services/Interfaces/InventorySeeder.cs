namespace ChurchPortal.Services.Interfaces;
using ChurchPortal.DataContext;
using global::ChurchPortal.Core.Entities;
using Microsoft.EntityFrameworkCore;


public static class InventorySeeder
{
    public static async Task SeedAsync(ChurchPortalDbContext context)
    {
        var defaultPurchaseDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var items = new List<InventoryItem>
        {
            new InventoryItem
            {
                ItemName = "Yamaha Piano",
                Description = "With Yorkville piano pedal AFP7",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 2200m,
                SerialNumber = "UCBL01044",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Yamaha Piano",
                Description = "With Yamaha piano pedal FC4A",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 2500m,
                SerialNumber = "CANBY01045",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Lead guitar chair",
                Description = "Main auditorium stage",
                Category = "Furniture",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 150m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Lead guitar synthesizer",
                Description = "Guitar effects processor",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 300m,
                SerialNumber = "R2N7811",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Lead guitar synthesizer",
                Description = "Model: Marshall",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 500m,
                SerialNumber = "VOLL5EE9AC",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Guitar mic",
                Description = "Wired Mic",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 150m,
                SerialNumber = "SM57",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Guitar stand",
                Description = "for the lead guitar",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 60m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Sub-woofer speaker",
                Description = "Model: PSI5's Parasource",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 1200m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Yorkville Stage light",
                Description = "light on a stand",
                Category = "Lighting",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 300m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Speakers",
                Description = "Lead speaker monitor",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 900m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Television",
                Description = "Dolby audio - Floor on the Altar",
                Category = "Electronics",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 700m,
                SerialNumber = "P1086441N001495",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "In-ear monitor",
                Description = "XSW IEM - Keyboard",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 650m,
                SerialNumber = "0032101205",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "In-ear monitor",
                Description = "XSW IEM - Keyboard",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 650m,
                SerialNumber = "0142100863",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "In-ear monitor",
                Description = "XSW IEM - Lead guitar",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 650m,
                SerialNumber = "0172101071",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Mic Stand",
                Description = "Vocalist",
                Category = "Audio Equipment",
                Quantity = 2,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 70m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Mic Stand",
                Description = "lead guitar",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 70m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Speakers",
                Description = "Keyboard",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 700m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Backup speaker",
                Description = "feedback speaker",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 900m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Fan",
                Description = "Electric fan",
                Category = "Appliances",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 80m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Lead guitar",
                Description = "Model: Epiphone",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 600m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Distribution board",
                Description = "Model: DPDB",
                Category = "Electrical",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 250m,
                SerialNumber = "U200203161",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Distribution board",
                Description = "Model: DPDB",
                Category = "Electrical",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 250m,
                SerialNumber = "UI40201469",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Distribution board",
                Description = "Model: PDB",
                Category = "Electrical",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 250m,
                SerialNumber = "I4190701508",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Chairs",
                Description = "Red colours",
                Category = "Furniture",
                Quantity = 142,
                Location = "Auditorium",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 50m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Monitor",
                Description = "Black Samsung",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "Technical",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 250m,
                SerialNumber = "4PMFH7BR203444W",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Monitor",
                Description = "White Samsung",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "Technical",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 650m,
                SerialNumber = "CWSSH4ZTA00504L",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Monitor",
                Description = "Black HP",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "Technical",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 200m,
                SerialNumber = "6CM5490CVF",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Keyboard",
                Description = "Black HP",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "Technical",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 50m,
                SerialNumber = "697737-DB1",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Keyboard",
                Description = "Black",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "Technical",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 40m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Mixer",
                Description = "WING Personal Mixing Console",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Technical",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 2500m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Digital Receiver (Wireless)",
                Description = "Teradek",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Technical",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 1000m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Digital Receiver (Wireless)",
                Description = "Mars 400s Pro",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Technical",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 1500m,
                SerialNumber = "002052RB00B729",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Switcher",
                Description = "Black Magic Design",
                Category = "Video Equipment",
                Quantity = 1,
                Location = "Technical",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 1800m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Camera Stand",
                Description = "Neewer Ultimax",
                Category = "Video Equipment",
                Quantity = 1,
                Location = "Technical",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 180m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Camera Stand",
                Description = "Mayotto",
                Category = "Video Equipment",
                Quantity = 1,
                Location = "Technical",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 100m,
                SerialNumber = "RE214226",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Vacuum",
                Description = "Purple",
                Category = "Appliances",
                Quantity = 1,
                Location = "Storage room",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 180m,
                SerialNumber = "21267103CJB",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Konga drums",
                Description = "Brown",
                Category = "Musical Instruments",
                Quantity = 2,
                Location = "Storage room",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 350m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Long Tables",
                Description = "White",
                Category = "Furniture",
                Quantity = 20,
                Location = "Storage room",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 180m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Short Tables",
                Description = "White",
                Category = "Furniture",
                Quantity = 5,
                Location = "Storage room",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 150m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Kiddy table",
                Description = "White",
                Category = "Furniture",
                Quantity = 14,
                Location = "Storage room",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 100m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Metal Pulpit",
                Description = "Black",
                Category = "Furniture",
                Quantity = 1,
                Location = "Storage room",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 400m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Air Conditioner",
                Description = "White Hisense",
                Category = "Appliances",
                Quantity = 2,
                Location = "Storage room",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 1000m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Vacuum",
                Description = "Red",
                Category = "Appliances",
                Quantity = 1,
                Location = "Storage room",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 180m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Vacuum",
                Description = "Blue",
                Category = "Appliances",
                Quantity = 1,
                Location = "Storage room",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 180m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Chairs",
                Description = "Fold-able Black Chairs",
                Category = "Furniture",
                Quantity = 222,
                Location = "Children auditorium",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 35m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Chairs",
                Description = "Red with fabric cover",
                Category = "Furniture",
                Quantity = 47,
                Location = "Children auditorium",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 60m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Drumset",
                Description = "5 piece",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Children auditorium",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 700m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Television",
                Description = "Black - hung on the wall",
                Category = "Electronics",
                Quantity = 1,
                Location = "Children auditorium",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 700m,
                SerialNumber = "40G111101H01380",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Speaker",
                Description = "Black - Top left corner",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Children auditorium",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 250m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Piano",
                Description = "Black",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Children auditorium",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 1000m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Wall clock",
                Description = "White",
                Category = "Furniture",
                Quantity = 1,
                Location = "Children auditorium",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 40m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Coat hanger",
                Description = "Black",
                Category = "Furniture",
                Quantity = 1,
                Location = "Children auditorium",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 50m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Fire extinguisher",
                Description = "Red",
                Category = "Safety Equipment",
                Quantity = 1,
                Location = "Children auditorium",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 100m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Water dispenser",
                Description = "Black",
                Category = "Appliances",
                Quantity = 1,
                Location = "Children auditorium",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 180m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "BBQ Grill",
                Description = "Black",
                Category = "Appliances",
                Quantity = 1,
                Location = "Storage room",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 300m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Kid Chairs",
                Description = "Green",
                Category = "Furniture",
                Quantity = 16,
                Location = "Children auditorium",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 30m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Kid Chairs",
                Description = "Blue",
                Category = "Furniture",
                Quantity = 16,
                Location = "Children auditorium",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 30m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Kid Chairs",
                Description = "Red",
                Category = "Furniture",
                Quantity = 4,
                Location = "Children auditorium",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 30m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Drum set",
                Description = "6 piece drum, Drum chair with 4   Sabian Cymbals/crash and Toca Jingle",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 1800m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Drum mics",
                Description = "drumset",
                Category = "Musical Instruments",
                Quantity = 2,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 150m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Drum mics",
                Description = "drumset",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 120m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Drum mics",
                Description = "drumset",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 250m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Drum mics",
                Description = "drumset",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 80m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Konga drum",
                Description = "drumset",
                Category = "Musical Instruments",
                Quantity = 2,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 350m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Drum mics",
                Description = "for Konga drum",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 80m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Speakers",
                Description = "lead monitor speaker",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 900m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Speakers",
                Description = "Sub speaker",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 1000m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Speakers",
                Description = "Backup monitor speaker",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 800m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Speakers",
                Description = "Bass combo speaker",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 600m,
                SerialNumber = "UD2140007104546",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Lights",
                Description = "4 lights on stand",
                Category = "Lighting",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 500m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Microphone",
                Description = "Talk back mic for Bass guitarist",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 150m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Mic Stand",
                Description = "For talk back mic for Bass guitarist",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 70m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Guitar stand",
                Description = "for the bass guitar",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 60m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Guitarist chair",
                Description = "for the bass guitar  player",
                Category = "Furniture",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 150m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Cabinet",
                Description = "Model: Samson",
                Category = "Furniture",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 300m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Receiver - shure",
                Description = "in the cabinet",
                Category = "Audio Equipment",
                Quantity = 4,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 400m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Receiver - shure",
                Description = "in the cabinet",
                Category = "Audio Equipment",
                Quantity = 2,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 500m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Receiver - shure",
                Description = "in the cabinet",
                Category = "Audio Equipment",
                Quantity = 2,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 650m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Receiver - EW Digital Siennheiser",
                Description = "in the cabinet",
                Category = "Audio Equipment",
                Quantity = 2,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 600m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Head Amp 6",
                Description = "in the cabinet",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 500m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Receiver - Siennheiser",
                Description = "in the cabinet",
                Category = "Audio Equipment",
                Quantity = 2,
                Location = "Stage/right",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 650m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Desktop Computer (All-In-One)",
                Description = "With wireless mouse and keyboard",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "Library",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 900m,
                SerialNumber = "00342-20879-00025-AAOEM",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Desktop Computer (All-In-One)",
                Description = "With wireless mouse and keyboard",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "Library",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 900m,
                SerialNumber = "00342-20879-00221-AAOEM",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Desktop Computer (All-In-One)",
                Description = "With wireless mouse and keyboard",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "Library",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 900m,
                SerialNumber = "00342-20879-00250-AADEM",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Desktop Computer (All-In-One)",
                Description = "With wireless mouse and keyboard",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "Library",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 900m,
                SerialNumber = "00342-20879-00023-AADEM",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Desktop Computer (All-In-One)",
                Description = "With wireless mouse and keyboard",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "Library",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 900m,
                SerialNumber = "00342-20879-00128-AAOEM",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Fan/cooling",
                Description = "Model: Honeywell",
                Category = "Appliances",
                Quantity = 1,
                Location = "Library",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 100m,
                SerialNumber = "HF910C",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Television",
                Description = "Mounted on a stand with remote",
                Category = "Electronics",
                Quantity = 1,
                Location = "Library",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 500m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Printer",
                Description = "HP Laser jet pro",
                Category = "Office Equipment",
                Quantity = 1,
                Location = "Library",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 600m,
                SerialNumber = "CNCRQ8TBWC",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Cabinet",
                Description = "Holds library book",
                Category = "Furniture",
                Quantity = 1,
                Location = "Library",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 300m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "Chairs",
                Description = "Black chairs with leather cover",
                Category = "Furniture",
                Quantity = 1,
                Location = "Library",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 150m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "ROOM FRIDGE",
                Description = "Model: DARBY",
                Category = "Appliances",
                Quantity = 1,
                Location = "PLA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 700m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "COFFEE MAKER",
                Description = "Model: KEURIG 2.0",
                Category = "Appliances",
                Quantity = 1,
                Location = "PLA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 100m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "STANDING FAN",
                Description = "Model: SEVILLE",
                Category = "Appliances",
                Quantity = 1,
                Location = "PLA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 100m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "HEATER",
                Description = "Model: BLONARE",
                Category = "Appliances",
                Quantity = 1,
                Location = "PLA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 100m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "TELEVISION",
                Description = "Model: LG 42\"",
                Category = "Electronics",
                Quantity = 1,
                Location = "PLA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 500m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "INTERNET ROWTER",
                Description = "Model: SHAW",
                Category = "Networking",
                Quantity = 1,
                Location = "PLA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 200m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "PRINTER",
                Description = "Model: CANON G 4210",
                Category = "Office Equipment",
                Quantity = 1,
                Location = "PLA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 250m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "HP DESKTOR",
                Description = "Model: HP 24''",
                Category = "General Equipment",
                Quantity = 1,
                Location = "PLA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 600m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "DESK PHONE",
                Description = "Model: AT & T CELLULAR PHONE PPT",
                Category = "Office Equipment",
                Quantity = 1,
                Location = "PLA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 80m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "POS MACHINE",
                Description = "Model: CLOVER",
                Category = "Office Equipment",
                Quantity = 1,
                Location = "PLA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 500m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "NOTE COUNTER (RS)",
                Description = "Model: RBC 3200 - CA",
                Category = "Office Equipment",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 300m,
                SerialNumber = "K1610LC12924",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "LG MONITOR",
                Description = "Model: 3260TM",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 500m,
                SerialNumber = "910NTLEA0616",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "CASIO CALCULATOR",
                Description = "Model: HR-170RC",
                Category = "Office Equipment",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 60m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "CASIO CALCULATOR",
                Description = "Model: HR-150TM",
                Category = "Office Equipment",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 50m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "ACER CPU",
                Description = "Model: TC-886-ER12",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 500m,
                SerialNumber = "DTBDCAA0020190EB99600",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "CANON PRINTER",
                Description = "Model: TS3320",
                Category = "Office Equipment",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 100m,
                SerialNumber = "KMFR1172",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "HITRON MODEM",
                Description = "Model: CODA-4582",
                Category = "Networking",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 150m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "CISCO FIREWALL",
                Description = "No additional description provided",
                Category = "Networking",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 500m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "ARIS MODEM",
                Description = "No additional description provided",
                Category = "Networking",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 100m,
                SerialNumber = "TM6026/P2/NA",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "HDMI SPLITTER",
                Description = "No additional description provided",
                Category = "Video Equipment",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 60m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "GREENET HUB",
                Description = "No additional description provided",
                Category = "Networking",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 100m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "PHIHONG POE ADAPTER",
                Description = "Model: POE3IU-1AT",
                Category = "Networking",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 40m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "HONEYWELL FAN",
                Description = "No additional description provided",
                Category = "Appliances",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 80m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "DESK PHONE",
                Description = "Model: AT & T CELLULAR PHONE PPT",
                Category = "Office Equipment",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 80m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "NOMA POWER EXTENDER",
                Description = "No additional description provided",
                Category = "Electrical",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 50m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "BRADFORD WATER HEATER",
                Description = "Model: M265R8DS-INCWW",
                Category = "Appliances",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 900m,
                SerialNumber = "HH15437275",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "HP SCANNER",
                Description = "Model: HP SCANJET 4370",
                Category = "Office Equipment",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 150m,
                SerialNumber = "CN615A25CP",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "ACER KEYBOARD",
                Description = "Model: KBCR21",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 40m,
                SerialNumber = "DKUSB1POHX",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "ACER MOUSE",
                Description = "No additional description provided",
                Category = "Computer Equipment",
                Quantity = 1,
                Location = "PEA OFFICE",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 25m,
                SerialNumber = "DS11211021",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "TELEVISION",
                Description = "Model: LG 55\"",
                Category = "Electronics",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 800m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "BASS GUITAR",
                Description = "No additional description provided",
                Category = "Musical Instruments",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 600m,
                SerialNumber = "I190121915",
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
            new InventoryItem
            {
                ItemName = "COMBO SPEAKERS (LEAD)",
                Description = "Model: MARSHAL (DSL20)",
                Category = "Audio Equipment",
                Quantity = 1,
                Location = "Main auditorium stage",
                Condition = ItemCondition.Good,
                PurchaseDate = defaultPurchaseDate,
                Value = 600m,
                SerialNumber = null,
                Custodian = "Church Admin",
                LastVerifiedDate = null
            },
        };

        var existing = await context.InventoryItems
            .AsNoTracking()
            .OrderBy(x => x.CreatedDate)
            .ToListAsync();

        var bySerial = existing
            .Where(x => !string.IsNullOrWhiteSpace(x.SerialNumber))
            .ToDictionary(x => x.SerialNumber!.Trim());

        var waiting = new List<InventoryItem>(existing);
        var matchedIds = new HashSet<Guid>();

        foreach (var seed in items)
        {
            InventoryItem? match = null;

            if (!string.IsNullOrWhiteSpace(seed.SerialNumber) && bySerial.TryGetValue(seed.SerialNumber.Trim(), out var serialMatch))
            {
                match = serialMatch;
            }
            else
            {
                foreach (var candidate in waiting)
                {
                    if (!matchedIds.Contains(candidate.Id) &&
                        NormalizeForMatch(candidate.ItemName) == NormalizeForMatch(seed.ItemName) &&
                        NormalizeForMatch(candidate.Location) == NormalizeForMatch(seed.Location) &&
                        NormalizeForMatch(candidate.Description) == NormalizeForMatch(seed.Description))
                    {
                        match = candidate;
                        waiting.Remove(candidate);
                        break;
                    }
                }
            }

            if (match != null)
            {
                matchedIds.Add(match.Id);
                match.ItemName = seed.ItemName;
                match.Category = seed.Category;
                match.Quantity = seed.Quantity;
                match.Location = seed.Location;
                match.Condition = seed.Condition;
                match.PurchaseDate = seed.PurchaseDate;
                match.Value = seed.Value;
                match.SerialNumber = seed.SerialNumber;
                match.Description = seed.Description;
                match.LastVerifiedDate = seed.LastVerifiedDate;
                match.Custodian = seed.Custodian;
                context.InventoryItems.Update(match);
            }
            else
            {
                await context.InventoryItems.AddAsync(seed);
            }
        }

        await context.SaveChangesAsync();
    }

    private static string NormalizeForMatch(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return new string(value
            .Replace('\u00A0', ' ')
            .Where(c => !char.IsWhiteSpace(c))
            .ToArray())
            .ToLowerInvariant();
    }
}