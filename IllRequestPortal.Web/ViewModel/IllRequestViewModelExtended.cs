namespace IllRequestPortal.Web.ViewModel
{
    public partial class IllRequestViewModel 
    {       
        public string EmailWithLink() => string.IsNullOrEmpty(this.RequesterEmail) ? string.Empty : $"<a href=\"mailto:{this.RequesterEmail}\">{this.RequesterEmail}</a>";
       
    }
} 