using System.Collections.Generic;
using System.Threading.Tasks;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Core.Interfaces;

public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
{
    Task<PasswordResetToken?> GetValidByTokenAsync(string token);
    Task<IEnumerable<PasswordResetToken>> GetUnusedByUserIdAsync(int userId);
}

