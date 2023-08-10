using Microsoft.EntityFrameworkCore;
using System;

namespace CryptoBank.Handlers
{
    public abstract class DbContextBase : IDisposable
    {
        protected DbContext DbContext { get; }

        protected DbContextBase(DbContext dbContext)
        {
            DbContext = dbContext;
        }

        ~DbContextBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DbContext.Dispose();
            }
        }
    }
}
