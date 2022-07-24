namespace SocialGuard.Client.GraphQL
{
    public partial class EmitterSortInput
    {
        public SortEnumType Login { get; set; }
        public SortEnumType EmitterType { get; set; }
        public SortEnumType Snowflake { get; set; }
        public SortEnumType DisplayName { get; set; }
    }
}