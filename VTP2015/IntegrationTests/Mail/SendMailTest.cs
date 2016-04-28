using System.Threading.Tasks;
using NUnit.Framework;
using VTP2015.ServiceLayer.Mail;

namespace IntegrationTests.Mail
{
    [TestFixture]
    internal class SendMailTest
    {

        [Test]
        public void TestSendMail()
        {
            IMailer mailer = new Mailer();

            var mail = mailer.ProduceMail();

            mail.To = "tim.ramandt@student.howest.be";

            mail.Body = "this is the body of the mail, now fuck off";

            mailer.SendMail(mail);

        }
    }
}
