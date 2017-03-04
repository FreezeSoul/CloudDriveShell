using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;

namespace CloudDriveShell.Infrastructure.Models
{
    public class WindowClosingEvent : PubSubEvent<CancelEventArgs>
    {
    }

    public class PasteFileEvent : PubSubEvent<string[]>
    {
    }

    public class PasteResourceEvent : PubSubEvent
    {
    }

    public class GlobalExceptionEvent : PubSubEvent<string>
    {
    }

    public class SwitchuserEvent : PubSubEvent
    {
    }
}
