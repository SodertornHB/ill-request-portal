
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System;
using System.ComponentModel.DataAnnotations;

namespace IllRequestPortal.Web.ViewModel
{
    public partial class IllRequestViewModel : ViewModelBase
    {
        [Required(ErrorMessage = "TitleRequired")]
        public virtual string MainTitle { get; set; } = "";
        public virtual string MainAuthor { get; set; } = "";
        public virtual string ContainerTitle { get; set; } = "";
        public virtual string ContainerAuthorOrEditor { get; set; }
        
        [Required(ErrorMessage = "IsbnIssnRequired")]
        [MinLength(8, ErrorMessage = "IsbnIssnTooShort")]
        public string IsbnIssn { get; set; } = "";

        [Required(ErrorMessage = "MaterialTypeRequired")]
        public string MaterialType { get; set; } = "";

        [Required(ErrorMessage = "RequestTypeRequired")]
        public string RequestType { get; set; } = "";

        [RegularExpression(@"^\d{10}$", ErrorMessage = "CardNumberMustBe10Digits")]
        public string CardNumber { get; set; } = "";

        [Required(ErrorMessage = "RequesterNameRequired")]
        public string RequesterName { get; set; } = "";

        [Required(ErrorMessage = "RequesterEmailRequired")]
        [EmailAddress(ErrorMessage = "RequesterEmailInvalid")]
        public string RequesterEmail { get; set; } = "";
        public virtual string PublicationYear {get;set;}  = ""; 
        public virtual string Isbn {get;set;}  = ""; 
        public virtual string Issn {get;set;}  = ""; 
        public virtual string Volume {get;set;}  = ""; 
        public virtual string Issue {get;set;}  = ""; 
        public virtual string Pages {get;set;}  = ""; 
        public virtual string Status {get;set;}  = ""; 
        [DataType(DataType.Text)]
        public virtual DateTime? CreatedOn {get;set;} 
        [DataType(DataType.Text)]
        public virtual DateTime? UpdatedOn {get;set;} 
        [DataType(DataType.Text)]
        public virtual DateTime? AddedInLibrisOn {get;set;} 
    }
} 