using Microsoft.AspNetCore.Identity;
using PRN232_FinalProject.Identity;

namespace PRN232_FinalProject.SeedData
{
    public class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "Staff", "Lecturer" };

            // Tạo các role nếu chưa tồn tại
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Tạo user Admin
            await CreateUserIfNotExists(userManager, "admin@site.com", "System Admin", "Admin@123", "Admin");

            // Tạo user Staff
            await CreateUserIfNotExists(userManager, "staff@site.com", "News Staff", "Staff@123", "Staff");

            // Tạo user Lecturer
            await CreateUserIfNotExists(userManager, "lecturer@site.com", "Lecturer Account", "Lecturer@123", "Lecturer");
        }

        private static async Task CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            string email,
            string fullName,
            string password,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    // Optional: log or throw exception
                    throw new Exception($"Cannot create user {email}: {string.Join(", ", result.Errors)}");
                }
            }
        }

    }
}
