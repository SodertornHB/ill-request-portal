
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
        public virtual string PublicationYear {get;set;}  = ""; 
        public virtual string Edition {get;set;}  = "";
        public virtual string IsbnIssn { get; set; } = "";
        public virtual string Isbn { get; set; } = "";
        public virtual string Issn {get;set;}  = ""; 
        public virtual string MaterialType {get;set;}  = ""; 
        public virtual string RequestType {get;set;}  = ""; 
        public virtual string RequesterName {get;set;}  = ""; 
        public virtual string RequesterEmail {get;set;}  = ""; 
        public virtual string CardNumber {get;set;}  = ""; 
        public virtual string Status {get;set;}  = ""; 
        public virtual string ExternalRequestId {get;set;}  = ""; 
        [DataType(DataType.Text)]
        public virtual DateTime? ExportedOn {get;set;} 
        public virtual string ExportError {get;set;}  = ""; 
        [DataType(DataType.Text)]
        public virtual DateTime? CreatedOn {get;set;} 
        [DataType(DataType.Text)]
        public virtual DateTime? UpdatedOn {get;set;} 
        public virtual string GetBackToListLink(string applicationName) => $"/{applicationName}/{GetType().Name.Replace("ViewModel","")}";
    }
} 