using System;
using System.Collections.Generic;

namespace AutoMatch.Models.ViewModels
{
    public class AdminFormsViewModel
    {
        public List<FormSubmissionViewModel> FormSubmissions { get; set; } = new List<FormSubmissionViewModel>();
    }

    public class FormSubmissionViewModel
    {
        public string RequestId { get; set; }
        public string Username { get; set; }
        public DateTime SubmissionDate { get; set; }
        public int ApplicationId { get; set; }
    }

    public class FormDetailsViewModel
    {
        public int ApplicationId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string SellingType { get; set; }
        public string DocumentNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string PostalCode { get; set; }
        public string PreferredContactMethod { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; }
    }
}