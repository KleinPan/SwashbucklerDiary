﻿using SqlSugar;
using SwashbucklerDiary.IRepository;
using SwashbucklerDiary.Models;
using System.Linq.Expressions;

namespace SwashbucklerDiary.Repository
{
    public class ResourceRepository : BaseRepository<ResourceModel>, IResourceRepository
    {
        public ResourceRepository(ISqlSugarClient context) : base(context)
        {
        }

        public Task<bool> DeleteUnusedResourcesAsync(Expression<Func<ResourceModel, bool>> expression)
        {
            return base.Context.Deleteable<ResourceModel>()
                .Where(expression)
                .Where(p => SqlFunc.Subqueryable<DiaryResourceModel>().Where(s => s.ResourceUri == p.ResourceUri).NotAny())
                .ExecuteCommandHasChangeAsync();
        }
    }
}
