using System.Collections.Generic;
using System.Threading.Tasks;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Core.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(int userId);
    Task<IEnumerable<RefreshToken>> GetAllByUserIdAsync(int userId);
}

