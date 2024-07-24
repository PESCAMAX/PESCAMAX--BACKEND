using API.Modelo;
using API.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    // IEspecieLoteService.cs
    public interface IEspecieLoteService
    {
        Task AsignarEspecieALote(EspecieLote especieLote);
        Task<IEnumerable<EspecieLoteDTO>> ObtenerEspeciePorLote(string userId);
    }

    // EspecieLoteService.cs
    public class EspecieLoteService : IEspecieLoteService
    {
        private readonly ApplicationDbContext _context;

        public EspecieLoteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AsignarEspecieALote(EspecieLote especieLote)
        {
            var existingAssignment = await _context.EspecieLote
                .FirstOrDefaultAsync(el => el.LoteId == especieLote.LoteId && el.UserId == especieLote.UserId);

            if (existingAssignment != null)
            {
                throw new Exception("Ya existe una especie asignada a este lote.");
            }

            _context.EspecieLote.Add(especieLote);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<EspecieLoteDTO>> ObtenerEspeciePorLote(string userId)
        {
            return await _context.EspecieLote
                .Where(el => el.UserId == userId)
                .Join<EspecieLote, CrearEspecie, int, EspecieLoteDTO>(
                    _context.CrearEspecie,
                    el => el.EspecieId,
                    e => e.Id,
                    (el, e) => new EspecieLoteDTO
                    {
                        LoteId = el.LoteId,
                        EspecieId = e.Id,
                        NombreEspecie = e.NombreEspecie
                    })
                .ToListAsync();
        }
    }
}