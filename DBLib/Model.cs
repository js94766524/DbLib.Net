using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DBLib.Attributes;

namespace DBLib
{
    [Table("Model")]
    public class Model
    {
        [Column("ID", DbType.Int32, PrimaryKey = true, AutoIncrement = true)]
        public int id { get; set; }

        [Column("NAME", DbType.String)]
        public string name { get; set; }

        [Column("BIRTHDAY", DbType.Date)]
        public DateTime birthday { get; set; }

        [Column("AGE", DbType.Int32)]
        public int age { get; set; }

        [Column("CITIZEN", DbType.Boolean)]
        public bool citizen { get; set; }

        [Column("DESCRIBE", DbType.String)]
        public string describe { get; set; }

        public static Model ForTest()
        {
            return new Model()
            {
                id = 1,
                name = "example",
                birthday = DateTime.Parse("1989-02-18"),
                age = 28,
                citizen = true,
                describe = null,
            };
        }

    }
}
