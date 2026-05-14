using Domain.Entities;
using Infranstructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infrastructure.Persistence
{
    public class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.Drugs.Any())
            {
                var drugsData = File.ReadAllText(
                    "../Infrastructure/Persistence/Seed/drugs_seed.json");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var drugs = JsonSerializer.Deserialize<List<Drug>>(drugsData, options);

                if (drugs != null && drugs.Count > 0)
                {
                    foreach (var drug in drugs)
                    {
                        // IMPORTANT FIX
                        drug.PrescriptionItems = new HashSet<PrescriptionItem>();

                        // SAFETY CHECK
                        if (string.IsNullOrWhiteSpace(drug.Name))
                            continue;

                        context.Drugs.Add(drug);
                    }

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}