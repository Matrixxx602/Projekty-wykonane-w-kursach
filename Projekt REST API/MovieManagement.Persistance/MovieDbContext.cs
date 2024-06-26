﻿using Microsoft.EntityFrameworkCore;
using MovieManagement.Application.Common.Interfaces;
using MovieManagement.Domain.Common;
using MovieManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MovieManagement.Persistance
{
    public class MovieDbContext : DbContext, IMovieDbContext
    {
        private readonly IDateTime _dateTime;
        private readonly ICurrentUserService _userService;

        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
        {
        }

        public MovieDbContext(DbContextOptions<MovieDbContext> options, IDateTime dateTime, ICurrentUserService userService) : base(options)
        {
            _dateTime = dateTime;
            _userService = userService;
        }

        public DbSet<Director> Directors { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<DirectorBiography> DirectorBiographies { get; set; }
        public DbSet<Genre> Genres { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.SeedData();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = _userService.Email;
                        entry.Entity.Created = _dateTime.Now;
                        entry.Entity.StatusId = 1;
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedBy = _userService.Email;
                        entry.Entity.Modified = _dateTime.Now;
                        break;
                    case EntityState.Deleted:
                        entry.Entity.ModifiedBy = _userService.Email;
                        entry.Entity.Modified = _dateTime.Now;
                        entry.Entity.Inactivated = _dateTime.Now;
                        entry.Entity.InactivatedBy = _userService.Email;
                        entry.Entity.StatusId = 0;
                        entry.State = EntityState.Modified;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
