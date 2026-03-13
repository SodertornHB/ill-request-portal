
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System;
using System.ComponentModel.DataAnnotations;

namespace IllRequestPortal.Web.ViewModel
{
    public partial class CreateIllRequestViewModel : ViewModelBase
    {
        [Required(ErrorMessage = "MaterialTypeRequired")]
        public string MaterialType { get; set; } = "";

        public virtual string Isbn { get; set; } = "";
        public virtual string Issn { get; set; } = "";

        public string BookTitle { get; set; } = "";
        public string BookAuthor { get; set; } = "";
        public string BookPublicationYear { get; set; } = "";

        public string ChapterBookTitle { get; set; } = "";
        public string ChapterBookAuthor { get; set; } = "";
        public string ChapterBookPublicationYear { get; set; } = "";
        public string ChapterTitle { get; set; } = "";
        public string ChapterAuthor { get; set; } = "";

        public string ArticleTitle { get; set; } = "";
        public string ArticleAuthor { get; set; } = "";

        public string JournalTitle { get; set; } = "";
        public string JournalAuthor { get; set; } = "";

        public string ArticlePublicationYear { get; set; } = "";
        public string Volume { get; set; } = "";
        public string Issue { get; set; } = "";
        public string ChapterPages { get; set; } = "";
        public string ArticlePages { get; set; } = "";

        [RegularExpression(@"^\d{10}$", ErrorMessage = "CardNumberMustBe10Digits")]
        public string CardNumber { get; set; } = "";

        [Required(ErrorMessage = "RequesterNameRequired")]
        public string RequesterName { get; set; } = "";

        [Required(ErrorMessage = "RequesterEmailRequired")]
        [EmailAddress(ErrorMessage = "RequesterEmailInvalid")]
        public string RequesterEmail { get; set; } = "";

        public string Status { get; set; } = "";

        [DataType(DataType.Text)]
        public DateTime? CreatedOn { get; set; }

        [DataType(DataType.Text)]
        public DateTime? UpdatedOn { get; set; }

        [DataType(DataType.Text)]
        public DateTime? AddedInLibrisOn { get; set; }
    }
} 