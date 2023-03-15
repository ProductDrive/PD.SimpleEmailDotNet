using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PD.EmailSender.Helpers.Model
{
    public class MessagingViewModel
    {
        public string Subject { get; set; }
        public string Message { get; set; }
        public List<IFormFile> Attachments { get; set; }
        public List<Stream> AttachmentInCode { get; set; }
        public string Filename { get; set; }

        public bool Ispersonalized { get; set; }
        public List<ContactsViewModel> Contacts { get; set; }
        public string[] GroupedContacts { get; set; }
        public string ToContacts { get; set; }
        public string ToOthers { get; set; }
        public string ISOCode { get; set; }
        public string Category { get; set; }

        public string EmailAddress { get; set; }
        public string EmailDisplayName { get; set; }

        //bulksms or hostedsms.
        public string GateWayToUse { get; set; }

        public string User { get; set; }
    }

    public class ContactsViewModel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string status { get; set; }
        public string otherInfo { get; set; }
    }
}
