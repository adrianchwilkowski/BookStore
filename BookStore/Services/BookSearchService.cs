using Infrastructure.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Models.Commands;
using Infrastructure.Models.Contracts;
using Infrastructure.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BookStore.Services
{
    public interface IBookSearchService
    {
        public Task<List<BookContract>> GetBooks();
        public Task AddBook(AddBookCommand book);
        public Task<BookContract> GetById(Guid id);
        public Task<BookContract> GetByTitle(string title);
        public Task<BookInfoContract> GetInfoByBookId(Guid id);
        public Task<BookInfo> GetFullInfoByBookId(Guid id);
    }
    public class BookSearchService : IBookSearchService
    {
        private readonly IBooksRepository _booksRepository;
        private readonly IBookInfoRepository _bookInfoRepository;
        public BookSearchService(IBooksRepository booksRepository, IBookInfoRepository bookInfoRepository)
        {
            _bookInfoRepository = bookInfoRepository;
            _booksRepository = booksRepository;
        }

        public async Task<List<BookContract>> GetBooks()
        {
            var response = await _booksRepository.GetBooks();
            List<BookContract> result = new List<BookContract>();
            foreach (var book in response)
            {
                result.Add(BookContract.FromDomainModel(book));
            }
            return result;
        }
        public async Task AddBook(AddBookCommand command)
        {
            var book = Book.Create(command.Title, command.Author, command.Description, command.Pages);
            await _booksRepository.Create(book);
        }
        public async Task<BookContract> GetById(Guid id)
        {
            var response = await _booksRepository.GetById(id);
            return BookContract.FromDomainModel(response);
        }
        public async Task<BookContract> GetByTitle(string title)
        {
            var response = await _booksRepository.GetByTitle(title);
            return BookContract.FromDomainModel(response);
        }

        public async Task<BookInfoContract> GetInfoByBookId(Guid id)
        {
            var response = await _bookInfoRepository.GetByBookId(id);
            var result = response.OrderBy(item => item.Price).First();
            return BookInfoContract.FromDomainModel(result);
        }

        public async Task<BookInfo> GetFullInfoByBookId(Guid id)
        {
            var response = await _bookInfoRepository.GetByBookId(id);
            return response.OrderBy(item => item.Price).First();
        }
    }
}
