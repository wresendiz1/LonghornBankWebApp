using Microsoft.Build.Framework;

namespace LonghornBankWebApp.Models
{
    public class Message
    {

        public Int32 MessageID { get; set; }

        public String Receiver { get; set; }
        
        public String Sender { get; set; }

        [Required]
        public String Subject { get; set; }

        [Required]
        public String Info { get; set; }

        public DateTime Date { get; set; }

        public Boolean IsRead { get; set; }

        // Have a list of all admins and remove once they view
        public List<AppUser> Admins { get; set; }


       
        public Message()
        {

            if(Admins == null)
            {
                Admins = new List<AppUser>();

            }
        }

        
    }
}
