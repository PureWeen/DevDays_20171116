using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DevDays.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }

        public byte[] UserImage { get; set; }


        public UserModel Clone()
        {
            return new UserModel()
            {
                Id = this.Id,
                UserName = this.UserName,
                UserImage = this.UserImage
            };
        }
    }
}
