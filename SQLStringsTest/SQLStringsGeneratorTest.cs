using Xunit;
using SQLStrings;
using System.IO;
using System.Collections.Generic;
using System;

namespace SQLStringsTest
{
    public class SQLStringsGeneratorTest
    {

        [Fact]
        public void TestParseText()
        {
            var query = @"-- Name: FirstQuery
-- comment line 1
-- comment line 2
select * from artists;
            
-- Name: SecondQuery
insert into artists(id,name) values
(1, 'name 1'),
(2, 'name 2'),
(3, 'name 3');";

            var expected = new List<QueryInfo>()
            {
                new QueryInfo
                {
                    Name = "FirstQuery",
                    Comments = new string[] 
                    {
                        @"comment line 1",
                        "comment line 2"
                    },
                    SQL = "select * from artists;"
                },
                new QueryInfo
                {
                    Name = "SecondQuery",
                    Comments = Array.Empty<string>(),
                    SQL = @"insert into artists(id,name) values
(1, 'name 1'),
(2, 'name 2'),
(3, 'name 3');"
                }
            };

            IEnumerable<QueryInfo> actual;

            using (TextReader reader = new StringReader(query))
            {
                actual = SQLStringsGenerator.ParseText(reader);
            }    

            Assert.Equal(expected, actual);
        }
    }
}