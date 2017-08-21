using System;
using DBLib;
using DBLib.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbLibUnitTest
{
    [TestClass]
    public class AttributeFuncTest
    {
        [TestMethod]
        public void GetInsertSQLTest()
        {
            Assert.AreEqual(
                "insert into Model (NAME,BIRTHDAY,AGE,CITIZEN) values ('example','1989-02-18 00:00:00',28,1);", 
                AttributeFunc.GetInsertSQL(Model.ForTest())
                );
        }

        [TestMethod]
        public void TransToJsonStringTest()
        {
            Model model = Model.ForTest();
            Assert.AreEqual(
                "{\"ID\"=1,\"NAME\"=\"example\",\"BIRTHDAY\"=\"1989-02-18 00:00:00\",\"AGE\"=28,\"CITIZEN\"=\"True\",\"DESCRIBE\"=null}", 
                AttributeFunc.TransToJsonString(model)
                );
        }

        
    }
}
