using HireMe.Models;
using HireMe.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HireMe.SeedingData
{
    public class AppDbSeeder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AppDbSeeder> _logger;

        public AppDbSeeder(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            IConfiguration config,
            ILogger<AppDbSeeder> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _logger = logger;
            _context = context;
        }

        public async Task SeedAsync()
        {
            //TODO
            // This Condition To Make Seeding Only Once Rather Than Go To Database And Check If Exist
            // Remove This Condition At Production Or When Start New Project And Want To Start Seeding
            // if (true)
            // {
            //     return;
            // }


            _logger.LogInformation("🚀 Starting database seeding...");


            // add default governates
            bool isGovernoratesEmpty = !_context.Governorates.Any();

            if (isGovernoratesEmpty)
            {
                _logger.LogInformation("🌍 Seeding default governates...");

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    // فعل IDENTITY_INSERT
                    await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [Governorates] ON");

                    var governorates = DefaultGovernorates.All;
                    await _context.Governorates.AddRangeAsync(governorates);

                    await _context.SaveChangesAsync();

                    // أغلق IDENTITY_INSERT
                    await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [Governorates] OFF");

                    await transaction.CommitAsync();
                }

                _logger.LogInformation("✅ Default governates seeded successfully.");
            }
            else
            {
                _logger.LogInformation("ℹ️ Governates already exist. Skipping governate seeding.");
            }

            // 1. Read DefaultAdmin section from appsettings
            var adminSection = _config.GetSection("DefaultAdmin").Get<DefaultAdmin>();
            if (adminSection == null || string.IsNullOrWhiteSpace(adminSection.Email) || string.IsNullOrWhiteSpace(adminSection.Password))
            {
                _logger.LogWarning("⚠️ DefaultAdmin config missing or invalid. Skipping admin seeding.");
                return;
            }

            // 2. Ensure Roles Exist
            await EnsureRoleAsync(DefaultRoles.Admin);
            await EnsureRoleAsync(DefaultRoles.Employer);
            await EnsureRoleAsync(DefaultRoles.Worker);

            // 3. Ensure Admin User Exists
            var adminUser = await _userManager.FindByEmailAsync(adminSection.Email);
            if (adminUser == null)
            {
                _logger.LogInformation("👤 Creating default admin user: {Email}", adminSection.Email);

                adminUser = new ApplicationUser
                {
                    UserName = adminSection.Email,
                    Email = adminSection.Email,
                    FirstName = adminSection.FirstName,
                    LastName = adminSection.LastName,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(adminUser, adminSection.Password);

                if (createResult.Succeeded)
                {
                    _logger.LogInformation("✅ Admin user created successfully.");

                    // Assign Role
                    await _userManager.AddToRoleAsync(adminUser, DefaultRoles.Admin);
                    _logger.LogInformation("🔑 Admin user assigned to {Role} role.", DefaultRoles.Admin);
                }
                else
                {
                    foreach (var error in createResult.Errors)
                    {
                        _logger.LogError("❌ Error creating admin: {Error}", error.Description);
                    }
                }
            }
            else
            {
                _logger.LogInformation("ℹ️ Admin user already exists: {Email}", adminSection.Email);
            }

            _logger.LogInformation("🎉 Database seeding completed.");
        }

        private async Task EnsureRoleAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogInformation("📌 Creating role: {Role}", roleName);
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
            else
            {
                _logger.LogInformation("ℹ️ Role already exists: {Role}", roleName);
            }
        }
    }
}
