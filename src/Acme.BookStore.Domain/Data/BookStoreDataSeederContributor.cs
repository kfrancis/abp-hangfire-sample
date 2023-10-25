using System;
using System.Threading.Tasks;
using Acme.BookStore.Books;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.TenantManagement;

namespace Acme.BookStore.Data
{
    public class BookStoreDataSeederContributor
    : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<Book, Guid> _bookRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ITenantManager _tenantManager;
        private readonly IRepository<Tenant, Guid> _tenantRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly IDataSeeder _dataSeeder;

        public BookStoreDataSeederContributor(IRepository<Book, Guid> bookRepository,
            IGuidGenerator guidGenerator,
            ITenantManager tenantManager,
            IRepository<Tenant, Guid> tenantRepository,
             UserManager<IdentityUser> userManager,
        IRepository<IdentityUser, Guid> userRepository,
        IDataSeeder dataSeeder)
        {
            _bookRepository = bookRepository;
            _guidGenerator = guidGenerator;
            _tenantManager = tenantManager;
            _tenantRepository = tenantRepository;
            _userManager = userManager;
            _userRepository = userRepository;
            _dataSeeder = dataSeeder;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            // Seed the tenant "default"
            var defaultTenant = await _tenantRepository.FindAsync(t => t.Name == "default");
            if (defaultTenant == null)
            {
                defaultTenant = await _tenantManager.CreateAsync(name: "default");
                await _tenantRepository.InsertAsync(defaultTenant, true);
            }

            // Seed the default "admin" user for this tenant
            var adminUser = await _userRepository.FindAsync(u => u.TenantId == defaultTenant.Id && u.UserName == "admin");
            if (adminUser == null)
            {
                var newUser = new IdentityUser(_guidGenerator.Create(), "admin", "admin@default.com", defaultTenant.Id);

                var createStatus = await _userManager.CreateAsync(newUser, "1q2w3E*");
                if (createStatus.Succeeded)
                {
                }
            }

            if (await _bookRepository.GetCountAsync() <= 0)
            {
                await _bookRepository.InsertAsync(
                    new Book
                    {
                        Name = "1984",
                        Type = BookType.Dystopia,
                        PublishDate = new DateTime(1949, 6, 8),
                        Price = 19.84f
                    },
                    autoSave: true
                );

                await _bookRepository.InsertAsync(
                    new Book
                    {
                        Name = "The Hitchhiker's Guide to the Galaxy",
                        Type = BookType.ScienceFiction,
                        PublishDate = new DateTime(1995, 9, 27),
                        Price = 42.0f,
                        TenantId = defaultTenant.Id
                    },
                    autoSave: true
                );
            }
        }
    }
}
