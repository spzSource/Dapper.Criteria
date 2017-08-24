namespace Dapper.Criteria.Formatters
{
    public interface IFormatter
    {
        void Format(ref object input);
    }
}