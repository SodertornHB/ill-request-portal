
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
        public virtual string Title {get;set;}  = ""; 
        public virtual string Author {get;set;}  = ""; 
        public virtual string ArticleTitle {get;set;}  = ""; 
        public virtual string ArticleAuthor {get;set;}  = ""; 
        public virtual string PublicationYear {get;set;}  = ""; 
        public virtual string Edition {get;set;}  = ""; 
        public virtual string Isbn {get;set;}  = ""; 
        public virtual string Issn {get;set;}  = ""; 
        public virtual string Volume {get;set;}  = ""; 
        public virtual string Issue {get;set;}  = ""; 
        public virtual string Pages {get;set;}  = "";
        [Required(ErrorMessage = "MaterialTypeRequired")]
        public virtual string MaterialType {get;set;}  = "";
        [Required(ErrorMessage = "RequesterNameRequired")]
        public virtual string RequesterName {get;set;}  = "";
        [Required(ErrorMessage = "RequesterEmailRequired")]
        [EmailAddress(ErrorMessage = "RequesterEmailInvalid")]
        public virtual string RequesterEmail {get;set;}  = "";
        [RegularExpression(@"^\d{10}$", ErrorMessage = "CardNumberMustBe10Digits")]
        public virtual string CardNumber {get;set;}  = ""; 
        public virtual string Status {get;set;}  = "";
        public virtual string KohaUrl { get; set; } = "";
        public virtual string LibrisUrl { get; set; } = "";

        [DataType(DataType.Text)]
        public virtual DateTime? CreatedOn {get;set;} 
        [DataType(DataType.Text)]
        public virtual DateTime? UpdatedOn {get;set;} 
        [DataType(DataType.Text)]
        public virtual DateTime? AddedInLibrisOn {get;set;}
        public virtual string Description { get; set; } = "";
        public virtual string PurchaseFormatPreference { get; set; } = "";
        public virtual string GetBackToListLink(string applicationName) => $"/{applicationName}/{GetType().Name.Replace("ViewModel","")}";
    }
} 