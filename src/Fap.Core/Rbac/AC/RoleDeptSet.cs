﻿using Fap.Core.DataAccess;
using Fap.Core.Infrastructure.Domain;
using Fap.Core.Rbac.Model;
/* ==============================================================================
 * 功能描述：  
 * 创 建 者：wyf
 * 创建日期：2016/7/13 16:45:35
 * ==============================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fap.Core.Rbac.AC
{
    [Serializable]
    public class RoleDeptSet : IRoleDeptSet
    {
        private IEnumerable<FapRoleDept> _allRoleDept = new List<FapRoleDept>();
        private static readonly object Locker = new object();
        private bool _initialized;
        private const string POWERORGDEPT = "POWERORGDEPT";
        private IDbSession _dbSession;
        private IFapPlatformDomain _fapPlatformDomain;
        internal RoleDeptSet(IDbSession dbSession,IFapPlatformDomain fapPlatformDomain)
        {
            _dbSession = dbSession;
            _fapPlatformDomain = fapPlatformDomain;
            Init();
        }
        public void Refresh()
        {
            if (_initialized)
            {
                _initialized = false;
            }
        }
        private void Init()
        {
            lock (Locker)
            {
                if (_initialized) return;

                #region 获取所有FapRoleDept

                _allRoleDept = _dbSession.Query<FapRoleDept>("select * from FapRoleDept");

                #endregion
                _initialized = true;
            }
        }
        public bool TryGetValue(string fid, out FapRoleDept roleColumn)
        {
            if (!_initialized)
            {
                Init();
            }
            var result = _allRoleDept.FirstOrDefault<FapRoleDept>(f => f.Fid == fid);
            if (result != null)
            {
                roleColumn = result;
                return true;
            }
            roleColumn = null;
            return false;
        }

        public bool TryGetValueByRole(string roleUid, out IEnumerable<FapRoleDept> roleColumns)
        {
            if (!_initialized)
            {
                Init();
            }
            var result = _allRoleDept.Where<FapRoleDept>(f => f.RoleUid == roleUid);
            if (result != null && result.Any())
            {
                roleColumns = result;
                return true;
            }
            roleColumns = null;
            return false;
        }
        /// <summary>
        /// 获取带权限的部门列表，如果仅仅是部分权限 OrgDept属性HasPartPower=true
        /// </summary>
        /// <param name="roleUid"></param>
        /// <param name="roleDepts"></param>
        /// <returns></returns>
        public bool TryGetValueByRole(string roleUid, out IEnumerable<OrgDept> roleDepts)
        {
            List<OrgDept> powerDepts = new List<OrgDept>();
            if (!_initialized)
            {
                //cacheSupport.RemoveCache(POWERORGDEPT + roleUid);
                Init();
            }
            //缓存中获取
            //if(cacheSupport.GetCache(POWERORGDEPT + roleUid)!=null)
            //{
            //   roleDepts= cacheSupport.GetCache(POWERORGDEPT + roleUid) as IEnumerable<OrgDept>;
            //   return true;
            //}
            var result = _allRoleDept.Where<FapRoleDept>(f => f.RoleUid == roleUid);
          
            IEnumerable<OrgDept> allDepts = _fapPlatformDomain.OrgDeptSet;
            OrgDept rootDept = allDepts.FirstOrDefault(d => string.IsNullOrWhiteSpace(d.Pid) || d.Pid == "#" || d.Pid == "~" || d.Pid == "");
            if (result != null && result.Any())
            {
                List<FapRoleDept> rds = result.ToList();
                if (rds.Exists(r => r.DeptUid == rootDept.Fid))
                {
                    roleDepts = allDepts;
                }
                else
                {
                    rootDept.HasPartPower = true;
                    powerDepts.Add(rootDept);
                    foreach (var rd in result)
                    {
                        OrgDept tempDept = allDepts.FirstOrDefault<OrgDept>(d => d.Fid == rd.DeptUid);
                        if (tempDept != null)
                        {
                            powerDepts.Add(tempDept);
                            AddParentOrgDept(rds, allDepts, powerDepts, tempDept);
                        }
                    }
                    roleDepts = powerDepts;
                }
                //cacheSupport.SetCache(POWERORGDEPT + roleUid, roleDepts);
                return true;
            }
            roleDepts = null;
            return false;
        }
        private void AddParentOrgDept(List<FapRoleDept> rds, IEnumerable<OrgDept> allDepts, List<OrgDept> powerDepts, OrgDept tempDept)
        {
            if (tempDept != null && !(string.IsNullOrWhiteSpace(tempDept.Pid) || tempDept.Pid == "#" || tempDept.Pid == "~" || tempDept.Pid == ""))
            {
                var tempDeptParent = allDepts.FirstOrDefault<OrgDept>(d => d.Fid == tempDept.Pid);
                //存在父部门
                if (tempDeptParent != null)
                {
                    //父部门没在权限中,且还没包含进去
                    if (!rds.Exists(r => r.DeptUid == tempDeptParent.Fid) && !powerDepts.Contains(tempDeptParent))
                    {
                        tempDeptParent.HasPartPower = true;
                        powerDepts.Add(tempDeptParent);
                    }

                    AddParentOrgDept(rds, allDepts, powerDepts, tempDeptParent);
                }
            }
        }
        public IEnumerator<FapRoleDept> GetEnumerator()
        {
            if (!_initialized)
            {
                Init();
            }
            return _allRoleDept.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            if (!_initialized)
            {
                Init();
            }
            return _allRoleDept.GetEnumerator();
        }


    }
}
