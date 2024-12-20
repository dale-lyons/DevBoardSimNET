﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleControl
{
    public class ProcessEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessEventArgs"/> class.
        /// </summary>
        public ProcessEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessEventArgs"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        public ProcessEventArgs(string content)
        {
            //  Set the content and code.
            Content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessEventArgs"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        public ProcessEventArgs(int code)
        {
            //  Set the content and code.
            Code = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessEventArgs"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="code">The code.</param>
        public ProcessEventArgs(string content, int code)
        {
            //  Set the content and code.
            Content = content;
            Code = code;
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public int? Code { get; private set; }
    }
}