using BenchmarkDotNet.Filters;

namespace BenchmarkDotNet.Attributes
{
    public class AnyCategoriesFilterAttribute : FilterConfigBaseAttribute
    {
        // CLS-Compliant Code requires a constructor without an array in the argument list
        public AnyCategoriesFilterAttribute() { }
        
        public AnyCategoriesFilterAttribute(params string[] targetCategories) : base(new AnyCategoriesFilter(targetCategories)) { }
    }
}