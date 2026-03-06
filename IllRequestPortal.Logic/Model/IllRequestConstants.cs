using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Model
{
    public static class IllRequestConstants
    {
        public static class MaterialTypes
        {
            public const string Book = "Book";
            public const string Article = "Article";
            public const string Chapter = "Chapter";
            public const string Journal = "Journal";
            public const string Other = "Other";
        }

        public static class RequestTypes
        {
            public const string Loan = "Loan";
            public const string Copy = "Copy";
        }

        public static class Statuses
        {
            public const string Created = "Created";
            public const string PendingExport = "PendingExport";
            public const string Exported = "Exported";
            public const string Failed = "Failed";
        }
    }
}
