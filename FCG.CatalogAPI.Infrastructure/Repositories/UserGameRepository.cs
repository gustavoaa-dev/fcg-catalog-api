using FCG.CatalogAPI.Domain.Entities;
using FCG.CatalogAPI.Domain.Interfaces;
using FCG.CatalogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.CatalogAPI.Infrastructure.Repositories;

public class UserGameRepository : IUserGameRepository
{
    private readonly CatalogDbContext _context;

    public UserGameRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserGame>> ObterPorUsuario(Guid userId)
    {
        return await _context.UserGames
            .Include(ug => ug.Game)
            .Where(ug => ug.UserId == userId)
            .ToListAsync();
    }

    public async Task<UserGame?> ObterPorIds(Guid userId, Guid gameId)
    {
        return await _context.UserGames
            .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId);
    }

    public async Task Adicionar(UserGame userGame)
    {
        await _context.UserGames.AddAsync(userGame);
    }

    public async Task Salvar()
    {
        await _context.SaveChangesAsync();
    }
}
