using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AngularJsDemo.Models
{
    public sealed class VCardFile : List<VCard>
    {
        const string begin_vcard_tag = "BEGIN:VCARD";
        const string end_vcard_tag = "END:VCARD";

        public void LoadFile(StreamReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            var line = reader.ReadLine();
            VCard card = null;
            while (!string.IsNullOrEmpty(line))
            {
                var u_line = line.ToUpper();
                if (u_line.Equals(begin_vcard_tag) || u_line.Equals(end_vcard_tag))
                {
                    if (u_line.Equals(begin_vcard_tag))
                    {
                        card = null;
                        card = new VCard();
                    }
                    else
                    {
                        this.Add(card);
                    }
                }
                else
                {
                    if (line.StartsWith("PHOTO;"))
                    {
                        var all_text = new List<string>();
                        all_text.Add(line);
                        line = reader.ReadLine();
                        while (!line.Contains(";"))
                        {
                            all_text.Add(line);
                            line = reader.ReadLine();
                        }
                        var long_string = string.Join("", all_text.ToArray());
                        if (!card.ParsePropertyString(long_string))
                        {
                            Console.WriteLine("Fail!: " + long_string);
                            //Console.ReadLine();
                        }
                    }
                    else
                    {
                        if (!card.ParsePropertyString(line))
                        {
                            Console.WriteLine("Fail!: " + line);
                            Console.ReadLine();
                        }
                    }

                }


                //Console.WriteLine("::> " + line);

                line = reader.ReadLine();

            }
        }
    }
    [Serializable]
    public class VCard
    {
        public VCard()
        {
            Names = new VCardName[0];
            PhoneNumers = new PhoneNumber[0];
            EmailAddresses = new EmailAddress[0];
            ImageFormat = "PNG";
            ImageRaw = new byte[0];
            Notes = string.Empty;

        }

        public override string ToString()
        {
            try
            {
                if (Names.Length > 0) return Names[0].ToString();
                //if (EmailAddress.Length > 0) return EmailAddress[0].ToString();
                if (PhoneNumers.Length > 0) return PhoneNumers[0].ToString();
                return base.ToString();
            }
            catch (Exception)
            {
                return base.ToString();
            }

        }


        public VCardName[] Names { get; set; }
        public PhoneNumber[] PhoneNumers { get; set; }
        public EmailAddress[] EmailAddresses { get; set; }
        public byte[] ImageRaw { get; set; }
        public string ImageFormat { get; set; }
        public string Notes { get; set; }


        internal bool ParsePropertyString(string line)
        {
            if (line.StartsWith("TEL;")) return this.AddPhoneNumber(line);
            if (line.StartsWith("EMAIL;")) return this.AddEmailAddress(line);
            if (line.StartsWith("FN:") || line.StartsWith("N:")) return this.AddName(line);
            if (line.StartsWith("VERSION:")) return true; // skip version for now.
            if (line.StartsWith("ANNIVERSARY:")) return true; // skip anniversary.    
            if (line.StartsWith("ORG")) return true; // skip org.    
            if (line.StartsWith("BDAY")) return true; // skip bday.    
            if (line.StartsWith("NOTE")) return true; // skip note.   
            if (line.StartsWith("ADR")) return true; // skip address.   
            if (line.StartsWith("TITLE")) return true; // skip title.   
            if (line.StartsWith("PHOTO")) return this.EncodePhoto(line);
            return true;
        }

        private bool EncodePhoto(string line)
        {
            //Console.WriteLine("Photo Encoded to Storage");
            return true;
        }



        private bool AddName(string line)
        {
            List<VCardName> nameslist = null;

            var prefix = line.Substring(0, line.IndexOf(":"));
            switch (prefix)
            {
                case "FN":
                    var data = line.Substring(line.IndexOf(":") + 1);
                    var isprimary = Names.Where(x => x.IsPrimary.HasValue && x.IsPrimary.Value).Count().Equals(0);
                    var formal_name = new VCardName { IsPrimary = isprimary, Name = data };
                    nameslist = new List<VCardName>(Names);
                    nameslist.Add(formal_name);
                    Names = nameslist.ToArray();
                    return true;
                case "N":
                    var namedata = line.Substring(line.IndexOf(":") + 1);
                    var isprimary1 = Names.Where(x => x.IsPrimary.HasValue && x.IsPrimary.Value).Count().Equals(0);
                    var name = new VCardName { IsPrimary = isprimary1, Name = namedata };
                    nameslist = new List<VCardName>(Names);
                    nameslist.Add(name);
                    Names = nameslist.ToArray();
                    return true;
                default:
                    break;
            }

            return false;
        }

        private bool AddEmailAddress(string line)
        {
            List<EmailAddress> addressList = null;

            var prefix = line.Substring(0, line.IndexOf(";"));
            switch (prefix)
            {
                case "EMAIL":
                    var emaildata = line.Substring(line.IndexOf(";") + 1);
                    var category = emaildata.Substring(0, emaildata.IndexOf(":"));
                    emaildata = emaildata.Substring(emaildata.IndexOf(":") + 1);
                    var isprimary = EmailAddresses.Where(x => x.IsPrimary.HasValue && x.IsPrimary.Value).Count().Equals(0);
                    var email = new EmailAddress { IsPrimary = isprimary, Email = emaildata };
                    addressList = new List<EmailAddress>(EmailAddresses);
                    addressList.Add(email);
                    EmailAddresses = addressList.ToArray();
                    return true;
                default:
                    break;
            }

            return false;
        }

        private bool AddPhoneNumber(string line)
        {
            List<PhoneNumber> phoneList = null;

            var prefix = line.Substring(0, line.IndexOf(";"));
            var rest = line.Substring(line.IndexOf(";") + 1);
            switch (prefix)
            {
                case "TEL":
                    var phonedata = rest.Substring(rest.IndexOf(":") + 1);
                    var category = rest.Substring(0, rest.IndexOf(":"));
                    var phone = new PhoneNumber { IsPrimary = new bool?(), Number = phonedata, Category = category };
                    phoneList = new List<PhoneNumber>(PhoneNumers);
                    phoneList.Add(phone);
                    PhoneNumers = phoneList.ToArray();
                    return true;
                default:
                    break;
            }

            return false;
        }
    }

    [Serializable]
    public class VCardName
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public bool? IsPrimary { get; set; }

        public override string ToString()
        {
            try
            {
                var formatted_data = (string.IsNullOrEmpty(Category)) ? Name : string.Format("{0}:{1}", Category, Name);
                var primary_suffix = (IsPrimary.HasValue && IsPrimary.Value) ? "(p)" : string.Empty;
                return formatted_data + primary_suffix;
            }
            catch (Exception)
            {
                return base.ToString();
            }

        }
    }

    [Serializable]
    public class EmailAddress
    {
        public string Email { get; set; }
        public string Category { get; set; }
        public bool? IsPrimary { get; set; }

        public override string ToString()
        {
            try
            {
                var formatted_data = (string.IsNullOrEmpty(Category)) ? Email : string.Format("{0}:{1}", Category, Email);
                var primary_suffix = (IsPrimary.HasValue && IsPrimary.Value) ? "(p)" : string.Empty;
                return formatted_data + primary_suffix;
            }
            catch (Exception)
            {
                return base.ToString();
            }

        }
    }

    [Serializable]
    public class PhoneNumber
    {
        public string Number { get; set; }
        public string Category { get; set; }
        public bool? IsPrimary { get; set; }

        public override string ToString()
        {
            try
            {
                var formatted_data = (string.IsNullOrEmpty(Category)) ? Number : string.Format("{0}:{1}", Category, Number);
                var primary_suffix = (IsPrimary.HasValue && IsPrimary.Value) ? "(p)" : string.Empty;
                return formatted_data + primary_suffix;
            }
            catch (Exception)
            {
                return base.ToString();
            }

        }
    }
}