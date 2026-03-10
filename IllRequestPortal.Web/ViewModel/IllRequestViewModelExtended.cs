using System.ComponentModel.DataAnnotations;
using static Logic.Util.StandardNumberUtil;

namespace IllRequestPortal.Web.ViewModel
{
    public partial class IllRequestViewModelExtended : IllRequestViewModel
    {


        [Required(ErrorMessage = "TitleRequired")]
        public new string Title { get; set; } = "";

        [Required(ErrorMessage = "IsbnIssnRequired")]
        [MinLength(8, ErrorMessage = "IsbnIssnTooShort")]
        public new string IsbnIssn { get; set; } = "";

        [Required(ErrorMessage = "MaterialTypeRequired")]
        public new string MaterialType { get; set; } = "";

        [Required(ErrorMessage = "RequestTypeRequired")]
        public new string RequestType { get; set; } = "";

        [Required(ErrorMessage = "CardNumberRequired")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "CardNumberMustBe10Digits")]
        public new string CardNumber { get; set; } = "";

        [Required(ErrorMessage = "RequesterNameRequired")]
        public new string RequesterName { get; set; } = "";

        [Required(ErrorMessage = "RequesterEmailRequired")]
        [EmailAddress(ErrorMessage = "RequesterEmailInvalid")]
        public new string RequesterEmail { get; set; } = "";


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