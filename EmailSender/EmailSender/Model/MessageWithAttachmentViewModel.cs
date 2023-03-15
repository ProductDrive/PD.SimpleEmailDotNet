using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PD.EmailSender.Helpers.Model
{
    public class MessageWithAttachmentViewModel
    {
        public string Subject { get; set; }
        public string Message { get; set; }
        public List<IFormFile> Attachments { get; set; }
        public List<Stream> AttachmentInCode { get; set; }
        public string Filename { get; set; }
        public string[] EmailAddresses { get; set; }
        public string[] Bcc { get; set; }
        public string[] Cc { get; set; }
        public string EmailDisplayName { get; set; }
        public string SenderEmail { get; set; }
        public string User { get; set; }
    }
}
