﻿namespace CDCHMailSystem.Models
{
    public class Mail
    {
        public int Id { get; set; }
        public string FromUsername { get; set; }
        public string Subject { get; set; }
        public string DateTime { get; set; }
        public string Body { get; set; }
    }
}

