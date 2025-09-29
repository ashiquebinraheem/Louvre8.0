using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize(Roles = "Employee")]
    [BindProperties]
    public class ViewVisitRequestModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly ICommonRepository _commonRepository;


        public ViewVisitRequestModel(IDbContext entity, IDbConnection cn, ICommonRepository commonRepository)
        {
            _dbContext = entity;
            this.cn = cn;
            _commonRepository = commonRepository;
        }

        public VisitRequest Data { get; set; }
        public List<VisitRequestDrink> Drinks { get; set; }
        public List<DocumentPostViewModel> Documents { get; set; }

        public async Task OnGetAsync(int id)
        {
            Data = await _dbContext.GetAsync<VisitRequest>(id);

            int requesterId = Convert.ToInt32(Data.AddedBy);

            var employees = await _commonRepository.GetEmployeesAsync(requesterId);
            ViewData["Employees"] = employees;
            ViewData["Countries"] = await GetSelectList<Country>(_dbContext, "CountryName");
            ViewData["Departments"] = await GetSelectList<Department>(_dbContext, "DepartmentName");
            ViewData["Areas"] = await GetSelectList<Area>(_dbContext, "AreaName");
            ViewData["Purposes"] = await GetSelectList<Purpose>(_dbContext, "PurposeName");
            ViewData["Durations"] = await GetSelectList<Duration>(_dbContext, "DurationName");
            ViewData["Vehicles"] = await GetSelectList<Vehicle>(_dbContext, "RegisterNo", $"AddedBy ={ requesterId }");
            Documents = await _commonRepository.GetAllVisitRequestDocumentsAsync(Convert.ToInt32(Data.VisitRequestID));
        }
    }
}