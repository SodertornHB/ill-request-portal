
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System;
using System.ComponentModel.DataAnnotations;

namespace IllRequestPortal.Web.ViewModel
{
    public partial class MigrationViewModel : ViewModelBase
    {
        public virtual string ClientVersion {get;set;}  = ""; 
        public virtual string DatabaseVersion {get;set;}  = ""; 
        [DataType(DataType.Text)]
        public virtual DateTime? CreatedOn {get;set;} 
        public virtual string GetBackToListLink(string applicationName) => $"/{applicationName}/{GetType().Name.Replace("ViewModel","")}";
    }
} 