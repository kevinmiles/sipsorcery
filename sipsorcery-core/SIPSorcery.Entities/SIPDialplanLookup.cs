//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SIPSorcery.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class SIPDialplanLookup
    {
        public string Description { get; set; }
        public string ID { get; set; }
        public string LookupKey { get; set; }
        public int LookupType { get; set; }
        public string LookupValue { get; set; }
        public string Owner { get; set; }
        public string DialPlanID { get; set; }
    
        public virtual SIPDialPlan sipdialplan { get; set; }
    }
}
