﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleControl
{
    /// <summary>
    /// The ConsoleEventArgs are arguments for a console event.
    /// </summary>
    public class ConsoleEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleEventArgs"/> class.
        /// </summary>
        public ConsoleEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleEventArgs"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        public ConsoleEventArgs(string content)
        {
            //  Set the content.
            Content = content;
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        public string Content
        {
            get;
            private set;
        }
    }
}