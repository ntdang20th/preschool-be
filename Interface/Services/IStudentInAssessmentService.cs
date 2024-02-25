﻿using Entities;
using Entities.DomainEntities;
using Entities.Search;
using Interface.Services.DomainServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Interface.Services
{
    public interface IStudentInAssessmentService : IDomainService<tbl_StudentInAssessment, StudentInAssessmentSearch>
    {
        Task<PagedList<tbl_Student>> GetStudentInClassForAssessment(StudentInClassForAssessmentSearch request);
        Task<List<AssessmentItem>> AllCritialByStudent(CriteriaResult request);
    }
}
