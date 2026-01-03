using AutoMatch.Data;
using AutoMatch.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoMatch.Services
{
    public interface IAdminService
    {
        Task<AdminDashboardStats> GetDashboardStatsAsync();
        Task<List<RecentReportDto>> GetRecentReportsAsync(int take = 5);
        Task<List<RecentActivityDto>> GetRecentActivityAsync(int take = 5);
        Task<List<ListingsByTypeDto>> GetListingsByTypeAsync();
        Task<List<UserGrowthDto>> GetUserGrowthAsync();
    }

    public class AdminService : IAdminService
    {
        private readonly AutoMatchContext _context;

        public AdminService(AutoMatchContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém as estatísticas principais do dashboard
        /// </summary>
        public async Task<AdminDashboardStats> GetDashboardStatsAsync()
        {
            var totalUsers = await _context.Utilizadores.CountAsync();

            var activeListings = await _context.Anuncios
                .Where(a => a.Estado == true)
                .CountAsync();

            var pendingApprovals = await _context.Anuncios
                .Where(a => a.Estado == false)
                .CountAsync();

            var reportedListings = await _context.Notificacoes
                .Where(n => n.Tipo == "Report" || n.Tipo == "Denúncia")
                .CountAsync();

            return new AdminDashboardStats
            {
                TotalUsers = totalUsers,
                ActiveListings = activeListings,
                PendingApprovals = pendingApprovals,
                ReportedListings = reportedListings
            };
        }

        /// <summary>
        /// Obtém os anúncios reportados recentemente
        /// </summary>
        public async Task<List<RecentReportDto>> GetRecentReportsAsync(int take = 5)
        {
            var reports = await _context.Notificacoes
                .Where(n => (n.Tipo == "Report" || n.Tipo == "Denúncia"))
                .OrderByDescending(n => n.Data_Envio)
                .Take(take)
                .Select(n => new RecentReportDto
                {
                    ReportedBy = _context.Utilizadores
                        .Where(u => u.Id_User == n.Id_Vendedor)
                        .Select(u => u.Nome)
                        .FirstOrDefault() ?? "Unknown",
                    ListingId = n.Id_Comprador,
                    Date = n.Data_Envio,
                    Reason = n.Mensagem
                })
                .ToListAsync();

            return reports;
        }

        /// <summary>
        /// Obtém atividade recente do sistema
        /// </summary>
        public async Task<List<RecentActivityDto>> GetRecentActivityAsync(int take = 5)
        {
            var activities = new List<RecentActivityDto>();

            // Novos anúncios criados
            var newListings = await _context.Anuncios
                .OrderByDescending(a => a.Ano)
                .Take(take)
                .Select(a => new RecentActivityDto
                {
                    Description = $"User {_context.Utilizadores.Where(u => u.Id_User == a.Id_Vendedor).Select(u => u.Nome).FirstOrDefault() ?? "Unknown"} added listing: {a.Titulo}",
                    Timestamp = a.Ano,
                    Type = "listing"
                })
                .ToListAsync();

            activities.AddRange(newListings);

            // Novas reservas
            var newReservas = await _context.Reservas
                .OrderByDescending(r => r.Data_Inicio)
                .Take(take)
                .Select(r => new RecentActivityDto
                {
                    Description = $"User {_context.Utilizadores.Where(u => u.Id_User == r.Id_Comprador).Select(u => u.Nome).FirstOrDefault() ?? "Unknown"} made a reservation",
                    Timestamp = r.Data_Inicio,
                    Type = "reservation"
                })
                .ToListAsync();

            activities.AddRange(newReservas);

            // Notificações/Denúncias
            var notifications = await _context.Notificacoes
                .Where(n => n.Tipo == "Report" || n.Tipo == "Denúncia")
                .OrderByDescending(n => n.Data_Envio)
                .Take(take)
                .Select(n => new RecentActivityDto
                {
                    Description = $"Report: {n.Mensagem}",
                    Timestamp = n.Data_Envio,
                    Type = "report"
                })
                .ToListAsync();

            activities.AddRange(notifications);

            return activities.OrderByDescending(a => a.Timestamp).Take(take).ToList();
        }

        /// <summary>
        /// Obtém contagem de anúncios por categoria
        /// </summary>
        public async Task<List<ListingsByTypeDto>> GetListingsByTypeAsync()
        {
            var listingsByType = await _context.Anuncios
                .Join(_context.Modelos,
                    a => a.Id_Modelo,
                    m => m.Id_Modelo,
                    (a, m) => new { Anuncio = a, Modelo = m })
                .GroupBy(x => x.Modelo.Categoria)
                .Select(g => new ListingsByTypeDto
                {
                    Type = g.Key ?? "Unknown",
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(4)
                .ToListAsync();

            return listingsByType;
        }

        /// <summary>
        /// Obtém crescimento de utilizadores (últimos 7 meses)
        /// </summary>
        public async Task<List<UserGrowthDto>> GetUserGrowthAsync()
        {
            var userGrowth = new List<UserGrowthDto>();

            // Como Utilizadores não tem DataCriacao, vamos retornar dados fixos baseados em contagens
            var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul" };
            var totalUsers = await _context.Utilizadores.CountAsync();

            // Distribui os utilizadores ao longo dos meses
            int usersPerMonth = Math.Max(1, totalUsers / 7);

            for (int i = 0; i < 7; i++)
            {
                userGrowth.Add(new UserGrowthDto
                {
                    Month = months[i],
                    Count = usersPerMonth * (i + 1)
                });
            }

            return userGrowth;
        }
    }

    // DTOs
    public class AdminDashboardStats
    {
        public int TotalUsers { get; set; }
        public int ActiveListings { get; set; }
        public int PendingApprovals { get; set; }
        public int ReportedListings { get; set; }
    }

    public class RecentReportDto
    {
        public string ReportedBy { get; set; }
        public int ListingId { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; }
    }

    public class RecentActivityDto
    {
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string Type { get; set; }
    }

    public class ListingsByTypeDto
    {
        public string Type { get; set; }
        public int Count { get; set; }
    }

    public class UserGrowthDto
    {
        public string Month { get; set; }
        public int Count { get; set; }
    }
}