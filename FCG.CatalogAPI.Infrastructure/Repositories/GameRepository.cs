using FCG.CatalogAPI.Domain.Entities;
using FCG.CatalogAPI.Domain.Interfaces;
using FCG.CatalogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.CatalogAPI.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly CatalogDbContext _context;

    public GameRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Game?> ObterPorId(Guid id)
    {
        return await _context.Games.FindAsync(id);
    }

    public async Task<IEnumerable<Game>> ObterTodos()
    {
        return await _context.Games.ToListAsync();
    }

    public async Task Adicionar(Game game)
    {
        await _context.Games.AddAsync(game);
    }

    public Task Remover(Game game)
    {
        _context.Games.Remove(game);
        return Task.CompletedTask;
    }

    public async Task Salvar()
    {
        await _context.SaveChangesAsync();
    }
}
