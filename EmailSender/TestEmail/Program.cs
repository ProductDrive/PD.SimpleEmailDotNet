using PD.EmailSender.Helpers;
using PD.EmailSender.Helpers.Model;
using System;
using System.Collections.Generic;

namespace TestEmail
{
    class Program
    {
        static void Main(string[] args)
        {
            EmailEngine engine = new EmailEngine();
            var ss = new MessagingViewModel
            {
                EmailAddress = "afeexclusive@gmail.com",
                Contacts = new List<ContactsViewModel>() { new ContactsViewModel() { Email = "adeoyetemitayo99@gmail.com", Name = "Tayo" } },
                Message = "Hello world from here",
                EmailDisplayName = "Afe Personal",
                Subject = "Testing Email"
            };
            var res = engine.SendSingleEmailAsync(ss);


            Console.WriteLine($"Hello World! {res}");
        }
    }
}
