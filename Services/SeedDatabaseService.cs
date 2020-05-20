using System.Linq;
using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Services
{
    /// <summary>
    /// Service to seed the DB
    /// </summary>
    public interface ISeedDatabaseService
    {
        /// <summary>
        /// Seeds the database with some predefined data
        /// </summary>
        void Seed();
    }

    /// <summary>
    /// Implementation of the DB service
    /// </summary>
    public class SeedDatabaseService : ISeedDatabaseService
    {
        private Db Context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">DB context</param>
        public SeedDatabaseService(
            Db context
        )
        {
            Context = context;
        }

        /// <summary>
        /// Seed DB implementation
        /// </summary>
        public void Seed()
        {
            foreach (var identityRole in SeedRoles)
            {
                if (Context.Roles.FirstOrDefault(ir => ir.Id == identityRole.Id) == null)
                {
                    Context.Roles.Add(identityRole);
                }
            }
            Context.SaveChanges();
        }



        #region data

        private static IdentityRole[] SeedRoles = new[] {
          new IdentityRole {
              Id = "1",
              Name = "Admin",
              NormalizedName = "ADMIN"
          }
        };

        #endregion
    }
}