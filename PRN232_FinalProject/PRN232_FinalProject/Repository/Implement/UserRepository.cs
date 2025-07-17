using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN232_FinalProject.Identity;
using PRN232_FinalProject.Models;
using PRN232_FinalProject.Repository.Interfaces;

namespace PRN232_FinalProject.Repository.Implement
{
    public class UserRepository : IUserRepository
    {
        private readonly Prn232FinalProjectContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserRepository(Prn232FinalProjectContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync() => await _context.Users.ToListAsync();

        public async Task<ApplicationUser?> GetByIdAsync(string id) => await _context.Users.FindAsync(id);

        public async Task<bool> DeleteAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<ApplicationUser> GetByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

    }

}
