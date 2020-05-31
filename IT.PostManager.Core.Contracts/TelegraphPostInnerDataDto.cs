using System;

namespace IT.PostManager.Core.Contracts
{
    public class TelegraphPostInnerDataDto
    {
        public DateTimeOffset PostDate { get; set; }
        public bool Disabled { get; set; }
    }
}