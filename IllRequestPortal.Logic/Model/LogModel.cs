
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System;

namespace IllRequestPortal.Logic.Model
{
    public partial class Log : Entity
    {
        public virtual string Origin {get;set;}
        public virtual string Message {get;set;}
        public virtual string LogLevel {get;set;}
        public virtual DateTime? CreatedOn {get;set;}
        public virtual string Exception {get;set;}
        public virtual string Trace {get;set;}
      
    }
} 