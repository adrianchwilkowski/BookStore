using Infrastructure.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface IBookInfoRepository
    {
        Task<List<BookInfo>> GetBookInfoList();
        Task<List<BookInfo>> GetByBookId(Guid bookId);
        Task Create(BookInfo book);
    }
    public class BookInfoRepository : IBookInfoRepository
    {
        public ApplicationDbContext Context { get; set; }
        public BookInfoRepository(ApplicationDbContext context) { Context = context; }

        public async Task<List<BookInfo>> GetBookInfoList()
        {
            return await Context.BookInfo
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<List<BookInfo>> GetByBookId(Guid bookId)
        {
            var response = await Context.BookInfo
            .Where(x => x.BookId == bookId)
            .ToListAsync();
            if (response.Count == 0)
            {
                throw new NotFoundException("Bookinfo for book with given ID doesn't exist.");
            }
            return response;
        }
        public async Task Create(BookInfo bookInfo)
        {
            Context.Attach(bookInfo.Book);
            Context.BookInfo
                .Add(bookInfo);
            await Context.SaveChangesAsync();
        }
    }
}
