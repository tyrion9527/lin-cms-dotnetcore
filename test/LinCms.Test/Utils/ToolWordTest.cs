﻿using ToolGood.Words;
using Xunit;

namespace LinCms.Test.Utils
{
    public class ToolWordTest
    {
        [Fact]
        [System.Obsolete]
        public void IssuesTest_17()
        {
            var illegalWordsSearch = new IllegalWordsSearch();
            string s = "中国|zg人";
            illegalWordsSearch.SetKeywords(s.Split('|'));
            var str = illegalWordsSearch.Replace("我是中美国人厉害中国完美zg人好的", '*');

            Assert.Equal("我是中美国人厉害**完美***好的", str);
        }
    }
}
