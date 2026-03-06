using static Logic.Util.StandardNumberUtil;

namespace IllRequestPortal.Logic.Model
{
    public partial class IllRequestExtended : IllRequest
    {
        public bool HasValidIsbnOrIssn() => StandardNumberUtility.IsValidIsbnOrIssn(this.Isbn) || StandardNumberUtility.IsValidIsbnOrIssn(this.Issn);
        public bool HasValidIsbn() => StandardNumberUtility.IsValidIsbnOrIssn(this.Isbn);
        public bool HasValidIssn() => StandardNumberUtility.IsValidIsbnOrIssn(this.Issn);
        public string ValidType() => !HasValidIsbnOrIssn() ? string.Empty : (HasValidIsbn() ? "ISBN" : "ISSN");
    }
} 