using System.Linq;

namespace SQLStrings
{
    public class QueryInfo
    {
        public string Name {get; set;}
        public string[] Comments {get; set;}
        public string SQL {get; set;}

        public override bool Equals(object obj)
        {
            if (obj is QueryInfo qi)
            {
                return Name == qi.Name && SQL == qi.SQL && Enumerable.SequenceEqual(Comments, qi.Comments);
            }
            else
            {
                return false;
            }         
        }

        public override int GetHashCode()
        {
            return (Name + string.Join("", Comments) + SQL).GetHashCode();
        }
    }
}