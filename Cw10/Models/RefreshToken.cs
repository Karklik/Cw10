namespace CW10.DBModels
{
    public partial class RefreshToken
    {
        public string Id { get; set; }
        public string IndexNumber { get; set; }

        public virtual Student IndexNumberNavigation { get; set; }
    }
}
