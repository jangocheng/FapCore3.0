﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using Fap.Core.Extensions;
using Fap.AspNetCore.Infrastructure;
using Fap.AspNetCore.ViewModel;
using System;
using Fap.Core.Rbac.Model;
using Fap.Core.Infrastructure.Query;
using Fap.Core.Infrastructure.Domain;
using Fap.Hcm.Service.Organization;
using Ardalis.GuardClauses;

namespace Fap.Hcm.Web.Areas.Organization.Controllers
{
    /// <summary>
    /// 组织部门
    /// </summary>
    [Area("Organization")]
    public class OrgDeptController : FapController
    {
        public OrgDeptController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }


        //组织部门首页
        // GET: /Organization/OrgDept/
        public ActionResult DeptIndex()
        {
            //表格权限，不在这里设置，需要取权限--数据权限哪里设置
            JqGridViewModel model = this.GetJqGridModel("OrgDept");
            return View(model);
        }
        //合并部门
        //Get:/Organization/OrgDept/MergeDept
        public PartialViewResult MergeDept()
        {
            FormViewModel fd = new FormViewModel();
            QuerySet sq = new QuerySet();
            sq.TableName = "OrgDept";
            sq.QueryCols = "*";
            sq.InitWhere = "Id=@Id";
            sq.Parameters.Add(new Parameter("Id", "-1"));
            fd.QueryOption = sq;
            fd.TableName = "OrgDept";
            return PartialView(fd);
        }
        //移动部门
        //Get:/Organization/OrgDept/MoveDept
        public PartialViewResult MoveDept()
        {
            return PartialView();
        }
        /// <summary>
        /// 组织机构图
        /// </summary>
        /// <returns></returns>
        public ViewResult OrgChart(string fid, string type)
        {
            ViewBag.Fid = fid;
            if (string.IsNullOrWhiteSpace(type))
            {
                type = "managerName";
            }
            ViewBag.Type = type;
            return View();
        }
        public IActionResult DeptEmployee(string fid)
        {
            var model = _dbContext.QueryWhere<Fap.Core.Rbac.Model.Employee>("DeptUid=@DeptUid ", new DynamicParameters(new { DeptUid = fid }),true);
            return View(model);
        }
        public ViewResult OrgChart2()
        {
            return View();
        }
        /// <summary>
        /// 职位族群
        /// </summary>
        /// <returns></returns>
        public IActionResult OrgJobGroup()
        {
            var model = GetJqGridModel("OrgJobGroup");
            return View(model);
        }
        /// <summary>
        /// 职位族群等级
        /// </summary>
        /// <returns></returns>
        public PartialViewResult OrgJobGroupGrade(int id)
        {
            dynamic jobGroup = _dbContext.Get("OrgJobGroup", id);
            var model = GetJqGridModel("OrgJobGroupGrade", qs =>
            {
                qs.GlobalWhere = "JobGroup=@Fid";
                qs.AddParameter("Fid", jobGroup.Fid);
                qs.AddDefaultValue("JobGroup", jobGroup.Fid);
                qs.AddDefaultValue("JobGroupMC", jobGroup.Name);
            });
            model.JqgridId = $"jobgroupgrade_{id}";
            return PartialView(model);
        }
        /// <summary>
        /// 职务头衔
        /// </summary>
        /// <returns></returns>
        public IActionResult OrgJobTitle()
        {
            JqGridViewModel model = this.GetJqGridModel("OrgJobTitle");
            return View(model);
        }
        /// <summary>
        /// 职位头衔说明书
        /// </summary>
        /// <returns></returns>
        public IActionResult TitleOutline(string fids)
        {
            Guard.Against.NullOrEmpty(fids, nameof(fids));
            var fidList= fids.SplitComma();
            var model= _dbContext.QueryWhere<OrgJobTitle>("Fid in @Fids", new DynamicParameters(new { Fids = fidList }));
            return PartialView(model);
        }
        /// <summary>
        /// 历史架构
        /// </summary>
        /// <returns></returns>
        public ActionResult OrgHistroy()
        {
            JqGridViewModel model = GetJqGridModel("OrgDept");
            return View(model);
        }
    }
}