using static Logic.Util.StandardNumberUtil;

namespace IllRequestPortal.Web.ViewModel
{
    public partial class IllRequestViewModelExtended : IllRequestViewModel
    {
        public override string Isbn
        {
            get
            {
                var type = StandardNumberUtility.Detect(base.IsbnIssn);

                if (type == "ISSN")
                {
                    return string.Empty;
                }
                return base.IsbnIssn;

            }
            set => base.Isbn = value;
        }
        public override string Issn
        {
            get
            {
                var type = StandardNumberUtility.Detect(base.IsbnIssn);

                if (type == "ISSN")
                {
                    return base.IsbnIssn;
                }
                return string.Empty;

            }
            set => base.Isbn = value;
        }
    }
}