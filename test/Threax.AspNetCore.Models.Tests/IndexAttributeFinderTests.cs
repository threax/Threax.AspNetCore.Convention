using System;
using System.Collections.Generic;
using Xunit;

namespace Threax.AspNetCore.Models.Tests
{
    public class IndexAttributeFinderTests
    {
        //Simulate a dbcontext using lists instead, should be the same to the reflection api
        class IndexAttributeFinderTest
        {
            public List<String> NoIndex { get; set; }

            [IndexProp]
            public List<String> WithIndex { get; set; }
        }

        [Fact]
        public void Test1()
        {
            var attributeFinder = new IndexAttributeFinder(typeof(IndexAttributeFinderTest), new Type[] { typeof(List<>) });
            var attributes = attributeFinder.GetIndexProps();
            var attrEnumerator = attributes.GetEnumerator();
            attrEnumerator.MoveNext();

            //NoIndex should be skipped

            //WithIndex
            Assert.Equal(typeof(String), attrEnumerator.Current.Type);
            Assert.Equal("WithIndex", attrEnumerator.Current.PropertyInfo.Name);
        }
    }
}
