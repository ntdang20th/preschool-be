using Entities;
using Entities.DomainEntities;
using Entities.Search;
using Interface.Services.DomainServices;
using Request.RequestCreate;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace Interface.Services
{
    public interface IClassService : IDomainService<tbl_Class, ClassSearch>
    {
        Task<List<tbl_Class>> GenerateClass(MultipleClassCreate request);
        Task<List<ClassToPrepare>> GetClassToPrepare(ClassPrepare request);
        Task<tbl_Class> AddClass(ClassCreate request);
        Task<tbl_Class> ClassByIdAsync(Guid id);
    }
}
