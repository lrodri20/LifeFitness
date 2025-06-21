namespace SmartFitnessApi.Services
{
    using Microsoft.EntityFrameworkCore;
    using SmartFitnessApi.Data;
    using SmartFitnessApi.Models;
    using SmartFitnessApi.Models.enums;
    using System;
    using System.Threading.Tasks;

    public class SearchService : ISearchService
    {
        private readonly SmartFitnessDbContext _context;

        public SearchService(SmartFitnessDbContext context)
        {
            _context = context;
        }
    }
}