﻿using IGeekFan.FreeKit.Extras.FreeSql;
using LinCms.Blog.Tags;
using LinCms.Entities.Blog;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LinCms.Test.Repositories.Blog
{
    public class TagRepositoryTest 
    {

        private readonly IAuditBaseRepository<Tag> _tagRepository;
        private readonly IAuditBaseRepository<TagArticle> _tagArticleRepository;

        public TagRepositoryTest(IAuditBaseRepository<Tag> tagRepository, IAuditBaseRepository<TagArticle> tagArticleRepository)
        {
            _tagRepository = tagRepository;
            _tagArticleRepository = tagArticleRepository;
        }

        [Fact]
        public void Get()
        {
            //LinUser关联无数据
            var d0 = _tagRepository.Select.Include(r => r.LinUser).ToList<TagListDto>();

            //LinUser关联有数据
            var d1 = _tagRepository.Select.Include(r => r.LinUser).ToList();

            //其他字段都有值，LinUser关联没有数据。
            var d2 = _tagRepository.Select.ToList(r => new TagListDto());
            var d22 = _tagRepository.Select.ToList<TagListDto>(); ;


            //其他字段不取
            var d4 = _tagRepository.Select.ToList(r => new TagListDto() { TagName = r.TagName });


            var d3 = _tagRepository.Select.ToList(r => new
            {
                OpenUserDto = new
                {
                    r.LinUser.Id,
                    r.LinUser.Username,
                    r.LinUser.Nickname
                }
            });


        }


    }
}
