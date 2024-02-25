﻿using Entities;
using Entities.DomainEntities;
using Entities.Search;
using Interface.Services.DomainServices;
using Request.DomainRequests;
using Request.RequestCreate;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Interface.Services
{
    public interface IStudentService : IDomainService<tbl_Student, StudentSearch>
    {
        Task<AppDomainResult> GetStudentsForArrange(ArrangeNewClassSearch request);
        Task<PagedList<tbl_Student>> AvailableStudent(AvailableStudentRequest request);
        Task<List<StudentSelection>> GetByParent(Guid parentId);
        Task<List<ProfileStudentForMobile>> GetProfileForMobile(Guid parentId);
        Task<PagedList<tbl_Student>> GetStudentByGrade(GetStudentByGradeRequest request);

    }
}
