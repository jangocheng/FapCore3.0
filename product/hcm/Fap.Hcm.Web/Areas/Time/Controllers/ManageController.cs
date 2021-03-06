﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fap.AspNetCore.Infrastructure;
using Fap.AspNetCore.ViewModel;
using Fap.Core.Infrastructure.Domain;
using Fap.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Fap.Core.Utility;
using Fap.Hcm.Service.Organization;
using Fap.Hcm.Service.Time;

namespace Fap.Hcm.Web.Areas.Time.Controllers
{
    [Area("Time")]
    public class ManageController : FapController
    {
        private readonly IOrganizationService _organizationService;
        public ManageController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _organizationService= serviceProvider.GetService<IOrganizationService>();
        }
        #region 基础设置
        /// <summary>
        /// 休息日设置
        /// </summary>
        /// <returns></returns>
        public ActionResult Holiday()
        {
            return View();
        }
        /// <summary>
        /// 班次设置
        /// </summary>
        /// <returns></returns>
        public ActionResult Shift()
        {
            JqGridViewModel model = GetJqGridModel("TmShift");
            return View(model);
        }
        /// <summary>
        /// 类别设置
        /// </summary>
        /// <returns></returns>
        public ActionResult TimeType()
        {
            MultiJqGridViewModel mmodel = new MultiJqGridViewModel();
            JqGridViewModel lm = GetJqGridModel("TmLeaveType");
            mmodel.JqGridViewModels.Add("leave", lm);
            JqGridViewModel tm = GetJqGridModel("TmTravelType");
            mmodel.JqGridViewModels.Add("travel", tm);
            JqGridViewModel om = GetJqGridModel("TmOvertimeType");
            mmodel.JqGridViewModels.Add("overtime", om);
            return View(mmodel);
        }
        /// <summary>
        /// 考勤期间
        /// </summary>
        /// <returns></returns>
        public ActionResult Period()
        {
            var model = this.GetJqGridModel("TmPeriod", (qs) =>
            {
                qs.AddOrderBy("CurrMonth", "desc");
            });
            return View(model);

        }
        /// <summary>
        /// 年假规则设置
        /// </summary>
        /// <returns></returns>
        public ActionResult AnnualLeaveRule()
        {
            var model = GetJqGridModel("TmAnnualLeaveRule");
            return View(model);
        }
        #endregion

        //GET: /Time/Manage/Schedule
        /// <summary>
        /// 排班计划表
        /// 管理员工一年的排班情况
        /// </summary>
        /// <returns></returns>
        public ActionResult Schedule()
        {
            JqGridViewModel empModel = this.GetJqGridModel("Employee", (q) =>
            {
                q.QueryCols = "Id,Fid,EmpCode,EmpName,EmpCategory,DeptUid,Gender,IDCard";
                q.GlobalWhere = "IsMainJob=1";
            });
            return View(empModel);
        }
        /// <summary>
        /// 排班详情
        /// </summary>
        /// <param name="fid">员工UID</param>
        /// <returns></returns>
        public PartialViewResult ScheduleInfo(string fid)
        {
            string sql = "select ScheduleUid from TmScheduleEmployee where EmpUid=@EmpUid and StartDate<=@CDate and EndDate>=@CDate";
            string scheduleUid = _dbContext.ExecuteScalar<string>(sql, new Dapper.DynamicParameters(new { EmpUid = fid, CDate = DateTimeUtils.CurrentDateStr }));
            if (scheduleUid.IsMissing())
            {
                scheduleUid = UUIDUtils.Fid;
            }
            JqGridViewModel scheduleModel = this.GetJqGridModel("TmSchedule",qs=> {
                qs.GlobalWhere = "ScheduleUid=@SchUid";
                qs.AddParameter("SchUid", scheduleUid);
            });
            return PartialView(scheduleModel);
        }

        /// <summary>
        /// 打卡记录
        /// </summary>
        /// <returns></returns>
        public ActionResult CardRecord()
        {
            var model = this.GetJqGridModel("TmCardRecord", (qs) => {
                qs.AddOrderBy("CardTime", "desc");
                qs.AddDefaultValue("DeviceName", "管理员补签");
            });

            return View(model);
        }
        /// <summary>
        /// 年假管理
        /// </summary>
        /// <returns></returns>
        public ActionResult AnnualLeave()
        {
            var model = this.GetJqGridModel("TmAnnualLeave", (qs) =>
            {
                qs.AddOrderBy("Annual", "desc");
            });
            return View(model);
        }
        /// <summary>
        /// 日结果
        /// </summary>
        /// <returns></returns>
        public ActionResult DayResult()
        {
            var model = this.GetJqGridModel("TmDayResult", (qs) =>
            {
                qs.AddOrderBy("CurrDate", "desc");
            });
            return View(model);
        }
        //GET: /Time/Manage/MonthResult
        /// <summary>
        /// 月结果
        /// </summary>
        /// <returns></returns>
        public ActionResult MonthResult()
        {
            var model = this.GetJqGridModel("TmMonthResult", (qs) =>
            {
                qs.AddOrderBy("CurrMonth", "desc");
            });
            return View(model);
        }
        /// <summary>
        /// 我的考勤
        /// </summary>
        /// <returns></returns>
        public IActionResult MyDayResult()
        {
            int annualNum=_dbContext.ExecuteScalar<int>($"select RemainderNum from TmAnnualLeave where Annual='{DateTime.Now.Year}' and EmpUid=@EmpUid",
                new Dapper.DynamicParameters(new { EmpUid = _applicationContext.EmpUid }));
            ViewBag.Annual = annualNum;
            return View();
        }
        public IActionResult EmpCardRecord(string empUid)
        {
            var model = this.GetJqGridModel("TmCardRecord", (q) =>
            {
                q.GlobalWhere = $"EmpUid=@EmpUid";
                q.AddParameter("EmpUid", empUid);
            });
            return PartialView(model);
        }
        /// <summary>
        /// 部门考勤结果
        /// </summary>
        /// <returns></returns>
        public IActionResult DeptDayResult()
        {
            var currPeriod= _dbContext.QueryFirstOrDefaultWhere<TmPeriod>($"{nameof(TmPeriod.IsPeriod)}=1");
            var deptUids = _organizationService.GetDominationDepartment().Select(d => $"'{d.Fid}'");
            var model = GetJqGridModel("TmDayResult", qs =>
            {
                qs.GlobalWhere = $"DeptUid in({string.Join(',', deptUids)})";
                if (currPeriod != null)
                {
                    qs.QueryCols = "Id,Fid,EmpUid,DeptUid,ShiftUid,CurrDate,CardStartTime,CardEndTime,CalResult";
                    qs.GlobalWhere += " and CurrDate>=@StartDate and CurrDate<=@EndDate";
                    qs.AddParameter("StartDate", currPeriod.StartDate);
                    qs.AddParameter("EndDate", currPeriod.EndDate);
                }
            });
            return View(model);
        }
    }
}