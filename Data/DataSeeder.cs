using Microsoft.AspNetCore.Identity;
using SkyLegends.Models;

namespace SkyLegends.Data
{
    public static class DataSeeder
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure Database is created
            await context.Database.EnsureCreatedAsync();

            // Seed Roles
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Seed Admin User
            var adminEmail = "admin@skylegends.local";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var user = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            // Seed Posters
            if (!context.Posters.Any())
            {
                context.Posters.AddRange(
                    new Poster
                    {
                        Title = "F-16 in virata ad alta energia",
                        Description = "La potenza pura del Fighting Falcon catturata in una manovra al limite. Un omaggio all'ingegneria aeronautica.",
                        ImageUrl = "/img/posters/f16-placeholder.jpg",
                        Tags = "F-16, Dogfight, Supersonic",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Poster
                    {
                        Title = "Dogfight sopra le nuvole",
                        Description = "L'adrenalina del combattimento aereo. Due caccia si sfidano per la supremazia dei cieli.",
                        ImageUrl = "/img/posters/dogfight-placeholder.jpg",
                        Tags = "Dogfight, Action, Clouds",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Poster
                    {
                        Title = "Supersonico all’alba",
                        Description = "Quando la velocità del suono viene infranta alle prime luci del giorno. Uno spettacolo visivo unico.",
                        ImageUrl = "/img/posters/supersonic-placeholder.jpg",
                        Tags = "Supersonic, Dawn, Atmospheric",
                        CreatedAt = DateTime.UtcNow
                    }
                );
                await context.SaveChangesAsync();
            }

            // Ensure specific real posters exist (if table was already seeded with placeholders)
            if (!context.Posters.Any(p => p.Title == "F-16 Fighting Falcon"))
            {
                context.Posters.Add(new Poster
                {
                    Title = "F-16 Fighting Falcon",
                    Description = "Il caccia multiruolo per eccellenza. Agilità e potenza in un unico pacchetto.",
                    ImageUrl = "/img/posters/F-16 Falcon.jpg",
                    Tags = "F-16, USAF, Fighter",
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (!context.Posters.Any(p => p.Title == "Ultimate Dogfight"))
            {
                context.Posters.Add(new Poster
                {
                    Title = "Ultimate Dogfight",
                    Description = "Combattimento aereo ravvicinato. Adrenalina pura ad alta quota.",
                    ImageUrl = "/img/posters/Dogfight.webp",
                    Tags = "Dogfight, Action, Vintage",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
