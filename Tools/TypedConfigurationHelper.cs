static class TypedConfigurationHelper
{
    public static T GetSection<T>(this IConfiguration configuration) where T : ISection
    {
        return configuration.GetSection(T.SectionName).Get<T>() ?? throw new Exception();
    }

    public interface ISection
    {
        static public abstract string SectionName { get; }
    }
}