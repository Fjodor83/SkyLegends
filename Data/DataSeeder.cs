using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            // EnsureCreated works for both SQLite and PostgreSQL
            await context.Database.EnsureCreatedAsync();

            await EnsureAdminRoleAndUserAsync(userManager, roleManager);
            await EnsurePostersAsync(context);
            await EnsureVideosAsync(context);
        }

        private static async Task EnsureAdminRoleAndUserAsync(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            const string adminRole = "Admin";
            const string adminEmail = "admin@skylegends.local";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser != null)
            {
                if (!await userManager.IsInRoleAsync(adminUser, adminRole))
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }

                return;
            }

            var user = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Password123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, adminRole);
            }
        }

        private static async Task EnsurePostersAsync(ApplicationDbContext context)
        {
            var postersToEnsure = new List<Poster>
            {
                new()
                {
                    Title = "F-16 in virata ad alta energia",
                    Description = "La potenza pura del Fighting Falcon catturata in una manovra al limite. Un omaggio all'ingegneria aeronautica.",
                    ImageUrl = "/img/posters/f16-turn.jpg",
                    Tags = "F-16, Dogfight, Supersonic",
                    Price = 29.99m,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Title = "Dogfight sopra le nuvole",
                    Description = "L'adrenalina del combattimento aereo. Due caccia si sfidano per la supremazia dei cieli.",
                    ImageUrl = "/img/posters/dogfight-clouds.jpg",
                    Tags = "Dogfight, Action, Clouds",
                    Price = 24.99m,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Title = "Supersonico all'alba",
                    Description = "Quando la velocità del suono viene infranta alle prime luci del giorno. Uno spettacolo visivo unico.",
                    ImageUrl = "/img/posters/supersonic-dawn.jpg",
                    Tags = "Supersonic, Dawn, Atmospheric",
                    Price = 34.99m,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Title = "F-16 Fighting Falcon",
                    Description = "Il caccia multiruolo per eccellenza. Agilità e potenza in un unico pacchetto.",
                    ImageUrl = "/img/posters/F-16 Falcon.jpg",
                    Tags = "F-16, USAF, Fighter",
                    Price = 29.99m,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Title = "Ultimate Dogfight",
                    Description = "Combattimento aereo ravvicinato. Adrenalina pura ad alta quota.",
                    ImageUrl = "/img/posters/Dogfight.webp",
                    Tags = "Dogfight, Action, Vintage",
                    Price = 19.99m,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var existingTitles = context.Posters
                .Select(p => p.Title)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var newPosters = postersToEnsure
                .Where(p => !existingTitles.Contains(p.Title))
                .ToList();

            if (newPosters.Count == 0)
            {
                return;
            }

            context.Posters.AddRange(newPosters);
            await context.SaveChangesAsync();
        }

        private static async Task EnsureVideosAsync(ApplicationDbContext context)
        {
            if (context.Videos.Any())
            {
                return;
            }

            context.Videos.Add(new Video
            {
                Title = "Fighter Jet Action",
                Description = "High speed fighter jet maneuvers.",
                VideoUrl = "/img/posters/Fighter Jet.mp4",
                ThumbnailUrl = "/img/posters/f16-turn.jpg",
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }
    }
}
