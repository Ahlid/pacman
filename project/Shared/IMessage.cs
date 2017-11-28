using System;

namespace Shared
{
  
    public interface IChatMessage
    {
         string Username { get;  set; }
         string Content { get;  set; }
    }
}