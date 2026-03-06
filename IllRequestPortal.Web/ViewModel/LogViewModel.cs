
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System;
using System.ComponentModel.DataAnnotations;

namespace IllRequestPortal.Web.ViewModel
{
    public partial class LogViewModel : ViewModelBase
    {
        public virtual string Origin {get;set;}  = ""; 
        public virtual string Message {get;set;}  = ""; 
        public virtual string LogLevel {get;set;}  = ""; 
        [DataType(DataType.Text)]
        public virtual DateTime? CreatedOn {get;set;} 
        public virtual string Exception {get;set;}  = ""; 
        public virtual string Trace {get;set;}  = ""; 
        public virtual string GetBackToListLink(string applicationName) => $"/{applicationName}/{GetType().Name.Replace("ViewModel","")}";
    }
} 