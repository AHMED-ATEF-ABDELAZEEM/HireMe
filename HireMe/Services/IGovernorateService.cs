using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HireMe.Contracts.Governorate.Responses;
using HireMe.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HireMe.Services
{
    public interface IGovernorateService
    {
        Task<IEnumerable<GovernorateResponse>> GetAllGovernoratesAsync();
    }

    public class GovernorateService : IGovernorateService
    {
        private readonly AppDbContext _context;

        public GovernorateService (AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GovernorateResponse>> GetAllGovernoratesAsync()
        {
            return await _context.Governorates
                .Select(g => new GovernorateResponse
                {
                    Id = g.Id,
                    NameArabic = g.NameArabic,
                    NameEnglish = g.NameEnglish
                })
                .ToListAsync(); 
        }
    }
}