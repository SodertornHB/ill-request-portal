
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using Newtonsoft.Json;
using System;

namespace IllRequestPortal.Web.ViewModel
{
    public class ViewModelBase
    {
        public virtual int  Id { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
