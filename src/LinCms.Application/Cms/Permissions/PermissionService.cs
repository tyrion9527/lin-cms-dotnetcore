﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using IGeekFan.FreeKit.Extras.FreeSql;
using IGeekFan.FreeKit.Extras.Security;
using LinCms.Common;
using LinCms.Data;
using LinCms.Entities;
using LinCms.Security;

namespace LinCms.Cms.Permissions;

public class PermissionService(IAuditBaseRepository<LinPermission, long> permissionRepository,
        IAuditBaseRepository<LinGroupPermission, long> groupPermissionRepository, ICurrentUser currentUser)
    : ApplicationService, IPermissionService
{
    private readonly ICurrentUser _currentUser = currentUser;

    public IDictionary<string, List<PermissionDto>> GetAllStructualPermissions()
    {
        return permissionRepository.Select.ToList()
            .GroupBy(r => r.Module)
            .ToDictionary(
                group => group.Key,
                group =>
                    Mapper.Map<List<PermissionDto>>(group.ToList())
            );

    }

    /// <summary>
    /// 检查当前登录的用户的分组权限
    /// </summary>
    /// <param name="module">模块</param>
    /// <param name="permission">权限名</param>
    /// <returns></returns>
    public async Task<bool> CheckPermissionAsync(string module, string permission)
    {
        //默认Admin角色拥有所有权限
        if (CurrentUser.IsInGroup(LinConsts.Group.Admin)) return true;
        long[] groups = CurrentUser.FindGroupIds().Select(long.Parse).ToArray();

        LinPermission linPermission = await permissionRepository.Where(r => r.Module == module && r.Name == permission).FirstAsync();

        if (linPermission == null || groups == null || groups.Length == 0) return false;

        bool existPermission = await groupPermissionRepository.Select
            .AnyAsync(r => groups.Contains(r.GroupId) && r.PermissionId == linPermission.Id);

        return existPermission;
    }


    public Task DeletePermissionsAsync(RemovePermissionDto permissionDto)
    {
        return groupPermissionRepository.DeleteAsync(r =>
            permissionDto.PermissionIds.Contains(r.PermissionId) && r.GroupId == permissionDto.GroupId);
    }

    public Task DispatchPermissions(DispatchPermissionsDto permissionDto, List<PermissionDefinition> permissionDefinitions)
    {
        List<LinGroupPermission> linPermissions = new();
        permissionDto.PermissionIds.ForEach(permissionId =>
        {
            linPermissions.Add(new LinGroupPermission(permissionDto.GroupId, permissionId));
        });
        return groupPermissionRepository.InsertAsync(linPermissions);
    }

    public async Task<List<LinPermission>> GetPermissionByGroupIds(List<long> groupIds)
    {
        List<long> permissionIds = groupPermissionRepository
            .Where(a => groupIds.Contains(a.GroupId))
            .ToList(r => r.PermissionId);

        List<LinPermission> listPermissions = await permissionRepository
            .Where(a => permissionIds.Contains(a.Id))
            .ToListAsync();

        return listPermissions;

    }

    public List<IDictionary<string, object>> StructuringPermissions(List<LinPermission> permissions)
    {
        var groupPermissions = permissions.GroupBy(r => r.Module).Select(r => new
        {
            r.Key,
            Children = r.Select(u => u.Name).ToList()
        }).ToList();

        List<IDictionary<string, object>> list = new();

        foreach (var groupPermission in groupPermissions)
        {
            IDictionary<string, object> moduleExpandoObject = new ExpandoObject();
            List<IDictionary<string, object>> perExpandList = new();
            groupPermission.Children.ForEach(permission =>
            {
                IDictionary<string, object> perExpandObject = new ExpandoObject();
                perExpandObject["module"] = groupPermission.Key;
                perExpandObject["permission"] = permission;
                perExpandList.Add(perExpandObject);
            });

            moduleExpandoObject[groupPermission.Key] = perExpandList;
            list.Add(moduleExpandoObject);
        }

        return list;
    }

    public Task<LinPermission> GetAsync(string permissionName)
    {
        return permissionRepository.Where(r => r.Name == permissionName).FirstAsync();
    }

    public async Task<List<TreePermissionDto>> GetTreePermissionListAsync()
    {
        var permissions = await permissionRepository.Select.ToListAsync();

        List<TreePermissionDto> treePermissionDtos = permissions.GroupBy(r => r.Module).Select(r =>
            new TreePermissionDto
            {
                Rowkey = Guid.NewGuid().ToString(),
                Children = new List<TreePermissionDto>(),
                Name = r.Key,
            }).ToList();


        foreach (var permission in treePermissionDtos)
        {
            var childrens = permissions.Where(u => u.Module == permission.Name)
                .Select(r => new TreePermissionDto
                {
                    Rowkey = r.Id.ToString(),
                    Name = r.Name,
                    Router = r.Router,
                    CreateTime = r.CreateTime
                })
                .ToList();
            permission.Children = childrens;
        }

        return treePermissionDtos;
    }
}