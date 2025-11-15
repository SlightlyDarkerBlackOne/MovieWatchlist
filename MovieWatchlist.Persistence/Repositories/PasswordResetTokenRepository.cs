using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Persistence.Data;

namespace MovieWatchlist.Persistence.Repositories;

public class PasswordResetTokenRepository : EfRepository<PasswordResetToken>, IPasswordResetTokenRepository
{
    public PasswordResetTokenRepository(MovieWatchlistDbContext context) : base(context)
    {
    }

    public async Task<PasswordResetToken?> GetValidByTokenAsync(string token)
    {
        return await _dbSet
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token && t.IsValid());
    }

    public async Task<IEnumerable<PasswordResetToken>> GetUnusedByUserIdAsync(int userId)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && !t.IsUsed)
            .ToListAsync();
    }
}

