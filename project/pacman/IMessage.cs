using System;

namespace pacman
{
    public interface IMessage
    {
         string Username { get;  set; }
         string Content { get;  set; }
    }
}